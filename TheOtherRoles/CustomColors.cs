using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Il2CppSystem;
using HarmonyLib;
using UnhollowerBaseLib;
using Assets.CoreScripts;

namespace TheOtherRoles {
    public class CustomColors {
        protected static Dictionary<int, string> ColorStrings = new Dictionary<int, string>();
        public static List<int> lighterColors = new List<int>(){ 3, 4, 5, 7, 10, 11};
        public static uint pickableColors = (uint)Palette.ColorNames.Length;

        /* version 1
        private static readonly List<int> ORDER = new List<int>() { 7, 17, 5, 33, 4, 
                                                                    30, 0, 19, 27, 3,
                                                                    13, 25, 18, 15, 23,
                                                                    8, 32, 1, 21, 31,
                                                                    10, 34, 12, 14, 28,
                                                                    22, 29, 11, 26, 2,
                                                                    20, 24, 9, 16, 6 }; */
        private static readonly List<int> ORDER = new List<int>() { 7, 17, 5, 33, 4, 
                                                                    30, 0, 19, 27, 3,
                                                                    13, 25, 18, 15, 23,
                                                                    8, 32, 1, 21, 31,
                                                                    10, 34, 12, 28, 22,
                                                                    29, 11, 2, 26, 14,
                                                                    20, 24, 9, 16, 6 };
        public static void Load() {
            List<StringNames> longlist = Enumerable.ToList<StringNames>(Palette.ColorNames);
            List<StringNames> shortlist = Enumerable.ToList<StringNames>(Palette.ShortColorNames);
            List<Color32> colorlist = Enumerable.ToList<Color32>(Palette.PlayerColors);
            List<Color32> shadowlist = Enumerable.ToList<Color32>(Palette.ShadowColors);

            List<CustomColor> colors = new List<CustomColor>();

            /* New official Colors */
            colors.Add(new CustomColor { longname = "Gray", shortname = "GRAY", // Gray     #8395a5 #475664
                                        color = new Color32(0x89, 0x95, 0xA5, byte.MaxValue), 
                                        shadow = new Color32(0x47, 0x56, 0x64, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Coral", shortname = "CORAL", // Coral    #ec747a #b44362
                                        color = new Color32(0xEC, 0x74, 0x7A, byte.MaxValue), 
                                        shadow = new Color32(0xB4, 0x43, 0x62, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Tan", shortname = "TAN", // Tan      #9f9787 #51403e
                                        color = new Color32(0x9F, 0x97, 0x87, byte.MaxValue), 
                                        shadow = new Color32(0x51, 0x40, 0x3E, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Rose", shortname = "ROSE",  // Rose     #ffd5ed #de91b2
                                        color = new Color32(0xFF, 0xD5, 0xED, byte.MaxValue), 
                                        shadow = new Color32(0xDE, 0x91, 0xB2, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Maroon", shortname = "MAROON", // Maroon   #6d2b3d #410f1c
                                        color = new Color32(0x6D, 0x2B, 0x3D, byte.MaxValue), 
                                        shadow = new Color32(0x41, 0xF, 0x1C, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Banana", shortname = "BANANA", // Banana   #fffdbd #d1bc89
                                        color = new Color32(0xFF, 0xFD, 0xBD, byte.MaxValue), 
                                        shadow = new Color32(0xD1, 0xBC, 0x89, byte.MaxValue),
                                        isLighterColor = true });
            /* Custom Colors */
            colors.Add(new CustomColor { longname = "Salmon", shortname = "SALMN", 
                                        color = new Color32(239, 191, 192, byte.MaxValue), // color = new Color32(0xD8, 0x82, 0x83, byte.MaxValue),
                                        shadow = new Color32(182, 119, 114, byte.MaxValue), // shadow = new Color32(0xA5, 0x63, 0x65, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Bordeaux", shortname = "BRDX", 
                                        color = new Color32(109, 7, 26, byte.MaxValue), 
                                        shadow = new Color32(54, 2, 11, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Olive", shortname = "OLIVE", 
                                        color = new Color32(154, 140, 61, byte.MaxValue), 
                                        shadow = new Color32(104, 95, 40, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Turqoise", shortname = "TURQ", 
                                        color = new Color32(22, 132, 176, byte.MaxValue), 
                                        shadow = new Color32(15, 89, 117, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Mint", shortname = "MINT", 
                                        color = new Color32(111, 192, 156, byte.MaxValue), 
                                        shadow = new Color32(65, 148, 111, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Lavender", shortname = "LVNDR", 
                                        color = new Color32(173, 126, 201, byte.MaxValue), 
                                        shadow = new Color32(131, 58, 203, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Nougat", shortname = "NOUGT", 
                                        color = new Color32(160, 101, 56, byte.MaxValue), 
                                        shadow = new Color32(115, 15, 78, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Peach", shortname = "PEACH", 
                                        color = new Color32(255, 164, 119, byte.MaxValue), 
                                        shadow = new Color32(238, 128, 100, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Wasabi", shortname = "WSBI", 
                                        color = new Color32(112, 143, 46, byte.MaxValue), 
                                        shadow = new Color32(72, 92, 29, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Hot Pink", shortname = "HTPNK", 
                                        color = new Color32(255, 51, 102, byte.MaxValue), 
                                        shadow = new Color32(232, 0, 58, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Petrol", shortname = "PTRL", 
                                        color = new Color32(0, 99, 105, byte.MaxValue), 
                                        shadow = new Color32(0, 61, 54, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Lemon", shortname = "LEMON", 
                                        color = new Color32(0xDB, 0xFD, 0x2F, byte.MaxValue), 
                                        shadow = new Color32(0x74, 0xE5, 0x10, byte.MaxValue), 
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Signal Orange", shortname = "SIGN", 
                                        color = new Color32(0xF7, 0x44, 0x17, byte.MaxValue), 
                                        shadow = new Color32(0x9B, 0x2E, 0x0F, byte.MaxValue),
                                        isLighterColor = true });   

            colors.Add(new CustomColor { longname = "Teal", shortname = "TEAL", 
                                        color = new Color32(0x25, 0xB8, 0xBF, byte.MaxValue), 
                                        shadow = new Color32(0x12, 0x89, 0x86, byte.MaxValue),
                                        isLighterColor = false });   

            colors.Add(new CustomColor { longname = "Blurple", shortname = "BLURP", 
                                        color = new Color32(0x59, 0x3C, 0xD6, byte.MaxValue), 
                                        shadow = new Color32(0x29, 0x17, 0x96, byte.MaxValue),
                                        isLighterColor = false });   

            colors.Add(new CustomColor { longname = "Sunrise", shortname = "SUN", 
                                        color = new Color32(0xFF, 0xCA, 0x19, byte.MaxValue), 
                                        shadow = new Color32(0xDB, 0x44, 0x42, byte.MaxValue),
                                        isLighterColor = true });

            colors.Add(new CustomColor { longname = "Eis", shortname = "EIS", 
                                        color = new Color32(0xA8, 0xDF, 0xFF, byte.MaxValue), 
                                        shadow = new Color32(0x59, 0x9F, 0xC8, byte.MaxValue),
                                        isLighterColor = true });     

            pickableColors += (uint)colors.Count; // Colors to show in Tab
            /** Hidden Colors **/     
                    
            /** Add Colors **/
            int id = 50000;
            foreach (CustomColor cc in colors) {
                longlist.Add((StringNames)id);
                CustomColors.ColorStrings[id++] = cc.longname;
                shortlist.Add((StringNames)id);
                CustomColors.ColorStrings[id++] = cc.shortname;
                colorlist.Add(cc.color);
                shadowlist.Add(cc.shadow);
                if (cc.isLighterColor)
                    lighterColors.Add(colorlist.Count - 1);
            }

            Palette.ShortColorNames = shortlist.ToArray();
            Palette.ColorNames = longlist.ToArray();
            Palette.PlayerColors = colorlist.ToArray();
            Palette.ShadowColors = shadowlist.ToArray();
            MedScanMinigame.ColorNames = Palette.ColorNames;
            Telemetry.ColorNames = Palette.ColorNames;
        }

        protected internal struct CustomColor {
            public string longname;
            public string shortname;
            public Color32 color;
            public Color32 shadow;
            public bool isLighterColor;
        }

        [HarmonyPatch]
        public static class CustomColorPatches {
            [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new[] {
                typeof(StringNames),
                typeof(Il2CppReferenceArray<Il2CppSystem.Object>)
            })]
            private class ColorStringPatch {
                public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name) {
                    if ((int)name >= 50000) {
                        string text = CustomColors.ColorStrings[(int)name];
                        if (text != null) {
                            __result = text;
                            return false;
                        }
                    }
                    return true;
                }
            }
            [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
            private static class PlayerTabEnablePatch {
                public static void Postfix(PlayerTab __instance) { // Replace instead
                    Il2CppArrayBase<ColorChip> chips = __instance.ColorChips.ToArray();

                    int cols = 5; // TODO: Design an algorithm to dynamically position chips to optimally fill space
                    for (int i = 0; i < ORDER.Count; i++) {
                        int pos = ORDER[i];
                        if (pos < 0 || pos > chips.Length)
                            continue;
                        ColorChip chip = chips[pos];
                        int row = i / cols, col = i % cols; // Dynamically do the positioning
                        chip.transform.localPosition = new Vector3(1.39f + (col * 0.485f), -0.34f - (row * 0.49f), chip.transform.localPosition.z);
                        chip.transform.localScale *= 0.78f;
                    }
                    for (int j = ORDER.Count; j < chips.Length; j++) { // If number isn't in order, hide it
                        ColorChip chip = chips[j];
                        chip.transform.localScale *= 0f; 
                        chip.enabled = false;
                        chip.Button.enabled = false;
                        chip.Button.OnClick.RemoveAllListeners();
                    }
                }
            }
            [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadPlayerPrefs))]
            private static class LoadPlayerPrefsPatch { // Fix Potential issues with broken colors
                private static bool needsPatch = false;
                public static void Prefix([HarmonyArgument(0)] bool overrideLoad) {
                    if (!SaveManager.loaded || overrideLoad)
                        needsPatch = true;
                }
                public static void Postfix() {
                    if (!needsPatch) return;
                    SaveManager.colorConfig %= CustomColors.pickableColors;
                    needsPatch = false;
                }
            }
            [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor))]
            private static class PlayerControlCheckColorPatch {
                private static bool isTaken(PlayerControl player, uint color) {
                    foreach (GameData.PlayerInfo p in GameData.Instance.AllPlayers)
                        if (!p.Disconnected && p.PlayerId != player.PlayerId && p.ColorId == color)
                            return true;
                    return false;
                }
                public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte bodyColor) { // Fix incorrect color assignment
                    uint color = (uint)bodyColor;
                   if (isTaken(__instance, color) || color >= Palette.PlayerColors.Length) {
                        int num = 0;
                        while (num++ < 50 && (color >= CustomColors.pickableColors || isTaken(__instance, color))) {
                            color = (color + 1) % CustomColors.pickableColors;
                        }
                    }
                    __instance.RpcSetColor((byte)color);
                    return false;
                }
            }
        }
    }
}
