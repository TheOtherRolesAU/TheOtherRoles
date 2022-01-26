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
        public static string[] presets = new string[]{"Preset 1", "Preset 2", "Preset 3", "Preset 4", "Preset 5"};

        public static CustomOption presetSelection;
        public static CustomOption activateRoles;
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
        public static CustomOption loversEnableChat;

        public static CustomOption guesserSpawnRate;
        public static CustomOption guesserIsImpGuesserRate;
        public static CustomOption guesserNumberOfShots;
        public static CustomOption guesserHasMultipleShotsPerMeeting;
        public static CustomOption guesserShowInfoInGhostChat;
        public static CustomOption guesserKillsThroughShield;
        public static CustomOption guesserEvilCanKillSpy;
        public static CustomOption guesserSpawnBothRate;
        public static CustomOption guesserCantGuessSnitchIfTaksDone;

        public static CustomOption jesterSpawnRate;
        public static CustomOption jesterCanCallEmergency;
        public static CustomOption jesterHasImpostorVision;

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
        public static CustomOption jackalPromotedFromSidekickCanCreateSidekick;
        public static CustomOption jackalCanCreateSidekickFromImpostor;
        public static CustomOption jackalAndSidekickHaveImpostorVision;

        public static CustomOption bountyHunterSpawnRate;
        public static CustomOption bountyHunterBountyDuration;
        public static CustomOption bountyHunterReducedCooldown;
        public static CustomOption bountyHunterPunishmentTime;
        public static CustomOption bountyHunterShowArrow;
        public static CustomOption bountyHunterArrowUpdateIntervall;

        public static CustomOption witchSpawnRate;
        public static CustomOption witchCooldown;
        public static CustomOption witchAdditionalCooldown;
        public static CustomOption witchCanSpellAnyone;
        public static CustomOption witchSpellCastingDuration;
        public static CustomOption witchTriggerBothCooldowns;
        public static CustomOption witchVoteSavesTargets;

        public static CustomOption shifterSpawnRate;
        public static CustomOption shifterShiftsModifiers;

        public static CustomOption mayorSpawnRate;

        public static CustomOption engineerSpawnRate;
        public static CustomOption engineerNumberOfFixes;
        public static CustomOption engineerHighlightForImpostors;
        public static CustomOption engineerHighlightForTeamJackal;

        public static CustomOption sheriffSpawnRate;
        public static CustomOption sheriffCooldown;
        public static CustomOption sheriffCanKillNeutrals;
        public static CustomOption deputySpawnRate;

        public static CustomOption deputyNumberOfHandcuffs;
        public static CustomOption deputyHandcuffCooldown;
        public static CustomOption deputyGetsPromoted;
        public static CustomOption deputyKeepsHandcuffs;
        public static CustomOption deputyHandcuffDuration;
        public static CustomOption deputyKnowsSheriff;

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
        public static CustomOption medicSetShieldAfterMeeting;
        public static CustomOption medicShowAttemptToMedic;

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
        public static CustomOption hackerToolsNumber;
        public static CustomOption hackerRechargeTasksNumber;
        public static CustomOption hackerNoMove;

        public static CustomOption trackerSpawnRate;
        public static CustomOption trackerUpdateIntervall;
        public static CustomOption trackerResetTargetAfterMeeting;
        public static CustomOption trackerCanTrackCorpses;
        public static CustomOption trackerCorpsesTrackingCooldown;
        public static CustomOption trackerCorpsesTrackingDuration;

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
        public static CustomOption securityGuardCamDuration;
        public static CustomOption securityGuardCamMaxCharges;
        public static CustomOption securityGuardCamRechargeTasksNumber;
        public static CustomOption securityGuardNoMove;

        public static CustomOption baitSpawnRate;
        public static CustomOption baitHighlightAllVents;
        public static CustomOption baitReportDelay;
        public static CustomOption baitShowKillFlash;

        public static CustomOption vultureSpawnRate;
        public static CustomOption vultureCooldown;
        public static CustomOption vultureNumberToWin;
        public static CustomOption vultureCanUseVents;
        public static CustomOption vultureShowArrows;

        public static CustomOption mediumSpawnRate;
        public static CustomOption mediumCooldown;
        public static CustomOption mediumDuration;
        public static CustomOption mediumOneTimeUse;

        public static CustomOption lawyerSpawnRate;
        public static CustomOption lawyerTargetKnows;
        public static CustomOption lawyerWinsAfterMeetings;
        public static CustomOption lawyerNeededMeetings;
        public static CustomOption lawyerVision;
        public static CustomOption lawyerKnowsRole;
        public static CustomOption pursuerCooldown;
        public static CustomOption pursuerBlanksNumber;

        public static CustomOption maxNumberOfMeetings;
        public static CustomOption blockSkippingInEmergencyMeetings;
        public static CustomOption noVoteIsSelfVote;
        public static CustomOption hidePlayerNames;
        public static CustomOption allowParallelMedBayScans;

        public static CustomOption dynamicMap;
        public static CustomOption dynamicMapEnableSkeld;
        public static CustomOption dynamicMapEnableMira;
        public static CustomOption dynamicMapEnablePolus;
        public static CustomOption dynamicMapEnableDleks;
        public static CustomOption dynamicMapEnableAirShip;


        internal static Dictionary<byte, byte[]> blockedRolePairings = new Dictionary<byte, byte[]>();

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
            activateRoles = CustomOption.Create(1, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Enable Mod Roles And Block Vanilla Roles"), true, null, true);

            // Using new id's for the options to not break compatibilty with older versions
            crewmateRolesCountMin = CustomOption.Create(300, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Minimum Crewmate Roles"), 0f, 0f, 15f, 1f, null, true);
            crewmateRolesCountMax = CustomOption.Create(301, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Maximum Crewmate Roles"), 0f, 0f, 15f, 1f);
            neutralRolesCountMin = CustomOption.Create(302, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Minimum Neutral Roles"), 0f, 0f, 15f, 1f);
            neutralRolesCountMax = CustomOption.Create(303, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Maximum Neutral Roles"), 0f, 0f, 15f, 1f);
            impostorRolesCountMin = CustomOption.Create(304, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Minimum Impostor Roles"), 0f, 0f, 3f, 1f);
            impostorRolesCountMax = CustomOption.Create(305, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Maximum Impostor Roles"), 0f, 0f, 3f, 1f);

            mafiaSpawnRate = CustomOption.Create(10, cs(Janitor.color, "Mafia"), rates, null, true);
            janitorCooldown = CustomOption.Create(11, "Janitor Cooldown", 30f, 10f, 60f, 2.5f, mafiaSpawnRate);

            morphlingSpawnRate = CustomOption.Create(20, cs(Morphling.color, "Morphling"), rates, null, true);
            morphlingCooldown = CustomOption.Create(21, "Morphling Cooldown", 30f, 10f, 60f, 2.5f, morphlingSpawnRate);
            morphlingDuration = CustomOption.Create(22, "Morph Duration", 10f, 1f, 20f, 0.5f, morphlingSpawnRate);

            camouflagerSpawnRate = CustomOption.Create(30, cs(Camouflager.color, "Camouflager"), rates, null, true);
            camouflagerCooldown = CustomOption.Create(31, "Camouflager Cooldown", 30f, 10f, 60f, 2.5f, camouflagerSpawnRate);
            camouflagerDuration = CustomOption.Create(32, "Camo Duration", 10f, 1f, 20f, 0.5f, camouflagerSpawnRate);

            vampireSpawnRate = CustomOption.Create(40, cs(Vampire.color, "Vampire"), rates, null, true);
            vampireKillDelay = CustomOption.Create(41, "Vampire Kill Delay", 10f, 1f, 20f, 1f, vampireSpawnRate);
            vampireCooldown = CustomOption.Create(42, "Vampire Cooldown", 30f, 10f, 60f, 2.5f, vampireSpawnRate);
            vampireCanKillNearGarlics = CustomOption.Create(43, "Vampire Can Kill Near Garlics", true, vampireSpawnRate);

            eraserSpawnRate = CustomOption.Create(230, cs(Eraser.color, "Eraser"), rates, null, true);
            eraserCooldown = CustomOption.Create(231, "Eraser Cooldown", 30f, 10f, 120f, 5f, eraserSpawnRate);
            eraserCanEraseAnyone = CustomOption.Create(232, "Eraser Can Erase Anyone", false, eraserSpawnRate);

            tricksterSpawnRate = CustomOption.Create(250, cs(Trickster.color, "Trickster"), rates, null, true);
            tricksterPlaceBoxCooldown = CustomOption.Create(251, "Trickster Box Cooldown", 10f, 2.5f, 30f, 2.5f, tricksterSpawnRate);
            tricksterLightsOutCooldown = CustomOption.Create(252, "Trickster Lights Out Cooldown", 30f, 10f, 60f, 5f, tricksterSpawnRate);
            tricksterLightsOutDuration = CustomOption.Create(253, "Trickster Lights Out Duration", 15f, 5f, 60f, 2.5f, tricksterSpawnRate);

            cleanerSpawnRate = CustomOption.Create(260, cs(Cleaner.color, "Cleaner"), rates, null, true);
            cleanerCooldown = CustomOption.Create(261, "Cleaner Cooldown", 30f, 10f, 60f, 2.5f, cleanerSpawnRate);

            warlockSpawnRate = CustomOption.Create(270, cs(Cleaner.color, "Warlock"), rates, null, true);
            warlockCooldown = CustomOption.Create(271, "Warlock Cooldown", 30f, 10f, 60f, 2.5f, warlockSpawnRate);
            warlockRootTime = CustomOption.Create(272, "Warlock Root Time", 5f, 0f, 15f, 1f, warlockSpawnRate);

            bountyHunterSpawnRate = CustomOption.Create(320, cs(BountyHunter.color, "Bounty Hunter"), rates, null, true);
            bountyHunterBountyDuration = CustomOption.Create(321, "Duration After Which Bounty Changes",  60f, 10f, 180f, 10f, bountyHunterSpawnRate);
            bountyHunterReducedCooldown = CustomOption.Create(322, "Cooldown After Killing Bounty", 2.5f, 0f, 30f, 2.5f, bountyHunterSpawnRate);
            bountyHunterPunishmentTime = CustomOption.Create(323, "Additional Cooldown After Killing Others", 20f, 0f, 60f, 2.5f, bountyHunterSpawnRate);
            bountyHunterShowArrow = CustomOption.Create(324, "Show Arrow Pointing Towards The Bounty", true, bountyHunterSpawnRate);
            bountyHunterArrowUpdateIntervall = CustomOption.Create(325, "Arrow Update Intervall", 15f, 2.5f, 60f, 2.5f, bountyHunterShowArrow);

            witchSpawnRate = CustomOption.Create(370, cs(Witch.color, "Witch"), rates, null, true);
            witchCooldown = CustomOption.Create(371, "Witch Spell Casting Cooldown", 30f, 10f, 120f, 5f, witchSpawnRate);
            witchAdditionalCooldown = CustomOption.Create(372, "Witch Additional Cooldown", 10f, 0f, 60f, 5f, witchSpawnRate);
            witchCanSpellAnyone = CustomOption.Create(373, "Witch Can Spell Anyone", false, witchSpawnRate);
            witchSpellCastingDuration = CustomOption.Create(374, "Spell Casting Duration", 1f, 0f, 10f, 1f, witchSpawnRate);
            witchTriggerBothCooldowns = CustomOption.Create(375, "Trigger Both Cooldowns", true, witchSpawnRate);
            witchVoteSavesTargets = CustomOption.Create(376, "Voting The Witch Saves All The Targets", true, witchSpawnRate);

            miniSpawnRate = CustomOption.Create(180, cs(Mini.color, "Mini"), rates, null, true);
            miniGrowingUpDuration = CustomOption.Create(181, "Mini Growing Up Duration", 400f, 100f, 1500f, 100f, miniSpawnRate);

            loversSpawnRate = CustomOption.Create(50, cs(Lovers.color, "Lovers"), rates, null, true);
            loversImpLoverRate = CustomOption.Create(51, "Chance That One Lover Is Impostor", rates, loversSpawnRate);
            loversBothDie = CustomOption.Create(52, "Both Lovers Die", true, loversSpawnRate);
            loversCanHaveAnotherRole = CustomOption.Create(53, "Lovers Can Have Another Role", true, loversSpawnRate);
            loversEnableChat = CustomOption.Create(54, "Enable Lover Chat", true, loversSpawnRate);

            guesserSpawnRate = CustomOption.Create(310, cs(Guesser.color, "Guesser"), rates, null, true);
            guesserIsImpGuesserRate = CustomOption.Create(311, "Chance That The Guesser Is An Impostor", rates, guesserSpawnRate);
            guesserNumberOfShots = CustomOption.Create(312, "Guesser Number Of Shots", 2f, 1f, 15f, 1f, guesserSpawnRate);
            guesserHasMultipleShotsPerMeeting = CustomOption.Create(313, "Guesser Can Shoot Multiple Times Per Meeting", false, guesserSpawnRate);
            guesserShowInfoInGhostChat = CustomOption.Create(314, "Guesses Visible In Ghost Chat", true, guesserSpawnRate);
            guesserKillsThroughShield  = CustomOption.Create(315, "Guesses Ignore The Medic Shield", true, guesserSpawnRate);
            guesserEvilCanKillSpy  = CustomOption.Create(316, "Evil Guesser Can Guess The Spy", true, guesserSpawnRate);
            guesserSpawnBothRate = CustomOption.Create(317, "Both Guesser Spawn Rate", rates, guesserSpawnRate);
            guesserCantGuessSnitchIfTaksDone = CustomOption.Create(318, "Guesser Can't Guess Snitch When Tasks Completed", true, guesserSpawnRate);

            jesterSpawnRate = CustomOption.Create(60, cs(Jester.color, "Jester"), rates, null, true);
            jesterCanCallEmergency = CustomOption.Create(61, "Jester Can Call Emergency Meeting", true, jesterSpawnRate);
            jesterHasImpostorVision = CustomOption.Create(62, "Jester Has Impostor Vision", false, jesterSpawnRate);

            arsonistSpawnRate = CustomOption.Create(290, cs(Arsonist.color, "Arsonist"), rates, null, true);
            arsonistCooldown = CustomOption.Create(291, "Arsonist Cooldown", 12.5f, 2.5f, 60f, 2.5f, arsonistSpawnRate);
            arsonistDuration = CustomOption.Create(292, "Arsonist Douse Duration", 3f, 1f, 10f, 1f, arsonistSpawnRate);

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
            jackalAndSidekickHaveImpostorVision = CustomOption.Create(430, "Jackal And Sidekick Have Impostor Vision", false, jackalSpawnRate);

            vultureSpawnRate = CustomOption.Create(340, cs(Vulture.color, "Vulture"), rates, null, true);
            vultureCooldown = CustomOption.Create(341, "Vulture Cooldown", 15f, 10f, 60f, 2.5f, vultureSpawnRate);
            vultureNumberToWin = CustomOption.Create(342, "Number Of Corpses Needed To Be Eaten", 4f, 1f, 10f, 1f, vultureSpawnRate);
            vultureCanUseVents = CustomOption.Create(343, "Vulture Can Use Vents", true, vultureSpawnRate);
            vultureShowArrows = CustomOption.Create(344, "Show Arrows Pointing Towards The Corpses", true, vultureSpawnRate);

            lawyerSpawnRate = CustomOption.Create(350, cs(Lawyer.color, "Lawyer"), rates, null, true);
            lawyerTargetKnows = CustomOption.Create(351, "Lawyer Target Knows", true, lawyerSpawnRate);
            lawyerWinsAfterMeetings = CustomOption.Create(352, "Lawyer Wins After Meetings", false, lawyerSpawnRate);
            lawyerNeededMeetings = CustomOption.Create(353, "Lawyer Needed Meetings To Win", 5f, 1f, 15f, 1f, lawyerWinsAfterMeetings);
            lawyerVision = CustomOption.Create(354, "Lawyer Vision", 1f, 0.25f, 3f, 0.25f, lawyerSpawnRate);
            lawyerKnowsRole = CustomOption.Create(355, "Lawyer Knows Target Role", false, lawyerSpawnRate);
            pursuerCooldown = CustomOption.Create(356, "Pursuer Blank Cooldown", 30f, 5f, 60f, 2.5f, lawyerSpawnRate);
            pursuerBlanksNumber = CustomOption.Create(357, "Pursuer Number Of Blanks", 5f, 1f, 20f, 1f, lawyerSpawnRate);

            shifterSpawnRate = CustomOption.Create(70, cs(Shifter.color, "Shifter"), rates, null, true);
            shifterShiftsModifiers = CustomOption.Create(71, "Shifter Shifts Modifiers", false, shifterSpawnRate);

            mayorSpawnRate = CustomOption.Create(80, cs(Mayor.color, "Mayor"), rates, null, true);

            engineerSpawnRate = CustomOption.Create(90, cs(Engineer.color, "Engineer"), rates, null, true);
            engineerNumberOfFixes = CustomOption.Create(91, "Number Of Sabotage Fixes", 1f, 1f, 3f, 1f, engineerSpawnRate);
            engineerHighlightForImpostors = CustomOption.Create(92, "Impostors See Vents Highlighted", true, engineerSpawnRate);
            engineerHighlightForTeamJackal = CustomOption.Create(93, "Jackal and Sidekick See Vents Highlighted ", true, engineerSpawnRate);

            sheriffSpawnRate = CustomOption.Create(100, cs(Sheriff.color, "Sheriff"), rates, null, true);
            sheriffCooldown = CustomOption.Create(101, "Sheriff Cooldown", 30f, 10f, 60f, 2.5f, sheriffSpawnRate);
            sheriffCanKillNeutrals = CustomOption.Create(102, "Sheriff Can Kill Neutrals", false, sheriffSpawnRate);
            deputySpawnRate = CustomOption.Create(103, "Sheriff Has A Deputy", rates, sheriffSpawnRate);
            deputyNumberOfHandcuffs = CustomOption.Create(104, "Deputy Number Of Handcuffs", 3f, 1f, 10f, 1f, deputySpawnRate);
            deputyHandcuffCooldown = CustomOption.Create(105, "Handcuff Cooldown", 30f, 10f, 60f, 2.5f, deputySpawnRate);
            deputyHandcuffDuration = CustomOption.Create(106, "Handcuff Duration", 15f, 5f, 60f, 2.5f, deputySpawnRate);
            deputyKnowsSheriff = CustomOption.Create(107, "Sheriff And Deputy Know Each Other ", true, deputySpawnRate);
            deputyGetsPromoted = CustomOption.Create(108, "Deputy Gets Promoted To Sheriff", new string[] { "Off", "On (Immediately)", "On (After Meeting)" }, deputySpawnRate);
            deputyKeepsHandcuffs = CustomOption.Create(109, "Deputy Keeps Handcuffs When Promoted", true, deputyGetsPromoted);

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
            medicSetShieldAfterMeeting = CustomOption.Create(145, "Shield Will Be Set After The Next Meeting", false, medicSpawnRate);
            medicShowAttemptToMedic = CustomOption.Create(146, "Medic Sees Murder Attempt On Shielded Player", false, medicSpawnRate);

            swapperSpawnRate = CustomOption.Create(150, cs(Swapper.color, "Swapper"), rates, null, true);
            swapperCanCallEmergency = CustomOption.Create(151, "Swapper can call emergency meeting", false, swapperSpawnRate);
            swapperCanOnlySwapOthers = CustomOption.Create(152, "Swapper can only swap others", false, swapperSpawnRate);

            seerSpawnRate = CustomOption.Create(160, cs(Seer.color, "Seer"), rates, null, true);
            seerMode = CustomOption.Create(161, "Seer Mode", new string[]{ "Show Death Flash + Souls", "Show Death Flash", "Show Souls"}, seerSpawnRate);
            seerLimitSoulDuration = CustomOption.Create(163, "Seer Limit Soul Duration", false, seerSpawnRate);
            seerSoulDuration = CustomOption.Create(162, "Seer Soul Duration", 15f, 0f, 120f, 5f, seerLimitSoulDuration);
        
            hackerSpawnRate = CustomOption.Create(170, cs(Hacker.color, "Hacker"), rates, null, true);
            hackerCooldown = CustomOption.Create(171, "Hacker Cooldown", 30f, 5f, 60f, 5f, hackerSpawnRate);
            hackerHackeringDuration = CustomOption.Create(172, "Hacker Duration", 10f, 2.5f, 60f, 2.5f, hackerSpawnRate);
            hackerOnlyColorType = CustomOption.Create(173, "Hacker Only Sees Color Type", false, hackerSpawnRate);
            hackerToolsNumber = CustomOption.Create(174, "Max Mobile Gadget Charges", 5f, 1f, 30f, 1f, hackerSpawnRate);
            hackerRechargeTasksNumber = CustomOption.Create(175, "Number Of Tasks Needed For Recharging", 2f, 1f, 5f, 1f, hackerSpawnRate);
            hackerNoMove = CustomOption.Create(176, "Cant Move During Mobile Gadget Duration", true, hackerSpawnRate);

            trackerSpawnRate = CustomOption.Create(200, cs(Tracker.color, "Tracker"), rates, null, true);
            trackerUpdateIntervall = CustomOption.Create(201, "Tracker Update Intervall", 5f, 1f, 30f, 1f, trackerSpawnRate);
            trackerResetTargetAfterMeeting = CustomOption.Create(202, "Tracker Reset Target After Meeting", false, trackerSpawnRate);
            trackerCanTrackCorpses = CustomOption.Create(203, "Tracker Can Track Corpses", true, trackerSpawnRate);
            trackerCorpsesTrackingCooldown = CustomOption.Create(204, "Corpses Tracking Cooldown", 30f, 5f, 120f, 5f, trackerCanTrackCorpses);
            trackerCorpsesTrackingDuration = CustomOption.Create(205, "Corpses Tracking Duration", 5f, 2.5f, 30f, 2.5f, trackerCanTrackCorpses);
                           
            snitchSpawnRate = CustomOption.Create(210, cs(Snitch.color, "Snitch"), rates, null, true);
            snitchLeftTasksForReveal = CustomOption.Create(211, "Task Count Where The Snitch Will Be Revealed", 1f, 0f, 5f, 1f, snitchSpawnRate);
            snitchIncludeTeamJackal = CustomOption.Create(212, "Include Team Jackal", false, snitchSpawnRate);
            snitchTeamJackalUseDifferentArrowColor = CustomOption.Create(213, "Use Different Arrow Color For Team Jackal", true, snitchIncludeTeamJackal);

            spySpawnRate = CustomOption.Create(240, cs(Spy.color, "Spy"), rates, null, true);
            spyCanDieToSheriff = CustomOption.Create(241, "Spy Can Die To Sheriff", false, spySpawnRate);
            spyImpostorsCanKillAnyone = CustomOption.Create(242, "Impostors Can Kill Anyone If There Is A Spy", true, spySpawnRate);
            spyCanEnterVents = CustomOption.Create(243, "Spy Can Enter Vents", false, spySpawnRate);
            spyHasImpostorVision = CustomOption.Create(244, "Spy Has Impostor Vision", false, spySpawnRate);

            securityGuardSpawnRate = CustomOption.Create(280, cs(SecurityGuard.color, "Security Guard"), rates, null, true);
            securityGuardCooldown = CustomOption.Create(281, "Security Guard Cooldown", 30f, 10f, 60f, 2.5f, securityGuardSpawnRate);
            securityGuardTotalScrews = CustomOption.Create(282, "Security Guard Number Of Screws", 7f, 1f, 15f, 1f, securityGuardSpawnRate);
            securityGuardCamPrice = CustomOption.Create(283, "Number Of Screws Per Cam", 2f, 1f, 15f, 1f, securityGuardSpawnRate);
            securityGuardVentPrice = CustomOption.Create(284, "Number Of Screws Per Vent", 1f, 1f, 15f, 1f, securityGuardSpawnRate);
            securityGuardCamDuration = CustomOption.Create(285, "Security Guard Duration", 10f, 2.5f, 60f, 2.5f, securityGuardSpawnRate);
            securityGuardCamMaxCharges = CustomOption.Create(286, "Gadged Max Charges", 5f, 1f, 30f, 1f, securityGuardSpawnRate);
            securityGuardCamRechargeTasksNumber = CustomOption.Create(287, "Number Of Tasks Needed For Recharging", 3f, 1f, 10f, 1f, securityGuardSpawnRate);
            securityGuardNoMove = CustomOption.Create(288, "Cant Move During Cam Duration", true, securityGuardSpawnRate);

            baitSpawnRate = CustomOption.Create(330, cs(Bait.color, "Bait"), rates, null, true);
            baitHighlightAllVents = CustomOption.Create(331, "Highlight All Vents If A Vent Is Occupied", false, baitSpawnRate);
            baitReportDelay = CustomOption.Create(332, "Bait Report Delay", 0f, 0f, 10f, 1f, baitSpawnRate);
            baitShowKillFlash = CustomOption.Create(333, "Warn The Killer With A Flash", true, baitSpawnRate);

            mediumSpawnRate = CustomOption.Create(360, cs(Medium.color, "Medium"), rates, null, true);
            mediumCooldown = CustomOption.Create(361, "Medium Questioning Cooldown", 30f, 5f, 120f, 5f, mediumSpawnRate);
            mediumDuration = CustomOption.Create(362, "Medium Questioning Duration", 3f, 0f, 15f, 1f, mediumSpawnRate);
            mediumOneTimeUse = CustomOption.Create(363, "Each Soul Can Only Be Questioned Once", false, mediumSpawnRate);

            // Other options
            maxNumberOfMeetings = CustomOption.Create(3, "Number Of Meetings (excluding Mayor meeting)", 10, 0, 15, 1, null, true);
            blockSkippingInEmergencyMeetings = CustomOption.Create(4, "Block Skipping In Emergency Meetings", false);
            noVoteIsSelfVote = CustomOption.Create(5, "No Vote Is Self Vote", false, blockSkippingInEmergencyMeetings);
            hidePlayerNames = CustomOption.Create(6, "Hide Player Names", false);
            allowParallelMedBayScans = CustomOption.Create(7, "Allow Parallel MedBay Scans", false);

            dynamicMap = CustomOption.Create(8, "Play On A Random Map", false, null, false);
            dynamicMapEnableSkeld = CustomOption.Create(501, "Enable Skeld Rotation", true, dynamicMap, false);
            dynamicMapEnableMira = CustomOption.Create(502, "Enable Mira Rotation", true, dynamicMap, false);
            dynamicMapEnablePolus = CustomOption.Create(503, "Enable Polus Rotation", true, dynamicMap, false);
            dynamicMapEnableAirShip = CustomOption.Create(504, "Enable Airship Rotation", true, dynamicMap, false);
            dynamicMapEnableDleks = CustomOption.Create(505, "Enable dlekS Rotation", false, dynamicMap, false);

            blockedRolePairings.Add((byte)RoleId.Vampire, new [] { (byte)RoleId.Warlock});
            blockedRolePairings.Add((byte)RoleId.Warlock, new [] { (byte)RoleId.Vampire});
            blockedRolePairings.Add((byte)RoleId.Spy, new [] { (byte)RoleId.Mini});
            blockedRolePairings.Add((byte)RoleId.Mini, new [] { (byte)RoleId.Spy});
            
        }
    }
}
