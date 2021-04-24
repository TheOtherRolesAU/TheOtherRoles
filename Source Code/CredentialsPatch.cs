using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public static class CredentialsPatch {
        public static string fullCredentials = 
$@"<color=#FCCE03FF>TheOtherRoles</color> v{TheOtherRolesPlugin.Major}.{TheOtherRolesPlugin.Minor}.{TheOtherRolesPlugin.Patch}:
- Modded by <color=#FCCE03FF>Eisbison</color>,
  <color=#FFEB91FF>Thunderstorm584</color> and <color=#FFEB91FF>EndOfFile</color>
- Balanced with <color=#FFEB91FF>Dhalucard</color>
- Button design by <color=#FFEB91FF>Bavari</color>";

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        private static class VersionShowerPatch
        {
            static void Postfix(VersionShower __instance) {
                string spacer = new String('\n', 8);

                if (__instance.text.text.Contains(spacer))
                    __instance.text.text = __instance.text.text + "\n" + fullCredentials;
                else
                    __instance.text.text = __instance.text.text + spacer + fullCredentials;
                __instance.text.alignment = TMPro.TextAlignmentOptions.TopLeft;
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        private static class PingTrackerPatch
        {
            static void Postfix(PingTracker __instance)
            {
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) {
                    __instance.text.text = "<color=#FCCE03FF>TheOtherRoles</color>\nModded by <color=#FCCE03FF>Eisbison</color>\n" + __instance.text.text;
                    __instance.transform.localPosition = new Vector3(2.583f, 2.675f, __instance.transform.localPosition.z);
                } else {
                    __instance.text.text = $"{fullCredentials}\n{__instance.text.text}";
                    __instance.transform.localPosition = new Vector3(1.25f, 2.675f, __instance.transform.localPosition.z);
                }
            }
        }
    }
}
