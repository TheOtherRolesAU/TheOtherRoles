using HarmonyLib;
using Hazel;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.HudManagerStartPatch;
using static TheOtherRoles.GameHistory;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;

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
        Spy,
        Child,
        BountyHunter,
        Tracker,
        Vampire,
        Snitch,
        Jackal,
        Sidekick
    }

    enum CustomRPC
    {
        // Main Controls

        ResetVaribles = 50,
        ForceEnd = 51,
        SetRole = 52,

        // Role functionality

        JesterBountyHunterWin = 80,
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
        TrackerUsedTracker = 94,
        LoverSuicide = 95,
        SetBountyHunterTarget = 96,
        VampireBiteNotification = 97,
        VampireTryKill = 98,
        PlaceGarlic = 99,
        JackalKill = 100,
        SidekickKill = 101,
        JackalCreatesSidekick = 102,
        SidekickPromotes = 103
    }

    public static class RPCProcedure {

        // Main Controls

        public static void resetVariables() {
            Garlic.clearGarlics();
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
                    player.MurderPlayer(player);
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
                    case RoleId.BountyHunter:
                        BountyHunter.bountyHunter = player;
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
                    }
                }
        }

        // Role functionality

        public static void jesterBountyHunterWin(byte exiledId) {
            PlayerControl exiled = Helpers.playerById(exiledId);
            if (exiled == null) return;

            bool jesterWin = false;
            bool bountyHunterWin = false;

            if (Jester.jester != null && exiled == Jester.jester) {
                Jester.jester.Revive();
                Jester.jester.Data.IsDead = false;
                Jester.jester.Data.IsImpostor = true;
                jesterWin = true;
            }
            if (BountyHunter.bountyHunter != null && !BountyHunter.bountyHunter.Data.IsDead && BountyHunter.target == exiled) {
                BountyHunter.bountyHunter.Data.IsImpostor = true;
                bountyHunterWin = true;
            }

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player != null && player != Jester.jester && player != BountyHunter.bountyHunter)
                {
                    player.RemoveInfected();
                    player.Die(DeathReason.Exile);
                    player.Data.IsDead = true;
                    player.Data.IsImpostor = false;
                }
            }
            if (jesterWin && !bountyHunterWin && BountyHunter.bountyHunter != null) {
                BountyHunter.bountyHunter.RemoveInfected();
                BountyHunter.bountyHunter.Die(DeathReason.Exile);
                BountyHunter.bountyHunter.Data.IsDead = true;
                BountyHunter.bountyHunter.Data.IsImpostor = false;
            } else if (bountyHunterWin && !jesterWin && Jester.jester != null) {
                Jester.jester.RemoveInfected();
                Jester.jester.Die(DeathReason.Exile);
                Jester.jester.Data.IsDead = true;
                Jester.jester.Data.IsImpostor = false;
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
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++) {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == playerId)
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
            if (TimeMaster.timeMaster == null) return;

            PlayerControl lp = PlayerControl.LocalPlayer;
            if (lp?.Data != null && !lp.Data.IsDead && lp.inVent) {
                if ((float)(DateTime.UtcNow -localVentEnterTimePoint).TotalMilliseconds < 1000 * TimeMaster.rewindTime) {
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
            }

            if (PlayerControl.LocalPlayer == TimeMaster.timeMaster) return; // Time Master himself does not rewind

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
                    if (player.Data.IsImpostor || player == Jackal.jackal || player == Sidekick.sidekick) {
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
                    } else if (BountyHunter.bountyHunter != null && BountyHunter.bountyHunter == player) {
                        BountyHunter.bountyHunter = oldShifter;
                    } else if (Tracker.tracker != null && Tracker.tracker == player) {
                        Tracker.tracker = oldShifter;
                    } else if (Snitch.snitch != null && Snitch.snitch == player) {
                        Snitch.snitch = oldShifter;
                    }else { // Crewmate
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

        public static void seerReveal(byte targetId, byte targetOrMistakeId) {
            if (Seer.seer == null) return;
            
            PlayerControl target = Helpers.playerById(targetId);
            PlayerControl targetOrMistake = Helpers.playerById(targetOrMistakeId);

            if (target != null && targetOrMistake != null && !Seer.revealedPlayers.Keys.Any(p => p.Data.PlayerId == targetId)) {
                Seer.revealedPlayers.Add(target, targetOrMistake);

                if (PlayerControl.LocalPlayer == target && HudManager.Instance?.FullScreen != null) {
                    RoleInfo si = RoleInfo.getRoleInfoForPlayer(target); // Use RoleInfo of target here, because we need the isGood of the targets role
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

        public static void loverSuicide(byte remainingLoverId) {
            if (Lovers.lover1 != null && !Lovers.lover1.Data.IsDead && Lovers.lover1.PlayerId == remainingLoverId) {
                Lovers.lover1.MurderPlayer(Lovers.lover1);
            } else if (Lovers.lover2 != null && !Lovers.lover2.Data.IsDead && Lovers.lover2.PlayerId == remainingLoverId) {
                Lovers.lover2.MurderPlayer(Lovers.lover2);
            }
        }

        public static void setBountyHunterTarget(byte targetId) {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == targetId)
                    BountyHunter.target = player;
        }

        public static void vampireBiteNotification(byte targetId) {
            if (Vampire.vampire == null) return;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                if (player.PlayerId == targetId && !player.Data.IsDead) {
                        Vampire.bitten = player;
                }
            }
        }

        public static void vampireTryKill() {
            if (Vampire.vampire == null || Vampire.bitten == null) return;

            if (!Vampire.bitten.Data.IsDead && Helpers.handleMurderAttempt(Vampire.bitten, false)) {
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
                    if(!Jackal.canCreateSidekickFromImpostor && player.Data.IsImpostor) {
                        Jackal.fakeSidekick = player;
                        return;
                    }
                    Sidekick.sidekick = player;
                    player.RemoveInfected();

                    // Crewmate roles
                    if(player == Jester.jester) Jester.clearAndReload();
                    if(player == Mayor.mayor) Mayor.clearAndReload();
                    if(player == Engineer.engineer) Engineer.clearAndReload();
                    if(player == Sheriff.sheriff) Sheriff.clearAndReload();
                    if(player == Lighter.lighter) Lighter.clearAndReload();
                    if(player == Detective.detective) Detective.clearAndReload();
                    if(player == TimeMaster.timeMaster) TimeMaster.clearAndReload();
                    if(player == Medic.medic) Medic.clearAndReload();
                    if(player == Shifter.shifter) Shifter.clearAndReload();
                    if(player == Seer.seer) Seer.clearAndReload();
                    if(player == Spy.spy) Spy.clearAndReload();
                    if(player == Child.child) Child.clearAndReload();
                    if(player == Tracker.tracker) Tracker.clearAndReload();
                    if(player == BountyHunter.bountyHunter) BountyHunter.clearAndReload();
                    if(player == Snitch.snitch) Snitch.clearAndReload();
                    if(player == Swapper.swapper) Swapper.clearAndReload();

                    // Impostor roles
                    if(player == Morphling.morphling) Morphling.clearAndReload();
                    if(player == Camouflager.camouflager) Camouflager.clearAndReload();
                    if(player == Godfather.godfather) Godfather.clearAndReload();
                    if(player == Mafioso.mafioso) Mafioso.clearAndReload();
                    if(player == Janitor.janitor) Janitor.clearAndReload();
                    if(player == Vampire.vampire) Vampire.clearAndReload();

                    // The Sidekick stays a part of the lover couple!
                    
                    if (PlayerControl.LocalPlayer == null) return;
                    if (PlayerControl.LocalPlayer == player) {
                        //Only the Lover keeps his ImportantTextTask
                        Helpers.removeTasksFromPlayer(player, player != Lovers.lover1 && player != Lovers.lover2);
                        
                        var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
                        task.transform.SetParent(player.transform, false);
                        task.Text = "[00B4EBFF]Sidekick: Help your Jackal to kill everyone";
                        player.myTasks.Insert(0, task);
                    }

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
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer == player) {
                Helpers.removeTasksFromPlayer(player, true);
                var textTask = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
                textTask.transform.SetParent(player.transform, false);
                var getSidekickText = Jackal.canCreateSidekick ? " and recruit a Sidekick" : "";
                textTask.Text = $"[00B4EBFF]Jackal: Kill everyone{getSidekickText}"; 
                player.myTasks.Insert(0, textTask);
            }
            return;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class RPCHandlerPatch
    {
        static void Postfix(byte ACCJCEHMKLN, MessageReader HFPCBBHJIPJ)
        {
            byte packetId = ACCJCEHMKLN;
            MessageReader reader = HFPCBBHJIPJ;
            switch (packetId) {

                // Main Controls

                case (byte)CustomRPC.ResetVaribles:
                    RPCProcedure.resetVariables();
                    break;
                case (byte)CustomRPC.ForceEnd:
                    RPCProcedure.forceEnd();
                    break;
                case (byte)CustomRPC.SetRole:
                    byte roleId = HFPCBBHJIPJ.ReadByte();
                    byte playerId = HFPCBBHJIPJ.ReadByte();
                    RPCProcedure.setRole(roleId, playerId);
                    break;


                // Role functionality

                case (byte)CustomRPC.JesterBountyHunterWin:
                    RPCProcedure.jesterBountyHunterWin(HFPCBBHJIPJ.ReadByte());
                    break;
                case (byte)CustomRPC.EngineerFixLights:
                    RPCProcedure.engineerFixLights();
                    break;
                case (byte)CustomRPC.EngineerUsedRepair:
                    RPCProcedure.engineerUsedRepair();
                    break;
                case (byte)CustomRPC.JanitorClean:
                    RPCProcedure.janitorClean(HFPCBBHJIPJ.ReadByte());
                    break;
                case (byte)CustomRPC.SheriffKill:
                    RPCProcedure.sheriffKill(HFPCBBHJIPJ.ReadByte());
                    break;
                case (byte)CustomRPC.TimeMasterRewindTime:
                    RPCProcedure.timeMasterRewindTime();
                    break;
                case (byte)CustomRPC.MedicSetShielded:
                    RPCProcedure.medicSetShielded(HFPCBBHJIPJ.ReadByte());
                    break;
                case (byte)CustomRPC.ShieldedMurderAttempt:
                    RPCProcedure.shieldedMurderAttempt();
                    break;
                case (byte)CustomRPC.TimeMasterRevive:
                    RPCProcedure.timeMasterRevive(HFPCBBHJIPJ.ReadByte());
                    break;
                case (byte)CustomRPC.ShifterShift:
                    RPCProcedure.shifterShift(HFPCBBHJIPJ.ReadByte());
                    break;
                case (byte)CustomRPC.SwapperSwap:
                    byte playerId1 = HFPCBBHJIPJ.ReadByte();
                    byte playerId2 = HFPCBBHJIPJ.ReadByte();
                    RPCProcedure.swapperSwap(playerId1, playerId2);
                    break;
                case (byte)CustomRPC.SeerReveal:
                    byte targetId = HFPCBBHJIPJ.ReadByte();
                    byte targetOrMistakeId = HFPCBBHJIPJ.ReadByte();
                    RPCProcedure.seerReveal(targetId, targetOrMistakeId);
                    break;
                case (byte)CustomRPC.MorphlingMorph:
                    RPCProcedure.morphlingMorph(HFPCBBHJIPJ.ReadByte());
                    break;
                case (byte)CustomRPC.CamouflagerCamouflage:
                    RPCProcedure.camouflagerCamouflage();
                    break;
                case (byte)CustomRPC.LoverSuicide:
                    RPCProcedure.loverSuicide(HFPCBBHJIPJ.ReadByte());
                    break;
                case (byte)CustomRPC.SetBountyHunterTarget:
                    RPCProcedure.setBountyHunterTarget(HFPCBBHJIPJ.ReadByte());
                    break;
                case (byte)CustomRPC.VampireBiteNotification:
                    RPCProcedure.vampireBiteNotification(HFPCBBHJIPJ.ReadByte());
                    break;
                case (byte)CustomRPC.VampireTryKill:
                    RPCProcedure.vampireTryKill();
                    break;
                case (byte)CustomRPC.PlaceGarlic:
                    RPCProcedure.placeGarlic(HFPCBBHJIPJ.ReadBytesAndSize());
                    break;
                case (byte)CustomRPC.TrackerUsedTracker:
                    RPCProcedure.trackerUsedTracker(HFPCBBHJIPJ.ReadByte());
                    break;
                case (byte)CustomRPC.JackalKill:
                    RPCProcedure.jackalKill(HFPCBBHJIPJ.ReadByte());
                    break;
                case (byte)CustomRPC.SidekickKill:
                    RPCProcedure.sidekickKill(HFPCBBHJIPJ.ReadByte());
                    break;
                case (byte)CustomRPC.JackalCreatesSidekick:
                    RPCProcedure.jackalCreatesSidekick(HFPCBBHJIPJ.ReadByte());
                    break;
                case (byte)CustomRPC.SidekickPromotes:
                    RPCProcedure.sidekickPromotes();
                    break;
            }
        }
    }
}
