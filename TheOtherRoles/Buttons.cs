using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using TheOtherRoles.Objects;
using TheOtherRoles.Roles;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    internal static class HudManagerStartPatch
    {
        private static CustomButton engineerRepairButton;
        private static CustomButton janitorCleanButton;
        private static CustomButton sheriffKillButton;
        private static CustomButton timeMasterShieldButton;
        private static CustomButton medicShieldButton;
        private static CustomButton shifterShiftButton;
        private static CustomButton morphlingButton;
        private static CustomButton camouflagerButton;
        private static CustomButton hackerButton;
        private static CustomButton trackerButton;
        private static CustomButton vampireKillButton;
        private static CustomButton garlicButton;
        private static CustomButton jackalKillButton;
        private static CustomButton sidekickKillButton;
        private static CustomButton jackalSidekickButton;
        private static CustomButton lighterButton;
        private static CustomButton eraserButton;
        private static CustomButton placeJackInTheBoxButton;
        private static CustomButton lightsOutButton;
        public static CustomButton cleanerCleanButton;
        public static CustomButton warlockCurseButton;
        private static CustomButton securityGuardButton;
        private static CustomButton arsonistButton;
        private static TMP_Text securityGuardButtonScrewsText;

        public static void SetCustomButtonCooldowns()
        {
            engineerRepairButton.maxTimer = 0f;
            janitorCleanButton.maxTimer = Janitor.cooldown;
            sheriffKillButton.maxTimer = Sheriff.cooldown;
            timeMasterShieldButton.maxTimer = TimeMaster.cooldown;
            medicShieldButton.maxTimer = 0f;
            shifterShiftButton.maxTimer = 0f;
            morphlingButton.maxTimer = Morphling.cooldown;
            camouflagerButton.maxTimer = Camouflager.cooldown;
            hackerButton.maxTimer = Hacker.cooldown;
            vampireKillButton.maxTimer = Vampire.cooldown;
            trackerButton.maxTimer = 0f;
            garlicButton.maxTimer = 0f;
            jackalKillButton.maxTimer = Jackal.cooldown;
            sidekickKillButton.maxTimer = Sidekick.cooldown;
            jackalSidekickButton.maxTimer = Jackal.createSidekickCooldown;
            lighterButton.maxTimer = Lighter.cooldown;
            eraserButton.maxTimer = Eraser.cooldown;
            placeJackInTheBoxButton.maxTimer = Trickster.placeBoxCooldown;
            lightsOutButton.maxTimer = Trickster.lightsOutCooldown;
            cleanerCleanButton.maxTimer = Cleaner.cooldown;
            warlockCurseButton.maxTimer = Warlock.cooldown;
            securityGuardButton.maxTimer = SecurityGuard.cooldown;
            arsonistButton.maxTimer = Arsonist.cooldown;

            timeMasterShieldButton.effectDuration = TimeMaster.shieldDuration;
            hackerButton.effectDuration = Hacker.duration;
            vampireKillButton.effectDuration = Vampire.delay;
            lighterButton.effectDuration = Lighter.duration;
            camouflagerButton.effectDuration = Camouflager.duration;
            morphlingButton.effectDuration = Morphling.duration;
            lightsOutButton.effectDuration = Trickster.lightsOutDuration;
            arsonistButton.effectDuration = Arsonist.duration;

            // Already set the timer to the max, as the button is enabled during the game and not available at the start
            lightsOutButton.timer = lightsOutButton.maxTimer;
        }

        public static void ResetTimeMasterButton()
        {
            timeMasterShieldButton.timer = timeMasterShieldButton.maxTimer;
            timeMasterShieldButton.isEffectActive = false;
            timeMasterShieldButton.killButtonManager.TimerText.color = Palette.EnabledColor;
        }

        // TODO: maybe move buttons to Role instances?
        public static void Postfix(HudManager __instance)
        {
            // Engineer Repair
            engineerRepairButton = new CustomButton(
                () =>
                {
                    engineerRepairButton.timer = 0f;

                    var usedRepairWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.EngineerUsedRepair, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(usedRepairWriter);
                    RPCProcedure.EngineerUsedRepair();

                    foreach (var task in PlayerControl.LocalPlayer.myTasks)
                        switch (task.TaskType)
                        {
                            case TaskTypes.FixLights:
                            {
                                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                                    (byte) CustomRPC.EngineerFixLights, SendOption.Reliable, -1);
                                AmongUsClient.Instance.FinishRpcImmediately(writer);
                                RPCProcedure.EngineerFixLights();
                                break;
                            }
                            case TaskTypes.RestoreOxy:
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                                break;
                            case TaskTypes.ResetReactor:
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16);
                                break;
                            case TaskTypes.ResetSeismic:
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 16);
                                break;
                            case TaskTypes.FixComms:
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                                break;
                            case TaskTypes.StopCharles:
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
                                break;
                        }
                },
                () => Engineer.Instance.player == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead,
                () =>
                {
                    var sabotageActive = false;
                    foreach (var task in PlayerControl.LocalPlayer.myTasks)
                        if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy ||
                            task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic ||
                            task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles)
                            sabotageActive = true;
                    return sabotageActive && !Engineer.usedRepair && PlayerControl.LocalPlayer.CanMove;
                },
                () => { },
                Engineer.GetButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q
            );

            // Janitor Clean
            janitorCleanButton = new CustomButton(
                () =>
                {
                    foreach (var collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(),
                        PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                        if (collider2D.CompareTag("DeadBody"))
                        {
                            var component = collider2D.GetComponent<DeadBody>();
                            if (!component || component.Reported) continue;
                            var truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                            var truePosition2 = component.TruePosition;
                            if (!(Vector2.Distance(truePosition2, truePosition) <=
                                  PlayerControl.LocalPlayer.MaxReportDistance) || !PlayerControl.LocalPlayer.CanMove ||
                                PhysicsHelpers.AnythingBetween(truePosition, truePosition2,
                                    Constants.ShipAndObjectsMask, false)) continue;
                            var playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                            var writer = AmongUsClient.Instance.StartRpcImmediately(
                                PlayerControl.LocalPlayer.NetId, (byte) CustomRPC.CleanBody,
                                SendOption.Reliable, -1);
                            writer.Write(playerInfo.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.CleanBody(playerInfo.PlayerId);
                            janitorCleanButton.timer = janitorCleanButton.maxTimer;

                            break;
                        }
                },
                () => Janitor.Instance.player == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead,
                () => __instance.ReportButton.renderer.color == Palette.EnabledColor &&
                      PlayerControl.LocalPlayer.CanMove,
                () => { janitorCleanButton.timer = janitorCleanButton.maxTimer; },
                Janitor.GetButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q
            );

            // Sheriff Kill
            sheriffKillButton = new CustomButton(
                () =>
                {
                    if (Medic.shielded != null && Medic.shielded == Sheriff.currentTarget)
                    {
                        var attemptWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.ShieldedMurderAttempt, SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(attemptWriter);
                        RPCProcedure.ShieldedMurderAttempt();
                        return;
                    }

                    byte targetId;
                    if (Sheriff.currentTarget.Data.IsImpostor &&
                        (Sheriff.currentTarget != Mini.Instance.player || Mini.IsGrownUp()) ||
                        Sheriff.spyCanDieToSheriff && Spy.Instance.player == Sheriff.currentTarget ||
                        Sheriff.canKillNeutrals && (Arsonist.Instance.player == Sheriff.currentTarget ||
                                                    Jester.Instance.player == Sheriff.currentTarget) ||
                        Jackal.Instance.player == Sheriff.currentTarget ||
                        Sidekick.Instance.player == Sheriff.currentTarget)
                        targetId = Sheriff.currentTarget.PlayerId;
                    else
                        targetId = PlayerControl.LocalPlayer.PlayerId;
                    var killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.SheriffKill, SendOption.Reliable, -1);
                    killWriter.Write(targetId);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCProcedure.SheriffKill(targetId);

                    sheriffKillButton.timer = sheriffKillButton.maxTimer;
                    Sheriff.currentTarget = null;
                },
                () => Sheriff.Instance.player == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead,
                () => Sheriff.currentTarget && PlayerControl.LocalPlayer.CanMove,
                () => { sheriffKillButton.timer = sheriffKillButton.maxTimer; },
                __instance.KillButton.renderer.sprite,
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q
            );

            // Time Master Rewind Time
            timeMasterShieldButton = new CustomButton(
                () =>
                {
                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.TimeMasterShield, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.TimeMasterShield();
                },
                () => TimeMaster.Instance.player == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead,
                () => PlayerControl.LocalPlayer.CanMove,
                () =>
                {
                    timeMasterShieldButton.timer = timeMasterShieldButton.maxTimer;
                    timeMasterShieldButton.isEffectActive = false;
                    timeMasterShieldButton.killButtonManager.TimerText.color = Palette.EnabledColor;
                },
                TimeMaster.GetButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q,
                true,
                TimeMaster.shieldDuration,
                () => { timeMasterShieldButton.timer = timeMasterShieldButton.maxTimer; }
            );

            // Medic Shield
            medicShieldButton = new CustomButton(
                () =>
                {
                    medicShieldButton.timer = 0f;

                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        Medic.setShieldAfterMeeting
                            ? (byte) CustomRPC.SetFutureShielded
                            : (byte) CustomRPC.MedicSetShielded, SendOption.Reliable, -1);
                    writer.Write(Medic.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    if (Medic.setShieldAfterMeeting)
                        RPCProcedure.SetFutureShielded(Medic.currentTarget.PlayerId);
                    else
                        RPCProcedure.MedicSetShielded(Medic.currentTarget.PlayerId);
                },
                () => Medic.Instance.player == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead,
                () => !Medic.usedShield && Medic.currentTarget && PlayerControl.LocalPlayer.CanMove,
                () => { },
                Medic.GetButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q
            );


            // Shifter shift
            shifterShiftButton = new CustomButton(
                () =>
                {
                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.SetFutureShifted, SendOption.Reliable, -1);
                    writer.Write(Shifter.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.SetFutureShifted(Shifter.currentTarget.PlayerId);
                },
                () => Shifter.Instance.player == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead,
                () => Shifter.currentTarget && Shifter.futureShift == null && PlayerControl.LocalPlayer.CanMove,
                () => { },
                Shifter.GetButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q
            );

            // Morphling morph
            morphlingButton = new CustomButton(
                () =>
                {
                    if (Morphling.sampledTarget != null)
                    {
                        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.MorphlingMorph, SendOption.Reliable, -1);
                        writer.Write(Morphling.sampledTarget.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.MorphlingMorph(Morphling.sampledTarget.PlayerId);
                        Morphling.sampledTarget = null;
                        morphlingButton.effectDuration = Morphling.duration;
                    }
                    else if (Morphling.currentTarget != null)
                    {
                        Morphling.sampledTarget = Morphling.currentTarget;
                        morphlingButton.sprite = Morphling.GetMorphSprite();
                        morphlingButton.effectDuration = 1f;
                    }
                },
                () => Morphling.Instance.player == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead,
                () => (Morphling.currentTarget || Morphling.sampledTarget) && PlayerControl.LocalPlayer.CanMove,
                () =>
                {
                    morphlingButton.timer = morphlingButton.maxTimer;
                    morphlingButton.sprite = Morphling.GetSampleSprite();
                    morphlingButton.isEffectActive = false;
                    morphlingButton.killButtonManager.TimerText.color = Palette.EnabledColor;
                    Morphling.sampledTarget = null;
                },
                Morphling.GetSampleSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                KeyCode.F,
                true,
                Morphling.duration,
                () =>
                {
                    if (Morphling.sampledTarget != null) return;
                    morphlingButton.timer = morphlingButton.maxTimer;
                    morphlingButton.sprite = Morphling.GetSampleSprite();
                }
            );

            // Camouflager camouflage
            camouflagerButton = new CustomButton(
                () =>
                {
                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.CamouflagerCamouflage, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.CamouflagerCamouflage();
                },
                () => Camouflager.Instance.player == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () => PlayerControl.LocalPlayer.CanMove,
                () =>
                {
                    camouflagerButton.timer = camouflagerButton.maxTimer;
                    camouflagerButton.isEffectActive = false;
                    camouflagerButton.killButtonManager.TimerText.color = Palette.EnabledColor;
                },
                Camouflager.GetButtonSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                KeyCode.F,
                true,
                Camouflager.duration,
                () => { camouflagerButton.timer = camouflagerButton.maxTimer; }
            );

            // Hacker button
            hackerButton = new CustomButton(
                () => { Hacker.hackerTimer = Hacker.duration; },
                () => Hacker.Instance.player == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead,
                () => PlayerControl.LocalPlayer.CanMove,
                () =>
                {
                    hackerButton.timer = hackerButton.maxTimer;
                    hackerButton.isEffectActive = false;
                    hackerButton.killButtonManager.TimerText.color = Palette.EnabledColor;
                },
                Hacker.GetButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q,
                true,
                0f,
                () => { hackerButton.timer = hackerButton.maxTimer; }
            );

            // Tracker button
            trackerButton = new CustomButton(
                () =>
                {
                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.TrackerUsedTracker, SendOption.Reliable, -1);
                    writer.Write(Tracker.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.TrackerUsedTracker(Tracker.currentTarget.PlayerId);
                },
                () => Tracker.Instance.player == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead,
                () => PlayerControl.LocalPlayer.CanMove && Tracker.currentTarget != null && !Tracker.usedTracker,
                () =>
                {
                    if (Tracker.resetTargetAfterMeeting) Tracker.ResetTracked();
                },
                Tracker.GetButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q
            );

            vampireKillButton = new CustomButton(
                () =>
                {
                    if (Helpers.HandleMurderAttempt(Vampire.currentTarget))
                    {
                        if (Vampire.targetNearGarlic)
                        {
                            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                                (byte) CustomRPC.UncheckedMurderPlayer, SendOption.Reliable, -1);
                            writer.Write(Vampire.Instance.player.PlayerId);
                            writer.Write(Vampire.currentTarget.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.UncheckedMurderPlayer(Vampire.Instance.player.PlayerId,
                                Vampire.currentTarget.PlayerId);

                            vampireKillButton.hasEffect = false; // Block effect on this click
                            vampireKillButton.timer = vampireKillButton.maxTimer;
                        }
                        else
                        {
                            Vampire.bitten = Vampire.currentTarget;
                            // Notify players about bitten
                            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                                (byte) CustomRPC.VampireSetBitten, SendOption.Reliable, -1);
                            writer.Write(Vampire.bitten.PlayerId);
                            writer.Write(0);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.VampireSetBitten(Vampire.bitten.PlayerId, 0);

                            HudManager.Instance.StartCoroutine(Effects.Lerp(Vampire.delay, new Action<float>(p =>
                            {
                                // Delayed action
                                if (!(Math.Abs(p - 1f) < 0.1f)) return;
                                if (Vampire.bitten != null && !Vampire.bitten.Data.IsDead &&
                                    Helpers.HandleMurderAttempt(Vampire.bitten))
                                {
                                    // Perform kill
                                    var killWriter = AmongUsClient.Instance.StartRpcImmediately(
                                        PlayerControl.LocalPlayer.NetId, (byte) CustomRPC.VampireTryKill,
                                        SendOption.Reliable, -1);
                                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                                    RPCProcedure.VampireTryKill();
                                }
                                else
                                {
                                    // Notify players about clearing bitten
                                    var writer2 = AmongUsClient.Instance.StartRpcImmediately(
                                        PlayerControl.LocalPlayer.NetId, (byte) CustomRPC.VampireSetBitten,
                                        SendOption.Reliable, -1);
                                    writer2.Write(byte.MaxValue);
                                    writer2.Write(byte.MaxValue);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer2);
                                    RPCProcedure.VampireSetBitten(byte.MaxValue, byte.MaxValue);
                                }
                            })));

                            vampireKillButton.hasEffect = true; // Trigger effect on this click
                        }
                    }
                    else
                    {
                        vampireKillButton.hasEffect = false; // Block effect if no action was fired
                    }
                },
                () => Vampire.Instance.player == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead,
                () =>
                {
                    if (Vampire.targetNearGarlic && Vampire.canKillNearGarlics)
                        vampireKillButton.killButtonManager.renderer.sprite = __instance.KillButton.renderer.sprite;
                    else
                        vampireKillButton.killButtonManager.renderer.sprite = Vampire.GetButtonSprite();
                    return Vampire.currentTarget != null && PlayerControl.LocalPlayer.CanMove &&
                           (!Vampire.targetNearGarlic || Vampire.canKillNearGarlics);
                },
                () =>
                {
                    vampireKillButton.timer = vampireKillButton.maxTimer;
                    vampireKillButton.isEffectActive = false;
                    vampireKillButton.killButtonManager.TimerText.color = Palette.EnabledColor;
                },
                Vampire.GetButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q,
                false,
                0f,
                () => { vampireKillButton.timer = vampireKillButton.maxTimer; }
            );

            garlicButton = new CustomButton(
                () =>
                {
                    Vampire.localPlacedGarlic = true;
                    var pos = PlayerControl.LocalPlayer.transform.position;
                    var buff = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                    var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.PlaceGarlic, SendOption.Reliable);
                    writer.WriteBytesAndSize(buff);
                    writer.EndMessage();
                    RPCProcedure.PlaceGarlic(buff);
                },
                () => !Vampire.localPlacedGarlic && !PlayerControl.LocalPlayer.Data.IsDead &&
                      Vampire.garlicsActive,
                () => PlayerControl.LocalPlayer.CanMove && !Vampire.localPlacedGarlic,
                () => { },
                Vampire.GetGarlicButtonSprite(),
                Vector3.zero,
                __instance,
                null,
                true
            );


            // Jackal Sidekick Button
            jackalSidekickButton = new CustomButton(
                () =>
                {
                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.JackalCreatesSidekick, SendOption.Reliable, -1);
                    writer.Write(Jackal.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.JackalCreatesSidekick(Jackal.currentTarget.PlayerId);
                },
                () => Jackal.canCreateSidekick &&
                      Jackal.Instance.player == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead,
                () => Jackal.canCreateSidekick && Jackal.currentTarget != null &&
                      PlayerControl.LocalPlayer.CanMove,
                () => { jackalSidekickButton.timer = jackalSidekickButton.maxTimer; },
                Jackal.GetSidekickButtonSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                KeyCode.F
            );

            // Jackal Kill
            jackalKillButton = new CustomButton(
                () =>
                {
                    if (!Helpers.HandleMurderAttempt(Jackal.currentTarget)) return;
                    var targetId = Jackal.currentTarget.PlayerId;
                    var killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.JackalKill, SendOption.Reliable, -1);
                    killWriter.Write(targetId);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCProcedure.JackalKill(targetId);
                    jackalKillButton.timer = jackalKillButton.maxTimer;
                    Jackal.currentTarget = null;
                },
                () => Jackal.Instance.player == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead,
                () => Jackal.currentTarget && PlayerControl.LocalPlayer.CanMove,
                () => { jackalKillButton.timer = jackalKillButton.maxTimer; },
                __instance.KillButton.renderer.sprite,
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q
            );

            // Sidekick Kill
            sidekickKillButton = new CustomButton(
                () =>
                {
                    if (!Helpers.HandleMurderAttempt(Sidekick.currentTarget)) return;
                    var targetId = Sidekick.currentTarget.PlayerId;
                    var killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.SidekickKill, SendOption.Reliable, -1);
                    killWriter.Write(targetId);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCProcedure.SidekickKill(targetId);

                    sidekickKillButton.timer = sidekickKillButton.maxTimer;
                    Sidekick.currentTarget = null;
                },
                () => Sidekick.canKill &&
                      Sidekick.Instance.player == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead,
                () => Sidekick.currentTarget && PlayerControl.LocalPlayer.CanMove,
                () => { sidekickKillButton.timer = sidekickKillButton.maxTimer; },
                __instance.KillButton.renderer.sprite,
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q
            );

            // Lighter light
            lighterButton = new CustomButton(
                () => { Lighter.lighterTimer = Lighter.duration; },
                () => Lighter.Instance.player == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead,
                () => PlayerControl.LocalPlayer.CanMove,
                () =>
                {
                    lighterButton.timer = lighterButton.maxTimer;
                    lighterButton.isEffectActive = false;
                    lighterButton.killButtonManager.TimerText.color = Palette.EnabledColor;
                },
                Lighter.GetButtonSprite(),
                new Vector3(-1.3f, 0f, 0f),
                __instance,
                KeyCode.Q,
                true,
                Lighter.duration,
                () => { lighterButton.timer = lighterButton.maxTimer; }
            );

            // Eraser erase button
            eraserButton = new CustomButton(
                () =>
                {
                    eraserButton.maxTimer += 10;
                    eraserButton.timer = eraserButton.maxTimer;

                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.SetFutureErased, SendOption.Reliable, -1);
                    writer.Write(Eraser.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.SetFutureErased(Eraser.currentTarget.PlayerId);
                },
                () => Eraser.Instance.player == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead,
                () => PlayerControl.LocalPlayer.CanMove && Eraser.currentTarget != null,
                () => { eraserButton.timer = eraserButton.maxTimer; },
                Eraser.GetButtonSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                KeyCode.F
            );

            placeJackInTheBoxButton = new CustomButton(
                () =>
                {
                    placeJackInTheBoxButton.timer = placeJackInTheBoxButton.maxTimer;

                    var pos = PlayerControl.LocalPlayer.transform.position;
                    var buff = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                    var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.PlaceJackInTheBox, SendOption.Reliable);
                    writer.WriteBytesAndSize(buff);
                    writer.EndMessage();
                    RPCProcedure.PlaceJackInTheBox(buff);
                },
                () => Trickster.Instance.player == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead && !JackInTheBox.HasJackInTheBoxLimitReached(),
                () => PlayerControl.LocalPlayer.CanMove && !JackInTheBox.HasJackInTheBoxLimitReached(),
                () => { placeJackInTheBoxButton.timer = placeJackInTheBoxButton.maxTimer; },
                Trickster.GetPlaceBoxButtonSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                KeyCode.F
            );

            lightsOutButton = new CustomButton(
                () =>
                {
                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.LightsOut, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.LightsOut();
                },
                () => Trickster.Instance.player == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead && JackInTheBox.HasJackInTheBoxLimitReached() &&
                      JackInTheBox.boxesConvertedToVents,
                () => PlayerControl.LocalPlayer.CanMove && JackInTheBox.HasJackInTheBoxLimitReached() &&
                      JackInTheBox.boxesConvertedToVents,
                () =>
                {
                    lightsOutButton.timer = lightsOutButton.maxTimer;
                    lightsOutButton.isEffectActive = false;
                    lightsOutButton.killButtonManager.TimerText.color = Palette.EnabledColor;
                },
                Trickster.GetLightsOutButtonSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                KeyCode.F,
                true,
                Trickster.lightsOutDuration,
                () => { lightsOutButton.timer = lightsOutButton.maxTimer; }
            );
            // Cleaner Clean
            cleanerCleanButton = new CustomButton(
                () =>
                {
                    foreach (var collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(),
                        PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                        if (collider2D.CompareTag("DeadBody"))
                        {
                            var component = collider2D.GetComponent<DeadBody>();
                            if (!component || component.Reported) continue;
                            var truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                            var truePosition2 = component.TruePosition;
                            if (!(Vector2.Distance(truePosition2, truePosition) <=
                                  PlayerControl.LocalPlayer.MaxReportDistance) || !PlayerControl.LocalPlayer.CanMove ||
                                PhysicsHelpers.AnythingBetween(truePosition, truePosition2,
                                    Constants.ShipAndObjectsMask, false)) continue;
                            var playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                            var writer = AmongUsClient.Instance.StartRpcImmediately(
                                PlayerControl.LocalPlayer.NetId, (byte) CustomRPC.CleanBody,
                                SendOption.Reliable, -1);
                            writer.Write(playerInfo.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.CleanBody(playerInfo.PlayerId);

                            Cleaner.Instance.player.killTimer = cleanerCleanButton.timer = cleanerCleanButton.maxTimer;
                            break;
                        }
                },
                () => Cleaner.Instance.player == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead,
                () => __instance.ReportButton.renderer.color == Palette.EnabledColor &&
                      PlayerControl.LocalPlayer.CanMove,
                () => { cleanerCleanButton.timer = cleanerCleanButton.maxTimer; },
                Cleaner.GetButtonSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                KeyCode.F
            );

            // Warlock curse
            warlockCurseButton = new CustomButton(
                () =>
                {
                    if (Warlock.curseVictim == null)
                    {
                        // Apply Curse
                        Warlock.curseVictim = Warlock.currentTarget;
                        warlockCurseButton.sprite = Warlock.GetCurseKillButtonSprite();
                        warlockCurseButton.timer = 1f;
                    }
                    else if (Warlock.curseVictim != null && Warlock.curseVictimTarget != null &&
                             Helpers.HandleMurderAttempt(Warlock.curseVictimTarget))
                    {
                        // Curse Kill
                        Warlock.curseKillTarget = Warlock.curseVictimTarget;

                        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.WarlockCurseKill, SendOption.Reliable, -1);
                        writer.Write(Warlock.curseKillTarget.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.WarlockCurseKill(Warlock.curseKillTarget.PlayerId);

                        Warlock.curseVictim = null;
                        Warlock.curseVictimTarget = null;
                        warlockCurseButton.sprite = Warlock.GetCurseButtonSprite();
                        Warlock.Instance.player.killTimer = warlockCurseButton.timer = warlockCurseButton.maxTimer;

                        if (!(Warlock.rootTime > 0)) return;
                        PlayerControl.LocalPlayer.moveable = false;
                        PlayerControl.LocalPlayer.NetTransform
                            .Halt(); // Stop current movement so the warlock is not just running straight into the next object
                        HudManager.Instance.StartCoroutine(Effects.Lerp(Warlock.rootTime, new Action<float>(p =>
                        {
                            // Delayed action
                            if (Math.Abs(p - 1f) < 0.1f) PlayerControl.LocalPlayer.moveable = true;
                        })));
                    }
                },
                () => Warlock.Instance.player == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead,
                () => (Warlock.curseVictim == null && Warlock.currentTarget != null ||
                       Warlock.curseVictim != null && Warlock.curseVictimTarget != null) &&
                      PlayerControl.LocalPlayer.CanMove,
                () =>
                {
                    warlockCurseButton.timer = warlockCurseButton.maxTimer;
                    warlockCurseButton.sprite = Warlock.GetCurseButtonSprite();
                    Warlock.curseVictim = null;
                    Warlock.curseVictimTarget = null;
                },
                Warlock.GetCurseButtonSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                KeyCode.F
            );

            // Security Guard button
            securityGuardButton = new CustomButton(
                () =>
                {
                    if (SecurityGuard.ventTarget != null)
                    {
                        // Seal vent
                        var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.SealVent, SendOption.Reliable);
                        writer.WritePacked(SecurityGuard.ventTarget.Id);
                        writer.EndMessage();
                        RPCProcedure.SealVent(SecurityGuard.ventTarget.Id);
                        SecurityGuard.ventTarget = null;
                    }
                    else if (PlayerControl.GameOptions.MapId != 1)
                    {
                        // Place camera if there's no vent and it's not MiraHQ
                        var pos = PlayerControl.LocalPlayer.transform.position;
                        var buff = new byte[sizeof(float) * 2];
                        Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                        Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                        var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.PlaceCamera, SendOption.Reliable);
                        writer.WriteBytesAndSize(buff);
                        writer.EndMessage();
                        RPCProcedure.PlaceCamera(buff);
                    }

                    securityGuardButton.timer = securityGuardButton.maxTimer;
                },
                () => SecurityGuard.Instance.player == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead && SecurityGuard.remainingScrews >=
                      Mathf.Min(SecurityGuard.ventPrice, SecurityGuard.camPrice),
                () =>
                {
                    securityGuardButton.killButtonManager.renderer.sprite =
                        SecurityGuard.ventTarget == null && PlayerControl.GameOptions.MapId != 1
                            ? SecurityGuard.GetPlaceCameraButtonSprite()
                            : SecurityGuard.GetCloseVentButtonSprite();
                    if (securityGuardButtonScrewsText != null)
                        securityGuardButtonScrewsText.text =
                            $"{SecurityGuard.remainingScrews}/{SecurityGuard.totalScrews}";

                    if (SecurityGuard.ventTarget != null)
                        return SecurityGuard.remainingScrews >= SecurityGuard.ventPrice &&
                               PlayerControl.LocalPlayer.CanMove;
                    return PlayerControl.GameOptions.MapId != 1 &&
                           SecurityGuard.remainingScrews >= SecurityGuard.camPrice && PlayerControl.LocalPlayer.CanMove;
                },
                () => { securityGuardButton.timer = securityGuardButton.maxTimer; },
                SecurityGuard.GetPlaceCameraButtonSprite(),
                new Vector3(-1.3f, 0f, 0f),
                __instance,
                KeyCode.Q
            );

            // Security Guard button screws counter
            securityGuardButtonScrewsText = Object.Instantiate(securityGuardButton.killButtonManager.TimerText,
                securityGuardButton.killButtonManager.TimerText.transform.parent);
            securityGuardButtonScrewsText.text = "";
            securityGuardButtonScrewsText.enableWordWrapping = false;
            securityGuardButtonScrewsText.transform.localScale = Vector3.one * 0.5f;
            securityGuardButtonScrewsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Arsonist button
            arsonistButton = new CustomButton(
                () =>
                {
                    var dousedEveryoneAlive = Arsonist.DousedEveryoneAlive();
                    if (dousedEveryoneAlive)
                    {
                        var winWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.ArsonistWin, SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(winWriter);
                        RPCProcedure.ArsonistWin();
                        arsonistButton.hasEffect = false;
                    }
                    else if (Arsonist.currentTarget != null)
                    {
                        Arsonist.douseTarget = Arsonist.currentTarget;
                        arsonistButton.hasEffect = true;
                    }
                },
                () => Arsonist.Instance.player != null && Arsonist.Instance.player == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () =>
                {
                    var dousedEveryoneAlive = Arsonist.DousedEveryoneAlive();
                    if (dousedEveryoneAlive)
                        arsonistButton.killButtonManager.renderer.sprite = Arsonist.GetIgniteSprite();

                    if (!arsonistButton.isEffectActive || Arsonist.douseTarget == Arsonist.currentTarget)
                        return PlayerControl.LocalPlayer.CanMove &&
                               (dousedEveryoneAlive || Arsonist.currentTarget != null);
                    Arsonist.douseTarget = null;
                    arsonistButton.timer = 0f;
                    arsonistButton.isEffectActive = false;

                    return PlayerControl.LocalPlayer.CanMove && (dousedEveryoneAlive || Arsonist.currentTarget != null);
                },
                () =>
                {
                    arsonistButton.timer = arsonistButton.maxTimer;
                    arsonistButton.isEffectActive = false;
                    Arsonist.douseTarget = null;
                },
                Arsonist.GetDouseSprite(),
                new Vector3(-1.3f, 0f, 0f),
                __instance,
                KeyCode.Q,
                true,
                Arsonist.duration,
                () =>
                {
                    if (Arsonist.douseTarget != null) Arsonist.DousedPlayers.Add(Arsonist.douseTarget);
                    Arsonist.douseTarget = null;
                    arsonistButton.timer = Arsonist.DousedEveryoneAlive() ? 0 : arsonistButton.maxTimer;

                    foreach (var p in Arsonist.DousedPlayers.Where(p => MapOptions.playerIcons.ContainsKey(p.PlayerId)))
                        MapOptions.playerIcons[p.PlayerId].SetSemiTransparent(false);
                }
            );

            // Set the default (or settings from the previous game) timers/durations when spawning the buttons
            SetCustomButtonCooldowns();
        }
    }
}