using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.GameHistory;
using TheOtherRoles.Objects;
using UnityEngine;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class PlayerControlFixedUpdatePatch {
        // Helpers

        static PlayerControl setTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null) {
            PlayerControl result = null;
            float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            if (!ShipStatus.Instance) return result;
            if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
            if (targetingPlayer.Data.IsDead) return result;

            Vector2 truePosition = targetingPlayer.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++) {
                GameData.PlayerInfo playerInfo = allPlayers[i];
                if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead && (!onlyCrewmates || !playerInfo.Role.IsImpostor)) {
                    PlayerControl @object = playerInfo.Object;
                    if (untargetablePlayers != null && untargetablePlayers.Any(x => x == @object)) {
                        // if that player is not targetable: skip check
                        continue;
                    }

                    if (@object && (!@object.inVent || targetPlayersInVents)) {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask)) {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }

        static void setPlayerOutline(PlayerControl target, Color color) {
            if (target == null || target.MyRend == null) return;

            target.MyRend.material.SetFloat("_Outline", 1f);
            target.MyRend.material.SetColor("_OutlineColor", color);
        }

        // Update functions

        static void setBasePlayerOutlines() {
            foreach (PlayerControl target in PlayerControl.AllPlayerControls) {
                if (target == null || target.MyRend == null) continue;

                bool isMorphedMorphling = target == Morphling.morphling && Morphling.morphTarget != null && Morphling.morphTimer > 0f;
                bool hasVisibleShield = false;
                Color color = Medic.shieldedColor;
                if (Camouflager.camouflageTimer <= 0f && Medic.shielded != null && ((target == Medic.shielded && !isMorphedMorphling) || (isMorphedMorphling && Morphling.morphTarget == Medic.shielded))) {
                    hasVisibleShield = Medic.showShielded == 0 // Everyone
                        || (Medic.showShielded == 1 && (PlayerControl.LocalPlayer == Medic.shielded || PlayerControl.LocalPlayer == Medic.medic)) // Shielded + Medic
                        || (Medic.showShielded == 2 && PlayerControl.LocalPlayer == Medic.medic); // Medic only
                    // Make shield invisible till after the next meeting if the option is set (the medic can already see the shield)
                    hasVisibleShield = hasVisibleShield && (Medic.meetingAfterShielding || !Medic.showShieldAfterMeeting || PlayerControl.LocalPlayer == Medic.medic);
                }

                if (Camouflager.camouflageTimer <= 0f && MapOptions.firstKillPlayer != null && MapOptions.shieldFirstKill && ((target == MapOptions.firstKillPlayer && !isMorphedMorphling) || (isMorphedMorphling && Morphling.morphTarget == MapOptions.firstKillPlayer))) {
                    hasVisibleShield = true;
                    color = Color.blue;
                }

                if (hasVisibleShield) {
                target.MyRend.material.SetFloat("_Outline", 1f);
                target.MyRend.material.SetColor("_OutlineColor", color);
                }
                else {
                    target.MyRend.material.SetFloat("_Outline", 0f);
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
                    }
                    else if (localPlayerPositions.Any(x => x.Item2 == true)) {
                        PlayerControl.LocalPlayer.transform.position = next.Item1;
                    }

                    localPlayerPositions.RemoveAt(0);

                    if (localPlayerPositions.Count > 1) localPlayerPositions.RemoveAt(0); // Skip every second position to rewinde twice as fast, but never skip the last position
                }
                else {
                    TimeMaster.isRewinding = false;
                    PlayerControl.LocalPlayer.moveable = true;
                }
            }
            else {
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

        static void deputySetTarget()
        {
            if (Deputy.deputy == null || Deputy.deputy != PlayerControl.LocalPlayer) return;
            Deputy.currentTarget = setTarget();
            setPlayerOutline(Deputy.currentTarget, Deputy.color);
        }

        public static void deputyCheckPromotion(bool isMeeting=false)
        {
            // If LocalPlayer is Deputy, the Sheriff is disconnected and Deputy promotion is enabled, then trigger promotion
            if (Deputy.deputy == null || Deputy.deputy != PlayerControl.LocalPlayer) return;
            if (Deputy.promotesToSheriff == 0 || Deputy.deputy.Data.IsDead == true || Deputy.promotesToSheriff == 2 && !isMeeting) return;
            if (Sheriff.sheriff == null || Sheriff.sheriff?.Data?.Disconnected == true || Sheriff.sheriff.Data.IsDead)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DeputyPromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.deputyPromotes();
            }
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
            if (Spy.spy != null || Sidekick.wasSpy || Jackal.wasSpy) {
                if (Spy.impostorsCanKillAnyone) {
                    target = setTarget(false, true);
                }
                else {
                    target = setTarget(true, true, new List<PlayerControl>() { Spy.spy, Sidekick.wasTeamRed ? Sidekick.sidekick : null, Jackal.wasTeamRed ? Jackal.jackal : null });
                }
            }
            else {
                target = setTarget(true, true, new List<PlayerControl>() { Sidekick.wasImpostor ? Sidekick.sidekick : null, Jackal.wasImpostor ? Jackal.jackal : null });
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
            if (Jackal.canCreateSidekickFromImpostor) {
                // Only exclude sidekick from beeing targeted if the jackal can create sidekicks from impostors
                if (Sidekick.sidekick != null) untargetablePlayers.Add(Sidekick.sidekick);
            }
            if (Mini.mini != null && !Mini.isGrownUp()) untargetablePlayers.Add(Mini.mini); // Exclude Jackal from targeting the Mini unless it has grown up
            Jackal.currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
            setPlayerOutline(Jackal.currentTarget, Palette.ImpostorRed);
        }

        static void sidekickSetTarget() {
            if (Sidekick.sidekick == null || Sidekick.sidekick != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if (Jackal.jackal != null) untargetablePlayers.Add(Jackal.jackal);
            if (Mini.mini != null && !Mini.isGrownUp()) untargetablePlayers.Add(Mini.mini); // Exclude Sidekick from targeting the Mini unless it has grown up
            Sidekick.currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
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

            List<PlayerControl> untargetables = new List<PlayerControl>();
            if (Spy.spy != null) untargetables.Add(Spy.spy);
            if (Sidekick.wasTeamRed) untargetables.Add(Sidekick.sidekick);
            if (Jackal.wasTeamRed) untargetables.Add(Jackal.jackal);
            Eraser.currentTarget = setTarget(onlyCrewmates: !Eraser.canEraseAnyone, untargetablePlayers: Eraser.canEraseAnyone ? new List<PlayerControl>() : untargetables);
            setPlayerOutline(Eraser.currentTarget, Eraser.color);
        }

        static void deputyUpdate()
        {
            if (PlayerControl.LocalPlayer == null || !Deputy.handcuffedKnows.ContainsKey(PlayerControl.LocalPlayer.PlayerId)) return;
            
            if (Deputy.handcuffedKnows[PlayerControl.LocalPlayer.PlayerId] <= 0)
            {
                Deputy.handcuffedKnows.Remove(PlayerControl.LocalPlayer.PlayerId);
                // Resets the buttons
                Deputy.setHandcuffedKnows(false);
            }
            
        }

        static void engineerUpdate() {
            bool jackalHighlight = Engineer.highlightForTeamJackal && (PlayerControl.LocalPlayer == Jackal.jackal || PlayerControl.LocalPlayer == Sidekick.sidekick);
            bool impostorHighlight = Engineer.highlightForImpostors && PlayerControl.LocalPlayer.Data.Role.IsImpostor;
            if ((jackalHighlight || impostorHighlight) && ShipStatus.Instance?.AllVents != null) {
                foreach (Vent vent in ShipStatus.Instance.AllVents) {
                    try {
                        if (vent?.myRend?.material != null) {
                            if (Engineer.engineer != null && Engineer.engineer.inVent) {
                                vent.myRend.material.SetFloat("_Outline", 1f);
                                vent.myRend.material.SetColor("_OutlineColor", Engineer.color);
                            }
                            else if (vent.myRend.material.GetColor("_AddColor") != Color.red) {
                                vent.myRend.material.SetFloat("_Outline", 0);
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        static void impostorSetTarget() {
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor ||!PlayerControl.LocalPlayer.CanMove || PlayerControl.LocalPlayer.Data.IsDead) { // !isImpostor || !canMove || isDead
                HudManager.Instance.KillButton.SetTarget(null);
                return;
            }

            PlayerControl target = null;
            if (Spy.spy != null || Sidekick.wasSpy || Jackal.wasSpy) {
                if (Spy.impostorsCanKillAnyone) {
                    target = setTarget(false, true);
                }
                else {
                    target = setTarget(true, true, new List<PlayerControl>() { Spy.spy, Sidekick.wasTeamRed ? Sidekick.sidekick : null, Jackal.wasTeamRed ? Jackal.jackal : null});
                }
            }
            else {
                target = setTarget(true, true, new List<PlayerControl>() { Sidekick.wasImpostor ? Sidekick.sidekick : null, Jackal.wasImpostor ? Jackal.jackal : null});
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
            }
            else {
                Warlock.curseVictimTarget = setTarget(targetingPlayer: Warlock.curseVictim);
                setPlayerOutline(Warlock.curseVictimTarget, Warlock.color);
            }
        }

        static void ninjaUpdate()
        {
            if (Ninja.arrow?.arrow != null)
            {
                if (Ninja.ninja == null || Ninja.ninja != PlayerControl.LocalPlayer || !Ninja.knowsTargetLocation) {
                    Ninja.arrow.arrow.SetActive(false);
                    return;
                }
                if (Ninja.ninjaMarked != null && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    bool trackedOnMap = !Ninja.ninjaMarked.Data.IsDead;
                    Vector3 position = Ninja.ninjaMarked.transform.position;
                    if (!trackedOnMap)
                    { // Check for dead body
                        DeadBody body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == Ninja.ninjaMarked.PlayerId);
                        if (body != null)
                        {
                            trackedOnMap = true;
                            position = body.transform.position;
                        }
                    }
                    Ninja.arrow.Update(position);
                    Ninja.arrow.arrow.SetActive(trackedOnMap);
                } else
                {
                    Ninja.arrow.arrow.SetActive(false);
                }
            }
        }

        static void trackerUpdate() {
            // Handle player tracking
            if (Tracker.arrow?.arrow != null) {
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

            // Handle corpses tracking
            if (Tracker.tracker != null && Tracker.tracker == PlayerControl.LocalPlayer && Tracker.corpsesTrackingTimer >= 0f && !Tracker.tracker.Data.IsDead) {
                bool arrowsCountChanged = Tracker.localArrows.Count != Tracker.deadBodyPositions.Count();
                int index = 0;

                if (arrowsCountChanged) {
                    foreach (Arrow arrow in Tracker.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                    Tracker.localArrows = new List<Arrow>();
                }
                foreach (Vector3 position in Tracker.deadBodyPositions) {
                    if (arrowsCountChanged) {
                        Tracker.localArrows.Add(new Arrow(Tracker.color));
                        Tracker.localArrows[index].arrow.SetActive(true);
                    }
                    if (Tracker.localArrows[index] != null) Tracker.localArrows[index].Update(position);
                    index++;
                }
            } else if (Tracker.localArrows.Count > 0) { 
                foreach (Arrow arrow in Tracker.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                Tracker.localArrows = new List<Arrow>();
            }
        }

        public static void playerSizeUpdate(PlayerControl p) {
            // Set default player size
            CircleCollider2D collider = p.GetComponent<CircleCollider2D>();

            p.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            collider.radius = Mini.defaultColliderRadius;
            collider.offset = Mini.defaultColliderOffset * Vector2.down;

            // Set adapted player size to Mini and Morphling
            if (Mini.mini == null || Camouflager.camouflageTimer > 0f || Mini.mini == Morphling.morphling && Morphling.morphTimer > 0) return;

            float growingProgress = Mini.growingProgress();
            float scale = growingProgress * 0.35f + 0.35f;
            float correctedColliderRadius = Mini.defaultColliderRadius * 0.7f / scale; // scale / 0.7f is the factor by which we decrease the player size, hence we need to increase the collider size by 0.7f / scale

            if (p == Mini.mini) {
                p.transform.localScale = new Vector3(scale, scale, 1f);
                collider.radius = correctedColliderRadius;
            }
            if (Morphling.morphling != null && p == Morphling.morphling && Morphling.morphTarget == Mini.mini && Morphling.morphTimer > 0f) {
                p.transform.localScale = new Vector3(scale, scale, 1f);
                collider.radius = correctedColliderRadius;
            }
        }

        public static void updatePlayerInfo() {
            foreach (PlayerControl p in PlayerControl.AllPlayerControls) {         
                if ((Lawyer.lawyerKnowsRole && PlayerControl.LocalPlayer == Lawyer.lawyer && p == Lawyer.target) || p == PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data.IsDead) {
                    Transform playerInfoTransform = p.nameText.transform.parent.FindChild("Info");
                    TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (playerInfo == null) {
                        playerInfo = UnityEngine.Object.Instantiate(p.nameText, p.nameText.transform.parent);
                        playerInfo.transform.localPosition += Vector3.up * 0.5f;
                        playerInfo.fontSize *= 0.75f;
                        playerInfo.gameObject.name = "Info";
                    }

                    PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == p.PlayerId);
                    Transform meetingInfoTransform = playerVoteArea != null ? playerVoteArea.NameText.transform.parent.FindChild("Info") : null;
                    TMPro.TextMeshPro meetingInfo = meetingInfoTransform != null ? meetingInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (meetingInfo == null && playerVoteArea != null) {
                        meetingInfo = UnityEngine.Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                        meetingInfo.transform.localPosition += Vector3.down * 0.10f;
                        meetingInfo.fontSize *= 0.60f;
                        meetingInfo.gameObject.name = "Info";
                    }

                    // Set player name higher to align in middle
                    if (meetingInfo != null && playerVoteArea != null) {
                        var playerName = playerVoteArea.NameText;
                        playerName.transform.localPosition = new Vector3(0.3384f, (0.0311f + 0.0683f), -0.1f);    
                    }

                    var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(p.Data);
                    string roleNames = RoleInfo.GetRolesString(p, true, false);
                    string roleText = RoleInfo.GetRolesString(p, true, MapOptions.ghostsSeeModifier);
                    string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({tasksCompleted}/{tasksTotal})</color>" : "";

                    string playerInfoText = "";
                    string meetingInfoText = "";                    
                    if (p == PlayerControl.LocalPlayer) {
                        if (p.Data.IsDead) roleNames = roleText;
                        playerInfoText = $"{roleNames}";
                        if (p == Swapper.swapper) playerInfoText = $"{roleNames}" + Helpers.cs(Swapper.color, $" ({Swapper.charges})");
                        if (DestroyableSingleton<TaskPanelBehaviour>.InstanceExists) {
                            TMPro.TextMeshPro tabText = DestroyableSingleton<TaskPanelBehaviour>.Instance.tab.transform.FindChild("TabText_TMP").GetComponent<TMPro.TextMeshPro>();
                            tabText.SetText($"Tasks {taskInfo}");
                        }
                        meetingInfoText = $"{roleNames} {taskInfo}".Trim();
                    }
                    else if (MapOptions.ghostsSeeRoles && MapOptions.ghostsSeeTasks) {
                        playerInfoText = $"{roleText} {taskInfo}".Trim();
                        meetingInfoText = playerInfoText;
                    }
                    else if (MapOptions.ghostsSeeTasks) {
                        playerInfoText = $"{taskInfo}".Trim();
                        meetingInfoText = playerInfoText;
                    }
                    else if (MapOptions.ghostsSeeRoles || (Lawyer.lawyerKnowsRole && PlayerControl.LocalPlayer == Lawyer.lawyer && p == Lawyer.target)) {
                        playerInfoText = $"{roleText}";
                        meetingInfoText = playerInfoText;
                    }

                    playerInfo.text = playerInfoText;
                    playerInfo.gameObject.SetActive(p.Visible);
                    if (meetingInfo != null) meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? "" : meetingInfoText;
                }                
            }
        }

        public static void securityGuardSetTarget() {
            if (SecurityGuard.securityGuard == null || SecurityGuard.securityGuard != PlayerControl.LocalPlayer || ShipStatus.Instance == null || ShipStatus.Instance.AllVents == null) return;

            Vent target = null;
            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
            float closestDistance = float.MaxValue;
            for (int i = 0; i < ShipStatus.Instance.AllVents.Length; i++) {
                Vent vent = ShipStatus.Instance.AllVents[i];
                if (vent.gameObject.name.StartsWith("JackInTheBoxVent_") || vent.gameObject.name.StartsWith("SealedVent_") || vent.gameObject.name.StartsWith("FutureSealedVent_")) continue;
                float distance = Vector2.Distance(vent.transform.position, truePosition);
                if (distance <= vent.UsableDistance && distance < closestDistance) {
                    closestDistance = distance;
                    target = vent;
                }
            }
            SecurityGuard.ventTarget = target;
        }

        public static void securityGuardUpdate() {
            if (SecurityGuard.securityGuard == null || PlayerControl.LocalPlayer != SecurityGuard.securityGuard || SecurityGuard.securityGuard.Data.IsDead) return;
            var (playerCompleted, _) = TasksHandler.taskInfo(SecurityGuard.securityGuard.Data);
            if (playerCompleted == SecurityGuard.rechargedTasks) {
                SecurityGuard.rechargedTasks += SecurityGuard.rechargeTasksNumber;
                if (SecurityGuard.maxCharges > SecurityGuard.charges) SecurityGuard.charges++;
            }
        }

        public static void arsonistSetTarget() {
            if (Arsonist.arsonist == null || Arsonist.arsonist != PlayerControl.LocalPlayer) return;
            List<PlayerControl> untargetables;
            if (Arsonist.douseTarget != null)
                untargetables = PlayerControl.AllPlayerControls.ToArray().Where(x => x.PlayerId != Arsonist.douseTarget.PlayerId).ToList();
            else
                untargetables = Arsonist.dousedPlayers;
            Arsonist.currentTarget = setTarget(untargetablePlayers: untargetables);
            if (Arsonist.currentTarget != null) setPlayerOutline(Arsonist.currentTarget, Arsonist.color);
        }

        static void snitchUpdate() {
            if (Snitch.localArrows == null) return;

            foreach (Arrow arrow in Snitch.localArrows) arrow.arrow.SetActive(false);

            if (Snitch.snitch == null || Snitch.snitch.Data.IsDead) return;

            var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.snitch.Data);
            int numberOfTasks = playerTotal - playerCompleted;

            if (numberOfTasks <= Snitch.taskCountForReveal && (PlayerControl.LocalPlayer.Data.Role.IsImpostor || (Snitch.includeTeamJackal && (PlayerControl.LocalPlayer == Jackal.jackal || PlayerControl.LocalPlayer == Sidekick.sidekick)))) {
                if (Snitch.localArrows.Count == 0) Snitch.localArrows.Add(new Arrow(Color.blue));
                if (Snitch.localArrows.Count != 0 && Snitch.localArrows[0] != null) {
                    Snitch.localArrows[0].arrow.SetActive(true);
                    Snitch.localArrows[0].Update(Snitch.snitch.transform.position);
                }
            }
            else if (PlayerControl.LocalPlayer == Snitch.snitch && numberOfTasks == 0) {
                int arrowIndex = 0;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    bool arrowForImp = p.Data.Role.IsImpostor;
                    bool arrowForTeamJackal = Snitch.includeTeamJackal && (p == Jackal.jackal || p == Sidekick.sidekick);

                    if (!p.Data.IsDead && (arrowForImp || arrowForTeamJackal)) {
                        if (arrowIndex >= Snitch.localArrows.Count) {
                            Snitch.localArrows.Add(new Arrow(Palette.ImpostorRed));
                        }
                        if (arrowIndex < Snitch.localArrows.Count && Snitch.localArrows[arrowIndex] != null) {
                            Snitch.localArrows[arrowIndex].arrow.SetActive(true);
                            Snitch.localArrows[arrowIndex].Update(p.transform.position, (arrowForTeamJackal && Snitch.teamJackalUseDifferentArrowColor ? Jackal.color : Palette.ImpostorRed));
                        }
                        arrowIndex++;
                    }
                }
            }
        }

        static void bountyHunterUpdate() {
            if (BountyHunter.bountyHunter == null || PlayerControl.LocalPlayer != BountyHunter.bountyHunter) return;

            if (BountyHunter.bountyHunter.Data.IsDead) {
                if (BountyHunter.arrow != null || BountyHunter.arrow.arrow != null) UnityEngine.Object.Destroy(BountyHunter.arrow.arrow);
                BountyHunter.arrow = null;
                if (BountyHunter.cooldownText != null && BountyHunter.cooldownText.gameObject != null) UnityEngine.Object.Destroy(BountyHunter.cooldownText.gameObject);
                BountyHunter.cooldownText = null;
                BountyHunter.bounty = null;
                foreach (PoolablePlayer p in MapOptions.playerIcons.Values) {
                    if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
                }
                return;
            }

            BountyHunter.arrowUpdateTimer -= Time.fixedDeltaTime;
            BountyHunter.bountyUpdateTimer -= Time.fixedDeltaTime;

            if (BountyHunter.bounty == null || BountyHunter.bountyUpdateTimer <= 0f) {
                // Set new bounty
                BountyHunter.bounty = null;
                BountyHunter.arrowUpdateTimer = 0f; // Force arrow to update
                BountyHunter.bountyUpdateTimer = BountyHunter.bountyDuration;
                var possibleTargets = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                    if (!p.Data.IsDead && !p.Data.Disconnected && p != p.Data.Role.IsImpostor && p != Spy.spy && (p != Sidekick.sidekick || !Sidekick.wasTeamRed) && (p != Jackal.jackal || !Jackal.wasTeamRed) && (p != Mini.mini || Mini.isGrownUp()) && (Lovers.getPartner(BountyHunter.bountyHunter) == null || p != Lovers.getPartner(BountyHunter.bountyHunter))) possibleTargets.Add(p);
                }
                BountyHunter.bounty = possibleTargets[TheOtherRoles.rnd.Next(0, possibleTargets.Count)];
                if (BountyHunter.bounty == null) return;

                // Show poolable player
                if (HudManager.Instance != null && HudManager.Instance.UseButton != null) {
                    foreach (PoolablePlayer pp in MapOptions.playerIcons.Values) pp.gameObject.SetActive(false);
                    if (MapOptions.playerIcons.ContainsKey(BountyHunter.bounty.PlayerId) && MapOptions.playerIcons[BountyHunter.bounty.PlayerId].gameObject != null)
                        MapOptions.playerIcons[BountyHunter.bounty.PlayerId].gameObject.SetActive(true);
                }
            }

            // Update Cooldown Text
            if (BountyHunter.cooldownText != null) {
                BountyHunter.cooldownText.text = Mathf.CeilToInt(Mathf.Clamp(BountyHunter.bountyUpdateTimer, 0, BountyHunter.bountyDuration)).ToString();
            }

            // Update Arrow
            if (BountyHunter.showArrow && BountyHunter.bounty != null) {
                if (BountyHunter.arrow == null) BountyHunter.arrow = new Arrow(Color.red);
                if (BountyHunter.arrowUpdateTimer <= 0f) {
                    BountyHunter.arrow.Update(BountyHunter.bounty.transform.position);
                    BountyHunter.arrowUpdateTimer = BountyHunter.arrowUpdateIntervall;
                }
                BountyHunter.arrow.Update();
            }
        }

        static void vultureUpdate() {
            if (Vulture.vulture == null || PlayerControl.LocalPlayer != Vulture.vulture || Vulture.localArrows == null || !Vulture.showArrows) return;
            if (Vulture.vulture.Data.IsDead) {
                foreach (Arrow arrow in Vulture.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                Vulture.localArrows = new List<Arrow>();
                return;
            }

            DeadBody[] deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            bool arrowUpdate = Vulture.localArrows.Count != deadBodies.Count();
            int index = 0;

            if (arrowUpdate) {
                foreach (Arrow arrow in Vulture.localArrows) UnityEngine.Object.Destroy(arrow.arrow);
                Vulture.localArrows = new List<Arrow>();
            }

            foreach (DeadBody db in deadBodies) {
                if (arrowUpdate) {
                    Vulture.localArrows.Add(new Arrow(Color.blue));
                    Vulture.localArrows[index].arrow.SetActive(true);
                }
                if (Vulture.localArrows[index] != null) Vulture.localArrows[index].Update(db.transform.position);
                index++;
            }
        }

        public static void mediumSetTarget() {
            if (Medium.medium == null || Medium.medium != PlayerControl.LocalPlayer || Medium.medium.Data.IsDead || Medium.deadBodies == null || ShipStatus.Instance?.AllVents == null) return;

            DeadPlayer target = null;
            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
            float closestDistance = float.MaxValue;
            float usableDistance = ShipStatus.Instance.AllVents.FirstOrDefault().UsableDistance;
            foreach ((DeadPlayer dp, Vector3 ps) in Medium.deadBodies) {
                float distance = Vector2.Distance(ps, truePosition);
                if (distance <= usableDistance && distance < closestDistance) {
                    closestDistance = distance;
                    target = dp;
                }
            }
            Medium.target = target;
        }

        static void morphlingAndCamouflagerUpdate() {
            float oldCamouflageTimer = Camouflager.camouflageTimer;
            float oldMorphTimer = Morphling.morphTimer;
            Camouflager.camouflageTimer = Mathf.Max(0f, Camouflager.camouflageTimer - Time.fixedDeltaTime);
            Morphling.morphTimer = Mathf.Max(0f, Morphling.morphTimer - Time.fixedDeltaTime);


            // Camouflage reset and set Morphling look if necessary
            if (oldCamouflageTimer > 0f && Camouflager.camouflageTimer <= 0f) {
                Camouflager.resetCamouflage();
                if (Morphling.morphTimer > 0f && Morphling.morphling != null && Morphling.morphTarget != null) {
                    PlayerControl target = Morphling.morphTarget;
                    Morphling.morphling.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                }
            }

            // Morphling reset (only if camouflage is inactive)
            if (Camouflager.camouflageTimer <= 0f && oldMorphTimer > 0f && Morphling.morphTimer <= 0f && Morphling.morphling != null)
                Morphling.resetMorph();
        }

        public static void lawyerUpdate() {
            if (Lawyer.lawyer == null || Lawyer.lawyer != PlayerControl.LocalPlayer) return;

            // Meeting win
            if (Lawyer.winsAfterMeetings && Lawyer.neededMeetings == Lawyer.meetings && Lawyer.target != null && !Lawyer.target.Data.IsDead) {
                Lawyer.winsAfterMeetings = false; // Avoid sending mutliple RPCs until the host finshes the game
                MessageWriter winWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerWin, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(winWriter);
                RPCProcedure.lawyerWin();
                return;
            }

            // Promote to Pursuer
            if (Lawyer.target != null && Lawyer.target.Data.Disconnected) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.lawyerPromotesToPursuer();
                return;
            }
        }

        public static void hackerUpdate() {
            if (Hacker.hacker == null || PlayerControl.LocalPlayer != Hacker.hacker || Hacker.hacker.Data.IsDead) return;
            var (playerCompleted, _) = TasksHandler.taskInfo(Hacker.hacker.Data);
            if (playerCompleted == Hacker.rechargedTasks) {
                Hacker.rechargedTasks += Hacker.rechargeTasksNumber;
                if (Hacker.toolsNumber > Hacker.chargesVitals) Hacker.chargesVitals++;
                if (Hacker.toolsNumber > Hacker.chargesAdminTable) Hacker.chargesAdminTable++;
            }
        }

        // For swapper swap charges        
        public static void swapperUpdate() {
            if (Swapper.swapper == null || PlayerControl.LocalPlayer != Swapper.swapper || PlayerControl.LocalPlayer.Data.IsDead) return;
            var (playerCompleted, _) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
            if (playerCompleted == Swapper.rechargedTasks) {
                Swapper.rechargedTasks += Swapper.rechargeTasksNumber;
                Swapper.charges++;
            }
        }

        static void pursuerSetTarget() {
            if (Pursuer.pursuer == null || Pursuer.pursuer != PlayerControl.LocalPlayer) return;
            Pursuer.target = setTarget();
            setPlayerOutline(Pursuer.target, Pursuer.color);
        }

        static void witchSetTarget() {
            if (Witch.witch == null || Witch.witch != PlayerControl.LocalPlayer) return;
            List<PlayerControl> untargetables;
            if (Witch.spellCastingTarget != null)
                untargetables = PlayerControl.AllPlayerControls.ToArray().Where(x => x.PlayerId != Witch.spellCastingTarget.PlayerId).ToList(); // Don't switch the target from the the one you're currently casting a spell on
            else {
                untargetables = new List<PlayerControl>(); // Also target players that have already been spelled, to hide spells that were blanks/blocked by shields
                if (Spy.spy != null && !Witch.canSpellAnyone) untargetables.Add(Spy.spy);
                if (Sidekick.wasTeamRed && !Witch.canSpellAnyone) untargetables.Add(Sidekick.sidekick);
                if (Jackal.wasTeamRed && !Witch.canSpellAnyone) untargetables.Add(Jackal.jackal);
            }
            Witch.currentTarget = setTarget(onlyCrewmates: !Witch.canSpellAnyone, untargetablePlayers: untargetables);
            setPlayerOutline(Witch.currentTarget, Witch.color);
        }

        static void ninjaSetTarget()
        {
            if (Ninja.ninja == null || Ninja.ninja != PlayerControl.LocalPlayer) return;
            List<PlayerControl> untargetables = new List<PlayerControl>();
            if (Spy.spy != null && !Spy.impostorsCanKillAnyone) untargetables.Add(Spy.spy);
            if (Mini.mini != null) untargetables.Add(Mini.mini);
            if (Sidekick.wasTeamRed && !Spy.impostorsCanKillAnyone) untargetables.Add(Sidekick.sidekick);
            if (Jackal.wasTeamRed && !Spy.impostorsCanKillAnyone) untargetables.Add(Jackal.jackal);
            Ninja.currentTarget = setTarget(onlyCrewmates: true, untargetablePlayers: untargetables);
            setPlayerOutline(Ninja.currentTarget, Ninja.color);
        }

        static void baitUpdate() {
            if (!Bait.active.Any()) return;

            // Bait report
            foreach (KeyValuePair<DeadPlayer, float> entry in new Dictionary<DeadPlayer, float>(Bait.active)) {
                Bait.active[entry.Key] = entry.Value - Time.fixedDeltaTime;
                if (entry.Value <= 0) {
                    Bait.active.Remove(entry.Key);
                    if (entry.Key.killerIfExisting != null) {
                        Helpers.handleVampireBiteOnBodyReport(); // Manually call Vampire handling, since the CmdReportDeadBody Prefix won't be called
                        RPCProcedure.uncheckedCmdReportDeadBody(entry.Key.killerIfExisting.PlayerId, entry.Key.player.PlayerId);

                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedCmdReportDeadBody, Hazel.SendOption.Reliable, -1);
                        writer.Write(entry.Key.killerIfExisting.PlayerId);
                        writer.Write(entry.Key.player.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                    }
                }
            }
        }

        static void bloodyUpdate() {
            if (!Bloody.active.Any()) return;
            foreach (KeyValuePair<byte, float> entry in new Dictionary<byte, float>(Bloody.active)) {
                PlayerControl player = Helpers.playerById(entry.Key);
                PlayerControl bloodyPlayer = Helpers.playerById(Bloody.bloodyKillerMap[player.PlayerId]);                
                new Bloodytrail(player, bloodyPlayer);
                Bloody.active[entry.Key] = entry.Value - Time.fixedDeltaTime;
                if (entry.Value <= 0) Bloody.active.Remove(entry.Key);
            }
        }

        // Mini set adapted button cooldown for Vampire, Sheriff, Jackal, Sidekick, Warlock, Cleaner
        public static void miniCooldownUpdate() {
            if (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini) {
                var multiplier = Mini.isGrownUp() ? 0.66f : 2f;
                HudManagerStartPatch.sheriffKillButton.MaxTimer = Sheriff.cooldown * multiplier;
                HudManagerStartPatch.vampireKillButton.MaxTimer = Vampire.cooldown * multiplier;
                HudManagerStartPatch.jackalKillButton.MaxTimer = Jackal.cooldown * multiplier;
                HudManagerStartPatch.sidekickKillButton.MaxTimer = Sidekick.cooldown * multiplier;
                HudManagerStartPatch.warlockCurseButton.MaxTimer = Warlock.cooldown * multiplier;
                HudManagerStartPatch.cleanerCleanButton.MaxTimer = Cleaner.cooldown * multiplier;
                HudManagerStartPatch.witchSpellButton.MaxTimer = (Witch.cooldown + Witch.currentCooldownAddition) * multiplier;
            }
        }

    public static void Postfix(PlayerControl __instance) {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

            // Mini and Morphling shrink
            playerSizeUpdate(__instance);
            
            if (PlayerControl.LocalPlayer == __instance) {
                // Update player outlines
                setBasePlayerOutlines();

                // Update Role Description
                Helpers.refreshRoleDescription(__instance);

                // Update Player Info
                updatePlayerInfo();

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
                // Deputy
                deputySetTarget();
                deputyUpdate();
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
                // Check for deputy promotion on Sheriff disconnect
                deputyCheckPromotion();
                // Check for sidekick promotion on Jackal disconnect
                sidekickCheckPromotion();
                // SecurityGuard
                securityGuardSetTarget();
                securityGuardUpdate();
                // Arsonist
                arsonistSetTarget();
                // Snitch
                snitchUpdate();
                // BountyHunter
                bountyHunterUpdate();
                // Vulture
                vultureUpdate();
                // Medium
                mediumSetTarget();
                // Morphling and Camouflager
                morphlingAndCamouflagerUpdate();
                // Lawyer
                lawyerUpdate();
                // Pursuer
                pursuerSetTarget();
                // Witch
                witchSetTarget();
                // Ninja
                ninjaSetTarget();
                NinjaTrace.UpdateAll();
                ninjaUpdate();

                hackerUpdate();
                swapperUpdate();

                // Hacker
                hackerUpdate();

                // --MODIFIER--
                // Bait
                baitUpdate();
                // Bloody
                bloodyUpdate();
                // mini (for the cooldowns)
                miniCooldownUpdate();
            } 
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.WalkPlayerTo))]
    class PlayerPhysicsWalkPlayerToPatch {
        private static Vector2 offset = Vector2.zero;
        public static void Prefix(PlayerPhysics __instance) {
            bool correctOffset = Camouflager.camouflageTimer <= 0f && (__instance.myPlayer == Mini.mini ||  (Morphling.morphling != null && __instance.myPlayer == Morphling.morphling && Morphling.morphTarget == Mini.mini && Morphling.morphTimer > 0f));
            correctOffset = correctOffset && !(Mini.mini == Morphling.morphling && Morphling.morphTimer > 0f);
            if (correctOffset) {
                float currentScaling = (Mini.growingProgress() + 1) * 0.5f;
                __instance.myPlayer.Collider.offset = currentScaling * Mini.defaultColliderOffset * Vector2.down;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    class PlayerControlCmdReportDeadBodyPatch {
        public static void Prefix(PlayerControl __instance) {
            Helpers.handleVampireBiteOnBodyReport();
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
                            msg =  $"Body Report: The killer appears to be {deadPlayer.killerIfExisting.Data.PlayerName}!";
                        } else if (timeSinceDeath < Detective.reportColorDuration * 1000) {
                            var typeOfColor = Helpers.isLighterColor(deadPlayer.killerIfExisting.Data.DefaultOutfit.ColorId) ? "lighter" : "darker";
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
            resetToCrewmate = !__instance.Data.Role.IsImpostor;
            resetToDead = __instance.Data.IsDead;
            __instance.Data.Role.TeamType = RoleTeamTypes.Impostor;
            __instance.Data.IsDead = false;
        }

        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)]PlayerControl target)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(target, DateTime.UtcNow, DeathReason.Kill, __instance);
            GameHistory.deadPlayers.Add(deadPlayer);

            // Reset killer to crewmate if resetToCrewmate
            if (resetToCrewmate) __instance.Data.Role.TeamType = RoleTeamTypes.Crewmate;
            if (resetToDead) __instance.Data.IsDead = true;

            // Remove fake tasks when player dies
            if (target.hasFakeTasks())
                target.clearAllTasks();

            // Lover suicide trigger on murder
            if ((Lovers.lover1 != null && target == Lovers.lover1) || (Lovers.lover2 != null && target == Lovers.lover2)) {
                PlayerControl otherLover = target == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                if (otherLover != null && !otherLover.Data.IsDead && Lovers.bothDie) {
                    otherLover.MurderPlayer(otherLover);
                }
            }

            // Sidekick promotion trigger on murder
            if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead && target == Jackal.jackal && Jackal.jackal == PlayerControl.LocalPlayer) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.sidekickPromotes();
            }

            // Pursuer promotion trigger on murder (the host sends the call such that everyone recieves the update before a possible game End)
            if (target == Lawyer.target && AmongUsClient.Instance.AmHost) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.lawyerPromotesToPursuer();
            }

            // Seer show flash and add dead player position
            if (Seer.seer != null && PlayerControl.LocalPlayer == Seer.seer && !Seer.seer.Data.IsDead && Seer.seer != target && Seer.mode <= 1) {
                Helpers.showFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f));
            }
            if (Seer.deadBodyPositions != null) Seer.deadBodyPositions.Add(target.transform.position);

            // Tracker store body positions
            if (Tracker.deadBodyPositions != null) Tracker.deadBodyPositions.Add(target.transform.position);

            // Medium add body
            if (Medium.deadBodies != null) {
                Medium.featureDeadBodies.Add(new Tuple<DeadPlayer, Vector3>(deadPlayer, target.transform.position));
            }

            // Set bountyHunter cooldown
            if (BountyHunter.bountyHunter != null && PlayerControl.LocalPlayer == BountyHunter.bountyHunter && __instance == BountyHunter.bountyHunter) {
                if (target == BountyHunter.bounty) {
                    BountyHunter.bountyHunter.SetKillTimer(BountyHunter.bountyKillCooldown);
                    BountyHunter.bountyUpdateTimer = 0f; // Force bounty update
                }
                else
                    BountyHunter.bountyHunter.SetKillTimer(PlayerControl.GameOptions.KillCooldown + BountyHunter.punishmentTime); 
            }

            // Mini Set Impostor Mini kill timer (Due to mini being a modifier, all "SetKillTimers" must have happened before this!)
            if (Mini.mini != null && __instance == Mini.mini && __instance == PlayerControl.LocalPlayer) {
                float multiplier = 1f;
                if (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini) multiplier = Mini.isGrownUp() ? 0.66f : 2f;
                Mini.mini.SetKillTimer(__instance.killTimer * multiplier);
            }

            // Cleaner Button Sync
            if (Cleaner.cleaner != null && PlayerControl.LocalPlayer == Cleaner.cleaner && __instance == Cleaner.cleaner && HudManagerStartPatch.cleanerCleanButton != null)
                HudManagerStartPatch.cleanerCleanButton.Timer = Cleaner.cleaner.killTimer;

            // Witch Button Sync
            if (Witch.triggerBothCooldowns && Witch.witch != null && PlayerControl.LocalPlayer == Witch.witch && __instance == Witch.witch && HudManagerStartPatch.witchSpellButton != null)
                HudManagerStartPatch.witchSpellButton.Timer = HudManagerStartPatch.witchSpellButton.MaxTimer;

            // Warlock Button Sync
            if (Warlock.warlock != null && PlayerControl.LocalPlayer == Warlock.warlock && __instance == Warlock.warlock && HudManagerStartPatch.warlockCurseButton != null) {
                if (Warlock.warlock.killTimer > HudManagerStartPatch.warlockCurseButton.Timer) {
                    HudManagerStartPatch.warlockCurseButton.Timer = Warlock.warlock.killTimer;
                }
            }
            // Ninja Button Sync
            if (Ninja.ninja != null && PlayerControl.LocalPlayer == Ninja.ninja && __instance == Ninja.ninja && HudManagerStartPatch.ninjaButton != null)
                HudManagerStartPatch.ninjaButton.Timer = HudManagerStartPatch.ninjaButton.MaxTimer;


            // Bait
            if (Bait.bait.FindAll(x => x.PlayerId == target.PlayerId).Count > 0) {
                float reportDelay = (float) rnd.Next((int)Bait.reportDelayMin, (int)Bait.reportDelayMax + 1);
                Bait.active.Add(deadPlayer, reportDelay);

                if (Bait.showKillFlash && __instance == PlayerControl.LocalPlayer) Helpers.showFlash(new Color(204f / 255f, 102f / 255f, 0f / 255f));
            }

            // Add Bloody Modifier
            if (Bloody.bloody.FindAll(x => x.PlayerId == target.PlayerId).Count > 0) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.Bloody, Hazel.SendOption.Reliable, -1);
                writer.Write(__instance.PlayerId);
                writer.Write(target.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.bloody(__instance.PlayerId, target.PlayerId);
            }

            // VIP Modifier
            if (Vip.vip.FindAll(x => x.PlayerId == target.PlayerId).Count > 0) {
                Color color = Color.yellow;
                if (Vip.showColor) {
                    color = Color.white;
                    if (target.Data.Role.IsImpostor) color = Color.red;
                    else if (RoleInfo.getRoleInfoForPlayer(target, false).FirstOrDefault().isNeutral) color = Color.blue;
                }
                Helpers.showFlash(color, 1.5f);
            }

            // First kill
            if (MapOptions.firstKillName == "") MapOptions.firstKillName = target.Data.PlayerName;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    class PlayerControlSetCoolDownPatch {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)]float time) {
            if (PlayerControl.GameOptions.KillCooldown <= 0f) return false;
            float multiplier = 1f;
            float addition = 0f;
            if (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini) multiplier = Mini.isGrownUp() ? 0.66f : 2f;
            if (BountyHunter.bountyHunter != null && PlayerControl.LocalPlayer == BountyHunter.bountyHunter) addition = BountyHunter.punishmentTime;

            __instance.killTimer = Mathf.Clamp(time, 0f, PlayerControl.GameOptions.KillCooldown * multiplier + addition);
            DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(__instance.killTimer, PlayerControl.GameOptions.KillCooldown * multiplier + addition);
            return false;
        }
    }

    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.CoPerformKill))]
    class KillAnimationCoPerformKillPatch {
        public static bool hideNextAnimation = false;
        public static void Prefix(KillAnimation __instance, [HarmonyArgument(0)]ref PlayerControl source, [HarmonyArgument(1)]ref PlayerControl target) {
            if (hideNextAnimation)
                source = target;
            hideNextAnimation = false;
        }
    }

    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.SetMovement))]
    class KillAnimationSetMovementPatch {
        private static int? colorId = null;
        public static void Prefix(PlayerControl source, bool canMove) {
            Color color = source.MyRend.material.GetColor("_BodyColor");
            if (color != null && Morphling.morphling != null && source.Data.PlayerId == Morphling.morphling.PlayerId) {
                var index = Palette.PlayerColors.IndexOf(color);
                if (index != -1) colorId = index;
            }
        }

        public static void Postfix(PlayerControl source, bool canMove) {
            if (colorId.HasValue) source.RawSetColor(colorId.Value);
            colorId = null;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class ExilePlayerPatch
    {
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

            // Pursuer promotion trigger on exile (the host sends the call such that everyone recieves the update before a possible game End)
            if (__instance == Lawyer.target && Lawyer.target != Jester.jester && AmongUsClient.Instance.AmHost) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LawyerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.lawyerPromotesToPursuer();
            }
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class PlayerPhysicsFixedUpdate {
        public static void Postfix(PlayerPhysics __instance) {
            if (__instance.AmOwner && !PlayerControl.LocalPlayer.Data.IsDead && Invert.invert.FindAll(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId).Count > 0 && Invert.meetings > 0 && GameData.Instance && __instance.myPlayer.CanMove)  __instance.body.velocity *= -1;
        }
    }
}
