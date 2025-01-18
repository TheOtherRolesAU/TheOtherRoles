using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using System;
using UnityEngine.Windows.Speech;
using TheOtherRoles;
using static UnityEngine.GraphicsBuffer;

namespace TheOtherRoles.Patches {
    [HarmonyPatch]
    public static class TransportationToolPatches {
        /* 
         * Moving Plattform / Zipline / Ladders move the player out of bounds, thus we want to disable functions of the mod if the player is currently using one of these.
         * Save the players anti tp position before using it.
         * 
         * Zipline can also break camo, fix that one too.
         */
       
        public static bool isUsingTransportation(PlayerControl pc) {
            return pc.inMovingPlat || pc.onLadder;
        }

        // Zipline:
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ZiplineBehaviour), nameof(ZiplineBehaviour.Use), new Type[] {typeof(PlayerControl), typeof(bool)})]
        public static void prefix3(ZiplineBehaviour __instance, PlayerControl player, bool fromTop) {
            AntiTeleport.position = PlayerControl.LocalPlayer.transform.position;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZiplineBehaviour), nameof(ZiplineBehaviour.Use), new Type[] { typeof(PlayerControl), typeof(bool) })]
        public static void postfix(ZiplineBehaviour __instance, PlayerControl player, bool fromTop) {
            // Fix camo:
            __instance.StartCoroutine(Effects.Lerp(fromTop ? __instance.downTravelTime : __instance.upTravelTime, new System.Action<float>((p) => {
                HandZiplinePoolable hand;
                __instance.playerIdHands.TryGetValue(player.PlayerId, out hand);
                if (hand != null) {
                    if (Camouflager.camouflageTimer <= 0 && !Helpers.MushroomSabotageActive()) {
                        if (player == Morphling.morphling && Morphling.morphTimer > 0) {
                            hand.SetPlayerColor(Morphling.morphTarget.CurrentOutfit, PlayerMaterial.MaskType.None, 1f);
                            // Also set hat color, cause the line destroys it...
                            player.RawSetHat(Morphling.morphTarget.Data.DefaultOutfit.HatId, Morphling.morphTarget.Data.DefaultOutfit.ColorId);
                        } else {
                            hand.SetPlayerColor(player.CurrentOutfit, PlayerMaterial.MaskType.None, 1f);
                        }
                    } else {
                        PlayerMaterial.SetColors(6, hand.handRenderer);
                    }
                }
            })));
        }

        // Save the position of the player prior to starting the climb / gap platform
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ClimbLadder))]
        public static void prefix() {
            AntiTeleport.position = PlayerControl.LocalPlayer.transform.position;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ClimbLadder))]
        public static void postfix2(PlayerPhysics __instance, Ladder source, byte climbLadderSid) {
            // Fix camo:
            var player = __instance.myPlayer;
            __instance.StartCoroutine(Effects.Lerp(5.0f, new System.Action<float>((p) => {
                if (Camouflager.camouflageTimer <= 0 && !Helpers.MushroomSabotageActive() && player == Morphling.morphling && Morphling.morphTimer > 0.1f) {
                    player.RawSetHat(Morphling.morphTarget.Data.DefaultOutfit.HatId, Morphling.morphTarget.Data.DefaultOutfit.ColorId);
                }
            })));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MovingPlatformBehaviour), nameof(MovingPlatformBehaviour.UsePlatform))]
        public static void prefix2() {
            AntiTeleport.position = PlayerControl.LocalPlayer.transform.position;
        }
    }
}
