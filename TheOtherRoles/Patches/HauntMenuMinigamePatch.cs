using HarmonyLib;
using AmongUs.GameOptions;
using System.Collections.Generic;
using System.Linq;

namespace TheOtherRoles.Patches {
    [HarmonyPatch]
    public static class HauntMenuMinigamePatch {

        // Show the role name instead of just Crewmate / Impostor
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetFilterText))]
        public static void Postfix(HauntMenuMinigame __instance) {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.Normal) return;
            var target = __instance.HauntTarget;
            var roleInfo = RoleInfo.getRoleInfoForPlayer(target, false);
            string roleString = (roleInfo.Count > 0 && MapOptions.ghostsSeeRoles) ? roleInfo[0].name : "";
            if (__instance.HauntTarget.Data.IsDead) {
                __instance.FilterText.text = roleString + " Ghost";
                return;
            }
            __instance.FilterText.text = roleString;
            TheOtherRolesPlugin.Logger.LogMessage(roleString);
            return;
        }

        // The impostor filter now includes neutral roles
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.MatchesFilter))]
        public static void MatchesFilterPostfix(HauntMenuMinigame __instance, PlayerControl pc, ref bool __result) {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.Normal) return;
            if (__instance.filterMode == HauntMenuMinigame.HauntFilters.Impostor) {
                var info = RoleInfo.getRoleInfoForPlayer(pc, false);
                __result = (pc.Data.Role.IsImpostor || info.Any(x => x.isNeutral)) && !pc.Data.IsDead;
            }
        }


        // Shows the "haunt evil roles button"
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.Start))]
        public static bool StartPrefix(HauntMenuMinigame __instance) {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.Normal || !MapOptions.ghostsSeeRoles) return true;
            __instance.FilterButtons[0].gameObject.SetActive(true);
            int numActive = 0;
            int numButtons = __instance.FilterButtons.Count((PassiveButton s) => s.isActiveAndEnabled);
            float edgeDist = 0.6f * (float)numButtons;
		    for (int i = 0; i< __instance.FilterButtons.Length; i++)
		    {
			    PassiveButton passiveButton = __instance.FilterButtons[i];
			    if (passiveButton.isActiveAndEnabled)
			    {
				    passiveButton.transform.SetLocalX(FloatRange.SpreadToEdges(-edgeDist, edgeDist, numActive, numButtons));
				    numActive++;
			    }
            }
            return false;
        }
    }
}
