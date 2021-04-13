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

using SystemTypes = BCPJLGGNHBC;
using Palette = BLMBFIODBKL;
using Constants = LNCOKMACBKP;
using PhysicsHelpers = FJFJIDCFLDJ;
using DeathReason = EGHDCAKGMKI;
using GameOptionsData = CEIOGGEDKAN;
using Effects = AEOEPNHOJDP;

namespace TheOtherRoles {
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class PlayerControlFixedUpdatePatch
    {
        public static void bendTimeUpdate() {
            if (TimeMaster.isRewinding) {
                if (localPlayerPositions.Count > 0) {
                    // Set position
                    var next = localPlayerPositions[0];
                    if (!PlayerControl.LocalPlayer.inVent)
                        PlayerControl.LocalPlayer.transform.position = next.Item1;
                    localPlayerPositions.RemoveAt(0);
                    if (localPlayerPositions.Count > 0) localPlayerPositions.RemoveAt(0); // Skip every second position to rewinde in half the time
                } else {
                    TimeMaster.isRewinding = false;
                    PlayerControl.LocalPlayer.moveable = true;
                }
            } else {
                while (localPlayerPositions.Count >= Mathf.Round(TimeMaster.rewindTime / Time.fixedDeltaTime)) localPlayerPositions.RemoveAt(localPlayerPositions.Count - 1);
                localPlayerPositions.Insert(0, new Tuple<Vector3, DateTime>(PlayerControl.LocalPlayer.transform.position, DateTime.UtcNow));
            }
        }

        static PlayerControl setTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null) {
            PlayerControl result = null;
            float num = GameOptionsData.DCAJHEGBLDD[Mathf.Clamp(PlayerControl.GameOptions.OCPGKHJJAHL, 0, 2)];
            if (!ShipStatus.Instance) return result;

            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<GameData.LGBOMGHJELL> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.LGBOMGHJELL LGBOMGHJELL = allPlayers[i];
                if (!LGBOMGHJELL.MFFAGDHDHLO && LGBOMGHJELL.FNPNJHNKEBK != PlayerControl.LocalPlayer.PlayerId && !LGBOMGHJELL.IAGJEKLJCCI && (!onlyCrewmates || !LGBOMGHJELL.FDNMBJOAPFL))
                {
                    PlayerControl @object = LGBOMGHJELL.GJPBCGFPMOD;
                    if(untargetablePlayers != null && untargetablePlayers.Any(x => x == @object)) {
                        // if that player is not targetable: skip check
                        continue;
                    }

                    if (@object && (!@object.inVent || targetPlayersInVents))
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.HLIEDNLNBBH(truePosition, vector.normalized, magnitude, Constants.LEOCDMEJGPA))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }

        static void medicSetTarget() {
            if (Medic.medic == null || Medic.medic != PlayerControl.LocalPlayer) return;
            Medic.currentTarget = setTarget();
        }

        static void shifterSetTarget() {
            if (Shifter.shifter == null || Shifter.shifter != PlayerControl.LocalPlayer) return;
            Shifter.currentTarget = setTarget();
        }

        
        static void morphlingSetTarget() {
            if (Morphling.morphling == null || Morphling.morphling != PlayerControl.LocalPlayer) return;
            Morphling.currentTarget = setTarget();
        }
        
        static void sheriffSetTarget() {
            if (Sheriff.sheriff == null || Sheriff.sheriff != PlayerControl.LocalPlayer) return;
            Sheriff.currentTarget = setTarget();
        }

        static void trackerSetTarget() {
            if (Tracker.tracker == null || Tracker.tracker != PlayerControl.LocalPlayer) return;
            Tracker.currentTarget = setTarget();
        }

        static void detectiveUpdateFootPrints() {            
            if (Detective.detective == null || Detective.detective != PlayerControl.LocalPlayer) return;

            Detective.timer -= Time.fixedDeltaTime;
            if (Detective.timer <= 0f) {
                Detective.timer = Detective.footprintIntervall;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                    if (player != null && player != PlayerControl.LocalPlayer && !player.PPMOEEPBHJO.IAGJEKLJCCI && !player.inVent) {
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
        }

        static void sidekickSetTarget() {
            if (Sidekick.sidekick == null || Sidekick.sidekick != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if(Jackal.jackal != null) untargetablePlayers.Add(Jackal.jackal);
            if(Child.child != null && !Child.isGrownUp()) untargetablePlayers.Add(Child.child); // Exclude Sidekick from targeting the Child unless it has grown up
            Sidekick.currentTarget = setTarget(untargetablePlayers : untargetablePlayers);
        }

        static void eraserSetTarget() {
            if (Eraser.eraser == null || Eraser.eraser != PlayerControl.LocalPlayer) return;

            List<PlayerControl> untargatables = new List<PlayerControl>();
            if (Spy.spy != null) untargatables.Add(Spy.spy);
            Eraser.currentTarget = setTarget(onlyCrewmates: !Eraser.canEraseAnyone, untargetablePlayers: Eraser.canEraseAnyone ? new List<PlayerControl>() : untargatables);
        }

        static void engineerUpdate() {
            if (PlayerControl.LocalPlayer.PPMOEEPBHJO.FDNMBJOAPFL && ShipStatus.Instance?.GJHKPDGJHJN != null) {
                foreach (Vent vent in ShipStatus.Instance.GJHKPDGJHJN) {
                    try {
                        if (vent?.KJAENOGGEOK?.material != null) {
                            if (Engineer.engineer != null && Engineer.engineer.inVent) {
                                vent.KJAENOGGEOK.material.SetFloat("_Outline", 1f);
                                vent.KJAENOGGEOK.material.SetColor("_OutlineColor", Engineer.color);
                            } else if (vent.KJAENOGGEOK.material.GetColor("_AddColor") != Color.red) {
                                vent.KJAENOGGEOK.material.SetFloat("_Outline", 0);
                            }
                        }
                    } catch {}
                }
            }
        }

        static void impostorSetTarget() {
            if (!PlayerControl.LocalPlayer.PPMOEEPBHJO.FDNMBJOAPFL ||!PlayerControl.LocalPlayer.POECPOEKKNO || PlayerControl.LocalPlayer.PPMOEEPBHJO.IAGJEKLJCCI) { // !isImpostor || !canMove || isDead
                HudManager.CHNDKKBEIDG.KillButton.SetTarget(null);
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

            HudManager.CHNDKKBEIDG.KillButton.SetTarget(target);
        }

        static void trackerUpdate() {
            if (Tracker.arrow?.arrow == null) return;

            if (Tracker.tracker == null || PlayerControl.LocalPlayer != Tracker.tracker) {
                Tracker.arrow.arrow.SetActive(false);
                return;
            }

            if (Tracker.tracker != null && Tracker.tracked != null && PlayerControl.LocalPlayer == Tracker.tracker && !Tracker.tracker.PPMOEEPBHJO.IAGJEKLJCCI) {
                Tracker.timeUntilUpdate -= Time.fixedDeltaTime;

                if (Tracker.timeUntilUpdate <= 0f) {
                    bool trackedOnMap = !Tracker.tracked.PPMOEEPBHJO.IAGJEKLJCCI;
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

        public static void Prefix(PlayerControl __instance) {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GCDONLGCMIL.Started) return;
        }

        public static void Postfix(PlayerControl __instance) {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GCDONLGCMIL.Started) return;

            // Update Role Description
            Helpers.refreshRoleDescription(__instance);
            // Child and Morphling shrink
            playerSizeUpdate(__instance);
            
            if (PlayerControl.LocalPlayer == __instance) {
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
            } 
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.WalkPlayerTo))]
    class PlayerPhysicsWalkPlayerToPatch {
        private static Vector2 offset = Vector2.zero;
        public static void Prefix(PlayerPhysics __instance) {
            bool correctOffset = Camouflager.camouflageTimer <= 0f && (__instance.EMHCBDEIOLO == Child.child ||  (Morphling.morphling != null && __instance.EMHCBDEIOLO == Morphling.morphling && Morphling.morphTarget == Child.child && Morphling.morphTimer > 0f));
            if (correctOffset) {
                float currentScaling = (Child.growingProgress() + 1) * 0.5f;
                __instance.EMHCBDEIOLO.Collider.offset = currentScaling * Child.defaultColliderOffset * Vector2.down;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    class PlayerControlCmdReportDeadBodyPatch {
        public static void Prefix(PlayerControl __instance) {
            // Murder the bitten player before the meeting starts or reset the bitten player
            if (Vampire.bitten != null && !Vampire.bitten.PPMOEEPBHJO.IAGJEKLJCCI && Helpers.handleMurderAttempt(Vampire.bitten, true)) {
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
    [HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill))]
    class PerformKillPatch
    {
        public static bool Prefix(KillButtonManager __instance) {
            return Helpers.handleMurderAttempt(__instance.CurrentTarget);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.LocalPlayer.CmdReportDeadBody))]
    class BodyReportPatch
    {
        static void Postfix(PlayerControl __instance, GameData.LGBOMGHJELL DGDGDKCCKHJ)
        {
            // Medic or Detective report
            bool isMedicReport = Medic.medic != null && Medic.medic == PlayerControl.LocalPlayer && __instance.PlayerId == Medic.medic.PlayerId;
            bool isDetectiveReport = Detective.detective != null && Detective.detective == PlayerControl.LocalPlayer && __instance.PlayerId == Detective.detective.PlayerId;
            if (isMedicReport || isDetectiveReport)
            {
                DeadPlayer deadPlayer = deadPlayers?.Where(x => x.player?.PlayerId == DGDGDKCCKHJ?.FNPNJHNKEBK)?.FirstOrDefault();

                if (deadPlayer != null && deadPlayer.killerIfExisting != null) {
                    float timeSinceDeath = ((float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds);
                    string msg = "";

                    if (isMedicReport) {
                        msg = $"Body Report: Killed {Math.Round(timeSinceDeath / 1000)}s ago!";
                    } else if (isDetectiveReport) {
                        if (timeSinceDeath < Detective.reportNameDuration * 1000) {
                            msg =  $"Body Report: The killer appears to be {deadPlayer.killerIfExisting.name}!";
                        } else if (timeSinceDeath < Detective.reportColorDuration * 1000) {
                            var typeOfColor = Helpers.isLighterColor(deadPlayer.killerIfExisting.PPMOEEPBHJO.IMMNCAGJJJC) ? "lighter" : "darker";
                            msg =  $"Body Report: The killer appears to be a {typeOfColor} color!";
                        } else {
                            msg = $"Body Report: The corpse is too old to gain information from!";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(msg))
                    {   
                        if (AmongUsClient.Instance.BPADAHAOBLM && DestroyableSingleton<HudManager>.CHNDKKBEIDG)
                        {
                            DestroyableSingleton<HudManager>.CHNDKKBEIDG.Chat.AddChat(PlayerControl.LocalPlayer, msg);
                        }
                        if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            DestroyableSingleton<Assets.CoreScripts.Telemetry>.CHNDKKBEIDG.SendWho();
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

        public static void Prefix(PlayerControl __instance, PlayerControl DGDGDKCCKHJ)
        {
            // Allow everyone to murder players
            resetToCrewmate = !__instance.PPMOEEPBHJO.FDNMBJOAPFL;
            resetToDead = __instance.PPMOEEPBHJO.IAGJEKLJCCI;
            __instance.PPMOEEPBHJO.FDNMBJOAPFL = true;
            __instance.PPMOEEPBHJO.IAGJEKLJCCI = false;
        }

        public static void Postfix(PlayerControl __instance, PlayerControl DGDGDKCCKHJ)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(DGDGDKCCKHJ, DateTime.UtcNow, DeathReason.Kill, __instance);
            GameHistory.deadPlayers.Add(deadPlayer);

            // Reset killer to crewmate if resetToCrewmate
            if (resetToCrewmate) __instance.PPMOEEPBHJO.FDNMBJOAPFL = false;
            if (resetToDead) __instance.PPMOEEPBHJO.IAGJEKLJCCI = true;

            // Lover suicide trigger on murder
            if ((Lovers.lover1 != null && DGDGDKCCKHJ == Lovers.lover1) || (Lovers.lover2 != null && DGDGDKCCKHJ == Lovers.lover2)) {
                PlayerControl otherLover = DGDGDKCCKHJ == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                if (PlayerControl.LocalPlayer == DGDGDKCCKHJ && otherLover != null && !otherLover.PPMOEEPBHJO.IAGJEKLJCCI && Lovers.bothDie) { // Only the dead lover sends the rpc
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LoverSuicide, Hazel.SendOption.Reliable, -1);
                    writer.Write(otherLover.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.loverSuicide(otherLover.PlayerId);
                }
            }
            
            // Sidekick promotion trigger on murder
            if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.PPMOEEPBHJO.IAGJEKLJCCI && DGDGDKCCKHJ == Jackal.jackal && Jackal.jackal == PlayerControl.LocalPlayer) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.sidekickPromotes();
            }

            // Seer show flash and add dead player position
            if (Seer.seer != null && PlayerControl.LocalPlayer == Seer.seer && !Seer.seer.PPMOEEPBHJO.IAGJEKLJCCI && Seer.seer != DGDGDKCCKHJ && Seer.mode <= 1) {
                HudManager.CHNDKKBEIDG.FullScreen.enabled = true;
                HudManager.CHNDKKBEIDG.StartCoroutine(Effects.DCHLMIDMBHG(1f, new Action<float>((p) => {
                    var renderer = HudManager.CHNDKKBEIDG.FullScreen;
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
            if (Seer.deadBodyPositions != null) Seer.deadBodyPositions.Add(DGDGDKCCKHJ.transform.position);

            // Child set adapted kill cooldown
            if (Child.child != null && PlayerControl.LocalPlayer == Child.child && Child.child.PPMOEEPBHJO.FDNMBJOAPFL) {
                var multiplier = Child.isGrownUp() ? 0.66f : 2f;
                Child.child.SetKillTimer(PlayerControl.GameOptions.DGOPNLEEAAJ * multiplier);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    class PlayerControlSetCoolDownPatch {
        public static bool Prefix(PlayerControl __instance, float IHIOFAHBJKK) {
            if (PlayerControl.GameOptions.DGOPNLEEAAJ <= 0f) return false;
            float multiplier = 1f;
            if (Child.child != null && PlayerControl.LocalPlayer == Child.child && Child.child.PPMOEEPBHJO.FDNMBJOAPFL) multiplier = Child.isGrownUp() ? 0.66f : 2f;

            __instance.killTimer = Mathf.Clamp(IHIOFAHBJKK, 0f, PlayerControl.GameOptions.DGOPNLEEAAJ * multiplier);
            DestroyableSingleton<HudManager>.CHNDKKBEIDG.KillButton.SetCoolDown(__instance.killTimer, PlayerControl.GameOptions.DGOPNLEEAAJ * multiplier);
            return false;
        }
    }

    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.CoPerformKill))]
    class KillAnimationCoPerformKillPatch {
        public static void Prefix(KillAnimation __instance, ref PlayerControl KIJHPICDEAD, ref PlayerControl DGDGDKCCKHJ) {
            if (Vampire.vampire != null && Vampire.vampire == KIJHPICDEAD && Vampire.bitten != null && Vampire.bitten == DGDGDKCCKHJ)
                KIJHPICDEAD = DGDGDKCCKHJ;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class ExilePlayerPatch
    {
        public static void Prefix(PlayerControl __instance) {
            // Child exile lose condition
            if (Child.child != null && Child.child == __instance && !Child.isGrownUp() && !Child.child.PPMOEEPBHJO.FDNMBJOAPFL) {
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

            // Lover suicide trigger on exile
            if ((Lovers.lover1 != null && __instance == Lovers.lover1) || (Lovers.lover2 != null && __instance == Lovers.lover2)) {
                PlayerControl otherLover = __instance == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                if (otherLover != null && !otherLover.PPMOEEPBHJO.IAGJEKLJCCI && Lovers.bothDie)
                    otherLover.Exiled();
            }
            
            // Sidekick promotion trigger on exile
            if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.PPMOEEPBHJO.IAGJEKLJCCI && __instance == Jackal.jackal && Jackal.jackal == PlayerControl.LocalPlayer) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.sidekickPromotes();
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.POECPOEKKNO), MethodType.Getter)]
    class PlayerControlCanMovePatch {
        public static bool Prefix(PlayerControl __instance, ref bool __result)
        {
            __result = __instance.moveable &&
                !Minigame.Instance &&
                (!DestroyableSingleton<HudManager>.BMHJGNNOGDM || (!DestroyableSingleton<HudManager>.CHNDKKBEIDG.Chat.ENPNFFCCKON && !DestroyableSingleton<HudManager>.CHNDKKBEIDG.KillOverlay.ENPNFFCCKON && !DestroyableSingleton<HudManager>.CHNDKKBEIDG.GameMenu.ENPNFFCCKON)) &&
                (!MapBehaviour.Instance || !MapBehaviour.Instance.FLLAHBDIMHD) &&
                !MeetingHud.Instance &&
                !CustomPlayerMenu.Instance &&
                !ExileController.Instance &&
                !IntroCutscene.Instance;
            return false;
        }
    }
}
