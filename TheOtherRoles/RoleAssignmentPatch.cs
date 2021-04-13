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

        public static void Postfix(Il2CppReferenceArray<GameData.LGBOMGHJELL> BHNEINNHPIJ)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ResetVaribles, Hazel.SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.resetVariables();

            List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            crewmates.RemoveAll(x => x.PPMOEEPBHJO.FDNMBJOAPFL);
            List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            impostors.RemoveAll(x => !x.PPMOEEPBHJO.FDNMBJOAPFL);

            float crewCountSettings = (float)CustomOptionHolder.crewmateRolesCount.getSelection() / 2;
            float impCountSettings = (float)CustomOptionHolder.impostorRolesCount.getSelection() / 2;

            if (crewCountSettings % 1 == 0.5f) crewCountSettings += 0.5f * (rnd.Next(2) * 2 - 1);
            if (impCountSettings % 1 == 0.5f) impCountSettings += 0.5f * (rnd.Next(2) * 2 - 1);

            int maxCrewmateRoles = Mathf.Min(crewmates.Count, Mathf.RoundToInt(crewCountSettings));
            int maxImpostorRoles = Mathf.Min(impostors.Count, Mathf.RoundToInt(impCountSettings));

            Dictionary<byte, int> impSettings = new Dictionary<byte, int>();
            Dictionary<byte, int> crewSettings = new Dictionary<byte, int>();
            
            impSettings.Add((byte)RoleId.Morphling, CustomOptionHolder.morphlingSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Camouflager, CustomOptionHolder.camouflagerSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Vampire, CustomOptionHolder.vampireSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Eraser, CustomOptionHolder.eraserSpawnRate.getSelection());
            impSettings.Add((byte)RoleId.Trickster, CustomOptionHolder.tricksterSpawnRate.getSelection());

            crewSettings.Add((byte)RoleId.Jester, CustomOptionHolder.jesterSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Mayor, CustomOptionHolder.mayorSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Engineer, CustomOptionHolder.engineerSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Sheriff, CustomOptionHolder.sheriffSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Lighter, CustomOptionHolder.lighterSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Detective, CustomOptionHolder.detectiveSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.TimeMaster, CustomOptionHolder.timeMasterSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Medic, CustomOptionHolder.medicSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Shifter, CustomOptionHolder.shifterSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Swapper,CustomOptionHolder.swapperSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Seer, CustomOptionHolder.seerSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Hacker, CustomOptionHolder.hackerSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Tracker, CustomOptionHolder.trackerSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Snitch, CustomOptionHolder.snitchSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Jackal, CustomOptionHolder.jackalSpawnRate.getSelection());
            crewSettings.Add((byte)RoleId.Spy, CustomOptionHolder.spySpawnRate.getSelection());

            // Set special roles
            if (impostors.Count >= 3 && maxImpostorRoles >= 3 && (rnd.Next(1, 101) <= CustomOptionHolder.mafiaSpawnRate.getSelection() * 10)) {
                setRoleToRandomPlayer((byte)RoleId.Godfather, impostors);
                setRoleToRandomPlayer((byte)RoleId.Janitor, impostors);
                setRoleToRandomPlayer((byte)RoleId.Mafioso, impostors);
                maxImpostorRoles -= 3;
            }

            if (rnd.Next(1, 101) <= CustomOptionHolder.loversSpawnRate.getSelection() * 10) {
                if (impostors.Count > 0 && crewmates.Count > 0 && maxCrewmateRoles > 0 && maxImpostorRoles > 0 && rnd.Next(1, 101) <= CustomOptionHolder.loversImpLoverRate.getSelection()) {
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

            if (rnd.Next(1, 101) <= CustomOptionHolder.childSpawnRate.getSelection() * 10) {
                if (impostors.Count > 0 && maxImpostorRoles > 0 && rnd.Next(1, 101) <= 33) {
                    setRoleToRandomPlayer((byte)RoleId.Child, impostors); 
                    maxImpostorRoles--;
                } else if (crewmates.Count > 0 && maxCrewmateRoles > 0) {
                    setRoleToRandomPlayer((byte)RoleId.Child, crewmates);
                    maxCrewmateRoles--;
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
        }
    }
}
