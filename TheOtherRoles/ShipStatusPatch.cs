  
using HarmonyLib;
using static TheOtherRoles.TheOtherRoles;
using UnityEngine;

using SystemTypes = LGBKLKNAINN;
using SwitchSystem = FNEHFOPHPJO;
using ISystemType = CBFMKGACLNE;

namespace TheOtherRoles {

    [HarmonyPatch(typeof(ShipStatus))]
    public class ShipStatusPatch {

		[HarmonyPostfix]
		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
        public static bool Prefix(ref float __result, ShipStatus __instance, [HarmonyArgument(0)] GameData.OFKOJOKOOAK player) {
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.Electrical) ? __instance.Systems[SystemTypes.Electrical] : null;
            if (systemType == null) return true;
            SwitchSystem switchSystem = systemType.TryCast<SwitchSystem>();
            if (switchSystem == null) return true;

            float num = (float)switchSystem.OBNIGODIBIO / 255f;
            
            if (player == null || player.FGNJJFABIHJ) // IsDead
                __result = __instance.MaxLightRadius;
            else if (player.CIDDOFDJHJH) // IsImpostor
                __result = __instance.MaxLightRadius * PlayerControl.GameOptions.AFGNADFLBDB;
            else if (Lighter.lighter != null && Lighter.lighter.PlayerId == player.GMBAIPNOKLP && Lighter.lighterTimer > 0f) // if player is Lighter and Lighter has his ability active
                __result = Mathf.Lerp(__instance.MaxLightRadius * Lighter.lighterModeLightsOffVision, __instance.MaxLightRadius * Lighter.lighterModeLightsOnVision, num);
            else if (Trickster.trickster != null && Trickster.lightsOutTimer > 0f)
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, 0f) * PlayerControl.GameOptions.OJCJDCMDKKC; // Instant lights out? Maybe add a smooth transition?
            else
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, num) * PlayerControl.GameOptions.OJCJDCMDKKC;
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