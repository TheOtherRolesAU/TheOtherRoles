using HarmonyLib;
using System;
using static TheOtherRoles.TheOtherRoles;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TheOtherRoles
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch {
        public static void Prefix(IntroCutscene __instance) {
            // Arsonist generate player icons
            if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer == Arsonist.arsonist && HudManager.Instance != null) {
                int playerCounter = 0;
                Vector3 bottomLeft = new Vector3(-HudManager.Instance.UseButton.transform.localPosition.x, HudManager.Instance.UseButton.transform.localPosition.y, HudManager.Instance.UseButton.transform.localPosition.z);
                bottomLeft += new Vector3(-0.25f, -0.25f, 0);
                foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                    if (player != PlayerControl.LocalPlayer) {
                        GameData.PlayerInfo data = player.Data;
                        PoolablePlayer poolablePlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, HudManager.Instance.transform);
                        poolablePlayer.transform.localPosition = bottomLeft + Vector3.right * playerCounter * 0.35f;
                        poolablePlayer.transform.localScale = Vector3.one * 0.3f;
                        PlayerControl.SetPlayerMaterialColors(data.ColorId, poolablePlayer.Body);
                        DestroyableSingleton<HatManager>.Instance.SetSkin(poolablePlayer.SkinSlot, data.SkinId);
                        poolablePlayer.HatSlot.SetHat(data.HatId, data.ColorId);
                        PlayerControl.SetPetImage(data.PetId, data.ColorId, poolablePlayer.PetSlot);
                        poolablePlayer.NameText.text = data.PlayerName;
                        poolablePlayer.SetFlipX(true);
                        poolablePlayer.setSemiTransparent(true);
                        Arsonist.dousedIcons[player.PlayerId] = poolablePlayer;
                        playerCounter++;
                    }
                }
            }
        }
    }

    [HarmonyPatch]
    class IntroPatch {
        public static void setupIntroTeam(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
            // Intro solo teams
            if (PlayerControl.LocalPlayer == Jester.jester || PlayerControl.LocalPlayer == Jackal.jackal || PlayerControl.LocalPlayer == Arsonist.arsonist) {
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(PlayerControl.LocalPlayer);
                yourTeam = soloTeam;
            }

            // Add the Spy to the Impostor team (for the Impostors)
            if (Spy.spy != null && PlayerControl.LocalPlayer.Data.IsImpostor) {
                List<PlayerControl> players = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
                var fakeImpostorTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                foreach (PlayerControl p in players) {
                    if (p == Spy.spy || p.Data.IsImpostor)
                        fakeImpostorTeam.Add(p);
                }
                yourTeam = fakeImpostorTeam;
            }
        }

        public static void setupIntroRole(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
            List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
            RoleInfo roleInfo = infos.Where(info => info.roleId != RoleId.Lover).FirstOrDefault();

            if (roleInfo != null) {
                __instance.Title.text = roleInfo.name;
                __instance.Title.color = roleInfo.color;
                __instance.ImpostorText.gameObject.SetActive(true);
                __instance.ImpostorText.text = roleInfo.introDescription;
                __instance.BackgroundBar.material.color = roleInfo.color;
            }

            if (infos.Any(info => info.roleId == RoleId.Lover)) {
                var loversText = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(__instance.ImpostorText, __instance.ImpostorText.transform.parent);
                loversText.transform.localPosition += Vector3.down * 3f;
                PlayerControl otherLover = PlayerControl.LocalPlayer == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                loversText.text = Helpers.cs(Lovers.color, $"❤ You are in lover with {otherLover?.Data?.PlayerName ?? ""} ❤");
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        class BeginCrewmatePatch {
            public static void Prefix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
                setupIntroTeam(__instance, ref yourTeam);
            }

            public static void Postfix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
                setupIntroRole(__instance, ref yourTeam);
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        class BeginImpostorPatch {
            public static void Prefix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
                setupIntroTeam(__instance, ref yourTeam);
            }

            public static void Postfix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
                setupIntroRole(__instance, ref yourTeam);
            }
        }
    }
}
