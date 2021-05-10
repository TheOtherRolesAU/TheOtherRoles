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
        public static string fullCredentials = $"<size=120%><color=#FCCE03FF>TheOtherRoles</color></size> <size=60%>v{TheOtherRolesPlugin.Version.ToString()}:</size><size=75%>\n- Modded by <color=#FCCE03FF>Eisbison</color>,\n<color=#FFEB91FF>Thunderstorm584</color> and <color=#FFEB91FF>EndOfFile</color>\n- Balanced with <color=#FFEB91FF>Dhalucard</color>\n- Button design by <color=#FFEB91FF>Bavari</color></size>";

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        private static class VersionShowerPatch {
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
        private static class PingTrackerPatch {
            static void Postfix(PingTracker __instance){
                __instance.text.alignment = TMPro.TextAlignmentOptions.BaselineRight;
				__instance.text.margin = new Vector4(0, 0, 0.5f, 0);
				__instance.text.fontSize = 3.0f;
				__instance.text.transform.localPosition = new Vector3(0, 0, 0);
				Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
				__instance.text.transform.position = new Vector3(topRight.x - 0.1f, topRight.y - 1.8f);

                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) {
				    __instance.text.text = __instance.text.text + "<size=120%><color=#FCCE03FF>\nTheOtherRoles</color></size> <size=70%>\nby <color=#FCCE03FF>Eisbison</color></size>";
                    AspectPosition component = __instance.GetComponent<AspectPosition>();
			        component.DistanceFromEdge = new Vector3(1.9f, 0.3f, 0f);
			        component.AdjustPosition();
                }
                else {
                    __instance.text.text += "\n" + fullCredentials;
                }
            }
        }	
    }
}
