using HarmonyLib;
using Hazel;
using System;
using System.IO;
using System.Net.Http;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;
using Reactor.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;

namespace TheOtherRoles
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    static class HudManagerStartPatch
    {
        private static CustomButton engineerRepairButton;
        private static CustomButton janitorCleanButton;
        private static CustomButton sheriffKillButton;
        private static CustomButton timeMasterRewindTimeButton;
        private static CustomButton medicShieldButton;
        private static CustomButton shifterShiftButton;
        private static CustomButton seerRevealButton;
        private static CustomButton morphlingButton;
        private static CustomButton camouflagerButton;
        private static CustomButton spyButton;
        private static CustomButton trackerButton;
        private static CustomButton vampireKillButton;
        private static CustomButton garlicButton;
        private static CustomButton jackalKillButton;
        private static CustomButton sidekickKillButton;
        private static CustomButton jackalSidekickButton;

        public static void setCustomButtonCooldowns() {
            engineerRepairButton.MaxTimer = 0f;
            janitorCleanButton.MaxTimer = Janitor.cooldown;
            sheriffKillButton.MaxTimer = Sheriff.cooldown;
            timeMasterRewindTimeButton.MaxTimer = TimeMaster.cooldown;
            medicShieldButton.MaxTimer = 0f;
            shifterShiftButton.MaxTimer = Shifter.cooldown;
            seerRevealButton.MaxTimer = Seer.cooldown;
            morphlingButton.MaxTimer = Morphling.cooldown;
            camouflagerButton.MaxTimer = Camouflager.cooldown;
            spyButton.MaxTimer = Spy.cooldown;
            vampireKillButton.MaxTimer = Vampire.cooldown;
            trackerButton.MaxTimer = 0f;
            garlicButton.MaxTimer = 0f;
            jackalKillButton.MaxTimer = Jackal.cooldown;
            sidekickKillButton.MaxTimer = Sidekick.cooldown;
            jackalSidekickButton.MaxTimer = Jackal.createSidekickCooldown;

            spyButton.EffectDuration = Spy.duration;
            vampireKillButton.EffectDuration= Vampire.delay;
        }

        public static void Postfix(HudManager __instance)
        {
            // Engineer Repair
            engineerRepairButton = new CustomButton(
                () => {
                    engineerRepairButton.Timer = 0f;

                    MessageWriter usedRepairWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EngineerUsedRepair, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(usedRepairWriter);
                    RPCProcedure.engineerUsedRepair();
 
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks) {
                        if (task.TaskType == TaskTypes.FixLights) {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EngineerFixLights, Hazel.SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.engineerFixLights();
                        } else if (task.TaskType == TaskTypes.RestoreOxy) {
                            ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                            ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                        } else if (task.TaskType == TaskTypes.ResetReactor) {
                            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16);
                        } else if (task.TaskType == TaskTypes.ResetSeismic) {
                            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 16);
                        } else if (task.TaskType == TaskTypes.FixComms) {
                            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                        }
                    }
                },
                () => { return Engineer.engineer != null && Engineer.engineer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    bool sabotageActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                        if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms)
                            sabotageActive = true;
                    return sabotageActive && !Engineer.usedRepair && PlayerControl.LocalPlayer.CanMove;
                },
                () => {},
                Engineer.getButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance
            );

            // Janitor Clean
            janitorCleanButton = new CustomButton(
                () => {
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask)) {
                        if (collider2D.tag == "DeadBody")
                        {
                            DeadBody component = collider2D.GetComponent<DeadBody>();
                            if (component && !component.Reported)
                            {
                                Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                                Vector2 truePosition2 = component.TruePosition;
                                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                                {
                                    GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
                                    
                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.JanitorClean, Hazel.SendOption.Reliable, -1);
                                    writer.Write(playerInfo.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RPCProcedure.janitorClean(playerInfo.PlayerId);
                                    janitorCleanButton.Timer = janitorCleanButton.MaxTimer;

                                    break;
                                }
                            }
                        }
                    }
                },
                () => { return Janitor.janitor != null && Janitor.janitor == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return __instance.ReportButton.renderer.color == Palette.EnabledColor  && PlayerControl.LocalPlayer.CanMove; },
                () => { janitorCleanButton.Timer = janitorCleanButton.MaxTimer;},
                Janitor.getButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance
            );

            // Sheriff Kill
            sheriffKillButton = new CustomButton(
                () => {
                    if (Medic.shielded != null && Medic.shielded == Sheriff.currentTarget) {
                        MessageWriter attemptWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(attemptWriter);
                        RPCProcedure.shieldedMurderAttempt();
                        return;    
                    }

                    byte targetId = 0;
                    if (Sheriff.currentTarget.Data.IsImpostor || 
                        Sheriff.currentTarget == Jackal.jackal || 
                        Sheriff.currentTarget == Sidekick.sidekick || 
                        (Sheriff.jesterCanDieToSheriff && Jester.jester != null && Jester.jester == Sheriff.currentTarget)) {
                        targetId = Sheriff.currentTarget.PlayerId;
                    }
                    else {
                        targetId = PlayerControl.LocalPlayer.PlayerId;
                    }
                    MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SheriffKill, Hazel.SendOption.Reliable, -1);
                    killWriter.Write(targetId);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCProcedure.sheriffKill(targetId);

                    sheriffKillButton.Timer = sheriffKillButton.MaxTimer; 
                    Sheriff.currentTarget = null;
                },
                () => { return Sheriff.sheriff != null && Sheriff.sheriff == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Sheriff.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { sheriffKillButton.Timer = sheriffKillButton.MaxTimer;},
                __instance.KillButton.renderer.sprite,
                new Vector3(-1.3f, 0, 0),
                __instance
            );

            // Time Master Rewind Time
            timeMasterRewindTimeButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TimeMasterRewindTime, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.timeMasterRewindTime();
                    timeMasterRewindTimeButton.Timer = timeMasterRewindTimeButton.MaxTimer;
                },
                () => { return TimeMaster.timeMaster != null && TimeMaster.timeMaster == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { timeMasterRewindTimeButton.Timer = timeMasterRewindTimeButton.MaxTimer;},
                TimeMaster.getButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance
            );

            // Medic Shield
            medicShieldButton = new CustomButton(
                () => {
                    medicShieldButton.Timer = 0f;
 
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MedicSetShielded, Hazel.SendOption.Reliable, -1);
                    writer.Write(Medic.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.medicSetShielded(Medic.currentTarget.PlayerId);
                },
                () => { return Medic.medic != null && Medic.medic == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return !Medic.usedShield && Medic.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => {},
                Medic.getButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance
            );

            
            // Shifter shift
            shifterShiftButton = new CustomButton(
                () => {
                    shifterShiftButton.Timer = shifterShiftButton.MaxTimer;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShifterShift, Hazel.SendOption.Reliable, -1);
                    writer.Write(Shifter.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    
                    RPCProcedure.shifterShift(Shifter.currentTarget.PlayerId);
                },
                () => { return Shifter.shifter != null && Shifter.shifter == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Shifter.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { shifterShiftButton.Timer = shifterShiftButton.MaxTimer; },
                Shifter.getButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance
            );

            // Seer reveal
            seerRevealButton = new CustomButton(
                () => {
                    seerRevealButton.Timer = seerRevealButton.MaxTimer;

                    PlayerControl targetOrMistake = Seer.currentTarget;
                    if (rnd.Next(1, 101) > Seer.chanceOfSeeingRight) {
                        var players = PlayerControl.AllPlayerControls.ToArray().ToList();
                        players.RemoveAll(p => p != null && (p.PlayerId == Seer.seer.PlayerId || p.PlayerId == Seer.currentTarget.PlayerId));
                        int index = rnd.Next(0, players.Count);

                        if (players.Count != 0 && players[index] != null)
                            targetOrMistake = players[index];
                    }

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SeerReveal, Hazel.SendOption.Reliable, -1);
                    writer.Write(Seer.currentTarget.PlayerId);
                    writer.Write(targetOrMistake.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    
                    RPCProcedure.seerReveal(Seer.currentTarget.PlayerId, targetOrMistake.PlayerId);
                },
                () => { return Seer.seer != null && Seer.seer == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Seer.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => {},
                Seer.getButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance
            );

            // Morphling morph
            morphlingButton = new CustomButton(
                () => {
                    if (Morphling.sampledTarget != null) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MorphlingMorph, Hazel.SendOption.Reliable, -1);
                        writer.Write(Morphling.sampledTarget.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.morphlingMorph(Morphling.sampledTarget.PlayerId);
                        Morphling.sampledTarget = null;
                        morphlingButton.HasEffect = true; // Trigger effect on this click
                    } else if (Morphling.currentTarget != null) {
                        Morphling.sampledTarget = Morphling.currentTarget;
                        morphlingButton.Sprite = Morphling.getMorphSprite();
                        morphlingButton.HasEffect = false; // Block effect on this click
                    }
                },
                () => { return Morphling.morphling != null && Morphling.morphling == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return (Morphling.currentTarget || Morphling.sampledTarget) && PlayerControl.LocalPlayer.CanMove; },
                () => { 
                    morphlingButton.Timer = morphlingButton.MaxTimer;
                    morphlingButton.Sprite = Morphling.getSampleSprite();
                    morphlingButton.isEffectActive = false;
                    morphlingButton.killButtonManager.TimerText.Color = Palette.EnabledColor;
                    Morphling.sampledTarget = null;
                },
                Morphling.getSampleSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                true,
                10f,
                () => {
                    morphlingButton.Timer = morphlingButton.MaxTimer;
                    morphlingButton.Sprite = Morphling.getSampleSprite();
                }
            );

            // Camouflager camouflage
            camouflagerButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CamouflagerCamouflage, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.camouflagerCamouflage();
                },
                () => { return Camouflager.camouflager != null && Camouflager.camouflager == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => {
                    camouflagerButton.Timer = camouflagerButton.MaxTimer;
                    camouflagerButton.isEffectActive = false;
                    camouflagerButton.killButtonManager.TimerText.Color = Palette.EnabledColor;
                },
                Camouflager.getButtonSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                true,
                10f,
                () => { camouflagerButton.Timer = camouflagerButton.MaxTimer; }
            );

            // Spy button
            spyButton = new CustomButton(
                () => {
                    Spy.spyTimer = Spy.duration;
                },
                () => { return Spy.spy != null && Spy.spy == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => {
                    spyButton.Timer = spyButton.MaxTimer;
                    spyButton.isEffectActive = false;
                    spyButton.killButtonManager.TimerText.Color = Palette.EnabledColor;
                },
                Spy.getButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                true,
                0f,
                () => {
                    spyButton.Timer = spyButton.MaxTimer;
                }
            );

            // Tracker button
            trackerButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TrackerUsedTracker, Hazel.SendOption.Reliable, -1);
                    writer.Write(Tracker.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.trackerUsedTracker(Tracker.currentTarget.PlayerId);
                },
                () => { return Tracker.tracker != null && Tracker.tracker == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove && Tracker.currentTarget != null && !Tracker.usedTracker; },
                () => { },
                Tracker.getButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance
            );

            vampireKillButton = new CustomButton(
                () => {
                    if (Helpers.handleMurderAttempt(Vampire.currentTarget)) {
                        if (Vampire.targetNearGarlic) {
			                PlayerControl.LocalPlayer.RpcMurderPlayer(Vampire.currentTarget);
                            vampireKillButton.HasEffect = false; // Block effect on this click
                            vampireKillButton.Timer = vampireKillButton.MaxTimer;
                        } else {
                            Vampire.bitten = Vampire.currentTarget;
                            Reactor.Coroutines.Start(Vampire.killWithDelay());
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VampireBiteNotification, Hazel.SendOption.Reliable, -1);
                            writer.Write(Vampire.bitten.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.vampireBiteNotification(Vampire.bitten.PlayerId); 
                            vampireKillButton.HasEffect = true; // Trigger effect on this click
                        }
                    } else {
                        vampireKillButton.HasEffect = false; // Block effect if no action was fired
                    }
                },
                () => { return Vampire.vampire != null && Vampire.vampire == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (Vampire.targetNearGarlic && Vampire.canKillNearGarlics)
                        vampireKillButton.killButtonManager.renderer.sprite = __instance.KillButton.renderer.sprite;
                    else
                        vampireKillButton.killButtonManager.renderer.sprite = Vampire.getButtonSprite();
                    return Vampire.currentTarget != null && PlayerControl.LocalPlayer.CanMove && (!Vampire.targetNearGarlic || Vampire.canKillNearGarlics);
                },
                () => {
                    vampireKillButton.Timer = vampireKillButton.MaxTimer;
                    vampireKillButton.isEffectActive = false;
                    vampireKillButton.killButtonManager.TimerText.Color = Palette.EnabledColor;
                },
                Vampire.getButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                false,
                0f,
                () => {
                    vampireKillButton.Timer = vampireKillButton.MaxTimer;
                }
            );

            garlicButton = new CustomButton(
                () => {
                    Vampire.localPlacedGarlic = true;
                    var pos = PlayerControl.LocalPlayer.transform.position;
                    byte[] buff = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0*sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1*sizeof(float), sizeof(float));

                    MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlaceGarlic, Hazel.SendOption.Reliable);
                    writer.WriteBytesAndSize(buff);
                    writer.EndMessage();
                    RPCProcedure.placeGarlic(buff); 
                },
                () => { return !Vampire.localPlacedGarlic && !PlayerControl.LocalPlayer.Data.IsDead && Vampire.garlicsActive; },
                () => { return PlayerControl.LocalPlayer.CanMove && !Vampire.localPlacedGarlic; },
                () => { },
                Vampire.getGarlicButtonSprite(),
                Vector3.zero,
                __instance,
                true
            );

            
            // Jackal Sidekick Button
            jackalSidekickButton = new CustomButton(
                () => {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.JackalCreatesSidekick, Hazel.SendOption.Reliable, -1);
                    writer.Write(Jackal.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.jackalCreatesSidekick(Jackal.currentTarget.PlayerId);
                },
                () => { return Jackal.canCreateSidekick && Sidekick.sidekick == null && Jackal.fakeSidekick == null && Jackal.jackal != null && Jackal.jackal == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Sidekick.sidekick == null && Jackal.fakeSidekick == null && Jackal.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { jackalSidekickButton.Timer = jackalSidekickButton.MaxTimer;},
                Jackal.getSidekickButtonSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance
            );

            // Jackal Kill
            jackalKillButton = new CustomButton(
                () => {
                    if (!Helpers.handleMurderAttempt(Jackal.currentTarget)) return;
                    byte targetId = Jackal.currentTarget.PlayerId;
                    MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.JackalKill, Hazel.SendOption.Reliable, -1);
                    killWriter.Write(targetId);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCProcedure.jackalKill(targetId);
                    jackalKillButton.Timer = jackalKillButton.MaxTimer; 
                    Jackal.currentTarget = null;
                },
                () => { return Jackal.jackal != null && Jackal.jackal == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Jackal.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { jackalKillButton.Timer = jackalKillButton.MaxTimer;},
                __instance.KillButton.renderer.sprite,
                new Vector3(-1.3f, 0, 0),
                __instance
            );
            
            // Sidekick Kill
            sidekickKillButton = new CustomButton(
                () => {
                    if (!Helpers.handleMurderAttempt(Sidekick.currentTarget)) return;
                    byte targetId = Sidekick.currentTarget.PlayerId;
                    MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickKill, Hazel.SendOption.Reliable, -1);
                    killWriter.Write(targetId);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCProcedure.sidekickKill(targetId);

                    sidekickKillButton.Timer = sidekickKillButton.MaxTimer; 
                    Sidekick.currentTarget = null;
                },
                () => { return Sidekick.canKill && Sidekick.sidekick != null && Sidekick.sidekick == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Sidekick.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { sidekickKillButton.Timer = sidekickKillButton.MaxTimer;},
                __instance.KillButton.renderer.sprite,
                new Vector3(-1.3f, 0, 0),
                __instance
            );
        }
    }
}