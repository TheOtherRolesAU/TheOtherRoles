using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using InnerNet;
using TheOtherRoles.Modules;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace TheOtherRoles
{
    [BepInPlugin(Id, "The Other Roles", VersionString)]
    [BepInProcess("Among Us.exe")]
    public class TheOtherRolesPlugin : BasePlugin
    {
        public const string Id = "me.eisbison.theotherroles";
        public const string VersionString = "2.8.1";
        public static Version Version = Version.Parse(VersionString);
        public static TheOtherRolesPlugin Instance;

        public static int optionsPage = 1;

        private static Sprite modStamp;

        private static IRegionInfo[] defaultRegions;

        private Harmony Harmony { get; } = new(Id);

        public static ConfigEntry<bool> DebugMode { get; private set; }
        public static ConfigEntry<bool> StreamerMode { get; private set; }
        public static ConfigEntry<bool> GhostsSeeTasks { get; private set; }
        public static ConfigEntry<bool> GhostsSeeRoles { get; private set; }
        public static ConfigEntry<bool> GhostsSeeVotes { get; private set; }
        public static ConfigEntry<bool> ShowRoleSummary { get; private set; }
        public static ConfigEntry<string> StreamerModeReplacementText { get; private set; }
        public static ConfigEntry<string> StreamerModeReplacementColor { get; private set; }
        public static ConfigEntry<string> Ip { get; private set; }
        public static ConfigEntry<ushort> Port { get; private set; }

        public static void UpdateRegions()
        {
            var serverManager = DestroyableSingleton<ServerManager>.Instance;
            var regions = defaultRegions;

            var customRegion = new DnsRegionInfo(Ip.Value, "Custom", StringNames.NoTranslation, Ip.Value, Port.Value);
            regions = regions.Concat(new[] {customRegion.Cast<IRegionInfo>()}).ToArray();
            ServerManager.DefaultRegions = regions;
            serverManager.AvailableRegions = regions;
        }

        public override void Load()
        {
            DebugMode = Config.Bind("Custom", "Enable Debug Mode", false);
            StreamerMode = Config.Bind("Custom", "Enable Streamer Mode", false);
            GhostsSeeTasks = Config.Bind("Custom", "Ghosts See Remaining Tasks", true);
            GhostsSeeRoles = Config.Bind("Custom", "Ghosts See Roles", true);
            GhostsSeeVotes = Config.Bind("Custom", "Ghosts See Votes", true);
            ShowRoleSummary = Config.Bind("Custom", "Show Role Summary", true);
            StreamerModeReplacementText =
                Config.Bind("Custom", "Streamer Mode Replacement Text", "\n\nThe Other Roles");
            StreamerModeReplacementColor =
                Config.Bind("Custom", "Streamer Mode Replacement Text Hex Color", "#87AAF5FF");


            Ip = Config.Bind("Custom", "Custom Server IP", "127.0.0.1");
            Port = Config.Bind("Custom", "Custom Server Port", (ushort) 22023);
            defaultRegions = ServerManager.DefaultRegions;

            UpdateRegions();

            GameOptionsData.RecommendedImpostors =
                GameOptionsData.MaxImpostors = Enumerable.Repeat(3, 16).ToArray(); // Max Imp = Recommended Imp = 3
            GameOptionsData.MinPlayers = Enumerable.Repeat(4, 15).ToArray(); // Min Players = 4

            DebugMode = Config.Bind("Custom", "Enable Debug Mode", false);
            Instance = this;
            CustomOptionHolder.Load();
            CustomColors.Load();

            Harmony.PatchAll();
        }

        public static Sprite GetModStamp()
        {
            if (modStamp) return modStamp;
            return modStamp = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.ModStamp.png", 150f);
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

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Awake))]
    public static class ChatControllerAwakePatch
    {
        private static void Prefix()
        {
            if (EOSManager.Instance.IsMinor()) return;
            SaveManager.chatModeType = 1;
            SaveManager.isGuest = false;
        }
    }

    // Debugging tools
    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class DebugManager
    {
        private static readonly Random Random = new((int) DateTime.Now.Ticks);

        public static void Postfix()
        {
            if (!TheOtherRolesPlugin.DebugMode.Value) return;

            // Spawn dummies
            if (Input.GetKeyDown(KeyCode.F))
            {
                var playerControl = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
                playerControl.PlayerId = (byte) GameData.Instance.GetAvailableId();

                GameData.Instance.AddPlayer(playerControl);
                AmongUsClient.Instance.Spawn(playerControl, -2, SpawnFlags.None);

                playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
                playerControl.GetComponent<DummyBehaviour>().enabled = true;
                playerControl.NetTransform.enabled = false;
                playerControl.SetName(RandomString(10));
                playerControl.SetColor((byte) Random.Next(Palette.PlayerColors.Length));
                playerControl.SetHat((uint) Random.Next(HatManager.Instance.AllHats.Count), playerControl.Data.ColorId);
                playerControl.SetPet((uint) Random.Next(HatManager.Instance.AllPets.Count));
                playerControl.SetSkin((uint) Random.Next(HatManager.Instance.AllSkins.Count));
                GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
            }

            // Terminate round
            if (!Input.GetKeyDown(KeyCode.L)) return;
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRPC.ForceEnd, SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.ForceEnd();
        }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}