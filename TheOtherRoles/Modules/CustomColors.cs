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
        private static readonly List<int> ORDER = new List<int>() { 7, 37, 14, 5, 33, 41, 25,
                                                                    4, 30, 0, 35, 3, 27, 17,
                                                                    13, 23, 8, 32, 38, 1, 21,
                                                                    40, 31, 10, 34, 22, 28, 36,
                                                                    2, 11, 26, 29, 20, 19, 18,
                                                                    12, 9, 24, 16, 15, 6, 39,
                                                                    };
        public static void Load() {
            List<StringNames> longlist = Enumerable.ToList<StringNames>(Palette.ColorNames);
            List<Color32> colorlist = Enumerable.ToList<Color32>(Palette.PlayerColors);
            List<Color32> shadowlist = Enumerable.ToList<Color32>(Palette.ShadowColors);

            List<CustomColor> colors = new List<CustomColor>();

            /* Custom Colors, starting with id (for ORDER) 18 */
            colors.Add(new CustomColor {
                longname = "Tamarind", //18
                color = new Color32(48, 28, 34, byte.MaxValue),
                shadow = new Color32(30, 11, 16, byte.MaxValue),
                isLighterColor = true });
            colors.Add(new CustomColor {
                longname = "Army", // 19
                color = new Color32(39, 45, 31, byte.MaxValue),
                shadow = new Color32(11, 30, 24, byte.MaxValue),
                isLighterColor = false });
            // 20
            colors.Add(new CustomColor { longname = "Olive",
                                        color = new Color32(154, 140, 61, byte.MaxValue), 
                                        shadow = new Color32(104, 95, 40, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Turquoise",
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
            colors.Add(new CustomColor { longname = "Nougat",
                                        color = new Color32(160, 101, 56, byte.MaxValue), 
                                        shadow = new Color32(115, 15, 78, byte.MaxValue),
                                        isLighterColor = false });
            // 25
            colors.Add(new CustomColor { longname = "Peach",
                                        color = new Color32(255, 164, 119, byte.MaxValue), 
                                        shadow = new Color32(238, 128, 100, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Wasabi",
                                        color = new Color32(112, 143, 46, byte.MaxValue), 
                                        shadow = new Color32(72, 92, 29, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Hot Pink",
                                        color = new Color32(255, 51, 102, byte.MaxValue), 
                                        shadow = new Color32(232, 0, 58, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Petrol", 
                                        color = new Color32(0, 99, 105, byte.MaxValue), 
                                        shadow = new Color32(0, 61, 54, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Lemon",
                                        color = new Color32(0xDB, 0xFD, 0x2F, byte.MaxValue), 
                                        shadow = new Color32(0x74, 0xE5, 0x10, byte.MaxValue), 
                                        isLighterColor = true });
            // 30
            colors.Add(new CustomColor { longname = "Signal\nOrange",
                                        color = new Color32(0xF7, 0x44, 0x17, byte.MaxValue), 
                                        shadow = new Color32(0x9B, 0x2E, 0x0F, byte.MaxValue),
                                        isLighterColor = true });   
            colors.Add(new CustomColor { longname = "Teal",
                                        color = new Color32(0x25, 0xB8, 0xBF, byte.MaxValue), 
                                        shadow = new Color32(0x12, 0x89, 0x86, byte.MaxValue),
                                        isLighterColor = true });   

            colors.Add(new CustomColor { longname = "Blurple",
                                        color = new Color32(61, 44, 142, byte.MaxValue), 
                                        shadow = new Color32(25, 14, 90, byte.MaxValue),
                                        isLighterColor = false });   

            colors.Add(new CustomColor { longname = "Sunrise", 
                                        color = new Color32(0xFF, 0xCA, 0x19, byte.MaxValue), 
                                        shadow = new Color32(0xDB, 0x44, 0x42, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Ice",
                                        color = new Color32(0xA8, 0xDF, 0xFF, byte.MaxValue), 
                                        shadow = new Color32(0x59, 0x9F, 0xC8, byte.MaxValue),
                                        isLighterColor = true });
            // 35
            colors.Add(new CustomColor { longname = "Fuchsia", //35 Color Credit: LaikosVK
                                        color = new Color32(164, 17, 129, byte.MaxValue),
                                        shadow = new Color32(104, 3, 79, byte.MaxValue),
                                        isLighterColor = false
            });
            colors.Add(new CustomColor { longname = "Royal\nGreen", //36
                                        color = new Color32(9, 82, 33, byte.MaxValue),
                                        shadow = new Color32(0, 46, 8, byte.MaxValue),
                                        isLighterColor = false
            });
            colors.Add(new CustomColor { longname = "Slime",
                                        color = new Color32(244, 255, 188, byte.MaxValue),
                                        shadow = new Color32(167, 239, 112, byte.MaxValue),
                                        isLighterColor = false
            });
            colors.Add(new CustomColor { longname = "Navy", //38
                                        color = new Color32(9, 43, 119, byte.MaxValue),
                                        shadow = new Color32(0, 13, 56, byte.MaxValue),
                                        isLighterColor = false
            });
            colors.Add(new CustomColor { longname = "Darkness", //39
                                        color = new Color32(36, 39, 40, byte.MaxValue),
                                        shadow = new Color32(10, 10, 10, byte.MaxValue),
                                        isLighterColor = false
            });
            colors.Add(new CustomColor {
                longname = "Ocean", //40
                color = new Color32(55, 159, 218, byte.MaxValue),
                shadow = new Color32(62, 92, 158, byte.MaxValue),
                isLighterColor = false
            });
            colors.Add(new CustomColor {
                longname = "Sundown", // 41
                color = new Color32(252, 194, 100, byte.MaxValue),
                shadow = new Color32(197, 98, 54, byte.MaxValue),
                isLighterColor = false
            });
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

            [HarmonyPatch(typeof(ChatNotification), nameof(ChatNotification.SetUp))]
            private class ChatNotificationColorsPatch {
                public static bool Prefix(ChatNotification __instance, PlayerControl sender, string text) {
                    if (ShipStatus.Instance && !TORMapOptions.ShowChatNotifications) {
                        return false;
                    }
                    __instance.timeOnScreen = 5f;
                    __instance.gameObject.SetActive(true);
                    __instance.SetCosmetics(sender.Data);
                    string str;
                    Color color;
                    try {
                        str = ColorUtility.ToHtmlStringRGB(Palette.TextColors[__instance.player.ColorId]);
                        color = Palette.TextOutlineColors[__instance.player.ColorId];
                    } catch {
                        Color32 c = Palette.PlayerColors[__instance.player.ColorId];
                        str = ColorUtility.ToHtmlStringRGB(c);
                        
                        color = c.r + c.g + c.b  > 180 ? Palette.Black : Palette.White;
                    }
                    __instance.playerColorText.text = __instance.player.ColorBlindName;
                    __instance.playerNameText.text = "<color=#" + str + ">" + (string.IsNullOrEmpty(sender.Data.PlayerName) ? "..." : sender.Data.PlayerName);
                    __instance.playerNameText.outlineColor = color;
                    __instance.chatText.text = text;
                    return false;
                }
            }

            [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
            private static class PlayerTabEnablePatch {
                public static void Postfix(PlayerTab __instance) { // Replace instead
                    Il2CppArrayBase<ColorChip> chips = __instance.ColorChips.ToArray();

                    int cols = 7; // TODO: Design an algorithm to dynamically position chips to optimally fill space
                    for (int i = 0; i < ORDER.Count; i++) {
                        int pos = ORDER[i];
                        if (pos < 0 || pos > chips.Length)
                            continue;
                        ColorChip chip = chips[pos];
                        int row = i / cols, col = i % cols; // Dynamically do the positioning
                        chip.transform.localPosition = new Vector3(-0.975f + (col * 0.5f), 1.475f - (row * 0.5f), chip.transform.localPosition.z);
                        chip.transform.localScale *= 0.76f;
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
                    foreach (NetworkedPlayerInfo p in GameData.Instance.AllPlayers.GetFastEnumerator())
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