using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using Reactor;
using System.Collections.Generic;
using Essentials.Options;
using System.Linq;
using System.Net;
using System.IO;
using System;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class TheOtherRolesPlugin : BasePlugin
    {
        public const string Id = "me.eisbison.theotherroles";
        public Harmony Harmony { get; } = new Harmony(Id);
        public static TheOtherRolesPlugin Instance { get { return PluginSingleton<TheOtherRolesPlugin>.Instance; } }
        internal static BepInEx.Logging.ManualLogSource Logger { get { return Instance.Log; } }

        // Roles
        public static string[] rates = new string[]{"0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%"};
        public static string[] crewmateRoleCaps = new string[]{"0", "0-1", "1", "1-2", "2", "2-3", "3", "3-4", "4", "4-5", "5", "5-6", "6", "6-7", "7", "7-8", "8", "8-9", "9", "9-10", "10"};
        public static string[] impostorRoleCaps = new string[]{"0", "0-1", "1", "1-2", "2", "2-3", "3"};
        public static string[] presets = new string[]{"Preset 1", "Preset 2", "Preset 3", "Preset 4", "Preset 5"};


        public static CustomStringOption presetSelection = CustomOption.AddString("0", "[CCCC00FF]Preset", false, presets);
        public static CustomStringOption crewmateRolesCount = CustomOption.AddString("1", "[CCCC00FF]Number Of Crewmate/Neutral Roles", crewmateRoleCaps);
        public static CustomStringOption impostorRolesCount = CustomOption.AddString("2", "[CCCC00FF]Number Of Impostor Roles", impostorRoleCaps);

        public static CustomStringOption mafiaSpawnRate = CustomOption.AddString("10", cs(Janitor.color) + "Mafia", rates); // Id 10-14
        public static CustomStringOption morphlingSpawnRate = CustomOption.AddString("15", cs(Morphling.color) + "Morphling", rates); // Id 15-19
        public static CustomStringOption camouflagerSpawnRate = CustomOption.AddString("20", cs(Camouflager.color) + "Camouflager", rates); // Id 20-24
        public static CustomStringOption vampireSpawnRate = CustomOption.AddString("25", cs(Vampire.color) + "Vampire", rates); // Id 25-29

        public static CustomStringOption loversSpawnRate = CustomOption.AddString("30", cs(Lovers.color) + "Lovers", rates); // Id 30-34
        public static CustomStringOption jesterSpawnRate = CustomOption.AddString("35", cs(Jester.color) + "Jester", rates); // Id 35-39
        public static CustomStringOption shifterSpawnRate = CustomOption.AddString("40", cs(Shifter.color) + "Shifter", rates); // Id 40-44

        public static CustomStringOption mayorSpawnRate = CustomOption.AddString("45", cs(Mayor.color) + "Mayor", rates); // Id 45-49
        public static CustomStringOption engineerSpawnRate = CustomOption.AddString("50", cs(Engineer.color) + "Engineer", rates); // Id 50-54 
        public static CustomStringOption sheriffSpawnRate = CustomOption.AddString("55", cs(Sheriff.color) + "Sheriff", rates); // Id 55-59
        public static CustomStringOption lighterSpawnRate = CustomOption.AddString("60", cs(Lighter.color) + "Lighter", rates); // Id 60-64
        public static CustomStringOption detectiveSpawnRate = CustomOption.AddString("65", cs(Detective.color) + "Detective", rates); // Id 65-69
        public static CustomStringOption timeMasterSpawnRate = CustomOption.AddString("70", cs(TimeMaster.color) + "Time Master", rates); // Id 70-74
        public static CustomStringOption medicSpawnRate = CustomOption.AddString("75", cs(Medic.color) + "Medic", rates); // Id 75-79
        public static CustomStringOption swapperSpawnRate = CustomOption.AddString("80", cs(Swapper.color) + "Swapper", rates); // Id 80-84
        public static CustomStringOption seerSpawnRate = CustomOption.AddString("85", cs(Seer.color) + "Seer", rates); // Id 85-89
        public static CustomStringOption spySpawnRate = CustomOption.AddString("90", cs(Spy.color) + "Spy", rates); // Id 90-94
        public static CustomStringOption childSpawnRate = CustomOption.AddString("95", cs(Child.color) + "Child", rates); // Id 95-99
        // public static CustomStringOption bountyHunterSpawnRate = CustomOption.AddString("100", cs(BountyHunter.color) + "Bounty Hunter", rates); // Id 100-104
        public static CustomStringOption trackerSpawnRate = CustomOption.AddString("105", cs(Tracker.color) + "Tracker", rates); // Id 105-109
        public static CustomStringOption snitchSpawnRate = CustomOption.AddString("110", cs(Snitch.color) + "Snitch", rates); // Id 110-114
        public static CustomStringOption jackalSpawnRate = CustomOption.AddString("115", cs(Jackal.color) + "Jackal", rates); // Id 115-124

        // Role settings
        public static CustomNumberOption janitorCooldown = CustomOption.AddNumber("11", "Janitor Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomNumberOption morphlingCooldown = CustomOption.AddNumber("16", "Morphling Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomNumberOption camouflagerCooldown = CustomOption.AddNumber("21", "Camouflager Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomNumberOption vampireKillDelay = CustomOption.AddNumber("26", "Vampire Kill Delay", 10f, 1f, 20f, 1f);
        public static CustomNumberOption vampireCooldown = CustomOption.AddNumber("27", "Vampire Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomToggleOption vampireCanKillNearGarlics = CustomOption.AddToggle("28", "Vampire Can Kill Near Garlics", true);

        public static CustomNumberOption loversImpLoverRate = CustomOption.AddNumber("31", "Chance That One Lover Is Impostor", 30f, 0f, 100f, 10f);
        public static CustomToggleOption loversBothDie = CustomOption.AddToggle("32", "Both Lovers Die", true);
        public static CustomNumberOption shifterCooldown = CustomOption.AddNumber("41", "Shifter Cooldown", 30f, 10f, 60f, 2.5f);

        public static CustomNumberOption sheriffCooldown = CustomOption.AddNumber("56", "Sheriff Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomToggleOption jesterCanDieToSheriff = CustomOption.AddToggle("57", "Jester Can Die To Sheriff", false);
        public static CustomNumberOption lighterVision = CustomOption.AddNumber("61", "Lighter Vision", 2f, 0.25f, 5f, 0.25f);
        public static CustomToggleOption detectiveAnonymousFootprints = CustomOption.AddToggle("66", "Anonymous Footprints", false); 
        public static CustomNumberOption detectiveFootprintIntervall = CustomOption.AddNumber("67", "Footprint Intervall", 0.5f, 0.25f, 10f, 0.25f);
        public static CustomNumberOption detectiveFootprintDuration = CustomOption.AddNumber("68", "Footprint Duration", 5f, 0.25f, 10f, 0.25f);
        public static CustomNumberOption timeMasterCooldown = CustomOption.AddNumber("71", "Time Master Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomNumberOption timeMasterRewindTime = CustomOption.AddNumber("72", "Rewind Time", 3f, 1f, 10f, 1f);
        public static CustomToggleOption timeMasterReviveDuringRewind = CustomOption.AddToggle("73", "Revive During Rewind", false);
        public static CustomNumberOption medicReportNameDuration = CustomOption.AddNumber("76", "Time Where Medic Reports Will Have Name", 10, 0, 60, 2.5f);
        public static CustomNumberOption medicReportColorDuration = CustomOption.AddNumber("77", "Time Where Medic Reports Will Have Color Type", 20, 0, 120, 2.5f);
        public static CustomStringOption medicShowShielded = CustomOption.AddString("78", "Show Shielded Player", new string[] {"Everyone", "Shielded + Medic", "Medic"});
        public static CustomToggleOption medicShowAttemptToShielded = CustomOption.AddToggle("79", "Shielded Player Sees Murder Attempt", false);
        public static CustomNumberOption seerCooldown = CustomOption.AddNumber("86", "Seer Cooldown (No Reset After Meeting)", 15f, 30f, 180f, 15f);
        public static CustomNumberOption seerChanceOfSeeingRight = CustomOption.AddNumber("87", "Seer Chance Of Seeing Right", 100, 0, 100, 5);
        public static CustomStringOption seerKindOfInfo = CustomOption.AddString("88", "Info That Seer Reveals", new string[] {"Role", "Good/Bad"});
        public static CustomStringOption seerPlayersWithNotification = CustomOption.AddString("89", "Players That See When They Are Being Revealed", new string[] {"Everyone", "The Good", "The Bad", "Nobody"});
        public static CustomNumberOption spyCooldown = CustomOption.AddNumber("91", "Spy Cooldown", 30f, 10f, 120f, 5f);
        public static CustomNumberOption spySpyingDuration = CustomOption.AddNumber("92", "Spy Duration", 10f, 2.5f, 60f, 2.5f);
        public static CustomNumberOption childGrowingUpDuration = CustomOption.AddNumber("96", "Child Growing Up Duration", 400f, 100f, 1500f, 100f);
        // public static CustomToggleOption bountyHunterNotifyBounty = CustomOption.AddToggle("101", "Bounty Gets Notified", true); 
        public static CustomNumberOption trackerUpdateIntervall = CustomOption.AddNumber("106", "Tracker Update Intervall", 5f, 2.5f, 30f, 2.5f);
        public static CustomNumberOption snitchLeftTasksForImpostors = CustomOption.AddNumber("111", "Task Count Where Impostors See Snitch", 1f, 0f, 5f, 1f);
        public static CustomNumberOption jackalKillCooldown = CustomOption.AddNumber("116", "Jackal/Sidekick Kill Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomNumberOption jackalCreateSidekickCooldown = CustomOption.AddNumber("117", "Jackal Create Sidekick Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomToggleOption jackalCanUseVents = CustomOption.AddToggle("118", "Jackal can use vents", true);
        public static CustomToggleOption jackalCanCreateSidekick = CustomOption.AddToggle("119", "Jackal can create a sidekick", false);
        public static CustomToggleOption sidekickPromotesToJackal = CustomOption.AddToggle("120", "Sidekick gets promoted to Jackal on Jackal death", false);
        public static CustomToggleOption sidekickCanKill = CustomOption.AddToggle("121", "Sidekick can kill", false);
        public static CustomToggleOption sidekickCanUseVents = CustomOption.AddToggle("122", "Sidekick can use vents", true);
        public static CustomToggleOption jackalPromotedFromSidekickCanCreateSidekick = CustomOption.AddToggle("123", "Jackals promoted from Sidekick can create a Sidekick", true);
        public static CustomToggleOption jackalCanCreateSidekickFromImpostor = CustomOption.AddToggle("124", "Jackals can make an Impostor to his Sidekick", true);


        public static List<CustomOption> options = new List<CustomOption>();
        public static int optionsPage = 1;

        public static ConfigEntry<bool> DebugMode { get; private set; }

        public override void Load() {
            DebugMode  = Config.Bind("Custom", "Enable Debug Mode", false);

            CustomOption.ShamelessPlug = false;
            Harmony.PatchAll();
        }

        public static string cs(Color c) {
            return string.Format("[{0:X2}{1:X2}{2:X2}{3:X2}]", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a));
        }
 
        private static byte ToByte(float f) {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public TheOtherRolesPlugin() {
            options = new List<CustomOption>() {
                presetSelection,

                crewmateRolesCount,
                impostorRolesCount,

                mafiaSpawnRate,
                morphlingSpawnRate,
                camouflagerSpawnRate,
                vampireSpawnRate,

                loversSpawnRate,
                jesterSpawnRate,
                shifterSpawnRate,

                mayorSpawnRate,
                engineerSpawnRate,
                sheriffSpawnRate,
                lighterSpawnRate,
                detectiveSpawnRate,
                timeMasterSpawnRate,
                medicSpawnRate,
                swapperSpawnRate,
                seerSpawnRate,
                spySpawnRate,
                childSpawnRate,
                // bountyHunterSpawnRate,
                trackerSpawnRate,
                snitchSpawnRate,
                jackalSpawnRate,

                janitorCooldown,
                morphlingCooldown,
                camouflagerCooldown,
                vampireKillDelay,
                vampireCooldown,
                vampireCanKillNearGarlics,

                loversImpLoverRate,
                loversBothDie,
                shifterCooldown,

                sheriffCooldown,
                jesterCanDieToSheriff,
                lighterVision,
                detectiveAnonymousFootprints,
                detectiveFootprintIntervall,
                detectiveFootprintDuration,
                timeMasterCooldown,
                timeMasterRewindTime,
                timeMasterReviveDuringRewind,
                medicReportNameDuration,
                medicReportColorDuration,
                medicShowShielded,
                medicShowAttemptToShielded,
                seerCooldown,
                seerChanceOfSeeingRight,
                seerKindOfInfo,
                seerPlayersWithNotification,
                spyCooldown,
                spySpyingDuration,
                childGrowingUpDuration,
                // bountyHunterNotifyBounty,
                trackerUpdateIntervall,
                snitchLeftTasksForImpostors,

                jackalKillCooldown,
                jackalCreateSidekickCooldown,
                jackalCanUseVents,
                jackalCanCreateSidekick,
                sidekickPromotesToJackal,
                sidekickCanKill,
                sidekickCanUseVents,
                jackalPromotedFromSidekickCanCreateSidekick,
                jackalCanCreateSidekickFromImpostor
            };
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

    // Debugging tools
    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class DebugManager
    {
        private static readonly System.Random random = new System.Random((int)DateTime.Now.Ticks);
        private static List<PlayerControl> bots = new List<PlayerControl>();

        public static void Postfix(KeyboardJoystick __instance)
        {
            if (!TheOtherRolesPlugin.DebugMode.Value) return;

            // Spawn dummys
            if (Input.GetKeyDown(KeyCode.F)) {
                var playerControl = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
                var i = playerControl.PlayerId = (byte) GameData.Instance.GetAvailableId();

                bots.Add(playerControl);
                GameData.Instance.AddPlayer(playerControl);
                AmongUsClient.Instance.Spawn(playerControl, -2, SpawnFlags.None);
                
                playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
                playerControl.GetComponent<DummyBehaviour>().enabled = true;
                playerControl.NetTransform.enabled = false;
                playerControl.SetName(RandomString(10));
                playerControl.SetColor((byte) random.Next(Palette.PlayerColors.Length));
                playerControl.SetHat((uint) random.Next(HatManager.Instance.AllHats.Count),playerControl.Data.ColorId);
                playerControl.SetPet((uint) random.Next(HatManager.Instance.AllPets.Count));
                playerControl.SetSkin((uint) random.Next(HatManager.Instance.AllSkins.Count));
                GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
            }

            // Terminate round
            if(Input.GetKeyDown(KeyCode.L)) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ForceEnd, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.forceEnd();
            }
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

}
