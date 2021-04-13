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

using Palette = GLNPIJPGGNJ;
using DeathReason = KAPJFCMEBJE;
using SwitchSystem = FNEHFOPHPJO;
using SystemTypes = LGBKLKNAINN;
using Effects = HLPCBNMDEHF;

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
                if (!player.IDOFAMCIJKE.CIDDOFDJHJH)
                {
                    player.RemoveInfected();
                    player.MurderPlayer(player);
                    player.IDOFAMCIJKE.FGNJJFABIHJ = true;
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
            if (AmongUsClient.Instance.CBKCIKKEJHI) { // If lobby host
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
            switchSystem.GNPBPJHLPAO = switchSystem.KNFBDAFFMGF;
        }

        public static void engineerUsedRepair() {
            Engineer.usedRepair = true;
        }

        public static void janitorClean(byte playerId) {
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++) {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).GMBAIPNOKLP == playerId)
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
            HudManager.CMJOLNCMAPD.FullScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
            HudManager.CMJOLNCMAPD.FullScreen.enabled = true;
            PlayerControl.LocalPlayer.StartCoroutine(Effects.LDACHPMFOIF(TimeMaster.rewindTime / 2, new Action<float>((p) => {
                if (p == 1f) HudManager.CMJOLNCMAPD.FullScreen.enabled = false;
            })));

            if (TimeMaster.timeMaster == null) return;

            PlayerControl lp = PlayerControl.LocalPlayer;
            if (lp?.IDOFAMCIJKE != null && !lp.IDOFAMCIJKE.FGNJJFABIHJ && lp.inVent) {
                if ((float)(DateTime.UtcNow -localVentEnterTimePoint).TotalMilliseconds < 1000 * TimeMaster.rewindTime) {
                    foreach (Vent vent in ShipStatus.Instance.GIDPCPOEFBC) {
                        bool canUse;
                        bool couldUse;
                        vent.CanUse(PlayerControl.LocalPlayer.IDOFAMCIJKE, out canUse, out couldUse);
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
            PlayerControl.LocalPlayer.StartCoroutine(Effects.LDACHPMFOIF(TimeMaster.shieldDuration, new Action<float>((p) => {
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
            if (Medic.shielded != null && Medic.shielded == PlayerControl.LocalPlayer && Medic.showAttemptToShielded && HudManager.CMJOLNCMAPD?.FullScreen != null) {
                HudManager.CMJOLNCMAPD.FullScreen.enabled = true;
                HudManager.CMJOLNCMAPD.StartCoroutine(Effects.LDACHPMFOIF(0.5f, new Action<float>((p) => {
                    var renderer = HudManager.CMJOLNCMAPD.FullScreen;
                    Color c = Palette.LDCHDOFJPGH;
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
            if (player.IDOFAMCIJKE.CIDDOFDJHJH || player == Jackal.jackal || player == Sidekick.sidekick) {
                oldShifter.Exiled();
                return;
            }

            // Switch shield
            if (Medic.shielded != null && Medic.shielded == player) {
                Medic.shielded.LNMJKMLHMIM.material.SetFloat("_Outline", 0f);
                Medic.shielded = oldShifter;
            } else if (Medic.shielded != null && Medic.shielded == oldShifter) {
                Medic.shielded.LNMJKMLHMIM.material.SetFloat("_Outline", 0f);
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
            if (Lovers.lover1 != null && !Lovers.lover1.IDOFAMCIJKE.FGNJJFABIHJ && Lovers.lover1.PlayerId == remainingLoverId) {
                Lovers.lover1.MurderPlayer(Lovers.lover1);
            } else if (Lovers.lover2 != null && !Lovers.lover2.IDOFAMCIJKE.FGNJJFABIHJ && Lovers.lover2.PlayerId == remainingLoverId) {
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
                if (player.PlayerId == targetId && !player.IDOFAMCIJKE.FGNJJFABIHJ) {
                        Vampire.bitten = player;
                }
            }
        }

        public static void vampireTryKill() {
            if (Vampire.bitten != null && !Vampire.bitten.IDOFAMCIJKE.FGNJJFABIHJ) {
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
                    if(!Jackal.canCreateSidekickFromImpostor && player.IDOFAMCIJKE.CIDDOFDJHJH) {
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
                if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.IDOFAMCIJKE.FGNJJFABIHJ) {
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
            if(PlayerControl.LocalPlayer.IDOFAMCIJKE.CIDDOFDJHJH) {
                new CustomMessage("Lights are out", Trickster.lightsOutDuration);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class RPCHandlerPatch
    {
        static void Postfix(byte GIICFHKILOB, MessageReader DOOILGKLBBF)
        {
            byte packetId = GIICFHKILOB;
            MessageReader reader = DOOILGKLBBF;
            switch (packetId) {

                // Main Controls

                case (byte)CustomRPC.ResetVaribles:
                    RPCProcedure.resetVariables();
                    break;
                case (byte)CustomRPC.ShareOptionSelection:
                    uint id = DOOILGKLBBF.ReadPackedUInt32();
                    uint selection = DOOILGKLBBF.ReadPackedUInt32();
                    RPCProcedure.shareOptionSelection(id, selection);
                    break;
                case (byte)CustomRPC.ForceEnd:
                    RPCProcedure.forceEnd();
                    break;
                case (byte)CustomRPC.SetRole:
                    byte roleId = DOOILGKLBBF.ReadByte();
                    byte playerId = DOOILGKLBBF.ReadByte();
                    RPCProcedure.setRole(roleId, playerId);
                    break;
                case (byte)CustomRPC.SetUncheckedColor:
                    byte c = DOOILGKLBBF.ReadByte();
                    byte p = DOOILGKLBBF.ReadByte();
                    RPCProcedure.setUncheckedColor(c, p);
                    break;
                case (byte)CustomRPC.VersionHandshake:
                    byte major = DOOILGKLBBF.ReadByte();
                    byte minor = DOOILGKLBBF.ReadByte();
                    byte patch = DOOILGKLBBF.ReadByte();
                    byte versionOwnerId = DOOILGKLBBF.ReadByte();
                    RPCProcedure.versionHandshake(major, minor, patch, versionOwnerId);
                    break;
                case (byte)CustomRPC.UseUncheckedVent:
                    int ventId = DOOILGKLBBF.ReadPackedInt32();
                    byte ventingPlayer = DOOILGKLBBF.ReadByte();
                    byte isEnter = DOOILGKLBBF.ReadByte();
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
                    RPCProcedure.janitorClean(DOOILGKLBBF.ReadByte());
                    break;
                case (byte)CustomRPC.SheriffKill:
                    RPCProcedure.sheriffKill(DOOILGKLBBF.ReadByte());
                    break;
                case (byte)CustomRPC.TimeMasterRewindTime:
                    RPCProcedure.timeMasterRewindTime();
                    break;
                case (byte)CustomRPC.TimeMasterShield:
                    RPCProcedure.timeMasterShield();
                    break;
                case (byte)CustomRPC.MedicSetShielded:
                    RPCProcedure.medicSetShielded(DOOILGKLBBF.ReadByte());
                    break;
                case (byte)CustomRPC.ShieldedMurderAttempt:
                    RPCProcedure.shieldedMurderAttempt();
                    break;
                case (byte)CustomRPC.ShifterShift:
                    RPCProcedure.shifterShift(DOOILGKLBBF.ReadByte());
                    break;
                case (byte)CustomRPC.SwapperSwap:
                    byte playerId1 = DOOILGKLBBF.ReadByte();
                    byte playerId2 = DOOILGKLBBF.ReadByte();
                    RPCProcedure.swapperSwap(playerId1, playerId2);
                    break;
                case (byte)CustomRPC.MorphlingMorph:
                    RPCProcedure.morphlingMorph(DOOILGKLBBF.ReadByte());
                    break;
                case (byte)CustomRPC.CamouflagerCamouflage:
                    RPCProcedure.camouflagerCamouflage();
                    break;
                case (byte)CustomRPC.LoverSuicide:
                    RPCProcedure.loverSuicide(DOOILGKLBBF.ReadByte());
                    break;
                case (byte)CustomRPC.VampireSetBitten:
                    byte bittenId = DOOILGKLBBF.ReadByte();
                    byte reset = DOOILGKLBBF.ReadByte();
                    RPCProcedure.vampireSetBitten(bittenId, reset);
                    break;
                case (byte)CustomRPC.VampireTryKill:
                    RPCProcedure.vampireTryKill();
                    break;
                case (byte)CustomRPC.PlaceGarlic:
                    RPCProcedure.placeGarlic(DOOILGKLBBF.ReadBytesAndSize());
                    break;
                case (byte)CustomRPC.TrackerUsedTracker:
                    RPCProcedure.trackerUsedTracker(DOOILGKLBBF.ReadByte());
                    break;
                case (byte)CustomRPC.JackalKill:
                    RPCProcedure.jackalKill(DOOILGKLBBF.ReadByte());
                    break;
                case (byte)CustomRPC.SidekickKill:
                    RPCProcedure.sidekickKill(DOOILGKLBBF.ReadByte());
                    break;
                case (byte)CustomRPC.JackalCreatesSidekick:
                    RPCProcedure.jackalCreatesSidekick(DOOILGKLBBF.ReadByte());
                    break;
                case (byte)CustomRPC.SidekickPromotes:
                    RPCProcedure.sidekickPromotes();
                    break;
                case (byte)CustomRPC.ErasePlayerRole:
                    RPCProcedure.erasePlayerRole(DOOILGKLBBF.ReadByte());
                    break;
                case (byte)CustomRPC.SetFutureErased:
                    RPCProcedure.setFutureErased(DOOILGKLBBF.ReadByte());
                    break;
                case (byte)CustomRPC.SetFutureShifted:
                    RPCProcedure.setFutureShifted(DOOILGKLBBF.ReadByte());
                    break;
                case (byte)CustomRPC.PlaceJackInTheBox:
                    RPCProcedure.placeJackInTheBox(DOOILGKLBBF.ReadBytesAndSize());
                    break;
                case (byte)CustomRPC.LightsOut:
                    RPCProcedure.lightsOut();
                    break;
            }
        }
    }
}
