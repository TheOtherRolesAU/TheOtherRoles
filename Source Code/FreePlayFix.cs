using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace TheOtherRoles {
    [HarmonyPatch]
    public static class FreePlayFix {
        public static bool isInFreePlay = false;

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
        public static class ShipStatusBeginPatch { 
            public static void Prefix(ShipStatus __instance) {
                System.Console.WriteLine("Begin");
                isInFreePlay = UnityEngine.Object.FindObjectsOfType<SystemConsole>().ToList()
                                .Find(console => console.name == "TaskAddConsole");
            }   
        }
    }
}