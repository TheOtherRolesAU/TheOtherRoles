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

using SystemTypes = LGBKLKNAINN;
using Palette = GLNPIJPGGNJ;
using Constants = NFONDPLFBCP;
using PhysicsHelpers = IEPBCHBGDOA;
using DeathReason = KAPJFCMEBJE;
using GameOptionsData = IGDMNKLDEPI;
using Effects = HLPCBNMDEHF;

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
            float num = GameOptionsData.FECFGOOCIJL[Mathf.Clamp(PlayerControl.GameOptions.MLLMFMOMIAC, 0, 2)];
            if (!ShipStatus.Instance) return result;

            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<GameData.OFKOJOKOOAK> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.OFKOJOKOOAK OFKOJOKOOAK = allPlayers[i];
                if (!OFKOJOKOOAK.GBPMEHJFECK && OFKOJOKOOAK.GMBAIPNOKLP != PlayerControl.LocalPlayer.PlayerId && !OFKOJOKOOAK.FGNJJFABIHJ && (!onlyCrewmates || !OFKOJOKOOAK.CIDDOFDJHJH))
                {
                    PlayerControl @object = OFKOJOKOOAK.GPBBCHGPABL;
                    if(untargetablePlayers != null && untargetablePlayers.Any(x => x == @object)) {
                        // if that player is not targetable: skip check
                        continue;
                    }

                    if (@object && (!@object.inVent || targetPlayersInVents))
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.HKFKKEKGLHF(truePosition, vector.normalized, magnitude, Constants.DHLPLBPJNBA))
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
                    if (player != null && player != PlayerControl.LocalPlayer && !player.IDOFAMCIJKE.FGNJJFABIHJ && !player.inVent) {
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
            Eraser.currentTarget = setTarget();
        }

        static void engineerUpdate() {
            if (PlayerControl.LocalPlayer.IDOFAMCIJKE.CIDDOFDJHJH && ShipStatus.Instance?.GIDPCPOEFBC != null) {
                foreach (Vent vent in ShipStatus.Instance.GIDPCPOEFBC) {
                    try {
                        if (vent?.LNMJKMLHMIM?.material != null) {
                            if (Engineer.engineer != null && Engineer.engineer.inVent) {
                                vent.LNMJKMLHMIM.material.SetFloat("_Outline", 1f);
                                vent.LNMJKMLHMIM.material.SetColor("_OutlineColor", Engineer.color);
                            } else if (vent.LNMJKMLHMIM.material.GetColor("_AddColor") != Color.red) {
                                vent.LNMJKMLHMIM.material.SetFloat("_Outline", 0);
                            }
                        }
                    } catch {}
                }
            }
        }

        static void impostorSetTarget() {
            if (!PlayerControl.LocalPlayer.IDOFAMCIJKE.CIDDOFDJHJH ||!PlayerControl.LocalPlayer.AMDJMEEHNIG || PlayerControl.LocalPlayer.IDOFAMCIJKE.FGNJJFABIHJ) { // !isImpostor || !canMove || isDead
                HudManager.CMJOLNCMAPD.KillButton.SetTarget(null);
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

            HudManager.CMJOLNCMAPD.KillButton.SetTarget(target);
        }

        static void trackerUpdate() {
            if (Tracker.arrow?.arrow == null) return;

            if (Tracker.tracker == null || PlayerControl.LocalPlayer != Tracker.tracker) {
                Tracker.arrow.arrow.SetActive(false);
                return;
            }

            if (Tracker.tracker != null && Tracker.tracked != null && PlayerControl.LocalPlayer == Tracker.tracker && !Tracker.tracker.IDOFAMCIJKE.FGNJJFABIHJ) {
                Tracker.timeUntilUpdate -= Time.fixedDeltaTime;

                if (Tracker.timeUntilUpdate <= 0f) {
                    bool trackedOnMap = !Tracker.tracked.IDOFAMCIJKE.FGNJJFABIHJ;
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
            if (Child.child == null  || Camouflager.camouflageTimer > 0f) return;

            float growingProgress = Child.growingProgress();
            float scale = growingProgress * 0.35f + 0.35f;
            
            if (p == Child.child)
                Child.child.transform.localScale = new Vector3(scale, scale, 1f);
            if (Morphling.morphling != null && p == Morphling.morphling && Morphling.morphTarget == Child.child && Morphling.morphTimer > 0f)
                p.transform.localScale = new Vector3(scale, scale, 1f);
        }

        public static void Prefix(PlayerControl __instance) {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.CJDCOJJNIGL.Started) return;

            // Reset player sizes
            __instance.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        }

        public static void Postfix(PlayerControl __instance) {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.CJDCOJJNIGL.Started) return;

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

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    class PlayerControlCmdReportDeadBodyPatch {
        public static void Prefix(PlayerControl __instance) {
            // Murder the bitten player before the meeting starts or reset the bitten player
            if (Vampire.bitten != null && !Vampire.bitten.IDOFAMCIJKE.FGNJJFABIHJ && Helpers.handleMurderAttempt(Vampire.bitten, true)) {
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
        static void Postfix(PlayerControl __instance, GameData.OFKOJOKOOAK IGLDJOKKFJE)
        {
            // Medic or Detective report
            bool isMedicReport = Medic.medic != null && Medic.medic == PlayerControl.LocalPlayer && __instance.PlayerId == Medic.medic.PlayerId;
            bool isDetectiveReport = Detective.detective != null && Detective.detective == PlayerControl.LocalPlayer && __instance.PlayerId == Detective.detective.PlayerId;
            if (isMedicReport || isDetectiveReport)
            {
                DeadPlayer deadPlayer = deadPlayers?.Where(x => x.player?.PlayerId == IGLDJOKKFJE?.GMBAIPNOKLP)?.FirstOrDefault();

                if (deadPlayer != null && deadPlayer.killerIfExisting != null) {
                    float timeSinceDeath = ((float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds);
                    string msg = "";

                    if (isMedicReport) {
                        msg = $"Body Report: Killed {Math.Round(timeSinceDeath / 1000)}s ago!";
                    } else if (isDetectiveReport) {
                        if (timeSinceDeath < Detective.reportNameDuration * 1000) {
                            msg =  $"Body Report: The killer appears to be {deadPlayer.killerIfExisting.name}!";
                        } else if (timeSinceDeath < Detective.reportColorDuration * 1000) {
                            var typeOfColor = Helpers.isLighterColor(deadPlayer.killerIfExisting.IDOFAMCIJKE.JFHFMIKFHGG) ? "lighter" : "darker";
                            msg =  $"Body Report: The killer appears to be a {typeOfColor} color!";
                        } else {
                            msg = $"Body Report: The corpse is too old to gain information from!";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(msg))
                    {   
                        if (AmongUsClient.Instance.HNMILJEOEKN && DestroyableSingleton<HudManager>.CMJOLNCMAPD)
                        {
                            DestroyableSingleton<HudManager>.CMJOLNCMAPD.Chat.AddChat(PlayerControl.LocalPlayer, msg);
                        }
                        if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            DestroyableSingleton<Assets.CoreScripts.Telemetry>.CMJOLNCMAPD.SendWho();
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

        public static void Prefix(PlayerControl __instance, PlayerControl IGLDJOKKFJE)
        {
            // Allow everyone to murder players
            resetToCrewmate = !__instance.IDOFAMCIJKE.CIDDOFDJHJH;
            resetToDead = __instance.IDOFAMCIJKE.FGNJJFABIHJ;
            __instance.IDOFAMCIJKE.CIDDOFDJHJH = true;
            __instance.IDOFAMCIJKE.FGNJJFABIHJ = false;
        }

        public static void Postfix(PlayerControl __instance, PlayerControl IGLDJOKKFJE)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(IGLDJOKKFJE, DateTime.UtcNow, DeathReason.Kill, __instance);
            GameHistory.deadPlayers.Add(deadPlayer);

            // Reset killer to crewmate if resetToCrewmate
            if (resetToCrewmate) __instance.IDOFAMCIJKE.CIDDOFDJHJH = false;
            if (resetToDead) __instance.IDOFAMCIJKE.FGNJJFABIHJ = true;

            // Lover suicide trigger on murder
            if ((Lovers.lover1 != null && IGLDJOKKFJE == Lovers.lover1) || (Lovers.lover2 != null && IGLDJOKKFJE == Lovers.lover2)) {
                PlayerControl otherLover = IGLDJOKKFJE == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                if (PlayerControl.LocalPlayer == IGLDJOKKFJE && otherLover != null && !otherLover.IDOFAMCIJKE.FGNJJFABIHJ && Lovers.bothDie) { // Only the dead lover sends the rpc
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.LoverSuicide, Hazel.SendOption.Reliable, -1);
                    writer.Write(otherLover.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.loverSuicide(otherLover.PlayerId);
                }
            }
            
            // Sidekick promotion trigger on murder
            if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.IDOFAMCIJKE.FGNJJFABIHJ && IGLDJOKKFJE == Jackal.jackal && Jackal.jackal == PlayerControl.LocalPlayer) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.sidekickPromotes();
            }

            // Seer show flash and add dead player position
            if (Seer.seer != null && PlayerControl.LocalPlayer == Seer.seer && !Seer.seer.IDOFAMCIJKE.FGNJJFABIHJ && Seer.seer != IGLDJOKKFJE && Seer.mode <= 1) {
                HudManager.CMJOLNCMAPD.FullScreen.enabled = true;
                HudManager.CMJOLNCMAPD.StartCoroutine(Effects.LDACHPMFOIF(1f, new Action<float>((p) => {
                    var renderer = HudManager.CMJOLNCMAPD.FullScreen;
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
            if (Seer.deadBodyPositions != null) Seer.deadBodyPositions.Add(IGLDJOKKFJE.transform.position);

            // Child set adapted kill cooldown
            if (Child.child != null && PlayerControl.LocalPlayer == Child.child && Child.child.IDOFAMCIJKE.CIDDOFDJHJH) {
                var multiplier = Child.isGrownUp() ? 0.66f : 2f;
                Child.child.SetKillTimer(PlayerControl.GameOptions.ELBDIKIOHHH * multiplier);
            }
        }
    }

    [HarmonyPatch(typeof(KillAnimation),nameof(KillAnimation.CoPerformKill))]
    class Test {
        public static void Prefix(KillAnimation __instance, ref PlayerControl KMMMAPHIMLH, ref PlayerControl IGLDJOKKFJE) {
            if (Vampire.vampire != null && Vampire.vampire == KMMMAPHIMLH && Vampire.bitten != null && Vampire.bitten == IGLDJOKKFJE)
                KMMMAPHIMLH = IGLDJOKKFJE;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class ExilePlayerPatch
    {
        public static void Prefix(PlayerControl __instance) {
            // Child exile lose condition
            if (Child.child != null && Child.child == __instance && !Child.isGrownUp() && !Child.child.IDOFAMCIJKE.CIDDOFDJHJH) {
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
                if (otherLover != null && !otherLover.IDOFAMCIJKE.FGNJJFABIHJ && Lovers.bothDie)
                    otherLover.Exiled();
            }
            
            // Sidekick promotion trigger on exile
            if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.IDOFAMCIJKE.FGNJJFABIHJ && __instance == Jackal.jackal && Jackal.jackal == PlayerControl.LocalPlayer) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.sidekickPromotes();
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.AMDJMEEHNIG), MethodType.Getter)]
    class PlayerControlCanMovePatch {
        public static bool Prefix(PlayerControl __instance, ref bool __result)
        {
            __result = __instance.moveable &&
                !Minigame.Instance &&
                (!DestroyableSingleton<HudManager>.JECNDKBIOFO || (!DestroyableSingleton<HudManager>.CMJOLNCMAPD.Chat.LFGAAGECFFO && !DestroyableSingleton<HudManager>.CMJOLNCMAPD.KillOverlay.LFGAAGECFFO && !DestroyableSingleton<HudManager>.CMJOLNCMAPD.GameMenu.LFGAAGECFFO)) &&
                (!MapBehaviour.Instance || !MapBehaviour.Instance.IPOGMCKNALI) &&
                !MeetingHud.Instance &&
                !CustomPlayerMenu.Instance &&
                !ExileController.Instance &&
                !IntroCutscene.Instance;
            return false;
        }
    }
}
