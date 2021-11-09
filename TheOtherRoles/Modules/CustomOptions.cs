using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using System.Reflection;
using System.Text;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles {
    public class CustomOption {
        public static List<CustomOption> options = new List<CustomOption>();
        public static int preset = 0;

        public int id;
        public string name;
        public System.Object[] selections;

        public int defaultSelection;
        public ConfigEntry<int> entry;
        public int selection;
        public OptionBehaviour optionBehaviour;
        public CustomOption parent;
        public bool isHeader;

        // Option creation

        public CustomOption(int id, string name,  System.Object[] selections, System.Object defaultValue, CustomOption parent, bool isHeader) {
            this.id = id;
            this.name = parent == null ? name : "- " + name;
            this.selections = selections;
            int index = Array.IndexOf(selections, defaultValue);
            this.defaultSelection = index >= 0 ? index : 0;
            this.parent = parent;
            this.isHeader = isHeader;
            selection = 0;
            if (id != 0) {
                entry = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", id.ToString(), defaultSelection);
                selection = Mathf.Clamp(entry.Value, 0, selections.Length - 1);
            }
            options.Add(this);
        }

        public static CustomOption Create(int id, string name, string[] selections, CustomOption parent = null, bool isHeader = false) {
            return new CustomOption(id, name, selections, "", parent, isHeader);
        }

        public static CustomOption Create(int id, string name, float defaultValue, float min, float max, float step, CustomOption parent = null, bool isHeader = false) {
            List<float> selections = new List<float>();
            for (float s = min; s <= max; s += step)
                selections.Add(s);
            return new CustomOption(id, name, selections.Cast<object>().ToArray(), defaultValue, parent, isHeader);
        }

        public static CustomOption Create(int id, string name, bool defaultValue, CustomOption parent = null, bool isHeader = false) {
            return new CustomOption(id, name, new string[]{"Off", "On"}, defaultValue ? "On" : "Off", parent, isHeader);
        }

        // Static behaviour

        public static void switchPreset(int newPreset) {
            CustomOption.preset = newPreset;
            foreach (CustomOption option in CustomOption.options) {
                if (option.id == 0) continue;

                option.entry = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", option.id.ToString(), option.defaultSelection);
                option.selection = Mathf.Clamp(option.entry.Value, 0, option.selections.Length - 1);
                if (option.optionBehaviour != null && option.optionBehaviour is StringOption stringOption) {
                    stringOption.oldValue = stringOption.Value = option.selection;
                    stringOption.ValueText.text = option.selections[option.selection].ToString();
                }
            }
        }

        public static void ShareOptionSelections() {
            if (PlayerControl.AllPlayerControls.Count <= 1 || AmongUsClient.Instance?.AmHost == false && PlayerControl.LocalPlayer == null) return;
            foreach (CustomOption option in CustomOption.options) {
                MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareOptionSelection, Hazel.SendOption.Reliable);
                messageWriter.WritePacked((uint)option.id);
                messageWriter.WritePacked((uint)Convert.ToUInt32(option.selection));
                messageWriter.EndMessage();
            }
        }

        // Getter

        public int getSelection() {
            return selection;
        }

        public bool getBool() {
            return selection > 0;
        }

        public float getFloat() {
            return (float)selections[selection];
        }

        // Option changes

        public void updateSelection(int newSelection) {
            selection = Mathf.Clamp((newSelection + selections.Length) % selections.Length, 0, selections.Length - 1);
            if (optionBehaviour != null && optionBehaviour is StringOption stringOption) {
                stringOption.oldValue = stringOption.Value = selection;
                stringOption.ValueText.text = selections[selection].ToString();

                if (AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer) {
                    if (id == 0) switchPreset(selection); // Switch presets
                    else if (entry != null) entry.Value = selection; // Save selection to config

                    ShareOptionSelections();// Share all selections
                }
           }
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    class GameOptionsMenuStartPatch {
        public static void Postfix(GameOptionsMenu __instance) {
            var template = UnityEngine.Object.FindObjectsOfType<StringOption>().FirstOrDefault();
            if (template == null) return;

            List<OptionBehaviour> allOptions = __instance.Children.ToList();
            for (int i = 0; i < CustomOption.options.Count; i++) {
                CustomOption option = CustomOption.options[i];
                if (option.optionBehaviour == null) {
                    StringOption stringOption = UnityEngine.Object.Instantiate(template, template.transform.parent);
                    allOptions.Add(stringOption);

                    stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => {});
                    stringOption.TitleText.text = option.name;
                    stringOption.Value = stringOption.oldValue = option.selection;
                    stringOption.ValueText.text = option.selections[option.selection].ToString();

                    option.optionBehaviour = stringOption;
                }
                option.optionBehaviour.gameObject.SetActive(true);
            }
            
            var commonTasksOption = allOptions.FirstOrDefault(x => x.name == "NumCommonTasks").TryCast<NumberOption>();
            if(commonTasksOption != null) commonTasksOption.ValidRange = new FloatRange(0f, 4f);

            var shortTasksOption = allOptions.FirstOrDefault(x => x.name == "NumShortTasks").TryCast<NumberOption>();
            if(shortTasksOption != null) shortTasksOption.ValidRange = new FloatRange(0f, 23f);

            var longTasksOption = allOptions.FirstOrDefault(x => x.name == "NumLongTasks").TryCast<NumberOption>();
            if(longTasksOption != null) longTasksOption.ValidRange = new FloatRange(0f, 15f);
            
            __instance.Children = allOptions.ToArray();
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
    public class StringOptionEnablePatch {
        public static bool Prefix(StringOption __instance) {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;

            __instance.OnValueChanged = new Action<OptionBehaviour>((o) => {});
            __instance.TitleText.text = option.name;
            __instance.Value = __instance.oldValue = option.selection;
            __instance.ValueText.text = option.selections[option.selection].ToString();
            
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
    public class StringOptionIncreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;
            option.updateSelection(option.selection + 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
    public class StringOptionDecreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;
            option.updateSelection(option.selection - 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
    public class RpcSyncSettingsPatch
    {
        public static void Postfix()
        {
            CustomOption.ShareOptionSelections();
        }
    }


    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    class GameOptionsMenuUpdatePatch
    {
        private static float timer = 1f;
        public static void Postfix(GameOptionsMenu __instance) {
            __instance.GetComponentInParent<Scroller>().YBounds.max = -0.5F + __instance.Children.Length * 0.55F; 
            timer += Time.deltaTime;
            if (timer < 0.1f) return;
            timer = 0f;

            float offset = -7.85f;
            foreach (CustomOption option in CustomOption.options) {
                if (option?.optionBehaviour != null && option.optionBehaviour.gameObject != null) {
                    bool enabled = true;
                    var parent = option.parent;
                    while (parent != null && enabled) {
                        enabled = parent.selection != 0;
                        parent = parent.parent;
                    }
                    option.optionBehaviour.gameObject.SetActive(enabled);
                    if (enabled) {
                        offset -= option.isHeader ? 0.75f : 0.5f;
                        option.optionBehaviour.transform.localPosition = new Vector3(option.optionBehaviour.transform.localPosition.x, offset, option.optionBehaviour.transform.localPosition.z);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameSettingMenu), "OnEnable")]
    class GameSettingMenuPatch {
        public static void Prefix(GameSettingMenu __instance) {
            __instance.HideForOnline = new Transform[]{};
        }

        public static void Postfix(GameSettingMenu __instance) {
            var mapNameTransform = __instance.AllItems.FirstOrDefault(x => x.gameObject.activeSelf && x.name.Equals("MapName", StringComparison.OrdinalIgnoreCase));
            if (mapNameTransform == null) return;

            var options = new Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.KeyValuePair<string, int>>();
            for (int i = 0; i < GameOptionsData.MapNames.Length; i++) {
                var kvp = new Il2CppSystem.Collections.Generic.KeyValuePair<string, int>();
                kvp.key = GameOptionsData.MapNames[i];
                kvp.value = i;
                options.Add(kvp);
            }
            mapNameTransform.GetComponent<KeyValueOption>().Values = options;
        }
    }

    [HarmonyPatch(typeof(Constants), nameof(Constants.ShouldFlipSkeld))]
    class ConstantsShouldFlipSkeldPatch {
        public static bool Prefix(ref bool __result) {
            if (PlayerControl.GameOptions == null) return true;
            __result = PlayerControl.GameOptions.MapId == 3;
            return false;
        }
    }

    [HarmonyPatch] 
    class GameOptionsDataPatch
    {
        private static IEnumerable<MethodBase> TargetMethods() {
            return typeof(GameOptionsData).GetMethods().Where(x => x.ReturnType == typeof(string) && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(int));
        }

        private static void Postfix(ref string __result)
        {
            StringBuilder sb = new StringBuilder(__result);
            foreach (CustomOption option in CustomOption.options) {
                if (option.parent == null) {
                    if (option == CustomOptionHolder.crewmateRolesCountMin) {
                        var optionName = CustomOptionHolder.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Crewmate Roles");
                        var min = CustomOptionHolder.crewmateRolesCountMin.getSelection();
                        var max = CustomOptionHolder.crewmateRolesCountMax.getSelection();
                        if (min > max) min = max;
                        var optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
                        sb.AppendLine($"{optionName}: {optionValue}");
                    } else if (option == CustomOptionHolder.neutralRolesCountMin) {
                        var optionName = CustomOptionHolder.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Neutral Roles");
                        var min = CustomOptionHolder.neutralRolesCountMin.getSelection();
                        var max = CustomOptionHolder.neutralRolesCountMax.getSelection();
                        if (min > max) min = max;
                        var optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
                        sb.AppendLine($"{optionName}: {optionValue}");
                    } else if (option == CustomOptionHolder.impostorRolesCountMin) {
                        var optionName = CustomOptionHolder.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Impostor Roles");
                        var min = CustomOptionHolder.impostorRolesCountMin.getSelection();
                        var max = CustomOptionHolder.impostorRolesCountMax.getSelection();
                        if (min > max) min = max;
                        var optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
                        sb.AppendLine($"{optionName}: {optionValue}");
                    } else if ((option == CustomOptionHolder.crewmateRolesCountMax) || (option == CustomOptionHolder.neutralRolesCountMax) || (option == CustomOptionHolder.impostorRolesCountMax)) {
                        continue;
                    } else {
                        sb.AppendLine($"{option.name}: {option.selections[option.selection].ToString()}");
                    }
                    
                }
            }
            CustomOption parent = null;
            foreach (CustomOption option in CustomOption.options)
                if (option.parent != null) {
                    if (option.parent != parent) {
                        sb.AppendLine();
                        parent = option.parent;
                    }
                    sb.AppendLine($"{option.name}: {option.selections[option.selection].ToString()}");
                }

            var hudString = sb.ToString();

            int defaultSettingsLines = 19;
            int roleSettingsLines = defaultSettingsLines + 37;
            int detailedSettingsP1 = roleSettingsLines + 37;
            int detailedSettingsP2 = detailedSettingsP1 + 38;
            int end1 = hudString.TakeWhile(c => (defaultSettingsLines -= (c == '\n' ? 1 : 0)) > 0).Count();
            int end2 = hudString.TakeWhile(c => (roleSettingsLines -= (c == '\n' ? 1 : 0)) > 0).Count();
            int end3 = hudString.TakeWhile(c => (detailedSettingsP1 -= (c == '\n' ? 1 : 0)) > 0).Count();
            int end4 = hudString.TakeWhile(c => (detailedSettingsP2 -= (c == '\n' ? 1 : 0)) > 0).Count();
            int counter = TheOtherRolesPlugin.optionsPage;
            if (counter == 0) {
                hudString = hudString.Substring(0, end1) + "\n";   
            } else if (counter == 1) {
                hudString = hudString.Substring(end1 + 1, end2 - end1);
                // Temporary fix, should add a new CustomOption for spaces
                int gap = 1;
                int index = hudString.TakeWhile(c => (gap -= (c == '\n' ? 1 : 0)) > 0).Count();
                hudString = hudString.Insert(index, "\n");
                gap = 5;
                index = hudString.TakeWhile(c => (gap -= (c == '\n' ? 1 : 0)) > 0).Count();
                hudString = hudString.Insert(index, "\n");
                gap = 18;
                index = hudString.TakeWhile(c => (gap -= (c == '\n' ? 1 : 0)) > 0).Count();
                hudString = hudString.Insert(index + 1, "\n");
                gap = 23;
                index = hudString.TakeWhile(c => (gap -= (c == '\n' ? 1 : 0)) > 0).Count();
                hudString = hudString.Insert(index + 1, "\n");
            } else if (counter == 2) {
                hudString = hudString.Substring(end2 + 1, end3 - end2);
            } else if (counter == 3) {
                hudString = hudString.Substring(end3 + 1, end4 - end3);
            } else if (counter == 4) {
                hudString = hudString.Substring(end4 + 1);
            }

            hudString += $"\n Press tab for more... ({counter+1}/5)";
            __result = hudString;
        }
    }

    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class GameOptionsNextPagePatch
    {
        public static void Postfix(KeyboardJoystick __instance)
        {
            if(Input.GetKeyDown(KeyCode.Tab)) {
                TheOtherRolesPlugin.optionsPage = (TheOtherRolesPlugin.optionsPage + 1) % 5;
            }
        }
    }

    
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class GameSettingsScalePatch {
        public static void Prefix(HudManager __instance) {
            if (__instance.GameSettings != null) __instance.GameSettings.fontSize = 1.2f; 
        }
    }
}