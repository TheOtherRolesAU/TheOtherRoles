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
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetRole, Hazel.SendOption.None, -1);
            writer.Write(roleId);
            writer.Write(playerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.setRole(roleId, playerId);
        }

        // public static void addTickets(byte roleId, int amount, List<byte> tickets) {
        //     for (int i = 0; i < amount; i++)
        //         tickets.Add(roleId);
        // }

        public static void Postfix(Il2CppReferenceArray<GameData.PlayerInfo> FMAOEJEHPAO)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ResetVaribles, Hazel.SendOption.None, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.resetVariables();

            List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            crewmates.RemoveAll(x => x.Data.IsImpostor);
            List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            impostors.RemoveAll(x => !x.Data.IsImpostor);

            // int totalCrewmateRoles = Mathf.Min(crewmates.Count, Mathf.RoundToInt(TheOtherRolesPlugin.crewmateRolesCount.GetValue()));
            // int totalImpostorsCount = Mathf.Min(impostors.Count, Mathf.RoundToInt(TheOtherRolesPlugin.impostorRolesCount.GetValue()));

            // List<byte> tickets = new List<byte>();

            // addTickets((byte)RoleId.Jester, Mathf.RoundToInt(TheOtherRolesPlugin.jesterSpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.Mayor, Mathf.RoundToInt(TheOtherRolesPlugin.mayorSpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.Engineer, Mathf.RoundToInt(TheOtherRolesPlugin.engineerSpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.Sheriff, Mathf.RoundToInt(TheOtherRolesPlugin.sheriffSpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.Lighter, Mathf.RoundToInt(TheOtherRolesPlugin.lighterSpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.Detective, Mathf.RoundToInt(TheOtherRolesPlugin.detectiveSpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.TimeMaster, Mathf.RoundToInt(TheOtherRolesPlugin.timeMasterSpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.Medic, Mathf.RoundToInt(TheOtherRolesPlugin.medicSpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.Shifter, Mathf.RoundToInt(TheOtherRolesPlugin.shifterSpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.Swapper,Mathf.RoundToInt(TheOtherRolesPlugin.swapperSpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.Seer, Mathf.RoundToInt(TheOtherRolesPlugin.seerSpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.Morphling, Mathf.RoundToInt(TheOtherRolesPlugin.morphlingSpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.Camouflager, Mathf.RoundToInt(TheOtherRolesPlugin.camouflagerSpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.Spy, Mathf.RoundToInt(TheOtherRolesPlugin.spySpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.Child, Mathf.RoundToInt(TheOtherRolesPlugin.childSpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.BountyHunter, Mathf.RoundToInt(TheOtherRolesPlugin.bountyHunterSpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.Tracker, Mathf.RoundToInt(TheOtherRolesPlugin.trackerSpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.Vampire, Mathf.RoundToInt(TheOtherRolesPlugin.vampireSpawnRate.GetValue()), tickets);
            // addTickets((byte)RoleId.Snitch, Mathf.RoundToInt(TheOtherRolesPlugin.snitchSpawnRate.GetValue()), tickets);

            

            // Special roles impostors can be converted to
            if (impostors.Count >= 3 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.mafiaSpawnRate.GetValue())) {
                setRoleToRandomPlayer((byte)RoleId.Godfather, impostors);
                setRoleToRandomPlayer((byte)RoleId.Janitor, impostors);
                setRoleToRandomPlayer((byte)RoleId.Mafioso, impostors);
            }

            // Special roles that involve crewmates and impostors
            if (rnd.Next(1, 101) <= TheOtherRolesPlugin.loversSpawnRate.GetValue())
            {
                if (impostors.Count > 0 && crewmates.Count > 0 && rnd.Next(1, 101) <= 33) {
                    setRoleToRandomPlayer((byte)RoleId.Lover1, impostors); 
                    setRoleToRandomPlayer((byte)RoleId.Lover2, crewmates); 
                } else if (crewmates.Count >= 2) {
                    setRoleToRandomPlayer((byte)RoleId.Lover1, crewmates); 
                    setRoleToRandomPlayer((byte)RoleId.Lover2, crewmates); 
                }
            }

            // Special roles impostors can be converted to
            if (impostors.Count >= 1 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.morphlingSpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Morphling, impostors);
            if (impostors.Count >= 1 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.camouflagerSpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Camouflager, impostors);
            if (impostors.Count >= 1 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.vampireSpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Vampire, impostors);
            
            // Special roles crewmates can be converted to
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.jesterSpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Jester, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.mayorSpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Mayor, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.engineerSpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Engineer, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.sheriffSpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Sheriff, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.lighterSpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Lighter, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.detectiveSpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Detective, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.timeMasterSpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.TimeMaster, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.medicSpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Medic, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.shifterSpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Shifter, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.swapperSpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Swapper, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.seerSpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Seer, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.spySpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Spy, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.childSpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Child, crewmates);
            // if (crewmates.Count > 0 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.bountyHunterSpawnRate.GetValue())) {
            //     List<PlayerControl> availableTargets = PlayerControl.AllPlayerControls.ToArray().ToList();
            //     var bountyHunterIndex = rnd.Next(0, crewmates.Count);
            //     byte bountyHunterId = crewmates[bountyHunterIndex].PlayerId;

            //     availableTargets.RemoveAll(x => x.Data.IsImpostor || x.PlayerId == bountyHunterId);

            //     if (availableTargets.Count > 0) {
            //         byte targetId = availableTargets[rnd.Next(0, availableTargets.Count)].PlayerId;
            //         crewmates.RemoveAt(bountyHunterIndex);
            //         setRoleToPlayer((byte)RoleId.BountyHunter, bountyHunterId);
            //         writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetBountyHunterTarget, Hazel.SendOption.None, -1);
            //         writer.Write(targetId);
            //         AmongUsClient.Instance.FinishRpcImmediately(writer);
            //         RPCProcedure.setBountyHunterTarget(targetId);
            //     }
            // }
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.trackerSpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Tracker, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= TheOtherRolesPlugin.snitchSpawnRate.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Snitch, crewmates);
        }
    }
}
