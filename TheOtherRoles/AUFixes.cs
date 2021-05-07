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

    [HarmonyPatch(typeof(MatchMaker), nameof(MatchMaker.Start))]
    public static class MatchMakerPatch { // Reactor Region Fix
        public static void Prefix(MatchMaker __instance) {
            var parent = __instance.GetComponentInParent<Transform>().parent;
            if (parent == null) return; // Local
            var tmps = parent.GetComponentsInChildren<TextMeshPro>();
            foreach (var tmp in tmps) {
                if (tmp.name == "RegionText_TMP") {
                    var region = DestroyableSingleton<ServerManager>.Instance.CurrentRegion;
                    var name = DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(region.TranslateName, region.Name, Array.Empty<Il2CppSystem.Object>());
                    tmp.text = name;
                    break;
                }
            }
        }
    }
}