using System;
using System.Collections.Generic;
using System.Linq;
using Assets.CoreScripts;
using HarmonyLib;
using Hazel;
using InnerNet;
using TheOtherRoles.Objects;
using TheOtherRoles.Roles;
using TMPro;
using UnityEngine;
using static TheOtherRoles.RoleReloader;
using static TheOtherRoles.GameHistory;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class PlayerControlFixedUpdatePatch
    {
        private static readonly int Outline = Shader.PropertyToID("_Outline");

        private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");

        private static readonly int AddColor = Shader.PropertyToID("_AddColor");
        // Helpers

        private static PlayerControl SetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false,
            IReadOnlyCollection<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
        {
            PlayerControl result = null;
            var num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            if (!ShipStatus.Instance) return null;
            if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
            if (targetingPlayer.Data.IsDead) return null;

            var truePosition = targetingPlayer.GetTruePosition();
            var allPlayers = GameData.Instance.AllPlayers;
            foreach (var playerInfo in allPlayers)
            {
                if (playerInfo.Disconnected || playerInfo.PlayerId == targetingPlayer.PlayerId || playerInfo.IsDead ||
                    onlyCrewmates && playerInfo.IsImpostor) continue;
                var @object = playerInfo.Object;
                if (untargetablePlayers != null &&
                    untargetablePlayers.Any(x => x == @object)) // if that player is not targetable: skip check
                    continue;

                if (!@object || @object.inVent && !targetPlayersInVents) continue;
                var vector = @object.GetTruePosition() - truePosition;
                var magnitude = vector.magnitude;
                if (!(magnitude <= num) || PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized,
                    magnitude, Constants.ShipAndObjectsMask)) continue;
                result = @object;
                num = magnitude;
            }

            return result;
        }

        private static void SetPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.myRend == null) return;

            target.myRend.material.SetFloat(Outline, 1f);
            target.myRend.material.SetColor(OutlineColor, color);
        }

        // Update functions

        private static void SetBasePlayerOutlines()
        {
            foreach (var target in PlayerControl.AllPlayerControls)
            {
                if (target == null || target.myRend == null) continue;

                var isMorphedMorphling = target == Morphling.Instance.player && Morphling.morphTarget != null &&
                                         Morphling.morphTimer > 0f;
                var hasVisibleShield = false;
                if (Camouflager.camouflageTimer <= 0f && Medic.shielded != null &&
                    (target == Medic.shielded && !isMorphedMorphling ||
                     isMorphedMorphling && Morphling.morphTarget == Medic.shielded))
                    hasVisibleShield = Medic.showShielded == 0 // Everyone
                                       || Medic.showShielded == 1 && (PlayerControl.LocalPlayer == Medic.shielded ||
                                                                      PlayerControl.LocalPlayer ==
                                                                      Medic.Instance.player) // Shielded + Medic
                                       || Medic.showShielded == 2 &&
                                       PlayerControl.LocalPlayer == Medic.Instance.player; // Medic only

                if (hasVisibleShield)
                {
                    target.myRend.material.SetFloat(Outline, 1f);
                    target.myRend.material.SetColor(OutlineColor, Medic.shieldedColor);
                }
                else
                {
                    target.myRend.material.SetFloat(Outline, 0f);
                }
            }
        }

        private static void BendTimeUpdate()
        {
            if (TimeMaster.isRewinding)
            {
                if (localPlayerPositions.Count > 0)
                {
                    // Set position
                    var (position, couldMove) = localPlayerPositions[0];
                    if (couldMove)
                    {
                        // Exit current vent if necessary
                        if (PlayerControl.LocalPlayer.inVent)
                            foreach (var vent in ShipStatus.Instance.AllVents)
                            {
                                vent.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out _);
                                if (!canUse) continue;
                                PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(vent.Id);
                                vent.SetButtons(false);
                            }

                        // Set position
                        PlayerControl.LocalPlayer.transform.position = position;
                    }
                    else if (localPlayerPositions.Any(x => x.Item2))
                    {
                        PlayerControl.LocalPlayer.transform.position = position;
                    }

                    localPlayerPositions.RemoveAt(0);

                    if (localPlayerPositions.Count > 1)
                        localPlayerPositions
                            .RemoveAt(0); // Skip every second position to rewind twice as fast, but never skip the last position
                }
                else
                {
                    TimeMaster.isRewinding = false;
                    PlayerControl.LocalPlayer.moveable = true;
                }
            }
            else
            {
                while (localPlayerPositions.Count >= Mathf.Round(TimeMaster.rewindTime / Time.fixedDeltaTime))
                    localPlayerPositions.RemoveAt(localPlayerPositions.Count - 1);
                localPlayerPositions.Insert(0,
                    new Tuple<Vector3, bool>(PlayerControl.LocalPlayer.transform.position,
                        PlayerControl.LocalPlayer.CanMove)); // CanMove = CanMove
            }
        }

        private static void MedicSetTarget()
        {
            if (Medic.Instance.player == null || Medic.Instance.player != PlayerControl.LocalPlayer) return;
            Medic.currentTarget = SetTarget();
            if (!Medic.usedShield) SetPlayerOutline(Medic.currentTarget, Medic.shieldedColor);
        }

        private static void ShifterSetTarget()
        {
            if (Shifter.Instance.player == null || Shifter.Instance.player != PlayerControl.LocalPlayer) return;
            Shifter.currentTarget = SetTarget();
            if (Shifter.futureShift == null) SetPlayerOutline(Shifter.currentTarget, Shifter.Instance.color);
        }


        private static void MorphlingSetTarget()
        {
            if (Morphling.Instance.player == null || Morphling.Instance.player != PlayerControl.LocalPlayer) return;
            Morphling.currentTarget = SetTarget();
            SetPlayerOutline(Morphling.currentTarget, Morphling.Instance.color);
        }

        private static void SheriffSetTarget()
        {
            if (Sheriff.Instance.player == null || Sheriff.Instance.player != PlayerControl.LocalPlayer) return;
            Sheriff.currentTarget = SetTarget();
            SetPlayerOutline(Sheriff.currentTarget, Sheriff.Instance.color);
        }

        private static void TrackerSetTarget()
        {
            if (Tracker.Instance.player == null || Tracker.Instance.player != PlayerControl.LocalPlayer) return;
            Tracker.currentTarget = SetTarget();
            if (!Tracker.usedTracker) SetPlayerOutline(Tracker.currentTarget, Tracker.Instance.color);
        }

        private static void DetectiveUpdateFootPrints()
        {
            if (Detective.Instance.player == null || Detective.Instance.player != PlayerControl.LocalPlayer) return;

            Detective.timer -= Time.fixedDeltaTime;
            if (!(Detective.timer <= 0f)) return;
            Detective.timer = Detective.footprintInterval;
            foreach (var player in PlayerControl.AllPlayerControls)
                if (player != null && player != PlayerControl.LocalPlayer && !player.Data.IsDead && !player.inVent)
                    _ = new Footprint(Detective.footprintDuration, Detective.anonymousFootprints, player);
        }

        private static void VampireSetTarget()
        {
            if (Vampire.Instance.player == null || Vampire.Instance.player != PlayerControl.LocalPlayer) return;

            var target = Spy.Instance.player
                ? Spy.impostorsCanKillAnyone ? SetTarget(targetPlayersInVents: true) :
                SetTarget(true, true, new List<PlayerControl> {Spy.Instance.player})
                : SetTarget(true, true);

            var targetNearGarlic = target != null && Garlic.garlics.Any(garlic =>
                Vector2.Distance(garlic.garlic.transform.position, target.transform.position) <= 1.91f);
            Vampire.targetNearGarlic = targetNearGarlic;
            Vampire.currentTarget = target;
            SetPlayerOutline(Vampire.currentTarget, Vampire.Instance.color);
        }

        private static void JackalSetTarget()
        {
            if (Jackal.Instance.player == null || Jackal.Instance.player != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            // Only exclude sidekick from being targeted if the jackal can create sidekicks from impostors
            if (Jackal.canCreateSidekickFromImpostor)
                if (Sidekick.Instance.player != null)
                    untargetablePlayers.Add(Sidekick.Instance.player);
            if (Mini.Instance.player != null && !Mini.IsGrownUp())
                untargetablePlayers.Add(Mini.Instance
                    .player); // Exclude Jackal from targeting the Mini unless it has grown up
            Jackal.currentTarget = SetTarget(untargetablePlayers: untargetablePlayers);
            SetPlayerOutline(Jackal.currentTarget, Palette.ImpostorRed);
        }

        private static void SidekickSetTarget()
        {
            if (Sidekick.Instance.player == null || Sidekick.Instance.player != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if (Jackal.Instance.player != null) untargetablePlayers.Add(Jackal.Instance.player);
            if (Mini.Instance.player != null && !Mini.IsGrownUp())
                untargetablePlayers.Add(Mini.Instance
                    .player); // Exclude Sidekick from targeting the Mini unless it has grown up
            Sidekick.currentTarget = SetTarget(untargetablePlayers: untargetablePlayers);
            if (Sidekick.canKill) SetPlayerOutline(Sidekick.currentTarget, Palette.ImpostorRed);
        }

        private static void SidekickCheckPromotion()
        {
            // If LocalPlayer is Sidekick, the Jackal is disconnected and Sidekick promotion is enabled, then trigger promotion
            if (Sidekick.Instance.player == null || Sidekick.Instance.player != PlayerControl.LocalPlayer) return;
            if (Sidekick.Instance.player.Data.IsDead || !Sidekick.promotesToJackal) return;
            if (Jackal.Instance.player != null && Jackal.Instance.player.Data?.Disconnected != true) return;
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRPC.SidekickPromotes, SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SidekickPromotes();
        }

        private static void EraserSetTarget()
        {
            if (Eraser.Instance.player == null || Eraser.Instance.player != PlayerControl.LocalPlayer) return;

            var untargetables = new List<PlayerControl>();
            if (Spy.Instance.player) untargetables.Add(Spy.Instance.player);
            Eraser.currentTarget = SetTarget(!Eraser.canEraseAnyone,
                untargetablePlayers: Eraser.canEraseAnyone ? new List<PlayerControl>() : untargetables);
            SetPlayerOutline(Eraser.currentTarget, Eraser.Instance.color);
        }

        private static void EngineerUpdate()
        {
            if (!PlayerControl.LocalPlayer.Data.IsImpostor || !ShipStatus.Instance ||
                ShipStatus.Instance.AllVents == null) return;
            foreach (var vent in ShipStatus.Instance.AllVents)
                try
                {
                    if (!vent || !vent.myRend || vent.myRend.material == null) continue;
                    if (Engineer.Instance.player != null && Engineer.Instance.player.inVent)
                    {
                        vent.myRend.material.SetFloat(Outline, 1f);
                        vent.myRend.material.SetColor(OutlineColor, Engineer.Instance.color);
                    }
                    else if (vent.myRend.material.GetColor(AddColor) != Color.red)
                    {
                        vent.myRend.material.SetFloat(Outline, 0);
                    }
                }
                catch
                {
                    // ignored
                }
        }

        private static void ImpostorSetTarget()
        {
            if (!PlayerControl.LocalPlayer.Data.IsImpostor || !PlayerControl.LocalPlayer.CanMove ||
                PlayerControl.LocalPlayer.Data.IsDead)
            {
                // !isImpostor || !canMove || isDead
                HudManager.Instance.KillButton.SetTarget(null);
                return;
            }

            var target = Spy.Instance.player != null
                ? Spy.impostorsCanKillAnyone ? SetTarget(false, true) :
                SetTarget(true, true, new List<PlayerControl> {Spy.Instance.player})
                : SetTarget(true, true);

            HudManager.Instance.KillButton.SetTarget(target); // Includes setPlayerOutline(target, Palette.ImpostorRed);
        }

        private static void WarlockSetTarget()
        {
            if (Warlock.Instance.player == null || Warlock.Instance.player != PlayerControl.LocalPlayer) return;
            if (Warlock.curseVictim != null &&
                (Warlock.curseVictim.Data.Disconnected ||
                 Warlock.curseVictim.Data
                     .IsDead)) // If the cursed victim is disconnected or dead reset the curse so a new curse can be applied
                Warlock.ResetCurse();
            if (Warlock.curseVictim == null)
            {
                Warlock.currentTarget = SetTarget();
                SetPlayerOutline(Warlock.currentTarget, Warlock.Instance.color);
            }
            else
            {
                Warlock.curseVictimTarget = SetTarget(targetingPlayer: Warlock.curseVictim);
                SetPlayerOutline(Warlock.curseVictimTarget, Warlock.Instance.color);
            }
        }

        private static void TrackerUpdate()
        {
            if (Tracker.arrow?.arrow == null) return;

            if (Tracker.Instance.player == null || PlayerControl.LocalPlayer != Tracker.Instance.player)
            {
                Tracker.arrow.arrow.SetActive(false);
                return;
            }

            if (Tracker.Instance.player == null || Tracker.tracked == null ||
                PlayerControl.LocalPlayer != Tracker.Instance.player ||
                Tracker.Instance.player.Data.IsDead) return;
            Tracker.timeUntilUpdate -= Time.fixedDeltaTime;

            if (Tracker.timeUntilUpdate <= 0f)
            {
                var trackedOnMap = !Tracker.tracked.Data.IsDead;
                var position = Tracker.tracked.transform.position;
                if (!trackedOnMap)
                {
                    // Check for dead body
                    var body = Object.FindObjectsOfType<DeadBody>()
                        .FirstOrDefault(b => b.ParentId == Tracker.tracked.PlayerId);
                    if (body != null)
                    {
                        trackedOnMap = true;
                        position = body.transform.position;
                    }
                }

                Tracker.arrow.Update(position);
                Tracker.arrow.arrow.SetActive(trackedOnMap);
                Tracker.timeUntilUpdate = Tracker.updateInterval;
            }
            else
            {
                Tracker.arrow.Update();
            }
        }

        private static void PlayerSizeUpdate(PlayerControl p)
        {
            // Set default player size
            var collider = p.GetComponent<CircleCollider2D>();

            p.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            collider.radius = Mini.DefaultColliderRadius;
            collider.offset = Mini.DefaultColliderOffset * Vector2.down;

            // Set adapted player size to Mini and Morphling
            if (Mini.Instance.player == null || Camouflager.camouflageTimer > 0f) return;

            var growingProgress = Mini.GrowingProgress();
            var scale = growingProgress * 0.35f + 0.35f;
            var correctedColliderRadius =
                Mini.DefaultColliderRadius * 0.7f /
                scale; // scale / 0.7f is the factor by which we decrease the player size, hence we need to increase the collider size by 0.7f / scale

            if (p == Mini.Instance.player)
            {
                p.transform.localScale = new Vector3(scale, scale, 1f);
                collider.radius = correctedColliderRadius;
            }

            if (Morphling.Instance.player == null || p != Morphling.Instance.player ||
                Morphling.morphTarget != Mini.Instance.player ||
                !(Morphling.morphTimer > 0f)) return;
            p.transform.localScale = new Vector3(scale, scale, 1f);
            collider.radius = correctedColliderRadius;
        }

        private static void UpdatePlayerInfo()
        {
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p != PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead) continue;

                var playerInfoTransform = p.nameText.transform.parent.FindChild("Info");
                var playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TextMeshPro>() : null;
                if (playerInfo == null)
                {
                    playerInfo = Object.Instantiate(p.nameText, p.nameText.transform.parent);
                    playerInfo.transform.localPosition += Vector3.up * 0.5f;
                    playerInfo.fontSize *= 0.75f;
                    playerInfo.gameObject.name = "Info";
                }

                var playerVoteArea =
                    MeetingHud.Instance.playerStates?.FirstOrDefault(x => x.TargetPlayerId == p.PlayerId);
                var meetingInfoTransform = playerVoteArea != null
                    ? playerVoteArea.NameText.transform.parent.FindChild("Info")
                    : null;
                var meetingInfo = meetingInfoTransform != null
                    ? meetingInfoTransform.GetComponent<TextMeshPro>()
                    : null;
                if (meetingInfo == null && playerVoteArea != null)
                {
                    meetingInfo = Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                    meetingInfo.transform.localPosition += Vector3.down * 0.20f;
                    meetingInfo.fontSize *= 0.75f;
                    meetingInfo.gameObject.name = "Info";
                }

                var (tasksCompleted, tasksTotal) = TasksHandler.TaskInfo(p.Data);
                var roleNames = string.Join(" ",
                    RoleInfo.GetRoleInfoForPlayer(p).Select(x => Helpers.Cs(x.color, x.name)).ToArray());
                var taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({tasksCompleted}/{tasksTotal})</color>" : "";

                var playerInfoText = "";
                var meetingInfoText = "";
                if (p == PlayerControl.LocalPlayer)
                {
                    playerInfoText = $"{roleNames}";
                    if (DestroyableSingleton<TaskPanelBehaviour>.InstanceExists)
                    {
                        var tabText = DestroyableSingleton<TaskPanelBehaviour>.Instance.tab.transform
                            .FindChild("TabText_TMP").GetComponent<TextMeshPro>();
                        tabText.SetText($"Tasks {taskInfo}");
                    }

                    meetingInfoText = $"{roleNames} {taskInfo}".Trim();
                }
                else if (MapOptions.ghostsSeeRoles && MapOptions.ghostsSeeTasks)
                {
                    playerInfoText = $"{roleNames} {taskInfo}".Trim();
                    meetingInfoText = playerInfoText;
                }
                else if (MapOptions.ghostsSeeTasks)
                {
                    playerInfoText = $"{taskInfo}".Trim();
                    meetingInfoText = playerInfoText;
                }
                else if (MapOptions.ghostsSeeRoles)
                {
                    playerInfoText = $"{roleNames}";
                    meetingInfoText = playerInfoText;
                }

                playerInfo.text = playerInfoText;
                playerInfo.gameObject.SetActive(p.Visible);
                if (meetingInfo != null)
                    meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results
                        ? ""
                        : meetingInfoText;
            }
        }

        private static void SecurityGuardSetTarget()
        {
            if (SecurityGuard.Instance.player == null || SecurityGuard.Instance.player != PlayerControl.LocalPlayer ||
                ShipStatus.Instance == null || ShipStatus.Instance.AllVents == null) return;

            Vent target = null;
            var truePosition = PlayerControl.LocalPlayer.GetTruePosition();
            var closestDistance = float.MaxValue;
            foreach (var vent in ShipStatus.Instance.AllVents)
            {
                if (vent.gameObject.name.StartsWith("JackInTheBoxVent_") ||
                    vent.gameObject.name.StartsWith("SealedVent_") ||
                    vent.gameObject.name.StartsWith("FutureSealedVent_")) continue;
                var distance = Vector2.Distance(vent.transform.position, truePosition);
                if (!(distance <= vent.UsableDistance) || !(distance < closestDistance)) continue;
                closestDistance = distance;
                target = vent;
            }

            SecurityGuard.ventTarget = target;
        }

        private static void ArsonistSetTarget()
        {
            if (Arsonist.Instance.player == null || Arsonist.Instance.player != PlayerControl.LocalPlayer) return;
            List<PlayerControl> untargetables;
            if (Arsonist.douseTarget != null)
                untargetables = PlayerControl.AllPlayerControls.ToArray()
                    .Where(x => x.PlayerId != Arsonist.douseTarget.PlayerId).ToList();
            else
                untargetables = Arsonist.DousedPlayers;
            Arsonist.currentTarget = SetTarget(untargetablePlayers: untargetables);
            if (Arsonist.currentTarget != null) SetPlayerOutline(Arsonist.currentTarget, Arsonist.Instance.color);
        }

        private static void SnitchUpdate()
        {
            if (Snitch.LocalArrows == null) return;

            foreach (var arrow in Snitch.LocalArrows) arrow.arrow.SetActive(false);

            if (Snitch.Instance.player == null || Snitch.Instance.player.Data.IsDead) return;

            var (playerCompleted, playerTotal) = TasksHandler.TaskInfo(Snitch.Instance.player.Data);
            var numberOfTasks = playerTotal - playerCompleted;

            if (numberOfTasks <= Snitch.taskCountForReveal && (PlayerControl.LocalPlayer.Data.IsImpostor ||
                                                               Snitch.includeTeamJackal &&
                                                               (PlayerControl.LocalPlayer == Jackal.Instance.player ||
                                                                PlayerControl.LocalPlayer == Sidekick.Instance.player)))
            {
                if (Snitch.LocalArrows.Count == 0) Snitch.LocalArrows.Add(new Arrow(Color.blue));
                if (Snitch.LocalArrows.Count == 0 || Snitch.LocalArrows[0] == null) return;
                Snitch.LocalArrows[0].arrow.SetActive(true);
                Snitch.LocalArrows[0].Update(Snitch.Instance.player.transform.position);
            }
            else if (PlayerControl.LocalPlayer == Snitch.Instance.player && numberOfTasks == 0)
            {
                var arrowIndex = 0;
                foreach (var p in PlayerControl.AllPlayerControls)
                    if (!p.Data.IsDead && (p.Data.IsImpostor ||
                                           Snitch.includeTeamJackal && (p == Jackal.Instance.player ||
                                                                        p == Sidekick.Instance.player)))
                    {
                        if (arrowIndex >= Snitch.LocalArrows.Count)
                        {
                            if (Snitch.teamJackalUseDifferentArrowColor &&
                                (p == Jackal.Instance.player || p == Sidekick.Instance.player))
                                Snitch.LocalArrows.Add(new Arrow(Jackal.Instance.color));
                            else Snitch.LocalArrows.Add(new Arrow(Palette.ImpostorRed));
                        }

                        if (arrowIndex < Snitch.LocalArrows.Count && Snitch.LocalArrows[arrowIndex] != null)
                        {
                            Snitch.LocalArrows[arrowIndex].arrow.SetActive(true);
                            Snitch.LocalArrows[arrowIndex].Update(p.transform.position);
                        }

                        arrowIndex++;
                    }
            }
        }

        private static void BountyHunterUpdate()
        {
            if (BountyHunter.Instance.player == null ||
                PlayerControl.LocalPlayer != BountyHunter.Instance.player) return;

            if (BountyHunter.Instance.player.Data.IsDead)
            {
                if (BountyHunter.arrow != null || BountyHunter.arrow?.arrow != null)
                    Object.Destroy(BountyHunter.arrow.arrow);
                BountyHunter.arrow = null;
                if (BountyHunter.cooldownText != null && BountyHunter.cooldownText.gameObject != null)
                    Object.Destroy(BountyHunter.cooldownText.gameObject);
                BountyHunter.cooldownText = null;
                BountyHunter.bounty = null;
                foreach (var p in MapOptions.playerIcons.Values.Where(p => p != null && p.gameObject != null))
                    p.gameObject.SetActive(false);
                return;
            }

            BountyHunter.arrowUpdateTimer -= Time.fixedDeltaTime;
            BountyHunter.bountyUpdateTimer -= Time.fixedDeltaTime;

            if (BountyHunter.bounty == null || BountyHunter.bountyUpdateTimer <= 0f)
            {
                // Set new bounty
                BountyHunter.bounty = null;
                BountyHunter.arrowUpdateTimer = 0f; // Force arrow to update
                BountyHunter.bountyUpdateTimer = BountyHunter.bountyDuration;
                var possibleTargets = new List<PlayerControl>();
                foreach (var p in PlayerControl.AllPlayerControls)
                    if (!p.Data.IsDead && !p.Data.Disconnected && p != p.Data.IsImpostor && p != Spy.Instance.player &&
                        (p != Mini.Instance.player || Mini.IsGrownUp()))
                        possibleTargets.Add(p);
                BountyHunter.bounty = possibleTargets[Rng.Next(0, possibleTargets.Count)];
                if (BountyHunter.bounty == null) return;

                // Show poolable player
                if (HudManager.Instance != null && HudManager.Instance.UseButton != null)
                {
                    foreach (var pp in MapOptions.playerIcons.Values) pp.gameObject.SetActive(false);
                    if (MapOptions.playerIcons.ContainsKey(BountyHunter.bounty.PlayerId) &&
                        MapOptions.playerIcons[BountyHunter.bounty.PlayerId].gameObject != null)
                        MapOptions.playerIcons[BountyHunter.bounty.PlayerId].gameObject.SetActive(true);
                }
            }

            // Update Cooldown Text
            if (BountyHunter.cooldownText != null)
                BountyHunter.cooldownText.text = Mathf
                    .CeilToInt(Mathf.Clamp(BountyHunter.bountyUpdateTimer, 0, BountyHunter.bountyDuration)).ToString();

            // Update Arrow
            if (!BountyHunter.showArrow || BountyHunter.bounty == null) return;
            BountyHunter.arrow ??= new Arrow(Color.red);
            if (BountyHunter.arrowUpdateTimer <= 0f)
            {
                BountyHunter.arrow.Update(BountyHunter.bounty.transform.position);
                BountyHunter.arrowUpdateTimer = BountyHunter.arrowUpdateInterval;
            }

            BountyHunter.arrow.Update();
        }

        private static void BaitUpdate()
        {
            if (Bait.Instance.player == null || Bait.Instance.player != PlayerControl.LocalPlayer) return;

            // Bait report
            if (Bait.Instance.player.Data.IsDead && !Bait.reported)
            {
                Bait.reportDelay -= Time.fixedDeltaTime;
                var deadPlayer =
                     deadPlayers?.FirstOrDefault(x =>
                         x.player != null && x.player.PlayerId == Bait.Instance.player.PlayerId);
                
                if (deadPlayer != null && deadPlayer.killerIfExisting != null && Bait.reportDelay <= 0f)
                {
                    Helpers.HandleVampireBiteOnBodyReport(); // Manually call Vampire handling, since the CmdReportDeadBody Prefix won't be called
                    RPCProcedure.UncheckedCmdReportDeadBody(deadPlayer.killerIfExisting.PlayerId, Bait.Instance.player.PlayerId);

                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedCmdReportDeadBody, Hazel.SendOption.Reliable, -1);
                    writer.Write(deadPlayer.killerIfExisting.PlayerId);
                    writer.Write(Bait.Instance.player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);

                    Bait.reported = true;
                }
            }

            // Bait Vents
            if (!ShipStatus.Instance || ShipStatus.Instance.AllVents == null) return;
            {
                var ventsWithPlayers = new List<int>();
                foreach (var player in PlayerControl.AllPlayerControls)
                    if (player.inVent)
                    {
                        var target = ShipStatus.Instance.AllVents
                            .OrderBy(x => Vector2.Distance(x.transform.position, player.GetTruePosition()))
                            .FirstOrDefault();
                        if (target != null) ventsWithPlayers.Add(target.Id);
                    }

                foreach (var vent in ShipStatus.Instance.AllVents)
                {
                    if (vent.myRend == null || vent.myRend.material == null) continue;
                    if (ventsWithPlayers.Contains(vent.Id) || ventsWithPlayers.Count > 0 && Bait.highlightAllVents)
                    {
                        vent.myRend.material.SetFloat(Outline, 1f);
                        vent.myRend.material.SetColor(OutlineColor, Color.yellow);
                    }
                    else
                    {
                        vent.myRend.material.SetFloat(Outline, 0);
                    }
                }
            }
        }

        public static void Postfix(PlayerControl __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return;

            // Mini and Morphling shrink
            PlayerSizeUpdate(__instance);

            if (PlayerControl.LocalPlayer != __instance) return;
            // Update player outlines
            SetBasePlayerOutlines();

            // Update Role Description
            Helpers.RefreshRoleDescription(__instance);

            // Update Player Info
            UpdatePlayerInfo();

            // Time Master
            BendTimeUpdate();
            // Morphling
            MorphlingSetTarget();
            // Medic
            MedicSetTarget();
            // Shifter
            ShifterSetTarget();
            // Sheriff
            SheriffSetTarget();
            // Detective
            DetectiveUpdateFootPrints();
            // Tracker
            TrackerSetTarget();
            // Vampire
            VampireSetTarget();
            Garlic.UpdateAll();
            // Eraser
            EraserSetTarget();
            // Engineer
            EngineerUpdate();
            // Tracker
            TrackerUpdate();
            // Jackal
            JackalSetTarget();
            // Sidekick
            SidekickSetTarget();
            // Impostor
            ImpostorSetTarget();
            // Warlock
            WarlockSetTarget();
            // Check for sidekick promotion on Jackal disconnect
            SidekickCheckPromotion();
            // SecurityGuard
            SecurityGuardSetTarget();
            // Arsonist
            ArsonistSetTarget();
            // Snitch
            SnitchUpdate();
            // BountyHunter
            BountyHunterUpdate();
            // Bait
            BaitUpdate();
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.WalkPlayerTo))]
    internal class PlayerPhysicsWalkPlayerToPatch
    {
        public static void Prefix(PlayerPhysics __instance)
        {
            var correctOffset = Camouflager.camouflageTimer <= 0f && (__instance.myPlayer == Mini.Instance.player ||
                                                                      Morphling.Instance.player != null &&
                                                                      __instance.myPlayer ==
                                                                      Morphling.Instance.player &&
                                                                      Morphling.morphTarget == Mini.Instance.player &&
                                                                      Morphling.morphTimer > 0f);
            if (!correctOffset) return;
            var currentScaling = (Mini.GrowingProgress() + 1) * 0.5f;
            __instance.myPlayer.Collider.offset = currentScaling * Mini.DefaultColliderOffset * Vector2.down;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    internal class PlayerControlCmdReportDeadBodyPatch
    {
        public static void Prefix()
        {
            Helpers.HandleVampireBiteOnBodyReport();
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcMurderPlayer))]
    internal class RpcMurderPlayer
    {
        public static bool Prefix([HarmonyArgument(0)] PlayerControl target)
        {
            if (!Helpers.HandleMurderAttempt(target)) return false;
            // Custom checks
            if (Mini.Instance.player != null && PlayerControl.LocalPlayer == Mini.Instance.player ||
                BountyHunter.Instance.player != null &&
                PlayerControl.LocalPlayer == BountyHunter.Instance.player)
            {
                // Not checked by official servers
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.UncheckedMurderPlayer, SendOption.Reliable, -1);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write(target.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.UncheckedMurderPlayer(PlayerControl.LocalPlayer.PlayerId, target.PlayerId);
            }
            else
            {
                // Checked by official servers
                return true;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.LocalPlayer.CmdReportDeadBody))]
    internal class BodyReportPatch
    {
        private static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo target)
        {
            // Medic or Detective report
            var isMedicReport = Medic.Instance.player != null && Medic.Instance.player == PlayerControl.LocalPlayer &&
                                __instance.PlayerId == Medic.Instance.player.PlayerId;
            var isDetectiveReport = Detective.Instance.player != null &&
                                    Detective.Instance.player == PlayerControl.LocalPlayer &&
                                    __instance.PlayerId == Detective.Instance.player.PlayerId;
            if (!isMedicReport && !isDetectiveReport) return;
            var deadPlayer =
                deadPlayers?.FirstOrDefault(x => x.player != null && x.player.PlayerId == target?.PlayerId);

            if (deadPlayer == null || deadPlayer.killerIfExisting == null) return;
            var timeSinceDeath = (float) (DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds;
            string msg;

            if (isMedicReport)
            {
                msg = $"Body Report: Killed {Math.Round(timeSinceDeath / 1000)}s ago!";
            }
            else
            {
                if (timeSinceDeath < Detective.reportNameDuration * 1000)
                {
                    msg = $"Body Report: The killer appears to be {deadPlayer.killerIfExisting.name}!";
                }
                else if (timeSinceDeath < Detective.reportColorDuration * 1000)
                {
                    var typeOfColor = Helpers.IsLighterColor(deadPlayer.killerIfExisting.Data.ColorId)
                        ? "lighter"
                        : "darker";
                    msg = $"Body Report: The killer appears to be a {typeOfColor} color!";
                }
                else
                {
                    msg = "Body Report: The corpse is too old to gain information from!";
                }
            }

            if (string.IsNullOrWhiteSpace(msg)) return;
            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg);
            if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                DestroyableSingleton<Telemetry>.Instance.SendWho();
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static class MurderPlayerPatch
    {
        private static bool resetToCrewmate;
        private static bool resetToDead;

        public static void Prefix(PlayerControl __instance)
        {
            // Allow everyone to murder players
            resetToCrewmate = !__instance.Data.IsImpostor;
            resetToDead = __instance.Data.IsDead;
            __instance.Data.IsImpostor = true;
            __instance.Data.IsDead = false;
        }

        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            // Collect dead player info
            var deadPlayer = new DeadPlayer(target, DateTime.UtcNow, DeathReason.Kill, __instance);
            deadPlayers.Add(deadPlayer);

            // Reset killer to crewmate if resetToCrewmate
            if (resetToCrewmate) __instance.Data.IsImpostor = false;
            if (resetToDead) __instance.Data.IsDead = true;

            // Remove fake tasks when player dies
            if (target.HasFakeTasks())
                target.ClearAllTasks();

            // Lover suicide trigger on murder
            if (Lovers.Instance.player != null && target == Lovers.Instance.player ||
                Lovers.Instance.secondPlayer != null && target == Lovers.Instance.secondPlayer)
            {
                var otherLover = target == Lovers.Instance.player
                    ? Lovers.Instance.secondPlayer
                    : Lovers.Instance.player;
                if (otherLover != null && !otherLover.Data.IsDead && Lovers.bothDie)
                    otherLover.MurderPlayer(otherLover);
            }

            // Sidekick promotion trigger on murder
            if (Sidekick.promotesToJackal && Sidekick.Instance.player != null &&
                !Sidekick.Instance.player.Data.IsDead &&
                target == Jackal.Instance.player && Jackal.Instance.player == PlayerControl.LocalPlayer)
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.SidekickPromotes, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.SidekickPromotes();
            }

            // Cleaner Button Sync
            if (Cleaner.Instance.player != null && PlayerControl.LocalPlayer == Cleaner.Instance.player &&
                __instance == Cleaner.Instance.player && HudManagerStartPatch.cleanerCleanButton != null)
                HudManagerStartPatch.cleanerCleanButton.timer = Cleaner.Instance.player.killTimer;

            // Warlock Button Sync
            if (Warlock.Instance.player != null && PlayerControl.LocalPlayer == Warlock.Instance.player &&
                __instance == Warlock.Instance.player && HudManagerStartPatch.warlockCurseButton != null)
                if (Warlock.Instance.player.killTimer > HudManagerStartPatch.warlockCurseButton.timer)
                    HudManagerStartPatch.warlockCurseButton.timer = Warlock.Instance.player.killTimer;

            // Seer show flash and add dead player position
            if (Seer.Instance.player != null && PlayerControl.LocalPlayer == Seer.Instance.player &&
                !Seer.Instance.player.Data.IsDead &&
                Seer.Instance.player != target && Seer.mode <= 1)
            {
                HudManager.Instance.FullScreen.enabled = true;
                HudManager.Instance.StartCoroutine(Effects.Lerp(1f, new Action<float>(p =>
                {
                    var renderer = HudManager.Instance.FullScreen;
                    if (p < 0.5)
                    {
                        if (renderer != null)
                            renderer.color = new Color(42f / 255f, 187f / 255f, 245f / 255f,
                                Mathf.Clamp01(p * 2 * 0.75f));
                    }
                    else
                    {
                        if (renderer != null)
                            renderer.color = new Color(42f / 255f, 187f / 255f, 245f / 255f,
                                Mathf.Clamp01((1 - p) * 2 * 0.75f));
                    }

                    if (Math.Abs(p - 1f) < 0.1f && renderer != null) renderer.enabled = false;
                })));
            }

            if (Seer.deadBodyPositions != null) Seer.deadBodyPositions.Add(target.transform.position);

            // Mini set adapted kill cooldown
            if (Mini.Instance.player != null && PlayerControl.LocalPlayer == Mini.Instance.player &&
                Mini.Instance.player.Data.IsImpostor &&
                Mini.Instance.player == __instance)
            {
                var multiplier = Mini.IsGrownUp() ? 0.66f : 2f;
                Mini.Instance.player.SetKillTimer(PlayerControl.GameOptions.KillCooldown * multiplier);
            }

            // Set bountyHunter cooldown
            if (BountyHunter.Instance.player == null || PlayerControl.LocalPlayer != BountyHunter.Instance.player ||
                __instance != BountyHunter.Instance.player) return;
            if (target == BountyHunter.bounty)
            {
                BountyHunter.Instance.player.SetKillTimer(BountyHunter.bountyKillCooldown);
                BountyHunter.bountyUpdateTimer = 0f; // Force bounty update
            }
            else
            {
                BountyHunter.Instance.player.SetKillTimer(PlayerControl.GameOptions.KillCooldown +
                                                          BountyHunter.punishmentTime);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    internal class PlayerControlSetCoolDownPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
        {
            if (PlayerControl.GameOptions.KillCooldown <= 0f) return false;
            var multiplier = 1f;
            var addition = 0f;
            if (Mini.Instance.player != null && PlayerControl.LocalPlayer == Mini.Instance.player &&
                Mini.Instance.player.Data.IsImpostor)
                multiplier = Mini.IsGrownUp() ? 0.66f : 2f;
            if (BountyHunter.Instance.player != null && PlayerControl.LocalPlayer == BountyHunter.Instance.player)
                addition = BountyHunter.punishmentTime;

            __instance.killTimer =
                Mathf.Clamp(time, 0f, PlayerControl.GameOptions.KillCooldown * multiplier + addition);
            DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(__instance.killTimer,
                PlayerControl.GameOptions.KillCooldown * multiplier + addition);
            return false;
        }
    }

    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.CoPerformKill))]
    internal class KillAnimationCoPerformKillPatch
    {
        public static void Prefix([HarmonyArgument(0)] ref PlayerControl source,
            [HarmonyArgument(1)] ref PlayerControl target)
        {
            if (Vampire.Instance.player != null && Vampire.Instance.player == source && Vampire.bitten != null &&
                Vampire.bitten == target)
                source = target;

            if (Warlock.Instance.player == null || Warlock.Instance.player != source ||
                Warlock.curseKillTarget == null ||
                Warlock.curseKillTarget != target) return;
            source = target;
            Warlock.curseKillTarget = null; // Reset here
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class ExilePlayerPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            // Collect dead player info
            var deadPlayer = new DeadPlayer(__instance, DateTime.UtcNow, DeathReason.Exile, null);
            deadPlayers.Add(deadPlayer);

            // Remove fake tasks when player dies
            if (__instance.HasFakeTasks())
                __instance.ClearAllTasks();

            // Lover suicide trigger on exile
            if (Lovers.Instance.player != null && __instance == Lovers.Instance.player ||
                Lovers.Instance.secondPlayer != null && __instance == Lovers.Instance.secondPlayer)
            {
                var otherLover = __instance == Lovers.Instance.player
                    ? Lovers.Instance.secondPlayer
                    : Lovers.Instance.player;
                if (otherLover != null && !otherLover.Data.IsDead && Lovers.bothDie)
                    otherLover.Exiled();
            }

            // Sidekick promotion trigger on exile
            if (!Sidekick.promotesToJackal || Sidekick.Instance.player == null ||
                Sidekick.Instance.player.Data.IsDead ||
                __instance != Jackal.Instance.player || Jackal.Instance.player != PlayerControl.LocalPlayer) return;
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRPC.SidekickPromotes, SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SidekickPromotes();
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanMove), MethodType.Getter)]
    internal class PlayerControlCanMovePatch
    {
        public static bool Prefix(PlayerControl __instance, ref bool __result)
        {
            __result = __instance.moveable &&
                       !Minigame.Instance &&
                       (!DestroyableSingleton<HudManager>.InstanceExists ||
                        !DestroyableSingleton<HudManager>.Instance.Chat.IsOpen &&
                        !DestroyableSingleton<HudManager>.Instance.KillOverlay.IsOpen &&
                        !DestroyableSingleton<HudManager>.Instance.GameMenu.IsOpen) &&
                       (!MapBehaviour.Instance || !MapBehaviour.Instance.IsOpenStopped) &&
                       !MeetingHud.Instance &&
                       !CustomPlayerMenu.Instance &&
                       !ExileController.Instance &&
                       !IntroCutscene.Instance;
            return false;
        }
    }
}