using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using TheOtherRoles.Roles;
using TMPro;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.Events;
using static TheOtherRoles.MapOptions;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch]
    internal class MeetingHudPatch
    {
        private static bool[] selections;
        private static SpriteRenderer[] renderers;
        private static GameData.PlayerInfo target;

        private static GameObject guesserUI;


        private static void swapperOnClick(int i, MeetingHud __instance)
        {
            if (__instance.state == MeetingHud.VoteStates.Results) return;
            if (__instance.playerStates[i].AmDead) return;

            var selectedCount = selections.Length;
            var renderer = renderers[i];

            switch (selectedCount)
            {
                case 0:
                    renderer.color = Color.green;
                    selections[i] = true;
                    break;
                case 1 when selections[i]:
                    renderer.color = Color.red;
                    selections[i] = false;
                    break;
                case 1:
                {
                    selections[i] = true;
                    renderer.color = Color.green;

                    PlayerVoteArea firstPlayer = null;
                    PlayerVoteArea secondPlayer = null;
                    for (var a = 0; a < selections.Length; a++)
                        if (selections[a])
                        {
                            if (firstPlayer != null)
                            {
                                secondPlayer = __instance.playerStates[a];
                                break;
                            }

                            firstPlayer = __instance.playerStates[a];
                        }

                    if (firstPlayer != null && secondPlayer != null)
                    {
                        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.SwapperSwap, SendOption.Reliable, -1);
                        writer.Write(firstPlayer.TargetPlayerId);
                        writer.Write(secondPlayer.TargetPlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);

                        RPCProcedure.SwapperSwap(firstPlayer.TargetPlayerId, secondPlayer.TargetPlayerId);
                    }

                    break;
                }
            }
        }

        private static void guesserOnClick(int buttonTarget, MeetingHud __instance)
        {
            if (guesserUI != null || !(__instance.state == MeetingHud.VoteStates.Voted ||
                                       __instance.state == MeetingHud.VoteStates.NotVoted)) return;
            __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(false));

            var container = Object.Instantiate(__instance.transform.FindChild("Background"), __instance.transform);
            container.FindChild("BlackBG").gameObject.SetActive(false);
            container.transform.localPosition = new Vector3(0, 0, -5f);
            guesserUI = container.gameObject;

            var i = 0;
            var buttonTemplate = __instance.playerStates[0].transform.FindChild("votePlayerBase");
            var maskTemplate = __instance.playerStates[0].transform.FindChild("MaskArea");
            var smallButtonTemplate = __instance.playerStates[0].Buttons.transform.Find("CancelButton");
            var textTemplate = __instance.playerStates[0].NameText;

            var exitButtonParent = new GameObject().transform;
            exitButtonParent.SetParent(container);
            var exitButton = Object.Instantiate(buttonTemplate.transform, exitButtonParent);
            _ = Object.Instantiate(maskTemplate, exitButtonParent);
            exitButton.gameObject.GetComponent<SpriteRenderer>().sprite =
                smallButtonTemplate.GetComponent<SpriteRenderer>().sprite;
            exitButtonParent.transform.localPosition = new Vector3(2.725f, 2.1f, -5);
            exitButtonParent.transform.localScale = new Vector3(0.25f, 0.9f, 1);
            exitButton.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
            exitButton.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction) OnExitButtonClick);

            void OnExitButtonClick()
            {
                __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                Object.Destroy(container.gameObject);
            }

            var buttons = new List<Transform>();
            Transform selectedButton = null;

            foreach (var roleInfo in RoleInfo.AllRoleInfos)
            {
                if (roleInfo.roleId is RoleId.Lover or RoleId.Guesser || roleInfo == RoleInfo.NiceMini)
                    continue; // Not guessable roles
                var buttonParent = new GameObject().transform;
                buttonParent.SetParent(container);
                var button = Object.Instantiate(buttonTemplate, buttonParent);
                var label = Object.Instantiate(textTemplate, button);
                buttons.Add(button);
                int row = i / 4, col = i % 4;
                buttonParent.localPosition = new Vector3(-2.725f + 1.83f * col, 1.5f - 0.45f * row, -5);
                buttonParent.localScale = new Vector3(0.55f, 0.55f, 1f);
                label.text = Helpers.Cs(roleInfo.color, roleInfo.name);
                label.alignment = TextAlignmentOptions.Center;
                label.transform.localPosition = new Vector3(0, 0, label.transform.localPosition.z);
                label.transform.localScale *= 1.7f;

                button.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();

                button.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction) OnButtonClick);

                void OnButtonClick()
                {
                    if (selectedButton != button)
                    {
                        selectedButton = button;
                        buttons.ForEach(x =>
                            x.GetComponent<SpriteRenderer>().color = x == selectedButton ? Color.red : Color.white);
                    }
                    else
                    {
                        var playerById = Helpers.PlayerById(__instance.playerStates[buttonTarget].TargetPlayerId);
                        if (__instance.state is not (MeetingHud.VoteStates.Voted or MeetingHud.VoteStates.NotVoted) ||
                            playerById == null ||
                            Guesser.remainingShots <= 0) return;

                        var mainRoleInfo = RoleInfo.GetRoleInfoForPlayer(playerById).FirstOrDefault();
                        if (mainRoleInfo == null) return;

                        playerById = mainRoleInfo == roleInfo ? playerById : PlayerControl.LocalPlayer;

                        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.GuesserShoot, SendOption.Reliable, -1);
                        writer.Write(playerById.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.GuesserShoot(playerById.PlayerId);

                        __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                        Object.Destroy(container.gameObject);
                        __instance.playerStates.ToList().ForEach(x =>
                        {
                            if (x.transform.FindChild("ShootButton") != null)
                                Object.Destroy(x.transform.FindChild("ShootButton").gameObject);
                        });
                    }
                }

                i++;
            }

            container.transform.localScale *= 0.75f;
        }


        private static void PopulateButtonsPostfix(MeetingHud __instance)
        {
            // Add Swapper Buttons
            if (Swapper.Instance.player != null && PlayerControl.LocalPlayer == Swapper.Instance.player &&
                !Swapper.Instance.player.Data.IsDead)
            {
                selections = new bool[__instance.playerStates.Length];
                renderers = new SpriteRenderer[__instance.playerStates.Length];

                for (var i = 0; i < __instance.playerStates.Length; i++)
                {
                    var playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.AmDead || playerVoteArea.TargetPlayerId == Swapper.Instance.player.PlayerId &&
                        Swapper.canOnlySwapOthers) continue;

                    var template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    var checkbox = Object.Instantiate(template, playerVoteArea.transform, true);
                    checkbox.transform.position = template.transform.position;
                    checkbox.transform.localPosition = new Vector3(-0.95f, 0.03f, -1f);
                    var renderer = checkbox.GetComponent<SpriteRenderer>();
                    renderer.sprite = Swapper.GetCheckSprite();
                    renderer.color = Color.red;

                    var button = checkbox.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    var copiedIndex = i;

                    void OnClick()
                    {
                        swapperOnClick(copiedIndex, __instance);
                    }

                    button.OnClick.AddListener((UnityAction) OnClick);

                    selections[i] = false;
                    renderers[i] = renderer;
                }
            }

            // Add Guesser Buttons
            if (Guesser.Instance.player == null || PlayerControl.LocalPlayer != Guesser.Instance.player ||
                Guesser.Instance.player.Data.IsDead || Guesser.remainingShots < 0) return;
            {
                for (var i = 0; i < __instance.playerStates.Length; i++)
                {
                    var playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.AmDead ||
                        playerVoteArea.TargetPlayerId == Guesser.Instance.player.PlayerId) continue;

                    var template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    var targetBox = Object.Instantiate(template, playerVoteArea.transform);
                    targetBox.name = "ShootButton";
                    targetBox.transform.localPosition = new Vector3(-0.95f, 0.03f, -1f);
                    var renderer = targetBox.GetComponent<SpriteRenderer>();
                    renderer.sprite = Guesser.GetTargetSprite();
                    var button = targetBox.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    var copiedIndex = i;

                    void OnClick()
                    {
                        guesserOnClick(copiedIndex, __instance);
                    }

                    button.OnClick.AddListener((UnityAction) OnClick);
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
        private class MeetingCalculateVotesPatch
        {
            private static Dictionary<byte, int> CalculateVotes(MeetingHud __instance)
            {
                var dictionary = new Dictionary<byte, int>();
                foreach (var playerVoteArea in __instance.playerStates)
                {
                    if (playerVoteArea.VotedFor is 252 or 255 or 254) continue;
                    var player = Helpers.PlayerById(playerVoteArea.TargetPlayerId);
                    if (player == null || player.Data == null || player.Data.IsDead || player.Data.Disconnected)
                        continue;

                    var additionalVotes =
                        Mayor.Instance.player != null && Mayor.Instance.player.PlayerId == playerVoteArea.TargetPlayerId
                            ? 2
                            : 1; // Mayor vote
                    if (dictionary.TryGetValue(playerVoteArea.VotedFor, out var currentVotes))
                        dictionary[playerVoteArea.VotedFor] = currentVotes + additionalVotes;
                    else
                        dictionary[playerVoteArea.VotedFor] = additionalVotes;
                }

                // Swapper swap votes
                if (Swapper.Instance.player == null || Swapper.Instance.player.Data.IsDead) return dictionary;
                {
                    PlayerVoteArea swapped1 = null;
                    PlayerVoteArea swapped2 = null;
                    foreach (var playerVoteArea in __instance.playerStates)
                    {
                        if (playerVoteArea.TargetPlayerId == Swapper.playerId1) swapped1 = playerVoteArea;
                        if (playerVoteArea.TargetPlayerId == Swapper.playerId2) swapped2 = playerVoteArea;
                    }

                    if (swapped1 == null || swapped2 == null) return dictionary;
                    if (!dictionary.ContainsKey(swapped1.TargetPlayerId)) dictionary[swapped1.TargetPlayerId] = 0;
                    if (!dictionary.ContainsKey(swapped2.TargetPlayerId)) dictionary[swapped2.TargetPlayerId] = 0;
                    var tmp = dictionary[swapped1.TargetPlayerId];
                    dictionary[swapped1.TargetPlayerId] = dictionary[swapped2.TargetPlayerId];
                    dictionary[swapped2.TargetPlayerId] = tmp;
                }

                return dictionary;
            }


            private static bool Prefix(MeetingHud __instance)
            {
                if (!__instance.playerStates.All(ps => ps.AmDead || ps.DidVote)) return false;

                var self = CalculateVotes(__instance);
                var max = self.MaxPair(out var tie);
                var exiled = GameData.Instance.AllPlayers.ToArray()
                    .FirstOrDefault(v => !tie && v.PlayerId == max.Key && !v.IsDead);

                var array = new MeetingHud.VoterState[__instance.playerStates.Length];
                for (var i = 0; i < __instance.playerStates.Length; i++)
                {
                    var playerVoteArea = __instance.playerStates[i];
                    array[i] = new MeetingHud.VoterState
                    {
                        VoterId = playerVoteArea.TargetPlayerId,
                        VotedForId = playerVoteArea.VotedFor
                    };
                }

                // RPCVotingComplete
                __instance.RpcVotingComplete(array, exiled, tie);

                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BloopAVoteIcon))]
        private class MeetingHudBloopAVoteIconPatch
        {
            public static bool Prefix(MeetingHud __instance, [HarmonyArgument(0)] GameData.PlayerInfo voterPlayer,
                [HarmonyArgument(1)] int index, [HarmonyArgument(2)] Transform parent)
            {
                var spriteRenderer = Object.Instantiate(__instance.PlayerVotePrefab, parent, true);
                if (!PlayerControl.GameOptions.AnonymousVotes ||
                    PlayerControl.LocalPlayer.Data.IsDead && ghostsSeeVotes)
                    PlayerControl.SetPlayerMaterialColors(voterPlayer.ColorId, spriteRenderer);
                else
                    PlayerControl.SetPlayerMaterialColors(Palette.DisabledGrey, spriteRenderer);
                spriteRenderer.transform.localScale = Vector3.zero;
                __instance.StartCoroutine(Effects.Bloop(index * 0.3f, spriteRenderer.transform, 1f, 0.5f));
                parent.GetComponent<VoteSpreader>().AddVote(spriteRenderer);
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
        private class MeetingHudPopulateVotesPatch
        {
            private static bool Prefix(MeetingHud __instance, Il2CppStructArray<MeetingHud.VoterState> states)
            {
                // Swapper swap

                PlayerVoteArea swapped1 = null;
                PlayerVoteArea swapped2 = null;
                foreach (var playerVoteArea in __instance.playerStates)
                {
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId1) swapped1 = playerVoteArea;
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId2) swapped2 = playerVoteArea;
                }

                var doSwap = swapped1 != null && swapped2 != null && Swapper.Instance.player != null &&
                             !Swapper.Instance.player.Data.IsDead;
                if (doSwap)
                {
                    __instance.StartCoroutine(Effects.Slide3D(swapped1.transform, swapped1.transform.localPosition,
                        swapped2.transform.localPosition, 1.5f));
                    __instance.StartCoroutine(Effects.Slide3D(swapped2.transform, swapped2.transform.localPosition,
                        swapped1.transform.localPosition, 1.5f));
                }


                __instance.TitleText.text =
                    DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults,
                        new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                var num = 0;
                for (var i = 0; i < __instance.playerStates.Length; i++)
                {
                    var playerVoteArea = __instance.playerStates[i];
                    var targetPlayerId = playerVoteArea.TargetPlayerId;
                    playerVoteArea = doSwap switch
                    {
                        // Swapper change playerVoteArea that gets the votes
                        true when playerVoteArea.TargetPlayerId == swapped1.TargetPlayerId => swapped2,
                        true when playerVoteArea.TargetPlayerId == swapped2.TargetPlayerId => swapped1,
                        _ => playerVoteArea
                    };

                    playerVoteArea.ClearForResults();
                    var num2 = 0;
                    var mayorFirstVoteDisplayed = false;
                    for (var j = 0; j < states.Length; j++)
                    {
                        var voterState = states[j];
                        var playerById = GameData.Instance.GetPlayerById(voterState.VoterId);
                        if (playerById == null)
                        {
                            Debug.LogError(
                                $"Couldn't find player info for voter: {voterState.VoterId}");
                        }
                        else if (i == 0 && voterState.SkippedVote && !playerById.IsDead)
                        {
                            __instance.BloopAVoteIcon(playerById, num, __instance.SkippedVoting.transform);
                            num++;
                        }
                        else if (voterState.VotedForId == targetPlayerId && !playerById.IsDead)
                        {
                            __instance.BloopAVoteIcon(playerById, num2, playerVoteArea.transform);
                            num2++;
                        }

                        // Major vote, redo this iteration to place a second vote
                        if (Mayor.Instance.player == null ||
                            voterState.VoterId != (sbyte) Mayor.Instance.player.PlayerId ||
                            mayorFirstVoteDisplayed) continue;
                        mayorFirstVoteDisplayed = true;
                        j--;
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
        private class MeetingHudVotingCompletedPatch
        {
            private static void Postfix([HarmonyArgument(1)] GameData.PlayerInfo exiled)
            {
                // Reset swapper values
                Swapper.playerId1 = byte.MaxValue;
                Swapper.playerId2 = byte.MaxValue;

                // Lovers save next to be exiled, because RPC of ending game comes before RPC of exiled
                Lovers.notAckedExiledIsLover = false;
                if (exiled != null)
                    Lovers.notAckedExiledIsLover =
                        Lovers.Instance.player != null && Lovers.Instance.player.PlayerId == exiled.PlayerId ||
                        Lovers.Instance.secondPlayer != null &&
                        Lovers.Instance.secondPlayer.PlayerId == exiled.PlayerId;
            }
        }

        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
        private class PlayerVoteAreaSelectPatch
        {
            private static bool Prefix()
            {
                return !(PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer == Guesser.Instance.player &&
                         guesserUI != null);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.ServerStart))]
        private class MeetingServerStartPatch
        {
            private static void Postfix(MeetingHud __instance)
            {
                PopulateButtonsPostfix(__instance);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Deserialize))]
        private class MeetingDeserializePatch
        {
            private static void Postfix(MeetingHud __instance, [HarmonyArgument(1)] bool initialState)
            {
                // Add swapper buttons
                if (initialState) PopulateButtonsPostfix(__instance);
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CoStartMeeting))]
        private class StartMeetingPatch
        {
            public static void Prefix([HarmonyArgument(0)] GameData.PlayerInfo meetingTarget)
            {
                // Reset vampire bitten
                Vampire.bitten = null;
                // Count meetings
                if (meetingTarget == null) meetingsCount++;
                // Save the meeting target
                target = meetingTarget;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        private class MeetingHudUpdatePatch
        {
            private static void Postfix(MeetingHud __instance)
            {
                // Deactivate skip Button if skipping on emergency meetings is disabled
                if (target == null && blockSkippingInEmergencyMeetings)
                    __instance.SkipVoteButton.gameObject.SetActive(false);
            }
        }
    }
}