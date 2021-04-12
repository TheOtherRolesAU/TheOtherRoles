using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;

namespace TheOtherRoles
{
    [BepInPlugin(Id, "The Other Roles", "2.2.0")]
    [BepInProcess("Among Us.exe")]
    public class TheOtherRolesPlugin : BasePlugin
    {
        public const string Id = "me.eisbison.theotherroles";
        public Harmony Harmony { get; } = new Harmony(Id);
        public static TheOtherRolesPlugin Instance;

        public static int optionsPage = 1;

        public static ConfigEntry<bool> DebugMode { get; private set; }
        public ConfigEntry<string> Ip { get; set; }
        public ConfigEntry<ushort> Port { get; set; }

        public override void Load() {
            DebugMode  = Config.Bind("Custom", "Enable Debug Mode", false);
            Ip = Config.Bind("Custom", "Custom Server IP", "127.0.0.1");
            Port = Config.Bind("Custom", "Custom Server Port", (ushort)22023);

            IRegionInfo customRegion = new DnsRegionInfo(Ip.Value, "Custom", StringNames.NoTranslation, Ip.Value, Port.Value).Cast<IRegionInfo>();
            ServerManager serverManager = DestroyableSingleton<ServerManager>.CMJOLNCMAPD;
            IRegionInfo[] regions = ServerManager.DefaultRegions;

            regions = regions.Concat(new IRegionInfo[] { customRegion }).ToArray();
            ServerManager.DefaultRegions = regions;
            serverManager.GDOLGIJJLBL = regions;
            serverManager.SaveServers();

            IGDMNKLDEPI.OHDJGJFDDDP = IGDMNKLDEPI.AHFOOEHOFNC = Enumerable.Repeat(3, 15).ToArray(); // Max Imp = Recommended Imp = 3
            IGDMNKLDEPI.GNPDNKOKIOK = Enumerable.Repeat(4, 15).ToArray(); // Min Players = 4

            DebugMode = Config.Bind("Custom", "Enable Debug Mode", false);
            Instance = this;
            CustomOptionHolder.Load();
            
            Harmony.PatchAll();
        }
    }

    // Deactivate bans, since I always leave my local testing game and ban myself
    [HarmonyPatch(typeof(MOJBJJPEFPG), nameof(MOJBJJPEFPG.IJMECJONBHM), MethodType.Getter)]
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
                AmongUsClient.Instance.Spawn(playerControl, -2, EOCEBECJBHG.None);
                
                playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
                playerControl.GetComponent<DummyBehaviour>().enabled = true;
                playerControl.NetTransform.enabled = false;
                playerControl.SetName(RandomString(10));
                playerControl.SetColor((byte) random.Next(GLNPIJPGGNJ.CALCLMEEPGL.Length));
                playerControl.SetHat((uint) random.Next(HatManager.CMJOLNCMAPD.AllHats.Count), playerControl.IDOFAMCIJKE.JFHFMIKFHGG);
                playerControl.SetPet((uint) random.Next(HatManager.CMJOLNCMAPD.AllPets.Count));
                playerControl.SetSkin((uint) random.Next(HatManager.CMJOLNCMAPD.AllSkins.Count));
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
