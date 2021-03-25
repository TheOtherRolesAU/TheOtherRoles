  
using HarmonyLib;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles {

    [HarmonyPatch(typeof(ShipStatus))]
    public class ShipStatusPatch {

		[HarmonyPostfix]
		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
        public static void Postfix1(ref float __result, ShipStatus __instance, [HarmonyArgument(0)] GameData.PlayerInfo PlayerData) {
            if (Lighter.lighter != null && Lighter.lighter.PlayerId == PlayerData.PlayerId) {
                __result = __instance.MaxLightRadius * Lighter.lighterVision;
            }
        }

        [HarmonyPostfix]
		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.IsGameOverDueToDeath))]
		public static void Postfix2(ShipStatus __instance, ref bool __result)
		{
			__result = false;
		}
    }

}