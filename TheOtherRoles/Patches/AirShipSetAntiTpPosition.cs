using HarmonyLib;
using System;

namespace TheOtherRoles.Patches {
    [HarmonyPatch]
    public static class AirShipSetAntiTpPosition {

        // Save the position of the player prior to starting the climb / gap platform
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ClimbLadder))]
        public static void prefix() {
            AntiTeleport.position = Players.CachedPlayer.LocalPlayer.transform.position;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MovingPlatformBehaviour), nameof(MovingPlatformBehaviour.UsePlatform))]
        public static void prefix2() {
            AntiTeleport.position = Players.CachedPlayer.LocalPlayer.transform.position;
        }
    }
}
