using HarmonyLib;
using System;
using System.IO;
using System.Net.Http;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;
using System.Collections.Generic;
using System.Linq;

namespace TheOtherRoles
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerUpdatePatch
    {
        public static bool hidePlayerName(PlayerControl source, PlayerControl target) {
            if (!MapOptions.hidePlayerNames) return false; // All names are visible
            else if (source == null || target == null) return true;
            else if (source == target) return false; // Player sees his own name
            else if (source.Data.IsImpostor && (target.Data.IsImpostor || target == Spy.spy)) return false; // Members of team Impostors see the names of Impostors/Spies
            else if ((source == Lovers.lover1 || source == Lovers.lover2) && (target == Lovers.lover1 || target == Lovers.lover2)) return false; // Members of team Lovers see the names of each other
            else if ((source == Jackal.jackal || source == Sidekick.sidekick) && (target == Jackal.jackal || target == Sidekick.sidekick || target == Jackal.fakeSidekick)) return false; // Members of team Jackal see the names of each other
            return true;
        }

        static void resetNameTagsAndColors() {
            Dictionary<byte, PlayerControl> playersById = Helpers.allPlayersById();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                player.nameText.text = hidePlayerName(PlayerControl.LocalPlayer, player) ? "" : player.Data.PlayerName;
                if (PlayerControl.LocalPlayer.Data.IsImpostor && player.Data.IsImpostor) {
                    player.nameText.color = Palette.ImpostorRed;
                } else {
                    player.nameText.color = Color.white;
                }
            }
            if (MeetingHud.Instance != null) {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates) {
                    PlayerControl playerControl = playersById.ContainsKey((byte)player.TargetPlayerId) ? playersById[(byte)player.TargetPlayerId] : null;
                    if (playerControl != null) {
                        player.NameText.text = playerControl.Data.PlayerName;
                        if (PlayerControl.LocalPlayer.Data.IsImpostor && playerControl.Data.IsImpostor) {
                            player.NameText.color = Palette.ImpostorRed;
                        } else {
                            player.NameText.color = Color.white;
                        }
                    }
                }
            }
            if (PlayerControl.LocalPlayer.Data.IsImpostor) {
                List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList();
                impostors.RemoveAll(x => !x.Data.IsImpostor);
                foreach (PlayerControl player in impostors)
                    player.nameText.color = Palette.ImpostorRed;
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates) {
                        PlayerControl playerControl = Helpers.playerById((byte)player.TargetPlayerId);
                        if (playerControl != null && playerControl.Data.IsImpostor)
                            player.NameText.color =  Palette.ImpostorRed;
                    }
            }

        }

        static void setPlayerNameColor(PlayerControl p, Color color) {
            p.nameText.color = color;
            if (MeetingHud.Instance != null)
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && p.PlayerId == player.TargetPlayerId)
                        player.NameText.color = color;
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
            } else if (SecurityGuard.securityGuard != null && SecurityGuard.securityGuard == PlayerControl.LocalPlayer) {
                setPlayerNameColor(SecurityGuard.securityGuard, SecurityGuard.color);
            } else if (Arsonist.arsonist != null && Arsonist.arsonist == PlayerControl.LocalPlayer) {
                setPlayerNameColor(Arsonist.arsonist, Arsonist.color);
            } else if (Guesser.guesser != null && Guesser.guesser == PlayerControl.LocalPlayer) {
                setPlayerNameColor(Guesser.guesser, Guesser.guesser.Data.IsImpostor ? Palette.ImpostorRed : Guesser.color);
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
            if (Spy.spy != null && PlayerControl.LocalPlayer.Data.IsImpostor) {
                setPlayerNameColor(Spy.spy, Spy.color);
            }

            // Crewmate roles with no changes: Mini
            // Impostor roles with no changes: Morphling, Camouflager, Vampire, Godfather, Eraser, Janitor, Cleaner, Warlock, BountyHunter and Mafioso
        }

        static void setNameTags() {
            // Mafia
            if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.IsImpostor) {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    if (Godfather.godfather != null && Godfather.godfather == player)
                            player.nameText.text = player.Data.PlayerName + " (G)";
                    else if (Mafioso.mafioso != null && Mafioso.mafioso == player)
                            player.nameText.text = player.Data.PlayerName + " (M)";
                    else if (Janitor.janitor != null && Janitor.janitor == player)
                            player.nameText.text = player.Data.PlayerName + " (J)";
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (Godfather.godfather != null && Godfather.godfather.PlayerId == player.TargetPlayerId)
                            player.NameText.text = Godfather.godfather.Data.PlayerName + " (G)";
                        else if (Mafioso.mafioso != null && Mafioso.mafioso.PlayerId == player.TargetPlayerId)
                            player.NameText.text = Mafioso.mafioso.Data.PlayerName + " (M)";
                        else if (Janitor.janitor != null && Janitor.janitor.PlayerId == player.TargetPlayerId)
                            player.NameText.text = Janitor.janitor.Data.PlayerName + " (J)";
            }

            // Lovers
            if (Lovers.lover1 != null && Lovers.lover2 != null && (Lovers.lover1 == PlayerControl.LocalPlayer || Lovers.lover2 == PlayerControl.LocalPlayer)) {
                string suffix = Helpers.cs(Lovers.color, " ❤");
                Lovers.lover1.nameText.text += suffix;
                Lovers.lover2.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (Lovers.lover1.PlayerId == player.TargetPlayerId || Lovers.lover2.PlayerId == player.TargetPlayerId)
                            player.NameText.text += suffix;
            }
        }

        static void updateShielded() {
            if (Medic.shielded == null) return;

            if (Medic.shielded.Data.IsDead || Medic.medic == null || Medic.medic.Data.IsDead) {
                Medic.shielded = null;
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
                    Morphling.morphling.nameText.text = hidePlayerName(PlayerControl.LocalPlayer, Morphling.morphling) ? "" : Morphling.morphTarget.Data.PlayerName;
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
                    p.nameText.text = "";
                    p.myRend.material.SetColor("_BackColor", Palette.PlayerColors[6]);
                    p.myRend.material.SetColor("_BodyColor", Palette.PlayerColors[6]);
                    p.HatRenderer.SetHat(0, 0);
                    Helpers.setSkinWithAnim(p.MyPhysics, 0);
                    bool spawnPet = false;
                    if (p.CurrentPet == null) spawnPet = true;
                    else if (p.CurrentPet.ProdId != DestroyableSingleton<HatManager>.Instance.AllPets[0].ProdId) {
                        UnityEngine.Object.Destroy(p.CurrentPet.gameObject);
                        spawnPet = true;
                    }

                    if (spawnPet) {
                        p.CurrentPet = UnityEngine.Object.Instantiate<PetBehaviour>(DestroyableSingleton<HatManager>.Instance.AllPets[0]);
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

        public static void miniUpdate() {
            if (Mini.mini == null || Camouflager.camouflageTimer > 0f) return;
                
            float growingProgress = Mini.growingProgress();
            float scale = growingProgress * 0.35f + 0.35f;
            string suffix = "";
            if (growingProgress != 1f)
                suffix = " <color=#FAD934FF>(" + Mathf.FloorToInt(growingProgress * 18) + ")</color>"; 

            Mini.mini.nameText.text += suffix;
            if (MeetingHud.Instance != null) {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && Mini.mini.PlayerId == player.TargetPlayerId)
                        player.NameText.text += suffix;
            }

            if (Morphling.morphling != null && Morphling.morphTarget == Mini.mini && Morphling.morphTimer > 0f)
                Morphling.morphling.nameText.text += suffix;
        }

        static void updateImpostorKillButton(HudManager __instance) {
            if (!PlayerControl.LocalPlayer.Data.IsImpostor) return;
            bool enabled = true;
            if (Vampire.vampire != null && Vampire.vampire == PlayerControl.LocalPlayer)
                enabled = false;
            else if (Mafioso.mafioso != null && Mafioso.mafioso == PlayerControl.LocalPlayer && Godfather.godfather != null && !Godfather.godfather.Data.IsDead)
                enabled = false;
            else if (Janitor.janitor != null && Janitor.janitor == PlayerControl.LocalPlayer)
                enabled = false;
            enabled &= __instance.UseButton.isActiveAndEnabled;
            
            __instance.KillButton.gameObject.SetActive(enabled);
            __instance.KillButton.renderer.enabled = enabled;
            __instance.KillButton.isActive = enabled;
            __instance.KillButton.enabled = enabled;
        }

        static void Postfix(HudManager __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

            CustomButton.HudUpdate();
            resetNameTagsAndColors();
            setNameColors();
            updateShielded();
            setNameTags();

            // Impostors
            updateImpostorKillButton(__instance);
            // Timer updates
            timerUpdate();
            // Camouflager and Morphling
            camouflageAndMorphActions();
            // Mini
            miniUpdate();
        }
    }
}
