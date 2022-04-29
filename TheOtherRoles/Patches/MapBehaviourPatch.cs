using HarmonyLib;
using UnityEngine;


namespace TheOtherRoles.Patches {
	
	[HarmonyPatch(typeof(MapBehaviour))]
	class MapBehaviourPatch {

		[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
		static bool Prefix(MapBehaviour __instance) {
			if (!MeetingHud.Instance) return true;  // Only run in meetings, and then set the Position of the HerePoint to the Position before the Meeting!
			if (!ShipStatus.Instance) {
				return false;
			}
			Vector3 vector = AntiTeleport.position != null? AntiTeleport.position : PlayerControl.LocalPlayer.transform.position;
			vector /= ShipStatus.Instance.MapScale;
			vector.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);
			vector.z = -1f;
			__instance.HerePoint.transform.localPosition = vector;
			PlayerControl.LocalPlayer.SetPlayerMaterialColors(__instance.HerePoint);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
		static bool Prefix3(MapBehaviour __instance) {
			if (!MeetingHud.Instance || __instance.IsOpen) return true;  // Only run in meetings and when the map is closed

			PlayerControl.LocalPlayer.SetPlayerMaterialColors(__instance.HerePoint);
			__instance.GenericShow();
			__instance.taskOverlay.Show();
			__instance.ColorControl.SetColor(new Color(0.05f, 0.2f, 1f, 1f));
			DestroyableSingleton<HudManager>.Instance.SetHudActive(false);
			return false;
		}
	}
}