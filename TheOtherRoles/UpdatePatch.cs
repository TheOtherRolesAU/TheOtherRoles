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
                
                bool hasVisibleShield = false;
                if (Camouflager.camouflageTimer <= 0f && Medic.shielded != null && (target == Medic.shielded || (target == Morphling.morphling && Morphling.morphTarget == Medic.shielded && Morphling.morphTimer > 0f))) {
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
            if (!Medic.usedShield) setPlayerOutline(Medic.currentTarget, Medic.color);
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
            setPlayerOutline(Jackal.currentTarget, Jackal.color);
        }

        static void sidekickSetTarget() {
            if (Sidekick.sidekick == null || Sidekick.sidekick != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if(Jackal.jackal != null) untargetablePlayers.Add(Jackal.jackal);
            if(Child.child != null && !Child.isGrownUp()) untargetablePlayers.Add(Child.child); // Exclude Sidekick from targeting the Child unless it has grown up
            Sidekick.currentTarget = setTarget(untargetablePlayers : untargetablePlayers);
            if (Sidekick.canKill) setPlayerOutline(Sidekick.currentTarget, Sidekick.color);
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

        public static void Postfix(PlayerControl __instance) {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

            // Child and Morphling shrink
            playerSizeUpdate(__instance);
            
            if (PlayerControl.LocalPlayer == __instance) {
                // Update player outlines
                setBasePlayerOutlines();

                // Update Role Description
                Helpers.refreshRoleDescription(__instance);

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
            } 
        }
    }
}