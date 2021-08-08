using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using PowerTools;
using TheOtherRoles.Objects;
using TheOtherRoles.Roles;
using UnhollowerBaseLib;
using UnityEngine;
using static TheOtherRoles.MapOptions;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch(typeof(ExileController), "Begin")]
    internal class ExileControllerBeginPatch
    {
        public static void Prefix()
        {
            // Medic shield
            if (Medic.Instance.player != null && AmongUsClient.Instance.AmHost && Medic.futureShielded != null &&
                !Medic.Instance.player.Data.IsDead)
            {
                // We need to send the RPC from the host here, to make sure that the order of shifting and setting the shield is correct(for that reason the futureShifted and futureShielded are being synced)
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.MedicSetShielded, SendOption.Reliable, -1);
                writer.Write(Medic.futureShielded.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.MedicSetShielded(Medic.futureShielded.PlayerId);
            }

            // Shifter shift
            if (Shifter.Instance.player != null && AmongUsClient.Instance.AmHost && Shifter.futureShift != null)
            {
                // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.ShifterShift, SendOption.Reliable, -1);
                writer.Write(Shifter.futureShift.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.ShifterShift(Shifter.futureShift.PlayerId);
            }

            Shifter.futureShift = null;

            // Eraser erase
            if (Eraser.Instance.player != null && AmongUsClient.Instance.AmHost &&
                Eraser.futureErased !=
                null) // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                foreach (var target in Eraser.futureErased)
                    if (target != null && target.CanBeErased())
                    {
                        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.ErasePlayerRoles, SendOption.Reliable, -1);
                        writer.Write(target.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.ErasePlayerRoles(target.PlayerId);
                    }

            Eraser.futureErased = new List<PlayerControl>();

            // Trickster boxes
            if (Trickster.Instance.player != null && JackInTheBox.HasJackInTheBoxLimitReached())
                JackInTheBox.ConvertToVents();

            // SecurityGuard vents and cameras
            var allCameras = ShipStatus.Instance.AllCameras.ToList();
            camerasToAdd.ForEach(camera =>
            {
                camera.gameObject.SetActive(true);
                camera.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                allCameras.Add(camera);
            });
            ShipStatus.Instance.AllCameras = allCameras.ToArray();
            camerasToAdd = new List<SurvCamera>();

            foreach (var vent in ventsToSeal)
            {
                var animator = vent.GetComponent<SpriteAnim>();
                animator.Stop();
                vent.EnterVentAnim = vent.ExitVentAnim = null;
                vent.myRend.sprite = animator == null
                    ? SecurityGuard.GetStaticVentSealedSprite()
                    : SecurityGuard.GetAnimatedVentSealedSprite();
                vent.myRend.color = Color.white;
                vent.name = "SealedVent_" + vent.name;
            }

            ventsToSeal = new List<Vent>();
        }
    }

    [HarmonyPatch]
    internal class ExileControllerWrapUpPatch
    {
        private static void WrapUpPostfix(GameData.PlayerInfo exiled)
        {
            // Mini exile lose condition
            if (exiled != null && Mini.Instance.player != null && Mini.Instance.player.PlayerId == exiled.PlayerId &&
                !Mini.IsGrownUp() &&
                !Mini.Instance.player.Data.IsImpostor)
                Mini.triggerMiniLose = true;
            // Jester win condition
            else if (exiled != null && Jester.Instance.player != null &&
                     Jester.Instance.player.PlayerId == exiled.PlayerId)
                Jester.triggerJesterWin = true;

            // Reset custom button timers where necessary
            CustomButton.MeetingEndedUpdate();

            // Mini set adapted cooldown
            if (Mini.Instance.player != null && PlayerControl.LocalPlayer == Mini.Instance.player &&
                Mini.Instance.player.Data.IsImpostor)
            {
                var multiplier = Mini.IsGrownUp() ? 0.66f : 2f;
                Mini.Instance.player.SetKillTimer(PlayerControl.GameOptions.KillCooldown * multiplier);
            }

            // Seer spawn souls
            if (Seer.deadBodyPositions != null && Seer.Instance.player != null &&
                PlayerControl.LocalPlayer == Seer.Instance.player &&
                (Seer.mode == 0 || Seer.mode == 2))
            {
                foreach (var pos in Seer.deadBodyPositions)
                {
                    var soul = new GameObject();
                    soul.transform.position = pos;
                    soul.layer = 5;
                    var rend = soul.AddComponent<SpriteRenderer>();
                    rend.sprite = Seer.GetSoulSprite();

                    if (Seer.limitSoulDuration)
                        HudManager.Instance.StartCoroutine(Effects.Lerp(Seer.soulDuration, new Action<float>(p =>
                        {
                            if (rend != null)
                            {
                                var tmp = rend.color;
                                tmp.a = Mathf.Clamp01(1 - p);
                                rend.color = tmp;
                            }

                            if (Math.Abs(p - 1f) < 0.1f && rend != null && rend.gameObject != null)
                                Object.Destroy(rend.gameObject);
                        })));
                }

                Seer.deadBodyPositions = new List<Vector3>();
            }

            // Arsonist deactivate dead poolable players
            if (Arsonist.Instance.player != null && Arsonist.Instance.player == PlayerControl.LocalPlayer)
            {
                var visibleCounter = 0;
                var bottomLeft = HudManager.Instance.UseButton.transform.localPosition;
                bottomLeft[0] = -bottomLeft[0];
                bottomLeft += new Vector3(-0.25f, -0.25f, 0);
                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    if (!playerIcons.ContainsKey(p.PlayerId)) continue;
                    if (p.Data.IsDead || p.Data.Disconnected)
                    {
                        playerIcons[p.PlayerId].gameObject.SetActive(false);
                    }
                    else
                    {
                        playerIcons[p.PlayerId].transform.localPosition =
                            bottomLeft + Vector3.right * visibleCounter * 0.35f;
                        visibleCounter++;
                    }
                }
            }

            // Force Bounty Hunter Bounty Update
            if (BountyHunter.Instance.player != null && BountyHunter.Instance.player == PlayerControl.LocalPlayer)
                BountyHunter.bountyUpdateTimer = 0f;
        }

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        private class BaseExileControllerPatch
        {
            public static void Postfix(ExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        private class AirshipExileControllerPatch
        {
            public static void Postfix(AirshipExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), typeof(StringNames),
        typeof(Il2CppReferenceArray<Il2CppSystem.Object>))]
    internal class ExileControllerMessagePatch
    {
        private static void Postfix(ref string __result, [HarmonyArgument(0)] StringNames id)
        {
            try
            {
                if (ExileController.Instance == null || ExileController.Instance.exiled == null) return;
                var player = Helpers.PlayerById(ExileController.Instance.exiled.Object.PlayerId);
                if (player == null) return;
                switch (id)
                {
                    case StringNames.ExileTextPN:
                    case StringNames.ExileTextSN:
                    case StringNames.ExileTextPP:
                    case StringNames.ExileTextSP:
                        __result = player.Data.PlayerName + " was The " + string.Join(" ",
                            RoleInfo.GetRoleInfoForPlayer(player).Select(x => x.name).ToArray());
                        break;
                    case StringNames.ImpostorsRemainP:
                    case StringNames.ImpostorsRemainS:
                    {
                        if (Jester.Instance.player != null && player.PlayerId == Jester.Instance.player.PlayerId)
                            __result = "";
                        break;
                    }
                }
            }
            catch
            {
                // pass - Hopefully prevent leaving while exiling to softlock game
            }
        }
    }
}