using HarmonyLib;
using Hazel;
using UnityEngine;
using TMPro;
using System;

namespace TheOtherRoles {
    [HarmonyPatch(typeof(OpenDoorConsole), nameof(OpenDoorConsole.Use))]
    class ToiletDoorFix {
        public static void Prefix(OpenDoorConsole __instance) {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.OpenToiletDoor, SendOption.None, -1);
            writer.Write(__instance.MyDoor.Id);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }
}