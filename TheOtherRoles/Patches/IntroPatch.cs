using HarmonyLib;
using System;
using static TheOtherRoles.TheOtherRoles;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch
    {
        public static PoolablePlayer playerPrefab;
        public static void Prefix(IntroCutscene __instance)
        {
            // Generate and initialize player icons
            int playerCounter = 0;
            if (CachedPlayer.LocalPlayer != null && FastDestroyableSingleton<HudManager>.Instance != null)
            {
                Vector3 bottomLeft = new Vector3(-FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.x, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.y, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.z);
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    GameData.PlayerInfo data = p.Data;
                    PoolablePlayer player = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, FastDestroyableSingleton<HudManager>.Instance.transform);
                    playerPrefab = __instance.PlayerPrefab;
                    PlayerControl.SetPlayerMaterialColors(data.DefaultOutfit.ColorId, player.CurrentBodySprite.BodySprite);
                    player.SetSkin(data.DefaultOutfit.SkinId, data.DefaultOutfit.ColorId);
                    player.HatSlot.SetHat(data.DefaultOutfit.HatId, data.DefaultOutfit.ColorId);
                    PlayerControl.SetPetImage(data.DefaultOutfit.PetId, data.DefaultOutfit.ColorId, player.PetSlot);
                    player.NameText.text = data.PlayerName;
                    player.SetFlipX(true);
                    MapOptions.playerIcons[p.PlayerId] = player;

                    if (CachedPlayer.LocalPlayer.PlayerControl == Arsonist.arsonist && p != Arsonist.arsonist)
                    {
                        player.transform.localPosition = bottomLeft + new Vector3(-0.25f, -0.25f, 0) + Vector3.right * playerCounter++ * 0.35f;
                        player.transform.localScale = Vector3.one * 0.2f;
                        player.setSemiTransparent(true);
                        player.gameObject.SetActive(true);
                    }
                    else if (CachedPlayer.LocalPlayer.PlayerControl == BountyHunter.bountyHunter)
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

            if (CustomOptionHolder.randomSpawnLocations.getBool() && PlayerControl.LocalPlayer != null)
            {
                int mapID = PlayerControl.GameOptions.MapId;
                List<Vector3> skeldCoords = new List<Vector3>()
                {
                    new Vector3(-1.1f, 5.8f, 0.0f),
                    new Vector3(9.5f, 3.7f, 0.0f),
                    new Vector3(4.9f, -3.8f, 0.0f),
                    new Vector3(18.2f, -3.8f, 0.0f),
                    new Vector3(7.5f, -14.1f, 0.0f),
                    new Vector3(4.7f, -15.4f, 0.0f),
                    new Vector3(-1.4f, -16.2f, 0.0f),
                    new Vector3(6.6f, -7.0f, 0.0f),
                    new Vector3(-7.3f, -4.7f, 0.0f),
                    new Vector3(-19.1f, -1.0f, 0.0f),
                    new Vector3(-21.4f, -5.0f, 0.0f),
                    new Vector3(-18.7f, -13.2f, 0.0f),
                    new Vector3(-6.2f, -7.9f, 0.0f),
                    new Vector3(-12.3f, -3.1f, 0.0f),
                    new Vector3(0.2f, -0.8f, 0.0f),
                    new Vector3(-3.8f, -9.9f, 0.0f),
                    new Vector3(9.4f, -9.0f, 0.0f),
                    new Vector3(4.5f, 3.5f, 0.0f),
                    new Vector3(-11.3f, 1.1f, 0.0f),
                    new Vector3(-12.3f, -14.7f, 0.0f),
                    new Vector3(0.2f, -17.1f, 0.0f)

                };
                List<Vector3> miraCoords = new List<Vector3>()
                {
                    new Vector3(-4.7f, 3.9f, 0.0f),
                    new Vector3(-4.7f, -2.2f, 0.0f),
                    new Vector3(5.5f, -1.7f, 0.0f),
                    new Vector3(12.5f, -1.7f, 0.0f),
                    new Vector3(16.3f, 0.6f, 0.0f),
                    new Vector3(16.2f, 4.4f, 0.0f),
                    new Vector3(12.3f, 7.6f, 0.0f),
                    new Vector3(17.9f, 11.6f, 0.0f),
                    new Vector3(23.7f, 6.5f, 0.0f),
                    new Vector3(28.2f, -1.9f, 0.0f),
                    new Vector3(19.5f, -2.4f, 0.0f),
                    new Vector3(19.9f, 5.3f, 0.0f),
                    new Vector3(17.7f, 15.2f, 0.0f),
                    new Vector3(14.7f, 21.1f, 0.0f),
                    new Vector3(22.3f, 20.9f, 0.0f),
                    new Vector3(18.2f, 25.5f, 0.0f),
                    new Vector3(4.8f, 0.7f, 0.0f),
                    new Vector3(6.0f, 6.9f, 0.0f),
                    new Vector3(1.1f, 14.5f, 0.0f),
                    new Vector3(9.8f, 10.7f, 0.0f),
                    new Vector3(6.4f, 14.4f, 0.0f)

                };
                List<Vector3> polusCoords = new List<Vector3>()
                {
                    new Vector3(16.7f, -0.8f, 0.0f),
                    new Vector3(4.7f, -7.4f, 0.0f),
                    new Vector3(3.0f, -12.4f, 0.0f),
                    new Vector3(10.1f, -12.2f, 0.0f),
                    new Vector3(1.4f, -16.3f, 0.0f),
                    new Vector3(1.3f, -23.8f, 0.0f),
                    new Vector3(8.3f, -25.3f, 0.0f),
                    new Vector3(12.2f, -15.5f, 0.0f),
                    new Vector3(10.4f, -23.0f, 0.0f),
                    new Vector3(22.3f, -25.2f, 0.0f),
                    new Vector3(19.7f, -16.3f, 0.0f),
                    new Vector3(30.5f, -15.6f, 0.0f),
                    new Vector3(25.9f, -12.9f, 0.0f),
                    new Vector3(27.8f, -7.1f, 0.0f),
                    new Vector3(34.0f, -5.6f, 0.0f),
                    new Vector3(39.9f, -9.8f, 0.0f),
                    new Vector3(23.8f, -24.6f, 0.0f),
                    new Vector3(21.4f, -11.7f, 0.0f),
                    new Vector3(24.3f, -3.0f, 0.0f),
                    new Vector3(14.2f, -15.4f, 0.0f),
                    new Vector3(19.4f, -14.9f, 0.0f),
                    new Vector3(11.7f, -9.7f, 0.0f),
                    new Vector3(24.7f, -9.7f, 0.0f)

                };
                Vector3 coords = PlayerControl.LocalPlayer.transform.position;
                if (mapID == 0)
                    coords = skeldCoords[(rnd.Next(skeldCoords.Count) + PlayerControl.LocalPlayer.PlayerId) % skeldCoords.Count()];
                else if (mapID == 1)
                    coords = miraCoords[(rnd.Next(miraCoords.Count) + PlayerControl.LocalPlayer.PlayerId) % miraCoords.Count()];
                else if (mapID == 2)
                    coords = polusCoords[rnd.Next((polusCoords.Count) + PlayerControl.LocalPlayer.PlayerId) % polusCoords.Count()];
                PlayerControl.LocalPlayer.transform.position = coords;
            }


            // Force Bounty Hunter to load a new Bounty when the Intro is over
            if (BountyHunter.bounty != null && CachedPlayer.LocalPlayer.PlayerControl == BountyHunter.bountyHunter)
            {
                BountyHunter.bountyUpdateTimer = 0f;
                if (FastDestroyableSingleton<HudManager>.Instance != null)
                {
                    Vector3 bottomLeft = new Vector3(-FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.x, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.y, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.z) + new Vector3(-0.25f, 1f, 0);
                    BountyHunter.cooldownText = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                    BountyHunter.cooldownText.alignment = TMPro.TextAlignmentOptions.Center;
                    BountyHunter.cooldownText.transform.localPosition = bottomLeft + new Vector3(0f, -1f, -1f);
                    BountyHunter.cooldownText.gameObject.SetActive(true);
                }
            }

            // First kill
            if (AmongUsClient.Instance.AmHost && MapOptions.shieldFirstKill && MapOptions.firstKillName != "")
            {
                PlayerControl target = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(MapOptions.firstKillName));
                if (target != null)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.SetFirstKill, Hazel.SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.setFirstKill(target.PlayerId);
                }
            }
            MapOptions.firstKillName = "";
        }
    }

    [HarmonyPatch]
    class IntroPatch
    {
        public static void setupIntroTeamIcons(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            // Intro solo teams
            if (CachedPlayer.LocalPlayer.PlayerControl == Jester.jester || CachedPlayer.LocalPlayer.PlayerControl == Jackal.jackal || CachedPlayer.LocalPlayer.PlayerControl == Arsonist.arsonist || CachedPlayer.LocalPlayer.PlayerControl == Vulture.vulture || CachedPlayer.LocalPlayer.PlayerControl == Lawyer.lawyer)
            {
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(CachedPlayer.LocalPlayer.PlayerControl);
                yourTeam = soloTeam;
            }

            // Add the Spy to the Impostor team (for the Impostors)
            if (Spy.spy != null && CachedPlayer.LocalPlayer.Data.Role.IsImpostor)
            {
                List<PlayerControl> players = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
                var fakeImpostorTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>(); // The local player always has to be the first one in the list (to be displayed in the center)
                fakeImpostorTeam.Add(CachedPlayer.LocalPlayer.PlayerControl);
                foreach (PlayerControl p in players)
                {
                    if (CachedPlayer.LocalPlayer.PlayerControl != p && (p == Spy.spy || p.Data.Role.IsImpostor))
                        fakeImpostorTeam.Add(p);
                }
                yourTeam = fakeImpostorTeam;
            }
        }

        public static void setupIntroTeam(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(CachedPlayer.LocalPlayer.PlayerControl);
            RoleInfo roleInfo = infos.Where(info => !info.isModifier).FirstOrDefault();
            if (roleInfo == null) return;
            if (roleInfo.isNeutral)
            {
                var neutralColor = new Color32(76, 84, 78, 255);
                __instance.BackgroundBar.material.color = neutralColor;
                __instance.TeamTitle.text = "Neutral";
                __instance.TeamTitle.color = neutralColor;
            }
        }

        public static IEnumerator<WaitForSeconds> EndShowRole(IntroCutscene __instance)
        {
            yield return new WaitForSeconds(5f);
            __instance.YouAreText.gameObject.SetActive(false);
            __instance.RoleText.gameObject.SetActive(false);
            __instance.RoleBlurbText.gameObject.SetActive(false);
            __instance.ourCrewmate.gameObject.SetActive(false);

        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
        class SetUpRoleTextPatch
        {
            static public void SetRoleTexts(IntroCutscene __instance)
            {
                // Don't override the intro of the vanilla roles
                List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(CachedPlayer.LocalPlayer.PlayerControl);
                RoleInfo roleInfo = infos.Where(info => !info.isModifier).FirstOrDefault();
                RoleInfo modifierInfo = infos.Where(info => info.isModifier).FirstOrDefault();
                __instance.RoleBlurbText.text = "";
                if (roleInfo != null)
                {
                    __instance.RoleText.text = roleInfo.name;
                    __instance.RoleText.color = roleInfo.color;
                    __instance.RoleBlurbText.text = roleInfo.introDescription;
                    __instance.RoleBlurbText.color = roleInfo.color;
                }
                if (modifierInfo != null)
                {
                    if (modifierInfo.roleId != RoleId.Lover)
                        __instance.RoleBlurbText.text += Helpers.cs(modifierInfo.color, $"\n{modifierInfo.introDescription}");
                    else
                    {
                        PlayerControl otherLover = CachedPlayer.LocalPlayer.PlayerControl == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                        __instance.RoleBlurbText.text += Helpers.cs(Lovers.color, $"\n♥ You are in love with {otherLover?.Data?.PlayerName ?? ""} ♥");
                    }
                }
                if (Deputy.knowsSheriff && Deputy.deputy != null && Sheriff.sheriff != null)
                {
                    if (infos.Any(info => info.roleId == RoleId.Sheriff))
                        __instance.RoleBlurbText.text += Helpers.cs(Sheriff.color, $"\nYour Deputy is {Deputy.deputy?.Data?.PlayerName ?? ""}");
                    else if (infos.Any(info => info.roleId == RoleId.Deputy))
                        __instance.RoleBlurbText.text += Helpers.cs(Sheriff.color, $"\nYour Sheriff is {Sheriff.sheriff?.Data?.PlayerName ?? ""}");
                }
            }
            public static bool Prefix(IntroCutscene __instance)
            {
                if (!CustomOptionHolder.activateRoles.getBool()) return true;
                FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(1f, new Action<float>((p) =>
                {
                    SetRoleTexts(__instance);
                })));
                return true;
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        class BeginCrewmatePatch
        {
            public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
            {
                setupIntroTeamIcons(__instance, ref teamToDisplay);
            }

            public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
            {
                setupIntroTeam(__instance, ref teamToDisplay);
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        class BeginImpostorPatch
        {
            public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
            {
                setupIntroTeamIcons(__instance, ref yourTeam);
            }

            public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
            {
                setupIntroTeam(__instance, ref yourTeam);
            }
        }
    }

    [HarmonyPatch(typeof(Constants), nameof(Constants.ShouldHorseAround))]
    public static class ShouldAlwaysHorseAround
    {
        public static bool isHorseMode;
        public static bool Prefix(ref bool __result)
        {
            if (isHorseMode != MapOptions.enableHorseMode && LobbyBehaviour.Instance != null) __result = isHorseMode;
            else
            {
                __result = MapOptions.enableHorseMode;
                isHorseMode = MapOptions.enableHorseMode;
            }
            return false;
        }
    }
}

