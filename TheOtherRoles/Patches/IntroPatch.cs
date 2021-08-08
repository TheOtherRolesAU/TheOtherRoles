using System;
using System.Linq;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using TheOtherRoles.Roles;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    internal class IntroCutsceneOnDestroyPatch
    {
        public static void Prefix(IntroCutscene __instance)
        {
            // Generate and initialize player icons
            var playerCounter = 0;
            if (PlayerControl.LocalPlayer != null && HudManager.Instance != null)
            {
                var bottomLeft = HudManager.Instance.UseButton.transform.localPosition;
                bottomLeft[0] = -bottomLeft[0];
                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    var data = p.Data;
                    var player = Object.Instantiate(__instance.PlayerPrefab, HudManager.Instance.transform);
                    PlayerControl.SetPlayerMaterialColors(data.ColorId, player.Body);
                    DestroyableSingleton<HatManager>.Instance.SetSkin(player.Skin.layer, data.SkinId);
                    player.HatSlot.SetHat(data.HatId, data.ColorId);
                    PlayerControl.SetPetImage(data.PetId, data.ColorId, player.PetSlot);
                    player.NameText.text = data.PlayerName;
                    player.SetFlipX(true);
                    MapOptions.playerIcons[p.PlayerId] = player;

                    if (PlayerControl.LocalPlayer == Arsonist.Instance.player && p != Arsonist.Instance.player)
                    {
                        player.transform.localPosition = bottomLeft + new Vector3(-0.25f, -0.25f, 0) +
                                                         Vector3.right * playerCounter++ * 0.35f;
                        player.transform.localScale = Vector3.one * 0.2f;
                        player.SetSemiTransparent(true);
                        player.gameObject.SetActive(true);
                    }
                    else if (PlayerControl.LocalPlayer == BountyHunter.Instance.player)
                    {
                        player.transform.localPosition = bottomLeft + new Vector3(-0.25f, 0f, 0);
                        player.transform.localScale = Vector3.one * 0.4f;
                        player.gameObject.SetActive(false);
                    }
                    else
                    {
                        player.gameObject.SetActive(false);
                    }
                }
            }

            // Force Bounty Hunter to load a new Bounty when the Intro is over
            if (BountyHunter.bounty == null || PlayerControl.LocalPlayer != BountyHunter.Instance.player) return;

            {
                // redefining bottomLeft
                BountyHunter.bountyUpdateTimer = 0f;
                if (HudManager.Instance == null) return;
                var bottomLeft = HudManager.Instance.UseButton.transform.localPosition;
                bottomLeft[0] = -bottomLeft[0];
                bottomLeft += new Vector3(-0.25f, 1f, 0);
                BountyHunter.cooldownText = Object.Instantiate(HudManager.Instance.KillButton.TimerText,
                    HudManager.Instance.transform);
                BountyHunter.cooldownText.alignment = TextAlignmentOptions.Center;
                BountyHunter.cooldownText.transform.localPosition = bottomLeft + new Vector3(0f, -1f, -1f);
                BountyHunter.cooldownText.gameObject.SetActive(true);
            }
        }
    }

    [HarmonyPatch]
    internal class IntroPatch
    {
        private static void SetupIntroTeam(ref List<PlayerControl> yourTeam)
        {
            // Intro solo teams
            if (PlayerControl.LocalPlayer == Jester.Instance.player ||
                PlayerControl.LocalPlayer == Jackal.Instance.player ||
                PlayerControl.LocalPlayer == Arsonist.Instance.player)
            {
                var soloTeam = new List<PlayerControl>();
                soloTeam.Add(PlayerControl.LocalPlayer);
                yourTeam = soloTeam;
            }

            // Add the Spy to the Impostor team (for the Impostors)
            if (Spy.Instance.player == null || !PlayerControl.LocalPlayer.Data.IsImpostor) return;
            var players = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(_ => Guid.NewGuid()).ToList();
            var fakeImpostorTeam = new List<PlayerControl>();
            foreach (var p in players.Where(p => p == Spy.Instance.player || p.Data.IsImpostor))
                fakeImpostorTeam.Add(p);
            yourTeam = fakeImpostorTeam;
        }

        private static void SetupIntroRole(IntroCutscene __instance)
        {
            var infos = RoleInfo.GetRoleInfoForPlayer(PlayerControl.LocalPlayer);
            var roleInfo = infos.FirstOrDefault(info => info.roleId != RoleId.Lover);

            if (roleInfo != null)
            {
                __instance.Title.text = roleInfo.name;
                __instance.ImpostorText.gameObject.SetActive(true);
                __instance.ImpostorText.text = roleInfo.introDescription;
                if (roleInfo.roleId != RoleId.Crewmate && roleInfo.roleId != RoleId.Impostor)
                {
                    // For native Crewmate or Impostor do not modify the colors
                    __instance.Title.color = roleInfo.color;
                    __instance.BackgroundBar.material.color = roleInfo.color;
                }
            }

            if (infos.Any(info => info.roleId == RoleId.Lover))
            {
                var loversText = Object.Instantiate(__instance.ImpostorText, __instance.ImpostorText.transform.parent);
                loversText.transform.localPosition += Vector3.down * 3f;
                var otherLover = PlayerControl.LocalPlayer == Lovers.Instance.player
                    ? Lovers.Instance.secondPlayer
                    : Lovers.Instance.player;
                loversText.text = Helpers.Cs(Lovers.Instance.color,
                    $"♥ You are in love with {otherLover?.Data?.PlayerName ?? ""} ♥");
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        private class BeginCrewmatePatch
        {
            public static void Prefix(ref List<PlayerControl> yourTeam)
            {
                SetupIntroTeam(ref yourTeam);
            }

            public static void Postfix(IntroCutscene __instance)
            {
                SetupIntroRole(__instance);
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        private class BeginImpostorPatch
        {
            public static void Prefix(ref List<PlayerControl> yourTeam)
            {
                SetupIntroTeam(ref yourTeam);
            }

            public static void Postfix(IntroCutscene __instance)
            {
                SetupIntroRole(__instance);
            }
        }
    }
}