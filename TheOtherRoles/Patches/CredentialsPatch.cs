using HarmonyLib;
using InnerNet;
using TMPro;
using UnityEngine;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch]
    public static class CredentialsPatch
    {
        private const string MainMenuCredentials =
            @"Modded by <color=#FCCE03FF>Eisbison</color>, <color=#FCCE03FF>Thunderstorm584</color> & <color=#FCCE03FF>EndOfFile</color>
Design by <color=#FCCE03FF>Bavari</color>";

        private static readonly string FullCredentials =
            $@"<size=130%><color=#ff351f>TheOtherRoles</color></size> v{TheOtherRolesPlugin.Version}
<size=80%>Modded by <color=#FCCE03FF>Eisbison</color>,
<color=#FCCE03FF>Thunderstorm584</color> & <color=#FCCE03FF>EndOfFile</color>
Button design by <color=#FCCE03FF>Bavari</color></size>";

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        private static class VersionShowerPatch
        {
            private static void Postfix(VersionShower __instance)
            {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo == null) return;

                var credentials = Object.Instantiate(__instance.text, amongUsLogo.transform, true);
                credentials.transform.position = new Vector3(0, 0.1f, 0);
                credentials.SetText(MainMenuCredentials);
                credentials.alignment = TextAlignmentOptions.Center;
                credentials.fontSize *= 0.75f;

                var version = Object.Instantiate(credentials, amongUsLogo.transform, true);
                version.transform.position = new Vector3(0, -0.25f, 0);
                version.SetText($"v{TheOtherRolesPlugin.Version}");
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        private static class PingTrackerPatch
        {
            private static GameObject modStamp;

            private static void Prefix(PingTracker __instance)
            {
                if (modStamp == null)
                {
                    modStamp = new GameObject("ModStamp");
                    var rend = modStamp.AddComponent<SpriteRenderer>();
                    rend.sprite = TheOtherRolesPlugin.GetModStamp();
                    rend.color = new Color(1, 1, 1, 0.5f);
                    modStamp.transform.parent = __instance.transform.parent;
                    modStamp.transform.localScale *= 0.6f;
                }

                var offset = AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started ? 0.75f : 0f;
                modStamp.transform.position = HudManager.Instance.MapButton.transform.position + Vector3.down * offset;
            }

            private static void Postfix(PingTracker __instance)
            {
                __instance.text.alignment = TextAlignmentOptions.TopRight;
                var position = __instance.transform.localPosition;
                if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
                {
                    __instance.text.text =
                        $"<size=130%><color=#ff351f>TheOtherRoles</color></size> v{TheOtherRolesPlugin.Version}\n" +
                        __instance.text.text;
                    __instance.transform.localPosition = new Vector3(
                        PlayerControl.LocalPlayer.Data.IsDead ? 3.45f : 4.2f, position.y,
                        position.z);
                }
                else
                {
                    __instance.text.text = $"{FullCredentials}\n{__instance.text.text}";
                    __instance.transform.localPosition = new Vector3(3.5f, position.y,
                        position.z);
                }
            }
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        private static class LogoPatch
        {
            private static void Postfix()
            {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo != null)
                {
                    amongUsLogo.transform.localScale *= 0.6f;
                    amongUsLogo.transform.position += Vector3.up * 0.25f;
                }

                var torLogo = new GameObject("bannerLogo_TOR");
                torLogo.transform.position = Vector3.up;
                var renderer = torLogo.AddComponent<SpriteRenderer>();
                renderer.sprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.Banner.png", 300f);
            }
        }
    }
}