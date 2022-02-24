using HarmonyLib;

namespace TheOtherRoles.Patches {
    [Harmony]
    public class ScreenShakePatch {
        
        // Patches FollowerCamera.Update()
        // Sets original or zero values for 
        [HarmonyPatch(typeof(FollowerCamera), nameof(FollowerCamera.Update))]
        private static class ShakePatch {

            private static float lobbyShakeAmount;
            private static float gameShakeAmount;
            static void Prefix(FollowerCamera __instance) {
                // Set values depending on mod option
                if (TheOtherRolesPlugin.ToggleScreenShake.Value) {
                    lobbyShakeAmount = 0.03f;
                    gameShakeAmount = 0.02f;
                }
                else {
                    lobbyShakeAmount = gameShakeAmount = 0;
                }
                // Sets __instance.shakeAmount value depending on Gamemode / Gamestate
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started || AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                    __instance.shakeAmount = gameShakeAmount;
                else
                    __instance.shakeAmount = lobbyShakeAmount;
            }
        }
    }
}
