using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BonusRoles.BonusRoles;
using static BonusRoles.GameHistory;
using UnityEngine;

namespace BonusRoles {
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
                
                    // Try reviving LOCAL player 
                    if (TimeMaster.reviveDuringRewind && PlayerControl.LocalPlayer.Data.IsDead) {
                        DeadPlayer deadPlayer = deadPlayers.Where(x => x.player == PlayerControl.LocalPlayer).FirstOrDefault();
                        if (deadPlayer != null && next.Item2 < deadPlayer.timeOfDeath) {
                            MessageWriter write = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte) CustomRPC.TimeMasterRevive, SendOption.None, -1);
                            write.Write(PlayerControl.LocalPlayer.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(write);
                            RPCProcedure.timeMasterRevive(PlayerControl.LocalPlayer.PlayerId);
                        }
                    }
                } else {
                    TimeMaster.isRewinding = false;
                    PlayerControl.LocalPlayer.moveable = true;
                    HudManager.Instance.FullScreen.enabled = false;
                }
            } else {
                while (localPlayerPositions.Count >= Mathf.Round(TimeMaster.rewindTime / Time.fixedDeltaTime)) localPlayerPositions.RemoveAt(localPlayerPositions.Count - 1);
                localPlayerPositions.Insert(0, new Tuple<Vector3, DateTime>(PlayerControl.LocalPlayer.transform.position, DateTime.UtcNow));
            }
        }

        static PlayerControl setAnyRoleTarget() {
            PlayerControl result = null;
            float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            if (!ShipStatus.Instance) return result;

            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];
                if (!playerInfo.Disconnected && playerInfo.PlayerId != PlayerControl.LocalPlayer.PlayerId && !playerInfo.IsDead)
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object && !@object.inVent)
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

        static void medicSetTarget() {
            if (Medic.medic == null || Medic.medic != PlayerControl.LocalPlayer) return;
            Medic.currentTarget = setAnyRoleTarget();
        }

        static void shifterSetTarget() {
            if (Shifter.shifter == null || Shifter.shifter != PlayerControl.LocalPlayer) return;
            Shifter.currentTarget = setAnyRoleTarget();
        }

        
        static void morphlingSetTarget() {
            if (Morphling.morphling == null || Morphling.morphling != PlayerControl.LocalPlayer) return;
            Morphling.currentTarget = setAnyRoleTarget();
        }
        
        static void sheriffSetTarget() {
            if (Sheriff.sheriff == null || Sheriff.sheriff != PlayerControl.LocalPlayer) return;
            Sheriff.currentTarget = setAnyRoleTarget();
        }

        static void seerSetTarget() {
            if (Seer.seer == null || Seer.seer != PlayerControl.LocalPlayer) return;
            Seer.currentTarget = setAnyRoleTarget();
            if (Seer.currentTarget != null && Seer.revealedPlayers.Keys.Any(p => p.Data.PlayerId == Seer.currentTarget.Data.PlayerId)) Seer.currentTarget = null; // Remove target if already revealed

        }

        static void detectiveSetFootPrints() {
            if (Detective.detective == null || Detective.detective != PlayerControl.LocalPlayer) return;

            Detective.timer -= Time.fixedDeltaTime;
            if (Detective.timer <= 0f) {
                Detective.timer = Detective.footprintIntervall;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                    if (player != null && player != PlayerControl.LocalPlayer && !player.Data.IsDead) {
                        new Footprint(Detective.footprintDuration, Detective.anonymousFootprints, player);
                    }
                }
            }

        }

        public static void Postfix(PlayerControl __instance) {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            
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
                // Seer
                seerSetTarget();
                // Detective
                detectiveSetFootPrints();
            } 
        }
    }

    [HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill))]
    class PerformKillPatch
    {
        public static bool Prefix(KillButtonManager __instance) {
            // Block impostor shielded kill
            if (Medic.shielded != null && Medic.shielded == __instance.CurrentTarget) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.None, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shieldedMurderAttempt();
                return false;
            }
            // Block impostor not fully grown child kill
            else if (Child.child != null && __instance.CurrentTarget == Child.child && !Child.isGrownUp()) {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.LocalPlayer.CmdReportDeadBody))]
    class BodyReportPatch
    {
        static void Postfix(PlayerControl __instance, GameData.PlayerInfo PAIBDFDMIGK)
        {
            // Medic report
            if (Medic.medic != null && Medic.medic == PlayerControl.LocalPlayer && __instance.PlayerId == Medic.medic.PlayerId)
            {
                DeadPlayer deadPlayer = deadPlayers?.Where(x => x.player?.PlayerId == PAIBDFDMIGK?.PlayerId)?.FirstOrDefault();

                if (deadPlayer != null && deadPlayer.killerIfExisting != null) {
                    float timeSinceDeath = ((float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds);
                    string msg = "";

                    if (timeSinceDeath < Medic.reportNameDuration * 1000) {
                        msg =  $"Body Report: The killer appears to be {deadPlayer.killerIfExisting.name}! (Killed {Math.Round(timeSinceDeath / 1000)}s ago)";
                    } else if (timeSinceDeath < Medic.reportColorDuration * 1000) {
                        var colors = new Dictionary<byte, string>() {
                            {0, "darker"},
                            {1, "darker"},
                            {2, "darker"},
                            {3, "lighter"},
                            {4, "lighter"},
                            {5, "lighter"},
                            {6, "darker"},
                            {7, "lighter"},
                            {8, "darker"},
                            {9, "darker"},
                            {10, "lighter"},
                            {11, "lighter"},
                        };
                        var typeOfColor = colors[deadPlayer.killerIfExisting.Data.ColorId] ?? "unknown";
                        msg =  $"Body Report: The killer appears to be a {typeOfColor} color. (Killed {Math.Round(timeSinceDeath / 1000)}s ago)";
                    } else {
                        msg = $"Body Report: The corpse is too old to gain information from. (Killed {Math.Round(timeSinceDeath / 1000)}s ago)";
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
        public static void Prefix(PlayerControl __instance, PlayerControl PAIBDFDMIGK, out bool __state)
        {
            // __state denotes if the player performing the murder needs to be reset to crewmate
            __state = false;

            // Allow Sheriff to kill "as crewmate" by assigning the impostor role
            if (Sheriff.sheriff != null && __instance == Sheriff.sheriff) {
                __instance.Data.IsImpostor = true;
                __state = true;
            }
            // Allow Shifter to suicide
            else if (Shifter.shifter != null && __instance == Shifter.shifter) {
                __instance.Data.IsImpostor = true;
                __state = true;
            }
            // Allow Lover which is no impostor to suicide
            else if ((Lovers.lover1 != null && __instance == Lovers.lover1) || (Lovers.lover2 != null && __instance == Lovers.lover2)) {
                if (!__instance.Data.IsImpostor) {
                    __instance.Data.IsImpostor = true;
                    __state = true;
                }
            }
        }

        public static void Postfix(PlayerControl __instance, PlayerControl PAIBDFDMIGK, bool __state)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(PAIBDFDMIGK, DateTime.UtcNow, DeathReason.Kill, __instance);
            GameHistory.deadPlayers.Add(deadPlayer);

            // Reset killer to crewmate if __state
            if (__state) __instance.Data.IsImpostor = false;

            // Lover suicide trigger on murder
            if ((Lovers.lover1 != null && PAIBDFDMIGK == Lovers.lover1) || (Lovers.lover2 != null && PAIBDFDMIGK == Lovers.lover2)) {
                PlayerControl otherLover = PAIBDFDMIGK == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                if (PlayerControl.LocalPlayer == PAIBDFDMIGK && otherLover != null && !otherLover.Data.IsDead && Lovers.bothDie) { // Only the dead lover sends the rpc
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LoverSuicide, Hazel.SendOption.None, -1);
                    writer.Write(otherLover.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.loverSuicide(otherLover.PlayerId);
                }
            }
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

            // Lover suicide trigger on exile
            if ((Lovers.lover1 != null && __instance == Lovers.lover1) || (Lovers.lover2 != null && __instance == Lovers.lover2)) {
                PlayerControl otherLover = __instance == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                if (PlayerControl.LocalPlayer == __instance && otherLover != null && !otherLover.Data.IsDead && Lovers.bothDie) { // Only the dead lover sends the rpc
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LoverSuicide, Hazel.SendOption.None, -1);
                    writer.Write(otherLover.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.loverSuicide(otherLover.PlayerId);
                }
            }

        }
    }
}
