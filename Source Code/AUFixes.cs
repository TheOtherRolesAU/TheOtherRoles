using HarmonyLib;
using Hazel;
using UnityEngine;
using TMPro;
using System;

namespace TheOtherRoles {

    [HarmonyPatch(typeof(OpenDoorConsole), nameof(OpenDoorConsole.Use))]
    class ToiletDoorFix { // Synchronize opening toilet doors among clients
        public static void Prefix(OpenDoorConsole __instance) {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.OpenToiletDoor, SendOption.None, -1);
            writer.Write(__instance.MyDoor.Id);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }

    [HarmonyPatch(typeof(NotificationPopper), nameof(NotificationPopper.Update))]
    public static class NotificationPopperUpdatePatch { // Fix position of notifications (e.g. player disconnected)
        public static void Postfix(NotificationPopper __instance) {
            if (__instance.alphaTimer > 0f) {
                var pos = __instance.transform.localPosition;
                __instance.transform.localPosition += new Vector3(0.5f, 0f, 0f);
            }
        }
    }

}