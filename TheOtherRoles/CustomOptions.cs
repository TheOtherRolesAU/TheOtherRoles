using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using static TheOtherRoles.TheOtherRoles;
using System;

namespace TheOtherRoles {
    class CustomOptionsHolder {
        public static string[] rates = new string[]{"0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%"};
        public static string[] crewmateRoleCaps = new string[]{"0", "0-1", "1", "1-2", "2", "2-3", "3", "3-4", "4", "4-5", "5", "5-6", "6", "6-7", "7", "7-8", "8", "8-9", "9", "9-10", "10"};
        public static string[] impostorRoleCaps = new string[]{"0", "0-1", "1", "1-2", "2", "2-3", "3"};
        public static string[] presets = new string[]{"Preset 1", "Preset 2", "Preset 3", "Preset 4", "Preset 5"};


        public static CustomOption presetSelection = CustomOption.Create(0, "[CCCC00FF]Preset[]", presets);
        public static CustomOption crewmateRolesCount = CustomOption.Create(1, "[CCCC00FF]Number Of Crewmate/Neutral Roles[]", crewmateRoleCaps);
        public static CustomOption impostorRolesCount = CustomOption.Create(2, "[CCCC00FF]Number Of Impostor Roles[]", impostorRoleCaps);

        public static CustomOption mafiaSpawnRate = CustomOption.Create(10, cs(Janitor.color, "Mafia"), rates);
        public static CustomOption morphlingSpawnRate = CustomOption.Create(20, cs(Morphling.color, "Morphling"), rates);
        public static CustomOption camouflagerSpawnRate = CustomOption.Create(30, cs(Camouflager.color, "Camouflager"), rates);
        public static CustomOption vampireSpawnRate = CustomOption.Create(40, cs(Vampire.color, "Vampire"), rates);
        public static CustomOption eraserSpawnRate = CustomOption.Create(230, cs(Eraser.color, "Eraser"), rates);

        public static CustomOption childSpawnRate = CustomOption.Create(180, cs(Child.color, "Child"), rates);
        public static CustomOption loversSpawnRate = CustomOption.Create(50, cs(Lovers.color, "Lovers"), rates);
        public static CustomOption jesterSpawnRate = CustomOption.Create(60, cs(Jester.color, "Jester"), rates);

        public static CustomOption shifterSpawnRate = CustomOption.Create(70, cs(Shifter.color, "Shifter"), rates);
        public static CustomOption mayorSpawnRate = CustomOption.Create(80, cs(Mayor.color, "Mayor"), rates);
        public static CustomOption engineerSpawnRate = CustomOption.Create(90, cs(Engineer.color, "Engineer"), rates);
        public static CustomOption sheriffSpawnRate = CustomOption.Create(100, cs(Sheriff.color, "Sheriff"), rates);
        public static CustomOption lighterSpawnRate = CustomOption.Create(110, cs(Lighter.color, "Lighter"), rates);
        public static CustomOption detectiveSpawnRate = CustomOption.Create(120, cs(Detective.color, "Detective"), rates);
        public static CustomOption timeMasterSpawnRate = CustomOption.Create(130, cs(TimeMaster.color, "Time Master"), rates);
        public static CustomOption medicSpawnRate = CustomOption.Create(140, cs(Medic.color, "Medic"), rates);
        public static CustomOption swapperSpawnRate = CustomOption.Create(150, cs(Swapper.color, "Swapper"), rates);
        public static CustomOption seerSpawnRate = CustomOption.Create(160, cs(Seer.color, "Seer"), rates);
        public static CustomOption hackerSpawnRate = CustomOption.Create(170, cs(Hacker.color, "Hacker"), rates);
        public static CustomOption trackerSpawnRate = CustomOption.Create(200, cs(Tracker.color, "Tracker"), rates);
        public static CustomOption snitchSpawnRate = CustomOption.Create(210, cs(Snitch.color, "Snitch"), rates);
        public static CustomOption jackalSpawnRate = CustomOption.Create(220, cs(Jackal.color, "Jackal"), rates);

        // Map settings
        public static CustomOption maxNumberOfMeetings = CustomOption.Create(3, "Number Of Meetings (excluding Mayor meeting)", 10, 0, 15, 1);

        // Role settings
        public static CustomOption janitorCooldown = CustomOption.Create(11, "Janitor Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomOption morphlingCooldown = CustomOption.Create(21, "Morphling Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomOption camouflagerCooldown = CustomOption.Create(31, "Camouflager Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomOption vampireKillDelay = CustomOption.Create(41, "Vampire Kill Delay", 10f, 1f, 20f, 1f);
        public static CustomOption vampireCooldown = CustomOption.Create(42, "Vampire Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomOption vampireCanKillNearGarlics = CustomOption.Create(43, "Vampire Can Kill Near Garlics", true);
        public static CustomOption eraserCooldown = CustomOption.Create(231, "Eraser Cooldown", 30f, 10f, 120f, 5f);

        public static CustomOption childGrowingUpDuration = CustomOption.Create(181, "Child Growing Up Duration", 400f, 100f, 1500f, 100f);
        public static CustomOption loversImpLoverRate = CustomOption.Create(51, "Chance That One Lover Is Impostor", 30f, 0f, 100f, 10f);
        public static CustomOption loversBothDie = CustomOption.Create(52, "Both Lovers Die", true);

        public static CustomOption sheriffCooldown = CustomOption.Create(101, "Sheriff Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomOption jesterCanDieToSheriff = CustomOption.Create(102, "Sheriff Can Kill The Jester", false);
        public static CustomOption lighterModeLightsOnVision = CustomOption.Create(111, "Lighter Mode Vision On Lights On", 2f, 0.25f, 5f, 0.25f);
        public static CustomOption lighterModeLightsOffVision = CustomOption.Create(112, "Lighter Mode Vision On Lights Off", 0.75f, 0.25f, 5f, 0.25f);
        public static CustomOption lighterCooldown = CustomOption.Create(113, "Lighter Cooldown", 30f, 5f, 120f, 5f);
        public static CustomOption lighterDuration = CustomOption.Create(114, "Lighter Duration", 5f, 2.5f, 60f, 2.5f);
        public static CustomOption detectiveAnonymousFootprints = CustomOption.Create(121, "Anonymous Footprints", false); 
        public static CustomOption detectiveFootprintIntervall = CustomOption.Create(122, "Footprint Intervall", 0.5f, 0.25f, 10f, 0.25f);
        public static CustomOption detectiveFootprintDuration = CustomOption.Create(123, "Footprint Duration", 5f, 0.25f, 10f, 0.25f);
        public static CustomOption detectiveReportNameDuration = CustomOption.Create(124, "Time Where Detective Reports Will Have Name", 0, 0, 60, 2.5f);
        public static CustomOption detectiveReportColorDuration = CustomOption.Create(125, "Time Where Detective Reports Will Have Color Type", 20, 0, 120, 2.5f);
        public static CustomOption timeMasterCooldown = CustomOption.Create(131, "Time Master Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomOption timeMasterRewindTime = CustomOption.Create(132, "Rewind Time", 3f, 1f, 10f, 1f);
        public static CustomOption medicShowShielded = CustomOption.Create(143, "Show Shielded Player", new string[] {"Everyone", "Shielded + Medic", "Medic"});
        public static CustomOption medicShowAttemptToShielded = CustomOption.Create(144, "Shielded Player Sees Murder Attempt", false);
        public static CustomOption seerMode = CustomOption.Create(161, "Seer Mode", new string[]{ "Show Death Flash + Souls", "Show Death Flash", "Show Souls"});
        public static CustomOption seerSoulDuration = CustomOption.Create(162, "Seer Soul Duration", 15f, 0f, 60f, 5f);
        public static CustomOption hackerCooldown = CustomOption.Create(171, "Hacker Cooldown", 30f, 0f, 60f, 5f);
        public static CustomOption hackerHackeringDuration = CustomOption.Create(172, "Hacker Duration", 10f, 2.5f, 60f, 2.5f);
        public static CustomOption hackerOnlyColorType = CustomOption.Create(173, "Hacker Only Sees Color Type", false);
        public static CustomOption trackerUpdateIntervall = CustomOption.Create(201, "Tracker Update Intervall", 5f, 2.5f, 30f, 2.5f);
        public static CustomOption snitchLeftTasksForImpostors = CustomOption.Create(211, "Task Count Where Impostors See Snitch", 1f, 0f, 5f, 1f);
        public static CustomOption jackalKillCooldown = CustomOption.Create(221, "Jackal/Sidekick Kill Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomOption jackalCreateSidekickCooldown = CustomOption.Create(222, "Jackal Create Sidekick Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomOption jackalCanUseVents = CustomOption.Create(223, "Jackal can use vents", true);
        public static CustomOption jackalCanCreateSidekick = CustomOption.Create(224, "Jackal can create a sidekick", false);
        public static CustomOption sidekickPromotesToJackal = CustomOption.Create(225, "Sidekick gets promoted to Jackal on Jackal death", false);
        public static CustomOption sidekickCanKill = CustomOption.Create(226, "Sidekick can kill", false);
        public static CustomOption sidekickCanUseVents = CustomOption.Create(227, "Sidekick can use vents", true);
        public static CustomOption jackalPromotedFromSidekickCanCreateSidekick = CustomOption.Create(228, "Jackals promoted from Sidekick can create a Sidekick", true);
        public static CustomOption jackalCanCreateSidekickFromImpostor = CustomOption.Create(229, "Jackals can make an Impostor to his Sidekick", true);
    
        public static string cs(Color c, string s) {
            return string.Format("[{0:X2}{1:X2}{2:X2}{3:X2}]{4}[]", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
        }
 
        private static byte ToByte(float f) {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }
    }

    class CustomOption {
        public static List<CustomOption> options = new List<CustomOption>();
        public static int preset = 0;

        public int id;
        public string name;
        public string[] selections;

        public int defaultSelection;
        public ConfigEntry<int> entry;
        public int selection;

        public CustomOption(int id, string name,  string[] selections, string defaultValue) {
            this.id = id;
            this.name = name;
            this.selections = selections;

            int index = Array.IndexOf(selections, defaultValue);
            this.defaultSelection = index >= 0 ? index : 0;

            entry = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", id.ToString(), defaultSelection);
            selection = Mathf.Clamp(entry.Value, 0, selections.Length - 1);
            options.Add(this);
        }

        public static CustomOption Create(int id, string name, string[] selections, string defaultValue = "") {
            return new CustomOption(id, name, selections, defaultValue);
        }

        public static CustomOption Create(int id, string name, float defaultValue, float min, float max, float step) {
            List<string> selections = new List<string>();
            for (float s = min; s <= max; s += step)
                selections.Add(s.ToString());
            return new CustomOption(id, name, selections.ToArray(), defaultValue.ToString());
        }

        public static CustomOption Create(int id, string name, bool defaultValue) {
            return new CustomOption(id, name, new string[]{"False", "True"}, default ? "True" : "False");
        }
    }
}