  
using HarmonyLib;
using static TheOtherRoles.TheOtherRoles;
using UnityEngine;

using SystemTypes = BCPJLGGNHBC;
using SwitchSystem = ABIMJJMBJJM;
using ISystemType = JBBCJFNFOBB;

namespace TheOtherRoles {

    [HarmonyPatch(typeof(ShipStatus))]
    public class ShipStatusPatch {

		[HarmonyPostfix]
		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
        public static bool Prefix(ref float __result, ShipStatus __instance, [HarmonyArgument(0)] GameData.LGBOMGHJELL player) {
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.Electrical) ? __instance.Systems[SystemTypes.Electrical] : null;
            if (systemType == null) return true;
            SwitchSystem switchSystem = systemType.TryCast<SwitchSystem>();
            if (switchSystem == null) return true;

            float num = (float)switchSystem.FPHHBGCHEJG / 255f;
            
            if (player == null || player.IAGJEKLJCCI) // IsDead
                __result = __instance.MaxLightRadius;
            else if (player.FDNMBJOAPFL) // IsImpostor
                __result = __instance.MaxLightRadius * PlayerControl.GameOptions.EJPJACEEECE;
            else if (Lighter.lighter != null && Lighter.lighter.PlayerId == player.FNPNJHNKEBK && Lighter.lighterTimer > 0f) // if player is Lighter and Lighter has his ability active
                __result = Mathf.Lerp(__instance.MaxLightRadius * Lighter.lighterModeLightsOffVision, __instance.MaxLightRadius * Lighter.lighterModeLightsOnVision, num);
            else if (Trickster.trickster != null && Trickster.lightsOutTimer > 0f) {
                float lerpValue = 1f;
                if (Trickster.lightsOutDuration - Trickster.lightsOutTimer < 0.5f) lerpValue = Mathf.Clamp01((Trickster.lightsOutDuration - Trickster.lightsOutTimer) * 2);
                else if (Trickster.lightsOutTimer < 0.5) lerpValue = Mathf.Clamp01(Trickster.lightsOutTimer*2);
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, 1 - lerpValue) * PlayerControl.GameOptions.FAJGBBKDEHP; // Instant lights out? Maybe add a smooth transition?
            }
            else
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, num) * PlayerControl.GameOptions.FAJGBBKDEHP;
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