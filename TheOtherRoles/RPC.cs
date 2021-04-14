using HarmonyLib;
using Hazel;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.HudManagerStartPatch;
using static TheOtherRoles.GameHistory;
using static TheOtherRoles.MapOptions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

using Palette = BLMBFIODBKL;
using DeathReason = EGHDCAKGMKI;
using SwitchSystem = ABIMJJMBJJM;
using SystemTypes = BCPJLGGNHBC;
using Effects = AEOEPNHOJDP;

namespace TheOtherRoles
{
    enum RoleId {
        Jester,
        Mayor,
        Engineer,
        Sheriff,
        Lighter,
        Godfather,
        Mafioso,
        Janitor,
        Detective,
        TimeMaster,
        Medic,
        Shifter,
        Swapper,
        Lover1,
        Lover2,
        Seer,
        Morphling,
        Camouflager,
        Hacker,
        Child,
        Tracker,
        Vampire,
        Snitch,
        Jackal,
        Sidekick,
        Eraser,
        Spy,
        Trickster
    }

    enum CustomRPC
    {
        // Main Controls

        ResetVaribles = 50,
        ShareOptionSelection,
        ForceEnd,
        SetRole,
        SetUncheckedColor,
        VersionHandshake,
        UseUncheckedVent,

        // Role functionality

        EngineerFixLights = 81,
        EngineerUsedRepair,
        JanitorClean,
        SheriffKill,
        MedicSetShielded,
        ShieldedMurderAttempt,
        TimeMasterShield,
        TimeMasterRewindTime,
        ShifterShift,
        SwapperSwap,
        MorphlingMorph,
        CamouflagerCamouflage,
        TrackerUsedTracker,
        LoverSuicide,
        VampireSetBitten,
        VampireTryKill,
        PlaceGarlic,
        JackalKill,
        SidekickKill,
        JackalCreatesSidekick,
        SidekickPromotes,
        ErasePlayerRole,
        SetFutureErased,
        SetFutureShifted,
        PlaceJackInTheBox,
        LightsOut
    }

    public static class RPCProcedure {

        // Main Controls

        public static void resetVariables() {
            Garlic.clearGarlics();
            JackInTheBox.clearJackInTheBoxes();
            clearAndReloadMapOptions();
            clearAndReloadRoles();
            clearGameHistory();
            setCustomButtonCooldowns();
        }

        public static void shareOptionSelection(uint id, uint selection) {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.id == (int)id);
            option.updateSelection((int)selection);
        }

        public static void forceEnd() {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (!player.PPMOEEPBHJO.FDNMBJOAPFL)
                {
                    player.RemoveInfected();
                    player.MurderPlayer(player);
                    player.PPMOEEPBHJO.IAGJEKLJCCI = true;
                }
            }
        }

        public static void setRole(byte roleId, byte playerId) {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == playerId) {
                    switch((RoleId)roleId) {
                    case RoleId.Jester:
                        Jester.jester = player;
                        break;
                    case RoleId.Mayor:
                        Mayor.mayor = player;
                        break;
                    case RoleId.Engineer:
                        Engineer.engineer = player;
                        break;
                    case RoleId.Sheriff:
                        Sheriff.sheriff = player;
                        break;
                    case RoleId.Lighter:
                        Lighter.lighter = player;
                        break;
                    case RoleId.Godfather:
                        Godfather.godfather = player;
                        break;
                    case RoleId.Mafioso:
                        Mafioso.mafioso = player;
                        break;
                    case RoleId.Janitor:
                        Janitor.janitor = player;
                        break;
                    case RoleId.Detective:
                        Detective.detective = player;
                        break;
                    case RoleId.TimeMaster:
                        TimeMaster.timeMaster = player;
                        break;
                    case RoleId.Medic:
                        Medic.medic = player;
                        break;
                    case RoleId.Shifter:
                        Shifter.shifter = player;
                        break;
                    case RoleId.Swapper:
                        Swapper.swapper = player;
                        break;
                    case RoleId.Lover1:
                        Lovers.lover1 = player;
                        break;
                    case RoleId.Lover2:
                        Lovers.lover2 = player;
                        break;
                    case RoleId.Seer:
                        Seer.seer = player;
                        break;
                    case RoleId.Morphling:
                        Morphling.morphling = player;
                        break;
                    case RoleId.Camouflager:
                        Camouflager.camouflager = player;
                        break;
                    case RoleId.Hacker:
                        Hacker.hacker = player;
                        break;
                    case RoleId.Child:
                        Child.child = player;
                        break;
                    case RoleId.Tracker:
                        Tracker.tracker = player;
                        break;
                    case RoleId.Vampire:
                        Vampire.vampire = player;
                        break;
                    case RoleId.Snitch:
                        Snitch.snitch = player;
                        break;
                    case RoleId.Jackal:
                        Jackal.jackal = player;
                        break;
                    case RoleId.Sidekick:
                        Sidekick.sidekick = player;
                        break;
                    case RoleId.Eraser:
                        Eraser.eraser = player;
                        break;
                    case RoleId.Spy:
                        Spy.spy = player;
                        break;
                    case RoleId.Trickster:
                        Trickster.trickster = player;
                        break;
                    }
                }
        }

        public static void setUncheckedColor(byte colorId, byte playerId) {
            var player = Helpers.playerById(playerId);
            if (player != null) player.SetColor(colorId);
        }

        public static void versionHandshake(byte major, byte minor, byte patch, byte playerId) {
            if (AmongUsClient.Instance.HHBLOCGKFAB) { // If lobby host
                GameStartManagerPatch.playerVersions[playerId] = new Tuple<byte, byte, byte>(major, minor, patch);
            }
        }

        public static void useUncheckedVent(int ventId, byte playerId, byte isEnter) {
            PlayerControl player = Helpers.playerById(playerId);
            if (player == null) return;
            // Fill dummy MessageReader and call MyPhysics.HandleRpc as the corountines cannot be accessed
            MessageReader reader = new MessageReader();
            byte[] bytes = BitConverter.GetBytes(ventId);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            reader.Buffer = bytes;
            reader.Length = bytes.Length;
            player.MyPhysics.HandleRpc(isEnter != 0 ? (byte)19 : (byte)20, reader);
        }

        // Role functionality

        public static void engineerFixLights() {
            SwitchSystem switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
            switchSystem.BBCFBNPEPIE = switchSystem.FLDLDHLDCLM;
        }

        public static void engineerUsedRepair() {
            Engineer.usedRepair = true;
        }

        public static void janitorClean(byte playerId) {
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++) {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).FNPNJHNKEBK == playerId)
                    UnityEngine.Object.Destroy(array[i].gameObject);
            }
        }

        public static void sheriffKill(byte targetId) {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == targetId)
                {
                    Sheriff.sheriff.MurderPlayer(player);
                    return;
                }
            }
        }

        public static void timeMasterRewindTime() {
            TimeMaster.shieldActive = false; // Shield is no longer active when rewinding
            if(TimeMaster.timeMaster != null && TimeMaster.timeMaster == PlayerControl.LocalPlayer) {
                resetTimeMasterButton();
            }
            HudManager.CHNDKKBEIDG.FullScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
            HudManager.CHNDKKBEIDG.FullScreen.enabled = true;
            PlayerControl.LocalPlayer.StartCoroutine(Effects.DCHLMIDMBHG(TimeMaster.rewindTime / 2, new Action<float>((p) => {
                if (p == 1f) HudManager.CHNDKKBEIDG.FullScreen.enabled = false;
            })));

            if (TimeMaster.timeMaster == null) return;

            PlayerControl lp = PlayerControl.LocalPlayer;
            if (lp?.PPMOEEPBHJO != null && !lp.PPMOEEPBHJO.IAGJEKLJCCI && lp.inVent) {
                if ((float)(DateTime.UtcNow -localVentEnterTimePoint).TotalMilliseconds < 1000 * TimeMaster.rewindTime) {
                    foreach (Vent vent in ShipStatus.Instance.GJHKPDGJHJN) {
                        bool canUse;
                        bool couldUse;
                        vent.CanUse(PlayerControl.LocalPlayer.PPMOEEPBHJO, out canUse, out couldUse);
                        if (canUse) {
                            PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(vent.Id);
			                vent.SetButtons(false);
                        }
                    }
                }
            }

            if (PlayerControl.LocalPlayer == TimeMaster.timeMaster) return; // Time Master himself does not rewind

            TimeMaster.isRewinding = true;

            if (MapBehaviour.Instance)
                MapBehaviour.Instance.Close();
            if (Minigame.Instance)
                Minigame.Instance.ForceClose();
            PlayerControl.LocalPlayer.moveable = false;
        }

        public static void timeMasterShield() {
            TimeMaster.shieldActive = true;
            PlayerControl.LocalPlayer.StartCoroutine(Effects.DCHLMIDMBHG(TimeMaster.shieldDuration, new Action<float>((p) => {
                if (p == 1f) TimeMaster.shieldActive = false;
            })));
        }

        public static void medicSetShielded(byte shieldedId) {
            Medic.usedShield = true;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == shieldedId)
                    Medic.shielded = player;
        }

        public static void shieldedMurderAttempt() {
            if (Medic.shielded != null && Medic.shielded == PlayerControl.LocalPlayer && Medic.showAttemptToShielded && HudManager.CHNDKKBEIDG?.FullScreen != null) {
                HudManager.CHNDKKBEIDG.FullScreen.enabled = true;
                HudManager.CHNDKKBEIDG.StartCoroutine(Effects.DCHLMIDMBHG(0.5f, new Action<float>((p) => {
                    var renderer = HudManager.CHNDKKBEIDG.FullScreen;
                    Color c = Palette.JPCHLLEJNEH;
                    if (p < 0.5) {
                        if (renderer != null)
                            renderer.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(p * 2 * 0.75f));
                    } else {
                        if (renderer != null)
                            renderer.color = new Color(c.r, c.g, c.b, Mathf.Clamp01((1-p) * 2 * 0.75f));
                    }
                    if (p == 1f && renderer != null) renderer.enabled = false;
                })));
            }
        }

        public static void shifterShift(byte targetId) {
            PlayerControl oldShifter = Shifter.shifter;
            PlayerControl player = Helpers.playerById(targetId);
            if (player == null || oldShifter == null) return;

            Shifter.futureShift = null;
            Shifter.clearAndReload();

            // Suicide (exile) when impostor or impostor variants
            if (player.PPMOEEPBHJO.FDNMBJOAPFL || player == Jackal.jackal || player == Sidekick.sidekick) {
                oldShifter.Exiled();
                return;
            }

            // Switch shield
            if (Medic.shielded != null && Medic.shielded == player) {
                Medic.shielded.KJAENOGGEOK.material.SetFloat("_Outline", 0f);
                Medic.shielded = oldShifter;
            } else if (Medic.shielded != null && Medic.shielded == oldShifter) {
                Medic.shielded.KJAENOGGEOK.material.SetFloat("_Outline", 0f);
                Medic.shielded = player;
            }

            // Shift role
            if (Jester.jester != null && Jester.jester == player) {
                Jester.jester = oldShifter;
            } else if (Mayor.mayor != null && Mayor.mayor == player) {
                Mayor.mayor = oldShifter;
            } else if (Engineer.engineer != null && Engineer.engineer == player) {
                Engineer.engineer = oldShifter;
            } else if (Sheriff.sheriff != null && Sheriff.sheriff == player) {
                Sheriff.sheriff = oldShifter;
            } else if (Lighter.lighter != null && Lighter.lighter == player) {
                Lighter.lighter = oldShifter;
            } else if (Detective.detective != null && Detective.detective == player) {
                Detective.detective = oldShifter;
            } else if (TimeMaster.timeMaster != null && TimeMaster.timeMaster == player) {
                TimeMaster.timeMaster = oldShifter;
            } else if (Medic.medic != null && Medic.medic == player) {
                Medic.medic = oldShifter;
            } else if (Swapper.swapper != null && Swapper.swapper == player) {
                Swapper.swapper = oldShifter;
            } else if (Lovers.lover1 != null && Lovers.lover1 == player) {
                Lovers.lover1 = oldShifter;
            } else if (Lovers.lover2 != null && Lovers.lover2 == player) {
                Lovers.lover2 = oldShifter;
            } else if (Seer.seer != null && Seer.seer == player) {
                Seer.seer = oldShifter;
            } else if (Hacker.hacker != null && Hacker.hacker == player) {
                Hacker.hacker = oldShifter;
            } else if (Child.child != null && Child.child == player) {
                Child.child = oldShifter;
            } else if (Tracker.tracker != null && Tracker.tracker == player) {
                Tracker.tracker = oldShifter;
            } else if (Snitch.snitch != null && Snitch.snitch == player) {
                Snitch.snitch = oldShifter;
            } else if (Spy.spy != null && Spy.spy == player) {
                Spy.spy = oldShifter;
            } else { // Crewmate
            }
            
            // Set cooldowns to max for both players
            if (PlayerControl.LocalPlayer == oldShifter || PlayerControl.LocalPlayer == player)
                CustomButton.ResetAllCooldowns();
        }

        public static void swapperSwap(byte playerId1, byte playerId2) {
            if (MeetingHud.Instance) {
                Swapper.playerId1 = playerId1;
                Swapper.playerId2 = playerId2;
            }
        }

        public static void morphlingMorph(byte playerId) {  
            PlayerControl target = Helpers.playerById(playerId);
            if (Morphling.morphling == null || target == null) return;

            Morphling.morphTimer = 10f;
            Morphling.morphTarget = target;
        }

        public static void camouflagerCamouflage() {
            if (Camouflager.camouflager == null) return;

            Camouflager.camouflageTimer = 10f;
        }

        public static void loverSuicide(byte remainingLoverId) {
            if (Lovers.lover1 != null && !Lovers.lover1.PPMOEEPBHJO.IAGJEKLJCCI && Lovers.lover1.PlayerId == remainingLoverId) {
                Lovers.lover1.MurderPlayer(Lovers.lover1);
            } else if (Lovers.lover2 != null && !Lovers.lover2.PPMOEEPBHJO.IAGJEKLJCCI && Lovers.lover2.PlayerId == remainingLoverId) {
                Lovers.lover2.MurderPlayer(Lovers.lover2);
            }
        }

        public static void vampireSetBitten(byte targetId, byte reset) {
            if (reset != 0) {
                Vampire.bitten = null;
                return;
            }

            if (Vampire.vampire == null) return;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                if (player.PlayerId == targetId && !player.PPMOEEPBHJO.IAGJEKLJCCI) {
                        Vampire.bitten = player;
                }
            }
        }

        public static void vampireTryKill() {
            if (Vampire.bitten != null && !Vampire.bitten.PPMOEEPBHJO.IAGJEKLJCCI) {
                Vampire.vampire.MurderPlayer(Vampire.bitten);
            }
            Vampire.bitten = null;
        }

        public static void placeGarlic(byte[] buff) {
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0*sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1*sizeof(float));
            new Garlic(position);
        }

        public static void trackerUsedTracker(byte targetId) {
            Tracker.usedTracker = true;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == targetId)
                    Tracker.tracked = player;
        }

        public static void jackalKill(byte targetId) {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == targetId)
                {
                    Jackal.jackal.MurderPlayer(player);
                    return;
                }
            }
        }

        public static void sidekickKill(byte targetId) {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == targetId)
                {
                    Sidekick.sidekick.MurderPlayer(player);
                    return;
                }
            }
        }

        public static void jackalCreatesSidekick(byte targetId) {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == targetId)
                {
                    if(!Jackal.canCreateSidekickFromImpostor && player.PPMOEEPBHJO.FDNMBJOAPFL) {
                        Jackal.fakeSidekick = player;
                        return;
                    }
                    player.RemoveInfected();
                    if (player != Lovers.lover1 && player != Lovers.lover2) erasePlayerRole(player.PlayerId);
                    
                    Sidekick.sidekick = player;
                    Helpers.removeTasksFromPlayer(player);
                    return;
                }
            }
        }

        public static void sidekickPromotes() {
            var player = Sidekick.sidekick;
            Jackal.removeCurrentJackal();
            Jackal.jackal = player;
            if (Jackal.jackalPromotedFromSidekickCanCreateSidekick == false) {
                Jackal.canCreateSidekick = false;
            }
            Sidekick.clearAndReload();
            return;
        }
        
        public static void erasePlayerRole(byte playerId) {
            PlayerControl player = Helpers.playerById(playerId);
            if (player == null) return;

            // Crewmate roles
            if (player == Mayor.mayor) Mayor.clearAndReload();
            if (player == Engineer.engineer) Engineer.clearAndReload();
            if (player == Sheriff.sheriff) Sheriff.clearAndReload();
            if (player == Lighter.lighter) Lighter.clearAndReload();
            if (player == Detective.detective) Detective.clearAndReload();
            if (player == TimeMaster.timeMaster) TimeMaster.clearAndReload();
            if (player == Medic.medic) Medic.clearAndReload();
            if (player == Shifter.shifter) Shifter.clearAndReload();
            if (player == Seer.seer) Seer.clearAndReload();
            if (player == Hacker.hacker) Hacker.clearAndReload();
            if (player == Child.child) Child.clearAndReload();
            if (player == Tracker.tracker) Tracker.clearAndReload();
            if (player == Snitch.snitch) Snitch.clearAndReload();
            if (player == Swapper.swapper) Swapper.clearAndReload();
            if (player == Spy.spy) Spy.clearAndReload();

            // Impostor roles
            if (player == Morphling.morphling) Morphling.clearAndReload();
            if (player == Camouflager.camouflager) Camouflager.clearAndReload();
            if (player == Godfather.godfather) Godfather.clearAndReload();
            if (player == Mafioso.mafioso) Mafioso.clearAndReload();
            if (player == Janitor.janitor) Janitor.clearAndReload();
            if (player == Vampire.vampire) Vampire.clearAndReload();
            if (player == Eraser.eraser) Eraser.clearAndReload();
            if (player == Trickster.trickster) Trickster.clearAndReload();
        
            // Other roles
            if (player == Jester.jester) Jester.clearAndReload();
            if (player == Lovers.lover1 || player == Lovers.lover2) { // The whole Lover couple is being erased
                Lovers.clearAndReload(); 
            }
            if (player == Jackal.jackal) { // Promote Sidekick and hence override the the Jackal or erase Jackal
                if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.PPMOEEPBHJO.IAGJEKLJCCI) {
                    RPCProcedure.sidekickPromotes();
                } else {
                    Jackal.clearAndReload();
                }
            }
            if (player == Sidekick.sidekick) Sidekick.clearAndReload();
        }

        public static void setFutureErased(byte playerId) {
            PlayerControl player = Helpers.playerById(playerId);
            if (Eraser.futureErased == null) Eraser.futureErased = new List<PlayerControl>();
            if (player != null) Eraser.futureErased.Add(player);
        }

        public static void setFutureShifted(byte playerId) {
            Shifter.futureShift = Helpers.playerById(playerId);
        }
        
        public static void placeJackInTheBox(byte[] buff) {
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0*sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1*sizeof(float));
            new JackInTheBox(position);
        }

        public static void lightsOut() {
            Trickster.lightsOutTimer = Trickster.lightsOutDuration;
            // If the local player is impostor indicate lights out
            if(PlayerControl.LocalPlayer.PPMOEEPBHJO.FDNMBJOAPFL) {
                new CustomMessage("Lights are out", Trickster.lightsOutDuration);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class RPCHandlerPatch
    {
        static void Postfix(byte ONIABIILFGF, MessageReader JIGFBHFFNFI)
        {
            byte packetId = ONIABIILFGF;
            MessageReader reader = JIGFBHFFNFI;
            switch (packetId) {

                // Main Controls

                case (byte)CustomRPC.ResetVaribles:
                    RPCProcedure.resetVariables();
                    break;
                case (byte)CustomRPC.ShareOptionSelection:
                    uint id = JIGFBHFFNFI.ReadPackedUInt32();
                    uint selection = JIGFBHFFNFI.ReadPackedUInt32();
                    RPCProcedure.shareOptionSelection(id, selection);
                    break;
                case (byte)CustomRPC.ForceEnd:
                    RPCProcedure.forceEnd();
                    break;
                case (byte)CustomRPC.SetRole:
                    byte roleId = JIGFBHFFNFI.ReadByte();
                    byte playerId = JIGFBHFFNFI.ReadByte();
                    RPCProcedure.setRole(roleId, playerId);
                    break;
                case (byte)CustomRPC.SetUncheckedColor:
                    byte c = JIGFBHFFNFI.ReadByte();
                    byte p = JIGFBHFFNFI.ReadByte();
                    RPCProcedure.setUncheckedColor(c, p);
                    break;
                case (byte)CustomRPC.VersionHandshake:
                    byte major = JIGFBHFFNFI.ReadByte();
                    byte minor = JIGFBHFFNFI.ReadByte();
                    byte patch = JIGFBHFFNFI.ReadByte();
                    byte versionOwnerId = JIGFBHFFNFI.ReadByte();
                    RPCProcedure.versionHandshake(major, minor, patch, versionOwnerId);
                    break;
                case (byte)CustomRPC.UseUncheckedVent:
                    int ventId = JIGFBHFFNFI.ReadPackedInt32();
                    byte ventingPlayer = JIGFBHFFNFI.ReadByte();
                    byte isEnter = JIGFBHFFNFI.ReadByte();
                    RPCProcedure.useUncheckedVent(ventId, ventingPlayer, isEnter);
                    break;

                // Role functionality

                case (byte)CustomRPC.EngineerFixLights:
                    RPCProcedure.engineerFixLights();
                    break;
                case (byte)CustomRPC.EngineerUsedRepair:
                    RPCProcedure.engineerUsedRepair();
                    break;
                case (byte)CustomRPC.JanitorClean:
                    RPCProcedure.janitorClean(JIGFBHFFNFI.ReadByte());
                    break;
                case (byte)CustomRPC.SheriffKill:
                    RPCProcedure.sheriffKill(JIGFBHFFNFI.ReadByte());
                    break;
                case (byte)CustomRPC.TimeMasterRewindTime:
                    RPCProcedure.timeMasterRewindTime();
                    break;
                case (byte)CustomRPC.TimeMasterShield:
                    RPCProcedure.timeMasterShield();
                    break;
                case (byte)CustomRPC.MedicSetShielded:
                    RPCProcedure.medicSetShielded(JIGFBHFFNFI.ReadByte());
                    break;
                case (byte)CustomRPC.ShieldedMurderAttempt:
                    RPCProcedure.shieldedMurderAttempt();
                    break;
                case (byte)CustomRPC.ShifterShift:
                    RPCProcedure.shifterShift(JIGFBHFFNFI.ReadByte());
                    break;
                case (byte)CustomRPC.SwapperSwap:
                    byte playerId1 = JIGFBHFFNFI.ReadByte();
                    byte playerId2 = JIGFBHFFNFI.ReadByte();
                    RPCProcedure.swapperSwap(playerId1, playerId2);
                    break;
                case (byte)CustomRPC.MorphlingMorph:
                    RPCProcedure.morphlingMorph(JIGFBHFFNFI.ReadByte());
                    break;
                case (byte)CustomRPC.CamouflagerCamouflage:
                    RPCProcedure.camouflagerCamouflage();
                    break;
                case (byte)CustomRPC.LoverSuicide:
                    RPCProcedure.loverSuicide(JIGFBHFFNFI.ReadByte());
                    break;
                case (byte)CustomRPC.VampireSetBitten:
                    byte bittenId = JIGFBHFFNFI.ReadByte();
                    byte reset = JIGFBHFFNFI.ReadByte();
                    RPCProcedure.vampireSetBitten(bittenId, reset);
                    break;
                case (byte)CustomRPC.VampireTryKill:
                    RPCProcedure.vampireTryKill();
                    break;
                case (byte)CustomRPC.PlaceGarlic:
                    RPCProcedure.placeGarlic(JIGFBHFFNFI.ReadBytesAndSize());
                    break;
                case (byte)CustomRPC.TrackerUsedTracker:
                    RPCProcedure.trackerUsedTracker(JIGFBHFFNFI.ReadByte());
                    break;
                case (byte)CustomRPC.JackalKill:
                    RPCProcedure.jackalKill(JIGFBHFFNFI.ReadByte());
                    break;
                case (byte)CustomRPC.SidekickKill:
                    RPCProcedure.sidekickKill(JIGFBHFFNFI.ReadByte());
                    break;
                case (byte)CustomRPC.JackalCreatesSidekick:
                    RPCProcedure.jackalCreatesSidekick(JIGFBHFFNFI.ReadByte());
                    break;
                case (byte)CustomRPC.SidekickPromotes:
                    RPCProcedure.sidekickPromotes();
                    break;
                case (byte)CustomRPC.ErasePlayerRole:
                    RPCProcedure.erasePlayerRole(JIGFBHFFNFI.ReadByte());
                    break;
                case (byte)CustomRPC.SetFutureErased:
                    RPCProcedure.setFutureErased(JIGFBHFFNFI.ReadByte());
                    break;
                case (byte)CustomRPC.SetFutureShifted:
                    RPCProcedure.setFutureShifted(JIGFBHFFNFI.ReadByte());
                    break;
                case (byte)CustomRPC.PlaceJackInTheBox:
                    RPCProcedure.placeJackInTheBox(JIGFBHFFNFI.ReadBytesAndSize());
                    break;
                case (byte)CustomRPC.LightsOut:
                    RPCProcedure.lightsOut();
                    break;
            }
        }
    }
}
