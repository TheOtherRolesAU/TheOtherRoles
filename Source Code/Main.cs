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
using System.Reflection;
using UnhollowerBaseLib;

namespace BonusRoles
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class BonusRolesPlugin : BasePlugin
    {
        public const string Id = "me.eisbison.bonusroles";
        public Harmony Harmony { get; } = new Harmony(Id);

        // Role spawn chances
        public static CustomNumberOption mafiaSpawnChance = CustomOption.AddNumber("Mafia Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption loversSpawnChance = CustomOption.AddNumber("Lovers Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption morphlingSpawnChance = CustomOption.AddNumber("Morphling Spawn Chance", 100, 0, 100, 10);
        public static CustomNumberOption camouflagerSpawnChance = CustomOption.AddNumber("Camouflager Spawn Chance", 100, 0, 100, 10);
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
        public static CustomStringOption ColorBlindComms = CustomOption.AddString("ColorBlind Comms", new string[] { "Enabled", "Disabled" });
        public static CustomNumberOption medicReportNameDuration = CustomOption.AddNumber("Time Where Medic Reports Will Have Name", 10, 0, 60, 2.5f);
        public static CustomNumberOption medicReportColorDuration = CustomOption.AddNumber("Time Where Medic Reports Will Have Color Type", 20, 0, 120, 2.5f);
        public static CustomStringOption medicShowShielded = CustomOption.AddString("Show Shielded Player", new string[] {"Everyone", "Shielded + Medic", "Medic"});
        public static CustomToggleOption medicShowAttemptToShielded = CustomOption.AddToggle("Shielded Player Sees Murder Attempt", false);
        public static CustomNumberOption shifterCooldown = CustomOption.AddNumber("Shifter Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomNumberOption seerCooldown = CustomOption.AddNumber("Seer Cooldown (No Reset After Meeting)", 15f, 30f, 180f, 15f);
        public static CustomNumberOption seerChanceOfSeeingRight = CustomOption.AddNumber("Seer Chance Of Seeing Right", 100, 0, 100, 5);
        public static CustomStringOption seerKindOfInfo = CustomOption.AddString("Info That Seer Reveals", new string[] {"Role", "Good/Bad"});
        public static CustomStringOption seerPlayersWithNotification = CustomOption.AddString("Players That See When They Are Being Revealed", new string[] {"Everyone", "The Good", "The Bad", "Nobody"});
        public static CustomNumberOption spyCooldown = CustomOption.AddNumber("Spy Cooldown", 30f, 10f, 120f, 5f);
        public static CustomNumberOption spySpyingDuration = CustomOption.AddNumber("Spy Duration", 10f, 2.5f, 60f, 2.5f);
        public static CustomNumberOption childGrowingUpDuration = CustomOption.AddNumber("Child Growing Up Duration", 400f, 100f, 1500f, 100f);

        public static ConfigEntry<bool> DebugMode { get; private set; }



                public bool IsCommsActive { get; private set; }
        private IEnumerator LastCoroutine { get; set; }
        public float AnimationDuration { get; set; }
        public Color32 DistortionBackColor { get; set; }
        public Color32 DistortionBodyColor { get; set; }

        public override void Load()
        {
            var duration = new ConfigDefinition("Distortion", "Duration");
            var back = new ConfigDefinition("Distortion", "Back");
            var body = new ConfigDefinition("Distortion", "Body");

            Config.Bind(duration, 2.0f);
            Config.Bind(back, "7F7F7FFF");
            Config.Bind(body, "7F7F7FFF");

            AnimationDuration = (float)Config[duration].BoxedValue;
            DistortionBackColor = Helpers.ColorFromHex((string)Config[back].BoxedValue);
            DistortionBodyColor = Helpers.ColorFromHex((string)Config[body].BoxedValue);
            DebugMode  = Config.Bind("Custom", "Enable Debug Mode", false);

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

    // Debugging tools
    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class DebugManager
    {
        private static readonly System.Random random = new System.Random((int)DateTime.Now.Ticks);
        private static List<PlayerControl> bots = new List<PlayerControl>();

        public static void Postfix(KeyboardJoystick __instance)
        {
            if (!BonusRolesPlugin.DebugMode.Value) return;

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
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ForceEnd, Hazel.SendOption.None, -1);
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
