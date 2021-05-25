using HarmonyLib;
using static TheOtherRoles.TheOtherRoles;
using System.Collections;
using System.Collections.Generic;
using System;

namespace TheOtherRoles {
    [HarmonyPatch]
    public static class TasksHandler {

        public static Tuple<int, int> taskInfo(GameData.PlayerInfo playerInfo) {
            int TotalTasks = 0;
            int CompletedTasks = 0;
            if (!playerInfo.Disconnected && playerInfo.Tasks != null &&
                playerInfo.Object &&
                (PlayerControl.GameOptions.GhostsDoTasks || !playerInfo.IsDead) &&
                !playerInfo.IsImpostor &&
                !playerInfo.Object.hasFakeTasks()
                ) {

                for (int j = 0; j < playerInfo.Tasks.Count; j++) {
                    TotalTasks++;
                    if (playerInfo.Tasks[j].Complete) {
                        CompletedTasks++;
                    }
                }
            }
            return Tuple.Create(CompletedTasks, TotalTasks);
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
        private static class GameDataRecomputeTaskCountsPatch {
            private static bool Prefix(GameData __instance) {
                __instance.TotalTasks = 0;
                __instance.CompletedTasks = 0;
                for (int i = 0; i < __instance.AllPlayers.Count; i++) {
                    GameData.PlayerInfo playerInfo = __instance.AllPlayers[i];
                    if (playerInfo.Object && playerInfo.Object.hasAliveKillingLover())
                        continue;
                    var (playerCompleted, playerTotal) = taskInfo(playerInfo);
                    __instance.TotalTasks += playerTotal;
                    __instance.CompletedTasks += playerCompleted;
                }
                return false;
            }
        }
        
    }
}
