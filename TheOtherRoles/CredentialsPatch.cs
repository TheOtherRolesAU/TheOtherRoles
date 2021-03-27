﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheOtherRoles
{
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public static class VersionShowerPatch
    {
        static void Postfix(VersionShower __instance) {
            string spacer = new String('\n', 15);
            string text = "[FCCE03FF]TheOtherRoles[] v1.8.2:\n- Modded by [FCCE03FF]Eisbison[] and [FFEB91FF]Thunderstorm584[]\n- Balanced with [FFEB91FF]Dhalucard";
            if (__instance.text.Text.Contains(spacer))
                __instance.text.Text += "\n" + text;
            else
                __instance.text.Text += spacer + text;
        }
    }

    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingTrackerPatch
    {
        static void Postfix(VersionShower __instance)
        {
            __instance.text.Text += "\n[FCCE03FF]TheOtherRoles[FFFFFFFF]";
            __instance.text.Text += "\nModded by [FCCE03FF]Eisbison[FFFFFFFF]";
        }
    }
}
