using HarmonyLib;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
    public static class ConstantsPatch {
        public static void Postfix(ref int __result) {
            /*if (AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame)
                __result += 25;*/
        }
    }
}