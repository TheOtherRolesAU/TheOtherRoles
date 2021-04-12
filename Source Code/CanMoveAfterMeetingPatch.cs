using HarmonyLib;

namespace TheOtherRoles
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.AMDJMEEHNIG), MethodType.Getter)]
    public static class CanMoveAfterMeetingPatch
    {
        private static bool Prefix(PlayerControl __instance, ref bool __result)
        {
            __result = __instance.moveable &&
                       !Minigame.Instance &&
                       (!DestroyableSingleton<HudManager>.JECNDKBIOFO ||
                        !DestroyableSingleton<HudManager>.CMJOLNCMAPD.Chat.LFGAAGECFFO &&
                        !DestroyableSingleton<HudManager>.CMJOLNCMAPD.KillOverlay.LFGAAGECFFO &&
                        !DestroyableSingleton<HudManager>.CMJOLNCMAPD.GameMenu.LFGAAGECFFO) &&
                       (!MapBehaviour.Instance || !MapBehaviour.Instance.IPOGMCKNALI) &&
                       !MeetingHud.Instance &&
                       !CustomPlayerMenu.Instance &&
                       !ExileController.Instance &&
                       !IntroCutscene.Instance;
            return false;
        }
    }
}