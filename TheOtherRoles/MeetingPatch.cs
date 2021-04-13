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

using Effects = AEOEPNHOJDP;
using Palette = BLMBFIODBKL;

namespace TheOtherRoles
{
    [HarmonyPatch]
    class MeetingHudPatch {
        static bool[] selections;
        static SpriteRenderer[] renderers;
        private static GameData.LGBOMGHJELL target = null;

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.KEICNHGALLI))]
        class MeetingCalculateVotesPatch {
            private static byte[] calculateVotes(MeetingHud __instance) {
                byte[] array = new byte[__instance.GBKFCOAKLAH.Length + 1];
                for (int i = 0; i < __instance.GBKFCOAKLAH.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.GBKFCOAKLAH[i];
                    if (playerVoteArea.didVote)
                    {
                        int num = (int)(playerVoteArea.votedFor + 1);
                        if (num >= 0 && num < array.Length)
                        {
                            byte[] array2 = array;
                            int num2 = num;
                            // Mayor count vote twice
                            if (Mayor.mayor != null && playerVoteArea.GEIOMAPOPKA == (sbyte)Mayor.mayor.PlayerId)
                                array2[num2] += 2;
                            else
                                array2[num2] += 1;
                        }
                    }
                }

                // Swapper swap votes
                PlayerVoteArea swapped1 = null;
                PlayerVoteArea swapped2 = null;

                foreach (PlayerVoteArea playerVoteArea in __instance.GBKFCOAKLAH) {
                    if (playerVoteArea.GEIOMAPOPKA == Swapper.playerId1) swapped1 = playerVoteArea;
                    if (playerVoteArea.GEIOMAPOPKA == Swapper.playerId2) swapped2 = playerVoteArea;
                }

                if (swapped1 != null && swapped2 != null && swapped1.GEIOMAPOPKA + 1 >= 0 && swapped1.GEIOMAPOPKA + 1 < array.Length && swapped2.GEIOMAPOPKA + 1 >= 0 && swapped2.GEIOMAPOPKA + 1 < array.Length) {
                    byte tmp = array[swapped1.GEIOMAPOPKA + 1];
                    array[swapped1.GEIOMAPOPKA + 1] = array[swapped2.GEIOMAPOPKA + 1];
                    array[swapped2.GEIOMAPOPKA + 1] = tmp;
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

            static bool Prefix(MeetingHud __instance)
            {
                if (__instance.GBKFCOAKLAH.All((PlayerVoteArea ps) => ps.isDead || ps.didVote))
                {
                    // If skipping is disabled, replace skipps/no-votes with self vote
                    if (target == null && !allowSkipOnEmergencyMeetings) {
                        foreach (PlayerVoteArea playerVoteArea in __instance.GBKFCOAKLAH) {
                            if (playerVoteArea.votedFor < 0) playerVoteArea.votedFor = playerVoteArea.GEIOMAPOPKA; // TargetPlayerId
                        }
                    }

                    byte[] self = calculateVotes(__instance);
                    bool tie;
                    int maxIdx = IndexOfMax(self, (byte p) => (int)p, out tie) - 1;
                    GameData.LGBOMGHJELL exiled = null;
                    foreach (GameData.LGBOMGHJELL pi in GameData.Instance.AllPlayers) {
                        if (pi.FNPNJHNKEBK == maxIdx) {
                            exiled = pi;
                            break;
                        }
                    }
                    byte[] array = new byte[__instance.GBKFCOAKLAH.Length];
                    for (int i = 0; i < __instance.GBKFCOAKLAH.Length; i++)
                    {
                        PlayerVoteArea playerVoteArea = __instance.GBKFCOAKLAH[i];
                        array[(int)playerVoteArea.GEIOMAPOPKA] = playerVoteArea.GetState();
                    }
                    // RPCVotingComplete
                    if (AmongUsClient.Instance.BPADAHAOBLM)
                        __instance.BBFDNCCEJHI(array, exiled, tie);
                    MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId, 23, Hazel.SendOption.Reliable);
                    messageWriter.WriteBytesAndSize(array);
                    messageWriter.Write((exiled != null) ? exiled.FNPNJHNKEBK : byte.MaxValue);
                    messageWriter.Write(tie);
                    messageWriter.EndMessage();
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.FIJFIACMIFB))]
        class MeetingPopulateVotesPatch {
            static bool Prefix(MeetingHud __instance, Il2CppStructArray<byte> COMOIMMLKHF)
            {
                // Swapper swap votes
                PlayerVoteArea swapped1 = null;
                PlayerVoteArea swapped2 = null;

                foreach (PlayerVoteArea playerVoteArea in __instance.GBKFCOAKLAH) {
                    if (playerVoteArea.GEIOMAPOPKA == Swapper.playerId1) swapped1 = playerVoteArea;
                    if (playerVoteArea.GEIOMAPOPKA == Swapper.playerId2) swapped2 = playerVoteArea;
                }

                bool doSwap = swapped1 != null && swapped2 != null;
                if (doSwap) {

                    __instance.StartCoroutine(Effects.KLAIEICHCNO(swapped1.transform, swapped1.transform.localPosition, swapped2.transform.localPosition, 2f));
                    __instance.StartCoroutine(Effects.KLAIEICHCNO(swapped2.transform, swapped2.transform.localPosition, swapped1.transform.localPosition, 2f));
                }

                // Mayor display vote twice
                __instance.TitleText.text = DestroyableSingleton<TranslationController>.CHNDKKBEIDG.GetString(StringNames.MeetingVotingResults, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                int num = doSwap ? 4 : 0; // Delay animaton if swapping
                for (int i = 0; i < __instance.GBKFCOAKLAH.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.GBKFCOAKLAH[i];
                    playerVoteArea.ClearForResults();
                    int num2 = doSwap ? 4 : 0; // Delay animaton if swapping
                    bool mayorFirstVoteDisplayed = false;

                    for (int j = 0; j < __instance.GBKFCOAKLAH.Length; j++)
                    {
                        PlayerVoteArea playerVoteArea2 = __instance.GBKFCOAKLAH[j];
                        byte self = COMOIMMLKHF[(int)playerVoteArea2.GEIOMAPOPKA];

                        if (!((self & 128) > 0))
                        {
                            GameData.LGBOMGHJELL playerById = GameData.Instance.GetPlayerById((byte)playerVoteArea2.GEIOMAPOPKA);
                            int votedFor = (int)PlayerVoteArea.GetVotedFor(self);
                            if (votedFor == (int)playerVoteArea.GEIOMAPOPKA)
                            {
                                SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab);
                                if (PlayerControl.GameOptions.BBPDJOCPEEJ)
                                {
                                    PlayerControl.SetPlayerMaterialColors(Palette.EGHCBLDNCGP, spriteRenderer);
                                }
                                else
                                {
                                    PlayerControl.SetPlayerMaterialColors((int)playerById.IMMNCAGJJJC, spriteRenderer);
                                }
                                spriteRenderer.transform.SetParent(playerVoteArea.transform);
                                spriteRenderer.transform.localPosition = __instance.FAJKDFHIHDN + new Vector3(__instance.GGHFHCMCJAL.x * (float)num2, 0f, 0f);
                                spriteRenderer.transform.localScale = Vector3.zero;
                                spriteRenderer.transform.SetParent(playerVoteArea.transform.parent); // Reparent votes so they don't move with their playerVoteArea
                                __instance.StartCoroutine(Effects.JCDLOIMPBFJ((float)num2 * 0.5f, spriteRenderer.transform, 1f, 0.5f));
                                num2++;
                            }
                            else if (i == 0 && votedFor == -1)
                            {
                                SpriteRenderer spriteRenderer2 = UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab);
                                if (PlayerControl.GameOptions.BBPDJOCPEEJ)
                                {
                                    PlayerControl.SetPlayerMaterialColors(Palette.EGHCBLDNCGP, spriteRenderer2);
                                }
                                else
                                {
                                    PlayerControl.SetPlayerMaterialColors((int)playerById.IMMNCAGJJJC, spriteRenderer2);
                                }
                                spriteRenderer2.transform.SetParent(__instance.SkippedVoting.transform);
                                spriteRenderer2.transform.localPosition = __instance.FAJKDFHIHDN + new Vector3(__instance.GGHFHCMCJAL.x * (float)num, 0f, 0f);
                                spriteRenderer2.transform.localScale = Vector3.zero;
                                spriteRenderer2.transform.SetParent(playerVoteArea.transform.parent); // Reparent votes so they don't move with their playerVoteArea
                                __instance.StartCoroutine(Effects.JCDLOIMPBFJ((float)num * 0.5f, spriteRenderer2.transform, 1f, 0.5f));
                                num++;
                            }

                            // Major vote, redo this iteration to place a second vote
                            if (Mayor.mayor != null && playerVoteArea2.GEIOMAPOPKA == (sbyte)Mayor.mayor.PlayerId && !mayorFirstVoteDisplayed) {
                                mayorFirstVoteDisplayed = true;
                                j--;    
                            }
                        }
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BBFDNCCEJHI))]
        class MeetingHudVotingCompletedPatch {
            static void Postfix(MeetingHud __instance, byte[] COMOIMMLKHF, GameData.LGBOMGHJELL EAFLJGMBLCH, bool EMBDDLIPBME)
            {
                // Reset swapper values
                Swapper.playerId1 = Byte.MaxValue;
                Swapper.playerId2 = Byte.MaxValue;

                // Lovers save next to be exiled, because RPC of ending game comes before RPC of exiled
                Lovers.notAckedExiledIsLover = false;
                if (EAFLJGMBLCH != null)
                    Lovers.notAckedExiledIsLover = ((Lovers.lover1 != null && Lovers.lover1.PlayerId == EAFLJGMBLCH.FNPNJHNKEBK) || (Lovers.lover2 != null && Lovers.lover2.PlayerId == EAFLJGMBLCH.FNPNJHNKEBK));
            }
        }


        static void onClick(int i, MeetingHud __instance)
        {
            if (Swapper.swapper == null || PlayerControl.LocalPlayer != Swapper.swapper || Swapper.swapper.PPMOEEPBHJO.IAGJEKLJCCI) return; 
            if (__instance.FOIGOPKABAA == MeetingHud.MANCENPNMAC.Results) return;
            if (__instance.GBKFCOAKLAH[i].isDead) return;

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
                                secondPlayer = __instance.GBKFCOAKLAH[A];
                                break;
                            } else {
                                firstPlayer = __instance.GBKFCOAKLAH[A];
                            }
                        }
                    }

                    if (firstPlayer != null && secondPlayer != null) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SwapperSwap, Hazel.SendOption.Reliable, -1);
                        writer.Write((byte)firstPlayer.GEIOMAPOPKA);
                        writer.Write((byte)secondPlayer.GEIOMAPOPKA);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);

                        RPCProcedure.swapperSwap((byte)firstPlayer.GEIOMAPOPKA, (byte)secondPlayer.GEIOMAPOPKA);
                    }
                }
            }
        }

        static void populateButtonsPostfix(MeetingHud __instance) {
            // Reposition button if there are more than 10 players
            float scale = 5f / 8f; // 8 rows are needed instead of 5
            if (__instance.GBKFCOAKLAH != null && __instance.GBKFCOAKLAH.Length > 10) {
                for (int i = 0; i < __instance.GBKFCOAKLAH.Length; i++) {
                    PlayerVoteArea area = __instance.GBKFCOAKLAH[i];
                    bool isLeft = i % 2 == 0;
                    int num2 = i / 2;
                    area.transform.localPosition = __instance.VoteOrigin + new Vector3(isLeft ? 1f : 3.9f, __instance.VoteButtonOffsets.y * (float)num2 * scale, area.transform.localPosition.z);
                    area.transform.localScale = new Vector3(area.transform.localScale.x * scale, area.transform.localScale.y * scale, area.transform.localScale.z);
                }
            }

            // Add Swapper Buttons
            if (Swapper.swapper == null || PlayerControl.LocalPlayer != Swapper.swapper || Swapper.swapper.PPMOEEPBHJO.IAGJEKLJCCI) return; 
            selections = new bool[__instance.GBKFCOAKLAH.Length];
            renderers = new SpriteRenderer[__instance.GBKFCOAKLAH.Length];

            for (int i = 0; i < __instance.GBKFCOAKLAH.Length; i++)
		    {
                PlayerVoteArea playerVoteArea = __instance.GBKFCOAKLAH[i];
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
                populateButtonsPostfix(__instance);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Deserialize))]
        class MeetingDeserializePatch {
            static void Postfix(MeetingHud __instance, MessageReader JIGFBHFFNFI, bool DNMLJNIADHH)
            {
                // Add swapper buttons
                if (DNMLJNIADHH) {
                    populateButtonsPostfix(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CoStartMeeting))]
        class StartMeetingPatch {
            public static void Prefix(PlayerControl __instance, GameData.LGBOMGHJELL DGDGDKCCKHJ) {
                // Reset vampire bitten
                Vampire.bitten = null;
                // Count meetings
                if (DGDGDKCCKHJ == null) meetingsCount++;
                // Save the meeting target
                target = DGDGDKCCKHJ;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        class MeetingHudUpdatePatch {
            static void Postfix(MeetingHud __instance) {
                // Deactivate skip Button if skipping on emergency meetings is disabled
                if (target == null && !allowSkipOnEmergencyMeetings)
                    __instance.SkipVoteButton.gameObject.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(ExileController), "Begin")]
    class ExileBeginPatch {

        public static void Prefix(ref GameData.LGBOMGHJELL EAFLJGMBLCH, bool EMBDDLIPBME) {
            // Shifter shift
            if (Shifter.shifter != null && AmongUsClient.Instance.HHBLOCGKFAB && Shifter.futureShift != null) { // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShifterShift, Hazel.SendOption.Reliable, -1);
                writer.Write(Shifter.futureShift.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shifterShift(Shifter.futureShift.PlayerId);
            }
            Shifter.futureShift = null;

            // Eraser erase
            if (Eraser.eraser != null && AmongUsClient.Instance.HHBLOCGKFAB && Eraser.futureErased != null) {  // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                foreach (PlayerControl target in Eraser.futureErased) {
                    if (target != null) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ErasePlayerRole, Hazel.SendOption.Reliable, -1);
                        writer.Write(target.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.erasePlayerRole(target.PlayerId);
                    }
                }
            }
            Eraser.futureErased = new List<PlayerControl>();

            // Trickster boxes
            if (Trickster.trickster != null && JackInTheBox.hasJackInTheBoxLimitReached()) {
                JackInTheBox.convertToVents();
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
                // Child set adapted cooldown
                if (Child.child != null && PlayerControl.LocalPlayer == Child.child && Child.child.PPMOEEPBHJO.FDNMBJOAPFL) {
                    var multiplier = Child.isGrownUp() ? 0.66f : 2f;
                    Child.child.SetKillTimer(PlayerControl.GameOptions.DGOPNLEEAAJ * multiplier);
                }

                // Seer spawn souls
                if (Seer.deadBodyPositions != null && Seer.seer != null && PlayerControl.LocalPlayer == Seer.seer && (Seer.mode == 0 || Seer.mode == 2)) {
                    foreach (Vector3 pos in Seer.deadBodyPositions) {
                        GameObject soul = new GameObject();
                        soul.transform.position = pos;
                        soul.layer = 5;
                        var rend = soul.AddComponent<SpriteRenderer>();
                        rend.sprite = Seer.getSoulSprite();
                        
                        if(Seer.limitSoulDuration) {
                            PlayerControl.LocalPlayer.StartCoroutine(Effects.DCHLMIDMBHG(Seer.soulDuration, new Action<float>((p) => {
                                if (rend != null) {
                                    var tmp = rend.color;
                                    tmp.a = Mathf.Clamp01(1 - p);
                                    rend.color = tmp;
                                }    
                                if (p == 1f && rend?.gameObject != null) UnityEngine.Object.Destroy(rend.gameObject);
                            })));
                        }
                    }
                    Seer.deadBodyPositions = new List<Vector3>();
                }
            }
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class MeetingExiledTextPatch
    {
        static void Postfix(ref string __result, StringNames AKGLBKHCEMI, Il2CppReferenceArray<Il2CppSystem.Object> FHLKFONKJLH)
        {
            if (ExileController.Instance != null && ExileController.Instance.EAFLJGMBLCH != null)
            {
                // Exile role text for roles that are being assigned to crewmates
                if (AKGLBKHCEMI == StringNames.ExileTextPN || AKGLBKHCEMI == StringNames.ExileTextSN)
                {
                    if( Jester.jester != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Jester.jester.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Jester.";
                    else if(Mayor.mayor != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Mayor.mayor.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Mayor.";
                    else if(Engineer.engineer != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Engineer.engineer.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Engineer.";
                    else if(Sheriff.sheriff != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Sheriff.sheriff.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Sheriff.";
                    else if(Lighter.lighter != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Lighter.lighter.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Lighter.";
                    else if(Detective.detective != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Detective.detective.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Detective.";
                    else if(TimeMaster.timeMaster != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == TimeMaster.timeMaster.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Time Master.";
                    else if(Medic.medic != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Medic.medic.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Medic.";
                    else if(Shifter.shifter != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Shifter.shifter.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Shifter.";
                    else if(Swapper.swapper != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Swapper.swapper.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Swapper.";
                    else if(Lovers.lover1 != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Lovers.lover1.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Lover.";
                    else if(Lovers.lover2 != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Lovers.lover2.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Lover.";
                    else if(Seer.seer != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Seer.seer.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Seer.";
                    else if(Hacker.hacker != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Hacker.hacker.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Hacker.";
                    else if(Child.child != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Child.child.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Child.";
                    else if(Tracker.tracker != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Tracker.tracker.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Tracker.";
                    else if(Snitch.snitch != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Snitch.snitch.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Snitch.";
                    else if(Jackal.jackal != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Jackal.jackal.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Jackal.";
                    else if(Sidekick.sidekick != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Sidekick.sidekick.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Sidekick.";
                    else if(Spy.spy != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Spy.spy.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Spy.";
                    else
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was not The Impostor.";
                }
                // Exile role text for roles that are being assigned to impostors
                if (AKGLBKHCEMI == StringNames.ExileTextPP || AKGLBKHCEMI == StringNames.ExileTextSP) {
                    if(Godfather.godfather != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Godfather.godfather.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Godfather.";
                    else if(Mafioso.mafioso != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Mafioso.mafioso.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Mafioso.";
                    else if(Janitor.janitor != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Janitor.janitor.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Janitor.";
                    else if(Morphling.morphling != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Morphling.morphling.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Morphling.";
                    else if(Camouflager.camouflager != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Camouflager.camouflager.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Camouflager.";
                    else if(Lovers.lover1 != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Lovers.lover1.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The ImpLover.";
                    else if(Lovers.lover2 != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Lovers.lover2.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The ImpLover.";
                    else if(Vampire.vampire != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Vampire.vampire.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Vampire.";
                    else if (Eraser.eraser != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Eraser.eraser.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Eraser.";
                    else if (Trickster.trickster != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Trickster.trickster.PlayerId)
                        __result = ExileController.Instance.EAFLJGMBLCH.PCLLABJCIPC + " was The Trickster.";
                }

                // Hide number of remaining impostors on Jester win
                if (AKGLBKHCEMI == StringNames.ImpostorsRemainP || AKGLBKHCEMI == StringNames.ImpostorsRemainS)
                {
                    if (Jester.jester != null && ExileController.Instance.EAFLJGMBLCH.GJPBCGFPMOD.PlayerId == Jester.jester.PlayerId)
                        __result = "";
                }
            }
        }
    }
}