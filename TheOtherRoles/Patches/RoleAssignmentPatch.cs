using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using UnhollowerBaseLib;
using UnityEngine;
using static TheOtherRoles.RoleReloader;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
    internal class SetInfectedPatch
    {
        public static void Postfix([HarmonyArgument(0)] Il2CppReferenceArray<GameData.PlayerInfo> infected)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRPC.ResetVaribles, SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.ResetVariables();

            if (!DestroyableSingleton<TutorialManager>.InstanceExists) // Don't assign Roles in Tutorial
                AssignRoles();
        }

        private static void AssignRoles()
        {
            var data = GetRoleAssignmentData();
            AssignSpecialRoles(
                data); // Assign special roles like mafia and lovers first as they assign a role to multiple players and the chances are independent of the ticket system
            SelectFactionForFactionIndependentRoles(data);
            AssignEnsuredRoles(data); // Assign roles that should always be in the game next
            AssignChanceRoles(data); // Assign roles that may or may not be in the game last
        }

        private static RoleAssignmentData GetRoleAssignmentData()
        {
            // Get the players that we want to assign the roles to. Crewmate and Neutral roles are assigned to natural crewmates. Impostor roles to impostors.
            var crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(_ => Guid.NewGuid()).ToList();
            crewmates.RemoveAll(x => x.Data.IsImpostor);
            var impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(_ => Guid.NewGuid()).ToList();
            impostors.RemoveAll(x => !x.Data.IsImpostor);

            var crewmateMin = CustomOptionHolder.crewmateRolesCountMin.GetSelection();
            var crewmateMax = CustomOptionHolder.crewmateRolesCountMax.GetSelection();
            var neutralMin = CustomOptionHolder.neutralRolesCountMin.GetSelection();
            var neutralMax = CustomOptionHolder.neutralRolesCountMax.GetSelection();
            var impostorMin = CustomOptionHolder.impostorRolesCountMin.GetSelection();
            var impostorMax = CustomOptionHolder.impostorRolesCountMax.GetSelection();

            // Make sure min is less or equal to max
            if (crewmateMin > crewmateMax) crewmateMin = crewmateMax;
            if (neutralMin > neutralMax) neutralMin = neutralMax;
            if (impostorMin > impostorMax) impostorMin = impostorMax;

            // Get the maximum allowed count of each role type based on the minimum and maximum option
            var crewCountSettings = Rng.Next(crewmateMin, crewmateMax + 1);
            var neutralCountSettings = Rng.Next(neutralMin, neutralMax + 1);
            var impCountSettings = Rng.Next(impostorMin, impostorMax + 1);

            // Potentially lower the actual maximum to the assignable players
            var maxCrewmateRoles = Mathf.Min(crewmates.Count, crewCountSettings);
            var maxNeutralRoles = Mathf.Min(crewmates.Count, neutralCountSettings);
            var maxImpostorRoles = Mathf.Min(impostors.Count, impCountSettings);

            // Fill in the lists with the roles that should be assigned to players. Note that the special roles (like Mafia or Lovers) are NOT included in these lists
            var impSettings = new Dictionary<byte, int>();
            var neutralSettings = new Dictionary<byte, int>();
            var crewSettings = new Dictionary<byte, int>();

            impSettings.Add((byte) RoleId.Morphling, CustomOptionHolder.morphlingSpawnRate.GetSelection());
            impSettings.Add((byte) RoleId.Camouflager, CustomOptionHolder.camouflagerSpawnRate.GetSelection());
            impSettings.Add((byte) RoleId.Vampire, CustomOptionHolder.vampireSpawnRate.GetSelection());
            impSettings.Add((byte) RoleId.Eraser, CustomOptionHolder.eraserSpawnRate.GetSelection());
            impSettings.Add((byte) RoleId.Trickster, CustomOptionHolder.tricksterSpawnRate.GetSelection());
            impSettings.Add((byte) RoleId.Cleaner, CustomOptionHolder.cleanerSpawnRate.GetSelection());
            impSettings.Add((byte) RoleId.Warlock, CustomOptionHolder.warlockSpawnRate.GetSelection());
            impSettings.Add((byte) RoleId.BountyHunter, CustomOptionHolder.bountyHunterSpawnRate.GetSelection());

            neutralSettings.Add((byte) RoleId.Jester, CustomOptionHolder.jesterSpawnRate.GetSelection());
            neutralSettings.Add((byte) RoleId.Arsonist, CustomOptionHolder.arsonistSpawnRate.GetSelection());
            neutralSettings.Add((byte) RoleId.Jackal, CustomOptionHolder.jackalSpawnRate.GetSelection());

            crewSettings.Add((byte) RoleId.Mayor, CustomOptionHolder.mayorSpawnRate.GetSelection());
            crewSettings.Add((byte) RoleId.Engineer, CustomOptionHolder.engineerSpawnRate.GetSelection());
            crewSettings.Add((byte) RoleId.Sheriff, CustomOptionHolder.sheriffSpawnRate.GetSelection());
            crewSettings.Add((byte) RoleId.Lighter, CustomOptionHolder.lighterSpawnRate.GetSelection());
            crewSettings.Add((byte) RoleId.Detective, CustomOptionHolder.detectiveSpawnRate.GetSelection());
            crewSettings.Add((byte) RoleId.TimeMaster, CustomOptionHolder.timeMasterSpawnRate.GetSelection());
            crewSettings.Add((byte) RoleId.Medic, CustomOptionHolder.medicSpawnRate.GetSelection());
            crewSettings.Add((byte) RoleId.Shifter, CustomOptionHolder.shifterSpawnRate.GetSelection());
            crewSettings.Add((byte) RoleId.Swapper, CustomOptionHolder.swapperSpawnRate.GetSelection());
            crewSettings.Add((byte) RoleId.Seer, CustomOptionHolder.seerSpawnRate.GetSelection());
            crewSettings.Add((byte) RoleId.Hacker, CustomOptionHolder.hackerSpawnRate.GetSelection());
            crewSettings.Add((byte) RoleId.Tracker, CustomOptionHolder.trackerSpawnRate.GetSelection());
            crewSettings.Add((byte) RoleId.Snitch, CustomOptionHolder.snitchSpawnRate.GetSelection());
            crewSettings.Add((byte) RoleId.Bait, CustomOptionHolder.baitSpawnRate.GetSelection());
            if (impostors.Count > 1) // Only add Spy if more than 1 impostor as the spy role is otherwise useless
                crewSettings.Add((byte) RoleId.Spy, CustomOptionHolder.spySpawnRate.GetSelection());
            crewSettings.Add((byte) RoleId.SecurityGuard, CustomOptionHolder.securityGuardSpawnRate.GetSelection());

            return new RoleAssignmentData
            {
                crewmates = crewmates,
                impostors = impostors,
                crewSettings = crewSettings,
                neutralSettings = neutralSettings,
                impSettings = impSettings,
                maxCrewmateRoles = maxCrewmateRoles,
                maxNeutralRoles = maxNeutralRoles,
                maxImpostorRoles = maxImpostorRoles
            };
        }

        private static void AssignSpecialRoles(RoleAssignmentData data)
        {
            // Assign Lovers
            if (Rng.Next(1, 101) <= CustomOptionHolder.loversSpawnRate.GetSelection() * 10)
            {
                var isOnlyRole = !CustomOptionHolder.loversCanHaveAnotherRole.GetBool();
                if (data.impostors.Count > 0 && data.crewmates.Count > 0 &&
                    (!isOnlyRole || data.maxCrewmateRoles > 0 && data.maxImpostorRoles > 0) &&
                    Rng.Next(1, 101) <= CustomOptionHolder.loversImpLoverRate.GetSelection() * 10)
                {
                    SetRoleToRandomPlayer((byte) RoleId.Lover, data.impostors, 0, isOnlyRole);
                    SetRoleToRandomPlayer((byte) RoleId.Lover, data.crewmates, 1, isOnlyRole);
                    if (isOnlyRole)
                    {
                        data.maxCrewmateRoles--;
                        data.maxImpostorRoles--;
                    }
                }
                else if (data.crewmates.Count >= 2 && (isOnlyRole || data.maxCrewmateRoles >= 2))
                {
                    var firstLoverId = SetRoleToRandomPlayer((byte) RoleId.Lover, data.crewmates, 0, isOnlyRole);
                    if (isOnlyRole)
                    {
                        SetRoleToRandomPlayer((byte) RoleId.Lover, data.crewmates, 1);
                        data.maxCrewmateRoles -= 2;
                    }
                    else
                    {
                        var crewmatesWithoutFirstLover = data.crewmates.ToList();
                        crewmatesWithoutFirstLover.RemoveAll(p => p.PlayerId == firstLoverId);
                        SetRoleToRandomPlayer((byte) RoleId.Lover, crewmatesWithoutFirstLover, 1, false);
                    }
                }
            }

            // Assign Mafia
            if (data.impostors.Count >= 3 && data.maxImpostorRoles >= 3 &&
                Rng.Next(1, 101) <= CustomOptionHolder.mafiaSpawnRate.GetSelection() * 10)
            {
                SetRoleToRandomPlayer((byte) RoleId.Godfather, data.impostors);
                SetRoleToRandomPlayer((byte) RoleId.Janitor, data.impostors);
                SetRoleToRandomPlayer((byte) RoleId.Mafioso, data.impostors);
                data.maxImpostorRoles -= 3;
            }
        }

        private static void SelectFactionForFactionIndependentRoles(RoleAssignmentData data)
        {
            // Assign Mini (33% chance impostor / 67% chance crewmate)
            if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && Rng.Next(1, 101) <= 33)
                data.impSettings.Add((byte) RoleId.Mini, CustomOptionHolder.miniSpawnRate.GetSelection());
            else if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0)
                data.crewSettings.Add((byte) RoleId.Mini, CustomOptionHolder.miniSpawnRate.GetSelection());

            // Assign Guesser (chance to be impostor based on setting)
            if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 &&
                Rng.Next(1, 101) <= CustomOptionHolder.guesserIsImpGuesserRate.GetSelection() * 10)
                data.impSettings.Add((byte) RoleId.Guesser, CustomOptionHolder.guesserSpawnRate.GetSelection());
            else if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0)
                data.crewSettings.Add((byte) RoleId.Guesser, CustomOptionHolder.guesserSpawnRate.GetSelection());
        }

        private static void AssignEnsuredRoles(RoleAssignmentData data)
        {
            // Get all roles where the chance to occur is set to 100%
            var ensuredCrewmateRoles = data.crewSettings.Where(x => x.Value == 10).Select(x => x.Key).ToList();
            var ensuredNeutralRoles = data.neutralSettings.Where(x => x.Value == 10).Select(x => x.Key).ToList();
            var ensuredImpostorRoles = data.impSettings.Where(x => x.Value == 10).Select(x => x.Key).ToList();

            // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
            while (
                data.impostors.Count > 0 && data.maxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0 ||
                data.crewmates.Count > 0 && (
                    data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0 ||
                    data.maxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0
                ))
            {
                var rolesToAssign = new Dictionary<RoleType, List<byte>>();
                if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0)
                    rolesToAssign.Add(RoleType.Crewmate, ensuredCrewmateRoles);
                if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0)
                    rolesToAssign.Add(RoleType.Neutral, ensuredNeutralRoles);
                if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0)
                    rolesToAssign.Add(RoleType.Impostor, ensuredImpostorRoles);

                // Randomly select a pool of roles to assign a role from next (Crewmate role, Neutral role or Impostor role) 
                // then select one of the roles from the selected pool to a player 
                // and remove the role (and any potentially blocked role pairings) from the pool(s)
                var roleType = rolesToAssign.Keys.ElementAt(Rng.Next(0, rolesToAssign.Keys.Count()));
                var players = roleType == RoleType.Crewmate || roleType == RoleType.Neutral
                    ? data.crewmates
                    : data.impostors;
                var index = Rng.Next(0, rolesToAssign[roleType].Count);
                var roleId = rolesToAssign[roleType][index];
                SetRoleToRandomPlayer(rolesToAssign[roleType][index], players);
                rolesToAssign[roleType].RemoveAt(index);

                if (CustomOptionHolder.BlockedRolePairings.ContainsKey(roleId))
                    foreach (var blockedRoleId in CustomOptionHolder.BlockedRolePairings[roleId])
                    {
                        // Set chance for the blocked roles to 0 for chances less than 100%
                        if (data.impSettings.ContainsKey(blockedRoleId)) data.impSettings[blockedRoleId] = 0;
                        if (data.neutralSettings.ContainsKey(blockedRoleId)) data.neutralSettings[blockedRoleId] = 0;
                        if (data.crewSettings.ContainsKey(blockedRoleId)) data.crewSettings[blockedRoleId] = 0;
                        // Remove blocked roles even if the chance was 100%
                        foreach (var ensuredRolesList in rolesToAssign.Values)
                            ensuredRolesList.RemoveAll(x => x == blockedRoleId);
                    }

                // Adjust the role limit
                switch (roleType)
                {
                    case RoleType.Crewmate:
                        data.maxCrewmateRoles--;
                        break;
                    case RoleType.Neutral:
                        data.maxNeutralRoles--;
                        break;
                    case RoleType.Impostor:
                        data.maxImpostorRoles--;
                        break;
                }
            }
        }


        private static void AssignChanceRoles(RoleAssignmentData data)
        {
            // Get all roles where the chance to occur is set grater than 0% but not 100% and build a ticket pool based on their weight
            var crewmateTickets = data.crewSettings.Where(x => x.Value > 0 && x.Value < 10)
                .Select(x => Enumerable.Repeat(x.Key, x.Value)).SelectMany(x => x).ToList();
            var neutralTickets = data.neutralSettings.Where(x => x.Value > 0 && x.Value < 10)
                .Select(x => Enumerable.Repeat(x.Key, x.Value)).SelectMany(x => x).ToList();
            var impostorTickets = data.impSettings.Where(x => x.Value > 0 && x.Value < 10)
                .Select(x => Enumerable.Repeat(x.Key, x.Value)).SelectMany(x => x).ToList();

            // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
            while (
                data.impostors.Count > 0 && data.maxImpostorRoles > 0 && impostorTickets.Count > 0 ||
                data.crewmates.Count > 0 && (
                    data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0 ||
                    data.maxNeutralRoles > 0 && neutralTickets.Count > 0
                ))
            {
                var rolesToAssign = new Dictionary<RoleType, List<byte>>();
                if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0)
                    rolesToAssign.Add(RoleType.Crewmate, crewmateTickets);
                if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && neutralTickets.Count > 0)
                    rolesToAssign.Add(RoleType.Neutral, neutralTickets);
                if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && impostorTickets.Count > 0)
                    rolesToAssign.Add(RoleType.Impostor, impostorTickets);

                // Randomly select a pool of role tickets to assign a role from next (Crewmate role, Neutral role or Impostor role) 
                // then select one of the roles from the selected pool to a player 
                // and remove all tickets of this role (and any potentially blocked role pairings) from the pool(s)
                var roleType = rolesToAssign.Keys.ElementAt(Rng.Next(0, rolesToAssign.Keys.Count()));
                var players = roleType == RoleType.Crewmate || roleType == RoleType.Neutral
                    ? data.crewmates
                    : data.impostors;
                var index = Rng.Next(0, rolesToAssign[roleType].Count);
                var roleId = rolesToAssign[roleType][index];
                SetRoleToRandomPlayer(rolesToAssign[roleType][index], players);
                rolesToAssign[roleType].RemoveAll(x => x == roleId);

                if (CustomOptionHolder.BlockedRolePairings.ContainsKey(roleId))
                    foreach (var blockedRoleId in CustomOptionHolder.BlockedRolePairings[roleId])
                    {
                        // Remove tickets of blocked roles from all pools
                        crewmateTickets.RemoveAll(x => x == blockedRoleId);
                        neutralTickets.RemoveAll(x => x == blockedRoleId);
                        impostorTickets.RemoveAll(x => x == blockedRoleId);
                    }

                // Adjust the role limit
                switch (roleType)
                {
                    case RoleType.Crewmate:
                        data.maxCrewmateRoles--;
                        break;
                    case RoleType.Neutral:
                        data.maxNeutralRoles--;
                        break;
                    case RoleType.Impostor:
                        data.maxImpostorRoles--;
                        break;
                }
            }
        }

        private static byte SetRoleToRandomPlayer(byte roleId, IList<PlayerControl> playerList, byte flag = 0,
            bool removePlayer = true)
        {
            var index = Rng.Next(0, playerList.Count);
            var playerId = playerList[index].PlayerId;
            if (removePlayer) playerList.RemoveAt(index);

            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRPC.SetRole, SendOption.Reliable, -1);
            writer.Write(roleId);
            writer.Write(playerId);
            writer.Write(flag);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SetRole(roleId, playerId, flag);
            return playerId;
        }


        private class RoleAssignmentData
        {
            public Dictionary<byte, int> crewSettings = new();
            public Dictionary<byte, int> impSettings = new();
            public Dictionary<byte, int> neutralSettings = new();
            public List<PlayerControl> crewmates { get; set; }
            public List<PlayerControl> impostors { get; set; }
            public int maxCrewmateRoles { get; set; }
            public int maxNeutralRoles { get; set; }
            public int maxImpostorRoles { get; set; }
        }

        private enum RoleType
        {
            Crewmate = 0,
            Neutral = 1,
            Impostor = 2
        }
    }
}