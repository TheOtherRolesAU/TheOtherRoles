using HarmonyLib;
using System;
using System.IO;
using System.Net.Http;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;
using System.Collections.Generic;
using System.Linq;

using Palette = GLNPIJPGGNJ;

namespace TheOtherRoles
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerUpdatePatch
    {
        static void resetNameTagsAndColors() {
            Dictionary<byte, PlayerControl> playersById = Helpers.allPlayersById();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                player.nameText.Text = player.IDOFAMCIJKE.HGGCLJHCDBM;
                if (PlayerControl.LocalPlayer.IDOFAMCIJKE.CIDDOFDJHJH && player.IDOFAMCIJKE.CIDDOFDJHJH) {
                    player.nameText.Color = Palette.LDCHDOFJPGH;
                } else {
                    player.nameText.Color = Color.white;
                }
            }
            if (MeetingHud.Instance != null) {
                foreach (PlayerVoteArea player in MeetingHud.Instance.DHCOPOOJCLN) {
                    PlayerControl playerControl = playersById.ContainsKey((byte)player.HMPHKKGPLAG) ? playersById[(byte)player.HMPHKKGPLAG] : null;
                    if (playerControl != null) {
                        player.NameText.Text = playerControl.IDOFAMCIJKE.HGGCLJHCDBM;
                        if (PlayerControl.LocalPlayer.IDOFAMCIJKE.CIDDOFDJHJH && playerControl.IDOFAMCIJKE.CIDDOFDJHJH) {
                            player.NameText.Color = Palette.LDCHDOFJPGH;
                        } else {
                            player.NameText.Color = Color.white;
                        }
                    }
                }
            }
            if (PlayerControl.LocalPlayer.IDOFAMCIJKE.CIDDOFDJHJH) {
                List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList();
                impostors.RemoveAll(x => !x.IDOFAMCIJKE.CIDDOFDJHJH);
                foreach (PlayerControl player in impostors)
                    player.nameText.Color = Palette.LDCHDOFJPGH;
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.DHCOPOOJCLN) {
                        PlayerControl playerControl = Helpers.playerById((byte)player.HMPHKKGPLAG);
                        if (playerControl != null && playerControl.IDOFAMCIJKE.CIDDOFDJHJH)
                            player.NameText.Color =  Palette.LDCHDOFJPGH;
                    }
            }

        }

        static void setPlayerNameColor(PlayerControl p, Color color) {
            p.nameText.Color = color;
            if (MeetingHud.Instance != null)
                foreach (PlayerVoteArea player in MeetingHud.Instance.DHCOPOOJCLN)
                    if (player.NameText != null && p.PlayerId == player.HMPHKKGPLAG)
                        player.NameText.Color = color;
        }

        static void setNameColors() {
            if (Jester.jester != null && Jester.jester == PlayerControl.LocalPlayer)
                setPlayerNameColor(Jester.jester, Jester.color);
            else if (Mayor.mayor != null && Mayor.mayor == PlayerControl.LocalPlayer)
                setPlayerNameColor(Mayor.mayor, Mayor.color);
            else if (Engineer.engineer != null && Engineer.engineer == PlayerControl.LocalPlayer)
                setPlayerNameColor(Engineer.engineer, Engineer.color);
            else if (Sheriff.sheriff != null && Sheriff.sheriff == PlayerControl.LocalPlayer) 
                setPlayerNameColor(Sheriff.sheriff, Sheriff.color);
            else if (Lighter.lighter != null && Lighter.lighter == PlayerControl.LocalPlayer) 
                setPlayerNameColor(Lighter.lighter, Lighter.color);
            else if (Detective.detective != null && Detective.detective == PlayerControl.LocalPlayer) 
                setPlayerNameColor(Detective.detective, Detective.color);
            else if (TimeMaster.timeMaster != null && TimeMaster.timeMaster == PlayerControl.LocalPlayer)
                setPlayerNameColor(TimeMaster.timeMaster, TimeMaster.color);
            else if (Medic.medic != null && Medic.medic == PlayerControl.LocalPlayer)
                setPlayerNameColor(Medic.medic, Medic.color);
            else if (Shifter.shifter != null && Shifter.shifter == PlayerControl.LocalPlayer)
                setPlayerNameColor(Shifter.shifter, Shifter.color);
            else if (Swapper.swapper != null && Swapper.swapper == PlayerControl.LocalPlayer)
                setPlayerNameColor(Swapper.swapper, Swapper.color);
            else if (Lovers.lover1 != null && Lovers.lover2 != null && (Lovers.lover1 == PlayerControl.LocalPlayer || Lovers.lover2 == PlayerControl.LocalPlayer)) {             
                setPlayerNameColor(Lovers.lover1, Lovers.color);
                setPlayerNameColor(Lovers.lover2, Lovers.color);
            }
            else if (Seer.seer != null && Seer.seer == PlayerControl.LocalPlayer)
                setPlayerNameColor(Seer.seer, Seer.color);  
            else if (Hacker.hacker != null && Hacker.hacker == PlayerControl.LocalPlayer) 
                setPlayerNameColor(Hacker.hacker, Hacker.color);
            else if (Tracker.tracker != null && Tracker.tracker == PlayerControl.LocalPlayer) 
                setPlayerNameColor(Tracker.tracker, Tracker.color);
            else if (Snitch.snitch != null && Snitch.snitch == PlayerControl.LocalPlayer) 
                setPlayerNameColor(Snitch.snitch, Snitch.color);
            else if (Jackal.jackal != null && Jackal.jackal == PlayerControl.LocalPlayer) {
                // Jackal can see his sidekick
                setPlayerNameColor(Jackal.jackal, Jackal.color);
                if (Sidekick.sidekick != null) {
                    setPlayerNameColor(Sidekick.sidekick, Jackal.color);
                }
                if (Jackal.fakeSidekick != null) {
                    setPlayerNameColor(Jackal.fakeSidekick, Jackal.color);
                }
            }
            else if (Spy.spy != null && Spy.spy == PlayerControl.LocalPlayer) {
                setPlayerNameColor(Spy.spy, Spy.color);
            }
            
            // No else if here, as a Lover of team Jackal needs the colors
            if (Sidekick.sidekick != null && Sidekick.sidekick == PlayerControl.LocalPlayer) {
                // Sidekick can see the jackal
                setPlayerNameColor(Sidekick.sidekick, Sidekick.color);
                if (Jackal.jackal != null) {
                    setPlayerNameColor(Jackal.jackal, Jackal.color);
                }
            }

            // No else if here, as the Impostors need the Spy name to be colored
            if (Spy.spy != null && PlayerControl.LocalPlayer.IDOFAMCIJKE.CIDDOFDJHJH) {
                setPlayerNameColor(Spy.spy, Spy.color);
            }

            // Crewmate roles with no changes: Child
            // Impostor roles with no changes: Morphling, Camouflager, Vampire, Godfather, Eraser, Janitor and Mafioso
        }

        static void setMafiaNameTags() {
            if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.IDOFAMCIJKE.CIDDOFDJHJH) {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    if (Godfather.godfather != null && Godfather.godfather == player)
                            player.nameText.Text = player.IDOFAMCIJKE.HGGCLJHCDBM + " (G)";
                    else if (Mafioso.mafioso != null && Mafioso.mafioso == player)
                            player.nameText.Text = player.IDOFAMCIJKE.HGGCLJHCDBM + " (M)";
                    else if (Janitor.janitor != null && Janitor.janitor == player)
                            player.nameText.Text = player.IDOFAMCIJKE.HGGCLJHCDBM + " (J)";
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.DHCOPOOJCLN)
                        if (Godfather.godfather != null && Godfather.godfather.PlayerId == player.HMPHKKGPLAG)
                            player.NameText.Text = Godfather.godfather.IDOFAMCIJKE.HGGCLJHCDBM + " (G)";
                        else if (Mafioso.mafioso != null && Mafioso.mafioso.PlayerId == player.HMPHKKGPLAG)
                            player.NameText.Text = Mafioso.mafioso.IDOFAMCIJKE.HGGCLJHCDBM + " (M)";
                        else if (Janitor.janitor != null && Janitor.janitor.PlayerId == player.HMPHKKGPLAG)
                            player.NameText.Text = Janitor.janitor.IDOFAMCIJKE.HGGCLJHCDBM + " (J)";
            }
        }

        static void updateShielded() {
            if (Medic.shielded == null) return;

            if(Medic.showShielded == 0) // Everyone
            {
                Medic.shielded.LNMJKMLHMIM.material.SetFloat("_Outline",  1f);
                Medic.shielded.LNMJKMLHMIM.material.SetColor("_OutlineColor", Medic.shieldedColor);
            }
            else if (Medic.showShielded == 1 && PlayerControl.LocalPlayer == Medic.shielded) // Shielded + Medic
            {
                Medic.shielded.LNMJKMLHMIM.material.SetFloat("_Outline", 1f);
                Medic.shielded.LNMJKMLHMIM.material.SetColor("_OutlineColor", Medic.shieldedColor);
            }
            else if(PlayerControl.LocalPlayer == Medic.medic) // Medic
            {
                Medic.shielded.LNMJKMLHMIM.material.SetFloat("_Outline",  1f);
                Medic.shielded.LNMJKMLHMIM.material.SetColor("_OutlineColor", Medic.shieldedColor);
            }

            // Break shield
            if (Medic.shielded.IDOFAMCIJKE.FGNJJFABIHJ || Medic.medic == null || Medic.medic.IDOFAMCIJKE.FGNJJFABIHJ) {
                Medic.shielded.LNMJKMLHMIM.material.SetFloat("_Outline", 0f);
                Medic.shielded = null;
            }
        }

        static void mafiosoDeactivateKillButtonIfNecessary(HudManager __instance) {
            if (Mafioso.mafioso == null || Mafioso.mafioso != PlayerControl.LocalPlayer) return;

            if (!PlayerControl.LocalPlayer.IDOFAMCIJKE.FGNJJFABIHJ && __instance.UseButton.isActiveAndEnabled && Godfather.godfather != null && Godfather.godfather.IDOFAMCIJKE.FGNJJFABIHJ) {
                __instance.KillButton.gameObject.SetActive(true);
                __instance.KillButton.renderer.enabled = true;
                __instance.KillButton.isActive = true;
                __instance.KillButton.enabled = true;
            } else {
                __instance.KillButton.gameObject.SetActive(false);
                __instance.KillButton.renderer.enabled = false;
                __instance.KillButton.isActive = false;
                __instance.KillButton.enabled = false;
            }
        }

        static void janitorDeactivateKillButton(HudManager __instance) {
            if (Janitor.janitor != null && Janitor.janitor == PlayerControl.LocalPlayer) {
                __instance.KillButton.gameObject.SetActive(false);
                __instance.KillButton.renderer.enabled = false;
                __instance.KillButton.isActive = false;
                __instance.KillButton.enabled = false;
            }
        }

        static void timerUpdate() {
            Hacker.hackerTimer -= Time.deltaTime;
            Lighter.lighterTimer -= Time.deltaTime;
            Trickster.lightsOutTimer -= Time.deltaTime;
        }

        static void camouflageAndMorphActions() {
            float oldCamouflageTimer = Camouflager.camouflageTimer;
            float oldMorphTimer = Morphling.morphTimer;

            Camouflager.camouflageTimer -= Time.deltaTime;
            Morphling.morphTimer -= Time.deltaTime;

            // Morphling player size not done here

            // Set morphling morphed look
            if (Morphling.morphTimer > 0f && Camouflager.camouflageTimer <= 0f) {
                if (Morphling.morphling != null && Morphling.morphTarget != null) {
                    Morphling.morphling.nameText.Text = Morphling.morphTarget.IDOFAMCIJKE.HGGCLJHCDBM;
                    Morphling.morphling.LNMJKMLHMIM.material.SetColor("_BackColor", Palette.CHIIBPFJACF[Morphling.morphTarget.IDOFAMCIJKE.JFHFMIKFHGG]);
                    Morphling.morphling.LNMJKMLHMIM.material.SetColor("_BodyColor", Palette.CALCLMEEPGL[Morphling.morphTarget.IDOFAMCIJKE.JFHFMIKFHGG]);
                    Morphling.morphling.LNMJKMLHMIM.material.SetFloat("_Outline",  Morphling.morphTarget.LNMJKMLHMIM.material.GetFloat("_Outline"));
                    Morphling.morphling.LNMJKMLHMIM.material.SetColor("_OutlineColor", Morphling.morphTarget.LNMJKMLHMIM.material.GetColor("_OutlineColor"));
                    Morphling.morphling.HatRenderer.SetHat(Morphling.morphTarget.IDOFAMCIJKE.MFIOGLKPMGD, Morphling.morphTarget.IDOFAMCIJKE.JFHFMIKFHGG);
                    Morphling.morphling.nameText.transform.localPosition = new Vector3(0f, (Morphling.morphTarget.IDOFAMCIJKE.MFIOGLKPMGD == 0U) ? 0.7f : 1.05f, -0.5f);

                    if (Morphling.morphling.MyPhysics.Skin.skin.ProdId != DestroyableSingleton<HatManager>.CMJOLNCMAPD.AllSkins[(int)Morphling.morphTarget.IDOFAMCIJKE.LFDAHOFPIAM].ProdId) {
                        Helpers.setSkinWithAnim(Morphling.morphling.MyPhysics, Morphling.morphTarget.IDOFAMCIJKE.LFDAHOFPIAM);
                    }
                    if (Morphling.morphling.CurrentPet == null || Morphling.morphling.CurrentPet.EKONGILOOPE != DestroyableSingleton<HatManager>.CMJOLNCMAPD.AllPets[(int)Morphling.morphTarget.IDOFAMCIJKE.GKFOHNJHFOE].EKONGILOOPE) {
                        if (Morphling.morphling.CurrentPet) UnityEngine.Object.Destroy(Morphling.morphling.CurrentPet.gameObject);
                        Morphling.morphling.CurrentPet = UnityEngine.Object.Instantiate<PetBehaviour>(DestroyableSingleton<HatManager>.CMJOLNCMAPD.AllPets[(int)Morphling.morphTarget.IDOFAMCIJKE.GKFOHNJHFOE]);
                        Morphling.morphling.CurrentPet.transform.position = Morphling.morphling.transform.position;
                        Morphling.morphling.CurrentPet.Source = Morphling.morphling;
                        Morphling.morphling.CurrentPet.FHBHBMIJFID = Morphling.morphling.FHBHBMIJFID;
                        PlayerControl.SetPlayerMaterialColors(Morphling.morphTarget.IDOFAMCIJKE.JFHFMIKFHGG, Morphling.morphling.CurrentPet.rend);
                    } else if (Morphling.morphling.CurrentPet) {
                        PlayerControl.SetPlayerMaterialColors(Morphling.morphTarget.IDOFAMCIJKE.JFHFMIKFHGG, Morphling.morphling.CurrentPet.rend);
                    }
                }
            }

            // Set camouflaged look (overrides morphling morphed look if existent)
            if (Camouflager.camouflageTimer > 0f) {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                    p.nameText.Text = "";
                    p.LNMJKMLHMIM.material.SetColor("_BackColor", Palette.CALCLMEEPGL[6]);
                    p.LNMJKMLHMIM.material.SetColor("_BodyColor", Palette.CALCLMEEPGL[6]);
                    p.LNMJKMLHMIM.material.SetFloat("_Outline",  0f);
                    p.HatRenderer.SetHat(0, 0);
                    Helpers.setSkinWithAnim(p.MyPhysics, 0);
                    bool spawnPet = false;
                    if (p.CurrentPet == null) spawnPet = true;
                    else if (p.CurrentPet.EKONGILOOPE != DestroyableSingleton<HatManager>.CMJOLNCMAPD.AllPets[0].EKONGILOOPE) {
                        UnityEngine.Object.Destroy(p.CurrentPet.gameObject);
                        spawnPet = true;
                    }

                    if (spawnPet) {
                        p.CurrentPet = UnityEngine.Object.Instantiate<PetBehaviour>(DestroyableSingleton<HatManager>.CMJOLNCMAPD.AllPets[0]);
                        p.CurrentPet.transform.position = p.transform.position;
                        p.CurrentPet.Source = p;
                    }
                }
            } 
            
            // Everyone but morphling reset
            if (oldCamouflageTimer > 0f && Camouflager.camouflageTimer <= 0f) {
                Camouflager.resetCamouflage();
            }

            // Morphling reset
            if ((oldMorphTimer > 0f || oldCamouflageTimer > 0f) && Camouflager.camouflageTimer <= 0f && Morphling.morphTimer <= 0f && Morphling.morphling != null) {
                Morphling.resetMorph();
            }
        }

        public static void childUpdate() {
            if (Child.child == null || Camouflager.camouflageTimer > 0f) return;
                
            float growingProgress = Child.growingProgress();
            float scale = growingProgress * 0.35f + 0.35f;
            string suffix = "";
            if (growingProgress != 1f)
                suffix = " [FAD934FF](" + Mathf.FloorToInt(growingProgress * 18) + ")"; 

            Child.child.nameText.Text += suffix;
            if (MeetingHud.Instance != null) {
                foreach (PlayerVoteArea player in MeetingHud.Instance.DHCOPOOJCLN)
                    if (player.NameText != null && Child.child.PlayerId == player.HMPHKKGPLAG)
                        player.NameText.Text += suffix;
            }

            if (Morphling.morphling != null && Morphling.morphTarget == Child.child && Morphling.morphTimer > 0f)
                Morphling.morphling.nameText.Text += suffix;
        }

        static void vampireDeactivateKillButton(HudManager __instance) {
            if (Vampire.vampire != null && Vampire.vampire == PlayerControl.LocalPlayer) {
                __instance.KillButton.gameObject.SetActive(false);
                __instance.KillButton.renderer.enabled = false;
                __instance.KillButton.isActive = false;
                __instance.KillButton.enabled = false;
            }
        }

        static void snitchUpdate() {
            if (Snitch.localArrows == null) return;

            foreach (Arrow arrow in Snitch.localArrows) arrow.arrow.SetActive(false);

            if (Snitch.snitch == null || Snitch.snitch.IDOFAMCIJKE.FGNJJFABIHJ) return;

            int numberOfTasks = 0;
            GameData.OFKOJOKOOAK OFKOJOKOOAK = Snitch.snitch.IDOFAMCIJKE;
			if (!OFKOJOKOOAK.GBPMEHJFECK && OFKOJOKOOAK.CLLMDIJHALO != null) {
				for (int i = 0; i < OFKOJOKOOAK.CLLMDIJHALO.Count; i++) {
					if (!OFKOJOKOOAK.CLLMDIJHALO[i].FNCBODEIJCN)
						numberOfTasks++;
				}
			}

            if (PlayerControl.LocalPlayer.IDOFAMCIJKE.CIDDOFDJHJH && numberOfTasks <= Snitch.taskCountForImpostors) {
                if (Snitch.localArrows.Count == 0) Snitch.localArrows.Add(new Arrow(Color.blue));
                if (Snitch.localArrows.Count != 0 && Snitch.localArrows[0] != null) {
                    Snitch.localArrows[0].arrow.SetActive(true);
                    Snitch.localArrows[0].Update(Snitch.snitch.transform.position);
                }
            } else if (PlayerControl.LocalPlayer == Snitch.snitch && numberOfTasks == 0) { 
                int arrowIndex = 0;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                    if (p.IDOFAMCIJKE.CIDDOFDJHJH && !p.IDOFAMCIJKE.FGNJJFABIHJ) {
                        if (arrowIndex >= Snitch.localArrows.Count) Snitch.localArrows.Add(new Arrow(Color.blue));
                        if (arrowIndex < Snitch.localArrows.Count && Snitch.localArrows[arrowIndex] != null) {
                            Snitch.localArrows[arrowIndex].arrow.SetActive(true);
                            Snitch.localArrows[arrowIndex].Update(p.transform.position);
                        }
                        arrowIndex++;
                    }
                }
            }
        }

        static void Postfix(HudManager __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.CJDCOJJNIGL.Started) return;

            CustomButton.HudUpdate();
            resetNameTagsAndColors();
            setNameColors();
            updateShielded();

            // Mafia
            setMafiaNameTags();
            // Jester
            Helpers.removeTasksFromPlayer(Jester.jester);
            // Mafioso
            mafiosoDeactivateKillButtonIfNecessary(__instance);
            // Janitor
            janitorDeactivateKillButton(__instance);
            // Timer updates
            timerUpdate();
            // Camouflager and Morphling
            camouflageAndMorphActions();
            // Child
            childUpdate();
            // Vampire
            vampireDeactivateKillButton(__instance);
            // Snitch
            snitchUpdate();
            // Jackal
            Helpers.removeTasksFromPlayer(Jackal.jackal);
            // Sidekick
            Helpers.removeTasksFromPlayer(Sidekick.sidekick);
        }
    }
}