using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public static class CredentialsPatch {
        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]

        private static class VersionShowerPatch
        {
            static void Postfix(VersionShower __instance) {
                string spacer = new String('\n', 21);
                string fullCredentials = "[FCCE03FF]TheOtherRoles[] v2.2.0:\n- Modded by [FCCE03FF]Eisbison[] and [FFEB91FF]Thunderstorm584[]\n- Balanced with [FFEB91FF]Dhalucard[]\n- Button design by [FFEB91FF]Bavari[]";
                if (__instance.text.Text.Contains(spacer))
                    __instance.text.Text += "\n" + fullCredentials;
                else
                    __instance.text.Text += spacer + fullCredentials;
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        private static class PingTrackerPatch
        {
            static void Postfix(VersionShower __instance)
            {
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.CJDCOJJNIGL.Started)
                    __instance.text.Text += "\n[FCCE03FF]TheOtherRoles[]\nModded by [FCCE03FF]Eisbison[]";
                else
                    __instance.text.Text += "\n\n[FCCE03FF]TheOtherRoles[]\nModded by [FCCE03FF]Eisbison[]\nand [FFEB91FF]Thunderstorm584[]\nBalanced with [FFEB91FF]Dhalucard[]\nButton design by [FFEB91FF]Bavari[]";
            }
        }
    }
}
