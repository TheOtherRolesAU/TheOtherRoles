using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheEpicRoles.Patches {
    [HarmonyPatch]
    public static class CredentialsPatch {
        public static string terColor               = "#00ffd9";
        public static string torColor               = "#fcce03";
        public static string fullCredentials        = $"<size=130%><color={terColor}>TheEpicRoles</color></size> <size=50%>v{TheEpicRolesPlugin.Version.ToString()}\nRemodded by <color={terColor}>LaicosVK</color>, <color={terColor}>Nova</color> & <color={terColor}>DasMonschta</color>\nGraphics by <color={terColor}>moep424</color></size>";
        public static string mainMenuCredentials    = $"Remodded by <color={terColor}>LaicosVK</color>, <color={terColor}>Nova</color> & <color={terColor}>DasMonschta</color>\nGraphics by <color={terColor}>moep424</color>";
        public static string torCredentials         = $"<size=40%><color={torColor}>Original Mod by github.com/Eisbison/TheOtherRoles</color></size>";

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        private static class VersionShowerPatch
        {
            static void Postfix(VersionShower __instance) {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo == null) return;

                var credentials = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(__instance.text);
                credentials.transform.position = new Vector3(0, 0, 0);
                credentials.SetText($"v{TheEpicRolesPlugin.Version.ToString()}\n<size=1f%>\n</size>{mainMenuCredentials}\n<size=1%>\n</size>\n{torCredentials}\n");
                credentials.alignment = TMPro.TextAlignmentOptions.Center;
                credentials.fontSize *= 0.75f;
                credentials.SetOutlineThickness(0);
                credentials.transform.SetParent(amongUsLogo.transform);
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
                    rend.sprite = TheEpicRolesPlugin.GetModStamp();
                    rend.color = new Color(1, 1, 1, 0.5f);
                    modStamp.transform.parent = __instance.transform.parent;
                    modStamp.transform.localScale *= 0.6f;
                }
                float offset = (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) ? 0.75f : 0f;
                modStamp.transform.position = HudManager.Instance.MapButton.transform.position + Vector3.down * offset;
            }

            static void Postfix(PingTracker __instance){
                __instance.text.alignment = TMPro.TextAlignmentOptions.TopRight;
                __instance.text.SetOutlineThickness(0);
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) {
                    __instance.text.text = $"<size=130%><color={terColor}>TheEpicRoles</color></size> <size=50%>v{TheEpicRolesPlugin.Version.ToString()}</size>\n" + __instance.text.text;
                    if (PlayerControl.LocalPlayer.Data.IsDead || (!(PlayerControl.LocalPlayer == null) && (PlayerControl.LocalPlayer == Lovers.lover1 || PlayerControl.LocalPlayer == Lovers.lover2))) {
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
            static void Postfix(MainMenuManager __instance) {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo != null) {
                    amongUsLogo.transform.localScale *= 0.6f;
                    amongUsLogo.transform.position += Vector3.up * 0.25f;
                }       

                var torLogo = new GameObject("bannerLogo_TOR");
                torLogo.transform.position = Vector3.up;
                var renderer = torLogo.AddComponent<SpriteRenderer>();
                renderer.sprite = Helpers.loadSpriteFromResources("TheEpicRoles.Resources.Banner.png", 300f);
            }
        }
    }
}


