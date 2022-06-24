using HarmonyLib;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;


namespace TheOtherRoles.Patches {
	
	[HarmonyPatch(typeof(MapBehaviour))]
	class MapBehaviourPatch {

		[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
		static bool Prefix(MapBehaviour __instance) {
			if (!MeetingHud.Instance) return true;  // Only run in meetings, and then set the Position of the HerePoint to the Position before the Meeting!
			if (!MapUtilities.CachedShipStatus) {
				return false;
			}
			Vector3 vector = AntiTeleport.position != null? AntiTeleport.position : CachedPlayer.LocalPlayer.transform.position;
			vector /= MapUtilities.CachedShipStatus.MapScale;
			vector.x *= Mathf.Sign(MapUtilities.CachedShipStatus.transform.localScale.x);
			vector.z = -1f;
			__instance.HerePoint.transform.localPosition = vector;
			CachedPlayer.LocalPlayer.PlayerControl.SetPlayerMaterialColors(__instance.HerePoint);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
		static bool Prefix3(MapBehaviour __instance) {
			if (!MeetingHud.Instance || __instance.IsOpen) return true;  // Only run in meetings and when the map is closed

			CachedPlayer.LocalPlayer.PlayerControl.SetPlayerMaterialColors(__instance.HerePoint);
			__instance.GenericShow();
			__instance.taskOverlay.Show();
			__instance.ColorControl.SetColor(new Color(0.05f, 0.2f, 1f, 1f));
			FastDestroyableSingleton<HudManager>.Instance.SetHudActive(false);
			return false;
		}
	}
}
