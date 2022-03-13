using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using System.Reflection;
using System.Text;
using static TheEpicRoles.TheEpicRoles;

namespace TheEpicRoles {
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
        public string type;

        // Option creation

        public CustomOption(int id, string name,  System.Object[] selections, System.Object defaultValue, CustomOption parent, bool isHeader, String type) {
            this.id = id;
            this.name = parent == null ? name : "- " + name;
            this.selections = selections;
            int index = Array.IndexOf(selections, defaultValue);
            this.defaultSelection = index >= 0 ? index : 0;
            this.parent = parent;
            this.isHeader = isHeader;
            this.type = type;
            selection = 0;
            if (id != 0) {
                entry = TheEpicRolesPlugin.Instance.Config.Bind($"Preset{preset}", id.ToString(), defaultSelection);
                selection = Mathf.Clamp(entry.Value, 0, selections.Length - 1);
            }
            options.Add(this);
        }

        public static CustomOption Create(int id, string name, String type, string[] selections, CustomOption parent = null, bool isHeader = false) {
            return new CustomOption(id, name, selections, "", parent, isHeader, type);
        }

        public static CustomOption Create(int id, string name, String type, float defaultValue, float min, float max, float step, CustomOption parent = null, bool isHeader = false) {
            List<float> selections = new List<float>();
            for (float s = min; s <= max; s += step)
                selections.Add(s);
            return new CustomOption(id, name, selections.Cast<object>().ToArray(), defaultValue, parent, isHeader, type);
        }

        public static CustomOption Create(int id, string name, String type, bool defaultValue, CustomOption parent = null, bool isHeader = false) {
            return new CustomOption(id, name, new string[]{"Off", "On"}, defaultValue ? "On" : "Off", parent, isHeader, type);
        }

        // Static behaviour

        public static void switchPreset(int newPreset) {
            CustomOption.preset = newPreset;
            foreach (CustomOption option in CustomOption.options) {
                if (option.id == 0) continue;

                option.entry = TheEpicRolesPlugin.Instance.Config.Bind($"Preset{preset}", option.id.ToString(), option.defaultSelection);
                option.selection = Mathf.Clamp(option.entry.Value, 0, option.selections.Length - 1);
                if (option.optionBehaviour != null && option.optionBehaviour is StringOption stringOption) {
                    stringOption.oldValue = stringOption.Value = option.selection;
                    stringOption.ValueText.text = option.selections[option.selection].ToString();
                }
            }
        }

        public static void ShareOptionSelections() {
            if (PlayerControl.AllPlayerControls.Count <= 1 || AmongUsClient.Instance?.AmHost == false && PlayerControl.LocalPlayer == null) return;
            
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareOptions, Hazel.SendOption.Reliable);
            messageWriter.WritePacked((uint)CustomOption.options.Count);
            foreach (CustomOption option in CustomOption.options) {
                messageWriter.WritePacked((uint)option.id);
                messageWriter.WritePacked((uint)Convert.ToUInt32(option.selection));
            }
            messageWriter.EndMessage();
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
            if (GameObject.Find("TERSettings") != null) { // Settings setup has already been performed, fixing the title of the tab and returning
                GameObject.Find("TERSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TMPro.TextMeshPro>().SetText("The Epic Roles Settings");
                return;
            }
            if (GameObject.Find("ImpostorSettings") != null) { 
                GameObject.Find("ImpostorSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TMPro.TextMeshPro>().SetText("Impostor Roles Settings");
                return;
            }
            if (GameObject.Find("NeutralSettings") != null) {
                GameObject.Find("NeutralSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TMPro.TextMeshPro>().SetText("Neutral Roles Settings");
                return;
            }
            if (GameObject.Find("CrewmateSettings") != null) {
                GameObject.Find("CrewmateSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TMPro.TextMeshPro>().SetText("Crewmate Roles Settings");
                return;
            }

            // Setup Custom Tabs
            var template = UnityEngine.Object.FindObjectsOfType<StringOption>().FirstOrDefault();
            if (template == null) return;
            var gameSettings = GameObject.Find("Game Settings");
            var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
            
            var terSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            var torMenu = terSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            terSettings.name = "TERSettings";

            var impostorSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            var impostorMenu = impostorSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            impostorSettings.name = "ImpostorSettings";

            var neutralSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            var neutralMenu = neutralSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            neutralSettings.name = "NeutralSettings";

            var crewmateSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            var crewmateMenu = crewmateSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            crewmateSettings.name = "CrewmateSettings";

            var roleTab = GameObject.Find("RoleTab");
            var gameTab = GameObject.Find("GameTab");

            var torTab = UnityEngine.Object.Instantiate(roleTab, roleTab.transform.parent);
            var torTabHighlight = torTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            torTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Helpers.loadSpriteFromResources("TheEpicRoles.Resources.TabIcon.png", 100f);
            torTab.name = "TorTab";

            var impostorTab = UnityEngine.Object.Instantiate(roleTab, roleTab.transform.parent);
            var impostorTabHighlight = impostorTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            impostorTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Helpers.loadSpriteFromResources("TheEpicRoles.Resources.TabIconImpostor.png", 100f);
            impostorTab.name = "ImpostorTab";

            var neutralTab = UnityEngine.Object.Instantiate(roleTab, roleTab.transform.parent);
            var neutralTabHighlight = neutralTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            neutralTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Helpers.loadSpriteFromResources("TheEpicRoles.Resources.TabIconNeutral.png", 100f);
            neutralTab.name = "NeutralTab";

            var crewmateTab = UnityEngine.Object.Instantiate(roleTab, roleTab.transform.parent);
            var crewmateTabHighlight = crewmateTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            crewmateTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Helpers.loadSpriteFromResources("TheEpicRoles.Resources.TabIconCrewmate.png", 100f);
            crewmateTab.name = "CrewmateTab";

            // Position of Tab Icons
            gameTab.transform.position += Vector3.left * 2f;
            roleTab.transform.position += Vector3.left * 2f;
            torTab.transform.position += Vector3.left * 1f;
            impostorTab.transform.position += Vector3.zero;
            neutralTab.transform.position += Vector3.right * 1f;
            crewmateTab.transform.position += Vector3.right * 2f;

            var tabs = new GameObject[]{gameTab, roleTab, torTab, impostorTab, neutralTab, crewmateTab};
            for (int i = 0; i < tabs.Length; i++) {
                var button = tabs[i].GetComponentInChildren<PassiveButton>();
                if (button == null) continue;
                int copiedIndex = i;
                button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => {
                    gameSettingMenu.RegularGameSettings.SetActive(false);
                    gameSettingMenu.RolesSettings.gameObject.SetActive(false);
                    terSettings.gameObject.SetActive(false);
                    impostorSettings.gameObject.SetActive(false);
                    neutralSettings.gameObject.SetActive(false);
                    crewmateSettings.gameObject.SetActive(false);
                    gameSettingMenu.GameSettingsHightlight.enabled = false;
                    gameSettingMenu.RolesSettingsHightlight.enabled = false;
                    torTabHighlight.enabled = false;
                    impostorTabHighlight.enabled = false;
                    neutralTabHighlight.enabled = false;
                    crewmateTabHighlight.enabled = false;
                    if (copiedIndex == 0) {
                        gameSettingMenu.RegularGameSettings.SetActive(true);
                        gameSettingMenu.GameSettingsHightlight.enabled = true;  
                    } else if (copiedIndex == 1) {
                        gameSettingMenu.RolesSettings.gameObject.SetActive(true);
                        gameSettingMenu.RolesSettingsHightlight.enabled = true;
                    } else if (copiedIndex == 2) {
                        terSettings.gameObject.SetActive(true);
                        torTabHighlight.enabled = true;
                    } else if (copiedIndex == 3) {
                        impostorSettings.gameObject.SetActive(true);
                        impostorTabHighlight.enabled = true;
                    } else if (copiedIndex == 4) {
                        neutralSettings.gameObject.SetActive(true);
                        neutralTabHighlight.enabled = true;
                    } else if (copiedIndex == 5) {
                        crewmateSettings.gameObject.SetActive(true);
                        crewmateTabHighlight.enabled = true;
                    }
                }));
            }

            foreach (OptionBehaviour option in torMenu.GetComponentsInChildren<OptionBehaviour>())
                UnityEngine.Object.Destroy(option.gameObject);
            List<OptionBehaviour> torOptions = new List<OptionBehaviour>();

            foreach (OptionBehaviour option in impostorMenu.GetComponentsInChildren<OptionBehaviour>())
                UnityEngine.Object.Destroy(option.gameObject);
            List<OptionBehaviour> impostorOptions = new List<OptionBehaviour>();

            foreach (OptionBehaviour option in neutralMenu.GetComponentsInChildren<OptionBehaviour>())
                UnityEngine.Object.Destroy(option.gameObject);
            List<OptionBehaviour> neutralOptions = new List<OptionBehaviour>();

            foreach (OptionBehaviour option in crewmateMenu.GetComponentsInChildren<OptionBehaviour>())
                UnityEngine.Object.Destroy(option.gameObject);
            List<OptionBehaviour> crewmateOptions = new List<OptionBehaviour>();

            // Add specific options to specific lists
            for (int i = 0; i < CustomOption.options.Count; i++) {
                CustomOption option = CustomOption.options[i];
                if (option.optionBehaviour == null) {
                    StringOption stringOption;                  
                    if (option.type == "impostor") {
                        stringOption = UnityEngine.Object.Instantiate(template, impostorMenu.transform);
                        impostorOptions.Add(stringOption);
                    }else if (option.type == "neutral" || option.type == "other") {
                        stringOption = UnityEngine.Object.Instantiate(template, neutralMenu.transform);
                        neutralOptions.Add(stringOption);
                    }else if (option.type == "crewmate") {
                        stringOption = UnityEngine.Object.Instantiate(template, crewmateMenu.transform);
                        crewmateOptions.Add(stringOption);
                    }else {
                        stringOption = UnityEngine.Object.Instantiate(template, torMenu.transform);
                        torOptions.Add(stringOption);
                    }
                    stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => {});
                    stringOption.TitleText.text = option.name;
                    stringOption.Value = stringOption.oldValue = option.selection;
                    stringOption.ValueText.text = option.selections[option.selection].ToString();

                    option.optionBehaviour = stringOption;
                }
                option.optionBehaviour.gameObject.SetActive(true);
            }

            torMenu.Children = torOptions.ToArray(); 
            terSettings.gameObject.SetActive(false);

            impostorMenu.Children = impostorOptions.ToArray();
            impostorSettings.gameObject.SetActive(false);

            neutralMenu.Children = neutralOptions.ToArray();
            neutralSettings.gameObject.SetActive(false);

            crewmateMenu.Children = crewmateOptions.ToArray();
            crewmateSettings.gameObject.SetActive(false);

            // Adapt task count for main options
            var commonTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumCommonTasks").TryCast<NumberOption>();
            if(commonTasksOption != null) commonTasksOption.ValidRange = new FloatRange(0f, 4f);

            var shortTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumShortTasks").TryCast<NumberOption>();
            if(shortTasksOption != null) shortTasksOption.ValidRange = new FloatRange(0f, 23f);

            var longTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumLongTasks").TryCast<NumberOption>();
            if(longTasksOption != null) longTasksOption.ValidRange = new FloatRange(0f, 15f);
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
            // Return Menu Update if in normal among us settings 
            var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
            if (gameSettingMenu.RegularGameSettings.active || gameSettingMenu.RolesSettings.gameObject.active) return;

            __instance.GetComponentInParent<Scroller>().ContentYBounds.max = -0.5F + __instance.Children.Length * 0.55F; 
            timer += Time.deltaTime;
            if (timer < 0.1f) return;
            timer = 0f;

            float offset = 2.75f;   
            foreach (CustomOption option in CustomOption.options) {
                if (GameObject.Find("TERSettings") && !(option.type == "option" || option.type == "map"))
                    continue;
                if (GameObject.Find("ImpostorSettings") && option.type != "impostor")
                    continue;
                if (GameObject.Find("NeutralSettings") && !(option.type == "neutral" || option.type == "other"))
                    continue;
                if (GameObject.Find("CrewmateSettings") && option.type != "crewmate")
                    continue;
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

    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
    class GameSettingMenuStartPatch {
        public static void Prefix(GameSettingMenu __instance) {
            __instance.HideForOnline = new Transform[]{};
        }
        
        public static void Postfix(GameSettingMenu __instance) {
            // Setup mapNameTransform
            var mapNameTransform = __instance.AllItems.FirstOrDefault(x => x.gameObject.activeSelf && x.name.Equals("MapName", StringComparison.OrdinalIgnoreCase));
            if (mapNameTransform == null) return;

            var options = new Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.KeyValuePair<string, int>>();
            for (int i = 0; i < Constants.MapNames.Length; i++) {
                var kvp = new Il2CppSystem.Collections.Generic.KeyValuePair<string, int>();
                kvp.key = Constants.MapNames[i];
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
            // Set max Font size to 1
            TMPro.TextMeshPro btnExitSprite = GameObject.Find("GameSettings_TMP").GetComponent<TMPro.TextMeshPro>();
            btnExitSprite.fontSizeMax = 1;

            StringBuilder result = new StringBuilder();

            if (TheEpicRolesPlugin.optionsPage == 0) {
                result.AppendLine($"1. Among Us Settings");
                result.AppendLine($"_______________________________");
                result.AppendLine();
                result.Append(__result);        
            }

            if (TheEpicRolesPlugin.optionsPage == 1) {
                result.AppendLine($"2. The Epic Roles Settings");
                result.AppendLine($"_______________________________");
                result.AppendLine();
                foreach (CustomOption option in CustomOption.options.Where(n => n.type == "option" || n.type == "map")) {
                    if (option == CustomOptionHolder.crewmateRolesCountMin) {
                        var optionName = CustomOptionHolder.cs(new Color(0, 1, 217f / 255f, 1f), "Crewmate Roles");
                        var min = CustomOptionHolder.crewmateRolesCountMin.getSelection();
                        var max = CustomOptionHolder.crewmateRolesCountMax.getSelection();
                        var auto = CustomOptionHolder.crewmateRolesMax.getSelection();
                        var optionValue = "";
                        if (min > max) min = max;
                        optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
                        if (auto == 1) optionValue = "Auto";
                        result.AppendLine($"{optionName}: {optionValue}");
                    }
                    else if (option == CustomOptionHolder.neutralRolesCountMin) {
                        var optionName = CustomOptionHolder.cs(new Color(0, 1, 217f / 255f, 1f), "Neutral Roles");
                        var min = CustomOptionHolder.neutralRolesCountMin.getSelection();
                        var max = CustomOptionHolder.neutralRolesCountMax.getSelection();
                        if (min > max) min = max;
                        var optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
                        result.AppendLine($"{optionName}: {optionValue}");
                    }
                    else if (option == CustomOptionHolder.impostorRolesCountMin) {
                        var optionName = CustomOptionHolder.cs(new Color(0, 1, 217f / 255f, 1f), "Impostor Roles");
                        var min = CustomOptionHolder.impostorRolesCountMin.getSelection();
                        var max = CustomOptionHolder.impostorRolesCountMax.getSelection();
                        if (min > max) min = max;
                        var optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
                        result.AppendLine($"{optionName}: {optionValue}");
                    }
                    else if ((option == CustomOptionHolder.crewmateRolesCountMax) || (option == CustomOptionHolder.neutralRolesCountMax) || (option == CustomOptionHolder.impostorRolesCountMax) || (option == CustomOptionHolder.crewmateRolesMax)) {
                        continue;
                    }
                    else {
                        result.AppendLine($"{option.name.Replace("\n", " ")}: {option.selections[option.selection].ToString()}");
                    }
                }
            }

            if (TheEpicRolesPlugin.optionsPage == 2) {
                result.AppendLine($"3. Role Allocation");
                result.AppendLine($"_______________________________");
                result.AppendLine();
                foreach (CustomOption option in CustomOption.options.Where(n => n.type == "impostor" && n.parent == null)) {
                    result.AppendLine($"{option.name.Replace("\n", " ")}: {option.selections[option.selection].ToString()}");
                }
                result.AppendLine();
                foreach (CustomOption option in CustomOption.options.Where(n => n.type == "neutral" && n.parent == null)) {
                    result.AppendLine($"{option.name.Replace("\n", " ")}: {option.selections[option.selection].ToString()}");
                }
                result.AppendLine();
                foreach (CustomOption option in CustomOption.options.Where(n => n.type == "other" && n.parent == null)) {
                    result.AppendLine($"{option.name.Replace("\n", " ")}: {option.selections[option.selection].ToString()}");
                }
                result.AppendLine();
                foreach (CustomOption option in CustomOption.options.Where(n => n.type == "crewmate" && n.parent == null)) {
                    result.AppendLine($"{option.name.Replace("\n", " ")}: {option.selections[option.selection].ToString()}");
                }       
            }

            if (TheEpicRolesPlugin.optionsPage == 3) {
                result.AppendLine($"4. Impostor Roles");
                result.AppendLine($"_______________________________");
                foreach (CustomOption option in CustomOption.options.Where(n => n.type == "impostor")) {
                    if (option.name.Contains("Witch")) // Linebreak before Witch
                        break;
                    if(option.parent == null)
                        result.AppendLine();                    
                    result.AppendLine($"{option.name.Replace("\n", " ")}: {option.selections[option.selection].ToString()}");                    
                }     
            }

            if (TheEpicRolesPlugin.optionsPage == 4) {
                bool start = false;
                foreach (CustomOption option in CustomOption.options.Where(n => n.type == "impostor")) {
                    if (option.name.Contains("Witch")) // Start with Witch
                        start = true;
                    if (start) {
                        if (option.parent == null)
                            result.AppendLine();
                        result.AppendLine($"{option.name.Replace("\n", " ")}: {option.selections[option.selection].ToString()}");
                    }
                }      
            }

            if (TheEpicRolesPlugin.optionsPage == 5) {
                result.AppendLine($"5. Neutral Roles");
                result.AppendLine($"_______________________________");
                foreach (CustomOption option in CustomOption.options.Where(n => n.type == "neutral")) {
                    if (option.parent == null)
                        result.AppendLine();
                    result.AppendLine($"{option.name.Replace("\n", " ")}: {option.selections[option.selection].ToString()}");
                }            
            }

            if (TheEpicRolesPlugin.optionsPage == 6) {
                result.AppendLine($"6. Other Roles");
                result.AppendLine($"_______________________________");
                foreach (CustomOption option in CustomOption.options.Where(n => n.type == "other")) {
                    if (option.parent == null)
                        result.AppendLine();
                    result.AppendLine($"{option.name.Replace("\n", " ")}: {option.selections[option.selection].ToString()}");
                }      
            }

            if (TheEpicRolesPlugin.optionsPage == 7) {                
                result.AppendLine($"7. Crewmate Roles");
                result.AppendLine($"_______________________________");
                foreach (CustomOption option in CustomOption.options.Where(n => n.type == "crewmate")) {
                    if (option.name.Contains("Medic")) // Linebreak before Medic
                        break;
                    if (option.parent == null)
                        result.AppendLine();
                    result.AppendLine($"{option.name.Replace("\n", " ")}: {option.selections[option.selection].ToString()}");
                }     
            }

            if (TheEpicRolesPlugin.optionsPage == 8) {
                bool start = false;
                foreach (CustomOption option in CustomOption.options.Where(n => n.type == "crewmate")) {
                    if (option.name.Contains("Security Guard")) // Linebreak before Security Guard
                        break;
                    if (option.name.Contains("Medic")) // Start with Medic
                        start = true;
                    if (start) {
                        if (option.parent == null)
                            result.AppendLine();
                        result.AppendLine($"{option.name.Replace("\n", " ")}: {option.selections[option.selection].ToString()}");
                    }
                }   
            }
            
            if (TheEpicRolesPlugin.optionsPage == 9) {
                bool start = false;
                foreach (CustomOption option in CustomOption.options.Where(n => n.type == "crewmate")) {
                    if (option.name.Contains("Security Guard")) // Start with Security Guard
                        start = true;
                    if (start) {
                        if (option.parent == null)
                            result.AppendLine();
                        result.AppendLine($"{option.name.Replace("\n", " ")}: {option.selections[option.selection].ToString()}");
                    }
                }   
            }
            // When no empty page
            if (TheEpicRolesPlugin.optionsPage != 10) {
                result.AppendLine($"_______________________________");
                result.AppendLine($"Page {TheEpicRolesPlugin.optionsPage + 1} of 10");
            }
            __result = result.ToString();
        }
    }

    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class GameOptionsNextPagePatch
    {
        public static void Postfix(KeyboardJoystick __instance) {
            if (!HudManager.Instance.Chat.IsOpen && AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) {
                if (Input.GetKeyDown(KeyCode.Tab)) {
                    TheEpicRolesPlugin.optionsPage++;
                    if (TheEpicRolesPlugin.optionsPage == 11) TheEpicRolesPlugin.optionsPage = 0;
                }
                if (Input.GetKeyDown(KeyCode.LeftShift)) {
                    TheEpicRolesPlugin.optionsPage--;
                    if (TheEpicRolesPlugin.optionsPage == -1) TheEpicRolesPlugin.optionsPage = 10;
                }

                if (Input.GetKeyDown(KeyCode.Alpha1))
                    TheEpicRolesPlugin.optionsPage = 0;         
                if (Input.GetKeyDown(KeyCode.Alpha2))
                    TheEpicRolesPlugin.optionsPage = 1;      
                if (Input.GetKeyDown(KeyCode.Alpha3))
                    TheEpicRolesPlugin.optionsPage = 2;       
                if (Input.GetKeyDown(KeyCode.Alpha4))
                    TheEpicRolesPlugin.optionsPage = 3;  
                if (Input.GetKeyDown(KeyCode.Alpha5))
                    TheEpicRolesPlugin.optionsPage = 4;       
                if (Input.GetKeyDown(KeyCode.Alpha6))
                    TheEpicRolesPlugin.optionsPage = 5;                
                if (Input.GetKeyDown(KeyCode.Alpha7))
                    TheEpicRolesPlugin.optionsPage = 6;                
                if (Input.GetKeyDown(KeyCode.Alpha8))
                    TheEpicRolesPlugin.optionsPage = 7;                
                if (Input.GetKeyDown(KeyCode.Alpha9))
                    TheEpicRolesPlugin.optionsPage = 8;
                if (Input.GetKeyDown(KeyCode.Alpha0)) 
                    TheEpicRolesPlugin.optionsPage = 9;
                if (Input.GetKeyDown(KeyCode.Backspace)) // Empty page
                    TheEpicRolesPlugin.optionsPage = 10;
            }
        }
    }
    
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class GameSettingsFontPatch {
        public static void Prefix(HudManager __instance) {    
            if (__instance.GameSettings != null) {
                __instance.GameSettings.SetOutlineThickness(0);
                __instance.GameSettings.fontSize = 1.2f; 
            }
        }
    }
}