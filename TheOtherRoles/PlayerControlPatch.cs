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
    [HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill))]
    class PerformKillPatch {
        public static bool Prefix(KillButtonManager __instance) {
            if (__instance.isActiveAndEnabled && __instance.CurrentTarget && !__instance.isCoolingDown && !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.CanMove) { // Among Us default checks
                if (Helpers.handleMurderAttempt(__instance.CurrentTarget)) { // Custom checks
                    if (Child.child != null && PlayerControl.LocalPlayer == Child.child) { // Not checked by official servers
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        writer.Write(__instance.CurrentTarget.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.uncheckedMurderPlayer(PlayerControl.LocalPlayer.PlayerId, __instance.CurrentTarget.PlayerId);
                    } else { // Checked by official servers
                        PlayerControl.LocalPlayer.RpcMurderPlayer(__instance.CurrentTarget);
                    }
                    __instance.SetTarget(null);
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
