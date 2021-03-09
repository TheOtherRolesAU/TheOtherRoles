using HarmonyLib;
using Hazel;
using static BonusRoles.BonusRoles;
using static BonusRoles.HudManagerStartPatch;
using static BonusRoles.GameHistory;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace BonusRoles
{
    enum RoleId {
        Jester = 0,
        Mayor = 1,
        Engineer = 2,
        Sheriff = 3,
        Lighter = 4,
        Godfather = 5,
        Mafioso = 6,
        Janitor = 7,
        Detective = 8,
        TimeMaster = 9,
        Medic = 10,
        Shifter = 11,
        Swapper = 12,
        Lover1 = 13,
        Lover2 = 14,
        Seer = 15,
        Morphling = 16,
        Camouflager = 17,
        Spy = 18,
        Child = 19,
    }

    enum CustomRPC
    {
        // Main Controls

        ResetVaribles = 50,
        ForceEnd = 51,
        SetRole = 52,

        // Role functionality

        JesterWin = 80,
        EngineerFixLights = 81,
        EngineerUsedRepair = 82,
        JanitorClean = 83,
        SheriffKill = 84,
        TimeMasterRewindTime = 85,
        MedicSetShielded = 86,
        ShieldedMurderAttempt = 87,
        TimeMasterRevive = 88,
        ShifterShift = 89,
        SwapperSwap = 90,
        SeerReveal = 91,
        MorphlingMorph = 92,
        CamouflagerCamouflage = 93,
        ChildDied = 94,
        LoverSuicide = 95,
    }

    public static class RPCProcedure {

        // Main Controls

        public static void resetVariables() {
            clearAndReloadRoles();
            clearGameHistory();
            setCustomButtonCooldowns();
        }

        public static void forceEnd() {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.IsImpostor)
                {
                    player.RemoveInfected();
                    player.Die(DeathReason.Exile);
                    player.Data.IsDead = true;
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
                    case RoleId.Spy:
                        Spy.spy = player;
                        break;
                    case RoleId.Child:
                        Child.child = player;
                        break;
                    }
                }
        }

        // Role functionality

        public static void jesterWin() {
            Jester.jester.Revive();
            Jester.jester.Data.IsDead = false;
            Jester.jester.Data.IsImpostor = true;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player != Jester.jester)
                {
                    player.RemoveInfected();
                    player.Die(DeathReason.Exile);
                    player.Data.IsDead = true;
                    player.Data.IsImpostor = false;
                }
            }
        }

        public static void engineerFixLights() {
            SwitchSystem switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
            switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
        }

        public static void engineerUsedRepair() {
            Engineer.usedRepair = true;
        }

        public static void janitorClean(byte playerId) {
            DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++) {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == playerId)
                    Object.Destroy(array[i].gameObject);
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
            TimeMaster.isRewinding = true;
            if (MapBehaviour.Instance)
                MapBehaviour.Instance.Close();
            if (Minigame.Instance)
                Minigame.Instance.ForceClose();
            PlayerControl.LocalPlayer.moveable = false;
            HudManager.Instance.FullScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
            HudManager.Instance.FullScreen.enabled = true;
        }

        public static void medicSetShielded(byte shieldedId) {
            Medic.usedShield = true;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == shieldedId)
                    Medic.shielded = player;
        }

        public static void shieldedMurderAttempt() {
            if (Medic.shielded != null && Medic.shielded == PlayerControl.LocalPlayer && Medic.showAttemptToShielded && HudManager.Instance?.FullScreen != null) {
                Color c = Palette.ImpostorRed;
                HudManager.Instance.FullScreen.enabled = true;
                Reactor.Coroutines.Start(Helpers.CoFlashAndDisable(
                    HudManager.Instance.FullScreen,
                    0.5f,
                    new Color(c.r, c.g, c.b, 0f),
                    new Color(c.r, c.g, c.b, 0.75f)
                ));
            }
        }

        public static void timeMasterRevive(byte playerId) {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == playerId) {
                    player.Revive();
                    var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == playerId);
                    DeadPlayer deadPlayerEntry = deadPlayers.Where(x => x.player.PlayerId == playerId).FirstOrDefault();
                    if (body != null) UnityEngine.Object.Destroy(body.gameObject);
                    if (deadPlayerEntry != null) deadPlayers.Remove(deadPlayerEntry);
                }
        }

        public static void shifterShift(byte targetId) {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == targetId && Shifter.shifter != null)
                {
                    // Suicide when impostor or impostor variants
                    if (player.Data.IsImpostor) {
                        Shifter.shifter.MurderPlayer(Shifter.shifter);
                        return;
                    }

                    PlayerControl oldShifter = Shifter.shifter;
                    // Switch tasks
                    var shifterSabotageTasks = oldShifter.myTasks;
                    oldShifter.myTasks = player.myTasks;
                    player.myTasks = shifterSabotageTasks;

                    // Switch shield
                    if (Medic.shielded != null && Medic.shielded == player) {
                        Medic.shielded.myRend.material.SetFloat("_Outline", 0f);
                        Medic.shielded = oldShifter;
                    } else if (Medic.shielded != null && Medic.shielded == oldShifter) {
                        Medic.shielded.myRend.material.SetFloat("_Outline", 0f);
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
                    } else if (Spy.spy != null && Spy.spy == player) {
                        Spy.spy = oldShifter;
                    } else if (Child.child != null && Child.child == player) {
                        Child.child = oldShifter;
                    } else { // Crewmate
                    }
                    
                    Shifter.shifter = player;

                    // Set cooldowns to max for both players
                    if (PlayerControl.LocalPlayer == Shifter.shifter || PlayerControl.LocalPlayer == oldShifter)
                        CustomButton.ResetAllCooldowns();
                }
            }
        }

        public static void swapperSwap(byte playerId1, byte playerId2) {
            if (MeetingHud.Instance) {
                Swapper.playerId1 = playerId1;
                Swapper.playerId2 = playerId2;
            }
        }

        public static void seerReveal(byte playerId) {
            if (Seer.seer == null) return;

            PlayerControl player = Helpers.playerById(playerId);
            if (player != null && !Seer.revealedPlayers.Any(p => p.Data.PlayerId == playerId)) {
                Seer.revealedPlayers.Add(player);

                if (PlayerControl.LocalPlayer == player && HudManager.Instance?.FullScreen != null) {
                    SeerInfo si = SeerInfo.getSeerInfoForPlayer(player);
                    bool showNotification = false;
                    if (Seer.playersWithNotification == 0 ) showNotification = true;
                    else if (Seer.playersWithNotification == 1 && si.isGood) showNotification = true;
                    else if (Seer.playersWithNotification == 2 && !si.isGood) showNotification = true;
                    else if (Seer.playersWithNotification == 3) showNotification = false;

                    if (showNotification) {
                        HudManager.Instance.FullScreen.enabled = true;
                        Reactor.Coroutines.Start(Helpers.CoFlashAndDisable(
                            HudManager.Instance.FullScreen,
                            0.5f,
                            new Color(42f / 255f, 187f / 255f, 245f / 255f, 0f),
                            new Color(42f / 255f, 187f / 255f, 245f / 255f, 0.75f)
                        ));
                    }
                }
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

        public static void childDied() {
            Child.child.Revive();
            Child.child.Data.IsDead = false;
            Child.child.Data.IsImpostor = true;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player != null && player != Child.child)
                {
                    player.RemoveInfected();
                    player.Die(DeathReason.Exile);
                    player.Data.IsDead = true;
                    player.Data.IsImpostor = false;
                }
            }
        }

        public static void loverSuicide(byte remainingLoverId) {
            if (Lovers.lover1 != null && !Lovers.lover1.Data.IsDead && Lovers.lover1.PlayerId == remainingLoverId) {
                Lovers.lover1.MurderPlayer(Lovers.lover1);
            } else if (Lovers.lover2 != null && !Lovers.lover2.Data.IsDead && Lovers.lover2.PlayerId == remainingLoverId) {
                Lovers.lover2.MurderPlayer(Lovers.lover2);
            }
        }     
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class RPCHandlerPatch
    {
        static void Postfix(byte HKHMBLJFLMC, MessageReader ALMCIJKELCP)
        {
            byte packetId = HKHMBLJFLMC;
            MessageReader reader = ALMCIJKELCP;
            switch (packetId) {

                // Main Controls

                case (byte)CustomRPC.ResetVaribles:
                    RPCProcedure.resetVariables();
                    break;
                case (byte)CustomRPC.ForceEnd:
                    RPCProcedure.forceEnd();
                    break;
                case (byte)CustomRPC.SetRole:
                    byte roleId = ALMCIJKELCP.ReadByte();
                    byte playerId = ALMCIJKELCP.ReadByte();
                    RPCProcedure.setRole(roleId, playerId);
                    break;


                // Role functionality

                case (byte)CustomRPC.JesterWin:
                    RPCProcedure.jesterWin();
                    break;
                case (byte)CustomRPC.EngineerFixLights:
                    RPCProcedure.engineerFixLights();
                    break;
                case (byte)CustomRPC.EngineerUsedRepair:
                    RPCProcedure.engineerUsedRepair();
                    break;
                case (byte)CustomRPC.JanitorClean:
                    RPCProcedure.janitorClean(ALMCIJKELCP.ReadByte());
                    break;
                case (byte)CustomRPC.SheriffKill:
                    RPCProcedure.sheriffKill(ALMCIJKELCP.ReadByte());
                    break;
                case (byte)CustomRPC.TimeMasterRewindTime:
                    RPCProcedure.timeMasterRewindTime();
                    break;
                case (byte)CustomRPC.MedicSetShielded:
                    RPCProcedure.medicSetShielded(ALMCIJKELCP.ReadByte());
                    break;
                case (byte)CustomRPC.ShieldedMurderAttempt:
                    RPCProcedure.shieldedMurderAttempt();
                    break;
                case (byte)CustomRPC.TimeMasterRevive:
                    RPCProcedure.timeMasterRevive(ALMCIJKELCP.ReadByte());
                    break;
                case (byte)CustomRPC.ShifterShift:
                    RPCProcedure.shifterShift(ALMCIJKELCP.ReadByte());
                    break;
                case (byte)CustomRPC.SwapperSwap:
                    byte playerId1 = ALMCIJKELCP.ReadByte();
                    byte playerId2 = ALMCIJKELCP.ReadByte();
                    RPCProcedure.swapperSwap(playerId1, playerId2);
                    break;
                case (byte)CustomRPC.SeerReveal:
                    RPCProcedure.seerReveal(ALMCIJKELCP.ReadByte());
                    break;
                case (byte)CustomRPC.MorphlingMorph:
                    RPCProcedure.morphlingMorph(ALMCIJKELCP.ReadByte());
                    break;
                case (byte)CustomRPC.CamouflagerCamouflage:
                    RPCProcedure.camouflagerCamouflage();
                    break;
                case (byte)CustomRPC.ChildDied:
                    RPCProcedure.childDied();
                    break;
                case (byte)CustomRPC.LoverSuicide:
                    RPCProcedure.loverSuicide(ALMCIJKELCP.ReadByte());
                    break;
            }
        }
    }
}
