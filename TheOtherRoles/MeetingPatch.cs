using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using static TheOtherRoles.TheOtherRoles;
using System.Collections;
using System;
using System.Text;
using UnityEngine;

namespace TheOtherRoles
{
    [HarmonyPatch(typeof(MeetingHud), "CalculateVotes")]
    class MeetingCalculateVotesPatch {
        static bool Prefix(MeetingHud __instance, ref Il2CppStructArray<byte> __result)
        {
            Il2CppStructArray<byte> array = new Il2CppStructArray<byte>(11);
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                if (playerVoteArea.didVote)
                {
                    int num = (int)(playerVoteArea.votedFor + 1);
                    if (num >= 0 && num < array.Length)
                    {
                        Il2CppStructArray<byte> array2 = array;
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

            __result = array;
            return false;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
    class MeetingPopulateVotesPatch {
        static bool Prefix(MeetingHud __instance, Il2CppStructArray<byte> GNKPMAGJLNC)
        {
            // Swapper swap votes
            PlayerVoteArea swapped1 = null;
            PlayerVoteArea swapped2 = null;

            foreach (PlayerVoteArea playerVoteArea in __instance.playerStates) {
                if (playerVoteArea.TargetPlayerId == Swapper.playerId1) swapped1 = playerVoteArea;
                if (playerVoteArea.TargetPlayerId == Swapper.playerId2) swapped2 = playerVoteArea;
            }

            bool doSwap = swapped1 != null && swapped2 != null;
            if (doSwap) {
                Reactor.Coroutines.Start(Helpers.Slide2D(swapped1.transform, swapped1.transform.localPosition, swapped2.transform.localPosition, 2f));
                Reactor.Coroutines.Start(Helpers.Slide2D(swapped2.transform, swapped2.transform.localPosition, swapped1.transform.localPosition, 2f));
            }

            // Mayor display vote twice
            __instance.TitleText.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
            int num = doSwap ? 4 : 0; // Delay animaton if swapping
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                playerVoteArea.ClearForResults();
                int num2 = doSwap ? 4 : 0; // Delay animaton if swapping
                bool mayorFirstVoteDisplayed = false;

                for (int j = 0; j < __instance.playerStates.Length; j++)
                {
                    PlayerVoteArea playerVoteArea2 = __instance.playerStates[j];
                    byte self = GNKPMAGJLNC[(int)playerVoteArea2.TargetPlayerId];

                    if (!((self & 128) > 0))
                    {
                        GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById((byte)playerVoteArea2.TargetPlayerId);
                        int votedFor = (int)PlayerVoteArea.GetVotedFor(self);
                        if (votedFor == (int)playerVoteArea.TargetPlayerId)
                        {
                            SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab);
                            if (PlayerControl.GameOptions.AnonymousVotes)
                            {
                                PlayerControl.SetPlayerMaterialColors(Palette.DisabledGrey, spriteRenderer);
                            }
                            else
                            {
                                PlayerControl.SetPlayerMaterialColors((int)playerById.ColorId, spriteRenderer);
                            }
                            spriteRenderer.transform.SetParent(playerVoteArea.transform);
                            spriteRenderer.transform.localPosition = __instance.CounterOrigin + new Vector3(__instance.CounterOffsets.x * (float)num2, 0f, 0f);
                            spriteRenderer.transform.localScale = Vector3.zero;
                            spriteRenderer.transform.SetParent(playerVoteArea.transform.parent); // Reparent votes so they don't move with their playerVoteArea
                            __instance.StartCoroutine(Effects.Bloop((float)num2 * 0.5f, spriteRenderer.transform, 1f, 0.5f));
                            num2++;
                        }
                        else if (i == 0 && votedFor == -1)
                        {
                            SpriteRenderer spriteRenderer2 = UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab);
                            if (PlayerControl.GameOptions.AnonymousVotes)
                            {
                                PlayerControl.SetPlayerMaterialColors(Palette.DisabledGrey, spriteRenderer2);
                            }
                            else
                            {
                                PlayerControl.SetPlayerMaterialColors((int)playerById.ColorId, spriteRenderer2);
                            }
                            spriteRenderer2.transform.SetParent(__instance.SkippedVoting.transform);
                            spriteRenderer2.transform.localPosition = __instance.CounterOrigin + new Vector3(__instance.CounterOffsets.x * (float)num, 0f, 0f);
                            spriteRenderer2.transform.localScale = Vector3.zero;
                            spriteRenderer2.transform.SetParent(playerVoteArea.transform.parent); // Reparent votes so they don't move with their playerVoteArea
                            __instance.StartCoroutine(Effects.Bloop((float)num * 0.5f, spriteRenderer2.transform, 1f, 0.5f));
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


    [HarmonyPatch]
    class MeetingHudPopulateButtonsPatch {
        static bool[] selections;
        static SpriteRenderer[] renderers;
        
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
        class MeetingHudVotingCompletedPatch {
            static void Postfix(MeetingHud __instance, byte[] GNKPMAGJLNC, GameData.PlayerInfo IHDMFDEEDEL, bool DCHFIBODGIL)
            {
                // Reset swapper values
                Swapper.playerId1 = Byte.MaxValue;
                Swapper.playerId2 = Byte.MaxValue;

                // Lovers save next to be exiled, because RPC of ending game comes before RPC of exiled
                Lovers.notAckedExiledIsLover = false;
                if (IHDMFDEEDEL != null)
                    Lovers.notAckedExiledIsLover = ((Lovers.lover1 != null && Lovers.lover1.PlayerId == IHDMFDEEDEL.PlayerId) || (Lovers.lover2 != null && Lovers.lover2.PlayerId == IHDMFDEEDEL.PlayerId));
            }
        }


        static void onClick(int i, MeetingHud __instance)
        {
            if (Swapper.swapper == null || PlayerControl.LocalPlayer != Swapper.swapper || Swapper.swapper.Data.IsDead) return; 
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

        static void addSwapperSwapButtons(MeetingHud __instance) {
            if (Swapper.swapper == null || PlayerControl.LocalPlayer != Swapper.swapper || Swapper.swapper.Data.IsDead) return; 

            selections = new bool[__instance.playerStates.Length];
            renderers = new SpriteRenderer[__instance.playerStates.Length];

            for (int i = 0; i < __instance.playerStates.Length; i++)
		    {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
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
                button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => onClick(copiedIndex, __instance)));
                


                selections[i] = false;
                renderers[i] = renderer;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.ServerStart))]
        class MeetingServerStartPatch {
            static void Postfix(MeetingHud __instance)
            {
                // Add swapper buttons
                addSwapperSwapButtons(__instance);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Deserialize))]
        class MeetingDeserializePatch {
            static void Postfix(MeetingHud __instance, MessageReader HFPCBBHJIPJ, bool CHDIOBMNLGH)
            {
                // Add swapper buttons
                if (CHDIOBMNLGH) {
                    addSwapperSwapButtons(__instance);
                }
            }
        }
    }

    [HarmonyPatch(typeof(ExileController), "Begin")]
    class ExileBeginPatch {

        public static void Prefix(ref GameData.PlayerInfo IHDMFDEEDEL, bool DCHFIBODGIL) {
            // Prevent growing Child exile
            if (Child.child != null && IHDMFDEEDEL != null && IHDMFDEEDEL.PlayerId == Child.child.PlayerId && !Child.isGrownUp()) {
                IHDMFDEEDEL = null;
            }
        }
    }


    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new Type[] { typeof(UnityEngine.Object) })]
    class MeetingExiledEndPatch
    {
        static void Prefix(UnityEngine.Object obj)
        {
            if (ExileController.Instance != null && obj == ExileController.Instance.gameObject)
            {
                // Reset custom button timers where necessary
                CustomButton.MeetingEndedUpdate();

                // Jester and Bounty Hunter win condition
                if (ExileController.Instance.exiled != null) {
                    byte exiledId = ExileController.Instance.exiled.PlayerId;
                    if ((Jester.jester != null && Jester.jester.PlayerId == exiledId) || (BountyHunter.bountyHunter != null && !BountyHunter.bountyHunter.Data.IsDead && BountyHunter.target != null && BountyHunter.target.PlayerId == exiledId)) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.JesterBountyHunterWin, Hazel.SendOption.Reliable, -1);
                        writer.Write(exiledId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.jesterBountyHunterWin(exiledId);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class MeetingExiledTextPatch
    {
        static void Postfix(ref string __result, StringNames DKEHCKOHMOH, Il2CppReferenceArray<Il2CppSystem.Object> DKBJCINDDCD)
        {
            if (ExileController.Instance != null && ExileController.Instance.exiled != null)
            {
                // Exile role text for roles that are being assigned to crewmates
                if (DKEHCKOHMOH == StringNames.ExileTextPN || DKEHCKOHMOH == StringNames.ExileTextSN)
                {
                    if( Jester.jester != null && ExileController.Instance.exiled.Object.PlayerId == Jester.jester.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Jester.";
                    else if(Mayor.mayor != null && ExileController.Instance.exiled.Object.PlayerId == Mayor.mayor.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Mayor.";
                    else if(Engineer.engineer != null && ExileController.Instance.exiled.Object.PlayerId == Engineer.engineer.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Engineer.";
                    else if(Sheriff.sheriff != null && ExileController.Instance.exiled.Object.PlayerId == Sheriff.sheriff.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Sheriff.";
                    else if(Lighter.lighter != null && ExileController.Instance.exiled.Object.PlayerId == Lighter.lighter.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Lighter.";
                    else if(Detective.detective != null && ExileController.Instance.exiled.Object.PlayerId == Detective.detective.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Detective.";
                    else if(TimeMaster.timeMaster != null && ExileController.Instance.exiled.Object.PlayerId == TimeMaster.timeMaster.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Time Master.";
                    else if(Medic.medic != null && ExileController.Instance.exiled.Object.PlayerId == Medic.medic.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Medic.";
                    else if(Shifter.shifter != null && ExileController.Instance.exiled.Object.PlayerId == Shifter.shifter.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Shifter.";
                    else if(Swapper.swapper != null && ExileController.Instance.exiled.Object.PlayerId == Swapper.swapper.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Swapper.";
                    else if(Lovers.lover1 != null && ExileController.Instance.exiled.Object.PlayerId == Lovers.lover1.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Lover.";
                    else if(Lovers.lover2 != null && ExileController.Instance.exiled.Object.PlayerId == Lovers.lover2.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Lover.";
                    else if(Seer.seer != null && ExileController.Instance.exiled.Object.PlayerId == Seer.seer.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Seer.";
                    else if(Spy.spy != null && ExileController.Instance.exiled.Object.PlayerId == Spy.spy.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Spy.";
                    else if(Child.child != null && ExileController.Instance.exiled.Object.PlayerId == Child.child.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Child.";
                    else if(BountyHunter.bountyHunter != null && ExileController.Instance.exiled.Object.PlayerId == BountyHunter.bountyHunter.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Bounty Hunter.";
                    else if(Tracker.tracker != null && ExileController.Instance.exiled.Object.PlayerId == Tracker.tracker.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Tracker.";
                    else if(Snitch.snitch != null && ExileController.Instance.exiled.Object.PlayerId == Snitch.snitch.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Snitch.";
                    else if(Jackal.jackal != null && ExileController.Instance.exiled.Object.PlayerId == Jackal.jackal.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Jackal.";
                    else if(Sidekick.sidekick != null && ExileController.Instance.exiled.Object.PlayerId == Sidekick.sidekick.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Sidekick.";
                    else
                        __result = ExileController.Instance.exiled.PlayerName + " was not The Impostor.";
                }
                // Exile role text for roles that are being assigned to impostors
                if (DKEHCKOHMOH == StringNames.ExileTextPP || DKEHCKOHMOH == StringNames.ExileTextSP) {
                    if(Godfather.godfather != null && ExileController.Instance.exiled.Object.PlayerId == Godfather.godfather.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Godfather.";
                    else if(Mafioso.mafioso != null && ExileController.Instance.exiled.Object.PlayerId == Mafioso.mafioso.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Mafioso.";
                    else if(Janitor.janitor != null && ExileController.Instance.exiled.Object.PlayerId == Janitor.janitor.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Janitor.";
                    else if(Morphling.morphling != null && ExileController.Instance.exiled.Object.PlayerId == Morphling.morphling.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Morphling.";
                    else if(Camouflager.camouflager != null && ExileController.Instance.exiled.Object.PlayerId == Camouflager.camouflager.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Camouflager.";
                    else if(Lovers.lover1 != null && ExileController.Instance.exiled.Object.PlayerId == Lovers.lover1.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The ImpLover.";
                    else if(Lovers.lover2 != null && ExileController.Instance.exiled.Object.PlayerId == Lovers.lover2.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The ImpLover.";
                    else if(Vampire.vampire != null && ExileController.Instance.exiled.Object.PlayerId == Vampire.vampire.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Vampire.";
                }

                // Hide number of remaining impostors on Jester win
                if (DKEHCKOHMOH == StringNames.ImpostorsRemainP || DKEHCKOHMOH == StringNames.ImpostorsRemainS)
                {
                    if (Jester.jester != null && ExileController.Instance.exiled.Object.PlayerId == Jester.jester.PlayerId)
                        __result = "";
                }
            }
        }
    }
}