using HarmonyLib;
using static TheOtherRoles.TheOtherRoles;
using System.Collections;
using System.Collections.Generic;

namespace TheOtherRoles {
    [HarmonyPatch]
    public static class FakeTasksForEveryone {
        [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
        private static class RecomputeTasksPatch {
            private static bool Prefix(GameData __instance) {
                __instance.TotalTasks = 0;
                __instance.CompletedTasks = 0;
                for (int i = 0; i < __instance.AllPlayers.Count; i++) {
                    GameData.LGBOMGHJELL playerInfo = __instance.AllPlayers[i]; // PlayerInfo
                    if (!playerInfo.MFFAGDHDHLO && playerInfo.PHGPJMKOKMC != null && // Disconnected | // Tasks
                        playerInfo.GJPBCGFPMOD && // Object -> PlayerControl
                        (PlayerControl.GameOptions.AHOIGIPIACJ || !playerInfo.IAGJEKLJCCI) && // GhostsDoTasks | IsDead
                        !playerInfo.FDNMBJOAPFL && // IsImpostor
                        playerInfo.GJPBCGFPMOD != Jackal.jackal &&
                        playerInfo.GJPBCGFPMOD != Sidekick.sidekick &&
                        playerInfo.GJPBCGFPMOD != Jester.jester 
                        ) {

                        for (int j = 0; j < playerInfo.PHGPJMKOKMC.Count; j++) {
                            __instance.TotalTasks++;
                            if (playerInfo.PHGPJMKOKMC[j].LBBFBHJINJK) { // Complete
                                __instance.CompletedTasks++;
                            }
                        }
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
        private static class CanDoTaskPatch {
            private static bool Prefix(ref float __result, Console __instance, [HarmonyArgument(0)] GameData.LGBOMGHJELL pc[HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse) {
                if (__instance.AllowImpostor) return true;
                PlayerControl player = pc.GJPBCGFPMOD;
                if (player != Jackal.jackal && player != Sidekick.sidekick && player != Jester.jester) return true;
                canUse = couldUse = false;
                __result = float.MaxValue;
                return false;
            }
        }
    }
}
