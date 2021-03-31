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
                    HudManager.Instance.FullScreen.enabled = false;
                }
            } else {
                while (localPlayerPositions.Count >= Mathf.Round(TimeMaster.rewindTime / Time.fixedDeltaTime)) localPlayerPositions.RemoveAt(localPlayerPositions.Count - 1);
                localPlayerPositions.Insert(0, new Tuple<Vector3, DateTime>(PlayerControl.LocalPlayer.transform.position, DateTime.UtcNow));
            }
        }

        static PlayerControl setTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null) {
            PlayerControl result = null;
            float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            if (!ShipStatus.Instance) return result;

            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];
                if (!playerInfo.Disconnected && playerInfo.PlayerId != PlayerControl.LocalPlayer.PlayerId && !playerInfo.IsDead && (!onlyCrewmates || !playerInfo.IsImpostor))
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

        static void seerSetTarget() {
            if (Seer.seer == null || Seer.seer != PlayerControl.LocalPlayer) return;
            Seer.currentTarget = setTarget();
            if (Seer.currentTarget != null && Seer.revealedPlayers.Keys.Any(p => p.Data.PlayerId == Seer.currentTarget.Data.PlayerId)) Seer.currentTarget = null; // Remove target if already revealed
        }

        static void trackerSetTarget() {
            if (Tracker.tracker == null || Tracker.tracker != PlayerControl.LocalPlayer) return;
            Tracker.currentTarget = setTarget();
        }

        static void detectiveSetFootPrints() {
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
		    PlayerControl target = setTarget(true, true);
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

        static void engineerUpdate() {
            if (PlayerControl.LocalPlayer.Data.IsImpostor) {
                foreach (Vent vent in ShipStatus.Instance.AllVents) {
                    if (vent.Field_7?.material != null) {
                        if (Engineer.engineer != null && Engineer.engineer.inVent) {
                            vent.Field_7.material.SetFloat("_Outline", 1f);
                            vent.Field_7.material.SetColor("_OutlineColor", Engineer.color);
                        } else if (vent.Field_7.material.GetColor("_AddColor") != Color.red) {
                            vent.Field_7.material.SetFloat("_Outline", 0);
                        }
                    }
                }
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
                // Tracker
                trackerSetTarget();
                // Vampire
                vampireSetTarget();
                Garlic.UpdateAll();
                // Engineer
                engineerUpdate();
                // Tracker
                trackerUpdate();
                // Jackal
                jackalSetTarget();
                // Sidekick
                sidekickSetTarget();
            } 
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.OpenMeetingRoom))]
    class StartMeetingHostPatch {
        public static void Prefix(PlayerControl __instance) {
            // Perform vampire bite kill before the meeting starts for HOST
            if (!MeetingHud.Instance && AmongUsClient.Instance.AmHost)
                RPCProcedure.vampireTryKill();
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CoStartMeeting))]
    class StartMeetingClientPatch {
        public static void Prefix(PlayerControl __instance) {
            // Perform vampire bite kill before the meeting starts for CLIENTS
            if (AmongUsClient.Instance.AmClient)
            RPCProcedure.vampireTryKill();
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
        public static bool resetToCrewmate = false;
        public static bool resetToDead = false;

        public static void Prefix(PlayerControl __instance, PlayerControl PAIBDFDMIGK)
        {
            // Allow everyone to murder players
            resetToCrewmate = !__instance.Data.IsImpostor;
            resetToDead = __instance.Data.IsDead;
            __instance.Data.IsImpostor = true;
            __instance.Data.IsDead = false;
        }

        public static void Postfix(PlayerControl __instance, PlayerControl PAIBDFDMIGK)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(PAIBDFDMIGK, DateTime.UtcNow, DeathReason.Kill, __instance);
            GameHistory.deadPlayers.Add(deadPlayer);

            // Reset killer to crewmate if resetToCrewmate
            if (resetToCrewmate) __instance.Data.IsImpostor = false;
            if (resetToDead) __instance.Data.IsDead = true;

            // Lover suicide trigger on murder
            if ((Lovers.lover1 != null && PAIBDFDMIGK == Lovers.lover1) || (Lovers.lover2 != null && PAIBDFDMIGK == Lovers.lover2)) {
                PlayerControl otherLover = PAIBDFDMIGK == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                if (PlayerControl.LocalPlayer == PAIBDFDMIGK && otherLover != null && !otherLover.Data.IsDead && Lovers.bothDie) { // Only the dead lover sends the rpc
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LoverSuicide, Hazel.SendOption.Reliable, -1);
                    writer.Write(otherLover.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.loverSuicide(otherLover.PlayerId);
                }
            }
            
            // Sidekick promotion trigger on murder
            if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead && PAIBDFDMIGK == Jackal.jackal) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.sidekickPromotes();
            }
        }
    }

    [HarmonyPatch(typeof(KillAnimation),nameof(KillAnimation.CoPerformKill))]
    class Test {
        public static void Prefix(KillAnimation __instance, ref PlayerControl CPKODPCJPOO, ref PlayerControl PAIBDFDMIGK) {
            if (Vampire.vampire != null && Vampire.vampire == CPKODPCJPOO && Vampire.bitten != null && Vampire.bitten == PAIBDFDMIGK)
                CPKODPCJPOO = PAIBDFDMIGK;
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
                if (otherLover != null && !otherLover.Data.IsDead && Lovers.bothDie)
                    otherLover.Exiled();
            }
            
            // Sidekick promotion trigger on exile
            if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead && __instance == Jackal.jackal) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.sidekickPromotes();
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetTasks))]
    public static class Role
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (PlayerControl.LocalPlayer == null) return;

            // Remove default ImportantTextTasks
            var toRemove = new List<PlayerTask>();
            foreach (PlayerTask t in __instance.myTasks) {
                if (t.gameObject.GetComponent<ImportantTextTask>() != null) {
                    toRemove.Add(t);
                }
            }   
            foreach (PlayerTask t in toRemove)
                __instance.RemoveTask(t);

            // Add description
            RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(__instance);        
            var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
            task.transform.SetParent(__instance.transform, false);

            if (__instance == Jackal.jackal) {
                var getSidekickText = Jackal.canCreateSidekick ? " and recruit a Sidekick" : "";
                task.Text = $"{roleInfo.colorHexString()}{roleInfo.name}: Kill everyone{getSidekickText}";  
            } else {
                task.Text = $"{roleInfo.colorHexString()}{roleInfo.name}: {roleInfo.shortDescription}";  
            }

            __instance.myTasks.Insert(0, task);
        }
    }
}
