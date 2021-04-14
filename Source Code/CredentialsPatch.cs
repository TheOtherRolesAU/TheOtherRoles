using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheOtherRoles.Hats

namespace TheOtherRoles
{
    [HarmonyPatch]
    public static class CredentialsPatch {
        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]

        private static class VersionShowerPatch
        {
            static void Postfix(VersionShower __instance) {
                string spacer = new String('\n', 21);
                string fullCredentials = $"<color=#FCCE03FF>TheOtherRoles</color> v{TheOtherRolesPlugin.Major}.{TheOtherRolesPlugin.Minor}.{TheOtherRolesPlugin.Patch}:\n- Modded by <color=#FCCE03FF>TheOtherRoles</color> and <color=#FFEB91FF>Thunderstorm584</color>\n- Balanced with <color=#FFEB91FF>Dhalucard</color>\n- Button design by <color=#FFEB91FF>Bavari</color>";
                if (__instance.text.text.Contains(spacer))
                    __instance.text.text += "\n" + fullCredentials;
                else
                    __instance.text.text += spacer + fullCredentials;
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        private static class PingTrackerPatch
        {
                    private static string GenerateHatText(HatCreation.HatData data)
            {
                return $"\n{data.name} hat by {data.author}";
            }
            static void Postfix(VersionShower __instance)
            {
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GCDONLGCMIL.Started)
                    {
                    __instance.text.text += "\n<color=#FCCE03FF>TheOtherRoles</color>\nModded by <color=#FCCE03FF>Eisbison</color>";
                    }
                    if (!MeetingHud.Instance)
                {
                    if (HatCreation.IdToData.ContainsKey(PlayerControl.LocalPlayer.PPMOEEPBHJO.CPGFLBANALE))
                    {
                        var data = HatCreation.IdToData[PlayerControl.LocalPlayer.PPMOEEPBHJO.CPGFLBANALE];
                        __instance.text.m_text += GenerateHatText(data);
                    }
                    else __instance.text.m_text += "\n<color=#ec1919>Custom Hats </color>By <color=#4D0268>MrFawkes1337</color>";
                    }
                else
                    __instance.text.text += "\n\n<color=#FCCE03FF>TheOtherRoles</color>\nModded by <color=#FCCE03FF>Eisbison</color>\nand <color=#FFEB91FF>Thunderstorm584</color>\nBalanced with <color=#FFEB91FF>Dhalucard</color>\nButton design by <color=#FFEB91FF>Bavari</color>";
            }
        }
    }
}
