using HarmonyLib;
using System;
using System.IO;
using System.Net.Http;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;
using Reactor.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace TheOtherRoles
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerUpdatePatch
    {
        static void resetNameTagsAndColors() {
            Dictionary<byte, PlayerControl> playersById = Helpers.allPlayersById();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                player.nameText.Text = player.Data.PlayerName;
                if (PlayerControl.LocalPlayer.Data.IsImpostor && player.Data.IsImpostor) {
                    player.nameText.Color = Palette.ImpostorRed;
                } else {
                    player.nameText.Color = Color.white;
                }
            }
            if (MeetingHud.Instance != null) {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates) {
                    PlayerControl playerControl = playersById.ContainsKey((byte)player.TargetPlayerId) ? playersById[(byte)player.TargetPlayerId] : null;
                    if (playerControl != null) {
                        player.NameText.Text = playerControl.Data.PlayerName;
                        if (PlayerControl.LocalPlayer.Data.IsImpostor && playerControl.Data.IsImpostor) {
                            player.NameText.Color = Palette.ImpostorRed;
                        } else {
                            player.NameText.Color = Color.white;
                        }
                    }
                }
            }
            if (PlayerControl.LocalPlayer.Data.IsImpostor) {
                List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList();
                impostors.RemoveAll(x => !x.Data.IsImpostor);
                foreach (PlayerControl player in impostors)
                    player.nameText.Color = Palette.ImpostorRed;
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates) {
                        PlayerControl playerControl = Helpers.playerById((byte)player.TargetPlayerId);
                        if (playerControl != null && playerControl.Data.IsImpostor)
                            player.NameText.Color =  Palette.ImpostorRed;
                    }
            }

        }

        static void setPlayerNameColor(PlayerControl p, Color color) {
            p.nameText.Color = color;
            if (MeetingHud.Instance != null)
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && p.PlayerId == player.TargetPlayerId)
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
            else if (Spy.spy != null && Spy.spy == PlayerControl.LocalPlayer) 
                setPlayerNameColor(Spy.spy, Spy.color);
            else if (BountyHunter.bountyHunter != null && BountyHunter.target != null && BountyHunter.bountyHunter == PlayerControl.LocalPlayer) {
                setPlayerNameColor(BountyHunter.bountyHunter, BountyHunter.color);
                setPlayerNameColor(BountyHunter.target, BountyHunter.color);
            }
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
            
            // No else if here, as a Lover of team Jackal needs the colors
            if (Sidekick.sidekick != null && Sidekick.sidekick == PlayerControl.LocalPlayer) {
                // Sidekick can see the jackal
                setPlayerNameColor(Sidekick.sidekick, Sidekick.color);
                if (Jackal.jackal != null) {
                    setPlayerNameColor(Jackal.jackal, Jackal.color);
                }
            }

            // Crewmate roles with no changes: Child
            // Impostor roles with no changes: Morphling, Camouflager, Vampire, Godfather, Janitor and Mafioso
        }

        static void setMafiaNameTags() {
            if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.IsImpostor) {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    if (Godfather.godfather != null && Godfather.godfather == player)
                            player.nameText.Text = player.Data.PlayerName + " (G)";
                    else if (Mafioso.mafioso != null && Mafioso.mafioso == player)
                            player.nameText.Text = player.Data.PlayerName + " (M)";
                    else if (Janitor.janitor != null && Janitor.janitor == player)
                            player.nameText.Text = player.Data.PlayerName + " (J)";
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (Godfather.godfather != null && Godfather.godfather.PlayerId == player.TargetPlayerId)
                            player.NameText.Text = Godfather.godfather.Data.PlayerName + " (G)";
                        else if (Mafioso.mafioso != null && Mafioso.mafioso.PlayerId == player.TargetPlayerId)
                            player.NameText.Text = Mafioso.mafioso.Data.PlayerName + " (M)";
                        else if (Janitor.janitor != null && Janitor.janitor.PlayerId == player.TargetPlayerId)
                            player.NameText.Text = Janitor.janitor.Data.PlayerName + " (J)";
            }
        }

        static void updateShielded() {
            if (Medic.shielded == null) return;

            if(Medic.showShielded == 0) // Everyone
            {
                Medic.shielded.myRend.material.SetFloat("_Outline",  1f);
                Medic.shielded.myRend.material.SetColor("_OutlineColor", Medic.shieldedColor);
            }
            else if (Medic.showShielded == 1 && PlayerControl.LocalPlayer == Medic.shielded) // Shielded + Medic
            {
                Medic.shielded.myRend.material.SetFloat("_Outline", 1f);
                Medic.shielded.myRend.material.SetColor("_OutlineColor", Medic.shieldedColor);
            }
            else if(PlayerControl.LocalPlayer == Medic.medic) // Medic
            {
                Medic.shielded.myRend.material.SetFloat("_Outline",  1f);
                Medic.shielded.myRend.material.SetColor("_OutlineColor", Medic.shieldedColor);
            }

            // Break shield
            if (Medic.shielded.Data.IsDead || Medic.medic == null || Medic.medic.Data.IsDead) {
                Medic.shielded.myRend.material.SetFloat("_Outline", 0f);
                Medic.shielded = null;
            }
        }

        static void mafiosoDeactivateKillButtonIfNecessary(HudManager __instance) {
            if (Mafioso.mafioso == null || Mafioso.mafioso != PlayerControl.LocalPlayer) return;

            if (!PlayerControl.LocalPlayer.Data.IsDead && __instance.UseButton.isActiveAndEnabled && Godfather.godfather != null && Godfather.godfather.Data.IsDead) {
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

        static void seerUpdate() {
            if (Seer.seer == null || Seer.seer != PlayerControl.LocalPlayer) return;

            // Update revealed players
            foreach (KeyValuePair<PlayerControl, PlayerControl> entry in Seer.revealedPlayers) {
                PlayerControl target = entry.Key;
                PlayerControl targetOrMistake = entry.Value;

                if (target == null || targetOrMistake == null) continue;

                // Update color and name regarding settings and given info
                string result = target.Data.PlayerName;
                RoleInfo si = RoleInfo.getRoleInfoForPlayer(targetOrMistake);
                if (Seer.kindOfInfo == 0)
                    result = target.Data.PlayerName + " (" + si.name + ")";
                else if (Seer.kindOfInfo == 1) {
                    si.color = si.isGood ? new Color(250f / 255f, 217f / 255f, 52f / 255f, 1) : new Color (51f / 255f, 61f / 255f, 54f / 255f, 1); 
                }

                // Set color and name
                target.nameText.Color = si.color;
                target.nameText.Text = result;
                if (MeetingHud.Instance != null) {
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates) {
                        if (target.PlayerId == player.TargetPlayerId) {
                            player.NameText.Text = result;
                            player.NameText.Color = si.color;
                            break;
                        }
                    }
                }
            }
        }

        static void spyUpdate() {
            Spy.spyTimer -= Time.deltaTime;
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
                    if (Seer.seer == null || Seer.seer != PlayerControl.LocalPlayer)
                        Morphling.morphling.nameText.Text = Morphling.morphTarget.Data.PlayerName;
                    Morphling.morphling.myRend.material.SetColor("_BackColor", Palette.ShadowColors[Morphling.morphTarget.Data.ColorId]);
                    Morphling.morphling.myRend.material.SetColor("_BodyColor", Palette.PlayerColors[Morphling.morphTarget.Data.ColorId]);
                    Morphling.morphling.HatRenderer.SetHat(Morphling.morphTarget.Data.HatId, Morphling.morphTarget.Data.ColorId);
                    Morphling.morphling.nameText.transform.localPosition = new Vector3(0f, (Morphling.morphTarget.Data.HatId == 0U) ? 0.7f : 1.05f, -0.5f);

                    if (Morphling.morphling.MyPhysics.Skin.skin.ProdId != DestroyableSingleton<HatManager>.Instance.AllSkins[(int)Morphling.morphTarget.Data.SkinId].ProdId) {
                        Helpers.setSkinWithAnim(Morphling.morphling.MyPhysics, Morphling.morphTarget.Data.SkinId);
                    }
                    if (Morphling.morphling.CurrentPet == null || Morphling.morphling.CurrentPet.ProdId != DestroyableSingleton<HatManager>.Instance.AllPets[(int)Morphling.morphTarget.Data.PetId].ProdId) {
                        if (Morphling.morphling.CurrentPet) UnityEngine.Object.Destroy(Morphling.morphling.CurrentPet.gameObject);
                        Morphling.morphling.CurrentPet = UnityEngine.Object.Instantiate<PetBehaviour>(DestroyableSingleton<HatManager>.Instance.AllPets[(int)Morphling.morphTarget.Data.PetId]);
                        Morphling.morphling.CurrentPet.transform.position = Morphling.morphling.transform.position;
                        Morphling.morphling.CurrentPet.Source = Morphling.morphling;
                        Morphling.morphling.CurrentPet.Visible = Morphling.morphling.Visible;
                        PlayerControl.SetPlayerMaterialColors(Morphling.morphTarget.Data.ColorId, Morphling.morphling.CurrentPet.rend);
                    } else if (Morphling.morphling.CurrentPet) {
                        PlayerControl.SetPlayerMaterialColors(Morphling.morphTarget.Data.ColorId, Morphling.morphling.CurrentPet.rend);
                    }
                }
            }

            // Set camouflaged look (overrides morphling morphed look if existent)
            if (Camouflager.camouflageTimer > 0f) {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                    if (Seer.seer == null || Seer.seer != PlayerControl.LocalPlayer)
                        p.nameText.Text = "";
                    p.myRend.material.SetColor("_BackColor", Color.grey);
                    p.myRend.material.SetColor("_BodyColor", Color.grey);
                    p.HatRenderer.SetHat(0, 0);
                    Helpers.setSkinWithAnim(p.MyPhysics, 0);
                    if (p.CurrentPet) UnityEngine.Object.Destroy(p.CurrentPet.gameObject);
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
            foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                if (p == null) continue;
                p.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                if (Child.child == null) continue;
                
                float growingProgress = Child.growingProgress();
                float scale = growingProgress * 0.35f + 0.35f;
                string suffix = "";
                if (growingProgress != 1f)
                    suffix = " [FAD934FF](" + Mathf.FloorToInt(growingProgress * 18) + ")"; 

                if (Child.child == p) {
                    p.transform.localScale = new Vector3(scale, scale, 1f);
                    p.nameText.Text += suffix;
                    if (MeetingHud.Instance != null)
                        foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                            if (player.NameText != null && p.PlayerId == player.TargetPlayerId)
                                player.NameText.Text += suffix;
                }
                else if (Morphling.morphling != null && Morphling.morphling == p && Morphling.morphTarget != null && Morphling.morphTarget == Child.child && Morphling.morphTimer > 0f) {
                    p.transform.localScale = new Vector3(scale, scale, 1f);
                    p.nameText.Text += suffix;
                }
                else
                    p.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            }
        }

        public static void bountyHunterUpdate() {
            if (BountyHunter.bountyHunter == null || BountyHunter.target == null) return;

            if (BountyHunter.notifyBounty && PlayerControl.LocalPlayer == BountyHunter.target) {
                string suffix = "[AD653BFF] (Bounty)[FFFFFFFF]";
                PlayerControl.LocalPlayer.nameText.Text += suffix;
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.NameText != null && PlayerControl.LocalPlayer.PlayerId == player.TargetPlayerId)
                            player.NameText.Text += suffix;
            }
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

            if (Snitch.snitch == null || Snitch.snitch.Data.IsDead) return;

            int numberOfTasks = 0;
            GameData.PlayerInfo playerInfo = Snitch.snitch.Data;
			if (!playerInfo.Disconnected && playerInfo.Tasks != null) {
				for (int i = 0; i < playerInfo.Tasks.Count; i++) {
					if (!playerInfo.Tasks[i].Complete)
						numberOfTasks++;
				}
			}

            if (PlayerControl.LocalPlayer.Data.IsImpostor && numberOfTasks <= Snitch.taskCountForImpostors) {
                if (Snitch.localArrows.Count == 0) Snitch.localArrows.Add(new Arrow(Color.blue));
                if (Snitch.localArrows.Count != 0 && Snitch.localArrows[0] != null) {
                    Snitch.localArrows[0].arrow.SetActive(true);
                    Snitch.localArrows[0].Update(Snitch.snitch.transform.position);
                }
            } else if (PlayerControl.LocalPlayer == Snitch.snitch && numberOfTasks == 0) { 
                int arrowIndex = 0;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                    if (p.Data.IsImpostor && !p.Data.IsDead) {
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
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

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
            // Shifter
            Helpers.removeTasksFromPlayer(Shifter.shifter);
            // Seer update
            seerUpdate();
            // Spy update();
            spyUpdate();
            // Camouflager and Morphling
            camouflageAndMorphActions();
            // Child
            childUpdate();
            // Bounty Hunter
            bountyHunterUpdate();
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