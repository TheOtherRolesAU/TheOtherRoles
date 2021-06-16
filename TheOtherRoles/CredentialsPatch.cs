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
$@"<size=130%><color=#ff351f>TheOtherRoles</color></size> v{TheOtherRolesPlugin.Version.ToString()}
<size=80%>Modded by <color=#FCCE03FF>Eisbison</color>,
<color=#FCCE03FF>Thunderstorm584</color> & <color=#FCCE03FF>EndOfFile</color>
Balanced with <color=#FCCE03FF>Dhalucard</color>
Button design by <color=#FCCE03FF>Bavari</color></size>";

    public static string mainMenuCredentials = 
$@"Modded by <color=#FCCE03FF>Eisbison</color>, <color=#FCCE03FF>Thunderstorm584</color> & <color=#FCCE03FF>EndOfFile</color>
Balanced with <color=#FCCE03FF>Dhalucard</color>      Design by <color=#FCCE03FF>Bavari</color>";

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        private static class VersionShowerPatch
        {
            static void Postfix(VersionShower __instance) {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo == null) return;

                var credentials = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(__instance.text);
                credentials.transform.position = new Vector3(0, 0.1f, 0);
                credentials.SetText(mainMenuCredentials);
                credentials.alignment = TMPro.TextAlignmentOptions.Center;
                credentials.fontSize *= 0.75f;

                var version = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(credentials);
                version.transform.position = new Vector3(0, -0.25f, 0);
                version.SetText($"v{TheOtherRolesPlugin.Version.ToString()}");

                credentials.transform.SetParent(amongUsLogo.transform);
                version.transform.SetParent(amongUsLogo.transform);
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        private static class PingTrackerPatch
        {
            private static GameObject modStamp;
            static void Prefix(PingTracker __instance) {
                if (modStamp == null) {
                    modStamp = new GameObject("ModStamp");
                    var rend = modStamp.AddComponent<SpriteRenderer>();
                    rend.sprite = TheOtherRolesPlugin.GetModStamp();
                    rend.color = new Color(1, 1, 1, 0.5f);
                    modStamp.transform.parent = __instance.transform.parent;
                    modStamp.transform.localScale *= 0.6f;
                }
                float offset = (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) ? 0.75f : 0f;
                modStamp.transform.position = HudManager.Instance.MapButton.transform.position + Vector3.down * offset;
            }

            static void Postfix(PingTracker __instance){
                __instance.text.alignment = TMPro.TextAlignmentOptions.TopRight;
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) {
                    __instance.text.text = $"<size=130%><color=#ff351f>TheOtherRoles</color></size> v{TheOtherRolesPlugin.Version.ToString()}\n" + __instance.text.text;
                    if (PlayerControl.LocalPlayer.Data.IsDead) {
                        __instance.transform.localPosition = new Vector3(3.45f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                    } else {
                        __instance.transform.localPosition = new Vector3(4.2f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                    }
                } else {
                    __instance.text.text = $"{fullCredentials}\n{__instance.text.text}";
                    __instance.transform.localPosition = new Vector3(3.5f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                }
            }
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        private static class LogoPatch
        {
            static void Postfix(PingTracker __instance) {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo != null) {
                    amongUsLogo.transform.localScale *= 0.6f;
                    amongUsLogo.transform.position += Vector3.up * 0.25f;
                }

                var torLogo = new GameObject("bannerLogo_TOR");
                torLogo.transform.position = Vector3.up;
                var renderer = torLogo.AddComponent<SpriteRenderer>();
                renderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Banner.png", 300f);                                
            }
        }
    }
}
