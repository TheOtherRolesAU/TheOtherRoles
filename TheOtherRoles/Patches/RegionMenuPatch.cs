// Adapted from https://github.com/MoltenMods/Unify
/*
MIT License

Copyright (c) 2021 Daemon

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System;
using TheOtherRoles.Utilities;
using UnityEngine.Events;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(RegionMenu), nameof(RegionMenu.Open))]
    public static class RegionMenuOpenPatch
    {
        private static TextBoxTMP ipField;
        private static TextBoxTMP portField;

        public static void Postfix(RegionMenu __instance) {
            if (!__instance.TryCast<RegionMenu>()) return;
            bool isCustomRegion = FastDestroyableSingleton<ServerManager>.Instance.CurrentRegion.Name == "Custom";
            if (!isCustomRegion)
            {
                if (ipField != null && ipField.gameObject != null) {
                    ipField.gameObject.SetActive(false);

                }
                if (portField != null && portField.gameObject != null) {
                    portField.gameObject.SetActive(false);
                }
            } else
            {
                if (ipField != null && ipField.gameObject != null)
                {
                    ipField.gameObject.SetActive(true);

                }
                if (portField != null && portField.gameObject != null)
                {
                    portField.gameObject.SetActive(true);
                }
            }
            var template = FastDestroyableSingleton<JoinGameButton>.Instance;
            var joinGameButtons = GameObject.FindObjectsOfType<JoinGameButton>();
            foreach (var t in joinGameButtons) {  // The correct button has a background, the other 2 dont
                if (t.GameIdText != null && t.GameIdText.Background != null) {
                    template = t;
                    break;
                }
            }
            if (template == null || template.GameIdText == null) return;

            if (ipField == null || ipField.gameObject == null) {
                ipField = UnityEngine.Object.Instantiate(template.GameIdText, __instance.transform);
                ipField.gameObject.name = "IpTextBox";
                var arrow = ipField.transform.FindChild("arrowEnter");
                if (arrow == null || arrow.gameObject == null) return;
                UnityEngine.Object.DestroyImmediate(arrow.gameObject);

                ipField.transform.localPosition = new Vector3(0.225f, -1f, -100f);
                ipField.characterLimit = 30;
                ipField.AllowSymbols = true;
                ipField.ForceUppercase = false;
                ipField.SetText(TheOtherRolesPlugin.Ip.Value);
                __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) => {
                    ipField.outputText.SetText(TheOtherRolesPlugin.Ip.Value);
                    ipField.SetText(TheOtherRolesPlugin.Ip.Value);
                })));

                ipField.ClearOnFocus = false; 
                ipField.OnEnter = ipField.OnChange = new Button.ButtonClickedEvent();
                ipField.OnFocusLost = new Button.ButtonClickedEvent();
                ipField.OnChange.AddListener((UnityAction)onEnterOrIpChange);
                ipField.OnFocusLost.AddListener((UnityAction)onFocusLost);
                ipField.gameObject.SetActive(isCustomRegion);

                void onEnterOrIpChange() {
                    TheOtherRolesPlugin.Ip.Value = ipField.text;
                }

                void onFocusLost() {
                    TheOtherRolesPlugin.UpdateRegions();
                    __instance.ChooseOption(ServerManager.DefaultRegions[ServerManager.DefaultRegions.Length - 1]);
                }
            }

            if (portField == null || portField.gameObject == null) {
                portField = UnityEngine.Object.Instantiate(template.GameIdText, __instance.transform);
                portField.gameObject.name = "PortTextBox";
                var arrow = portField.transform.FindChild("arrowEnter");
                if (arrow == null || arrow.gameObject == null) return;
                UnityEngine.Object.DestroyImmediate(arrow.gameObject);

                portField.transform.localPosition = new Vector3(0.225f, -1.75f, -100f);
                portField.characterLimit = 5;
                portField.SetText(TheOtherRolesPlugin.Port.Value.ToString());
                __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) => {
                    portField.outputText.SetText(TheOtherRolesPlugin.Port.Value.ToString());
                    portField.SetText(TheOtherRolesPlugin.Port.Value.ToString()); 
                })));


                portField.ClearOnFocus = false;
                portField.OnEnter = portField.OnChange = new Button.ButtonClickedEvent();
                portField.OnFocusLost = new Button.ButtonClickedEvent();
                portField.OnChange.AddListener((UnityAction)onEnterOrPortFieldChange);
                portField.OnFocusLost.AddListener((UnityAction)onFocusLost);
                portField.gameObject.SetActive(isCustomRegion);

                void onEnterOrPortFieldChange() {
                    ushort port = 0;
                    if (ushort.TryParse(portField.text, out port)) {
                        TheOtherRolesPlugin.Port.Value = port;
                        portField.outputText.color = Color.white;
                    } else {
                        portField.outputText.color = Color.red;
                    }
                }
                
                void onFocusLost() {
                    TheOtherRolesPlugin.UpdateRegions();
                    __instance.ChooseOption(ServerManager.DefaultRegions[ServerManager.DefaultRegions.Length - 1]);
                }
            }
        }
    }
}