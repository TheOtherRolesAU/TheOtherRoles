using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using TheOtherRoles.Roles;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Patches
{
    internal enum CustomGameOverReason
    {
        LoversWin = 10,
        TeamJackalWin = 11,
        MiniLose = 12,
        JesterWin = 13,
        ArsonistWin = 14
    }

    internal enum WinCondition
    {
        Default,
        LoversTeamWin,
        LoversSoloWin,
        JesterWin,
        JackalWin,
        MiniLose,
        ArsonistWin
    }

    internal static class AdditionalTempData
    {
        // Should be implemented using a proper GameOverReason in the future
        public static WinCondition winCondition = WinCondition.Default;
        public static readonly List<PlayerRoleInfo> PlayerRoles = new();

        public static void Clear()
        {
            PlayerRoles.Clear();
            winCondition = WinCondition.Default;
        }

        internal class PlayerRoleInfo
        {
            public string PlayerName { get; set; }
            public List<RoleInfo> Roles { get; set; }
            public int TasksCompleted { get; set; }
            public int TasksTotal { get; set; }
        }
    }


    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class OnGameEndPatch
    {
        private static GameOverReason gameOverReason;

        public static void Prefix([HarmonyArgument(0)] ref GameOverReason reason)
        {
            gameOverReason = reason;
            if ((int) reason >= 10) reason = GameOverReason.ImpostorByKill;
        }

        public static void Postfix()
        {
            AdditionalTempData.Clear();

            foreach (var playerControl in PlayerControl.AllPlayerControls)
            {
                var roles = RoleInfo.GetRoleInfoForPlayer(playerControl);
                var (tasksCompleted, tasksTotal) = TasksHandler.TaskInfo(playerControl.Data);
                AdditionalTempData.PlayerRoles.Add(new AdditionalTempData.PlayerRoleInfo
                {
                    PlayerName = playerControl.Data.PlayerName, Roles = roles, TasksTotal = tasksTotal,
                    TasksCompleted = tasksCompleted
                });
            }

            // Remove Jester, Arsonist, Jackal, former Jackals and Sidekick from winners (if they win, they'll be readded)
            var notWinners = new List<PlayerControl>();
            if (Jester.Instance.player != null) notWinners.Add(Jester.Instance.player);
            if (Sidekick.Instance.player != null) notWinners.Add(Sidekick.Instance.player);
            if (Jackal.Instance.player != null) notWinners.Add(Jackal.Instance.player);
            if (Arsonist.Instance.player != null) notWinners.Add(Arsonist.Instance.player);
            notWinners.AddRange(Jackal.FormerJackals);

            var winnersToRemove = new List<WinningPlayerData>();
            foreach (var winner in TempData.winners)
                if (notWinners.Any(x => x.Data.PlayerName == winner.Name))
                    winnersToRemove.Add(winner);
            foreach (var winner in winnersToRemove) TempData.winners.Remove(winner);

            var jesterWin = Jester.Instance.player != null &&
                            gameOverReason == (GameOverReason) CustomGameOverReason.JesterWin;
            var arsonistWin = Arsonist.Instance.player != null &&
                              gameOverReason == (GameOverReason) CustomGameOverReason.ArsonistWin;
            var miniLose = Mini.Instance.player != null &&
                           gameOverReason == (GameOverReason) CustomGameOverReason.MiniLose;
            var loversWin = Lovers.ExistingAndAlive() &&
                            (gameOverReason == (GameOverReason) CustomGameOverReason.LoversWin ||
                             TempData.DidHumansWin(gameOverReason) &&
                             !Lovers
                                 .ExistingWithKiller()); // Either they win if they are among the last 3 players, or they win if they are both Crewmates and both alive and the Crew wins (Team Imp/Jackal Lovers can only win solo wins)
            var teamJackalWin = gameOverReason == (GameOverReason) CustomGameOverReason.TeamJackalWin &&
                                (Jackal.Instance.player != null && !Jackal.Instance.player.Data.IsDead ||
                                 Sidekick.Instance.player != null && !Sidekick.Instance.player.Data.IsDead);

            // Mini lose
            if (miniLose)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                var wpd = new WinningPlayerData(Mini.Instance.player.Data) {IsYou = false};
                // If "no one is the Mini", it will display the Mini, but also show defeat to everyone
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.MiniLose;
            }

            // Jester win
            else if (jesterWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                var wpd = new WinningPlayerData(Jester.Instance.player.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            }

            // Arsonist win
            else if (arsonistWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                var wpd = new WinningPlayerData(Arsonist.Instance.player.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.ArsonistWin;
            }

            // Lovers win conditions
            else if (loversWin)
            {
                // Double win for lovers, crewmates also win
                if (!Lovers.ExistingWithKiller())
                {
                    AdditionalTempData.winCondition = WinCondition.LoversTeamWin;
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    foreach (var p in PlayerControl.AllPlayerControls)
                    {
                        if (p == null) continue;
                        if (p == Lovers.Instance.player || p == Lovers.Instance.secondPlayer)
                            TempData.winners.Add(new WinningPlayerData(p.Data));
                        else if (p != Jester.Instance.player && p != Jackal.Instance.player &&
                                 p != Sidekick.Instance.player &&
                                 p != Arsonist.Instance.player && !Jackal.FormerJackals.Contains(p) &&
                                 !p.Data.IsImpostor)
                            TempData.winners.Add(new WinningPlayerData(p.Data));
                    }
                }
                // Lovers solo win
                else
                {
                    AdditionalTempData.winCondition = WinCondition.LoversSoloWin;
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    TempData.winners.Add(new WinningPlayerData(Lovers.Instance.player.Data));
                    TempData.winners.Add(new WinningPlayerData(Lovers.Instance.secondPlayer.Data));
                }
            }

            // Jackal win condition (should be implemented using a proper GameOverReason in the future)
            else if (teamJackalWin)
            {
                // Jackal wins if nobody except jackal is alive
                AdditionalTempData.winCondition = WinCondition.JackalWin;
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                var wpd = new WinningPlayerData(Jackal.Instance.player.Data) {IsImpostor = false};
                TempData.winners.Add(wpd);
                // If there is a sidekick. The sidekick also wins
                if (Sidekick.Instance.player != null)
                {
                    var wpdSidekick = new WinningPlayerData(Sidekick.Instance.player.Data) {IsImpostor = false};
                    TempData.winners.Add(wpdSidekick);
                }

                foreach (var wpdFormerJackal in Jackal.FormerJackals.Select(
                    player => new WinningPlayerData(player.Data)))
                {
                    wpdFormerJackal.IsImpostor = false;
                    TempData.winners.Add(wpdFormerJackal);
                }
            }

            // Reset Settings
            RPCProcedure.ResetVariables();
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public class EndGameManagerSetUpPatch
    {
        private static readonly int Color1 = Shader.PropertyToID("_Color");

        public static void Postfix(EndGameManager __instance)
        {
            var bonusText = Object.Instantiate(__instance.WinText.gameObject);
            bonusText.transform.position -= new Vector3(0, 0.8f, 0);
            bonusText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            var textRenderer = bonusText.GetComponent<TMP_Text>();
            textRenderer.text = "";

            switch (AdditionalTempData.winCondition)
            {
                case WinCondition.JesterWin:
                    textRenderer.text = "Jester Wins";
                    textRenderer.color = Jester.Instance.color;
                    break;
                case WinCondition.ArsonistWin:
                    textRenderer.text = "Arsonist Wins";
                    textRenderer.color = Arsonist.Instance.color;
                    break;
                case WinCondition.LoversTeamWin:
                    textRenderer.text = "Lovers And Crewmates Win";
                    textRenderer.color = Lovers.Instance.color;
                    __instance.BackgroundBar.material.SetColor(Color1, Lovers.Instance.color);
                    break;
                case WinCondition.LoversSoloWin:
                    textRenderer.text = "Lovers Win";
                    textRenderer.color = Lovers.Instance.color;
                    __instance.BackgroundBar.material.SetColor(Color1, Lovers.Instance.color);
                    break;
                case WinCondition.JackalWin:
                    textRenderer.text = "Team Jackal Wins";
                    textRenderer.color = Jackal.Instance.color;
                    break;
                case WinCondition.MiniLose:
                    textRenderer.text = "Mini died";
                    textRenderer.color = Mini.Instance.color;
                    break;
            }

            if (MapOptions.showRoleSummary)
            {
                var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
                var roleSummary = Object.Instantiate(__instance.WinText.gameObject);
                roleSummary.transform.position = new Vector3(__instance.ExitButton.transform.position.x + 0.1f,
                    position.y - 0.1f, -14f);
                roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

                var roleSummaryText = new StringBuilder();
                roleSummaryText.AppendLine("Players and roles at the end of the game:");
                foreach (var data in AdditionalTempData.PlayerRoles)
                {
                    var roles = string.Join(" ", data.Roles.Select(x => Helpers.Cs(x.color, x.name)));
                    var taskInfo = data.TasksTotal > 0
                        ? $" - <color=#FAD934FF>({data.TasksCompleted}/{data.TasksTotal})</color>"
                        : "";
                    roleSummaryText.AppendLine($"{data.PlayerName} - {roles}{taskInfo}");
                }

                var roleSummaryTextMesh = roleSummary.GetComponent<TMP_Text>();
                roleSummaryTextMesh.alignment = TextAlignmentOptions.TopLeft;
                roleSummaryTextMesh.color = Color.white;
                roleSummaryTextMesh.fontSizeMin = 1.5f;
                roleSummaryTextMesh.fontSizeMax = 1.5f;
                roleSummaryTextMesh.fontSize = 1.5f;

                var roleSummaryTextMeshRectTransform = roleSummaryTextMesh.GetComponent<RectTransform>();
                roleSummaryTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
                roleSummaryTextMesh.text = roleSummaryText.ToString();
            }

            AdditionalTempData.Clear();
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))]
    internal class CheckEndCriteriaPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            if (!GameData.Instance) return false;
            if (DestroyableSingleton<TutorialManager>
                .InstanceExists) // InstanceExists | Don't check Custom Criteria when in Tutorial
                return true;
            var statistics = new PlayerStatistics();
            if (CheckAndEndGameForMiniLose(__instance)) return false;
            if (CheckAndEndGameForJesterWin(__instance)) return false;
            if (CheckAndEndGameForArsonistWin(__instance)) return false;
            if (CheckAndEndGameForSabotageWin(__instance)) return false;
            if (CheckAndEndGameForTaskWin(__instance)) return false;
            if (CheckAndEndGameForLoverWin(__instance, statistics)) return false;
            if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
            if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
            if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
            return false;
        }

        private static bool CheckAndEndGameForMiniLose(ShipStatus __instance)
        {
            if (!Mini.triggerMiniLose) return false;
            __instance.enabled = false;
            ShipStatus.RpcEndGame((GameOverReason) CustomGameOverReason.MiniLose, false);
            return true;
        }

        private static bool CheckAndEndGameForJesterWin(ShipStatus __instance)
        {
            if (!Jester.triggerJesterWin) return false;
            __instance.enabled = false;
            ShipStatus.RpcEndGame((GameOverReason) CustomGameOverReason.JesterWin, false);
            return true;
        }

        private static bool CheckAndEndGameForArsonistWin(ShipStatus __instance)
        {
            if (!Arsonist.triggerArsonistWin) return false;
            __instance.enabled = false;
            ShipStatus.RpcEndGame((GameOverReason) CustomGameOverReason.ArsonistWin, false);
            return true;
        }

        private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance)
        {
            if (__instance.Systems == null) return false;
            var systemType = __instance.Systems.ContainsKey(SystemTypes.LifeSupp)
                ? __instance.Systems[SystemTypes.LifeSupp]
                : null;
            var lifeSuppSystemType = systemType?.TryCast<LifeSuppSystemType>();
            if (lifeSuppSystemType is {Countdown: < 0f})
            {
                EndGameForSabotage(__instance);
                lifeSuppSystemType.Countdown = 10000f;
                return true;
            }

            var systemType2 = (__instance.Systems.ContainsKey(SystemTypes.Reactor)
                ? __instance.Systems[SystemTypes.Reactor]
                : null) ?? (__instance.Systems.ContainsKey(SystemTypes.Laboratory)
                ? __instance.Systems[SystemTypes.Laboratory]
                : null);
            var criticalSystem = systemType2?.TryCast<ICriticalSabotage>();
            if (criticalSystem is not {Countdown: < 0f}) return false;
            EndGameForSabotage(__instance);
            criticalSystem.ClearSabotage();
            return true;
        }

        private static bool CheckAndEndGameForTaskWin(ShipStatus __instance)
        {
            if (GameData.Instance.TotalTasks > GameData.Instance.CompletedTasks) return false;
            __instance.enabled = false;
            ShipStatus.RpcEndGame(GameOverReason.HumansByTask, false);
            return true;
        }

        private static bool CheckAndEndGameForLoverWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamLoversAlive != 2 || statistics.TotalAlive > 3) return false;
            __instance.enabled = false;
            ShipStatus.RpcEndGame((GameOverReason) CustomGameOverReason.LoversWin, false);
            return true;
        }

        private static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamJackalAlive < statistics.TotalAlive - statistics.TeamJackalAlive ||
                statistics.TeamImpostorsAlive != 0 ||
                statistics.TeamJackalHasAliveLover && statistics.TeamLoversAlive == 2) return false;
            __instance.enabled = false;
            ShipStatus.RpcEndGame((GameOverReason) CustomGameOverReason.TeamJackalWin, false);
            return true;
        }

        private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamImpostorsAlive < statistics.TotalAlive - statistics.TeamImpostorsAlive ||
                statistics.TeamJackalAlive != 0 ||
                statistics.TeamImpostorHasAliveLover && statistics.TeamLoversAlive == 2) return false;
            __instance.enabled = false;
            var endReason = TempData.LastDeathReason switch
            {
                DeathReason.Exile => GameOverReason.ImpostorByVote,
                DeathReason.Kill => GameOverReason.ImpostorByKill,
                _ => GameOverReason.ImpostorByVote
            };

            ShipStatus.RpcEndGame(endReason, false);
            return true;
        }

        private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamImpostorsAlive != 0 || statistics.TeamJackalAlive != 0) return false;
            __instance.enabled = false;
            ShipStatus.RpcEndGame(GameOverReason.HumansByVote, false);
            return true;
        }

        private static void EndGameForSabotage(ShipStatus __instance)
        {
            __instance.enabled = false;
            ShipStatus.RpcEndGame(GameOverReason.ImpostorBySabotage, false);
        }
    }

    internal class PlayerStatistics
    {
        public PlayerStatistics()
        {
            GetPlayerCounts();
        }

        public int TeamImpostorsAlive { get; private set; }
        public int TeamJackalAlive { get; private set; }
        public int TeamLoversAlive { get; private set; }
        public int TotalAlive { get; private set; }
        public bool TeamImpostorHasAliveLover { get; private set; }
        public bool TeamJackalHasAliveLover { get; private set; }

        private static bool IsLover(GameData.PlayerInfo p)
        {
            return Lovers.Instance.player != null && Lovers.Instance.player.PlayerId == p.PlayerId ||
                   Lovers.Instance.secondPlayer != null && Lovers.Instance.secondPlayer.PlayerId == p.PlayerId;
        }

        private void GetPlayerCounts()
        {
            var numJackalAlive = 0;
            var numImpostorsAlive = 0;
            var numLoversAlive = 0;
            var numTotalAlive = 0;
            var impLover = false;
            var jackalLover = false;

            foreach (var playerInfo in GameData.Instance.AllPlayers)
            {
                if (playerInfo.Disconnected) continue;
                if (playerInfo.IsDead) continue;
                numTotalAlive++;

                var lover = IsLover(playerInfo);
                if (lover) numLoversAlive++;

                if (playerInfo.IsImpostor)
                {
                    numImpostorsAlive++;
                    if (lover) impLover = true;
                }

                if (Jackal.Instance.player != null && Jackal.Instance.player.PlayerId == playerInfo.PlayerId)
                {
                    numJackalAlive++;
                    if (lover) jackalLover = true;
                }

                if (Sidekick.Instance.player == null ||
                    Sidekick.Instance.player.PlayerId != playerInfo.PlayerId) continue;
                numJackalAlive++;
                if (lover) jackalLover = true;
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