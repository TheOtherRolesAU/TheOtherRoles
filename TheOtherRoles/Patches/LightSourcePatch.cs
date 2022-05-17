using HarmonyLib;
using UnityEngine;


namespace TheOtherRoles.Patches {

	[HarmonyPatch(typeof(LightSource), nameof(LightSource.Start))]

	class LightSourceStartPatch {
		static void Postfix(LightSource __instance) {
			__instance.transform.position += Vector3.down * 0.095f;  // Fixes Polus Rock / Garbage / Boxes reducing vision to 0
			return;
		}
	}
}