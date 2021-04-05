using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using System;
using System.Linq;
using HarmonyLib;
using System.Reflection;
using System.Text;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles {
    public class CustomOptionHolder {
        public static string[] rates = new string[]{"0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%"};
        public static string[] crewmateRoleCaps = new string[]{"0", "0-1", "1", "1-2", "2", "2-3", "3", "3-4", "4", "4-5", "5", "5-6", "6", "6-7", "7", "7-8", "8", "8-9", "9", "9-10", "10"};
        public static string[] impostorRoleCaps = new string[]{"0", "0-1", "1", "1-2", "2", "2-3", "3"};
        public static string[] presets = new string[]{"Preset 1", "Preset 2", "Preset 3", "Preset 4", "Preset 5"};


        public static CustomOption presetSelection, crewmateRolesCount, impostorRolesCount;
        public static CustomOption mafiaSpawnRate, morphlingSpawnRate, camouflagerSpawnRate, vampireSpawnRate, eraserSpawnRate;
        public static CustomOption childSpawnRate, loversSpawnRate, jesterSpawnRate;
        public static CustomOption shifterSpawnRate, mayorSpawnRate, engineerSpawnRate, sheriffSpawnRate, lighterSpawnRate, detectiveSpawnRate, timeMasterSpawnRate, medicSpawnRate, swapperSpawnRate, seerSpawnRate, hackerSpawnRate, trackerSpawnRate, snitchSpawnRate, jackalSpawnRate;
        public static CustomOption maxNumberOfMeetings;
        public static CustomOption janitorCooldown, morphlingCooldown, camouflagerCooldown, vampireKillDelay, vampireCooldown, vampireCanKillNearGarlics, eraserCooldown;
        public static CustomOption childGrowingUpDuration, loversImpLoverRate, loversBothDie;
        public static CustomOption sheriffCooldown, jesterCanDieToSheriff, lighterModeLightsOnVision, lighterModeLightsOffVision, lighterCooldown, lighterDuration, detectiveAnonymousFootprints, detectiveFootprintIntervall, detectiveFootprintDuration, detectiveReportNameDuration, detectiveReportColorDuration, timeMasterCooldown, timeMasterRewindTime, medicShowShielded, medicShowAttemptToShielded, seerMode, seerSoulDuration, hackerCooldown, hackerHackeringDuration, hackerOnlyColorType, trackerUpdateIntervall, snitchLeftTasksForImpostors, jackalKillCooldown, jackalCreateSidekickCooldown, jackalCanUseVents, jackalCanCreateSidekick, sidekickPromotesToJackal, sidekickCanKill, sidekickCanUseVents, jackalPromotedFromSidekickCanCreateSidekick, jackalCanCreateSidekickFromImpostor;
    
        public static string cs(Color c, string s) {
            return string.Format("[{0:X2}{1:X2}{2:X2}{3:X2}]{4}[]", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
        }
 
        private static byte ToByte(float f) {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static void Load() {
            presetSelection = CustomOption.Create(0, "[CCCC00FF]Preset[]", presets);
            crewmateRolesCount = CustomOption.Create(1, "[CCCC00FF]Number Of Crewmate/Neutral Roles[]", crewmateRoleCaps);
            impostorRolesCount = CustomOption.Create(2, "[CCCC00FF]Number Of Impostor Roles[]", impostorRoleCaps);

            mafiaSpawnRate = CustomOption.Create(10, cs(Janitor.color, "Mafia"), rates);
            morphlingSpawnRate = CustomOption.Create(20, cs(Morphling.color, "Morphling"), rates);
            camouflagerSpawnRate = CustomOption.Create(30, cs(Camouflager.color, "Camouflager"), rates);
            vampireSpawnRate = CustomOption.Create(40, cs(Vampire.color, "Vampire"), rates);
            eraserSpawnRate = CustomOption.Create(230, cs(Eraser.color, "Eraser"), rates);

            childSpawnRate = CustomOption.Create(180, cs(Child.color, "Child"), rates);
            loversSpawnRate = CustomOption.Create(50, cs(Lovers.color, "Lovers"), rates);
            jesterSpawnRate = CustomOption.Create(60, cs(Jester.color, "Jester"), rates);

            shifterSpawnRate = CustomOption.Create(70, cs(Shifter.color, "Shifter"), rates);
            mayorSpawnRate = CustomOption.Create(80, cs(Mayor.color, "Mayor"), rates);
            engineerSpawnRate = CustomOption.Create(90, cs(Engineer.color, "Engineer"), rates);
            sheriffSpawnRate = CustomOption.Create(100, cs(Sheriff.color, "Sheriff"), rates);
            lighterSpawnRate = CustomOption.Create(110, cs(Lighter.color, "Lighter"), rates);
            detectiveSpawnRate = CustomOption.Create(120, cs(Detective.color, "Detective"), rates);
            timeMasterSpawnRate = CustomOption.Create(130, cs(TimeMaster.color, "Time Master"), rates);
            medicSpawnRate = CustomOption.Create(140, cs(Medic.color, "Medic"), rates);
            swapperSpawnRate = CustomOption.Create(150, cs(Swapper.color, "Swapper"), rates);
            seerSpawnRate = CustomOption.Create(160, cs(Seer.color, "Seer"), rates);
            hackerSpawnRate = CustomOption.Create(170, cs(Hacker.color, "Hacker"), rates);
            trackerSpawnRate = CustomOption.Create(200, cs(Tracker.color, "Tracker"), rates);
            snitchSpawnRate = CustomOption.Create(210, cs(Snitch.color, "Snitch"), rates);
            jackalSpawnRate = CustomOption.Create(220, cs(Jackal.color, "Jackal"), rates);

            // Map settings
            maxNumberOfMeetings = CustomOption.Create(3, "Number Of Meetings (excluding Mayor meeting)", 10, 0, 15, 1);

            // Role settings
            janitorCooldown = CustomOption.Create(11, "Janitor Cooldown", 30f, 10f, 60f, 2.5f);
            morphlingCooldown = CustomOption.Create(21, "Morphling Cooldown", 30f, 10f, 60f, 2.5f);
            camouflagerCooldown = CustomOption.Create(31, "Camouflager Cooldown", 30f, 10f, 60f, 2.5f);
            vampireKillDelay = CustomOption.Create(41, "Vampire Kill Delay", 10f, 1f, 20f, 1f);
            vampireCooldown = CustomOption.Create(42, "Vampire Cooldown", 30f, 10f, 60f, 2.5f);
            vampireCanKillNearGarlics = CustomOption.Create(43, "Vampire Can Kill Near Garlics", true);
            eraserCooldown = CustomOption.Create(231, "Eraser Cooldown", 30f, 10f, 120f, 5f);

            childGrowingUpDuration = CustomOption.Create(181, "Child Growing Up Duration", 400f, 100f, 1500f, 100f);
            loversImpLoverRate = CustomOption.Create(51, "Chance That One Lover Is Impostor", 30f, 0f, 100f, 10f);
            loversBothDie = CustomOption.Create(52, "Both Lovers Die", true);

            sheriffCooldown = CustomOption.Create(101, "Sheriff Cooldown", 30f, 10f, 60f, 2.5f);
            jesterCanDieToSheriff = CustomOption.Create(102, "Sheriff Can Kill The Jester", false);
            lighterModeLightsOnVision = CustomOption.Create(111, "Lighter Mode Vision On Lights On", 2f, 0.25f, 5f, 0.25f);
            lighterModeLightsOffVision = CustomOption.Create(112, "Lighter Mode Vision On Lights Off", 0.75f, 0.25f, 5f, 0.25f);
            lighterCooldown = CustomOption.Create(113, "Lighter Cooldown", 30f, 5f, 120f, 5f);
            lighterDuration = CustomOption.Create(114, "Lighter Duration", 5f, 2.5f, 60f, 2.5f);
            detectiveAnonymousFootprints = CustomOption.Create(121, "Anonymous Footprints", false); 
            detectiveFootprintIntervall = CustomOption.Create(122, "Footprint Intervall", 0.5f, 0.25f, 10f, 0.25f);
            detectiveFootprintDuration = CustomOption.Create(123, "Footprint Duration", 5f, 0.25f, 10f, 0.25f);
            detectiveReportNameDuration = CustomOption.Create(124, "Time Where Detective Reports Will Have Name", 0, 0, 60, 2.5f);
            detectiveReportColorDuration = CustomOption.Create(125, "Time Where Detective Reports Will Have Color Type", 20, 0, 120, 2.5f);
            timeMasterCooldown = CustomOption.Create(131, "Time Master Cooldown", 30f, 10f, 60f, 2.5f);
            timeMasterRewindTime = CustomOption.Create(132, "Rewind Time", 3f, 1f, 10f, 1f);
            medicShowShielded = CustomOption.Create(143, "Show Shielded Player", new string[] {"Everyone", "Shielded + Medic", "Medic"});
            medicShowAttemptToShielded = CustomOption.Create(144, "Shielded Player Sees Murder Attempt", false);
            seerMode = CustomOption.Create(161, "Seer Mode", new string[]{ "Show Death Flash + Souls", "Show Death Flash", "Show Souls"});
            seerSoulDuration = CustomOption.Create(162, "Seer Soul Duration", 15f, 0f, 60f, 5f);
            hackerCooldown = CustomOption.Create(171, "Hacker Cooldown", 30f, 0f, 60f, 5f);
            hackerHackeringDuration = CustomOption.Create(172, "Hacker Duration", 10f, 2.5f, 60f, 2.5f);
            hackerOnlyColorType = CustomOption.Create(173, "Hacker Only Sees Color Type", false);
            trackerUpdateIntervall = CustomOption.Create(201, "Tracker Update Intervall", 5f, 2.5f, 30f, 2.5f);
            snitchLeftTasksForImpostors = CustomOption.Create(211, "Task Count Where Impostors See Snitch", 1f, 0f, 5f, 1f);
            jackalKillCooldown = CustomOption.Create(221, "Jackal/Sidekick Kill Cooldown", 30f, 10f, 60f, 2.5f);
            jackalCreateSidekickCooldown = CustomOption.Create(222, "Jackal Create Sidekick Cooldown", 30f, 10f, 60f, 2.5f);
            jackalCanUseVents = CustomOption.Create(223, "Jackal can use vents", true);
            jackalCanCreateSidekick = CustomOption.Create(224, "Jackal can create a sidekick", false);
            sidekickPromotesToJackal = CustomOption.Create(225, "Sidekick gets promoted to Jackal on Jackal death", false);
            sidekickCanKill = CustomOption.Create(226, "Sidekick can kill", false);
            sidekickCanUseVents = CustomOption.Create(227, "Sidekick can use vents", true);
            jackalPromotedFromSidekickCanCreateSidekick = CustomOption.Create(228, "Jackals promoted from Sidekick can create a Sidekick", true);
            jackalCanCreateSidekickFromImpostor = CustomOption.Create(229, "Jackals can make an Impostor to his Sidekick", true);
        }
    }

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

        // Option creation

        public CustomOption(int id, string name,  System.Object[] selections, System.Object defaultValue) {
            this.id = id;
            this.name = name;
            this.selections = selections;
            int index = Array.IndexOf(selections, defaultValue);
            this.defaultSelection = index >= 0 ? index : 0;
            selection = 0;
            if (id != 0) {
                entry = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", id.ToString(), defaultSelection);
                selection = Mathf.Clamp(entry.Value, 0, selections.Length - 1);
            }
            options.Add(this);
        }

        public static CustomOption Create(int id, string name, string[] selections, string defaultValue = "") {
            return new CustomOption(id, name, selections, defaultValue);
        }

        public static CustomOption Create(int id, string name, float defaultValue, float min, float max, float step) {
            List<float> selections = new List<float>();
            for (float s = min; s <= max; s += step)
                selections.Add(s);
            return new CustomOption(id, name, selections.Cast<object>().ToArray(), defaultValue);
        }

        public static CustomOption Create(int id, string name, bool defaultValue) {
            return new CustomOption(id, name, new string[]{"False", "True"}, default ? "True" : "False");
        }

        // Static behaviour

        public static void switchPreset(int newPreset) {
            System.Console.WriteLine("Preset change");
            CustomOption.preset = newPreset;
            foreach (CustomOption option in CustomOption.options) {
                if (option.id == 0) continue;

                option.entry = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", option.id.ToString(), option.defaultSelection);
                option.selection = Mathf.Clamp(option.entry.Value, 0, option.selections.Length - 1);
                if (option.optionBehaviour != null && option.optionBehaviour is StringOption stringOption) {
                    stringOption.IOFLMCGMJBA = stringOption.Value = option.selection;
                    stringOption.ValueText.Text = option.selections[option.selection].ToString();
                }
            }
        }

        // Getter

        public int getSelection() {
            return selection;
        }

        public float getFloat() {
            return (float)selections[selection];
        }

        // Option changes

        public void valueChanged(OptionBehaviour o) {
            if (o is StringOption stringOption) {
                selection = stringOption.IOFLMCGMJBA = stringOption.Value;
                stringOption.ValueText.Text = selections[selection].ToString();

                if (id == 0) { // Switch preset before sharing the settings
                    switchPreset(selection);
                }
                System.Console.WriteLine(stringOption.GetInt());
           }
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    class GameOptionsMenuStartPatch {
        public static void Postfix(GameOptionsMenu __instance) {
            var template = UnityEngine.Object.FindObjectsOfType<StringOption>().FirstOrDefault();
            if (template == null) return;

            List<OptionBehaviour> allOptions = __instance.IGFJIPMAJHF.ToList();
            for (int i = 0; i < CustomOption.options.Count; i++) {
                CustomOption option = CustomOption.options[i];
                StringOption stringOption = (UnityEngine.Object.Instantiate(template, template.transform.parent));
                allOptions.Add(stringOption);
                stringOption.transform.localPosition = new Vector3(template.transform.localPosition.x, -7.85f - (i + 1) * 0.5F, template.transform.localPosition.z);
                option.optionBehaviour = stringOption;
            }
            __instance.IGFJIPMAJHF = allOptions.ToArray();
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    class GameOptionsMenuUpdatePatch
    {
        public static void Postfix(GameOptionsMenu __instance) {
            __instance.GetComponentInParent<Scroller>().YBounds.max = -0.5F + __instance.IGFJIPMAJHF.Length * 0.5F;
        }
    }


    [HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
    class StringOptionOnEnablePatch {
        public static void Postfix(StringOption __instance) {
            CustomOption option = CustomOption.options.FirstOrDefault(o => o.optionBehaviour == __instance);
            if (option != null) {
                __instance.Value = __instance.IOFLMCGMJBA = option.selection;
                __instance.ValueText.Text = option.selections[option.selection].ToString();
                __instance.OnValueChanged = new Action<OptionBehaviour>(option.valueChanged);
            }
        }
    }

    [HarmonyPatch(typeof(GameSettingMenu), "OnEnable")]
    class GameSettingMenuPatch {
        public static void Prefix(GameSettingMenu __instance) {
            __instance.HideForOnline = new Transform[]{};
        }
    }

    [HarmonyPatch] 
    class GameOptionsDataPatch
    {
        private static IEnumerable<MethodBase> TargetMethods() {
            return typeof(IGDMNKLDEPI).GetMethods().Where(x => x.ReturnType == typeof(string) && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(int));
        }

        private static void Postfix(ref string __result)
        {
            StringBuilder sb = new StringBuilder(__result);
            foreach (CustomOption option in CustomOption.options) { 
                sb.AppendLine($"{option.name}: {option.selections[option.selection].ToString()}");
            }
            var hudString = sb.ToString();

            int defaultSettingsLines = 19;
            int roleSettingsLines = 19 + 25;
            int detailedSettingsLines = 19 + 25 + 25;
            int end1 = hudString.TakeWhile(c => (defaultSettingsLines -= (c == '\n' ? 1 : 0)) > 0).Count();
            int end2 = hudString.TakeWhile(c => (roleSettingsLines -= (c == '\n' ? 1 : 0)) > 0).Count();
            int end3 = hudString.TakeWhile(c => (detailedSettingsLines -= (c == '\n' ? 1 : 0)) > 0).Count();
            int counter = TheOtherRolesPlugin.optionsPage;
            if (counter == 0) {
                hudString = hudString.Substring(0, end1) + "\n";   
            } else if (counter == 1) {
                hudString = hudString.Substring(end1 + 1, end2 - end1);
                // Temporary fix, should add a new CustomOption for spaces
                int gap = 1;
                int index = hudString.TakeWhile(c => (gap -= (c == '\n' ? 1 : 0)) > 0).Count();
                hudString = hudString.Insert(index, "\n");
                gap = 4;
                index = hudString.TakeWhile(c => (gap -= (c == '\n' ? 1 : 0)) > 0).Count();
                hudString = hudString.Insert(index, "\n");
                gap = 10;
                index = hudString.TakeWhile(c => (gap -= (c == '\n' ? 1 : 0)) > 0).Count();
                hudString = hudString.Insert(index + 1, "\n");
                gap = 14;
                index = hudString.TakeWhile(c => (gap -= (c == '\n' ? 1 : 0)) > 0).Count();
                hudString = hudString.Insert(index + 1, "\n");
            } else if (counter == 2) {
                hudString = hudString.Substring(end2 + 1, end3 - end2);
            } else if (counter == 3) {
                hudString = hudString.Substring(end3 + 1);
            }

            hudString += $"\n Press tab for more... ({counter+1}/4)";
            __result = hudString;
        }
    }

    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class GameOptionsNextPagePatch
    {
        public static void Postfix(KeyboardJoystick __instance)
        {
            if(Input.GetKeyDown(KeyCode.Tab)) {
                TheOtherRolesPlugin.optionsPage = (TheOtherRolesPlugin.optionsPage + 1) % 4;
            }
        }
    }

    
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class GameSettingsScalePatch {
        public static void Prefix(HudManager __instance) {
            __instance.GameSettings.scale = 0.5f; 
        }
    }
}