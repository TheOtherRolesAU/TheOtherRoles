using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.GameHistory;
using UnityEngine;

namespace TheOtherRoles {
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class PlayerControlFixedUpdatePatch
    {
        // Helpers

        static PlayerControl setTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null) {
            PlayerControl result = null;
            float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            if (!ShipStatus.Instance) return result;
            if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
            if (targetingPlayer.Data.IsDead) return result;
		
            Vector2 truePosition = targetingPlayer.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];
                if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead && (!onlyCrewmates || !playerInfo.IsImpostor))
                {
                    PlayerControl @object = playerInfo.Object;
                    if(untargetablePlayers != null && untargetablePlayers.Any(x => x == @object)) {
                        // if that player is not targetable: skip check
                        continue;
                    }

                    if (@object && (!@object.inVent || targetPlayersInVents))
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }

        static void setPlayerOutline(PlayerControl target, Color color) {
            if (target == null || target.myRend == null) return;
            
            target.myRend.material.SetFloat("_Outline", 1f);
            target.myRend.material.SetColor("_OutlineColor", color);
        }

        // Update functions

        static void setBasePlayerOutlines() {
            foreach (PlayerControl target in PlayerControl.AllPlayerControls) {
                if (target == null || target.myRend == null) continue;
                
                bool isMorphedMorphling = target == Morphling.morphling && Morphling.morphTarget != null && Morphling.morphTimer > 0f;
                bool hasVisibleShield = false;
                if (Camouflager.camouflageTimer <= 0f && Medic.shielded != null && ((target == Medic.shielded && !isMorphedMorphling) || (isMorphedMorphling && Morphling.morphTarget == Medic.shielded))) {
                    hasVisibleShield = Medic.showShielded == 0 // Everyone
                        || (Medic.showShielded == 1 && (PlayerControl.LocalPlayer == Medic.shielded || PlayerControl.LocalPlayer == Medic.medic)) // Shielded + Medic
                        || (Medic.showShielded == 2 && PlayerControl.LocalPlayer == Medic.medic); // Medic only
                }

                if (hasVisibleShield) {
                    target.myRend.material.SetFloat("_Outline", 1f);
                    target.myRend.material.SetColor("_OutlineColor", Medic.shieldedColor);
                } else {
                    target.myRend.material.SetFloat("_Outline", 0f);
                }
            }
        }

        public static void bendTimeUpdate() {
            if (TimeMaster.isRewinding) {
                if (localPlayerPositions.Count > 0) {
                    // Set position
                    var next = localPlayerPositions[0];
                    if (next.Item2 == true) {
                        // Exit current vent if necessary
                        if (PlayerControl.LocalPlayer.inVent) {
                            foreach (Vent vent in ShipStatus.Instance.AllVents) {
                                bool canUse;
                                bool couldUse;
                                vent.CanUse(PlayerControl.LocalPlayer.Data, out canUse, out couldUse);
                                if (canUse) {
                                    PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(vent.Id);
                                    vent.SetButtons(false);
                                }
                            }
                        }
                        // Set position
                        PlayerControl.LocalPlayer.transform.position = next.Item1;
                    } else if (localPlayerPositions.Any(x => x.Item2 == true)) {
                        PlayerControl.LocalPlayer.transform.position = next.Item1;
                    }
                        
                    localPlayerPositions.RemoveAt(0);

                    if (localPlayerPositions.Count > 1) localPlayerPositions.RemoveAt(0); // Skip every second position to rewinde twice as fast, but never skip the last position
                } else {
                    TimeMaster.isRewinding = false;
                    PlayerControl.LocalPlayer.moveable = true;
                }
            } else {
                while (localPlayerPositions.Count >= Mathf.Round(TimeMaster.rewindTime / Time.fixedDeltaTime)) localPlayerPositions.RemoveAt(localPlayerPositions.Count - 1);
                localPlayerPositions.Insert(0, new Tuple<Vector3, bool>(PlayerControl.LocalPlayer.transform.position, PlayerControl.LocalPlayer.CanMove)); // CanMove = CanMove
            }
        }

        static void medicSetTarget() {
            if (Medic.medic == null || Medic.medic != PlayerControl.LocalPlayer) return;
            Medic.currentTarget = setTarget();
            if (!Medic.usedShield) setPlayerOutline(Medic.currentTarget, Medic.shieldedColor);
        }

        static void shifterSetTarget() {
            if (Shifter.shifter == null || Shifter.shifter != PlayerControl.LocalPlayer) return;
            Shifter.currentTarget = setTarget();
            if (Shifter.futureShift == null) setPlayerOutline(Shifter.currentTarget, Shifter.color);
        }

        
        static void morphlingSetTarget() {
            if (Morphling.morphling == null || Morphling.morphling != PlayerControl.LocalPlayer) return;
            Morphling.currentTarget = setTarget();
            setPlayerOutline(Morphling.currentTarget, Morphling.color);
        }
        
        static void sheriffSetTarget() {
            if (Sheriff.sheriff == null || Sheriff.sheriff != PlayerControl.LocalPlayer) return;
            Sheriff.currentTarget = setTarget();
            setPlayerOutline(Sheriff.currentTarget, Sheriff.color);
        }

        static void trackerSetTarget() {
            if (Tracker.tracker == null || Tracker.tracker != PlayerControl.LocalPlayer) return;
            Tracker.currentTarget = setTarget();
            if (!Tracker.usedTracker) setPlayerOutline(Tracker.currentTarget, Tracker.color);
        }

        static void detectiveUpdateFootPrints() {            
            if (Detective.detective == null || Detective.detective != PlayerControl.LocalPlayer) return;

            Detective.timer -= Time.fixedDeltaTime;
            if (Detective.timer <= 0f) {
                Detective.timer = Detective.footprintIntervall;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                    if (player != null && player != PlayerControl.LocalPlayer && !player.Data.IsDead && !player.inVent) {
                        new Footprint(Detective.footprintDuration, Detective.anonymousFootprints, player);
                    }
                }
            }
        }

        static void vampireSetTarget() {
            if (Vampire.vampire == null || Vampire.vampire != PlayerControl.LocalPlayer) return;

		    PlayerControl target = null;
            if (Spy.spy != null) {
                if (Spy.impostorsCanKillAnyone) {
                    target = setTarget(false, true);
                } else {
                    target = setTarget(true, true, new List<PlayerControl>() { Spy.spy });
                }
            } else {
                target = setTarget(true, true);
            }

            bool targetNearGarlic = false;
            if (target != null) {
                foreach (Garlic garlic in Garlic.garlics) {
                    if (Vector2.Distance(garlic.garlic.transform.position, target.transform.position) <= 1.91f) {
                        targetNearGarlic = true;
                    }
                }
            }
            Vampire.targetNearGarlic = targetNearGarlic;
            Vampire.currentTarget = target;
            setPlayerOutline(Vampire.currentTarget, Vampire.color);
        }

        static void jackalSetTarget() {
            if (Jackal.jackal == null || Jackal.jackal != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if(Jackal.canCreateSidekickFromImpostor) {
                // Only exclude sidekick from beeing targeted if the jackal can create sidekicks from impostors
                if(Sidekick.sidekick != null) untargetablePlayers.Add(Sidekick.sidekick);
            }
            if(Child.child != null && !Child.isGrownUp()) untargetablePlayers.Add(Child.child); // Exclude Jackal from targeting the Child unless it has grown up
            Jackal.currentTarget = setTarget(untargetablePlayers : untargetablePlayers);
            setPlayerOutline(Jackal.currentTarget, Palette.ImpostorRed);
        }

        static void sidekickSetTarget() {
            if (Sidekick.sidekick == null || Sidekick.sidekick != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if(Jackal.jackal != null) untargetablePlayers.Add(Jackal.jackal);
            if(Child.child != null && !Child.isGrownUp()) untargetablePlayers.Add(Child.child); // Exclude Sidekick from targeting the Child unless it has grown up
            Sidekick.currentTarget = setTarget(untargetablePlayers : untargetablePlayers);
            if (Sidekick.canKill) setPlayerOutline(Sidekick.currentTarget, Palette.ImpostorRed);
        }

        static void sidekickCheckPromotion() {
            // If LocalPlayer is Sidekick, the Jackal is disconnected and Sidekick promotion is enabled, then trigger promotion
            if (Sidekick.sidekick == null || Sidekick.sidekick != PlayerControl.LocalPlayer) return;
            if (Sidekick.sidekick.Data.IsDead == true || !Sidekick.promotesToJackal) return;
            if (Jackal.jackal == null || Jackal.jackal?.Data?.Disconnected == true) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.sidekickPromotes(); 
            }
        }

        static void eraserSetTarget() {
            if (Eraser.eraser == null || Eraser.eraser != PlayerControl.LocalPlayer) return;

            List<PlayerControl> untargatables = new List<PlayerControl>();
            if (Spy.spy != null) untargatables.Add(Spy.spy);
            Eraser.currentTarget = setTarget(onlyCrewmates: !Eraser.canEraseAnyone, untargetablePlayers: Eraser.canEraseAnyone ? new List<PlayerControl>() : untargatables);
            setPlayerOutline(Eraser.currentTarget, Eraser.color);
        }

        static void engineerUpdate() {
            if (PlayerControl.LocalPlayer.Data.IsImpostor && ShipStatus.Instance?.AllVents != null) {
                foreach (Vent vent in ShipStatus.Instance.AllVents) {
                    try {
                        if (vent?.myRend?.material != null) {
                            if (Engineer.engineer != null && Engineer.engineer.inVent) {
                                vent.myRend.material.SetFloat("_Outline", 1f);
                                vent.myRend.material.SetColor("_OutlineColor", Engineer.color);
                            } else if (vent.myRend.material.GetColor("_AddColor") != Color.red) {
                                vent.myRend.material.SetFloat("_Outline", 0);
                            }
                        }
                    } catch {}
                }
            }
        }

        static void impostorSetTarget() {
            if (!PlayerControl.LocalPlayer.Data.IsImpostor ||!PlayerControl.LocalPlayer.CanMove || PlayerControl.LocalPlayer.Data.IsDead) { // !isImpostor || !canMove || isDead
                HudManager.Instance.KillButton.SetTarget(null);
                return;
            }
            
            PlayerControl target = null; 
            if (Spy.spy != null) {
                if (Spy.impostorsCanKillAnyone) {
                    target = setTarget(false, true);
                } else {
                    target = setTarget(true, true, new List<PlayerControl>() { Spy.spy });
                }
            } else {
                target = setTarget(true, true);
            }

            HudManager.Instance.KillButton.SetTarget(target); // Includes setPlayerOutline(target, Palette.ImpstorRed);
        }

        static void warlockSetTarget() {
            if (Warlock.warlock == null || Warlock.warlock != PlayerControl.LocalPlayer) return;
            if (Warlock.curseVictim != null && (Warlock.curseVictim.Data.Disconnected || Warlock.curseVictim.Data.IsDead)) {
                // If the cursed victim is disconnected or dead reset the curse so a new curse can be applied
                Warlock.resetCurse();
            }
            if (Warlock.curseVictim == null) {
                Warlock.currentTarget = setTarget();
                setPlayerOutline(Warlock.currentTarget, Warlock.color);
            } else {
                Warlock.curseVictimTarget = setTarget(targetingPlayer: Warlock.curseVictim);
                setPlayerOutline(Warlock.curseVictimTarget, Warlock.color);
            }
        }

        static void trackerUpdate() {
            if (Tracker.arrow?.arrow == null) return;

            if (Tracker.tracker == null || PlayerControl.LocalPlayer != Tracker.tracker) {
                Tracker.arrow.arrow.SetActive(false);
                return;
            }

            if (Tracker.tracker != null && Tracker.tracked != null && PlayerControl.LocalPlayer == Tracker.tracker && !Tracker.tracker.Data.IsDead) {
                Tracker.timeUntilUpdate -= Time.fixedDeltaTime;

                if (Tracker.timeUntilUpdate <= 0f) {
                    bool trackedOnMap = !Tracker.tracked.Data.IsDead;
                    Vector3 position = Tracker.tracked.transform.position;
                    if (!trackedOnMap) { // Check for dead body
                        DeadBody body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == Tracker.tracked.PlayerId);
                        if (body != null) {
                            trackedOnMap = true;
                            position = body.transform.position;
                        }
                    }

                    Tracker.arrow.Update(position);
                    Tracker.arrow.arrow.SetActive(trackedOnMap);
                    Tracker.timeUntilUpdate = Tracker.updateIntervall;
                } else {
                    Tracker.arrow.Update();
                }
            }
        }

        public static void playerSizeUpdate(PlayerControl p) {
            // Set default player size
            CircleCollider2D collider = p.GetComponent<CircleCollider2D>();
            
            p.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            collider.radius = Child.defaultColliderRadius;
            collider.offset = Child.defaultColliderOffset * Vector2.down;

            // Set adapted player size to Child and Morphling
            if (Child.child == null  || Camouflager.camouflageTimer > 0f) return;

            float growingProgress = Child.growingProgress();
            float scale = growingProgress * 0.35f + 0.35f;
            float correctedColliderRadius = Child.defaultColliderRadius * 0.7f / scale; // scale / 0.7f is the factor by which we decrease the player size, hence we need to increase the collider size by 0.7f / scale

            if (p == Child.child) {
                p.transform.localScale = new Vector3(scale, scale, 1f);
                collider.radius = correctedColliderRadius;
            }
            if (Morphling.morphling != null && p == Morphling.morphling && Morphling.morphTarget == Child.child && Morphling.morphTimer > 0f) {
                p.transform.localScale = new Vector3(scale, scale, 1f);
                collider.radius = correctedColliderRadius;
            }
        }

        public static void updateGhostInfo() {
            foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                if (p != PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead) continue;

                Transform playerGhostInfoTransform = p.transform.FindChild("GhostInfo");
                TMPro.TextMeshPro playerGhostInfo = playerGhostInfoTransform != null ? playerGhostInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerGhostInfo == null) {
                    playerGhostInfo = UnityEngine.Object.Instantiate(p.nameText, p.nameText.transform.parent);
                    playerGhostInfo.transform.localPosition += Vector3.up * 0.25f;
                    playerGhostInfo.fontSize *= 0.75f;
                    playerGhostInfo.gameObject.name = "GhostInfo";
                }

                PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == p.PlayerId);
                Transform meetingGhostInfoTransform = playerVoteArea != null ? playerVoteArea.transform.FindChild("GhostInfo") : null;
                TMPro.TextMeshPro meetingGhostInfo = meetingGhostInfoTransform != null ? meetingGhostInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (meetingGhostInfo == null && playerVoteArea != null) {
                    meetingGhostInfo = UnityEngine.Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                    meetingGhostInfo.transform.localPosition += Vector3.down * (MeetingHud.Instance.playerStates.Length > 10 ? 0.4f : 0.25f);
                    meetingGhostInfo.fontSize *= 0.75f;
                    meetingGhostInfo.gameObject.name = "GhostInfo";
                }
                
                var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(p.Data);
                string roleNames = String.Join(", ", RoleInfo.getRoleInfoForPlayer(p).Select(x => Helpers.cs(x.color, x.name)).ToArray());
                string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({tasksCompleted}/{tasksTotal})</color>" : "";
                
                string info = "";
                if (p == PlayerControl.LocalPlayer || (MapOptions.ghostsSeeRoles && MapOptions.ghostsSeeTasks))
                    info = $"{roleNames} {taskInfo}".Trim();
                else if (MapOptions.ghostsSeeTasks)
                    info = $"{taskInfo}".Trim();
                else if (MapOptions.ghostsSeeRoles)
                    info = $"{roleNames}";

                playerGhostInfo.text = info;
                playerGhostInfo.gameObject.SetActive(p.Visible);
                if (meetingGhostInfo != null) meetingGhostInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? "" : info;
            }
        }

        public static void securityGuardSetTarget() {
            if (SecurityGuard.securityGuard == null || SecurityGuard.securityGuard != PlayerControl.LocalPlayer || ShipStatus.Instance == null || ShipStatus.Instance.AllVents == null) return;

            Vent target = null;
            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
            float closestDistance = float.MaxValue;
            for (int i = 0; i < ShipStatus.Instance.AllVents.Length; i++) {
                Vent vent = ShipStatus.Instance.AllVents[i];
                if (vent.gameObject.name.StartsWith("JackInTheBoxVent_") || vent.gameObject.name.StartsWith("SealedVent_")) continue;
                float distance = Vector2.Distance(vent.transform.position, truePosition);
                if (distance <= vent.UsableDistance && distance < closestDistance) {
                    closestDistance = distance;
                    target = vent;
                }
            }
            SecurityGuard.ventTarget = target;
        }

        public static void Postfix(PlayerControl __instance) {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

            // Child and Morphling shrink
            playerSizeUpdate(__instance);
            
            if (PlayerControl.LocalPlayer == __instance) {
                // Update player outlines
                setBasePlayerOutlines();

                // Update Role Description
                Helpers.refreshRoleDescription(__instance);

                // Update Ghost Info
                updateGhostInfo();

                // Time Master
                bendTimeUpdate();
                // Morphling
                morphlingSetTarget();
                // Medic
                medicSetTarget();
                // Shifter
                shifterSetTarget();
                // Sheriff
                sheriffSetTarget();
                // Detective
                detectiveUpdateFootPrints();
                // Tracker
                trackerSetTarget();
                // Vampire
                vampireSetTarget();
                Garlic.UpdateAll();
                // Eraser
                eraserSetTarget();
                // Engineer
                engineerUpdate();
                // Tracker
                trackerUpdate();
                // Jackal
                jackalSetTarget();
                // Sidekick
                sidekickSetTarget();
                // Impostor
                impostorSetTarget();
                // Warlock
                warlockSetTarget();
                // Check for sidekick promotion on Jackal disconnect
                sidekickCheckPromotion();
                // SecurityGuard
                securityGuardSetTarget();
            } 
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.WalkPlayerTo))]
    class PlayerPhysicsWalkPlayerToPatch {
        private static Vector2 offset = Vector2.zero;
        public static void Prefix(PlayerPhysics __instance) {
            bool correctOffset = Camouflager.camouflageTimer <= 0f && (__instance.myPlayer == Child.child ||  (Morphling.morphling != null && __instance.myPlayer == Morphling.morphling && Morphling.morphTarget == Child.child && Morphling.morphTimer > 0f));
            if (correctOffset) {
                float currentScaling = (Child.growingProgress() + 1) * 0.5f;
                __instance.myPlayer.Collider.offset = currentScaling * Child.defaultColliderOffset * Vector2.down;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    class PlayerControlCmdReportDeadBodyPatch {
        public static void Prefix(PlayerControl __instance) {
            // Murder the bitten player before the meeting starts or reset the bitten player
            if (Vampire.bitten != null && !Vampire.bitten.Data.IsDead && Helpers.handleMurderAttempt(Vampire.bitten, true)) {
                MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VampireTryKill, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                RPCProcedure.vampireTryKill();
            } else {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VampireSetBitten, Hazel.SendOption.Reliable, -1);
                writer.Write(byte.MaxValue);
                writer.Write(byte.MaxValue);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.vampireSetBitten(byte.MaxValue, byte.MaxValue);
            }
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcMurderPlayer))]
    class RpcMurderPlayer {
        public static bool Prefix([HarmonyArgument(0)]PlayerControl target) {
            if (Helpers.handleMurderAttempt(target)) { // Custom checks
                if (Child.child != null && PlayerControl.LocalPlayer == Child.child) { // Not checked by official servers
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.uncheckedMurderPlayer(PlayerControl.LocalPlayer.PlayerId, target.PlayerId);
                } else { // Checked by official servers
                    return true;
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.LocalPlayer.CmdReportDeadBody))]
    class BodyReportPatch
    {
        static void Postfix(PlayerControl __instance, [HarmonyArgument(0)]GameData.PlayerInfo target)
        {
            // Medic or Detective report
            bool isMedicReport = Medic.medic != null && Medic.medic == PlayerControl.LocalPlayer && __instance.PlayerId == Medic.medic.PlayerId;
            bool isDetectiveReport = Detective.detective != null && Detective.detective == PlayerControl.LocalPlayer && __instance.PlayerId == Detective.detective.PlayerId;
            if (isMedicReport || isDetectiveReport)
            {
                DeadPlayer deadPlayer = deadPlayers?.Where(x => x.player?.PlayerId == target?.PlayerId)?.FirstOrDefault();

                if (deadPlayer != null && deadPlayer.killerIfExisting != null) {
                    float timeSinceDeath = ((float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds);
                    string msg = "";

                    if (isMedicReport) {
                        msg = $"Body Report: Killed {Math.Round(timeSinceDeath / 1000)}s ago!";
                    } else if (isDetectiveReport) {
                        if (timeSinceDeath < Detective.reportNameDuration * 1000) {
                            msg =  $"Body Report: The killer appears to be {deadPlayer.killerIfExisting.name}!";
                        } else if (timeSinceDeath < Detective.reportColorDuration * 1000) {
                            var typeOfColor = Helpers.isLighterColor(deadPlayer.killerIfExisting.Data.ColorId) ? "lighter" : "darker";
                            msg =  $"Body Report: The killer appears to be a {typeOfColor} color!";
                        } else {
                            msg = $"Body Report: The corpse is too old to gain information from!";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(msg))
                    {   
                        if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
                        {
                            DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg);
                        }
                        if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            DestroyableSingleton<Assets.CoreScripts.Telemetry>.Instance.SendWho();
                        }
                    }
                }
            }  
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static class MurderPlayerPatch
    {
        public static bool resetToCrewmate = false;
        public static bool resetToDead = false;

        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)]PlayerControl target)
        {
            // Allow everyone to murder players
            resetToCrewmate = !__instance.Data.IsImpostor;
            resetToDead = __instance.Data.IsDead;
            __instance.Data.IsImpostor = true;
            __instance.Data.IsDead = false;
        }

        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)]PlayerControl target)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(target, DateTime.UtcNow, DeathReason.Kill, __instance);
            GameHistory.deadPlayers.Add(deadPlayer);

            // Reset killer to crewmate if resetToCrewmate
            if (resetToCrewmate) __instance.Data.IsImpostor = false;
            if (resetToDead) __instance.Data.IsDead = true;

            // Remove fake tasks when player dies
            if (target.hasFakeTasks())
                target.clearAllTasks();

            // Lover suicide trigger on murder
            if ((Lovers.lover1 != null && target == Lovers.lover1) || (Lovers.lover2 != null && target == Lovers.lover2)) {
                PlayerControl otherLover = target == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                if (PlayerControl.LocalPlayer == target && otherLover != null && !otherLover.Data.IsDead && Lovers.bothDie) { // Only the dead lover sends the rpc
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LoverSuicide, Hazel.SendOption.Reliable, -1);
                    writer.Write(otherLover.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.loverSuicide(otherLover.PlayerId);
                }
            }
            
            // Sidekick promotion trigger on murder
            if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead && target == Jackal.jackal && Jackal.jackal == PlayerControl.LocalPlayer) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.sidekickPromotes();
            }

            // Cleaner Button Sync
            if (Cleaner.cleaner != null && PlayerControl.LocalPlayer == Cleaner.cleaner && __instance == Cleaner.cleaner && HudManagerStartPatch.cleanerCleanButton != null) 
                HudManagerStartPatch.cleanerCleanButton.Timer = Cleaner.cleaner.killTimer; 
            
            // Warlock Button Sync
            if (Warlock.warlock != null && PlayerControl.LocalPlayer == Warlock.warlock && __instance == Warlock.warlock && HudManagerStartPatch.warlockCurseButton != null) {
                if(Warlock.warlock.killTimer > HudManagerStartPatch.warlockCurseButton.Timer) {
                    HudManagerStartPatch.warlockCurseButton.Timer = Warlock.warlock.killTimer;
                }
            }

            // Seer show flash and add dead player position
            if (Seer.seer != null && PlayerControl.LocalPlayer == Seer.seer && !Seer.seer.Data.IsDead && Seer.seer != target && Seer.mode <= 1) {
                HudManager.Instance.FullScreen.enabled = true;
                HudManager.Instance.StartCoroutine(Effects.Lerp(1f, new Action<float>((p) => {
                    var renderer = HudManager.Instance.FullScreen;
                    if (p < 0.5) {
                        if (renderer != null)
                            renderer.color = new Color(42f / 255f, 187f / 255f, 245f / 255f, Mathf.Clamp01(p * 2 * 0.75f));
                    } else {
                        if (renderer != null)
                            renderer.color = new Color(42f / 255f, 187f / 255f, 245f / 255f, Mathf.Clamp01((1-p) * 2 * 0.75f));
                    }
                    if (p == 1f && renderer != null) renderer.enabled = false;
                })));
            }
            if (Seer.deadBodyPositions != null) Seer.deadBodyPositions.Add(target.transform.position);

            // Child set adapted kill cooldown
            if (Child.child != null && PlayerControl.LocalPlayer == Child.child && Child.child.Data.IsImpostor && Child.child == __instance) {
                var multiplier = Child.isGrownUp() ? 0.66f : 2f;
                Child.child.SetKillTimer(PlayerControl.GameOptions.KillCooldown * multiplier);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    class PlayerControlSetCoolDownPatch {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)]float time) {
            if (PlayerControl.GameOptions.KillCooldown <= 0f) return false;
            float multiplier = 1f;
            if (Child.child != null && PlayerControl.LocalPlayer == Child.child && Child.child.Data.IsImpostor) multiplier = Child.isGrownUp() ? 0.66f : 2f;

            __instance.killTimer = Mathf.Clamp(time, 0f, PlayerControl.GameOptions.KillCooldown * multiplier);
            DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(__instance.killTimer, PlayerControl.GameOptions.KillCooldown * multiplier);
            return false;
        }
    }

    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.CoPerformKill))]
    class KillAnimationCoPerformKillPatch {
        public static void Prefix(KillAnimation __instance, [HarmonyArgument(0)]ref PlayerControl source, [HarmonyArgument(1)]ref PlayerControl target) {
            if (Vampire.vampire != null && Vampire.vampire == source && Vampire.bitten != null && Vampire.bitten == target)
                source = target;
            
            if (Warlock.warlock != null && Warlock.warlock == source && Warlock.curseKillTarget != null && Warlock.curseKillTarget == target) {
                source = target;
                Warlock.curseKillTarget = null; // Reset here
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class ExilePlayerPatch
    {
        public static void Prefix(PlayerControl __instance) {
            // Child exile lose condition
            if (Child.child != null && Child.child == __instance && !Child.isGrownUp() && !Child.child.Data.IsImpostor) {
                Child.triggerChildLose = true;
            }
            // Jester win condition
            else if (Jester.jester != null && Jester.jester == __instance) {
                Jester.triggerJesterWin = true;
            } 
        }

        public static void Postfix(PlayerControl __instance)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(__instance, DateTime.UtcNow, DeathReason.Exile, null);
            GameHistory.deadPlayers.Add(deadPlayer);

            // Remove fake tasks when player dies
            if (__instance.hasFakeTasks())
                __instance.clearAllTasks();

            // Lover suicide trigger on exile
            if ((Lovers.lover1 != null && __instance == Lovers.lover1) || (Lovers.lover2 != null && __instance == Lovers.lover2)) {
                PlayerControl otherLover = __instance == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                if (otherLover != null && !otherLover.Data.IsDead && Lovers.bothDie)
                    otherLover.Exiled();
            }
            
            // Sidekick promotion trigger on exile
            if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead && __instance == Jackal.jackal && Jackal.jackal == PlayerControl.LocalPlayer) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.sidekickPromotes();
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanMove), MethodType.Getter)]
    class PlayerControlCanMovePatch {
        public static bool Prefix(PlayerControl __instance, ref bool __result)
        {
            __result = __instance.moveable &&
                !Minigame.Instance &&
                (!DestroyableSingleton<HudManager>.InstanceExists || (!DestroyableSingleton<HudManager>.Instance.Chat.IsOpen && !DestroyableSingleton<HudManager>.Instance.KillOverlay.IsOpen && !DestroyableSingleton<HudManager>.Instance.GameMenu.IsOpen)) &&
                (!MapBehaviour.Instance || !MapBehaviour.Instance.IsOpenStopped) &&
                !MeetingHud.Instance &&
                !CustomPlayerMenu.Instance &&
                !ExileController.Instance &&
                !IntroCutscene.Instance;
            return false;
        }
    }
}
