using HarmonyLib;

/*
 * This patch removes the screen shake effect from the lobby
 * and from the game. 
 */

namespace TheEpicRoles.Patches {
    [Harmony]
    public class ScreenShakePatch {   

        [HarmonyPatch(typeof(FollowerCamera), nameof(FollowerCamera.Update))]
        private static class ShakePatch {

            private static float lobbyShakeAmount;
            private static float gameShakeAmount;
            static void Prefix(FollowerCamera __instance) {
                // Set values depending on mod option
                if (TheEpicRolesPlugin.ToggleScreenShake.Value) {
                    // Default Among Us shake values
                    lobbyShakeAmount = 0.03f;
                    gameShakeAmount = 0.02f;
                }
                else {
                    lobbyShakeAmount = gameShakeAmount = 0;
                }
                // Sets shakeAmount value depending on Gamemode / Gamestate
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started || AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                    __instance.shakeAmount = gameShakeAmount;
                else
                    __instance.shakeAmount = lobbyShakeAmount;
            }
        }
    }
}
