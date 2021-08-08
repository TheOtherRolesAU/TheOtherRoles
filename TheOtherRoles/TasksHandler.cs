using System;
using HarmonyLib;
using TheOtherRoles.Roles;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public static class TasksHandler
    {
        public static Tuple<int, int> TaskInfo(GameData.PlayerInfo playerInfo)
        {
            var totalTasks = 0;
            var completedTasks = 0;
            if (playerInfo.Disconnected || playerInfo.Tasks == null || !playerInfo.Object ||
                !PlayerControl.GameOptions.GhostsDoTasks && playerInfo.IsDead || playerInfo.IsImpostor ||
                playerInfo.Object.HasFakeTasks()) return Tuple.Create(completedTasks, totalTasks);
            foreach (var task in playerInfo.Tasks)
            {
                totalTasks++;
                if (task.Complete) completedTasks++;
            }

            return Tuple.Create(completedTasks, totalTasks);
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
        private static class GameDataRecomputeTaskCountsPatch
        {
            private static bool Prefix(GameData __instance)
            {
                __instance.TotalTasks = 0;
                __instance.CompletedTasks = 0;
                foreach (var player in __instance.AllPlayers)
                {
                    if (player.Object && Lovers.HasAliveKillingLover(player.Object))
                        continue;
                    var (playerCompleted, playerTotal) = TaskInfo(player);
                    __instance.TotalTasks += playerTotal;
                    __instance.CompletedTasks += playerCompleted;
                }

                return false;
            }
        }
    }
}