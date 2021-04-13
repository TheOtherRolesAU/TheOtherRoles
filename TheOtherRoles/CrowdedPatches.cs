// Adapted from https://github.com/CrowdedMods/CrowdedMod
/*
MIT License

Copyright (c) 2020 andry08 & CrowdedMods

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

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Hazel;

using Palette = BLMBFIODBKL;
using SaveManager = BLCGIFOPMIA;
using GameOptionsData = CEIOGGEDKAN;

namespace TheOtherRoles {
    [HarmonyPatch]
    static class GameOptionsPatch {
        public static List<SpriteRenderer> additionalButtons = new List<SpriteRenderer>();

        [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.Start))]
        public static class CreateOptionsPickerStartPatch {
            public static void Postfix(CreateOptionsPicker __instance) {
                List<SpriteRenderer> maxPlayerButtons = __instance.MaxPlayerButtons.ToList();
                if (maxPlayerButtons == null || maxPlayerButtons.Count <= 2) return;
                additionalButtons = new List<SpriteRenderer>();

                for (int i = 1; i < 6; i++) {
                    SpriteRenderer nextButton = Object.Instantiate(maxPlayerButtons.Last(), maxPlayerButtons.Last().transform.parent);
                    additionalButtons.Add(nextButton);
                    nextButton.enabled = false;
                    nextButton.gameObject.name = "1" + i;
                    TMPro.TMP_Text text = nextButton.gameObject.GetComponentInChildren<TMPro.TMP_Text>();
                    text.text = "1" + i;
                    text.color = Helpers.isCustomServer() ? Color.white : Palette.EGHCBLDNCGP;

                    nextButton.transform.position = nextButton.transform.position + new Vector3(i * (maxPlayerButtons[1].transform.position.x - maxPlayerButtons[0].transform.position.x), 0, 0);
                    var passiveButton = nextButton.GetComponent<PassiveButton>();
                    passiveButton.OnClick.RemoveAllListeners();

                    void onClick() {
                        bool isCustom = Helpers.isCustomServer();
                        foreach (SpriteRenderer renderer in additionalButtons) {
                            if (renderer != null && renderer.gameObject != null) {
                                renderer.enabled = false;
                                renderer.gameObject.GetComponentInChildren<TMPro.TMP_Text>().color = isCustom ? Color.white : Palette.EGHCBLDNCGP;
                            }
                        }

                        if (!isCustom) return;

                        nextButton.enabled = true;                    

                        byte value = byte.Parse(nextButton.name);
                        var targetOptions = __instance.GetTargetOptions();
                        if (value <= targetOptions.DIGMGCJMGDB) {
                            targetOptions.DIGMGCJMGDB = value - 1;
                            __instance.SetImpostorButtons(targetOptions.DIGMGCJMGDB);
                        }
                        __instance.SetMaxPlayersButtons(value);
                    }
                    passiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)onClick);
                }
            }
        }

        public static void setLegalSettings() {
            GameOptionsData hostOptions = SaveManager.LCNLLGFAEJE;
            GameOptionsData searchOptions = SaveManager.HGGNKBMAJLO;
            if (searchOptions.OKAPDMGFNED == 0) searchOptions.PFOGBKICOHD(0); // ToggleMapFilter

            hostOptions.DIGMGCJMGDB = searchOptions.DIGMGCJMGDB = 1; // NumImpostors
            hostOptions.OAFCIBONLNJ = searchOptions.OAFCIBONLNJ = 4; // MaxPlayers

            SaveManager.LCNLLGFAEJE = hostOptions;
            SaveManager.HGGNKBMAJLO = searchOptions;
        }

        [HarmonyPatch(typeof(FindGameButton), nameof(FindGameButton.OnClick))]
        class FindGameButtonOnClickPatch {
            public static void Prefix() {
                // Set legal settings
                setLegalSettings();
            }
        }

        [HarmonyPatch(typeof(JoinGameButton), nameof(JoinGameButton.OnClick))]
        class JoinGameButtonOnClickPatch {
            public static void Prefix() {
                // Set legal settings
                setLegalSettings();
            }
        }
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LCNLLGFAEJE), MethodType.Getter)]
    public static class SaveManagerGetHostOptions
    {
        public static bool Prefix(out GameOptionsData __result)
        {
            SaveManager.KMHAKICCDOJ ??= SaveManager.DHHFAHODPGO("gameHostOptions");

            SaveManager.KMHAKICCDOJ.DIGMGCJMGDB = Mathf.Clamp(SaveManager.KMHAKICCDOJ.DIGMGCJMGDB, 1, SaveManager.KMHAKICCDOJ.OAFCIBONLNJ - 1);
            SaveManager.KMHAKICCDOJ.OCPGKHJJAHL = Mathf.Clamp(SaveManager.KMHAKICCDOJ.OCPGKHJJAHL, 0, 2);

            __result = SaveManager.KMHAKICCDOJ;
            return false;
        }
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.GetAvailableId))]
    public static class GameDataGetAvailableIdPatch {
        public static bool Prefix(ref GameData __instance, out sbyte __result) {
            for (sbyte i = 0; i <= 15; i++)
                if (!__instance.AllPlayers.ToArray().Any(p => p.FNPNJHNKEBK == i)) {
                    __result = i;
                    return false;
                }
            __result = -1;
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckColor))]
    public static class PlayerControlCmdCheckColorPatch {
        public static bool Prefix(PlayerControl __instance, byte CKNNKEMNHAK) {
            if (!Helpers.isCustomServer()) return true;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetUncheckedColor, Hazel.SendOption.Reliable, -1);
            writer.Write(CKNNKEMNHAK);
            writer.Write(__instance.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.setUncheckedColor(CKNNKEMNHAK, __instance.PlayerId);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.UpdateAvailableColors))]
    public static class PlayerTabUpdateAvailableColorsPatch {
        public static bool Prefix(PlayerTab __instance) {
            if (!Helpers.isCustomServer()) return true;

            PlayerControl.SetPlayerMaterialColors(PlayerControl.LocalPlayer.PPMOEEPBHJO.IMMNCAGJJJC, __instance.DemoImage);
            for (int i = 0; i < Palette.AEDCMKGJKAG.Length; i++)
                __instance.HDGKLGFNEIB.Add(i);
            return false;
        }
    }

    [HarmonyPatch(typeof(SecurityLogger), nameof(SecurityLogger.Awake))]
    public static class SecurityLoggerAwakePatch
    {
        public static void Postfix(SecurityLogger __instance) {
            __instance.DLAMJHGNEIF = new float[15];
        }
    }

    [HarmonyPatch(typeof(KeyMinigame),nameof(KeyMinigame.Start))]
    public static class KeyMinigameStartPatch
    {
        public static bool Prefix(KeyMinigame __instance)
        {
            __instance.CKEKJGDMNDB = (PlayerControl.LocalPlayer != null) ? PlayerControl.LocalPlayer.PlayerId % 10 : 0;
            __instance.Slots[__instance.CKEKJGDMNDB].Image.sprite = __instance.Slots[__instance.CKEKJGDMNDB].Highlit;
            return false;
        }
    }
}