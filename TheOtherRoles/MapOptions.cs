using System.Collections.Generic;
using UnityEngine;

namespace TheOtherRoles
{
    internal static class MapOptions
    {
        // Set values
        public static int maxNumberOfMeetings = 10;
        public static bool blockSkippingInEmergencyMeetings;
        public static bool noVoteIsSelfVote;
        public static bool hidePlayerNames;
        public static bool ghostsSeeRoles = true;
        public static bool ghostsSeeTasks = true;
        public static bool ghostsSeeVotes = true;
        public static bool showRoleSummary = true;

        // Updating values
        public static int meetingsCount;
        public static List<SurvCamera> camerasToAdd = new();
        public static List<Vent> ventsToSeal = new();
        public static Dictionary<byte, PoolablePlayer> playerIcons = new();

        public static void ClearAndReloadMapOptions()
        {
            meetingsCount = 0;
            camerasToAdd.Clear();
            ventsToSeal.Clear();
            playerIcons.Clear();

            maxNumberOfMeetings = Mathf.RoundToInt(CustomOptionHolder.maxNumberOfMeetings.GetSelection());
            blockSkippingInEmergencyMeetings = CustomOptionHolder.blockSkippingInEmergencyMeetings.GetBool();
            noVoteIsSelfVote = CustomOptionHolder.noVoteIsSelfVote.GetBool();
            hidePlayerNames = CustomOptionHolder.hidePlayerNames.GetBool();
            ghostsSeeRoles = TheOtherRolesPlugin.GhostsSeeRoles.Value;
            ghostsSeeTasks = TheOtherRolesPlugin.GhostsSeeTasks.Value;
            ghostsSeeVotes = TheOtherRolesPlugin.GhostsSeeVotes.Value;
            showRoleSummary = TheOtherRolesPlugin.ShowRoleSummary.Value;
        }
    }
}