  
using HarmonyLib;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles {

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    public class ShipStatusPatch {
        public static void Postfix(ref float __result, ShipStatus __instance, [HarmonyArgument(0)] GameData.PlayerInfo PlayerData) {
            if (Lighter.lighter != null && Lighter.lighter.PlayerId == PlayerData.PlayerId) {
                __result = __instance.MaxLightRadius * Lighter.lighterVision;
            }
        }
    }
}