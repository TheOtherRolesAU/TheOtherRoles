using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.MapOptions;
using System.Collections;
using System;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace TheOtherRoles
{
    [HarmonyPatch]
    class MeetingHudPatch {
        static bool[] selections;
        static SpriteRenderer[] renderers;
        private static GameData.PlayerInfo target = null;
        private const float scale = 0.65f;

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
        class MeetingCalculateVotesPatch {
            private static byte[] calculateVotes(MeetingHud __instance) {
                byte[] array = new byte[16];
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.didVote)
                    {
                        int num = (int)(playerVoteArea.votedFor + 1);
                        if (num >= 0 && num < array.Length)
                        {
                            byte[] array2 = array;
                            int num2 = num;
                            // Mayor count vote twice
                            if (Mayor.mayor != null && playerVoteArea.TargetPlayerId == (sbyte)Mayor.mayor.PlayerId)
                                array2[num2] += 2;
                            else
                                array2[num2] += 1;
                        }
                    }
                }

                // Swapper swap votes
                PlayerVoteArea swapped1 = null;
                PlayerVoteArea swapped2 = null;

                foreach (PlayerVoteArea playerVoteArea in __instance.playerStates) {
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId1) swapped1 = playerVoteArea;
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId2) swapped2 = playerVoteArea;
                }

                if (swapped1 != null && swapped2 != null && swapped1.TargetPlayerId + 1 >= 0 && swapped1.TargetPlayerId + 1 < array.Length && swapped2.TargetPlayerId + 1 >= 0 && swapped2.TargetPlayerId + 1 < array.Length) {
                    byte tmp = array[swapped1.TargetPlayerId + 1];
                    array[swapped1.TargetPlayerId + 1] = array[swapped2.TargetPlayerId + 1];
                    array[swapped2.TargetPlayerId + 1] = tmp;
                }
                return array;
            }

            private static int IndexOfMax(byte[] self, Func<byte, int> comparer, out bool tie) {
                tie = false;
                int num = int.MinValue;
                int result = -1;
                for (int i = 0; i < self.Length; i++)
                {
                    int num2 = comparer(self[i]);
                    if (num2 > num)
                    {
                        result = i;
                        num = num2;
                        tie = false;
                    }
                    else if (num2 == num)
                    {
                        tie = true;
                        result = -1;
                    }
                }
                return result;
            }

            static bool Prefix(MeetingHud __instance) {
                if (__instance.playerStates.All((PlayerVoteArea ps) => ps.isDead || ps.didVote)) {
                    // If skipping is disabled, replace skipps/no-votes with self vote
                    if (target == null && blockSkippingInEmergencyMeetings && noVoteIsSelfVote) {
                        foreach (PlayerVoteArea playerVoteArea in __instance.playerStates) {
                            if (playerVoteArea.votedFor < 0) playerVoteArea.votedFor = playerVoteArea.TargetPlayerId; // TargetPlayerId
                        }
                    }

                    byte[] self = calculateVotes(__instance);
                    bool tie;
                    int maxIdx = IndexOfMax(self, (byte p) => (int)p, out tie) - 1;
                    GameData.PlayerInfo exiled = null;
                    foreach (GameData.PlayerInfo pi in GameData.Instance.AllPlayers) {
                        if (pi.PlayerId == maxIdx && !pi.IsDead) {
                            exiled = pi;
                            break;
                        }
                    }
                    byte[] array = new byte[15];
                    for (int i = 0; i < __instance.playerStates.Length; i++) {
                        PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                        array[(int)playerVoteArea.TargetPlayerId] = playerVoteArea.GetState();
                    }

                    // RPCVotingComplete
                    if (AmongUsClient.Instance.AmClient)
                        __instance.VotingComplete(array, exiled, tie);
                    MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId, 23, Hazel.SendOption.Reliable);
                    messageWriter.WriteBytesAndSize(array);
                    messageWriter.Write((exiled != null) ? exiled.PlayerId : byte.MaxValue);
                    messageWriter.Write(tie);
                    messageWriter.EndMessage();
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
        class MeetingPopulateVotesPatch {
            static bool Prefix(MeetingHud __instance, [HarmonyArgument(0)]Il2CppStructArray<byte> states)
            {
                // Swapper swap votes
                PlayerVoteArea swapped1 = null;
                PlayerVoteArea swapped2 = null;

                foreach (PlayerVoteArea playerVoteArea in __instance.playerStates) {
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId1) swapped1 = playerVoteArea;
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId2) swapped2 = playerVoteArea;
                }

                bool doSwap = swapped1 != null && swapped2 != null;
                float delay = 0f;
                if (doSwap) {
                    delay = 2f;
                    __instance.StartCoroutine(Effects.Slide3D(swapped1.transform, swapped1.transform.localPosition, swapped2.transform.localPosition, 2f));
                    __instance.StartCoroutine(Effects.Slide3D(swapped2.transform, swapped2.transform.localPosition, swapped1.transform.localPosition, 2f));
                }

                float votesXOffset = 0f;
                float votesFinalSize = 1f;
                if (__instance.playerStates.Length > 10) {
                    votesXOffset = 0.1f;
                    votesFinalSize = scale;
                }

                // Mayor display vote twice
                __instance.TitleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                int num = 0;
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    playerVoteArea.ClearForResults();
                    int num2 = 0;
                    bool mayorFirstVoteDisplayed = false;

                    for (int j = 0; j < __instance.playerStates.Length; j++)
                    {
                        PlayerVoteArea playerVoteArea2 = __instance.playerStates[j];
                        byte self = states[(int)playerVoteArea2.TargetPlayerId];

                        if (!((self & 128) > 0))
                        {
                            GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById((byte)playerVoteArea2.TargetPlayerId);
                            int votedFor = (int)PlayerVoteArea.GetVotedFor(self);
                            if (votedFor == (int)playerVoteArea.TargetPlayerId)
                            {
                                SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab);
                                if (!PlayerControl.GameOptions.AnonymousVotes || (PlayerControl.LocalPlayer.Data.IsDead && MapOptions.ghostsSeeVotes))
                                    PlayerControl.SetPlayerMaterialColors((int)playerById.ColorId, spriteRenderer);
                                else
                                    PlayerControl.SetPlayerMaterialColors(Palette.DisabledGrey, spriteRenderer);

                                spriteRenderer.transform.SetParent(playerVoteArea.transform);
                                spriteRenderer.transform.localPosition = __instance.CounterOrigin + new Vector3(votesXOffset + __instance.CounterOffsets.x * (float)(num2), 0f, 0f);
                                spriteRenderer.transform.localScale = Vector3.zero;
                                spriteRenderer.transform.SetParent(playerVoteArea.transform.parent); // Reparent votes so they don't move with their playerVoteArea
                                __instance.StartCoroutine(Effects.Bloop((float)(num2) * 0.5f + delay, spriteRenderer.transform, votesFinalSize, 0.5f));
                                num2++;
                            }
                            else if (i == 0 && votedFor == -1)
                            {
                                SpriteRenderer spriteRenderer2 = UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab);
                                if (!PlayerControl.GameOptions.AnonymousVotes || (PlayerControl.LocalPlayer.Data.IsDead && MapOptions.ghostsSeeVotes))
                                    PlayerControl.SetPlayerMaterialColors((int)playerById.ColorId, spriteRenderer2);
                                else
                                    PlayerControl.SetPlayerMaterialColors(Palette.DisabledGrey, spriteRenderer2);
                                spriteRenderer2.transform.SetParent(__instance.SkippedVoting.transform);
                                spriteRenderer2.transform.localPosition = __instance.CounterOrigin + new Vector3(votesXOffset + __instance.CounterOffsets.x * (float)(num), 0f, 0f);
                                spriteRenderer2.transform.localScale = Vector3.zero;
                                spriteRenderer2.transform.SetParent(playerVoteArea.transform.parent); // Reparent votes so they don't move with their playerVoteArea
                                __instance.StartCoroutine(Effects.Bloop((float)num * 0.5f + delay, spriteRenderer2.transform, votesFinalSize, 0.5f));
                                num++;
                            }

                            // Major vote, redo this iteration to place a second vote
                            if (Mayor.mayor != null && playerVoteArea2.TargetPlayerId == (sbyte)Mayor.mayor.PlayerId && !mayorFirstVoteDisplayed) {
                                mayorFirstVoteDisplayed = true;
                                j--;    
                            }
                        }
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
        class MeetingHudVotingCompletedPatch {
            static void Postfix(MeetingHud __instance, [HarmonyArgument(0)]byte[] states, [HarmonyArgument(1)]GameData.PlayerInfo exiled, [HarmonyArgument(2)]bool tie)
            {
                // Reset swapper values
                Swapper.playerId1 = Byte.MaxValue;
                Swapper.playerId2 = Byte.MaxValue;

                // Lovers save next to be exiled, because RPC of ending game comes before RPC of exiled
                Lovers.notAckedExiledIsLover = false;
                if (exiled != null)
                    Lovers.notAckedExiledIsLover = ((Lovers.lover1 != null && Lovers.lover1.PlayerId == exiled.PlayerId) || (Lovers.lover2 != null && Lovers.lover2.PlayerId == exiled.PlayerId));
            }
        }


        static void swapperOnClick(int i, MeetingHud __instance) {
            if (__instance.state == MeetingHud.VoteStates.Results) return;
            if (__instance.playerStates[i].isDead) return;

            int selectedCount = selections.Where(b => b).Count();
            SpriteRenderer renderer = renderers[i];

            if (selectedCount == 0) {
                renderer.color = Color.green;
                selections[i] = true;
            } else if (selectedCount == 1) {
                if (selections[i]) {
                    renderer.color = Color.red;
                    selections[i] = false;
                } else {
                    selections[i] = true;
                    renderer.color = Color.green;   
                    
                    PlayerVoteArea firstPlayer = null;
                    PlayerVoteArea secondPlayer = null;
                    for (int A = 0; A < selections.Length; A++) {
                        if (selections[A]) {
                            if (firstPlayer != null) {
                                secondPlayer = __instance.playerStates[A];
                                break;
                            } else {
                                firstPlayer = __instance.playerStates[A];
                            }
                        }
                    }

                    if (firstPlayer != null && secondPlayer != null) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SwapperSwap, Hazel.SendOption.Reliable, -1);
                        writer.Write((byte)firstPlayer.TargetPlayerId);
                        writer.Write((byte)secondPlayer.TargetPlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);

                        RPCProcedure.swapperSwap((byte)firstPlayer.TargetPlayerId, (byte)secondPlayer.TargetPlayerId);
                    }
                }
            }
        }

        private static GameObject guesserUI;
        static void guesserOnClick(int buttonTarget, MeetingHud __instance) {
            if (guesserUI != null || !(__instance.state == MeetingHud.VoteStates.Voted || __instance.state == MeetingHud.VoteStates.NotVoted)) return;

            Transform container = UnityEngine.Object.Instantiate(__instance.transform.FindChild("Background"), __instance.transform);
            container.transform.localPosition = new Vector3(0, 0, -5f);
            guesserUI = container.gameObject;

            int i = 0;
            var buttonTemplate = __instance.playerStates[0].transform.FindChild("votePlayerBase");
            var smallButtonTemplate = __instance.playerStates[0].Buttons.transform.Find("CancelButton");
            var textTemplate = __instance.playerStates[0].NameText;


            Transform exitButton = UnityEngine.Object.Instantiate(smallButtonTemplate.transform, container);
            exitButton.transform.localPosition = new Vector3(2.5f, 2.25f, -5);
            exitButton.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
            exitButton.GetComponent<PassiveButton>().OnClick.AddListener((UnityEngine.Events.UnityAction)(() => {
                UnityEngine.Object.Destroy(container.gameObject);
            }));

            List<Transform> confirmButtons = new List<Transform>();

            foreach (RoleInfo roleInfo in RoleInfo.allRoleInfos) {
                if (roleInfo.roleId == RoleId.Lover || roleInfo.roleId == RoleId.Guesser || roleInfo == RoleInfo.niceMini) continue; // Not guessable roles

                Transform button = UnityEngine.Object.Instantiate(buttonTemplate.transform, container);
                Transform confirm = UnityEngine.Object.Instantiate(smallButtonTemplate.transform, button);
                confirmButtons.Add(confirm);
                TMPro.TextMeshPro label = UnityEngine.Object.Instantiate(textTemplate, button);
                int row = i/4, col = i%4;
                button.localPosition = new Vector3(-3 + 1.83f * col, 1.5f - 0.4f * row, -5);
                button.localScale = new Vector3(0.4f, 0.4f, 1f);
                confirm.localScale = new Vector3(1.5f, 1.5f, 1f);
                confirm.localPosition = new Vector3(0, 0, confirm.localPosition.z);
                confirm.GetComponent<SpriteRenderer>().sprite = Guesser.getTargetSprite();
                confirm.GetComponent<SpriteRenderer>().color = Color.black;
                confirm.gameObject.SetActive(false);
                label.text = Helpers.cs(roleInfo.color, roleInfo.name);
                label.alignment = TMPro.TextAlignmentOptions.Center;
                label.transform.localPosition = new Vector3(0, 0, label.transform.localPosition.z);
                label.transform.localScale *= 2;
                int copiedIndex = i;

                button.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
                button.GetComponent<PassiveButton>().OnClick.AddListener((UnityEngine.Events.UnityAction)(() => {
                    confirmButtons.ForEach(x => x.gameObject.SetActive(false));
                    confirm.gameObject.SetActive(true);
                }));


                confirm.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
                confirm.GetComponent<PassiveButton>().OnClick.AddListener((UnityEngine.Events.UnityAction)(() => {
                    PlayerControl target = Helpers.playerById((byte)__instance.playerStates[buttonTarget].TargetPlayerId);
                    if (!(__instance.state == MeetingHud.VoteStates.Voted || __instance.state == MeetingHud.VoteStates.NotVoted) || target == null || Guesser.remainingShots <= 0 ) return;

                    var mainRoleInfo = RoleInfo.getRoleInfoForPlayer(target).FirstOrDefault();
                    if (mainRoleInfo == null) return;

                    target = (mainRoleInfo == roleInfo) ? target : PlayerControl.LocalPlayer;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuesserShoot, Hazel.SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.guesserShoot(target.PlayerId);

                    UnityEngine.Object.Destroy(container.gameObject);
                    __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
                }));

                i++;
            }
            container.transform.localScale *= 0.75f;
        }

        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
        class PlayerVoteAreaSelectPatch {
            static bool Prefix(MeetingHud __instance) {
                return !(PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer == Guesser.guesser && guesserUI != null);
            }
        }


        static void populateButtonsPostfix(MeetingHud __instance) {
            // Add Swapper Buttons
            if (Swapper.swapper != null && PlayerControl.LocalPlayer == Swapper.swapper && !Swapper.swapper.Data.IsDead) {
                selections = new bool[__instance.playerStates.Length];
                renderers = new SpriteRenderer[__instance.playerStates.Length];

                for (int i = 0; i < __instance.playerStates.Length; i++) {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.isDead || (playerVoteArea.TargetPlayerId == Swapper.swapper.PlayerId && Swapper.canOnlySwapOthers)) continue;

                    GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    GameObject checkbox = UnityEngine.Object.Instantiate(template);
                    checkbox.transform.SetParent(playerVoteArea.transform);
                    checkbox.transform.position = template.transform.position;
                    checkbox.transform.localPosition = new Vector3(0f, 0.03f, template.transform.localPosition.z);
                    SpriteRenderer renderer = checkbox.GetComponent<SpriteRenderer>();
                    renderer.sprite = Swapper.getCheckSprite();
                    renderer.color = Color.red;

                    PassiveButton button = checkbox.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    int copiedIndex = i;
                    button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => swapperOnClick(copiedIndex, __instance)));
                    
                    selections[i] = false;
                    renderers[i] = renderer;
                }
            }

            // Add Guesser Buttons
            if (Guesser.guesser != null && PlayerControl.LocalPlayer == Guesser.guesser && !Guesser.guesser.Data.IsDead && Guesser.remainingShots >= 0) {
                for (int i = 0; i < __instance.playerStates.Length; i++) {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.isDead || playerVoteArea.TargetPlayerId == Guesser.guesser.PlayerId) continue;

                    GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    GameObject targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);
                    targetBox.name = "ShootButton";
                    targetBox.transform.localPosition = new Vector3(0f, 0.03f, template.transform.localPosition.z);
                    SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                    renderer.sprite = Guesser.getTargetSprite();
                    PassiveButton button = targetBox.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    int copiedIndex = i;
                    button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => guesserOnClick(copiedIndex, __instance)));
                }
            }

            // Change buttons if there are more than 10 players
            if (__instance.playerStates != null && __instance.playerStates.Length > 10) { 
                PlayerVoteArea[] playerStates = __instance.playerStates.OrderBy((PlayerVoteArea p) => p.isDead ? 50 : 0)
						   	       .ThenBy((PlayerVoteArea p) => p.TargetPlayerId)
						   	       .ToArray<PlayerVoteArea>();
                for (int i = 0; i < playerStates.Length; i++) {
                    PlayerVoteArea area = playerStates[i];

                    int row = i/3, col = i%3;

                    // Update scalings
                    area.Overlay.transform.localScale = area.PlayerButton.transform.localScale = new Vector3(1, 1/scale, 1);
                    area.NameText.transform.localScale = new Vector3(1/scale, 1/scale, 1);
                    area.gameObject.transform.localScale = new Vector3(scale, scale, 1);
                    GameObject megaphoneWrapper = new GameObject();
                    megaphoneWrapper.transform.SetParent(area.transform);
                    megaphoneWrapper.transform.localScale = Vector3.one * 1/scale;
                    area.Megaphone.transform.SetParent(megaphoneWrapper.transform);

                    // Update positions
                    area.Megaphone.transform.localPosition += Vector3.left * 0.1f;
                    area.NameText.transform.localPosition += new Vector3(0.25f, 0.043f, 0f);
                    area.transform.localPosition = new Vector3(-3.63f + 2.43f * col, 1.5f - 0.76f * row, -0.9f - 0.01f * row);
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.ServerStart))]
        class MeetingServerStartPatch {
            static void Postfix(MeetingHud __instance)
            {
                populateButtonsPostfix(__instance);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Deserialize))]
        class MeetingDeserializePatch {
            static void Postfix(MeetingHud __instance, [HarmonyArgument(0)]MessageReader reader, [HarmonyArgument(1)]bool initialState)
            {
                // Add swapper buttons
                if (initialState) {
                    populateButtonsPostfix(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CoStartMeeting))]
        class StartMeetingPatch {
            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)]GameData.PlayerInfo meetingTarget) {
                // Reset vampire bitten
                Vampire.bitten = null;
                // Count meetings
                if (meetingTarget == null) meetingsCount++;
                // Save the meeting target
                target = meetingTarget;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        class MeetingHudUpdatePatch {
            static void Postfix(MeetingHud __instance) {
                // Deactivate skip Button if skipping on emergency meetings is disabled
                if (target == null && blockSkippingInEmergencyMeetings)
                    __instance.SkipVoteButton.gameObject.SetActive(false);
            }
        }
    }
}