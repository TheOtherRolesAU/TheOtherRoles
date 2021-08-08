using HarmonyLib;
using TheOtherRoles.Roles;
using UnityEngine;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch(typeof(ShipStatus))]
    public class ShipStatusPatch
    {
        private static int originalNumCommonTasksOption;
        private static int originalNumShortTasksOption;
        private static int originalNumLongTasksOption;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
        public static bool Prefix(ref float __result, ShipStatus __instance,
            [HarmonyArgument(0)] GameData.PlayerInfo player)
        {
            var systemType = __instance.Systems.ContainsKey(SystemTypes.Electrical)
                ? __instance.Systems[SystemTypes.Electrical]
                : null;
            var switchSystem = systemType?.TryCast<SwitchSystem>();
            if (switchSystem == null) return true;

            var num = switchSystem.Value / 255f;

            if (player == null || player.IsDead) // IsDead
            {
                __result = __instance.MaxLightRadius;
            }
            else if (player.IsImpostor
                     || Jackal.Instance.player && Jackal.Instance.player.PlayerId == player.PlayerId &&
                     Jackal.hasImpostorVision
                     || Sidekick.Instance.player && Sidekick.Instance.player.PlayerId == player.PlayerId &&
                     Sidekick.hasImpostorVision
                     || Spy.Instance.player && Spy.Instance.player.PlayerId == player.PlayerId &&
                     Spy.hasImpostorVision) // Impostor, Jackal/Sidekick or Spy with Impostor vision
            {
                __result = __instance.MaxLightRadius * PlayerControl.GameOptions.ImpostorLightMod;
            }
            else if (Lighter.Instance.player != null && Lighter.Instance.player.PlayerId == player.PlayerId &&
                     Lighter.lighterTimer > 0f) // if player is Lighter and Lighter has his ability active
            {
                __result = Mathf.Lerp(__instance.MaxLightRadius * Lighter.lighterModeLightsOffVision,
                    __instance.MaxLightRadius * Lighter.lighterModeLightsOnVision, num);
            }
            else if (Trickster.Instance.player != null && Trickster.lightsOutTimer > 0f)
            {
                var lerpValue = 1f;
                if (Trickster.lightsOutDuration - Trickster.lightsOutTimer < 0.5f)
                    lerpValue = Mathf.Clamp01((Trickster.lightsOutDuration - Trickster.lightsOutTimer) * 2);
                else if (Trickster.lightsOutTimer < 0.5) lerpValue = Mathf.Clamp01(Trickster.lightsOutTimer * 2);
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, 1 - lerpValue) *
                           PlayerControl.GameOptions.CrewLightMod; // Instant lights out? Maybe add a smooth transition?
            }
            else
            {
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, num) *
                           PlayerControl.GameOptions.CrewLightMod;
            }

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.IsGameOverDueToDeath))]
        public static void Postfix2(ShipStatus __instance, ref bool __result)
        {
            __result = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
        public static bool Prefix(ShipStatus __instance)
        {
            var commonTaskCount = __instance.CommonTasks.Count;
            var normalTaskCount = __instance.NormalTasks.Count;
            var longTaskCount = __instance.LongTasks.Count;
            originalNumCommonTasksOption = PlayerControl.GameOptions.NumCommonTasks;
            originalNumShortTasksOption = PlayerControl.GameOptions.NumShortTasks;
            originalNumLongTasksOption = PlayerControl.GameOptions.NumLongTasks;
            if (PlayerControl.GameOptions.NumCommonTasks > commonTaskCount)
                PlayerControl.GameOptions.NumCommonTasks = commonTaskCount;
            if (PlayerControl.GameOptions.NumShortTasks > normalTaskCount)
                PlayerControl.GameOptions.NumShortTasks = normalTaskCount;
            if (PlayerControl.GameOptions.NumLongTasks > longTaskCount)
                PlayerControl.GameOptions.NumLongTasks = longTaskCount;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
        public static void Postfix3(ShipStatus __instance)
        {
            // Restore original settings after the tasks have been selected
            PlayerControl.GameOptions.NumCommonTasks = originalNumCommonTasksOption;
            PlayerControl.GameOptions.NumShortTasks = originalNumShortTasksOption;
            PlayerControl.GameOptions.NumLongTasks = originalNumLongTasksOption;
        }
    }
}