using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using static TheEpicRoles.TheEpicRoles;

namespace TheEpicRoles {
    static class MapOptions {
        // Set values
        public static int maxNumberOfMeetings = 10;
        public static bool blockSkippingInEmergencyMeetings = false;
        public static bool noVoteIsSelfVote = false;
        public static bool hidePlayerNames = false;
        public static bool ghostsSeeRoles = true;
        public static bool ghostsSeeTasks = true;
        public static bool ghostsSeeVotes = true;
        public static bool showRoleSummary = true;
        public static bool allowParallelMedBayScans = false;
        public static bool showLighterDarker = true;
        public static bool toggleCursor = true;
        public static bool toggleScreenShake = false;

        // Updating values
        public static int meetingsCount = 0;
        public static List<SurvCamera> camerasToAdd = new List<SurvCamera>();
        public static List<Vent> ventsToSeal = new List<Vent>();
        public static Dictionary<byte, PoolablePlayer> playerIcons = new Dictionary<byte, PoolablePlayer>();

public static void clearAndReloadMapOptions() {
            meetingsCount = 0;
            camerasToAdd = new List<SurvCamera>();
            ventsToSeal = new List<Vent>();
            playerIcons = new Dictionary<byte, PoolablePlayer>(); ;

            maxNumberOfMeetings = Mathf.RoundToInt(CustomOptionHolder.maxNumberOfMeetings.getSelection());
            blockSkippingInEmergencyMeetings = CustomOptionHolder.blockSkippingInEmergencyMeetings.getBool();
            noVoteIsSelfVote = CustomOptionHolder.noVoteIsSelfVote.getBool();
            hidePlayerNames = CustomOptionHolder.hidePlayerNames.getBool();
            allowParallelMedBayScans = CustomOptionHolder.allowParallelMedBayScans.getBool();
            ghostsSeeRoles = TheEpicRolesPlugin.GhostsSeeRoles.Value;
            ghostsSeeTasks = TheEpicRolesPlugin.GhostsSeeTasks.Value;
            ghostsSeeVotes = TheEpicRolesPlugin.GhostsSeeVotes.Value;
            showRoleSummary = TheEpicRolesPlugin.ShowRoleSummary.Value;
            showLighterDarker = TheEpicRolesPlugin.ShowLighterDarker.Value;
            toggleCursor = TheEpicRolesPlugin.ToggleCursor.Value;
            toggleScreenShake = TheEpicRolesPlugin.ToggleScreenShake.Value;
        }
    }
}