  
using HarmonyLib;
using UnityEngine;

namespace TheOtherRoles {
    public class GameStartManagerPatch  {
        private static float timer = 600f;

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch {
            public static void Postfix() {
                timer = 600f; 
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch {
            private static bool update = false;
            private static string currentText = "";

            public static void Prefix(GameStartManager __instance) {
                if (!AmongUsClient.Instance.CBKCIKKEJHI  || !GameData.Instance) return; // Not host or no instance
                update = GameData.Instance.BCFPPIDIMJK != __instance.GGIPHNCFKFH;
            }

            public static void Postfix(GameStartManager __instance) {
                if (!AmongUsClient.Instance.CBKCIKKEJHI  || !GameData.Instance) return; // Not host or no instance

                if (update) currentText = __instance.PlayerCounter.Text;

                timer = Mathf.Max(0f, timer -= Time.deltaTime);
                int minutes = (int)timer / 60;
                int seconds = (int)timer % 60;
                string suffix = $" ({minutes:00}:{seconds:00})";

                __instance.PlayerCounter.Text = currentText + suffix;
            }
        }
    }
}