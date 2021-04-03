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


        public static CustomStringOption presetSelection = CustomOption.AddString("0", "[CCCC00FF]Preset[]", false, presets);
        public static CustomStringOption crewmateRolesCount = CustomOption.AddString("1", "[CCCC00FF]Number Of Crewmate/Neutral Roles[]", crewmateRoleCaps);
        public static CustomStringOption impostorRolesCount = CustomOption.AddString("2", "[CCCC00FF]Number Of Impostor Roles[]", impostorRoleCaps);

        public static CustomStringOption mafiaSpawnRate = CustomOption.AddString("10", cs(Janitor.color, "Mafia"), rates);
        public static CustomStringOption morphlingSpawnRate = CustomOption.AddString("20", cs(Morphling.color, "Morphling"), rates);
        public static CustomStringOption camouflagerSpawnRate = CustomOption.AddString("30", cs(Camouflager.color, "Camouflager"), rates);
        public static CustomStringOption vampireSpawnRate = CustomOption.AddString("40", cs(Vampire.color, "Vampire"), rates);
        public static CustomStringOption eraserSpawnRate = CustomOption.AddString("230", cs(Eraser.color, "Eraser"), rates);

        public static CustomStringOption childSpawnRate = CustomOption.AddString("180", cs(Child.color, "Child"), rates);
        public static CustomStringOption loversSpawnRate = CustomOption.AddString("50", cs(Lovers.color, "Lovers"), rates);
        public static CustomStringOption jesterSpawnRate = CustomOption.AddString("60", cs(Jester.color, "Jester"), rates);

        public static CustomStringOption shifterSpawnRate = CustomOption.AddString("70", cs(Shifter.color, "Shifter"), rates);
        public static CustomStringOption mayorSpawnRate = CustomOption.AddString("80", cs(Mayor.color, "Mayor"), rates);
        public static CustomStringOption engineerSpawnRate = CustomOption.AddString("90", cs(Engineer.color, "Engineer"), rates);
        public static CustomStringOption sheriffSpawnRate = CustomOption.AddString("100", cs(Sheriff.color, "Sheriff"), rates);
        public static CustomStringOption lighterSpawnRate = CustomOption.AddString("110", cs(Lighter.color, "Lighter"), rates);
        public static CustomStringOption detectiveSpawnRate = CustomOption.AddString("120", cs(Detective.color, "Detective"), rates);
        public static CustomStringOption timeMasterSpawnRate = CustomOption.AddString("130", cs(TimeMaster.color, "Time Master"), rates);
        public static CustomStringOption medicSpawnRate = CustomOption.AddString("140", cs(Medic.color, "Medic"), rates);
        public static CustomStringOption swapperSpawnRate = CustomOption.AddString("150", cs(Swapper.color, "Swapper"), rates);
        public static CustomStringOption seerSpawnRate = CustomOption.AddString("160", cs(Seer.color, "Seer"), rates);
        public static CustomStringOption hackerSpawnRate = CustomOption.AddString("170", cs(Hacker.color, "Hacker"), rates);
        // public static CustomStringOption bountyHunterSpawnRate = CustomOption.AddString("190", cs(BountyHunter.color) + "Bounty Hunter", rates);
        public static CustomStringOption trackerSpawnRate = CustomOption.AddString("200", cs(Tracker.color, "Tracker"), rates);
        public static CustomStringOption snitchSpawnRate = CustomOption.AddString("210", cs(Snitch.color, "Snitch"), rates);
        public static CustomStringOption jackalSpawnRate = CustomOption.AddString("220", cs(Jackal.color, "Jackal"), rates);

        // Map settings
        public static CustomNumberOption maxNumberOfMeetings = CustomOption.AddNumber("3", "Number Of Meetings (excluding Mayor meeting)", 10, 0, 15, 1);

        // Role settings
        public static CustomNumberOption janitorCooldown = CustomOption.AddNumber("11", "Janitor Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomNumberOption morphlingCooldown = CustomOption.AddNumber("21", "Morphling Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomNumberOption camouflagerCooldown = CustomOption.AddNumber("31", "Camouflager Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomNumberOption vampireKillDelay = CustomOption.AddNumber("41", "Vampire Kill Delay", 10f, 1f, 20f, 1f);
        public static CustomNumberOption vampireCooldown = CustomOption.AddNumber("42", "Vampire Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomToggleOption vampireCanKillNearGarlics = CustomOption.AddToggle("43", "Vampire Can Kill Near Garlics", true);
        public static CustomNumberOption eraserCooldown = CustomOption.AddNumber("231", "Eraser Cooldown", 30f, 10f, 120f, 5f);

        public static CustomNumberOption childGrowingUpDuration = CustomOption.AddNumber("181", "Child Growing Up Duration", 400f, 100f, 1500f, 100f);
        public static CustomNumberOption loversImpLoverRate = CustomOption.AddNumber("51", "Chance That One Lover Is Impostor", 30f, 0f, 100f, 10f);
        public static CustomToggleOption loversBothDie = CustomOption.AddToggle("52", "Both Lovers Die", true);

        public static CustomNumberOption sheriffCooldown = CustomOption.AddNumber("101", "Sheriff Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomToggleOption jesterCanDieToSheriff = CustomOption.AddToggle("102", "Sheriff Can Kill The Jester", false);
        public static CustomNumberOption lighterModeLightsOnVision = CustomOption.AddNumber("111", "Lighter Mode Vision On Lights On", 2f, 0.25f, 5f, 0.25f);
        public static CustomNumberOption lighterModeLightsOffVision = CustomOption.AddNumber("112", "Lighter Mode Vision On Lights Off", 0.75f, 0.25f, 5f, 0.25f);
        public static CustomNumberOption lighterCooldown = CustomOption.AddNumber("113", "Lighter Cooldown", 30f, 5f, 120f, 5f);
        public static CustomNumberOption lighterDuration = CustomOption.AddNumber("114", "Lighter Duration", 5f, 2.5f, 60f, 2.5f);
        public static CustomToggleOption detectiveAnonymousFootprints = CustomOption.AddToggle("121", "Anonymous Footprints", false); 
        public static CustomNumberOption detectiveFootprintIntervall = CustomOption.AddNumber("122", "Footprint Intervall", 0.5f, 0.25f, 10f, 0.25f);
        public static CustomNumberOption detectiveFootprintDuration = CustomOption.AddNumber("123", "Footprint Duration", 5f, 0.25f, 10f, 0.25f);
        public static CustomNumberOption detectiveReportNameDuration = CustomOption.AddNumber("124", "Time Where Detective Reports Will Have Name", 0, 0, 60, 2.5f);
        public static CustomNumberOption detectiveReportColorDuration = CustomOption.AddNumber("125", "Time Where Detective Reports Will Have Color Type", 20, 0, 120, 2.5f);
        public static CustomNumberOption timeMasterCooldown = CustomOption.AddNumber("131", "Time Master Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomNumberOption timeMasterRewindTime = CustomOption.AddNumber("132", "Rewind Time", 3f, 1f, 10f, 1f);
        public static CustomStringOption medicShowShielded = CustomOption.AddString("143", "Show Shielded Player", new string[] {"Everyone", "Shielded + Medic", "Medic"});
        public static CustomToggleOption medicShowAttemptToShielded = CustomOption.AddToggle("144", "Shielded Player Sees Murder Attempt", false);
        public static CustomStringOption seerMode = CustomOption.AddString("161", "Seer Mode", new string[]{ "Show Death Flash + Souls", "Show Death Flash", "Show Souls"});
        public static CustomNumberOption seerSoulDuration = CustomOption.AddNumber("162", "Seer Soul Duration", 15f, 0f, 60f, 5f);
        public static CustomNumberOption hackerCooldown = CustomOption.AddNumber("171", "Hacker Cooldown", 30f, 0f, 60f, 5f);
        public static CustomNumberOption hackerHackeringDuration = CustomOption.AddNumber("172", "Hacker Duration", 10f, 2.5f, 60f, 2.5f);
        public static CustomToggleOption hackerOnlyColorType = CustomOption.AddToggle("173", "Hacker Only Sees Color Type", false);
        // public static CustomToggleOption bountyHunterNotifyBounty = CustomOption.AddToggle("191", "Bounty Gets Notified", true); 
        public static CustomNumberOption trackerUpdateIntervall = CustomOption.AddNumber("201", "Tracker Update Intervall", 5f, 2.5f, 30f, 2.5f);
        public static CustomNumberOption snitchLeftTasksForImpostors = CustomOption.AddNumber("211", "Task Count Where Impostors See Snitch", 1f, 0f, 5f, 1f);
        public static CustomNumberOption jackalKillCooldown = CustomOption.AddNumber("221", "Jackal/Sidekick Kill Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomNumberOption jackalCreateSidekickCooldown = CustomOption.AddNumber("222", "Jackal Create Sidekick Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomToggleOption jackalCanUseVents = CustomOption.AddToggle("223", "Jackal can use vents", true);
        public static CustomToggleOption jackalCanCreateSidekick = CustomOption.AddToggle("224", "Jackal can create a sidekick", false);
        public static CustomToggleOption sidekickPromotesToJackal = CustomOption.AddToggle("225", "Sidekick gets promoted to Jackal on Jackal death", false);
        public static CustomToggleOption sidekickCanKill = CustomOption.AddToggle("226", "Sidekick can kill", false);
        public static CustomToggleOption sidekickCanUseVents = CustomOption.AddToggle("227", "Sidekick can use vents", true);
        public static CustomToggleOption jackalPromotedFromSidekickCanCreateSidekick = CustomOption.AddToggle("228", "Jackals promoted from Sidekick can create a Sidekick", true);
        public static CustomToggleOption jackalCanCreateSidekickFromImpostor = CustomOption.AddToggle("229", "Jackals can make an Impostor to his Sidekick", true);

        public static int optionsPage = 1;

        public static ConfigEntry<bool> DebugMode { get; private set; }

        public override void Load() {
            DebugMode  = Config.Bind("Custom", "Enable Debug Mode", false);

            CustomOption.ShamelessPlug = false;
            Harmony.PatchAll();
        }

        public static string cs(Color c, string s) {
            return string.Format("[{0:X2}{1:X2}{2:X2}{3:X2}]{4}[]", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
        }
 
        private static byte ToByte(float f) {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
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
                AmongUsClient.Instance.Spawn(playerControl, -2, InnerNet.SpawnFlags.None);
                
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
