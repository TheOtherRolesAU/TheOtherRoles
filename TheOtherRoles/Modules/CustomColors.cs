using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnhollowerBaseLib;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace TheOtherRoles.Modules
{
    public static class CustomColors
    {
        private static readonly Dictionary<int, string> ColorStrings = new();
        public static readonly List<int> LighterColors = new() {3, 4, 5, 7, 10, 11, 13, 14, 17};
        private static uint pickableColors = (uint) Palette.ColorNames.Length;

        /* version 1
        private static readonly List<int> ORDER = new List<int>() { 7, 17, 5, 33, 4, 
                                                                    30, 0, 19, 27, 3,
                                                                    13, 25, 18, 15, 23,
                                                                    8, 32, 1, 21, 31,
                                                                    10, 34, 12, 14, 28,
                                                                    22, 29, 11, 26, 2,
                                                                    20, 24, 9, 16, 6 }; */
        private static readonly List<int> ORDER = new()
        {
            7, 14, 5, 33, 4,
            30, 0, 19, 27, 3,
            17, 25, 18, 13, 23,
            8, 32, 1, 21, 31,
            10, 34, 15, 28, 22,
            29, 11, 2, 26, 16,
            20, 24, 9, 12, 6
        };

        public static void Load()
        {
            var longList = Palette.ColorNames.ToList();
            var colorList = Palette.PlayerColors.ToList();
            var shadowList = Palette.ShadowColors.ToList();

            var colors = new List<CustomColor>
            {
                new()
                {
                    longName = "Salmon",
                    color = new Color32(239, 191, 192,
                        byte.MaxValue), // color = new Color32(0xD8, 0x82, 0x83, byte.MaxValue),
                    shadow = new Color32(182, 119, 114,
                        byte.MaxValue), // shadow = new Color32(0xA5, 0x63, 0x65, byte.MaxValue),
                    isLighterColor = true
                },
                new()
                {
                    longName = "Bordeaux",
                    color = new Color32(109, 7, 26, byte.MaxValue),
                    shadow = new Color32(54, 2, 11, byte.MaxValue),
                    isLighterColor = false
                },
                new()
                {
                    longName = "Olive",
                    color = new Color32(154, 140, 61, byte.MaxValue),
                    shadow = new Color32(104, 95, 40, byte.MaxValue),
                    isLighterColor = false
                },
                new()
                {
                    longName = "Turquoise",
                    color = new Color32(22, 132, 176, byte.MaxValue),
                    shadow = new Color32(15, 89, 117, byte.MaxValue),
                    isLighterColor = false
                },
                new()
                {
                    longName = "Mint",
                    color = new Color32(111, 192, 156, byte.MaxValue),
                    shadow = new Color32(65, 148, 111, byte.MaxValue),
                    isLighterColor = true
                },
                new()
                {
                    longName = "Lavender",
                    color = new Color32(173, 126, 201, byte.MaxValue),
                    shadow = new Color32(131, 58, 203, byte.MaxValue),
                    isLighterColor = true
                },
                new()
                {
                    longName = "Nougat",
                    color = new Color32(160, 101, 56, byte.MaxValue),
                    shadow = new Color32(115, 15, 78, byte.MaxValue),
                    isLighterColor = false
                },
                new()
                {
                    longName = "Peach",
                    color = new Color32(255, 164, 119, byte.MaxValue),
                    shadow = new Color32(238, 128, 100, byte.MaxValue),
                    isLighterColor = true
                },
                new()
                {
                    longName = "Wasabi",
                    color = new Color32(112, 143, 46, byte.MaxValue),
                    shadow = new Color32(72, 92, 29, byte.MaxValue),
                    isLighterColor = false
                },
                new()
                {
                    longName = "Hot Pink",
                    color = new Color32(255, 51, 102, byte.MaxValue),
                    shadow = new Color32(232, 0, 58, byte.MaxValue),
                    isLighterColor = true
                },
                new()
                {
                    longName = "Petrol",
                    color = new Color32(0, 99, 105, byte.MaxValue),
                    shadow = new Color32(0, 61, 54, byte.MaxValue),
                    isLighterColor = false
                },
                new()
                {
                    longName = "Lemon",
                    color = new Color32(0xDB, 0xFD, 0x2F, byte.MaxValue),
                    shadow = new Color32(0x74, 0xE5, 0x10, byte.MaxValue),
                    isLighterColor = true
                },
                new()
                {
                    longName = "Signal Orange",
                    color = new Color32(0xF7, 0x44, 0x17, byte.MaxValue),
                    shadow = new Color32(0x9B, 0x2E, 0x0F, byte.MaxValue),
                    isLighterColor = true
                },
                new()
                {
                    longName = "Teal",
                    color = new Color32(0x25, 0xB8, 0xBF, byte.MaxValue),
                    shadow = new Color32(0x12, 0x89, 0x86, byte.MaxValue),
                    isLighterColor = false
                },
                new()
                {
                    longName = "Blurple",
                    color = new Color32(0x59, 0x3C, 0xD6, byte.MaxValue),
                    shadow = new Color32(0x29, 0x17, 0x96, byte.MaxValue),
                    isLighterColor = false
                },
                new()
                {
                    longName = "Sunrise",
                    color = new Color32(0xFF, 0xCA, 0x19, byte.MaxValue),
                    shadow = new Color32(0xDB, 0x44, 0x42, byte.MaxValue),
                    isLighterColor = true
                },
                new()
                {
                    longName = "Ice",
                    color = new Color32(0xA8, 0xDF, 0xFF, byte.MaxValue),
                    shadow = new Color32(0x59, 0x9F, 0xC8, byte.MaxValue),
                    isLighterColor = true
                }
            };

            /* Custom Colors */


            pickableColors += (uint) colors.Count; // Colors to show in Tab
            /* Hidden Colors */

            /* Add Colors */
            var id = 50000;
            foreach (var cc in colors)
            {
                longList.Add((StringNames) id);
                ColorStrings[id++] = cc.longName;
                colorList.Add(cc.color);
                shadowList.Add(cc.shadow);
                if (cc.isLighterColor)
                    LighterColors.Add(colorList.Count - 1);
            }

            Palette.ColorNames = longList.ToArray();
            Palette.PlayerColors = colorList.ToArray();
            Palette.ShadowColors = shadowList.ToArray();
        }

        private struct CustomColor
        {
            public string longName;
            public Color32 color;
            public Color32 shadow;
            public bool isLighterColor;
        }

        [HarmonyPatch]
        public static class CustomColorPatches
        {
            [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), typeof(StringNames),
                typeof(Il2CppReferenceArray<Object>))]
            private class ColorStringPatch
            {
                public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name)
                {
                    if ((int) name < 50000) return true;
                    var text = ColorStrings[(int) name];
                    if (text == null) return true;
                    __result = text;
                    return false;
                }
            }

            [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
            private static class PlayerTabEnablePatch
            {
                public static void Postfix(PlayerTab __instance)
                {
                    // Replace instead
                    var chips = __instance.ColorChips.ToArray();

                    const int
                        cols = 5; // TODO: Design an algorithm to dynamically position chips to optimally fill space
                    for (var i = 0; i < ORDER.Count; i++)
                    {
                        var pos = ORDER[i];
                        if (pos < 0 || pos > chips.Length)
                            continue;
                        var chip = chips[pos];
                        int row = i / cols, col = i % cols; // Dynamically do the positioning
                        var chipPosition = chip.transform.localPosition;
                        chipPosition = new Vector3(-0.975f + col * 0.485f, 1.475f - row * 0.49f,
                            chipPosition.z);
                        chip.transform.localPosition = chipPosition;
                        chip.transform.localScale *= 0.78f;
                    }

                    for (var j = ORDER.Count; j < chips.Length; j++)
                    {
                        // If number isn't in order, hide it
                        var chip = chips[j];
                        chip.transform.localScale *= 0f;
                        chip.enabled = false;
                        chip.Button.enabled = false;
                        chip.Button.OnClick.RemoveAllListeners();
                    }
                }
            }

            [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadPlayerPrefs))]
            private static class LoadPlayerPrefsPatch
            {
                // Fix Potential issues with broken colors
                private static bool needsPatch;

                public static void Prefix([HarmonyArgument(0)] bool overrideLoad)
                {
                    if (!SaveManager.loaded || overrideLoad)
                        needsPatch = true;
                }

                public static void Postfix()
                {
                    if (!needsPatch) return;
                    SaveManager.colorConfig %= pickableColors;
                    needsPatch = false;
                }
            }

            [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor))]
            private static class PlayerControlCheckColorPatch
            {
                private static bool IsTaken(PlayerControl player, uint color)
                {
                    foreach (var p in GameData.Instance.AllPlayers)
                        if (!p.Disconnected && p.PlayerId != player.PlayerId && p.ColorId == color)
                            return true;
                    return false;
                }

                public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte bodyColor)
                {
                    // Fix incorrect color assignment
                    uint color = bodyColor;
                    if (IsTaken(__instance, color) || color >= Palette.PlayerColors.Length)
                    {
                        var num = 0;
                        while (num++ < 50 && (color >= pickableColors || IsTaken(__instance, color)))
                            color = (color + 1) % pickableColors;
                    }

                    __instance.RpcSetColor((byte) color);
                    return false;
                }
            }
        }
    }
}