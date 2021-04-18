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
using UnityEngine.Events;

namespace TheOtherRoles {
    [HarmonyPatch(typeof(RegionMenu), nameof(RegionMenu.Open))]
    public static class RegionMenuOpenPatch
    {
        private static TextBoxTMP ipField;
        private static TextBoxTMP portField;

        public static void Postfix(RegionMenu __instance)
        {
            var template = DestroyableSingleton<JoinGameButton>.CHNDKKBEIDG;

            if (ipField == null || ipField.gameObject == null) {
                ipField = UnityEngine.Object.Instantiate(template.GameIdText, __instance.transform);
                UnityEngine.Object.DestroyImmediate(ipField.transform.FindChild("arrowEnter").gameObject);

                ipField.transform.localPosition = new Vector3(0, -1f, -100f);
                ipField.characterLimit = 30;
                ipField.AllowSymbols = true;
                ipField.SetText(TheOtherRolesPlugin.Ip.Value);
                __instance.StartCoroutine(AEOEPNHOJDP.DCHLMIDMBHG(0.1f, new Action<float>((p) => {
                    ipField.outputText.SetText(TheOtherRolesPlugin.Ip.Value);
                    ipField.outputText.ForceMeshUpdate(true, true);      
                })));

                ipField.ClearOnFocus = false; 
                ipField.OnFocusLost = ipField.OnEnter = ipField.OnChange = new Button.ButtonClickedEvent();
                ipField.OnChange.AddListener((UnityAction)ipEvent);

                void ipEvent() {
                    TheOtherRolesPlugin.Ip.Value = ipField.text;
                    TheOtherRolesPlugin.CustomRegion.DefaultIp = ipField.text;
                }
            }
            if (portField == null || portField.gameObject == null) {
                portField = UnityEngine.Object.Instantiate(template.GameIdText, __instance.transform);
                UnityEngine.Object.DestroyImmediate(portField.transform.FindChild("arrowEnter").gameObject);

                portField.transform.localPosition = new Vector3(0, -1.75f, -100f);
                portField.characterLimit = 15;
                portField.SetText(TheOtherRolesPlugin.Port.Value.ToString());
                __instance.StartCoroutine(AEOEPNHOJDP.DCHLMIDMBHG(0.1f, new Action<float>((p) => {
                    portField.outputText.SetText(TheOtherRolesPlugin.Port.Value.ToString());
                    portField.outputText.ForceMeshUpdate(true, true);      
                })));


                portField.ClearOnFocus = false;
                portField.OnFocusLost = portField.OnEnter = portField.OnChange = new Button.ButtonClickedEvent();
                portField.OnChange.AddListener((UnityAction)portEvent);

                void portEvent() {
                    ushort port = 0;
                    if (ushort.TryParse(portField.text, out port)) {
                        TheOtherRolesPlugin.Port.Value = port;
                        TheOtherRolesPlugin.CustomRegion.Port = port;
                        portField.outputText.color = Color.white;
                    } else {
                        portField.outputText.color = Color.red;
                    }
                }
            }
        }
    }
}