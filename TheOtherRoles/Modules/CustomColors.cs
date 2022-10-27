using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using AmongUs.Data.Legacy;
using TheOtherRoles.Utilities;

namespace TheOtherRoles.Modules {
    public class CustomColors {
        protected static Dictionary<int, string> ColorStrings = new Dictionary<int, string>();
        public static List<int> lighterColors = new List<int>(){ 3, 4, 5, 7, 10, 11, 13, 14, 17 };
        public static uint pickableColors = (uint)Palette.ColorNames.Length;

        private static readonly List<int> ORDER = new List<int>() { 7, 14, 5, 33, 4, 
                                                                    30, 0, 19, 27, 3,
                                                                    17, 25, 18, 13, 23,
                                                                    8, 32, 1, 21, 31,
                                                                    10, 34, 15, 28, 22,
                                                                    29, 11, 2, 26, 16,
                                                                    20, 24, 9, 12, 6 };
        public static void Load() {
            List<StringNames> longlist = Enumerable.ToList<StringNames>(Palette.ColorNames);
            List<Color32> colorlist = Enumerable.ToList<Color32>(Palette.PlayerColors);
            List<Color32> shadowlist = Enumerable.ToList<Color32>(Palette.ShadowColors);

            List<CustomColor> colors = new List<CustomColor>();

            /* Custom Colors */
            colors.Add(new CustomColor { longname = "PuThorple",
                                        color = new Color32(150, 2, 242, byte.MaxValue), // shadow = new Color32(0xA5, 0x63, 0x65, byte.MaxValue),
                                        shadow = new Color32(17, 6, 38, byte.MaxValue), // color = new Color32(0xD8, 0x82, 0x83, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Bordeaux",
                                        color = new Color32(109, 7, 26, byte.MaxValue), 
                                        shadow = new Color32(54, 2, 11, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "ParaGreen",
                                        color = new Color32(50, 205, 50, byte.MaxValue), 
                                        shadow = new Color32(42, 245, 154, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Turqoise",
                                        color = new Color32(22, 132, 176, byte.MaxValue), 
                                        shadow = new Color32(15, 89, 117, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Mint", 
                                        color = new Color32(111, 192, 156, byte.MaxValue), 
                                        shadow = new Color32(65, 148, 111, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Lavender",
                                        color = new Color32(173, 126, 201, byte.MaxValue), 
                                        shadow = new Color32(131, 58, 203, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Stitch",
                                        color = new Color32(51, 136, 255, byte.MaxValue), 
                                        shadow = new Color32(253, 93, 168, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "LuLuBlue", // 25
                                        color = new Color32(148, 190, 224, byte.MaxValue), 
                                        shadow = new Color32(100, 135, 163, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "IrishPat", // 26
                                        color = new Color32(0, 158, 95, byte.MaxValue), 
                                        shadow = new Color32(1, 120, 72, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "LilyBan", //27
                                        color = new Color32(94, 203, 200, byte.MaxValue),
                                        shadow = new Color32(182, 109, 255, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "DuckGreen",  // 28
                                        color = new Color32(35, 107, 56, byte.MaxValue), 
                                        shadow = new Color32(26, 28, 27, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "NorthOrange", // 29
                                        color = new Color32(255, 185, 0, byte.MaxValue),
                                        shadow = new Color32(255, 175, 0, byte.MaxValue),
                                        isLighterColor = true });

	    // NEW COLORS START ON ID 30!!!!
            colors.Add(new CustomColor { longname = "Panda", //30
			                 color = new Color32(0xE5, 0xE5, 0xE5, byte.MaxValue),
			                 shadow = new Color32(0x0C, 0x0C, 0x0C, byte.MaxValue),
			                 isLighterColor = true });

            colors.Add(new CustomColor { longname = "True dark", //31
 			                 color = new Color32(0x00, 0x00, 0x00, byte.MaxValue),
  			                 shadow = new Color32(0x13, 0x13, 0x13, byte.MaxValue),
			                 isLighterColor = false });

            colors.Add(new CustomColor { longname = "LJBlue", //32
 			                 color = new Color32(48, 213, 200, byte.MaxValue),
  			                 shadow = new Color32(0, 0, 139, byte.MaxValue),
			                 isLighterColor = true });

            colors.Add(new CustomColor { longname = "ScoomRed", //33
 			                 color = new Color32(59, 5, 5, byte.MaxValue),
  			                 shadow = new Color32(0, 0, 0, byte.MaxValue),
			                 isLighterColor = false });

            colors.Add(new CustomColor { longname = "SvettyBlue", //34
 			                 color = new Color32(19, 54, 82, byte.MaxValue),
  			                 shadow = new Color32(9, 34, 56, byte.MaxValue),
			                 isLighterColor = false });



            pickableColors += (uint)colors.Count; // Colors to show in Tab
            /** Hidden Colors **/     
                    
            /** Add Colors **/
            int id = 50000;
            foreach (CustomColor cc in colors) {
                longlist.Add((StringNames)id);
                CustomColors.ColorStrings[id++] = cc.longname;
                colorlist.Add(cc.color);
                shadowlist.Add(cc.shadow);
                if (cc.isLighterColor)
                    lighterColors.Add(colorlist.Count - 1);
            }

            Palette.ColorNames = longlist.ToArray();
            Palette.PlayerColors = colorlist.ToArray();
            Palette.ShadowColors = shadowlist.ToArray();
        }

        protected internal struct CustomColor {
            public string longname;
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
                [HarmonyPriority(Priority.Last)]
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
                        chip.transform.localPosition = new Vector3(-0.975f + (col * 0.485f), 1.475f - (row * 0.49f), chip.transform.localPosition.z);
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
            [HarmonyPatch(typeof(LegacySaveManager), nameof(LegacySaveManager.LoadPlayerPrefs))]
            private static class LoadPlayerPrefsPatch { // Fix Potential issues with broken colors
                private static bool needsPatch = false;
                public static void Prefix([HarmonyArgument(0)] bool overrideLoad) {
                    if (!LegacySaveManager.loaded || overrideLoad)
                        needsPatch = true;
                }
                public static void Postfix() {
                    if (!needsPatch) return;
                    LegacySaveManager.colorConfig %= CustomColors.pickableColors;
                    needsPatch = false;
                }
            }
            [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor))]
            private static class PlayerControlCheckColorPatch {
                private static bool isTaken(PlayerControl player, uint color) {
                    foreach (GameData.PlayerInfo p in GameData.Instance.AllPlayers.GetFastEnumerator())
                        if (!p.Disconnected && p.PlayerId != player.PlayerId && p.DefaultOutfit.ColorId == color)
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
