using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using UnityEngine;
using System;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
    class SetInfectedPatch
    {

        private static void setRoleToRandomPlayer(byte roleId, List<PlayerControl> playerList) {
            var index = rnd.Next(0, playerList.Count);
            byte playerId = playerList[index].PlayerId;
            playerList.RemoveAt(index);

            setRoleToPlayer(roleId, playerId);
        }

        private static void setRoleToPlayer(byte roleId, byte playerId) {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetRole, Hazel.SendOption.Reliable, -1);
            writer.Write(roleId);
            writer.Write(playerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.setRole(roleId, playerId);
        }

        public static void Postfix(Il2CppReferenceArray<GameData.PlayerInfo> FMAOEJEHPAO)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ResetVaribles, Hazel.SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.resetVariables();

            List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            crewmates.RemoveAll(x => x.Data.IsImpostor);
            List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            impostors.RemoveAll(x => !x.Data.IsImpostor);

            float crewCountSettings = (float)TheOtherRolesPlugin.crewmateRolesCount.GetValue() / 2;
            float impCountSettings = (float)TheOtherRolesPlugin.impostorRolesCount.GetValue() / 2;

            if (crewCountSettings % 1 == 0.5f) crewCountSettings += 0.5f * (rnd.Next(2) * 2 - 1);
            if (impCountSettings % 1 == 0.5f) impCountSettings += 0.5f * (rnd.Next(2) * 2 - 1);

            int maxCrewmateRoles = Mathf.Min(crewmates.Count, Mathf.RoundToInt(crewCountSettings));
            int maxImpostorRoles = Mathf.Min(impostors.Count, Mathf.RoundToInt(impCountSettings));

            Dictionary<byte, int> impSettings = new Dictionary<byte, int>();
            Dictionary<byte, int> crewSettings = new Dictionary<byte, int>();
            
            impSettings.Add((byte)RoleId.Morphling, TheOtherRolesPlugin.morphlingSpawnRate.GetValue());
            impSettings.Add((byte)RoleId.Camouflager, TheOtherRolesPlugin.camouflagerSpawnRate.GetValue());
            impSettings.Add((byte)RoleId.Vampire, TheOtherRolesPlugin.vampireSpawnRate.GetValue());

            crewSettings.Add((byte)RoleId.Jester, TheOtherRolesPlugin.jesterSpawnRate.GetValue());
            crewSettings.Add((byte)RoleId.Mayor, TheOtherRolesPlugin.mayorSpawnRate.GetValue());
            crewSettings.Add((byte)RoleId.Engineer, TheOtherRolesPlugin.engineerSpawnRate.GetValue());
            crewSettings.Add((byte)RoleId.Sheriff, TheOtherRolesPlugin.sheriffSpawnRate.GetValue());
            crewSettings.Add((byte)RoleId.Lighter, TheOtherRolesPlugin.lighterSpawnRate.GetValue());
            crewSettings.Add((byte)RoleId.Detective, TheOtherRolesPlugin.detectiveSpawnRate.GetValue());
            crewSettings.Add((byte)RoleId.TimeMaster, TheOtherRolesPlugin.timeMasterSpawnRate.GetValue());
            crewSettings.Add((byte)RoleId.Medic, TheOtherRolesPlugin.medicSpawnRate.GetValue());
            crewSettings.Add((byte)RoleId.Shifter, TheOtherRolesPlugin.shifterSpawnRate.GetValue());
            crewSettings.Add((byte)RoleId.Swapper,TheOtherRolesPlugin.swapperSpawnRate.GetValue());
            crewSettings.Add((byte)RoleId.Seer, TheOtherRolesPlugin.seerSpawnRate.GetValue());
            crewSettings.Add((byte)RoleId.Spy, TheOtherRolesPlugin.spySpawnRate.GetValue());
            crewSettings.Add((byte)RoleId.Child, TheOtherRolesPlugin.childSpawnRate.GetValue());
            crewSettings.Add((byte)RoleId.Tracker, TheOtherRolesPlugin.trackerSpawnRate.GetValue());
            crewSettings.Add((byte)RoleId.Snitch, TheOtherRolesPlugin.snitchSpawnRate.GetValue());
            crewSettings.Add((byte)RoleId.Jackal, TheOtherRolesPlugin.jackalSpawnRate.GetValue());
            // crewSettings.Add((byte)RoleId.BountyHunter, TheOtherRolesPlugin.bountyHunterSpawnRate.GetValue()); BOUNTY HUNTER

            // Set multiple player roles
            if (impostors.Count >= 3 && maxImpostorRoles >= 3 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.mafiaSpawnRate.GetValue() * 10)) {
                setRoleToRandomPlayer((byte)RoleId.Godfather, impostors);
                setRoleToRandomPlayer((byte)RoleId.Janitor, impostors);
                setRoleToRandomPlayer((byte)RoleId.Mafioso, impostors);
                maxImpostorRoles -= 3;
            }

            if (rnd.Next(1, 101) <= TheOtherRolesPlugin.loversSpawnRate.GetValue() * 10) {
                if (impostors.Count > 0 && crewmates.Count > 0 && maxCrewmateRoles > 0 && maxImpostorRoles > 0 && rnd.Next(1, 101) <= TheOtherRolesPlugin.loversImpLoverRate.GetValue()) {
                    setRoleToRandomPlayer((byte)RoleId.Lover1, impostors); 
                    setRoleToRandomPlayer((byte)RoleId.Lover2, crewmates);
                    maxCrewmateRoles--;
                    maxImpostorRoles--;
                } else if (crewmates.Count >= 2 && maxCrewmateRoles >= 2) {
                    setRoleToRandomPlayer((byte)RoleId.Lover1, crewmates); 
                    setRoleToRandomPlayer((byte)RoleId.Lover2, crewmates); 
                    maxCrewmateRoles -= 2; 
                }
            }

            // Set tickets and always active roles

            List<byte> crewTickets = new List<byte>();
            List<byte> impTickets = new List<byte>();

            foreach (KeyValuePair<byte, int> entry in crewSettings) {
                if (entry.Value == 0) { // Never
                } else if (entry.Value == 10) { // Always
                    if (crewmates.Count > 0 && maxCrewmateRoles > 0) {
                        setRoleToRandomPlayer(entry.Key, crewmates);
                        maxCrewmateRoles--;
                    }
                } else { // Other
                    for (int i = 0; i < entry.Value; i++) crewTickets.Add(entry.Key);
                }
            }

            foreach (KeyValuePair<byte, int> entry in impSettings) {
                if (entry.Value == 0) { // Never
                } else if (entry.Value == 10) { // Always
                    if (impostors.Count > 0 && maxImpostorRoles > 0) {
                        setRoleToRandomPlayer(entry.Key, impostors);
                        maxImpostorRoles--;
                    }
                } else { // Other
                    for (int i = 0; i < entry.Value; i++) impTickets.Add(entry.Key);
                }
            }

            // Set solo player roles

            for (int i = 0; i < maxCrewmateRoles; i++) {
                if (crewTickets.Count > 0 && crewmates.Count > 0) {
                    var index = rnd.Next(0, crewTickets.Count);
                    byte roleId = crewTickets[index];
                    crewTickets.RemoveAll(x => x == roleId);
                    setRoleToRandomPlayer(roleId, crewmates);
                }
            }

            for (int i = 0; i < maxImpostorRoles; i++) {
                if (impTickets.Count > 0 && impostors.Count > 0) {
                    var index = rnd.Next(0, impTickets.Count);
                    byte roleId = impTickets[index];
                    impTickets.RemoveAll(x => x == roleId);
                    setRoleToRandomPlayer(roleId, impostors);
                }
            }

            // if (crewmates.Count > 0 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.bountyHunterSpawnRate.GetValue())) {
            //     List<PlayerControl> availableTargets = PlayerControl.AllPlayerControls.ToArray().ToList();
            //     var bountyHunterIndex = rnd.Next(0, crewmates.Count);
            //     byte bountyHunterId = crewmates[bountyHunterIndex].PlayerId;

            //     availableTargets.RemoveAll(x => x.Data.IsImpostor || x.PlayerId == bountyHunterId);

            //     if (availableTargets.Count > 0) {
            //         byte targetId = availableTargets[rnd.Next(0, availableTargets.Count)].PlayerId;
            //         crewmates.RemoveAt(bountyHunterIndex);
            //         setRoleToPlayer((byte)RoleId.BountyHunter, bountyHunterId);
            //         writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetBountyHunterTarget, Hazel.SendOption.Reliable, -1);
            //         writer.Write(targetId);
            //         AmongUsClient.Instance.FinishRpcImmediately(writer);
            //         RPCProcedure.setBountyHunterTarget(targetId);
            //     }
            // }
        }
    }
}
