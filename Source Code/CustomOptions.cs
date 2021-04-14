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
    public class CustomOptionHolder {
        public static string[] rates = new string[]{"0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%"};
        public static string[] crewmateRoleCaps = new string[]{"0", "0-1", "1", "1-2", "2", "2-3", "3", "3-4", "4", "4-5", "5", "5-6", "6", "6-7", "7", "7-8", "8", "8-9", "9", "9-10", "10", "10-11", "11", "11-12", "12", "12-13", "13", "13-14", "14", "14-15", "15"};
        public static string[] impostorRoleCaps = new string[]{"0", "0-1", "1", "1-2", "2", "2-3", "3"};
        public static string[] presets = new string[]{"Preset 1", "Preset 2", "Preset 3", "Preset 4", "Preset 5"};

        public static CustomOption presetSelection;
        public static CustomOption crewmateRolesCount;
        public static CustomOption impostorRolesCount;

        public static CustomOption mafiaSpawnRate;
        public static CustomOption janitorCooldown;

        public static CustomOption morphlingSpawnRate;
        public static CustomOption morphlingCooldown;

        public static CustomOption camouflagerSpawnRate;
        public static CustomOption camouflagerCooldown;

        public static CustomOption vampireSpawnRate;
        public static CustomOption vampireKillDelay;
        public static CustomOption vampireCooldown;
        public static CustomOption vampireCanKillNearGarlics;

        public static CustomOption eraserSpawnRate;
        public static CustomOption eraserCooldown;
        public static CustomOption eraserCanEraseAnyone;

        public static CustomOption childSpawnRate;
        public static CustomOption childGrowingUpDuration;

        public static CustomOption loversSpawnRate;
        public static CustomOption loversImpLoverRate;
        public static CustomOption loversBothDie;

        public static CustomOption jesterSpawnRate;
        public static CustomOption jesterCanCallEmergency;

        public static CustomOption shifterSpawnRate;

        public static CustomOption mayorSpawnRate;

        public static CustomOption engineerSpawnRate;

        public static CustomOption sheriffSpawnRate;
        public static CustomOption sheriffCooldown;
        public static CustomOption jesterCanDieToSheriff;

        public static CustomOption lighterSpawnRate;
        public static CustomOption lighterModeLightsOnVision;
        public static CustomOption lighterModeLightsOffVision;
        public static CustomOption lighterCooldown;
        public static CustomOption lighterDuration;

        public static CustomOption detectiveSpawnRate;
        public static CustomOption detectiveAnonymousFootprints;
        public static CustomOption detectiveFootprintIntervall;
        public static CustomOption detectiveFootprintDuration;
        public static CustomOption detectiveReportNameDuration;
        public static CustomOption detectiveReportColorDuration;

        public static CustomOption timeMasterSpawnRate;
        public static CustomOption timeMasterCooldown;
        public static CustomOption timeMasterRewindTime;
        public static CustomOption timeMasterShieldDuration;

        public static CustomOption medicSpawnRate;
        public static CustomOption medicShowShielded;
        public static CustomOption medicShowAttemptToShielded;

        public static CustomOption swapperSpawnRate;
        public static CustomOption swapperCanCallEmergency;

        public static CustomOption seerSpawnRate;
        public static CustomOption seerMode;
        public static CustomOption seerSoulDuration;
        public static CustomOption seerLimitSoulDuration;

        public static CustomOption hackerSpawnRate;
        public static CustomOption hackerCooldown;
        public static CustomOption hackerHackeringDuration;
        public static CustomOption hackerOnlyColorType;

        public static CustomOption trackerSpawnRate;
        public static CustomOption trackerUpdateIntervall;

        public static CustomOption snitchSpawnRate;
        public static CustomOption snitchLeftTasksForImpostors;

        public static CustomOption jackalSpawnRate;
        public static CustomOption jackalKillCooldown;
        public static CustomOption jackalCreateSidekickCooldown;
        public static CustomOption jackalCanUseVents;
        public static CustomOption jackalCanCreateSidekick;
        public static CustomOption sidekickPromotesToJackal;
        public static CustomOption sidekickCanKill;
        public static CustomOption sidekickCanUseVents;
        public static CustomOption jackalPromotedFromSidekickCanCreateSidekick;
        public static CustomOption jackalCanCreateSidekickFromImpostor;

        public static CustomOption spySpawnRate;
        public static CustomOption spyCanDieToSheriff;
        public static CustomOption spyImpostorsCanKillAnyone;

        public static CustomOption tricksterSpawnRate;
        public static CustomOption tricksterPlaceBoxCooldown;
        public static CustomOption tricksterLightsOutCooldown;
        public static CustomOption tricksterLightsOutDuration;

        public static CustomOption maxNumberOfMeetings;
        public static CustomOption allowSkipOnEmergencyMeetings;

        public static string cs(Color c, string s) {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
        }
 
        private static byte ToByte(float f) {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static void Load() {
            
            // Role Options
            presetSelection = CustomOption.Create(0, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Preset"), presets, null, true);

            crewmateRolesCount = CustomOption.Create(1, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Number Of Crewmate/Neutral Roles"), crewmateRoleCaps, null, true);
            impostorRolesCount = CustomOption.Create(2, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Number Of Impostor Roles"), impostorRoleCaps);

            mafiaSpawnRate = CustomOption.Create(10, cs(Janitor.color, "Mafia"), rates, null, true);
            janitorCooldown = CustomOption.Create(11, "Janitor Cooldown", 30f, 10f, 60f, 2.5f, mafiaSpawnRate);

            morphlingSpawnRate = CustomOption.Create(20, cs(Morphling.color, "Morphling"), rates, null, true);
            morphlingCooldown = CustomOption.Create(21, "Morphling Cooldown", 30f, 10f, 60f, 2.5f, morphlingSpawnRate);

            camouflagerSpawnRate = CustomOption.Create(30, cs(Camouflager.color, "Camouflager"), rates, null, true);
            camouflagerCooldown = CustomOption.Create(31, "Camouflager Cooldown", 30f, 10f, 60f, 2.5f, camouflagerSpawnRate);

            vampireSpawnRate = CustomOption.Create(40, cs(Vampire.color, "Vampire"), rates, null, true);
            vampireKillDelay = CustomOption.Create(41, "Vampire Kill Delay", 10f, 1f, 20f, 1f, vampireSpawnRate);
            vampireCooldown = CustomOption.Create(42, "Vampire Cooldown", 30f, 10f, 60f, 2.5f, vampireSpawnRate);
            vampireCanKillNearGarlics = CustomOption.Create(43, "Vampire Can Kill Near Garlics", true, vampireSpawnRate);

            eraserSpawnRate = CustomOption.Create(230, cs(Eraser.color, "Eraser"), rates, null, true);
            eraserCooldown = CustomOption.Create(231, "Eraser Cooldown", 30f, 10f, 120f, 5f, eraserSpawnRate);
            eraserCanEraseAnyone = CustomOption.Create(232, "Eraser Can Erase Anyone", false, eraserSpawnRate);

            tricksterSpawnRate = CustomOption.Create(250, cs(Trickster.color, "Trickster"), rates, null, true);
            tricksterPlaceBoxCooldown = CustomOption.Create(251, "Trickster Box Cooldown", 10f, 0f, 30f, 2.5f, tricksterSpawnRate);
            tricksterLightsOutCooldown = CustomOption.Create(252, "Trickster Lights Out Cooldown", 30f, 10f, 60f, 5f, tricksterSpawnRate);
            tricksterLightsOutDuration = CustomOption.Create(253, "Trickster Lights Out Duration", 15f, 5f, 60f, 2.5f, tricksterSpawnRate);

            childSpawnRate = CustomOption.Create(180, cs(Child.color, "Child"), rates, null, true);
            childGrowingUpDuration = CustomOption.Create(181, "Child Growing Up Duration", 400f, 100f, 1500f, 100f, childSpawnRate);

            loversSpawnRate = CustomOption.Create(50, cs(Lovers.color, "Lovers"), rates, null, true);
            loversImpLoverRate = CustomOption.Create(51, "Chance That One Lover Is Impostor", 30f, 0f, 100f, 10f, loversSpawnRate);
            loversBothDie = CustomOption.Create(52, "Both Lovers Die", true, loversSpawnRate);

            jesterSpawnRate = CustomOption.Create(60, cs(Jester.color, "Jester"), rates, null, true);
            jesterCanCallEmergency = CustomOption.Create(61, "Jester can call emergency meeting", true, jesterSpawnRate);

            shifterSpawnRate = CustomOption.Create(70, cs(Shifter.color, "Shifter"), rates, null, true);

            mayorSpawnRate = CustomOption.Create(80, cs(Mayor.color, "Mayor"), rates, null, true);

            engineerSpawnRate = CustomOption.Create(90, cs(Engineer.color, "Engineer"), rates, null, true);

            sheriffSpawnRate = CustomOption.Create(100, cs(Sheriff.color, "Sheriff"), rates, null, true);
            sheriffCooldown = CustomOption.Create(101, "Sheriff Cooldown", 30f, 10f, 60f, 2.5f, sheriffSpawnRate);
            jesterCanDieToSheriff = CustomOption.Create(102, "Sheriff Can Kill The Jester", false, sheriffSpawnRate);


            lighterSpawnRate = CustomOption.Create(110, cs(Lighter.color, "Lighter"), rates, null, true);
            lighterModeLightsOnVision = CustomOption.Create(111, "Lighter Mode Vision On Lights On", 2f, 0.25f, 5f, 0.25f, lighterSpawnRate);
            lighterModeLightsOffVision = CustomOption.Create(112, "Lighter Mode Vision On Lights Off", 0.75f, 0.25f, 5f, 0.25f, lighterSpawnRate);
            lighterCooldown = CustomOption.Create(113, "Lighter Cooldown", 30f, 5f, 120f, 5f, lighterSpawnRate);
            lighterDuration = CustomOption.Create(114, "Lighter Duration", 5f, 2.5f, 60f, 2.5f, lighterSpawnRate);

            detectiveSpawnRate = CustomOption.Create(120, cs(Detective.color, "Detective"), rates, null, true);
            detectiveAnonymousFootprints = CustomOption.Create(121, "Anonymous Footprints", false, detectiveSpawnRate); 
            detectiveFootprintIntervall = CustomOption.Create(122, "Footprint Intervall", 0.5f, 0.25f, 10f, 0.25f, detectiveSpawnRate);
            detectiveFootprintDuration = CustomOption.Create(123, "Footprint Duration", 5f, 0.25f, 10f, 0.25f, detectiveSpawnRate);
            detectiveReportNameDuration = CustomOption.Create(124, "Time Where Detective Reports Will Have Name", 0, 0, 60, 2.5f, detectiveSpawnRate);
            detectiveReportColorDuration = CustomOption.Create(125, "Time Where Detective Reports Will Have Color Type", 20, 0, 120, 2.5f, detectiveSpawnRate);

            timeMasterSpawnRate = CustomOption.Create(130, cs(TimeMaster.color, "Time Master"), rates, null, true);
            timeMasterCooldown = CustomOption.Create(131, "Time Master Cooldown", 30f, 10f, 120f, 2.5f, timeMasterSpawnRate);
            timeMasterRewindTime = CustomOption.Create(132, "Rewind Time", 3f, 1f, 10f, 1f, timeMasterSpawnRate);
            timeMasterShieldDuration = CustomOption.Create(133, "Time Master Shield Duration", 3f, 1f, 20f, 1f, timeMasterSpawnRate);

            medicSpawnRate = CustomOption.Create(140, cs(Medic.color, "Medic"), rates, null, true);
            medicShowShielded = CustomOption.Create(143, "Show Shielded Player", new string[] {"Everyone", "Shielded + Medic", "Medic"}, medicSpawnRate);
            medicShowAttemptToShielded = CustomOption.Create(144, "Shielded Player Sees Murder Attempt", false, medicSpawnRate);

            swapperSpawnRate = CustomOption.Create(150, cs(Swapper.color, "Swapper"), rates, null, true);
            swapperCanCallEmergency = CustomOption.Create(151, "Swapper can call emergency meeting", false, swapperSpawnRate);

            seerSpawnRate = CustomOption.Create(160, cs(Seer.color, "Seer"), rates, null, true);
            seerMode = CustomOption.Create(161, "Seer Mode", new string[]{ "Show Death Flash + Souls", "Show Death Flash", "Show Souls"}, seerSpawnRate);
            seerLimitSoulDuration = CustomOption.Create(163, "Seer Limit Soul Duration", false, seerSpawnRate);
            seerSoulDuration = CustomOption.Create(162, "Seer Soul Duration", 15f, 0f, 60f, 5f, seerLimitSoulDuration);
        
            hackerSpawnRate = CustomOption.Create(170, cs(Hacker.color, "Hacker"), rates, null, true);
            hackerCooldown = CustomOption.Create(171, "Hacker Cooldown", 30f, 0f, 60f, 5f, hackerSpawnRate);
            hackerHackeringDuration = CustomOption.Create(172, "Hacker Duration", 10f, 2.5f, 60f, 2.5f, hackerSpawnRate);
            hackerOnlyColorType = CustomOption.Create(173, "Hacker Only Sees Color Type", false, hackerSpawnRate);

            trackerSpawnRate = CustomOption.Create(200, cs(Tracker.color, "Tracker"), rates, null, true);
            trackerUpdateIntervall = CustomOption.Create(201, "Tracker Update Intervall", 5f, 2.5f, 30f, 2.5f, trackerSpawnRate);

            snitchSpawnRate = CustomOption.Create(210, cs(Snitch.color, "Snitch"), rates, null, true);
            snitchLeftTasksForImpostors = CustomOption.Create(211, "Task Count Where Impostors See Snitch", 1f, 0f, 5f, 1f, snitchSpawnRate);

            jackalSpawnRate = CustomOption.Create(220, cs(Jackal.color, "Jackal"), rates, null, true);
            jackalKillCooldown = CustomOption.Create(221, "Jackal/Sidekick Kill Cooldown", 30f, 10f, 60f, 2.5f, jackalSpawnRate);
            jackalCreateSidekickCooldown = CustomOption.Create(222, "Jackal Create Sidekick Cooldown", 30f, 10f, 60f, 2.5f, jackalSpawnRate);
            jackalCanUseVents = CustomOption.Create(223, "Jackal Can Use Vents", true, jackalSpawnRate);
            jackalCanCreateSidekick = CustomOption.Create(224, "Jackal Can Create A Sidekick", false, jackalSpawnRate);
            sidekickPromotesToJackal = CustomOption.Create(225, "Sidekick Gets Promoted To Jackal On Jackal Death", false, jackalSpawnRate);
            sidekickCanKill = CustomOption.Create(226, "Sidekick Can Kill", false, jackalSpawnRate);
            sidekickCanUseVents = CustomOption.Create(227, "Sidekick Can Use Vents", true, jackalSpawnRate);
            jackalPromotedFromSidekickCanCreateSidekick = CustomOption.Create(228, "Jackals Promoted From Sidekick Can Create A Sidekick", true, jackalSpawnRate);
            jackalCanCreateSidekickFromImpostor = CustomOption.Create(229, "Jackals Can Make An Impostor To His Sidekick", true, jackalSpawnRate);

            spySpawnRate = CustomOption.Create(240, cs(Spy.color, "Spy"), rates, null, true);
            spyCanDieToSheriff = CustomOption.Create(241, "Spy Can Die To Sheriff", false, spySpawnRate);
            spyImpostorsCanKillAnyone = CustomOption.Create(242, "Impostors Can Kill Anyone If There Is A Spy", true, spySpawnRate);

            // Other options
            maxNumberOfMeetings = CustomOption.Create(3, "Number Of Meetings (excluding Mayor meeting)", 10, 0, 15, 1, null, true);
            allowSkipOnEmergencyMeetings = CustomOption.Create(4, "Allow Skips On Emergency Meetings", true);
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
                    stringOption.LCDAKOCANPH = stringOption.Value = option.selection;
                    stringOption.ValueText.text = option.selections[option.selection].ToString();
                }
            }
        }

        public static void ShareOptionSelections() {
            if (PlayerControl.AllPlayerControls.Count <= 1 || AmongUsClient.Instance?.HHBLOCGKFAB == false && PlayerControl.LocalPlayer == null) return;
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
                stringOption.LCDAKOCANPH = stringOption.Value = selection;
                stringOption.ValueText.text = selections[selection].ToString();

                if (AmongUsClient.Instance?.HHBLOCGKFAB == true && PlayerControl.LocalPlayer) {
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

            List<OptionBehaviour> allOptions = __instance.MCAHCPOHNFI.ToList();
            for (int i = 0; i < CustomOption.options.Count; i++) {
                CustomOption option = CustomOption.options[i];
                if (option.optionBehaviour == null) {
                    StringOption stringOption = UnityEngine.Object.Instantiate(template, template.transform.parent);
                    allOptions.Add(stringOption);

                    stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => {});
                    stringOption.TitleText.text = option.name;
                    stringOption.Value = stringOption.LCDAKOCANPH = option.selection;
                    stringOption.ValueText.text = option.selections[option.selection].ToString();

                    option.optionBehaviour = stringOption;
                }
                option.optionBehaviour.gameObject.SetActive(true);
            }
            __instance.MCAHCPOHNFI = allOptions.ToArray();
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
    public class StringOptionEnablePatch {
        public static bool Prefix(StringOption __instance) {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;

            __instance.OnValueChanged = new Action<OptionBehaviour>((o) => {});
            __instance.TitleText.text = option.name;
            __instance.Value = __instance.LCDAKOCANPH = option.selection;
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
            __instance.GetComponentInParent<Scroller>().YBounds.max = -0.5F + __instance.MCAHCPOHNFI.Length * 0.55F; 
            timer += Time.deltaTime;
            if (timer < 0.1f) return;
            timer = 0f;

            float offset = -7.85f;
            foreach (CustomOption option in CustomOption.options) {
                if (option?.optionBehaviour?.gameObject != null) {
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
    }

    [HarmonyPatch] 
    class GameOptionsDataPatch
    {
        private static IEnumerable<MethodBase> TargetMethods() {
            return typeof(CEIOGGEDKAN).GetMethods().Where(x => x.ReturnType == typeof(string) && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(int));
        }

        private static void Postfix(ref string __result)
        {
            StringBuilder sb = new StringBuilder(__result);
            foreach (CustomOption option in CustomOption.options)
                if (option.parent == null)
                    sb.AppendLine($"{option.name}: {option.selections[option.selection].ToString()}");
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
            int roleSettingsLines = 19 + 27;
            int detailedSettingsLines = 19 + 27 + 37;
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
                gap = 11;
                index = hudString.TakeWhile(c => (gap -= (c == '\n' ? 1 : 0)) > 0).Count();
                hudString = hudString.Insert(index + 1, "\n");
                gap = 15;
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
            if (__instance.GameSettings != null) __instance.GameSettings.fontSize = 1.2f; 
        }
    }
}