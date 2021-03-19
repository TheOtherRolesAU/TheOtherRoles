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
        private static T removeRandomElement<T>(List<T> list) {
            int index = rnd.Next(0, list.Count);
            T element = list[index];
            list.RemoveAt(index);
            return element;
        }

        private static void setRoleToRandomPlayer(byte roleId, List<PlayerControl> playerList) {
            byte playerId = removeRandomElement(playerList).PlayerId;

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
                if (impostors.Count > 0 && crewmates.Count > 0 && rnd.Next(1, 101) <= 33) {
                    setRoleToRandomPlayer((byte)RoleId.Lover1, impostors); 
                    setRoleToRandomPlayer((byte)RoleId.Lover2, crewmates); 
                } else if (crewmates.Count >= 2) {
                    setRoleToRandomPlayer((byte)RoleId.Lover1, crewmates); 
                    setRoleToRandomPlayer((byte)RoleId.Lover2, crewmates); 
                }
            }

            // Special roles impostors can be converted to
            List<RoleId> specialImpostorRoles = new List<RoleId>();

            if (rnd.Next(1, 101) <= BonusRolesPlugin.morphlingSpawnChance.GetValue())
                specialImpostorRoles.Add(RoleId.Morphling);

            if (rnd.Next(1, 101) <= BonusRolesPlugin.camouflagerSpawnChance.GetValue())
                specialImpostorRoles.Add(RoleId.Camouflager);

            while (impostors.Count > 0 && specialImpostorRoles.Count > 0)
            {
                setRoleToRandomPlayer((byte)removeRandomElement(specialImpostorRoles), impostors);
            }

            // Special roles crewmates can be converted to
            List<RoleId> specialCrewmateRoles = new List<RoleId>();

            if (rnd.Next(1, 101) <= BonusRolesPlugin.jesterSpawnChance.GetValue())
                specialCrewmateRoles.Add(RoleId.Jester);
            if (rnd.Next(1, 101) <= BonusRolesPlugin.mayorSpawnChance.GetValue())
                specialCrewmateRoles.Add(RoleId.Mayor);
            if (rnd.Next(1, 101) <= BonusRolesPlugin.engineerSpawnChance.GetValue())
                specialCrewmateRoles.Add(RoleId.Engineer);
            if (rnd.Next(1, 101) <= BonusRolesPlugin.sheriffSpawnChance.GetValue())
                specialCrewmateRoles.Add(RoleId.Sheriff);
            if (rnd.Next(1, 101) <= BonusRolesPlugin.lighterSpawnChance.GetValue())
                specialCrewmateRoles.Add(RoleId.Lighter);
            if (rnd.Next(1, 101) <= BonusRolesPlugin.detectiveSpawnChance.GetValue())
                specialCrewmateRoles.Add(RoleId.Detective);
            if (rnd.Next(1, 101) <= BonusRolesPlugin.timeMasterSpawnChance.GetValue())
                specialCrewmateRoles.Add(RoleId.TimeMaster);
            if (rnd.Next(1, 101) <= BonusRolesPlugin.medicSpawnChance.GetValue())
                specialCrewmateRoles.Add(RoleId.Medic);
            if (rnd.Next(1, 101) <= BonusRolesPlugin.shifterSpawnChance.GetValue())
                specialCrewmateRoles.Add(RoleId.Shifter);
            if (rnd.Next(1, 101) <= BonusRolesPlugin.swapperSpawnChance.GetValue())
                specialCrewmateRoles.Add(RoleId.Swapper);
            if (rnd.Next(1, 101) <= BonusRolesPlugin.seerSpawnChance.GetValue())
                specialCrewmateRoles.Add(RoleId.Seer);
            if (rnd.Next(1, 101) <= BonusRolesPlugin.spySpawnChance.GetValue())
                specialCrewmateRoles.Add(RoleId.Spy);
            if (rnd.Next(1, 101) <= BonusRolesPlugin.childSpawnChance.GetValue())
                specialCrewmateRoles.Add(RoleId.Child);

            while (crewmates.Count > 0 && specialCrewmateRoles.Count > 0)
            {
                setRoleToRandomPlayer((byte)removeRandomElement(specialCrewmateRoles), crewmates);
            }
        }
    }
}
