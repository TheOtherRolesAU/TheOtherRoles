using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BonusRoles
{
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public static class VersionShowerPatch
    {
        static void Postfix(VersionShower __instance) {
            string spacer = new String('\n', 15);
            if (__instance.text.Text.Contains(spacer))
                __instance.text.Text += "\n[FFFFFFFF]- Loaded [FCCE03FF]TheOtherRoles[FFFFFFFF] v1.0\n  by [FCCE03FF]Eisbison";
            else
                __instance.text.Text += spacer + "[FFFFFFFF]- Loaded [FCCE03FF]TheOtherRoles[FFFFFFFF] v1.0\n  by [FCCE03FF]Eisbison";
        }
    }

    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingTrackerPatch
    {
        static void Postfix(VersionShower __instance)
        {
            __instance.text.Text += "\n[FCCE03FF]TheOtherRoles[FFFFFFFF]";
            __instance.text.Text += "\nModded by [FCCE03FF]Eisbison";
        }
    }
}
