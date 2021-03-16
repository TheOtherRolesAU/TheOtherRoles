using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using System;
using static BonusRoles.BonusRoles;

namespace BonusRoles
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
    class SetInfectedPatch
    {
        private static void setRoleToRandomPlayer(byte roleId, List<PlayerControl> playerList) {
            var index = rnd.Next(0, playerList.Count);
            byte playerId = playerList[index].PlayerId;
            playerList.RemoveAt(index);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetRole, Hazel.SendOption.None, -1);
            writer.Write(roleId);
            writer.Write(playerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.setRole(roleId, playerId);
        }

        public static void Postfix(Il2CppReferenceArray<GameData.PlayerInfo> FMAOEJEHPAO)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ResetVaribles, Hazel.SendOption.None, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.resetVariables();

            List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            crewmates.RemoveAll(x => x.Data.IsImpostor);
            List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            impostors.RemoveAll(x => !x.Data.IsImpostor);

            // Special roles impostors can be converted to
            if (impostors.Count >= 3 && (rnd.Next(1, 101) <= BonusRolesPlugin.mafiaSpawnChance.GetValue())) {
                setRoleToRandomPlayer((byte)RoleId.Godfather, impostors);
                setRoleToRandomPlayer((byte)RoleId.Janitor, impostors);
                setRoleToRandomPlayer((byte)RoleId.Mafioso, impostors);
            }

            // Special roles that involve crewmates and impostors
            if (rnd.Next(1, 101) <= BonusRolesPlugin.loversSpawnChance.GetValue())
            {
                if (impostors.Count > 0 && crewmates.Count > 0 && rnd.Next(1, 101) <= BonusRolesPlugin.impLoverChance.GetValue()) {
                    setRoleToRandomPlayer((byte)RoleId.Lover1, impostors); 
                    setRoleToRandomPlayer((byte)RoleId.Lover2, crewmates); 
                } else if (crewmates.Count >= 2) {
                    setRoleToRandomPlayer((byte)RoleId.Lover1, crewmates); 
                    setRoleToRandomPlayer((byte)RoleId.Lover2, crewmates); 
                }
            }

            // Special roles impostors can be converted to
            if (impostors.Count >= 1 && (rnd.Next(1, 101) <= BonusRolesPlugin.morphlingSpawnChance.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Morphling, impostors);

            if (impostors.Count >= 1 && (rnd.Next(1, 101) <= BonusRolesPlugin.camouflagerSpawnChance.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Camouflager, impostors);
            
            // Special roles crewmates can be converted to
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= BonusRolesPlugin.jesterSpawnChance.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Jester, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= BonusRolesPlugin.mayorSpawnChance.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Mayor, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= BonusRolesPlugin.engineerSpawnChance.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Engineer, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= BonusRolesPlugin.sheriffSpawnChance.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Sheriff, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= BonusRolesPlugin.lighterSpawnChance.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Lighter, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= BonusRolesPlugin.detectiveSpawnChance.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Detective, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= BonusRolesPlugin.timeMasterSpawnChance.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.TimeMaster, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= BonusRolesPlugin.medicSpawnChance.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Medic, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= BonusRolesPlugin.shifterSpawnChance.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Shifter, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= BonusRolesPlugin.swapperSpawnChance.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Swapper, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= BonusRolesPlugin.seerSpawnChance.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Seer, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= BonusRolesPlugin.spySpawnChance.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Spy, crewmates);
            if (crewmates.Count > 0 && (rnd.Next(1, 101) <= BonusRolesPlugin.childSpawnChance.GetValue()))
                setRoleToRandomPlayer((byte)RoleId.Child, crewmates);
        }
    }
}
