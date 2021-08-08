using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx.Configuration;
using HarmonyLib;
using Hazel;
using TheOtherRoles.Roles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles
{
    public static class CustomOptionHolder
    {
        private static readonly object[] Rates =
            {"0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%"};

        private static readonly object[] Presets = {"Preset 1", "Preset 2", "Preset 3", "Preset 4", "Preset 5"};

        private static CustomOption presetSelection;
        public static CustomOption crewmateRolesCountMin;
        public static CustomOption crewmateRolesCountMax;
        public static CustomOption neutralRolesCountMin;
        public static CustomOption neutralRolesCountMax;
        public static CustomOption impostorRolesCountMin;
        public static CustomOption impostorRolesCountMax;

        public static CustomOption mafiaSpawnRate;
        public static CustomOption janitorCooldown;

        public static CustomOption morphlingSpawnRate;
        public static CustomOption morphlingCooldown;
        public static CustomOption morphlingDuration;

        public static CustomOption camouflagerSpawnRate;
        public static CustomOption camouflagerCooldown;
        public static CustomOption camouflagerDuration;

        public static CustomOption vampireSpawnRate;
        public static CustomOption vampireKillDelay;
        public static CustomOption vampireCooldown;
        public static CustomOption vampireCanKillNearGarlics;

        public static CustomOption eraserSpawnRate;
        public static CustomOption eraserCooldown;
        public static CustomOption eraserCanEraseAnyone;

        public static CustomOption miniSpawnRate;
        public static CustomOption miniGrowingUpDuration;

        public static CustomOption loversSpawnRate;
        public static CustomOption loversImpLoverRate;
        public static CustomOption loversBothDie;
        public static CustomOption loversCanHaveAnotherRole;

        public static CustomOption guesserSpawnRate;
        public static CustomOption guesserIsImpGuesserRate;
        public static CustomOption guesserNumberOfShots;

        public static CustomOption jesterSpawnRate;
        public static CustomOption jesterCanCallEmergency;
        public static CustomOption jesterCanSabotage;

        public static CustomOption arsonistSpawnRate;
        public static CustomOption arsonistCooldown;
        public static CustomOption arsonistDuration;

        public static CustomOption jackalSpawnRate;
        public static CustomOption jackalKillCooldown;
        public static CustomOption jackalCreateSidekickCooldown;
        public static CustomOption jackalCanUseVents;
        public static CustomOption jackalCanCreateSidekick;
        public static CustomOption sidekickPromotesToJackal;
        public static CustomOption sidekickCanKill;
        public static CustomOption sidekickCanUseVents;
        public static CustomOption recursiveSidekicks;
        public static CustomOption jackalCanCreateSidekickFromImpostor;
        public static CustomOption jackalAndSidekickHaveImpostorVision;

        public static CustomOption bountyHunterSpawnRate;
        public static CustomOption bountyHunterBountyDuration;
        public static CustomOption bountyHunterReducedCooldown;
        public static CustomOption bountyHunterPunishmentTime;
        public static CustomOption bountyHunterShowArrow;
        public static CustomOption bountyHunterArrowUpdateInterval;

        public static CustomOption shifterSpawnRate;
        public static CustomOption shifterShiftsModifiers;

        public static CustomOption mayorSpawnRate;

        public static CustomOption engineerSpawnRate;

        public static CustomOption sheriffSpawnRate;
        public static CustomOption sheriffCooldown;
        public static CustomOption sheriffCanKillNeutrals;

        public static CustomOption lighterSpawnRate;
        public static CustomOption lighterModeLightsOnVision;
        public static CustomOption lighterModeLightsOffVision;
        public static CustomOption lighterCooldown;
        public static CustomOption lighterDuration;

        public static CustomOption detectiveSpawnRate;
        public static CustomOption detectiveAnonymousFootprints;
        public static CustomOption detectiveFootprintInterval;
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
        public static CustomOption medicSetShieldAfterMeeting;

        public static CustomOption swapperSpawnRate;
        public static CustomOption swapperCanCallEmergency;
        public static CustomOption swapperCanOnlySwapOthers;

        public static CustomOption seerSpawnRate;
        public static CustomOption seerMode;
        public static CustomOption seerSoulDuration;
        public static CustomOption seerLimitSoulDuration;

        public static CustomOption hackerSpawnRate;
        public static CustomOption hackerCooldown;
        public static CustomOption hackerHackeringDuration;
        public static CustomOption hackerOnlyColorType;

        public static CustomOption trackerSpawnRate;
        public static CustomOption trackerUpdateInterval;
        public static CustomOption trackerResetTargetAfterMeeting;

        public static CustomOption snitchSpawnRate;
        public static CustomOption snitchLeftTasksForReveal;
        public static CustomOption snitchIncludeTeamJackal;
        public static CustomOption snitchTeamJackalUseDifferentArrowColor;

        public static CustomOption spySpawnRate;
        public static CustomOption spyCanDieToSheriff;
        public static CustomOption spyImpostorsCanKillAnyone;
        public static CustomOption spyCanEnterVents;
        public static CustomOption spyHasImpostorVision;

        public static CustomOption tricksterSpawnRate;
        public static CustomOption tricksterPlaceBoxCooldown;
        public static CustomOption tricksterLightsOutCooldown;
        public static CustomOption tricksterLightsOutDuration;

        public static CustomOption cleanerSpawnRate;
        public static CustomOption cleanerCooldown;

        public static CustomOption warlockSpawnRate;
        public static CustomOption warlockCooldown;
        public static CustomOption warlockRootTime;

        public static CustomOption securityGuardSpawnRate;
        public static CustomOption securityGuardCooldown;
        public static CustomOption securityGuardTotalScrews;
        public static CustomOption securityGuardCamPrice;
        public static CustomOption securityGuardVentPrice;

        public static CustomOption baitSpawnRate;
        public static CustomOption baitHighlightAllVents;
        public static CustomOption baitReportDelay;

        public static CustomOption maxNumberOfMeetings;
        public static CustomOption blockSkippingInEmergencyMeetings;
        public static CustomOption noVoteIsSelfVote;
        public static CustomOption hidePlayerNames;

        internal static readonly Dictionary<byte, byte[]> BlockedRolePairings = new();

        public static string Cs(Color c, string s)
        {
            return $"<color=#{ToByte(c.r):X2}{ToByte(c.g):X2}{ToByte(c.b):X2}{ToByte(c.a):X2}>{s}</color>";
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte) (f * 255);
        }

        public static void Load()
        {
            // Role Options
            presetSelection = CustomOption.Create(0, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Preset"), Presets,
                null, true);

            // Using new id's for the options to not break compatibility with older versions
            crewmateRolesCountMin = CustomOption.Create(300,
                Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Minimum Crewmate Roles"), 0f, 0f, 15f, 1f, null, true);
            crewmateRolesCountMax = CustomOption.Create(301,
                Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Maximum Crewmate Roles"), 0f, 0f, 15f, 1f);
            neutralRolesCountMin = CustomOption.Create(302,
                Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Minimum Neutral Roles"), 0f, 0f, 15f, 1f);
            neutralRolesCountMax = CustomOption.Create(303,
                Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Maximum Neutral Roles"), 0f, 0f, 15f, 1f);
            impostorRolesCountMin = CustomOption.Create(304,
                Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Minimum Impostor Roles"), 0f, 0f, 3f, 1f);
            impostorRolesCountMax = CustomOption.Create(305,
                Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Maximum Impostor Roles"), 0f, 0f, 3f, 1f);

            mafiaSpawnRate = CustomOption.Create(10, Cs(Janitor.Instance.color, "Mafia"), Rates, null, true);
            janitorCooldown = CustomOption.Create(11, "Janitor Cooldown", 30f, 10f, 60f, 2.5f, mafiaSpawnRate);

            morphlingSpawnRate = CustomOption.Create(20, Cs(Morphling.Instance.color, "Morphling"), Rates, null, true);
            morphlingCooldown = CustomOption.Create(21, "Morphling Cooldown", 30f, 10f, 60f, 2.5f, morphlingSpawnRate);
            morphlingDuration = CustomOption.Create(22, "Morph Duration", 10f, 1f, 20f, 0.5f, morphlingSpawnRate);

            camouflagerSpawnRate =
                CustomOption.Create(30, Cs(Camouflager.Instance.color, "Camouflager"), Rates, null, true);
            camouflagerCooldown =
                CustomOption.Create(31, "Camouflager Cooldown", 30f, 10f, 60f, 2.5f, camouflagerSpawnRate);
            camouflagerDuration = CustomOption.Create(32, "Camo Duration", 10f, 1f, 20f, 0.5f, camouflagerSpawnRate);

            vampireSpawnRate = CustomOption.Create(40, Cs(Vampire.Instance.color, "Vampire"), Rates, null, true);
            vampireKillDelay = CustomOption.Create(41, "Vampire Kill Delay", 10f, 1f, 20f, 1f, vampireSpawnRate);
            vampireCooldown = CustomOption.Create(42, "Vampire Cooldown", 30f, 10f, 60f, 2.5f, vampireSpawnRate);
            vampireCanKillNearGarlics =
                CustomOption.Create(43, "Vampire Can Kill Near Garlics", true, vampireSpawnRate);

            eraserSpawnRate = CustomOption.Create(230, Cs(Eraser.Instance.color, "Eraser"), Rates, null, true);
            eraserCooldown = CustomOption.Create(231, "Eraser Cooldown", 30f, 10f, 120f, 5f, eraserSpawnRate);
            eraserCanEraseAnyone = CustomOption.Create(232, "Eraser Can Erase Anyone", false, eraserSpawnRate);

            tricksterSpawnRate = CustomOption.Create(250, Cs(Trickster.Instance.color, "Trickster"), Rates, null, true);
            tricksterPlaceBoxCooldown =
                CustomOption.Create(251, "Trickster Box Cooldown", 10f, 0f, 30f, 2.5f, tricksterSpawnRate);
            tricksterLightsOutCooldown = CustomOption.Create(252, "Trickster Lights Out Cooldown", 30f, 10f, 60f, 5f,
                tricksterSpawnRate);
            tricksterLightsOutDuration = CustomOption.Create(253, "Trickster Lights Out Duration", 15f, 5f, 60f, 2.5f,
                tricksterSpawnRate);

            cleanerSpawnRate = CustomOption.Create(260, Cs(Cleaner.Instance.color, "Cleaner"), Rates, null, true);
            cleanerCooldown = CustomOption.Create(261, "Cleaner Cooldown", 30f, 10f, 60f, 2.5f, cleanerSpawnRate);

            warlockSpawnRate = CustomOption.Create(270, Cs(Cleaner.Instance.color, "Warlock"), Rates, null, true);
            warlockCooldown = CustomOption.Create(271, "Warlock Cooldown", 30f, 10f, 60f, 2.5f, warlockSpawnRate);
            warlockRootTime = CustomOption.Create(272, "Warlock Root Time", 5f, 0f, 15f, 1f, warlockSpawnRate);

            bountyHunterSpawnRate =
                CustomOption.Create(320, Cs(BountyHunter.Instance.color, "Bounty Hunter"), Rates, null, true);
            bountyHunterBountyDuration = CustomOption.Create(321, "Duration After Which Bounty Changes", 60f, 10f, 180f,
                10f, bountyHunterSpawnRate);
            bountyHunterReducedCooldown = CustomOption.Create(322, "Cooldown After Killing Bounty", 2.5f, 0f, 30f, 2.5f,
                bountyHunterSpawnRate);
            bountyHunterPunishmentTime = CustomOption.Create(323, "Additional Cooldown After Killing Others", 20f, 0f,
                60f, 2.5f, bountyHunterSpawnRate);
            bountyHunterShowArrow =
                CustomOption.Create(324, "Show Arrow Pointing Towards The Bounty", true, bountyHunterSpawnRate);
            bountyHunterArrowUpdateInterval = CustomOption.Create(325, "Arrow Update Interval", 15f, 2.5f, 60f, 2.5f,
                bountyHunterShowArrow);


            miniSpawnRate = CustomOption.Create(180, Cs(Mini.Instance.color, "Mini"), Rates, null, true);
            miniGrowingUpDuration =
                CustomOption.Create(181, "Mini Growing Up Duration", 400f, 100f, 1500f, 100f, miniSpawnRate);

            loversSpawnRate = CustomOption.Create(50, Cs(Lovers.Instance.color, "Lovers"), Rates, null, true);
            loversImpLoverRate = CustomOption.Create(51, "Chance That One Lover Is Impostor", Rates, loversSpawnRate);
            loversBothDie = CustomOption.Create(52, "Both Lovers Die", true, loversSpawnRate);
            loversCanHaveAnotherRole = CustomOption.Create(53, "Lovers Can Have Another Role", true, loversSpawnRate);

            guesserSpawnRate = CustomOption.Create(310, Cs(Guesser.Instance.color, "Guesser"), Rates, null, true);
            guesserIsImpGuesserRate =
                CustomOption.Create(311, "Chance That The Guesser Is An Impostor", Rates, guesserSpawnRate);
            guesserNumberOfShots =
                CustomOption.Create(312, "Guesser Number Of Shots", 2f, 1f, 15f, 1f, guesserSpawnRate);

            jesterSpawnRate = CustomOption.Create(60, Cs(Jester.Instance.color, "Jester"), Rates, null, true);
            jesterCanCallEmergency =
                CustomOption.Create(61, "Jester can call emergency meeting", true, jesterSpawnRate);
            jesterCanSabotage = CustomOption.Create(62, "Jester can sabotage", true, jesterSpawnRate);

            arsonistSpawnRate = CustomOption.Create(290, Cs(Arsonist.Instance.color, "Arsonist"), Rates, null, true);
            arsonistCooldown = CustomOption.Create(291, "Arsonist Cooldown", 12.5f, 2.5f, 60f, 2.5f, arsonistSpawnRate);
            arsonistDuration = CustomOption.Create(292, "Arsonist Douse Duration", 3f, 1f, 10f, 1f, arsonistSpawnRate);

            jackalSpawnRate = CustomOption.Create(220, Cs(Jackal.Instance.color, "Jackal"), Rates, null, true);
            jackalKillCooldown =
                CustomOption.Create(221, "Jackal/Sidekick Kill Cooldown", 30f, 10f, 60f, 2.5f, jackalSpawnRate);
            jackalCreateSidekickCooldown = CustomOption.Create(222, "Jackal Create Sidekick Cooldown", 30f, 10f, 60f,
                2.5f, jackalSpawnRate);
            jackalCanUseVents = CustomOption.Create(223, "Jackal Can Use Vents", true, jackalSpawnRate);
            jackalCanCreateSidekick = CustomOption.Create(224, "Jackal Can Create A Sidekick", false, jackalSpawnRate);
            sidekickPromotesToJackal = CustomOption.Create(225, "Sidekick Gets Promoted To Jackal On Jackal Death",
                false, jackalSpawnRate);
            sidekickCanKill = CustomOption.Create(226, "Sidekick Can Kill", false, jackalSpawnRate);
            sidekickCanUseVents = CustomOption.Create(227, "Sidekick Can Use Vents", true, jackalSpawnRate);
            recursiveSidekicks = CustomOption.Create(228,
                "Jackals Promoted From Sidekick Can Create A Sidekick", true, jackalSpawnRate);
            jackalCanCreateSidekickFromImpostor = CustomOption.Create(229,
                "Jackals Can Make An Impostor To His Sidekick", true, jackalSpawnRate);
            jackalAndSidekickHaveImpostorVision = CustomOption.Create(430, "Jackal And Sidekick Have Impostor Vision",
                false, jackalSpawnRate);

            shifterSpawnRate = CustomOption.Create(70, Cs(Shifter.Instance.color, "Shifter"), Rates, null, true);
            shifterShiftsModifiers = CustomOption.Create(71, "Shifter Shifts Modifiers", false, shifterSpawnRate);

            mayorSpawnRate = CustomOption.Create(80, Cs(Mayor.Instance.color, "Mayor"), Rates, null, true);

            engineerSpawnRate = CustomOption.Create(90, Cs(Engineer.Instance.color, "Engineer"), Rates, null, true);

            sheriffSpawnRate = CustomOption.Create(100, Cs(Sheriff.Instance.color, "Sheriff"), Rates, null, true);
            sheriffCooldown = CustomOption.Create(101, "Sheriff Cooldown", 30f, 10f, 60f, 2.5f, sheriffSpawnRate);
            sheriffCanKillNeutrals = CustomOption.Create(102, "Sheriff Can Kill Neutrals", false, sheriffSpawnRate);


            lighterSpawnRate = CustomOption.Create(110, Cs(Lighter.Instance.color, "Lighter"), Rates, null, true);
            lighterModeLightsOnVision = CustomOption.Create(111, "Lighter Mode Vision On Lights On", 2f, 0.25f, 5f,
                0.25f, lighterSpawnRate);
            lighterModeLightsOffVision = CustomOption.Create(112, "Lighter Mode Vision On Lights Off", 0.75f, 0.25f, 5f,
                0.25f, lighterSpawnRate);
            lighterCooldown = CustomOption.Create(113, "Lighter Cooldown", 30f, 5f, 120f, 5f, lighterSpawnRate);
            lighterDuration = CustomOption.Create(114, "Lighter Duration", 5f, 2.5f, 60f, 2.5f, lighterSpawnRate);

            detectiveSpawnRate = CustomOption.Create(120, Cs(Detective.Instance.color, "Detective"), Rates, null, true);
            detectiveAnonymousFootprints = CustomOption.Create(121, "Anonymous Footprints", false, detectiveSpawnRate);
            detectiveFootprintInterval =
                CustomOption.Create(122, "Footprint Interval", 0.5f, 0.25f, 10f, 0.25f, detectiveSpawnRate);
            detectiveFootprintDuration =
                CustomOption.Create(123, "Footprint Duration", 5f, 0.25f, 10f, 0.25f, detectiveSpawnRate);
            detectiveReportNameDuration = CustomOption.Create(124, "Time Where Detective Reports Will Have Name", 0, 0,
                60, 2.5f, detectiveSpawnRate);
            detectiveReportColorDuration = CustomOption.Create(125, "Time Where Detective Reports Will Have Color Type",
                20, 0, 120, 2.5f, detectiveSpawnRate);

            timeMasterSpawnRate =
                CustomOption.Create(130, Cs(TimeMaster.Instance.color, "Time Master"), Rates, null, true);
            timeMasterCooldown =
                CustomOption.Create(131, "Time Master Cooldown", 30f, 10f, 120f, 2.5f, timeMasterSpawnRate);
            timeMasterRewindTime = CustomOption.Create(132, "Rewind Time", 3f, 1f, 10f, 1f, timeMasterSpawnRate);
            timeMasterShieldDuration =
                CustomOption.Create(133, "Time Master Shield Duration", 3f, 1f, 20f, 1f, timeMasterSpawnRate);

            medicSpawnRate = CustomOption.Create(140, Cs(Medic.Instance.color, "Medic"), Rates, null, true);
            medicShowShielded = CustomOption.Create(143, "Show Shielded Player",
                new object[] {"Everyone", "Shielded + Medic", "Medic"}, medicSpawnRate);
            medicShowAttemptToShielded =
                CustomOption.Create(144, "Shielded Player Sees Murder Attempt", false, medicSpawnRate);
            medicSetShieldAfterMeeting =
                CustomOption.Create(145, "Shield Will Be Set After The Next Meeting", false, medicSpawnRate);

            swapperSpawnRate = CustomOption.Create(150, Cs(Swapper.Instance.color, "Swapper"), Rates, null, true);
            swapperCanCallEmergency =
                CustomOption.Create(151, "Swapper can call emergency meeting", false, swapperSpawnRate);
            swapperCanOnlySwapOthers =
                CustomOption.Create(152, "Swapper can only swap others", false, swapperSpawnRate);

            seerSpawnRate = CustomOption.Create(160, Cs(Seer.Instance.color, "Seer"), Rates, null, true);
            seerMode = CustomOption.Create(161, "Seer Mode",
                new object[] {"Show Death Flash + Souls", "Show Death Flash", "Show Souls"}, seerSpawnRate);
            seerLimitSoulDuration = CustomOption.Create(163, "Seer Limit Soul Duration", false, seerSpawnRate);
            seerSoulDuration = CustomOption.Create(162, "Seer Soul Duration", 15f, 0f, 60f, 5f, seerLimitSoulDuration);

            hackerSpawnRate = CustomOption.Create(170, Cs(Hacker.Instance.color, "Hacker"), Rates, null, true);
            hackerCooldown = CustomOption.Create(171, "Hacker Cooldown", 30f, 0f, 60f, 5f, hackerSpawnRate);
            hackerHackeringDuration =
                CustomOption.Create(172, "Hacker Duration", 10f, 2.5f, 60f, 2.5f, hackerSpawnRate);
            hackerOnlyColorType = CustomOption.Create(173, "Hacker Only Sees Color Type", false, hackerSpawnRate);

            trackerSpawnRate = CustomOption.Create(200, Cs(Tracker.Instance.color, "Tracker"), Rates, null, true);
            trackerUpdateInterval =
                CustomOption.Create(201, "Tracker Update Interval", 5f, 2.5f, 30f, 2.5f, trackerSpawnRate);
            trackerResetTargetAfterMeeting =
                CustomOption.Create(202, "Tracker Reset Target After Meeting", false, trackerSpawnRate);
            snitchSpawnRate = CustomOption.Create(210, Cs(Snitch.Instance.color, "Snitch"), Rates, null, true);
            snitchLeftTasksForReveal = CustomOption.Create(211, "Task Count Where The Snitch Will Be Revealed", 1f, 0f,
                5f, 1f, snitchSpawnRate);
            snitchIncludeTeamJackal = CustomOption.Create(212, "Include Team Jackal", false, snitchSpawnRate);
            snitchTeamJackalUseDifferentArrowColor = CustomOption.Create(213,
                "Use Different Arrow Color For Team Jackal", true, snitchIncludeTeamJackal);

            spySpawnRate = CustomOption.Create(240, Cs(Spy.Instance.color, "Spy"), Rates, null, true);
            spyCanDieToSheriff = CustomOption.Create(241, "Spy Can Die To Sheriff", false, spySpawnRate);
            spyImpostorsCanKillAnyone =
                CustomOption.Create(242, "Impostors Can Kill Anyone If There Is A Spy", true, spySpawnRate);
            spyCanEnterVents = CustomOption.Create(243, "Spy Can Enter Vents", false, spySpawnRate);
            spyHasImpostorVision = CustomOption.Create(244, "Spy Has Impostor Vision", false, spySpawnRate);

            securityGuardSpawnRate =
                CustomOption.Create(280, Cs(SecurityGuard.Instance.color, "Security Guard"), Rates, null, true);
            securityGuardCooldown = CustomOption.Create(281, "Security Guard Cooldown", 30f, 10f, 60f, 2.5f,
                securityGuardSpawnRate);
            securityGuardTotalScrews = CustomOption.Create(282, "Security Guard Number Of Screws", 7f, 1f, 15f, 1f,
                securityGuardSpawnRate);
            securityGuardCamPrice =
                CustomOption.Create(283, "Number Of Screws Per Cam", 2f, 1f, 15f, 1f, securityGuardSpawnRate);
            securityGuardVentPrice =
                CustomOption.Create(284, "Number Of Screws Per Vent", 1f, 1f, 15f, 1f, securityGuardSpawnRate);

            baitSpawnRate = CustomOption.Create(330, Cs(Bait.Instance.color, "Bait"), Rates, null, true);
            baitHighlightAllVents =
                CustomOption.Create(331, "Highlight All Vents If A Vent Is Occupied", false, baitSpawnRate);
            baitReportDelay = CustomOption.Create(332, "Bait Report Delay", 0f, 0f, 10f, 1f, baitSpawnRate);

            // Other options
            maxNumberOfMeetings = CustomOption.Create(3, "Number Of Meetings (excluding Mayor meeting)", 10, 0, 15, 1,
                null, true);
            blockSkippingInEmergencyMeetings = CustomOption.Create(4, "Block Skipping In Emergency Meetings", false);
            noVoteIsSelfVote = CustomOption.Create(5, "No Vote Is Self Vote", false, blockSkippingInEmergencyMeetings);
            hidePlayerNames = CustomOption.Create(6, "Hide Player Names", false);

            BlockedRolePairings.Add((byte) RoleId.Vampire, new[] {(byte) RoleId.Warlock});
            BlockedRolePairings.Add((byte) RoleId.Warlock, new[] {(byte) RoleId.Vampire});
            BlockedRolePairings.Add((byte) RoleId.Spy, new[] {(byte) RoleId.Mini});
            BlockedRolePairings.Add((byte) RoleId.Mini, new[] {(byte) RoleId.Spy});
        }
    }

    public class CustomOption
    {
        public static readonly List<CustomOption> Options = new();
        private static int preset;

        private readonly int _defaultSelection;

        public readonly int id;
        public readonly bool isHeader;
        public readonly string name;
        public readonly CustomOption parent;
        public readonly object[] selections;
        private ConfigEntry<int> _entry;
        public OptionBehaviour optionBehaviour;
        public int selection;

        // Option creation

        private CustomOption(int id, string name, object[] selections, object defaultValue, CustomOption parent,
            bool isHeader)
        {
            this.id = id;
            this.name = parent == null ? name : "- " + name;
            this.selections = selections;
            var index = Array.IndexOf(selections, defaultValue);
            _defaultSelection = index >= 0 ? index : 0;
            this.parent = parent;
            this.isHeader = isHeader;
            selection = 0;
            if (id != 0)
            {
                _entry = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", id.ToString(), _defaultSelection);
                selection = Mathf.Clamp(_entry.Value, 0, selections.Length - 1);
            }

            Options.Add(this);
        }

        public static CustomOption Create(int id, string name, object[] selections, CustomOption parent = null,
            bool isHeader = false)
        {
            return new(id, name, selections, "", parent, isHeader);
        }

        public static CustomOption Create(int id, string name, float defaultValue, float min, float max, float step,
            CustomOption parent = null, bool isHeader = false)
        {
            var selections = new List<float>();
            for (var s = min; s <= max; s += step)
                selections.Add(s);
            return new CustomOption(id, name, selections.Cast<object>().ToArray(), defaultValue, parent, isHeader);
        }

        public static CustomOption Create(int id, string name, bool defaultValue, CustomOption parent = null,
            bool isHeader = false)
        {
            return new(id, name, new object[] {"Off", "On"}, defaultValue ? "On" : "Off", parent,
                isHeader);
        }

        // Static behaviour

        private static void SwitchPreset(int newPreset)
        {
            preset = newPreset;
            foreach (var option in Options.Where(option => option.id != 0))
            {
                option._entry = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", option.id.ToString(),
                    option._defaultSelection);
                option.selection = Mathf.Clamp(option._entry.Value, 0, option.selections.Length - 1);
                if (option.optionBehaviour == null || !(option.optionBehaviour is StringOption stringOption)) continue;
                stringOption.oldValue = stringOption.Value = option.selection;
                stringOption.ValueText.text = option.selections[option.selection].ToString();
            }
        }

        public static void ShareOptionSelections()
        {
            if (PlayerControl.AllPlayerControls.Count <= 1 ||
                AmongUsClient.Instance && !AmongUsClient.Instance.AmHost && PlayerControl.LocalPlayer == null)
                return;
            foreach (var option in Options)
            {
                var messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.ShareOptionSelection, SendOption.Reliable);
                messageWriter.WritePacked((uint) option.id);
                messageWriter.WritePacked(Convert.ToUInt32(option.selection));
                messageWriter.EndMessage();
            }
        }

        // Getter

        public int GetSelection()
        {
            return selection;
        }

        public bool GetBool()
        {
            return selection > 0;
        }

        public float GetFloat()
        {
            return (float) selections[selection];
        }

        // Option changes

        public void UpdateSelection(int newSelection)
        {
            selection = Mathf.Clamp((newSelection + selections.Length) % selections.Length, 0, selections.Length - 1);
            if (optionBehaviour == null || optionBehaviour is not StringOption stringOption) return;
            stringOption.oldValue = stringOption.Value = selection;
            stringOption.ValueText.text = selections[selection].ToString();

            if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost || !PlayerControl.LocalPlayer) return;
            if (id == 0) SwitchPreset(selection); // Switch presets
            else if (_entry != null) _entry.Value = selection; // Save selection to config

            ShareOptionSelections(); // Share all selections
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    internal class GameOptionsMenuStartPatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            var template = Object.FindObjectsOfType<StringOption>().FirstOrDefault();
            if (template == null) return;

            var allOptions = __instance.Children.ToList();
            foreach (var option in CustomOption.Options)
            {
                if (option.optionBehaviour == null)
                {
                    var stringOption = Object.Instantiate(template, template.transform.parent);
                    allOptions.Add(stringOption);

                    stringOption.OnValueChanged = new Action<OptionBehaviour>(o => { });
                    stringOption.TitleText.text = option.name;
                    stringOption.Value = stringOption.oldValue = option.selection;
                    stringOption.ValueText.text = option.selections[option.selection].ToString();

                    option.optionBehaviour = stringOption;
                }

                option.optionBehaviour.gameObject.SetActive(true);
            }

            var commonTasksOption = allOptions.FirstOrDefault(x => x.name == "NumCommonTasks")?.TryCast<NumberOption>();
            if (commonTasksOption != null) commonTasksOption.ValidRange = new FloatRange(0f, 4f);

            var shortTasksOption = allOptions.FirstOrDefault(x => x.name == "NumShortTasks")?.TryCast<NumberOption>();
            if (shortTasksOption != null) shortTasksOption.ValidRange = new FloatRange(0f, 23f);

            var longTasksOption = allOptions.FirstOrDefault(x => x.name == "NumLongTasks")?.TryCast<NumberOption>();
            if (longTasksOption != null) longTasksOption.ValidRange = new FloatRange(0f, 15f);

            __instance.Children = allOptions.ToArray();
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
    public class StringOptionEnablePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            var option =
                CustomOption.Options.FirstOrDefault(customOption => customOption.optionBehaviour == __instance);
            if (option == null) return true;

            __instance.OnValueChanged = new Action<OptionBehaviour>(o => { });
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
            var option =
                CustomOption.Options.FirstOrDefault(customOption => customOption.optionBehaviour == __instance);
            if (option == null) return true;
            option.UpdateSelection(option.selection + 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
    public class StringOptionDecreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            var option =
                CustomOption.Options.FirstOrDefault(customOption => customOption.optionBehaviour == __instance);
            if (option == null) return true;
            option.UpdateSelection(option.selection - 1);
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
    internal class GameOptionsMenuUpdatePatch
    {
        private static float timer = 1f;

        public static void Postfix(GameOptionsMenu __instance)
        {
            __instance.GetComponentInParent<Scroller>().YBounds.max = -0.5F + __instance.Children.Length * 0.55F;
            timer += Time.deltaTime;
            if (timer < 0.1f) return;
            timer = 0f;

            var offset = -7.85f;
            foreach (var option in CustomOption.Options)
            {
                if (!option?.optionBehaviour || !option.optionBehaviour.gameObject) continue;
                var enabled = true;
                var parent = option.parent;
                while (parent != null && enabled)
                {
                    enabled = parent.selection != 0;
                    parent = parent.parent;
                }

                option.optionBehaviour.gameObject.SetActive(enabled);
                if (!enabled) continue;
                offset -= option.isHeader ? 0.75f : 0.5f;
                var pos = option.optionBehaviour.transform.localPosition;
                pos[1] = offset;
                option.optionBehaviour.transform.localPosition = pos;
            }
        }
    }

    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.OnEnable))]
    internal class GameSettingMenuPatch
    {
        public static void Prefix(GameSettingMenu __instance)
        {
            __instance.HideForOnline = new Transform[] { };
        }

        public static void Postfix(GameSettingMenu __instance)
        {
            var mapNameTransform = __instance.AllItems.FirstOrDefault(x =>
                x.gameObject.activeSelf && x.name.Equals("MapName", StringComparison.OrdinalIgnoreCase));
            if (mapNameTransform == null) return;

            var options =
                new Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.KeyValuePair<string, int>>();
            for (var i = 0; i < GameOptionsData.MapNames.Length; i++)
            {
                var kvp = new Il2CppSystem.Collections.Generic.KeyValuePair<string, int>
                {
                    key = GameOptionsData.MapNames[i], value = i
                };
                options.Add(kvp);
            }

            mapNameTransform.GetComponent<KeyValueOption>().Values = options;
        }
    }

    [HarmonyPatch(typeof(Constants), nameof(Constants.ShouldFlipSkeld))]
    internal class ConstantsShouldFlipSkeldPatch
    {
        public static bool Prefix(ref bool __result)
        {
            if (PlayerControl.GameOptions == null) return true;
            __result = PlayerControl.GameOptions.MapId == 3;
            return false;
        }
    }

    [HarmonyPatch]
    internal class GameOptionsDataPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(GameOptionsData).GetMethods().Where(x =>
                x.ReturnType == typeof(string) && x.GetParameters().Length == 1 &&
                x.GetParameters()[0].ParameterType == typeof(int));
        }

        private static void Postfix(ref string __result)
        {
            var sb = new StringBuilder(__result);
            foreach (var option in CustomOption.Options.Where(option => option.parent == null))
                if (option == CustomOptionHolder.crewmateRolesCountMin)
                {
                    var optionName = CustomOptionHolder.Cs(new Color(204f / 255f, 204f / 255f, 0, 1f),
                        "Crewmate Roles");
                    var min = CustomOptionHolder.crewmateRolesCountMin.GetSelection();
                    var max = CustomOptionHolder.crewmateRolesCountMax.GetSelection();
                    if (min > max) min = max;
                    var optionValue = min == max ? $"{max}" : $"{min} - {max}";
                    sb.AppendLine($"{optionName}: {optionValue}");
                }
                else if (option == CustomOptionHolder.neutralRolesCountMin)
                {
                    var optionName =
                        CustomOptionHolder.Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Neutral Roles");
                    var min = CustomOptionHolder.neutralRolesCountMin.GetSelection();
                    var max = CustomOptionHolder.neutralRolesCountMax.GetSelection();
                    if (min > max) min = max;
                    var optionValue = min == max ? $"{max}" : $"{min} - {max}";
                    sb.AppendLine($"{optionName}: {optionValue}");
                }
                else if (option == CustomOptionHolder.impostorRolesCountMin)
                {
                    var optionName = CustomOptionHolder.Cs(new Color(204f / 255f, 204f / 255f, 0, 1f),
                        "Impostor Roles");
                    var min = CustomOptionHolder.impostorRolesCountMin.GetSelection();
                    var max = CustomOptionHolder.impostorRolesCountMax.GetSelection();
                    if (min > max) min = max;
                    var optionValue = min == max ? $"{max}" : $"{min} - {max}";
                    sb.AppendLine($"{optionName}: {optionValue}");
                }
                else if (option == CustomOptionHolder.crewmateRolesCountMax ||
                         option == CustomOptionHolder.neutralRolesCountMax ||
                         option == CustomOptionHolder.impostorRolesCountMax)
                {
                }
                else
                {
                    sb.AppendLine($"{option.name}: {option.selections[option.selection]}");
                }

            CustomOption parent = null;
            foreach (var option in CustomOption.Options.Where(option => option.parent != null))
            {
                if (option.parent != parent)
                {
                    sb.AppendLine();
                    parent = option.parent;
                }

                sb.AppendLine($"{option.name}: {option.selections[option.selection]}");
            }

            var hudString = sb.ToString();

            var defaultSettingsLines = 19;
            var roleSettingsLines = defaultSettingsLines + 35;
            var detailedSettingsP1 = roleSettingsLines + 37;
            var detailedSettingsP2 = detailedSettingsP1 + 38;
            var end1 = hudString.TakeWhile(c => (defaultSettingsLines -= c == '\n' ? 1 : 0) > 0).Count();
            var end2 = hudString.TakeWhile(c => (roleSettingsLines -= c == '\n' ? 1 : 0) > 0).Count();
            var end3 = hudString.TakeWhile(c => (detailedSettingsP1 -= c == '\n' ? 1 : 0) > 0).Count();
            var end4 = hudString.TakeWhile(c => (detailedSettingsP2 -= c == '\n' ? 1 : 0) > 0).Count();
            var counter = TheOtherRolesPlugin.optionsPage;
            switch (counter)
            {
                case 0:
                    hudString = hudString.Substring(0, end1) + "\n";
                    break;
                case 1:
                {
                    hudString = hudString.Substring(end1 + 1, end2 - end1);
                    // Temporary fix, should add a new CustomOption for spaces
                    var gap = 1;
                    var index = hudString.TakeWhile(c => (gap -= c == '\n' ? 1 : 0) > 0).Count();
                    hudString = hudString.Insert(index, "\n");
                    gap = 5;
                    index = hudString.TakeWhile(c => (gap -= c == '\n' ? 1 : 0) > 0).Count();
                    hudString = hudString.Insert(index, "\n");
                    gap = 18;
                    index = hudString.TakeWhile(c => (gap -= c == '\n' ? 1 : 0) > 0).Count();
                    hudString = hudString.Insert(index + 1, "\n");
                    gap = 22;
                    index = hudString.TakeWhile(c => (gap -= c == '\n' ? 1 : 0) > 0).Count();
                    hudString = hudString.Insert(index + 1, "\n");
                    break;
                }
                case 2:
                    hudString = hudString.Substring(end2 + 1, end3 - end2);
                    break;
                case 3:
                    hudString = hudString.Substring(end3 + 1, end4 - end3);
                    break;
                case 4:
                    hudString = hudString.Substring(end4 + 1);
                    break;
            }

            hudString += $"\n Press tab for more... ({counter + 1}/5)";
            __result = hudString;
        }
    }

    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class GameOptionsNextPagePatch
    {
        public static void Postfix()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                TheOtherRolesPlugin.optionsPage = (TheOtherRolesPlugin.optionsPage + 1) % 5;
        }
    }


    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class GameSettingsScalePatch
    {
        public static void Prefix(HudManager __instance)
        {
            if (__instance.GameSettings != null) __instance.GameSettings.fontSize = 1.2f;
        }
    }
}