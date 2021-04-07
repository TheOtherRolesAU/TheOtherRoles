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
using System.Reflection;

using Effects = HLPCBNMDEHF;
using Palette = GLNPIJPGGNJ;

namespace TheOtherRoles
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CDLGIAMFHBH))]
    class MeetingCalculateVotesPatch {
        private static byte[] calculateVotes(MeetingHud __instance) {
            byte[] array = new byte[11];
            for (int i = 0; i < __instance.DHCOPOOJCLN.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.DHCOPOOJCLN[i];
                if (playerVoteArea.didVote)
                {
                    int num = (int)(playerVoteArea.votedFor + 1);
                    if (num >= 0 && num < array.Length)
                    {
                        byte[] array2 = array;
                        int num2 = num;
                        // Mayor count vote twice
                        if (Mayor.mayor != null && playerVoteArea.HMPHKKGPLAG == (sbyte)Mayor.mayor.PlayerId)
                            array2[num2] += 2;
                        else
                            array2[num2] += 1;
                    }
                }
            }

            // Swapper swap votes
            PlayerVoteArea swapped1 = null;
            PlayerVoteArea swapped2 = null;

            foreach (PlayerVoteArea playerVoteArea in __instance.DHCOPOOJCLN) {
                if (playerVoteArea.HMPHKKGPLAG == Swapper.playerId1) swapped1 = playerVoteArea;
                if (playerVoteArea.HMPHKKGPLAG == Swapper.playerId2) swapped2 = playerVoteArea;
            }

            if (swapped1 != null && swapped2 != null && swapped1.HMPHKKGPLAG + 1 >= 0 && swapped1.HMPHKKGPLAG + 1 < array.Length && swapped2.HMPHKKGPLAG + 1 >= 0 && swapped2.HMPHKKGPLAG + 1 < array.Length) {
                byte tmp = array[swapped1.HMPHKKGPLAG + 1];
                array[swapped1.HMPHKKGPLAG + 1] = array[swapped2.HMPHKKGPLAG + 1];
                array[swapped2.HMPHKKGPLAG + 1] = tmp;
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
            if (__instance.DHCOPOOJCLN.All((PlayerVoteArea ps) => ps.isDead || ps.didVote))
            {
                byte[] self = calculateVotes(__instance);
                bool tie;
                int maxIdx = IndexOfMax(self, (byte p) => (int)p, out tie) - 1;
                GameData.OFKOJOKOOAK exiled = null;
                foreach (GameData.OFKOJOKOOAK pi in GameData.Instance.AllPlayers) {
                    if (pi.GMBAIPNOKLP == maxIdx) {
                        exiled = pi;
                        break;
                    }
                }
                byte[] array = new byte[10];
                for (int i = 0; i < __instance.DHCOPOOJCLN.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.DHCOPOOJCLN[i];
                    array[(int)playerVoteArea.HMPHKKGPLAG] = playerVoteArea.GetState();
                }
                // RPCVotingComplete
                if (AmongUsClient.Instance.HNMILJEOEKN)
                    __instance.MJIJGEBBMAO(array, exiled, tie);
                MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId, 23, Hazel.SendOption.Reliable);
                messageWriter.WriteBytesAndSize(array);
                messageWriter.Write((exiled != null) ? exiled.GMBAIPNOKLP : byte.MaxValue);
                messageWriter.Write(tie);
                messageWriter.EndMessage();
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OMFOBKIENPI))]
    class MeetingPopulateVotesPatch {
        static bool Prefix(MeetingHud __instance, Il2CppStructArray<byte> HIDHPMAKEKH)
        {
            // Swapper swap votes
            PlayerVoteArea swapped1 = null;
            PlayerVoteArea swapped2 = null;

            foreach (PlayerVoteArea playerVoteArea in __instance.DHCOPOOJCLN) {
                if (playerVoteArea.HMPHKKGPLAG == Swapper.playerId1) swapped1 = playerVoteArea;
                if (playerVoteArea.HMPHKKGPLAG == Swapper.playerId2) swapped2 = playerVoteArea;
            }

            bool doSwap = swapped1 != null && swapped2 != null;
            if (doSwap) {

                __instance.StartCoroutine(Effects.NFAIFCPOFJK(swapped1.transform, swapped1.transform.localPosition, swapped2.transform.localPosition, 2f));
                __instance.StartCoroutine(Effects.NFAIFCPOFJK(swapped2.transform, swapped2.transform.localPosition, swapped1.transform.localPosition, 2f));
            }

            // Mayor display vote twice
            __instance.TitleText.Text = DestroyableSingleton<TranslationController>.CMJOLNCMAPD.GetString(StringNames.MeetingVotingResults, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
            int num = doSwap ? 4 : 0; // Delay animaton if swapping
            for (int i = 0; i < __instance.DHCOPOOJCLN.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.DHCOPOOJCLN[i];
                playerVoteArea.ClearForResults();
                int num2 = doSwap ? 4 : 0; // Delay animaton if swapping
                bool mayorFirstVoteDisplayed = false;

                for (int j = 0; j < __instance.DHCOPOOJCLN.Length; j++)
                {
                    PlayerVoteArea playerVoteArea2 = __instance.DHCOPOOJCLN[j];
                    byte self = HIDHPMAKEKH[(int)playerVoteArea2.HMPHKKGPLAG];

                    if (!((self & 128) > 0))
                    {
                        GameData.OFKOJOKOOAK playerById = GameData.Instance.GetPlayerById((byte)playerVoteArea2.HMPHKKGPLAG);
                        int votedFor = (int)PlayerVoteArea.GetVotedFor(self);
                        if (votedFor == (int)playerVoteArea.HMPHKKGPLAG)
                        {
                            SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab);
                            if (PlayerControl.GameOptions.LNMFPEMGKOB)
                            {
                                PlayerControl.SetPlayerMaterialColors(Palette.JMELLHINKGM, spriteRenderer);
                            }
                            else
                            {
                                PlayerControl.SetPlayerMaterialColors((int)playerById.JFHFMIKFHGG, spriteRenderer);
                            }
                            spriteRenderer.transform.SetParent(playerVoteArea.transform);
                            spriteRenderer.transform.localPosition = __instance.ALGONDAMLHA + new Vector3(__instance.LKIOFMMBOBJ.x * (float)num2, 0f, 0f);
                            spriteRenderer.transform.localScale = Vector3.zero;
                            spriteRenderer.transform.SetParent(playerVoteArea.transform.parent); // Reparent votes so they don't move with their playerVoteArea
                            __instance.StartCoroutine(Effects.POFLJMGFBEJ((float)num2 * 0.5f, spriteRenderer.transform, 1f, 0.5f));
                            num2++;
                        }
                        else if (i == 0 && votedFor == -1)
                        {
                            SpriteRenderer spriteRenderer2 = UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab);
                            if (PlayerControl.GameOptions.LNMFPEMGKOB)
                            {
                                PlayerControl.SetPlayerMaterialColors(Palette.JMELLHINKGM, spriteRenderer2);
                            }
                            else
                            {
                                PlayerControl.SetPlayerMaterialColors((int)playerById.JFHFMIKFHGG, spriteRenderer2);
                            }
                            spriteRenderer2.transform.SetParent(__instance.SkippedVoting.transform);
                            spriteRenderer2.transform.localPosition = __instance.ALGONDAMLHA + new Vector3(__instance.LKIOFMMBOBJ.x * (float)num, 0f, 0f);
                            spriteRenderer2.transform.localScale = Vector3.zero;
                            spriteRenderer2.transform.SetParent(playerVoteArea.transform.parent); // Reparent votes so they don't move with their playerVoteArea
                            __instance.StartCoroutine(Effects.POFLJMGFBEJ((float)num * 0.5f, spriteRenderer2.transform, 1f, 0.5f));
                            num++;
                        }

                        // Major vote, redo this iteration to place a second vote
                        if (Mayor.mayor != null && playerVoteArea2.HMPHKKGPLAG == (sbyte)Mayor.mayor.PlayerId && !mayorFirstVoteDisplayed) {
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
        
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.MJIJGEBBMAO))]
        class MeetingHudVotingCompletedPatch {
            static void Postfix(MeetingHud __instance, byte[] HIDHPMAKEKH, GameData.OFKOJOKOOAK KLHCDCKJHKC, bool EMLKEPIBJLK)
            {
                // Reset swapper values
                Swapper.playerId1 = Byte.MaxValue;
                Swapper.playerId2 = Byte.MaxValue;

                // Lovers save next to be exiled, because RPC of ending game comes before RPC of exiled
                Lovers.notAckedExiledIsLover = false;
                if (KLHCDCKJHKC != null)
                    Lovers.notAckedExiledIsLover = ((Lovers.lover1 != null && Lovers.lover1.PlayerId == KLHCDCKJHKC.GMBAIPNOKLP) || (Lovers.lover2 != null && Lovers.lover2.PlayerId == KLHCDCKJHKC.GMBAIPNOKLP));
            }
        }


        static void onClick(int i, MeetingHud __instance)
        {
            if (Swapper.swapper == null || PlayerControl.LocalPlayer != Swapper.swapper || Swapper.swapper.IDOFAMCIJKE.FGNJJFABIHJ) return; 
            if (__instance.MJMOOPLLNPO == MeetingHud.DDMMMDGMFIK.Results) return;
            if (__instance.DHCOPOOJCLN[i].isDead) return;

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
                                secondPlayer = __instance.DHCOPOOJCLN[A];
                                break;
                            } else {
                                firstPlayer = __instance.DHCOPOOJCLN[A];
                            }
                        }
                    }

                    if (firstPlayer != null && secondPlayer != null) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SwapperSwap, Hazel.SendOption.Reliable, -1);
                        writer.Write((byte)firstPlayer.HMPHKKGPLAG);
                        writer.Write((byte)secondPlayer.HMPHKKGPLAG);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);

                        RPCProcedure.swapperSwap((byte)firstPlayer.HMPHKKGPLAG, (byte)secondPlayer.HMPHKKGPLAG);
                    }
                }
            }
        }

        static void addSwapperSwapButtons(MeetingHud __instance) {
            if (Swapper.swapper == null || PlayerControl.LocalPlayer != Swapper.swapper || Swapper.swapper.IDOFAMCIJKE.FGNJJFABIHJ) return; 

            selections = new bool[__instance.DHCOPOOJCLN.Length];
            renderers = new SpriteRenderer[__instance.DHCOPOOJCLN.Length];

            for (int i = 0; i < __instance.DHCOPOOJCLN.Length; i++)
		    {
                PlayerVoteArea playerVoteArea = __instance.DHCOPOOJCLN[i];
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
            static void Postfix(MeetingHud __instance, MessageReader DOOILGKLBBF, bool IHJEKEOFMGJ)
            {
                // Add swapper buttons
                if (IHJEKEOFMGJ) {
                    addSwapperSwapButtons(__instance);
                }
            }
        }
    }

    [HarmonyPatch(typeof(ExileController), "Begin")]
    class ExileBeginPatch {

        public static void Prefix(ref GameData.OFKOJOKOOAK KLHCDCKJHKC, bool EMLKEPIBJLK) {
            // Shifter shift
            if (Shifter.shifter != null && AmongUsClient.Instance.CBKCIKKEJHI && Shifter.futureShift != null) { // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShifterShift, Hazel.SendOption.Reliable, -1);
                writer.Write(Shifter.futureShift.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shifterShift(Shifter.futureShift.PlayerId);
            }
            Shifter.futureShift = null;

            // Eraser erase
            if (Eraser.eraser != null && AmongUsClient.Instance.CBKCIKKEJHI && Eraser.futureErased != null) {  // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
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
                if (Child.child != null && PlayerControl.LocalPlayer == Child.child && Child.child.IDOFAMCIJKE.CIDDOFDJHJH) {
                    var multiplier = Child.isGrownUp() ? 0.66f : 2f;
                    Child.child.SetKillTimer(PlayerControl.GameOptions.ELBDIKIOHHH * multiplier);
                }

                if (ExileController.Instance.KLHCDCKJHKC != null) {
                    byte exiledId = ExileController.Instance.KLHCDCKJHKC.GMBAIPNOKLP;

                    // Child lose condition
                    if (Child.child != null && Child.child.PlayerId == exiledId && !Child.isGrownUp() && !Child.child.IDOFAMCIJKE.CIDDOFDJHJH) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ChildLose, Hazel.SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.childLose();
                    }
                    // Jester and Bounty Hunter win condition
                    else if ((Jester.jester != null && Jester.jester.PlayerId == exiledId) || (BountyHunter.bountyHunter != null && !BountyHunter.bountyHunter.IDOFAMCIJKE.FGNJJFABIHJ && BountyHunter.target != null && BountyHunter.target.PlayerId == exiledId)) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.JesterBountyHunterWin, Hazel.SendOption.Reliable, -1);
                        writer.Write(exiledId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.jesterBountyHunterWin(exiledId);
                    }
                }

                // Seer spawn souls
                if (Seer.deadBodyPositions != null && Seer.seer != null && PlayerControl.LocalPlayer == Seer.seer && (Seer.mode == 0 || Seer.mode == 2)) {
                    foreach (Vector3 pos in Seer.deadBodyPositions) {
                        GameObject soul = new GameObject();
                        soul.transform.position = pos;
                        soul.layer = 5;
                        var rend = soul.AddComponent<SpriteRenderer>();
                        rend.sprite = Seer.getSoulSprite();
                        
                        PlayerControl.LocalPlayer.StartCoroutine(Effects.LDACHPMFOIF(Seer.soulDuration, new Action<float>((p) => {
                            if (rend != null) {
                                var tmp = rend.color;
                                tmp.a = Mathf.Clamp01(1 - p);
                                rend.color = tmp;
                            }    
                            if (p == 1f && rend?.gameObject != null) UnityEngine.Object.Destroy(rend.gameObject);
                        })));
                    }
                    Seer.deadBodyPositions = new List<Vector3>();
                }
            }
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class MeetingExiledTextPatch
    {
        static void Postfix(ref string __result, StringNames MKFNKGIBBHP, Il2CppReferenceArray<Il2CppSystem.Object> BPBFAAEIABN)
        {
            if (ExileController.Instance != null && ExileController.Instance.KLHCDCKJHKC != null)
            {
                // Exile role text for roles that are being assigned to crewmates
                if (MKFNKGIBBHP == StringNames.ExileTextPN || MKFNKGIBBHP == StringNames.ExileTextSN)
                {
                    if( Jester.jester != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Jester.jester.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Jester.";
                    else if(Mayor.mayor != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Mayor.mayor.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Mayor.";
                    else if(Engineer.engineer != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Engineer.engineer.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Engineer.";
                    else if(Sheriff.sheriff != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Sheriff.sheriff.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Sheriff.";
                    else if(Lighter.lighter != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Lighter.lighter.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Lighter.";
                    else if(Detective.detective != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Detective.detective.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Detective.";
                    else if(TimeMaster.timeMaster != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == TimeMaster.timeMaster.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Time Master.";
                    else if(Medic.medic != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Medic.medic.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Medic.";
                    else if(Shifter.shifter != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Shifter.shifter.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Shifter.";
                    else if(Swapper.swapper != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Swapper.swapper.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Swapper.";
                    else if(Lovers.lover1 != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Lovers.lover1.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Lover.";
                    else if(Lovers.lover2 != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Lovers.lover2.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Lover.";
                    else if(Seer.seer != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Seer.seer.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Seer.";
                    else if(Hacker.hacker != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Hacker.hacker.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Hacker.";
                    else if(Child.child != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Child.child.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Child.";
                    else if(BountyHunter.bountyHunter != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == BountyHunter.bountyHunter.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Bounty Hunter.";
                    else if(Tracker.tracker != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Tracker.tracker.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Tracker.";
                    else if(Snitch.snitch != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Snitch.snitch.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Snitch.";
                    else if(Jackal.jackal != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Jackal.jackal.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Jackal.";
                    else if(Sidekick.sidekick != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Sidekick.sidekick.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Sidekick.";
                    else
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was not The Impostor.";
                }
                // Exile role text for roles that are being assigned to impostors
                if (MKFNKGIBBHP == StringNames.ExileTextPP || MKFNKGIBBHP == StringNames.ExileTextSP) {
                    if(Godfather.godfather != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Godfather.godfather.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Godfather.";
                    else if(Mafioso.mafioso != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Mafioso.mafioso.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Mafioso.";
                    else if(Janitor.janitor != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Janitor.janitor.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Janitor.";
                    else if(Morphling.morphling != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Morphling.morphling.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Morphling.";
                    else if(Camouflager.camouflager != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Camouflager.camouflager.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Camouflager.";
                    else if(Lovers.lover1 != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Lovers.lover1.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The ImpLover.";
                    else if(Lovers.lover2 != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Lovers.lover2.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The ImpLover.";
                    else if(Vampire.vampire != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Vampire.vampire.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Vampire.";
                    else if (Eraser.eraser != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Eraser.eraser.PlayerId)
                        __result = ExileController.Instance.KLHCDCKJHKC.HGGCLJHCDBM + " was The Eraser.";
                }

                // Hide number of remaining impostors on Jester win
                if (MKFNKGIBBHP == StringNames.ImpostorsRemainP || MKFNKGIBBHP == StringNames.ImpostorsRemainS)
                {
                    if (Jester.jester != null && ExileController.Instance.KLHCDCKJHKC.GPBBCHGPABL.PlayerId == Jester.jester.PlayerId)
                        __result = "";
                }
            }
        }
    }
}