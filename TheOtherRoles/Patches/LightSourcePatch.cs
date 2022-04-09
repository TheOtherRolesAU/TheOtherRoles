using HarmonyLib;
using System;
using Hazel;
using UnityEngine;
using UnityEngine.Rendering;


namespace TheOtherRoles.Patches {

	[HarmonyPatch(typeof(LightSource), nameof(LightSource.DrawOcclusion))]

	class LightSourceUpdatePatch {
		static bool Prefix(LightSource __instance, float effectiveRadius) {
			if (__instance.cb == null) {
				__instance.cb = new CommandBuffer();
				__instance.cb.name = "Draw occlusion";
			}
			if (__instance.shadowTexture && __instance.shadowCasterMaterial) {
				float num = (float)__instance.shadowTexture.width;
				__instance.shadowCasterMaterial.SetFloat("_DepthCompressionValue", effectiveRadius);
				__instance.cb.Clear();
				__instance.cb.SetRenderTarget(__instance.shadowTexture);
				__instance.cb.ClearRenderTarget(true, true, new Color(1f, 1f, 1f, 1f));
				__instance.cb.SetGlobalTexture("_ShmapTexture", __instance.shadowTexture);
				__instance.cb.SetGlobalFloat("_Radius", __instance.LightRadius);
				__instance.cb.SetGlobalFloat("_Column", 0f);
				__instance.cb.SetGlobalVector("_lightPosition", __instance.transform.position + Vector3.down * 0.095f); ;
				__instance.cb.SetGlobalVector("_TexelSize", new Vector4(1f / num, 1f / num, num, num));
				__instance.cb.SetGlobalFloat("_DepthCompressionValue", effectiveRadius);
				__instance.cb.DrawMesh(__instance.occluderMesh, Matrix4x4.identity, __instance.shadowCasterMaterial);
				Graphics.ExecuteCommandBuffer(__instance.cb);
			}
			return false;
		}
	}
}