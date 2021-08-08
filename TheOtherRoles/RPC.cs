using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using PowerTools;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using TheOtherRoles.Roles;
using UnityEngine;
using static TheOtherRoles.RoleReloader;
using static TheOtherRoles.HudManagerStartPatch;
using static TheOtherRoles.GameHistory;
using static TheOtherRoles.MapOptions;
using Object = UnityEngine.Object;

namespace TheOtherRoles
{
    internal enum CustomRPC
    {
        // Main Controls

        ResetVaribles = 50,
        ShareOptionSelection,
        ForceEnd,
        SetRole,
        VersionHandshake,
        UseUncheckedVent,
        UncheckedMurderPlayer,
        UncheckedCmdReportDeadBody,

        // Role functionality

        EngineerFixLights = 81,
        EngineerUsedRepair,
        CleanBody,
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
        VampireSetBitten,
        VampireTryKill,
        PlaceGarlic,
        JackalKill,
        SidekickKill,
        JackalCreatesSidekick,
        SidekickPromotes,
        ErasePlayerRoles,
        SetFutureErased,
        SetFutureShifted,
        SetFutureShielded,
        PlaceJackInTheBox,
        LightsOut,
        WarlockCurseKill,
        PlaceCamera,
        SealVent,
        ArsonistWin,
        GuesserShoot
    }

    public static class RPCProcedure
    {
        // Main Controls

        public static void ResetVariables()
        {
            Garlic.ClearGarlics();
            JackInTheBox.ClearJackInTheBoxes();
            ClearAndReloadMapOptions();
            ClearAndReloadRoles();
            ClearGameHistory();
            SetCustomButtonCooldowns();
        }

        public static void ShareOptionSelection(uint id, uint selection)
        {
            var option = CustomOption.Options.FirstOrDefault(customOption => customOption.id == (int) id);
            option?.UpdateSelection((int) selection);
        }

        public static void ForceEnd()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsImpostor) continue;
                player.RemoveInfected();
                player.MurderPlayer(player);
                player.Data.IsDead = true;
            }
        }

        public static void SetRole(byte roleByte, byte playerId, byte isSecondLover = 0)
        {
            var player = Helpers.PlayerById(playerId);
            var roleId = (RoleId) roleByte;
            if (isSecondLover != 0)
            {
                Lovers.Instance.secondPlayer = player;
                return;
            }

            AllRoles[roleId].player = player;
        }

        public static void VersionHandshake(int major, int minor, int build, int revision, Guid guid, int clientId)
        {
            var ver = revision < 0
                ? new Version(major, minor, build)
                : new Version(major, minor, build, revision);

            GameStartManagerPatch.PlayerVersions[clientId] = new GameStartManagerPatch.PlayerVersion(ver, guid);
        }

        public static void UseUncheckedVent(int ventId, byte playerId, byte isEnter)
        {
            var player = Helpers.PlayerById(playerId);
            if (player == null) return;
            // Fill dummy MessageReader and call MyPhysics.HandleRpc as the coroutines cannot be accessed
            var reader = new MessageReader();
            var bytes = BitConverter.GetBytes(ventId);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            reader.Buffer = bytes;
            reader.Length = bytes.Length;

            JackInTheBox.StartAnimation(ventId);
            player.MyPhysics.HandleRpc(isEnter != 0 ? (byte) 19 : (byte) 20, reader);
        }

        public static void UncheckedMurderPlayer(byte sourceId, byte targetId)
        {
            var source = Helpers.PlayerById(sourceId);
            var target = Helpers.PlayerById(targetId);
            if (source && target) source.MurderPlayer(target);
        }

        public static void UncheckedCmdReportDeadBody(byte sourceId, byte targetId) {
            var source = Helpers.PlayerById(sourceId);
            var target = Helpers.PlayerById(targetId);
            if (source != null && target != null) source.ReportDeadBody(target.Data);
        }


        // Role functionality

        public static void EngineerFixLights()
        {
            var switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
            switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
        }

        public static void EngineerUsedRepair()
        {
            Engineer.usedRepair = true;
        }

        public static void CleanBody(byte playerId)
        {
            DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
            foreach (var t in array)
                if (GameData.Instance.GetPlayerById(t.ParentId).PlayerId == playerId)
                    Object.Destroy(t.gameObject);
        }

        public static void SheriffKill(byte targetId)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId != targetId) continue;
                Sheriff.Instance.player.MurderPlayer(player);
                return;
            }
        }

        public static void TimeMasterRewindTime()
        {
            TimeMaster.shieldActive = false; // Shield is no longer active when rewinding
            if (TimeMaster.Instance.player && TimeMaster.Instance.player == PlayerControl.LocalPlayer)
                ResetTimeMasterButton();
            HudManager.Instance.FullScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
            HudManager.Instance.FullScreen.enabled = true;
            HudManager.Instance.StartCoroutine(Effects.Lerp(TimeMaster.rewindTime / 2, new Action<float>(p =>
            {
                if (Math.Abs(p - 1f) < 0.1f) HudManager.Instance.FullScreen.enabled = false;
            })));

            if (PlayerControl.LocalPlayer == TimeMaster.Instance.player)
                return; // Time Master himself does not rewind

            TimeMaster.isRewinding = true;

            if (MapBehaviour.Instance)
                MapBehaviour.Instance.Close();
            if (Minigame.Instance)
                Minigame.Instance.ForceClose();
            PlayerControl.LocalPlayer.moveable = false;
        }

        public static void TimeMasterShield()
        {
            TimeMaster.shieldActive = true;
            HudManager.Instance.StartCoroutine(Effects.Lerp(TimeMaster.shieldDuration, new Action<float>(p =>
            {
                if (Math.Abs(p - 1f) < 0.1f) TimeMaster.shieldActive = false;
            })));
        }

        public static void MedicSetShielded(byte shieldedId)
        {
            Medic.usedShield = true;
            Medic.shielded = Helpers.PlayerById(shieldedId);
            Medic.futureShielded = null;
        }

        public static void ShieldedMurderAttempt()
        {
            if (Medic.shielded != PlayerControl.LocalPlayer || !Medic.showAttemptToShielded || !HudManager.Instance ||
                !HudManager.Instance.FullScreen) return;
            HudManager.Instance.FullScreen.enabled = true;
            HudManager.Instance.StartCoroutine(Effects.Lerp(0.5f, new Action<float>(p =>
            {
                var renderer = HudManager.Instance.FullScreen;
                var c = Palette.ImpostorRed;
                if (p < 0.5)
                {
                    if (renderer != null)
                        renderer.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(p * 2 * 0.75f));
                }
                else
                {
                    if (renderer != null)
                        renderer.color = new Color(c.r, c.g, c.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                }

                if (Math.Abs(p - 1f) < 0.1f && renderer != null) renderer.enabled = false;
            })));
        }

        public static void ShifterShift(byte targetId)
        {
            var oldShifter = Shifter.Instance.player;
            var player = Helpers.PlayerById(targetId);
            if (player == null || oldShifter == null) return;

            Shifter.futureShift = null;
            Shifter.Instance.ClearAndReload();

            // Suicide (exile) when impostor or impostor variants
            if (player.Data.IsImpostor || player == Jackal.Instance.player || player == Sidekick.Instance.player ||
                Jackal.FormerJackals.Contains(player) || player == Jester.Instance.player ||
                player == Arsonist.Instance.player)
            {
                oldShifter.Exiled();
                return;
            }

            if (Shifter.shiftModifiers)
            {
                // Switch shield
                if (Medic.shielded && Medic.shielded == player)
                    Medic.shielded = oldShifter;
                else if (Medic.shielded && Medic.shielded == oldShifter)
                    Medic.shielded = player;

                // Shift Lovers Role
                if (Lovers.Instance.player && oldShifter == Lovers.Instance.player)
                    Lovers.Instance.player = player;
                else if (Lovers.Instance.player && player == Lovers.Instance.player)
                    Lovers.Instance.player = oldShifter;

                if (Lovers.Instance.secondPlayer && oldShifter == Lovers.Instance.secondPlayer)
                    Lovers.Instance.secondPlayer = player;
                else if (Lovers.Instance.secondPlayer && player == Lovers.Instance.secondPlayer)
                    Lovers.Instance.secondPlayer = oldShifter;
            }

            foreach (var role in AllRoles.Values.Where(x => x.player == player && x.roleType == RoleType.Crewmate))
                role.player = oldShifter;

            // Set cooldowns to max for both players
            if (PlayerControl.LocalPlayer == oldShifter || PlayerControl.LocalPlayer == player)
                CustomButton.ResetAllCooldowns();
        }

        public static void SwapperSwap(byte playerId1, byte playerId2)
        {
            if (!MeetingHud.Instance) return;
            Swapper.playerId1 = playerId1;
            Swapper.playerId2 = playerId2;
        }

        public static void MorphlingMorph(byte playerId)
        {
            var target = Helpers.PlayerById(playerId);
            if (!Morphling.Instance.player || !target) return;

            Morphling.morphTimer = Morphling.duration;
            Morphling.morphTarget = target;
        }

        public static void CamouflagerCamouflage()
        {
            if (!Camouflager.Instance.player) return;

            Camouflager.camouflageTimer = Camouflager.duration;
        }

        public static void VampireSetBitten(byte targetId, byte reset)
        {
            if (reset != 0)
            {
                Vampire.bitten = null;
                return;
            }

            if (Vampire.Instance.player == null) return;
            foreach (var player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == targetId && !player.Data.IsDead)
                    Vampire.bitten = player;
        }

        public static void VampireTryKill()
        {
            if (Vampire.bitten && !Vampire.bitten.Data.IsDead)
                Vampire.Instance.player.MurderPlayer(Vampire.bitten);
            Vampire.bitten = null;
        }

        public static void PlaceGarlic(byte[] buff)
        {
            var position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            _ = new Garlic(position);
        }

        public static void TrackerUsedTracker(byte targetId)
        {
            Tracker.usedTracker = true;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId != targetId) continue;
                Tracker.tracked = player;
            }
        }

        public static void JackalKill(byte targetId)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId != targetId) continue;
                Jackal.Instance.player.MurderPlayer(player);
                return;
            }
        }

        public static void SidekickKill(byte targetId)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId != targetId) continue;
                Sidekick.Instance.player.MurderPlayer(player);
                return;
            }
        }

        public static void JackalCreatesSidekick(byte targetId)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId != targetId) continue;
                if (!Jackal.canCreateSidekickFromImpostor && player.Data.IsImpostor)
                {
                    Jackal.fakeSidekick = player;
                }
                else
                {
                    player.RemoveInfected();
                    ErasePlayerRoles(player.PlayerId, true);
                    Sidekick.Instance.player = player;
                }

                Jackal.canCreateSidekick = false;
                return;
            }
        }

        public static void SidekickPromotes()
        {
            Jackal.RemoveCurrentJackal();
            Jackal.Instance.player = Sidekick.Instance.player;
            Jackal.canCreateSidekick = Jackal.recursiveSidekicks;
            Sidekick.Instance.ClearAndReload();
        }

        public static void ErasePlayerRoles(byte playerId, bool ignoreLovers = false)
        {
            var player = Helpers.PlayerById(playerId);
            if (player == null) return;

            foreach (var role in AllRoles.Values.Where(x => x.player == player)) role.ClearAndReload();

            if (!ignoreLovers && player == Lovers.Instance.secondPlayer) // The whole Lover couple is being erased
                Lovers.Instance.ClearAndReload();
            if (player == Jackal.Instance.player)
                // Promote Sidekick and hence override the the Jackal or erase Jackal
                if (Sidekick.promotesToJackal && Sidekick.Instance.player && !Sidekick.Instance.player.Data.IsDead)
                    SidekickPromotes();

            if (player == Sidekick.Instance.player) Sidekick.Instance.ClearAndReload();
            if (player == BountyHunter.Instance.player) BountyHunter.Instance.ClearAndReload();
        }

        public static void SetFutureErased(byte playerId)
        {
            var player = Helpers.PlayerById(playerId);
            Eraser.futureErased ??= new List<PlayerControl>();
            if (player != null) Eraser.futureErased.Add(player);
        }

        public static void SetFutureShifted(byte playerId)
        {
            Shifter.futureShift = Helpers.PlayerById(playerId);
        }

        public static void SetFutureShielded(byte playerId)
        {
            Medic.futureShielded = Helpers.PlayerById(playerId);
            Medic.usedShield = true;
        }

        public static void PlaceJackInTheBox(byte[] buff)
        {
            var position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            _ = new JackInTheBox(position);
        }

        public static void LightsOut()
        {
            Trickster.lightsOutTimer = Trickster.lightsOutDuration;
            // If the local player is impostor indicate lights out
            if (PlayerControl.LocalPlayer.Data.IsImpostor)
                _ = new CustomMessage("Lights are out", Trickster.lightsOutDuration);
        }

        public static void WarlockCurseKill(byte targetId)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId != targetId) continue;
                Warlock.curseKillTarget = player;
                Warlock.Instance.player.MurderPlayer(player);
                return;
            }
        }

        public static void PlaceCamera(byte[] buff)
        {
            var referenceCamera = Object.FindObjectOfType<SurvCamera>();
            if (referenceCamera == null) return; // Mira HQ

            SecurityGuard.remainingScrews -= SecurityGuard.camPrice;
            SecurityGuard.placedCameras++;

            var position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));

            var camera = Object.Instantiate(referenceCamera);
            camera.transform.position = new Vector3(position.x, position.y, referenceCamera.transform.position.z - 1f);
            camera.CamName = $"Security Camera {SecurityGuard.placedCameras}";
            camera.Offset = new Vector3(0f, 0f, camera.Offset.z);
            if (PlayerControl.GameOptions.MapId is 2 or 4)
                camera.transform.localRotation = new Quaternion(0, 0, 1, 1); // Polus and Airship 

            if (PlayerControl.LocalPlayer == SecurityGuard.Instance.player)
            {
                camera.gameObject.SetActive(true);
                camera.gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
            }
            else
            {
                camera.gameObject.SetActive(false);
            }

            camerasToAdd.Add(camera);
        }

        public static void SealVent(int ventId)
        {
            var vent = ShipStatus.Instance.AllVents.FirstOrDefault(x => x && x.Id == ventId);
            if (vent == null) return;

            SecurityGuard.remainingScrews -= SecurityGuard.ventPrice;
            if (PlayerControl.LocalPlayer == SecurityGuard.Instance.player)
            {
                var animator = vent.GetComponent<SpriteAnim>();
                if (animator)
                    animator.Stop();
                vent.EnterVentAnim = vent.ExitVentAnim = null;
                vent.myRend.sprite = animator == null
                    ? SecurityGuard.GetStaticVentSealedSprite()
                    : SecurityGuard.GetAnimatedVentSealedSprite();
                vent.myRend.color = new Color(1f, 1f, 1f, 0.5f);
                vent.name = "FutureSealedVent_" + vent.name;
            }

            ventsToSeal.Add(vent);
        }

        public static void ArsonistWin()
        {
            Arsonist.triggerArsonistWin = true;
        }

        public static void GuesserShoot(byte playerId)
        {
            var target = Helpers.PlayerById(playerId);
            if (target == null) return;
            target.Exiled();
            var partner = Lovers.GetPartner(target); // Lover check
            var partnerId = partner ? partner.PlayerId : playerId;
            Guesser.remainingShots = Mathf.Max(0, Guesser.remainingShots - 1);
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(target.KillSfx, false, 0.8f);
            if (MeetingHud.Instance)
            {
                foreach (var pva in MeetingHud.Instance.playerStates)
                    if (pva.TargetPlayerId == playerId || pva.TargetPlayerId == partnerId)
                    {
                        pva.SetDead(pva.DidReport, true);
                        pva.Overlay.gameObject.SetActive(true);
                    }

                if (AmongUsClient.Instance.AmHost)
                    MeetingHud.Instance.CheckForEndVoting();
            }

            if (HudManager.Instance == null || Guesser.Instance.player == null) return;
            if (PlayerControl.LocalPlayer == target)
                HudManager.Instance.KillOverlay.ShowKillAnimation(Guesser.Instance.player.Data, target.Data);
            else if (partner && PlayerControl.LocalPlayer == partner)
                HudManager.Instance.KillOverlay.ShowKillAnimation(partner.Data, partner.Data);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    internal class RPCHandlerPatch
    {
        private static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            switch (callId)
            {
                // Main Controls

                case (byte) CustomRPC.ResetVaribles:
                    RPCProcedure.ResetVariables();
                    break;
                case (byte) CustomRPC.ShareOptionSelection:
                    var id = reader.ReadPackedUInt32();
                    var selection = reader.ReadPackedUInt32();
                    RPCProcedure.ShareOptionSelection(id, selection);
                    break;
                case (byte) CustomRPC.ForceEnd:
                    RPCProcedure.ForceEnd();
                    break;
                case (byte) CustomRPC.SetRole:
                    var roleId = reader.ReadByte();
                    var playerId = reader.ReadByte();
                    var flag = reader.ReadByte();
                    RPCProcedure.SetRole(roleId, playerId, flag);
                    break;
                case (byte) CustomRPC.VersionHandshake:
                    var major = reader.ReadByte();
                    var minor = reader.ReadByte();
                    var patch = reader.ReadByte();
                    var versionOwnerId = reader.ReadPackedInt32();
                    byte revision = 0xFF;
                    Guid guid;
                    if (reader.Length - reader.Position >= 17)
                    {
                        // enough bytes left to read
                        revision = reader.ReadByte();
                        // GUID
                        byte[] gbytes = reader.ReadBytes(16);
                        guid = new Guid(gbytes);
                    }
                    else
                    {
                        guid = new Guid(new byte[16]);
                    }

                    RPCProcedure.VersionHandshake(major, minor, patch, revision == 0xFF ? -1 : revision, guid,
                        versionOwnerId);
                    break;
                case (byte) CustomRPC.UseUncheckedVent:
                    var ventId = reader.ReadPackedInt32();
                    var ventingPlayer = reader.ReadByte();
                    var isEnter = reader.ReadByte();
                    RPCProcedure.UseUncheckedVent(ventId, ventingPlayer, isEnter);
                    break;
                case (byte) CustomRPC.UncheckedMurderPlayer:
                    var source = reader.ReadByte();
                    var target = reader.ReadByte();
                    RPCProcedure.UncheckedMurderPlayer(source, target);
                    break;
                case (byte)CustomRPC.UncheckedCmdReportDeadBody:
                    var reportSource = reader.ReadByte();
                    var reportTarget = reader.ReadByte();
                    RPCProcedure.UncheckedCmdReportDeadBody(reportSource, reportTarget);
                    break;

                // Role functionality

                case (byte) CustomRPC.EngineerFixLights:
                    RPCProcedure.EngineerFixLights();
                    break;
                case (byte) CustomRPC.EngineerUsedRepair:
                    RPCProcedure.EngineerUsedRepair();
                    break;
                case (byte) CustomRPC.CleanBody:
                    RPCProcedure.CleanBody(reader.ReadByte());
                    break;
                case (byte) CustomRPC.SheriffKill:
                    RPCProcedure.SheriffKill(reader.ReadByte());
                    break;
                case (byte) CustomRPC.TimeMasterRewindTime:
                    RPCProcedure.TimeMasterRewindTime();
                    break;
                case (byte) CustomRPC.TimeMasterShield:
                    RPCProcedure.TimeMasterShield();
                    break;
                case (byte) CustomRPC.MedicSetShielded:
                    RPCProcedure.MedicSetShielded(reader.ReadByte());
                    break;
                case (byte) CustomRPC.ShieldedMurderAttempt:
                    RPCProcedure.ShieldedMurderAttempt();
                    break;
                case (byte) CustomRPC.ShifterShift:
                    RPCProcedure.ShifterShift(reader.ReadByte());
                    break;
                case (byte) CustomRPC.SwapperSwap:
                    var playerId1 = reader.ReadByte();
                    var playerId2 = reader.ReadByte();
                    RPCProcedure.SwapperSwap(playerId1, playerId2);
                    break;
                case (byte) CustomRPC.MorphlingMorph:
                    RPCProcedure.MorphlingMorph(reader.ReadByte());
                    break;
                case (byte) CustomRPC.CamouflagerCamouflage:
                    RPCProcedure.CamouflagerCamouflage();
                    break;
                case (byte) CustomRPC.VampireSetBitten:
                    var bittenId = reader.ReadByte();
                    var reset = reader.ReadByte();
                    RPCProcedure.VampireSetBitten(bittenId, reset);
                    break;
                case (byte) CustomRPC.VampireTryKill:
                    RPCProcedure.VampireTryKill();
                    break;
                case (byte) CustomRPC.PlaceGarlic:
                    RPCProcedure.PlaceGarlic(reader.ReadBytesAndSize());
                    break;
                case (byte) CustomRPC.TrackerUsedTracker:
                    RPCProcedure.TrackerUsedTracker(reader.ReadByte());
                    break;
                case (byte) CustomRPC.JackalKill:
                    RPCProcedure.JackalKill(reader.ReadByte());
                    break;
                case (byte) CustomRPC.SidekickKill:
                    RPCProcedure.SidekickKill(reader.ReadByte());
                    break;
                case (byte) CustomRPC.JackalCreatesSidekick:
                    RPCProcedure.JackalCreatesSidekick(reader.ReadByte());
                    break;
                case (byte) CustomRPC.SidekickPromotes:
                    RPCProcedure.SidekickPromotes();
                    break;
                case (byte) CustomRPC.ErasePlayerRoles:
                    RPCProcedure.ErasePlayerRoles(reader.ReadByte());
                    break;
                case (byte) CustomRPC.SetFutureErased:
                    RPCProcedure.SetFutureErased(reader.ReadByte());
                    break;
                case (byte) CustomRPC.SetFutureShifted:
                    RPCProcedure.SetFutureShifted(reader.ReadByte());
                    break;
                case (byte) CustomRPC.SetFutureShielded:
                    RPCProcedure.SetFutureShielded(reader.ReadByte());
                    break;
                case (byte) CustomRPC.PlaceJackInTheBox:
                    RPCProcedure.PlaceJackInTheBox(reader.ReadBytesAndSize());
                    break;
                case (byte) CustomRPC.LightsOut:
                    RPCProcedure.LightsOut();
                    break;
                case (byte) CustomRPC.WarlockCurseKill:
                    RPCProcedure.WarlockCurseKill(reader.ReadByte());
                    break;
                case (byte) CustomRPC.PlaceCamera:
                    RPCProcedure.PlaceCamera(reader.ReadBytesAndSize());
                    break;
                case (byte) CustomRPC.SealVent:
                    RPCProcedure.SealVent(reader.ReadPackedInt32());
                    break;
                case (byte) CustomRPC.ArsonistWin:
                    RPCProcedure.ArsonistWin();
                    break;
                case (byte) CustomRPC.GuesserShoot:
                    RPCProcedure.GuesserShoot(reader.ReadByte());
                    break;
            }
        }
    }
}