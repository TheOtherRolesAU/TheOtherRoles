  
using HarmonyLib;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.GameHistory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Hazel;
using UnhollowerBaseLib;
using System;
using System.Text;

namespace TheOtherRoles {
    enum CustomGameOverReason {
        LoversWin = 10,
        TeamJackalWin = 11,
        ChildLose = 12,
        JesterWin = 13
    }

    enum WinCondition {
        Default,
        LoversTeamWin,
        LoversSoloWin,
        JesterWin,
        JackalWin,
        ChildLose
    }

    static class AdditionalTempData {
        // Should be implemented using a proper GameOverReason in the future
        public static WinCondition winCondition = WinCondition.Default;
        public static bool localIsLover = false;


        public static void clear() {
            winCondition = WinCondition.Default;
            localIsLover = false;

        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class OnGameEndPatch {
        private static GameOverReason gameOverReason;
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)]ref GameOverReason reason, [HarmonyArgument(1)]bool showAd) {
            gameOverReason = reason;
            if ((int)reason >= 10) reason = GameOverReason.ImpostorByKill;
        }

        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)]ref GameOverReason reason, [HarmonyArgument(1)]bool showAd) {
            AdditionalTempData.clear();

            // Remove Jester from winners (on Jester win he will be added again, see below)
            if (Jester.jester != null) {
                WinningPlayerData jesterWinner = null;
                foreach (WinningPlayerData winner in  TempData.winners)
                    if (winner.Name == Jester.jester.Data.PlayerName) jesterWinner = winner;
                
                if (jesterWinner != null) TempData.winners.Remove(jesterWinner);
            }
            // Remove Jackal and Sidekick from winners (on Jackal win he will be added again, see below)
            if (Jackal.jackal != null || Sidekick.sidekick != null) {
                List<WinningPlayerData> winnersToRemove = new List<WinningPlayerData>();
                foreach (WinningPlayerData winner in TempData.winners) {
                    if (winner.Name == Jackal.jackal?.Data?.PlayerName) winnersToRemove.Add(winner);
                    if (winner.Name == Sidekick.sidekick?.Data?.PlayerName) winnersToRemove.Add(winner);
                    foreach(var player in Jackal.formerJackals) {
                        if (winner.Name == player.Data.PlayerName) {
                            winnersToRemove.Add(winner);
                        }
                    }
                }
                
                foreach (var winner in winnersToRemove) {
                    TempData.winners.Remove(winner);
                }
            }

            bool jesterWin = Jester.jester != null && gameOverReason == (GameOverReason)CustomGameOverReason.JesterWin;
            bool childLose = Child.child != null && gameOverReason == (GameOverReason)CustomGameOverReason.ChildLose;
            bool loversWin = Lovers.existingAndAlive() && (gameOverReason == (GameOverReason)CustomGameOverReason.LoversWin || (TempData.DidHumansWin(gameOverReason) && Lovers.existingAndCrewLovers())); // Either they win if they are among the last 3 players, or they win if they are both Crewmates and both alive and the Crew wins (Team Imp/Jackal Lovers can only win solo wins)
            bool teamJackalWin = gameOverReason == (GameOverReason)CustomGameOverReason.TeamJackalWin && ((Jackal.jackal != null && !Jackal.jackal.Data.IsDead) || (Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead));

            // Child lose
            if (childLose) {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new WinningPlayerData(Child.child.Data);
                wpd.IsYou = false; // If "no one is the Child", it will display the Child, but also show defeat to everyone
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.ChildLose;  
            }

            // Jester win
            else if (jesterWin) {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new WinningPlayerData(Jester.jester.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            }

            // Lovers win conditions
            else if (loversWin) {
                AdditionalTempData.localIsLover = (PlayerControl.LocalPlayer == Lovers.lover1 || PlayerControl.LocalPlayer == Lovers.lover2);
                // Double win for lovers, crewmates also win
                if (Lovers.existingAndCrewLovers()) {
                    AdditionalTempData.winCondition = WinCondition.LoversTeamWin;
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                        if (p == null) continue;
                        if (p == Lovers.lover1 || p == Lovers.lover2)
                            TempData.winners.Add(new WinningPlayerData(p.Data));
                        else if (p != Jester.jester && p != Jackal.jackal && p != Sidekick.sidekick && !p.Data.IsImpostor)
                            TempData.winners.Add(new WinningPlayerData(p.Data));
                    }
                }
                // Lovers solo win
                else {
                    AdditionalTempData.winCondition = WinCondition.LoversSoloWin;
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    TempData.winners.Add(new WinningPlayerData(Lovers.lover1.Data));
                    TempData.winners.Add(new WinningPlayerData(Lovers.lover2.Data));
                }
            }
            
            // Jackal win condition (should be implemented using a proper GameOverReason in the future)
            else if (teamJackalWin) {
                // Jackal wins if nobody except jackal is alive
                AdditionalTempData.winCondition = WinCondition.JackalWin;
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new WinningPlayerData(Jackal.jackal.Data);
                wpd.IsImpostor = false; 
                TempData.winners.Add(wpd);
                // If there is a sidekick. The sidekick also wins
                if (Sidekick.sidekick != null) {
                    WinningPlayerData wpdSidekick = new WinningPlayerData(Sidekick.sidekick.Data);
                    wpdSidekick.IsImpostor = false; 
                    TempData.winners.Add(wpdSidekick);
                }
                foreach(var player in Jackal.formerJackals) {
                    WinningPlayerData wpdFormerJackal = new WinningPlayerData(player.Data);
                    wpdFormerJackal.IsImpostor = false; 
                    TempData.winners.Add(wpdFormerJackal);
                }
            }

            // Reset Settings
            RPCProcedure.resetVariables();
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public class EndGameManagerSetUpPatch {
        public static void Postfix(EndGameManager __instance) {
            GameObject bonusText = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            bonusText.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.8f, __instance.WinText.transform.position.z);
            bonusText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            TMPro.TMP_Text textRenderer = bonusText.GetComponent<TMPro.TMP_Text>();
            textRenderer.text = "";

            if (AdditionalTempData.winCondition == WinCondition.JesterWin) {
                textRenderer.text = "Jester Wins";
                textRenderer.color = Jester.color;
            }
            else if (AdditionalTempData.winCondition == WinCondition.LoversTeamWin) {
                if (AdditionalTempData.localIsLover) {
                    __instance.WinText.text = "Double Victory";
                } 
                textRenderer.text = "Lovers And Crewmates Win";
                textRenderer.color = Lovers.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
            } 
            else if (AdditionalTempData.winCondition == WinCondition.LoversSoloWin) {
                textRenderer.text = "Lovers Win";
                textRenderer.color = Lovers.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.JackalWin) {
                textRenderer.text = "Team Jackal Wins";
                textRenderer.color = Jackal.color;
            }
            else if (AdditionalTempData.winCondition == WinCondition.ChildLose) {
                textRenderer.text = "Child died";
                textRenderer.color = Child.color;
            }
            
            AdditionalTempData.clear();
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))] 
    class CheckEndCriteriaPatch {
        public static bool Prefix(ShipStatus __instance) {
            if (!GameData.Instance) return false;
            if (DestroyableSingleton<TutorialManager>.InstanceExists) // InstanceExists | Don't check Custom Criteria when in Tutorial
                return true;
            var statistics = new PlayerStatistics(__instance);
            if (CheckAndEndGameForChildLose(__instance)) return false;
            if (CheckAndEndGameForJesterWin(__instance)) return false;
            if (CheckAndEndGameForSabotageWin(__instance)) return false;
            if (CheckAndEndGameForTaskWin(__instance)) return false;
            if (CheckAndEndGameForLoverWin(__instance, statistics)) return false;
            if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
            if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
            if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
            return false;
        }

        private static bool CheckAndEndGameForChildLose(ShipStatus __instance) {
            if (Child.triggerChildLose) {
                __instance.enabled = false;
                ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.ChildLose, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForJesterWin(ShipStatus __instance) {
            if (Jester.triggerJesterWin) {
                __instance.enabled = false;
                ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.JesterWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance) {
            if (__instance.Systems == null) return false;
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.LifeSupp) ? __instance.Systems[SystemTypes.LifeSupp] : null;
            if (systemType != null) {
                LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f) {
                    EndGameForSabotage(__instance);
                    lifeSuppSystemType.Countdown = 10000f;
                    return true;
                }
            }
            ISystemType systemType2 = __instance.Systems.ContainsKey(SystemTypes.Reactor) ? __instance.Systems[SystemTypes.Reactor] : null;
            if (systemType2 == null) {
                systemType2 = __instance.Systems.ContainsKey(SystemTypes.Laboratory) ? __instance.Systems[SystemTypes.Laboratory] : null;
            }
            if (systemType2 != null) {
                ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
                if (criticalSystem != null && criticalSystem.Countdown < 0f) {
                    EndGameForSabotage(__instance);
                    criticalSystem.ClearSabotage();
                    return true;
                }
            }
            return false;
        }

        private static bool CheckAndEndGameForTaskWin(ShipStatus __instance) {
            if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks) {
                __instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.HumansByTask, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForLoverWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.TeamLoversAlive == 2 && statistics.TotalAlive <= 3) {
                __instance.enabled = false;
                ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.LoversWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive && statistics.TeamImpostorsAlive == 0 && !(statistics.TeamJackalHasAliveLover && statistics.TeamLoversAlive == 2)) {
                __instance.enabled = false;
                ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.TeamJackalWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive && statistics.TeamJackalAlive == 0 && !(statistics.TeamImpostorHasAliveLover && statistics.TeamLoversAlive == 2)) {
                __instance.enabled = false;
                GameOverReason endReason;
                switch (TempData.LastDeathReason) {
                    case DeathReason.Exile:
                        endReason = GameOverReason.ImpostorByVote;
                        break;
                    case DeathReason.Kill:
                        endReason = GameOverReason.ImpostorByKill;
                        break;
                    default:
                        endReason = GameOverReason.ImpostorByVote;
                        break;
                }
                ShipStatus.RpcEndGame(endReason, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0) {
                __instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.HumansByVote, false);
                return true;
            }
            return false;
        }

        private static void EndGameForSabotage(ShipStatus __instance) {
            __instance.enabled = false;
            ShipStatus.RpcEndGame(GameOverReason.ImpostorBySabotage, false);
            return;
        }

    }

    internal class PlayerStatistics {
        public int TeamImpostorsAlive {get;set;}
        public int TeamJackalAlive {get;set;}
        public int TeamLoversAlive {get;set;}
        public int TotalAlive {get;set;}
        public bool TeamImpostorHasAliveLover {get;set;}
        public bool TeamJackalHasAliveLover {get;set;}

        public PlayerStatistics(ShipStatus __instance) {
            GetPlayerCounts();
        }

        private bool isLover(GameData.PlayerInfo p) {
            return (Lovers.lover1 != null && Lovers.lover1.PlayerId == p.PlayerId) || (Lovers.lover2 != null && Lovers.lover2.PlayerId == p.PlayerId);
        }

        private void GetPlayerCounts() {
            int numJackalAlive = 0;
            int numImpostorsAlive = 0;
            int numLoversAlive = 0;
            int numTotalAlive = 0;
            bool impLover = false;
            bool jackalLover = false;

            for (int i = 0; i < GameData.Instance.PlayerCount; i++)
            {
                GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                if (!playerInfo.Disconnected)
                {
                    if (!playerInfo.IsDead)
                    {
                        numTotalAlive++;

                        bool lover = isLover(playerInfo);
                        if (lover) numLoversAlive++;

                        if (playerInfo.IsImpostor) {
                            numImpostorsAlive++;
                            if (lover) impLover = true;
                        }
                        if (Jackal.jackal != null && Jackal.jackal.PlayerId == playerInfo.PlayerId) {
                            numJackalAlive++;
                            if (lover) jackalLover = true;
                        }
                        if (Sidekick.sidekick != null && Sidekick.sidekick.PlayerId == playerInfo.PlayerId) {
                            numJackalAlive++;
                            if (lover) jackalLover = true;
                        }
                    }
                }
            }

            TeamJackalAlive = numJackalAlive;
            TeamImpostorsAlive = numImpostorsAlive;
            TeamLoversAlive = numLoversAlive;
            TotalAlive = numTotalAlive;
            TeamImpostorHasAliveLover = impLover;
            TeamJackalHasAliveLover = jackalLover;
        }
    }
}