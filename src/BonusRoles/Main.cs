using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using Reactor;
using System.Collections.Generic;
using Essentials.Options;
using UnityEngine;
using System.Linq;
using System.Net;
using System.IO;
using System;

namespace BonusRoles
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class BonusRolesPlugin : BasePlugin
    {
        public const string Id = "me.eisbison.bonusroles";
        public Harmony Harmony { get; } = new Harmony(Id);
        public ConfigEntry<string> Name { get; private set; }

        // Role spawn chances
        public static CustomNumberOption mafiaSpawnChance = CustomOption.AddNumber("Mafia Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption morphlingSpawnChance = CustomOption.AddNumber("Morphling Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption camouflagerSpawnChance = CustomOption.AddNumber("Camouflager Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption loversSpawnChance = CustomOption.AddNumber("Lovers Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption jesterSpawnChance = CustomOption.AddNumber("Jester Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption mayorSpawnChance = CustomOption.AddNumber("Mayor Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption engineerSpawnChance = CustomOption.AddNumber("Engineer Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption sheriffSpawnChance = CustomOption.AddNumber("Sheriff Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption lighterSpawnChance = CustomOption.AddNumber("Lighter Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption detectiveSpawnChance = CustomOption.AddNumber("Detective Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption timeMasterSpawnChance = CustomOption.AddNumber("Time Master Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption medicSpawnChance = CustomOption.AddNumber("Medic Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption shifterSpawnChance = CustomOption.AddNumber("Shifter Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption swapperSpawnChance = CustomOption.AddNumber("Swapper Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption seerSpawnChance = CustomOption.AddNumber("Seer Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption spySpawnChance = CustomOption.AddNumber("Spy Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption childSpawnChance = CustomOption.AddNumber("Child Spawn Chance", 100, 0, 100, 10);

        // Role settings
        public static CustomNumberOption janitorCooldown = CustomOption.AddNumber("Janitor Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomNumberOption morphlingCooldown = CustomOption.AddNumber("Morphling Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomNumberOption camouflagerCooldown = CustomOption.AddNumber("Camouflager Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomToggleOption loversBothDie = CustomOption.AddToggle("Both Lovers Die", true); 
        public static CustomNumberOption sheriffCooldown = CustomOption.AddNumber("Sheriff Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomToggleOption jesterCanDieToSheriff = CustomOption.AddToggle("Jester Can Die To Sheriff", false); 
        public static CustomNumberOption lighterVision = CustomOption.AddNumber("Lighter Vision", 2f, 0.25f, 5f, 0.25f);
        public static CustomToggleOption detectiveAnonymousFootprints = CustomOption.AddToggle("Anonymous Footprints", false); 
        public static CustomNumberOption detectiveFootprintIntervall = CustomOption.AddNumber("Footprint Intervall", 0.5f, 0.25f, 10f, 0.25f);
        public static CustomNumberOption detectiveFootprintDuration = CustomOption.AddNumber("Footprint Duration", 5f, 0.25f, 10f, 0.25f);
        public static CustomNumberOption timeMasterCooldown = CustomOption.AddNumber("Time Master Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomNumberOption timeMasterRewindTime = CustomOption.AddNumber("Rewind Time", 3f, 1f, 10f, 1f);
        public static CustomToggleOption timeMasterReviveDuringRewind = CustomOption.AddToggle("Revive During Rewind", false);
        public static CustomNumberOption medicReportNameDuration = CustomOption.AddNumber("Time Where Medic Reports Will Have Name", 10, 0, 60, 2.5f);
        public static CustomNumberOption medicReportColorDuration = CustomOption.AddNumber("Time Where Medic Reports Will Have Color Type", 20, 0, 120, 2.5f);
        public static CustomStringOption medicShowShielded = CustomOption.AddString("Show Shielded Player", new string[] {"Everyone", "Shielded + Medic", "Medic"});
        public static CustomToggleOption medicShowAttemptToShielded = CustomOption.AddToggle("Shielded Player Sees Murder Attempt", false);
        public static CustomNumberOption shifterCooldown = CustomOption.AddNumber("Shifter Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomNumberOption seerCooldown = CustomOption.AddNumber("Seer Cooldown (No Reset After Meeting)", 15f, 30f, 180f, 15f);
        public static CustomStringOption seerKindOfInfo = CustomOption.AddString("Info That Seer Reveals", new string[] {"Role", "Good/Bad"});
        public static CustomStringOption seerPlayersWithNotification = CustomOption.AddString("Players That See When They Are Being Revealed", new string[] {"Everyone", "The Good", "The Bad", "Nobody"}); 

        public ConfigEntry<string> Ip { get; set; }
        public ConfigEntry<ushort> Port { get; set; }

        public override void Load()
        {
            Ip = Config.Bind("Custom", "Ipv4 or Hostname", "127.0.0.1");
            Port = Config.Bind("Custom", "Port", (ushort)22023);

            var defaultRegions = ServerManager.DefaultRegions.ToList();
            var ip = Ip.Value;
            if (Uri.CheckHostName(Ip.Value).ToString() == "Dns")
            {
                foreach (IPAddress address in Dns.GetHostAddresses(Ip.Value))
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ip = address.ToString(); break;
                    }
                }
            }
            defaultRegions.Insert(0, new RegionInfo(
                "Custom", ip, new[]
                {
                    new ServerInfo($"Custom-Server", ip, Port.Value)
                })
            );
            ServerManager.DefaultRegions = defaultRegions.ToArray();


            CustomOption.ShamelessPlug = false;
            Harmony.PatchAll();
        }
    }

    // Deactivate bans, since I always leave my local testing game and ban myself
    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
    public static class AmBannedPatch
    {
        public static void Postfix(out bool __result)
        {
            __result = false;
        }
    }
}
