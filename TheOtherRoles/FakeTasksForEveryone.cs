using HarmonyLib;
using static TheOtherRoles.TheOtherRoles;
using System.Collections;
using System.Collections.Generic;

namespace TheOtherRoles {
    [HarmonyPatch]
    public static class FakeTasksForEveryone {
        [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
        private static class GameDataRecomputeTaskCountsPatch {
            private static bool Prefix(GameData __instance) {
                __instance.TotalTasks = 0;
                __instance.CompletedTasks = 0;
                for (int i = 0; i < __instance.AllPlayers.Count; i++) {
                    GameData.PlayerInfo playerInfo = __instance.AllPlayers[i]; // PlayerInfo
                    if (!playerInfo.Disconnected && playerInfo.Tasks != null && // Disconnected | // Tasks
                        playerInfo.Object && // Object -> PlayerControl
                        (PlayerControl.GameOptions.GhostsDoTasks || !playerInfo.IsDead) && // GhostsDoTasks | IsDead
                        !playerInfo.IsImpostor && // IsImpostor
                        !Helpers.hasFakeTasks(playerInfo.Object)
                        ) {

                        for (int j = 0; j < playerInfo.Tasks.Count; j++) {
                            __instance.TotalTasks++;
                            if (playerInfo.Tasks[j].Complete) { // Complete
                                __instance.CompletedTasks++;
                            }
                        }
                    }
                }
                return false;
            }
        }
    }
}
