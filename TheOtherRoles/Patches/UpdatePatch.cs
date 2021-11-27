using HarmonyLib;
using System;
using System.IO;
using System.Net.Http;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;
using TheOtherRoles.Objects;
using System.Collections.Generic;
using System.Linq;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerUpdatePatch
    {
        static void resetNameTagsAndColors() {
            Dictionary<byte, PlayerControl> playersById = Helpers.allPlayersById();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                String playerName = player.Data.PlayerName;
                if (Morphling.morphTimer > 0f && Morphling.morphling == player && Morphling.morphTarget != null) playerName = Morphling.morphTarget.Data.PlayerName; // Temporary hotfix for the Morphling's name

                player.nameText.text = Helpers.hidePlayerName(PlayerControl.LocalPlayer, player) ? "" : playerName;
                if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && player.Data.Role.IsImpostor) {
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
                        if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && playerControl.Data.Role.IsImpostor) {
                            player.NameText.color = Palette.ImpostorRed;
                        } else {
                            player.NameText.color = Color.white;
                        }
                    }
                }
            }
            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor) {
                List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList();
                impostors.RemoveAll(x => !x.Data.Role.IsImpostor);
                foreach (PlayerControl player in impostors)
                    player.nameText.color = Palette.ImpostorRed;
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates) {
                        PlayerControl playerControl = Helpers.playerById((byte)player.TargetPlayerId);
                        if (playerControl != null && playerControl.Data.Role.IsImpostor)
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
                setPlayerNameColor(Guesser.guesser, Guesser.guesser.Data.Role.IsImpostor ? Palette.ImpostorRed : Guesser.color);
            } else if (Bait.bait != null && Bait.bait == PlayerControl.LocalPlayer) {
                setPlayerNameColor(Bait.bait, Bait.color);
            } else if (Vulture.vulture != null && Vulture.vulture == PlayerControl.LocalPlayer) {
                setPlayerNameColor(Vulture.vulture, Vulture.color);
            } else if (Medium.medium != null && Medium.medium == PlayerControl.LocalPlayer) {
                setPlayerNameColor(Medium.medium, Medium.color);
            } else if (Lawyer.lawyer != null && Lawyer.lawyer == PlayerControl.LocalPlayer) {
                setPlayerNameColor(Lawyer.lawyer, Lawyer.color);
            } else if (Pursuer.pursuer != null && Pursuer.pursuer == PlayerControl.LocalPlayer) {
                setPlayerNameColor(Pursuer.pursuer, Pursuer.color);
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
            if (Spy.spy != null && PlayerControl.LocalPlayer.Data.Role.IsImpostor) {
                setPlayerNameColor(Spy.spy, Spy.color);
            }

            // Crewmate roles with no changes: Mini
            // Impostor roles with no changes: Morphling, Camouflager, Vampire, Godfather, Eraser, Janitor, Cleaner, Warlock, BountyHunter,  Witch and Mafioso
        }

        static void setNameTags() {
            // Mafia
            if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.Role.IsImpostor) {
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
                string suffix = Helpers.cs(Lovers.color, " ♥");
                Lovers.lover1.nameText.text += suffix;
                Lovers.lover2.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (Lovers.lover1.PlayerId == player.TargetPlayerId || Lovers.lover2.PlayerId == player.TargetPlayerId)
                            player.NameText.text += suffix;
            }

            // Lawyer
            bool localIsLawyer = Lawyer.lawyer != null && Lawyer.target != null && Lawyer.lawyer == PlayerControl.LocalPlayer;
            bool localIsKnowingTarget = Lawyer.lawyer != null && Lawyer.target != null && Lawyer.targetKnows && Lawyer.target == PlayerControl.LocalPlayer;
            if (localIsLawyer || localIsKnowingTarget) {
                string suffix = Helpers.cs(Lawyer.color, " §");
                Lawyer.target.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == Lawyer.target.PlayerId)
                            player.NameText.text += suffix;
            }

            // Hacker and Detective
            if (PlayerControl.LocalPlayer != null && !PlayerControl.LocalPlayer.Data.IsDead && (PlayerControl.LocalPlayer == Hacker.hacker || PlayerControl.LocalPlayer == Detective.detective || PlayerControl.LocalPlayer == Medium.medium)) {
                if (MeetingHud.Instance != null) {
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates) {
                        var target = Helpers.playerById(player.TargetPlayerId);
                        if (target != null)  player.NameText.text += $" ({(Helpers.isLighterColor(target.Data.DefaultOutfit.ColorId) ? "L" : "D")})";
                    }
                }
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
            Tracker.corpsesTrackingTimer -= Time.deltaTime;
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
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor) return;
            bool enabled = true;
            if (Vampire.vampire != null && Vampire.vampire == PlayerControl.LocalPlayer)
                enabled = false;
            else if (Mafioso.mafioso != null && Mafioso.mafioso == PlayerControl.LocalPlayer && Godfather.godfather != null && !Godfather.godfather.Data.IsDead)
                enabled = false;
            else if (Janitor.janitor != null && Janitor.janitor == PlayerControl.LocalPlayer)
                enabled = false;
            
            if (enabled) __instance.KillButton.Show();
            else __instance.KillButton.Hide();
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
            // Mini
            miniUpdate();
        }
    }
}
