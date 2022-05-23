using HarmonyLib;
using System;
using static TheOtherRoles.TheOtherRoles;
using TheOtherRoles.Objects;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using TheOtherRoles.Utilities;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch
    {
        public static PoolablePlayer playerPrefab;
        public static void Prefix(IntroCutscene __instance) {
            // Generate and initialize player icons
            int playerCounter = 0;
            if (PlayerControl.LocalPlayer != null && FastDestroyableSingleton<HudManager>.Instance != null) {
                Vector3 bottomLeft = new Vector3(-FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.x, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.y, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.z);
                foreach (PlayerControl p in PlayerControl.AllPlayerControls.GetFastEnumerator()) {
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

                    if (PlayerControl.LocalPlayer == Arsonist.arsonist && p != Arsonist.arsonist) {
			Arsonist.poolIcons.Add(player);
                        player.transform.localPosition = bottomLeft + new Vector3(-0.25f, -0.25f, 0) + Vector3.right * playerCounter++ * 0.35f;
                        player.transform.localScale = Vector3.one * 0.2f;
                        player.setSemiTransparent(true);
                        player.gameObject.SetActive(true);
                    } else if (PlayerControl.LocalPlayer == Amnisiac.amnisiac && p != Amnisiac.amnisiac) {
                        player.transform.localPosition = bottomLeft + new Vector3(-0.25f, -0.25f, 0) + Vector3.right * playerCounter++ * 0.35f;
                        player.transform.localScale = Vector3.one * 0.2f;
                        player.setSemiTransparent(true);
                        player.gameObject.SetActive(false);
                        Amnisiac.poolIcons.Add(player);
                    } else if (PlayerControl.LocalPlayer == BountyHunter.bountyHunter) {
                        player.transform.localPosition = bottomLeft + new Vector3(-0.25f, 0f, 0);
                        player.transform.localScale = Vector3.one * 0.4f;
                        player.gameObject.SetActive(false);
                    } else {
                        player.gameObject.SetActive(false);
                    }
                }
            }

            // Force Bounty Hunter to load a new Bounty when the Intro is over
            if (BountyHunter.bounty != null && PlayerControl.LocalPlayer == BountyHunter.bountyHunter) {
                BountyHunter.bountyUpdateTimer = 0f;
                if (FastDestroyableSingleton<HudManager>.Instance != null) {
                    Vector3 bottomLeft = new Vector3(-FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.x, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.y, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.z) + new Vector3(-0.25f, 1f, 0);
                    BountyHunter.cooldownText = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                    BountyHunter.cooldownText.alignment = TMPro.TextAlignmentOptions.Center;
                    BountyHunter.cooldownText.transform.localPosition = bottomLeft + new Vector3(0f, -1f, -1f);
                    BountyHunter.cooldownText.gameObject.SetActive(true);
                }
            } 

            if (CustomOptionHolder.randomGameStartPosition.getBool()) { //Random spawn on game start

                List<Vector3> skeldSpawn = new List<Vector3>() {
                new Vector3(-2.2f, 2.2f, 0.0f), //cafeteria. botton. top left.
                new Vector3(0.7f, 2.2f, 0.0f), //caffeteria. button. top right.
                new Vector3(-2.2f, -0.2f, 0.0f), //caffeteria. button. bottom left.
                new Vector3(0.7f, -0.2f, 0.0f), //caffeteria. button. bottom right.
                new Vector3(10.0f, 3.0f, 0.0f), //weapons top
                new Vector3(9.0f, 1.0f, 0.0f), //weapons bottom
                new Vector3(6.5f, -3.5f, 0.0f), //O2
                new Vector3(11.5f, -3.5f, 0.0f), //O2-nav hall
                new Vector3(17.0f, -3.5f, 0.0f), //navigation top
                new Vector3(18.2f, -5.7f, 0.0f), //navigation bottom
                new Vector3(11.5f, -6.5f, 0.0f), //nav-shields top
                new Vector3(9.5f, -8.5f, 0.0f), //nav-shields bottom
                new Vector3(9.2f, -12.2f, 0.0f), //shields top
                new Vector3(8.0f, -14.3f, 0.0f), //shields bottom
                new Vector3(2.5f, -16f, 0.0f), //coms left
                new Vector3(4.2f, -16.4f, 0.0f), //coms middle
                new Vector3(5.5f, -16f, 0.0f), //coms right
                new Vector3(-1.5f, -10.0f, 0.0f), //storage top
                new Vector3(-1.5f, -15.5f, 0.0f), //storage bottom
                new Vector3(-4.5f, -12.5f, 0.0f), //storrage left
                new Vector3(0.3f, -12.5f, 0.0f), //storrage right
                new Vector3(4.5f, -7.5f, 0.0f), //admin top
                new Vector3(4.5f, -9.5f, 0.0f), //admin bottom
                new Vector3(-9.0f, -8.0f, 0.0f), //elec top left
                new Vector3(-6.0f, -8.0f, 0.0f), //elec top right
                new Vector3(-8.0f, -11.0f, 0.0f), //elec bottom
                new Vector3(-12.0f, -13.0f, 0.0f), //elec-lower hall
                new Vector3(-17f, -10f, 0.0f), //lower engine top
                new Vector3(-17.0f, -13.0f, 0.0f), //lower engine bottom
                new Vector3(-21.5f, -3.0f, 0.0f), //reactor top
                new Vector3(-21.5f, -8.0f, 0.0f), //reactor bottom
                new Vector3(-13.0f, -3.0f, 0.0f), //security top
                new Vector3(-12.6f, -5.6f, 0.0f), // security bottom
                new Vector3(-17.0f, 2.5f, 0.0f), //upper engibe top
                new Vector3(-17.0f, -1.0f, 0.0f), //upper engine bottom
                new Vector3(-10.5f, 1.0f, 0.0f), //upper-mad hall
                new Vector3(-10.5f, -2.0f, 0.0f), //medbay top
                new Vector3(-6.5f, -4.5f, 0.0f) //medbay bottom
                };

                List<Vector3> miraSpawn = new List<Vector3>() {
                new Vector3(-4.5f, 3.5f, 0.0f), //launchpad top
                new Vector3(-4.5f, -1.4f, 0.0f), //launchpad bottom
                new Vector3(8.5f, -1f, 0.0f), //launchpad- med hall
                new Vector3(14f, -1.5f, 0.0f), //medbay
                new Vector3(16.5f, 3f, 0.0f), // comms
                new Vector3(10f, 5f, 0.0f), //lockers
                new Vector3(6f, 1.5f, 0.0f), //locker room
                new Vector3(2.5f, 13.6f, 0.0f), //reactor
                new Vector3(6f, 12f, 0.0f), //reactor middle
                new Vector3(9.5f, 13f, 0.0f), //lab
                new Vector3(15f, 9f, 0.0f), //bottom left cross
                new Vector3(17.9f, 11.5f, 0.0f), //middle cross
                new Vector3(14f, 17.3f, 0.0f), //office
                new Vector3(19.5f, 21f, 0.0f), //admin
                new Vector3(14f, 24f, 0.0f), //greenhouse left
                new Vector3(22f, 24f, 0.0f), //greenhouse right
                new Vector3(21f, 8.5f, 0.0f), //bottom right cross
                new Vector3(28f, 3f, 0.0f), //caf right
                new Vector3(22f, 3f, 0.0f), //caf left
                new Vector3(19f, 4f, 0.0f), //storage
                new Vector3(22f, -2f, 0.0f), //balcony
                };

                List<Vector3> polusSpawn = new List<Vector3>() {
                new Vector3(16.6f, -1f, 0.0f), //dropship top
                new Vector3(16.6f, -5f, 0.0f), //dropship bottom
                new Vector3(20f, -9f, 0.0f), //above storrage
                new Vector3(22f, -7f, 0.0f), //right fuel
                new Vector3(25.5f, -6.9f, 0.0f), //drill
                new Vector3(29f, -9.5f, 0.0f), //lab lockers
                new Vector3(29.5f, -8f, 0.0f), //lab weather notes
                new Vector3(35f, -7.6f, 0.0f), //lab table
                new Vector3(40.4f, -8f, 0.0f), //lab scan
                new Vector3(33f, -10f, 0.0f), //lab toilet
                new Vector3(39f, -15f, 0.0f), //specimen hall top
                new Vector3(36.5f, -19.5f, 0.0f), //specimen top
                new Vector3(36.5f, -21f, 0.0f), //specimen bottom
                new Vector3(28f, -21f, 0.0f), //specimen hall bottom
                new Vector3(24f, -20.5f, 0.0f), //admin tv
                new Vector3(22f, -25f, 0.0f), //admin books
                new Vector3(16.6f, -17.5f, 0.0f), //office coffe
                new Vector3(22.5f, -16.5f, 0.0f), //office projector
                new Vector3(24f, -17f, 0.0f), //office figure
                new Vector3(27f, -16.5f, 0.0f), //office lifelines
                new Vector3(32.7f, -15.7f, 0.0f), //lavapool
                new Vector3(31.5f, -12f, 0.0f), //snowmad below lab
                new Vector3(10f, -14f, 0.0f), //below storrage
                new Vector3(21.5f, -12.5f, 0.0f), //storrage vent
                new Vector3(19f, -11f, 0.0f), //storrage toolrack
                new Vector3(12f, -7f, 0.0f), //left fuel
                new Vector3(5f, -7.5f, 0.0f), //above elec
                new Vector3(10f, -12f, 0.0f), //elec fence
                new Vector3(9f, -9f, 0.0f), //elec lockers
                new Vector3(5f, -9f, 0.0f), //elec window
                new Vector3(4f, -11.2f, 0.0f), //elec tapes
                new Vector3(5.5f, -16f, 0.0f), //elec-O2 hall
                new Vector3(1f, -17.5f, 0.0f), //O2 tree hayball
                new Vector3(3f, -21f, 0.0f), //O2 middle
                new Vector3(2f, -19f, 0.0f), //O2 gas
                new Vector3(1f, -24f, 0.0f), //O2 water
                new Vector3(7f, -24f, 0.0f), //under O2
                new Vector3(9f, -20f, 0.0f), //right outside of O2
                new Vector3(7f, -15.8f, 0.0f), //snowman under elec
                new Vector3(11f, -17f, 0.0f), //comms table
                new Vector3(12.7f, -15.5f, 0.0f), //coms antenna pult
                new Vector3(13f, -24.5f, 0.0f), //weapons window
                new Vector3(15f, -17f, 0.0f), //between coms-office
                new Vector3(17.5f, -25.7f, 0.0f), //snowman under office
                };

                List<Vector3> dleksSpawn = new List<Vector3>() {
                new Vector3(2.2f, 2.2f, 0.0f), //cafeteria. botton. top left.
                new Vector3(-0.7f, 2.2f, 0.0f), //caffeteria. button. top right.
                new Vector3(2.2f, -0.2f, 0.0f), //caffeteria. button. bottom left.
                new Vector3(-0.7f, -0.2f, 0.0f), //caffeteria. button. bottom right.
                new Vector3(-10.0f, 3.0f, 0.0f), //weapons top
                new Vector3(-9.0f, 1.0f, 0.0f), //weapons bottom
                new Vector3(-6.5f, -3.5f, 0.0f), //O2
                new Vector3(-11.5f, -3.5f, 0.0f), //O2-nav hall
                new Vector3(-17.0f, -3.5f, 0.0f), //navigation top
                new Vector3(-18.2f, -5.7f, 0.0f), //navigation bottom
                new Vector3(-11.5f, -6.5f, 0.0f), //nav-shields top
                new Vector3(-9.5f, -8.5f, 0.0f), //nav-shields bottom
                new Vector3(-9.2f, -12.2f, 0.0f), //shields top
                new Vector3(-8.0f, -14.3f, 0.0f), //shields bottom
                new Vector3(-2.5f, -16f, 0.0f), //coms left
                new Vector3(-4.2f, -16.4f, 0.0f), //coms middle
                new Vector3(-5.5f, -16f, 0.0f), //coms right
                new Vector3(1.5f, -10.0f, 0.0f), //storage top
                new Vector3(1.5f, -15.5f, 0.0f), //storage bottom
                new Vector3(4.5f, -12.5f, 0.0f), //storrage left
                new Vector3(-0.3f, -12.5f, 0.0f), //storrage right
                new Vector3(-4.5f, -7.5f, 0.0f), //admin top
                new Vector3(-4.5f, -9.5f, 0.0f), //admin bottom
                new Vector3(9.0f, -8.0f, 0.0f), //elec top left
                new Vector3(6.0f, -8.0f, 0.0f), //elec top right
                new Vector3(8.0f, -11.0f, 0.0f), //elec bottom
                new Vector3(12.0f, -13.0f, 0.0f), //elec-lower hall
                new Vector3(17f, -10f, 0.0f), //lower engine top
                new Vector3(17.0f, -13.0f, 0.0f), //lower engine bottom
                new Vector3(21.5f, -3.0f, 0.0f), //reactor top
                new Vector3(21.5f, -8.0f, 0.0f), //reactor bottom
                new Vector3(13.0f, -3.0f, 0.0f), //security top
                new Vector3(12.6f, -5.6f, 0.0f), // security bottom
                new Vector3(17.0f, 2.5f, 0.0f), //upper engibe top
                new Vector3(17.0f, -1.0f, 0.0f), //upper engine bottom
                new Vector3(10.5f, 1.0f, 0.0f), //upper-mad hall
                new Vector3(10.5f, -2.0f, 0.0f), //medbay top
                new Vector3(6.5f, -4.5f, 0.0f) //medbay bottom
                };

                List<Vector3> airshipSpawn = new List<Vector3>() { }; //no spawns since it already has random spawns

                if (PlayerControl.GameOptions.MapId == 0) PlayerControl.LocalPlayer.transform.position = skeldSpawn[rnd.Next(skeldSpawn.Count)];
                if (PlayerControl.GameOptions.MapId == 1) PlayerControl.LocalPlayer.transform.position = miraSpawn[rnd.Next(miraSpawn.Count)];
                if (PlayerControl.GameOptions.MapId == 2) PlayerControl.LocalPlayer.transform.position = polusSpawn[rnd.Next(polusSpawn.Count)];
                if (PlayerControl.GameOptions.MapId == 3) PlayerControl.LocalPlayer.transform.position = dleksSpawn[rnd.Next(dleksSpawn.Count)];
                if (PlayerControl.GameOptions.MapId == 4) PlayerControl.LocalPlayer.transform.position = airshipSpawn[rnd.Next(airshipSpawn.Count)];

            }
            if (CustomOptionHolder.resetRoundStartCooldown.getBool()) { //reset cooldown on round start
                PlayerControl.LocalPlayer.killTimer = 60; //set imp cooldown to the max
                CustomButton.ResetAllCooldowns(); //reset button cooldowns
            }

            // First kill
            if (AmongUsClient.Instance.AmHost && MapOptions.shieldFirstKill && MapOptions.firstKillName != "") {
                PlayerControl target = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(MapOptions.firstKillName));
                if (target != null) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetFirstKill, Hazel.SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.setFirstKill(target.PlayerId);
                }
            }
            MapOptions.firstKillName = "";
        }
    }

    [HarmonyPatch]
    class IntroPatch {
        public static void setupIntroTeamIcons(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
            // Intro solo teams
            if (PlayerControl.LocalPlayer == Jester.jester || PlayerControl.LocalPlayer == Jackal.jackal || PlayerControl.LocalPlayer == Arsonist.arsonist || PlayerControl.LocalPlayer == Vulture.vulture || PlayerControl.LocalPlayer == Lawyer.lawyer) {
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(PlayerControl.LocalPlayer);
                yourTeam = soloTeam;
            }
			
            // Intro Exe and Target
            if (PlayerControl.LocalPlayer == Prosecutor.prosecutor) {
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(PlayerControl.LocalPlayer);
				soloTeam.Add(Prosecutor.target);
                yourTeam = soloTeam;
            }

            // Add the Spy to the Impostor team (for the Impostors)
            if (Spy.spy != null && PlayerControl.LocalPlayer.Data.Role.IsImpostor) {
                List<PlayerControl> players = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
                var fakeImpostorTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>(); // The local player always has to be the first one in the list (to be displayed in the center)
                fakeImpostorTeam.Add(PlayerControl.LocalPlayer);
                foreach (PlayerControl p in players) {
                    if (PlayerControl.LocalPlayer != p && (p == Spy.spy || p.Data.Role.IsImpostor))
                        fakeImpostorTeam.Add(p);
                }
                yourTeam = fakeImpostorTeam;
            }
        }

        public static void setupIntroTeam(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
            List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
            RoleInfo roleInfo = infos.Where(info => !info.isModifier).FirstOrDefault();
            if (roleInfo == null) return;
            if (roleInfo.isNeutral) {
                var neutralColor = new Color32(76, 84, 78, 255);
                __instance.BackgroundBar.material.color = roleInfo.color;
                __instance.TeamTitle.text = roleInfo.name;
                __instance.TeamTitle.color = roleInfo.color;
            } else {
                bool isCrew = true;
                if (roleInfo.color == Palette.ImpostorRed) isCrew = false;
                if (isCrew) {
                    __instance.BackgroundBar.material.color = roleInfo.color;
                    __instance.TeamTitle.text = "Crewmate";
                    __instance.TeamTitle.color = Color.cyan;
                } else {
                    __instance.BackgroundBar.material.color = roleInfo.color;
                    __instance.TeamTitle.text = "Impostor";
                    __instance.TeamTitle.color = Palette.ImpostorRed;
                }
            }
        }

        public static IEnumerator<WaitForSeconds> EndShowRole(IntroCutscene __instance) {
            yield return new WaitForSeconds(5f);
            __instance.YouAreText.gameObject.SetActive(false);
            __instance.RoleText.gameObject.SetActive(false);
            __instance.RoleBlurbText.gameObject.SetActive(false);
            __instance.ourCrewmate.gameObject.SetActive(false);
           
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
        class SetUpRoleTextPatch {
            static public void SetRoleTexts(IntroCutscene __instance) {
                // Don't override the intro of the vanilla roles
                List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
                RoleInfo roleInfo = infos.Where(info => !info.isModifier).FirstOrDefault();
                RoleInfo modifierInfo = infos.Where(info => info.isModifier).FirstOrDefault();
                __instance.RoleBlurbText.text = "";
                if (roleInfo != null) {
                    __instance.RoleText.text = roleInfo.name;
                    __instance.RoleText.color = roleInfo.color;
                    __instance.RoleBlurbText.text = roleInfo.introDescription;
                    __instance.RoleBlurbText.color = roleInfo.color;
                }
                if (modifierInfo != null) {
                    if (modifierInfo.roleId != RoleId.Lover)
                        __instance.RoleBlurbText.text += Helpers.cs(modifierInfo.color, $"\n{modifierInfo.introDescription}");
                    else {
                        PlayerControl otherLover = PlayerControl.LocalPlayer == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                        __instance.RoleBlurbText.text += Helpers.cs(Lovers.color, $"\n♥ You are in love with {otherLover?.Data?.PlayerName ?? ""} ♥");
                    }
                }
				
				if (infos.Any(info => info.roleId == RoleId.Prosecutor)) {
                    PlayerControl target = Prosecutor.target;
                    __instance.RoleBlurbText.text += Helpers.cs(Prosecutor.color, $"\nVote out {target?.Data?.PlayerName ?? ""} ");
                }

                if (Deputy.knowsSheriff && Deputy.deputy != null && Sheriff.sheriff != null) {
                    if (infos.Any(info => info.roleId == RoleId.Sheriff))
                        __instance.RoleBlurbText.text += Helpers.cs(Sheriff.color, $"\nYour Deputy is {Deputy.deputy?.Data?.PlayerName ?? ""}");
                    else if (infos.Any(info => info.roleId == RoleId.Deputy))
                        __instance.RoleBlurbText.text += Helpers.cs(Sheriff.color, $"\nYour Sheriff is {Sheriff.sheriff?.Data?.PlayerName ?? ""}");
                }
            }
            public static bool Prefix(IntroCutscene __instance) {
                if (!CustomOptionHolder.activateRoles.getBool()) return true;
                FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(1f, new Action<float>((p) => {
                    SetRoleTexts(__instance);
                })));
                return true;
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        class BeginCrewmatePatch {
            public static void Prefix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay) {
                setupIntroTeamIcons(__instance, ref teamToDisplay);
            }

            public static void Postfix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay) {
                setupIntroTeam(__instance, ref teamToDisplay);
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        class BeginImpostorPatch {
            public static void Prefix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
                setupIntroTeamIcons(__instance, ref yourTeam);
            }

            public static void Postfix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
                setupIntroTeam(__instance, ref yourTeam);
            }
        }
    }

    [HarmonyPatch(typeof(Constants), nameof(Constants.ShouldHorseAround))]
    public static class ShouldAlwaysHorseAround {
        public static bool isHorseMode;
        public static bool Prefix(ref bool __result) {
            if (isHorseMode != MapOptions.enableHorseMode && LobbyBehaviour.Instance != null) __result = isHorseMode;
            else {
                __result = MapOptions.enableHorseMode;
                isHorseMode = MapOptions.enableHorseMode;
            }
            return false;
        }
    }
}

