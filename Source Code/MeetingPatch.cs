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
                        if (pi.PlayerId == maxIdx) {
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
                                if (PlayerControl.GameOptions.AnonymousVotes)
                                {
                                    PlayerControl.SetPlayerMaterialColors(Palette.DisabledGrey, spriteRenderer);
                                }
                                else
                                {
                                    PlayerControl.SetPlayerMaterialColors((int)playerById.ColorId, spriteRenderer);
                                }
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
                                if (PlayerControl.GameOptions.AnonymousVotes)
                                {
                                    PlayerControl.SetPlayerMaterialColors(Palette.DisabledGrey, spriteRenderer2);
                                }
                                else
                                {
                                    PlayerControl.SetPlayerMaterialColors((int)playerById.ColorId, spriteRenderer2);
                                }
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

        static void populateButtonsPostfix(MeetingHud __instance) {
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

            // Add Swapper Buttons
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

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote))]
        class MeetingHudCastVotePatch {
            static void Postfix([HarmonyArgument(0)]byte srcPlayerId, [HarmonyArgument(1)]sbyte suspectPlayerId) {
                var source = Helpers.playerById(srcPlayerId);
                if (source != null && source.Data != null && AmongUsClient.Instance.AmHost && TheOtherRolesPlugin.HostSeesVotesLog.Value) {
                    string target = null;
                    if (suspectPlayerId == -2) target = "didn't vote";
                    else if (suspectPlayerId == -1) target = "skipped";
                    else if (suspectPlayerId >= 0) {
                        System.Console.WriteLine(suspectPlayerId);
                        System.Console.WriteLine((byte)suspectPlayerId);
                        var targetPlayer = Helpers.playerById((byte)suspectPlayerId);
                        if (targetPlayer != null && targetPlayer.Data != null) target = $"voted {targetPlayer.Data.PlayerName}";
                    }                    
                    
                    if (target != null) System.Console.WriteLine($"{source.Data.PlayerName} {target}");
                }
            }
        }
    }

    [HarmonyPatch(typeof(ExileController), "Begin")]
    class ExileBeginPatch {
        public static void Prefix(ExileController __instance, [HarmonyArgument(0)]ref GameData.PlayerInfo exiled, [HarmonyArgument(1)]bool tie) {
            // Shifter shift
            if (Shifter.shifter != null && AmongUsClient.Instance.AmHost && Shifter.futureShift != null) { // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShifterShift, Hazel.SendOption.Reliable, -1);
                writer.Write(Shifter.futureShift.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shifterShift(Shifter.futureShift.PlayerId);
            }
            Shifter.futureShift = null;

            // Eraser erase
            if (Eraser.eraser != null && AmongUsClient.Instance.AmHost && Eraser.futureErased != null) {  // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
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

            // SecurityGuard vents and cameras
            var allCameras = ShipStatus.Instance.AllCameras.ToList();
            MapOptions.camerasToAdd.ForEach(camera => {
                camera.gameObject.SetActive(true);
                camera.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                allCameras.Add(camera);
            });
            ShipStatus.Instance.AllCameras = allCameras.ToArray();
            MapOptions.camerasToAdd = new List<SurvCamera>();

            foreach (Vent vent in MapOptions.ventsToSeal) {
                PowerTools.SpriteAnim animator = vent.GetComponent<PowerTools.SpriteAnim>(); 
                animator?.Stop();
                vent.EnterVentAnim = vent.ExitVentAnim = null;
                vent.myRend.sprite = animator == null ? SecurityGuard.getStaticVentSealedSprite() : SecurityGuard.getAnimatedVentSealedSprite();
                vent.myRend.color = Color.white;
                vent.name = "SealedVent_" + vent.name;
            }
            MapOptions.ventsToSeal = new List<Vent>();
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
                if (Child.child != null && PlayerControl.LocalPlayer == Child.child && Child.child.Data.IsImpostor) {
                    var multiplier = Child.isGrownUp() ? 0.66f : 2f;
                    Child.child.SetKillTimer(PlayerControl.GameOptions.KillCooldown * multiplier);
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
                            HudManager.Instance.StartCoroutine(Effects.Lerp(Seer.soulDuration, new Action<float>((p) => {
                                if (rend != null) {
                                    var tmp = rend.color;
                                    tmp.a = Mathf.Clamp01(1 - p);
                                    rend.color = tmp;
                                }    
                                if (p == 1f && rend != null && rend.gameObject != null) UnityEngine.Object.Destroy(rend.gameObject);
                            })));
                        }
                    }
                    Seer.deadBodyPositions = new List<Vector3>();
                }

                // Arsonist deactivate dead poolable players
                if (Arsonist.arsonist != null && Arsonist.arsonist == PlayerControl.LocalPlayer) {
                    int visibleCounter = 0;
                    Vector3 bottomLeft = new Vector3(-HudManager.Instance.UseButton.transform.localPosition.x, HudManager.Instance.UseButton.transform.localPosition.y, HudManager.Instance.UseButton.transform.localPosition.z);
                    bottomLeft += new Vector3(-0.25f, -0.25f, 0);
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                        if (!Arsonist.dousedIcons.ContainsKey(p.PlayerId)) continue;
                        if (p.Data.IsDead || p.Data.Disconnected) {
                            Arsonist.dousedIcons[p.PlayerId].gameObject.SetActive(false);
                        } else {
                            Arsonist.dousedIcons[p.PlayerId].transform.localPosition = bottomLeft + Vector3.right * visibleCounter * 0.35f;
                            visibleCounter++;
                        }                        
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class MeetingExiledTextPatch
    {
        static void Postfix(ref string __result, [HarmonyArgument(0)]StringNames id, [HarmonyArgument(1)]Il2CppReferenceArray<Il2CppSystem.Object> parts)
        {
            if (ExileController.Instance != null && ExileController.Instance.exiled != null)
            {
                // Exile role text for roles that are being assigned to crewmates
                if (id == StringNames.ExileTextPN || id == StringNames.ExileTextSN)
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
                    else if(Hacker.hacker != null && ExileController.Instance.exiled.Object.PlayerId == Hacker.hacker.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Hacker.";
                    else if(Child.child != null && ExileController.Instance.exiled.Object.PlayerId == Child.child.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Child.";
                    else if(Tracker.tracker != null && ExileController.Instance.exiled.Object.PlayerId == Tracker.tracker.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Tracker.";
                    else if(Snitch.snitch != null && ExileController.Instance.exiled.Object.PlayerId == Snitch.snitch.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Snitch.";
                    else if(Jackal.jackal != null && ExileController.Instance.exiled.Object.PlayerId == Jackal.jackal.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Jackal.";
                    else if(Sidekick.sidekick != null && ExileController.Instance.exiled.Object.PlayerId == Sidekick.sidekick.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Sidekick.";
                    else if(Spy.spy != null && ExileController.Instance.exiled.Object.PlayerId == Spy.spy.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Spy.";
                    else if(SecurityGuard.securityGuard != null && ExileController.Instance.exiled.Object.PlayerId == SecurityGuard.securityGuard.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Security Guard.";
                    else if(Arsonist.arsonist != null && ExileController.Instance.exiled.Object.PlayerId == Arsonist.arsonist.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Arsonist.";
                    else
                        __result = ExileController.Instance.exiled.PlayerName + " was not The Impostor.";
                }
                // Exile role text for roles that are being assigned to impostors
                if (id == StringNames.ExileTextPP || id == StringNames.ExileTextSP) {
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
                    else if (Eraser.eraser != null && ExileController.Instance.exiled.Object.PlayerId == Eraser.eraser.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Eraser.";
                    else if (Trickster.trickster != null && ExileController.Instance.exiled.Object.PlayerId == Trickster.trickster.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Trickster.";
                    else if (Cleaner.cleaner != null && ExileController.Instance.exiled.Object.PlayerId == Cleaner.cleaner.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Cleaner.";
                    else if (Warlock.warlock != null && ExileController.Instance.exiled.Object.PlayerId == Warlock.warlock.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Warlock.";
                }

                // Hide number of remaining impostors on Jester win
                if (id == StringNames.ImpostorsRemainP || id == StringNames.ImpostorsRemainS)
                {
                    if (Jester.jester != null && ExileController.Instance.exiled.Object.PlayerId == Jester.jester.PlayerId)
                        __result = "";
                }
            }
        }
    }
}