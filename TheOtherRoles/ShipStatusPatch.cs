  
using HarmonyLib;
using static TheOtherRoles.TheOtherRoles;
using UnityEngine;

namespace TheOtherRoles {

    [HarmonyPatch(typeof(ShipStatus))]
    public class ShipStatusPatch {

		[HarmonyPostfix]
		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
        public static bool Prefix(ref float __result, ShipStatus __instance, [HarmonyArgument(0)] GameData.PlayerInfo player) {
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.Electrical) ? __instance.Systems[SystemTypes.Electrical] : null;
            if (systemType == null) return true;
            SwitchSystem switchSystem = systemType.TryCast<SwitchSystem>();
            if (switchSystem == null) return true;

            float num = (float)switchSystem.Value / 255f;
            
            if (player == null || player.IsDead)
                __result = __instance.MaxLightRadius;
            else if (player.IsImpostor)
                __result = __instance.MaxLightRadius * PlayerControl.GameOptions.ImpostorLightMod;
            else if (Lighter.lighter != null && Lighter.lighter.PlayerId == player.PlayerId && Lighter.lighterTimer > 0f) 
                __result = Mathf.Lerp(__instance.MaxLightRadius * Lighter.lighterModeLightsOffVision, __instance.MaxLightRadius * Lighter.lighterModeLightsOnVision, num);
            else
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, num) * PlayerControl.GameOptions.CrewLightMod;
            return false;
        }

        [HarmonyPostfix]
		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.IsGameOverDueToDeath))]
		public static void Postfix2(ShipStatus __instance, ref bool __result)
		{
			__result = false;
		}
    }

}