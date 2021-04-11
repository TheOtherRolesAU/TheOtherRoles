  
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using Hazel;
using System;

using Palette = GLNPIJPGGNJ;

namespace TheOtherRoles {
    public class GameStartManagerPatch  {
        public static Dictionary<byte, uint> playerVersions = new Dictionary<byte, uint>();
        private static float timer = 600f;
        private static bool versionSent = false;

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch {
            public static void Postfix() {
                // Refresh version infos
                playerVersions = new Dictionary<byte, uint>();
                versionSent = false;

                // Reset lobby countdown timer
                timer = 600f; 
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch {
            private static bool update = false;
            private static string currentText = "";

            public static void Prefix(GameStartManager __instance) {
                if (!AmongUsClient.Instance.CBKCIKKEJHI  || !GameData.Instance) return; // Not host or no instance
                update = GameData.Instance.BCFPPIDIMJK != __instance.GGIPHNCFKFH;
            }

            public static void Postfix(GameStartManager __instance) {
                // Send version as soon as PlayerControl.LocalPlayer exists
                if (PlayerControl.LocalPlayer != null && !versionSent) {
                    versionSent = true;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VersionHandshake, Hazel.SendOption.Reliable, -1);
                    uint version = Convert.ToUInt32(TheOtherRolesPlugin.Version.Replace(".", string.Empty));
                    writer.WritePacked(version);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.versionHandshake(version, PlayerControl.LocalPlayer.PlayerId);
                }

                // Host update with version handshake infos
                if (AmongUsClient.Instance.CBKCIKKEJHI) {
                    uint hostVersion = Convert.ToUInt32(TheOtherRolesPlugin.Version.Replace(".", string.Empty));
                    bool blockStart = false;
                    string message = "";
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                        if (!playerVersions.ContainsKey(player.PlayerId))  {
                            blockStart = true;
                            message += $"[FF0000FF]{player.IDOFAMCIJKE.HGGCLJHCDBM} has an outdated or no version of The Other Roles\n";
                            System.Console.WriteLine(player.IDOFAMCIJKE.HGGCLJHCDBM);
                        } else if (playerVersions[player.PlayerId] != hostVersion) {
                            blockStart = true;
                            message += $"[FF0000FF]{player.IDOFAMCIJKE.HGGCLJHCDBM} has an outdated version ({playerVersions[player.PlayerId]}) of The Other Roles\n";
                        }
                    }
                    if (blockStart) {
                        __instance.StartButton.color = Palette.POCKGPCFGOE;
                        __instance.GameStartText.Text = message;
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    } else {
                        __instance.StartButton.color = ((__instance.GGIPHNCFKFH >= __instance.MinPlayers) ? Palette.MKAFGNEBHKC : Palette.POCKGPCFGOE);
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                    }
                }

                // Lobby timer
                if (!AmongUsClient.Instance.CBKCIKKEJHI  || !GameData.Instance) return; // Not host or no instance

                if (update) currentText = __instance.PlayerCounter.Text;

                timer = Mathf.Max(0f, timer -= Time.deltaTime);
                int minutes = (int)timer / 60;
                int seconds = (int)timer % 60;
                string suffix = $" ({minutes:00}:{seconds:00})";

                __instance.PlayerCounter.Text = currentText + suffix;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        public class GameStartManagerBeginGame {
            public static bool Prefix(GameStartManager __instance) {
                // Block game start if not everyone has the same mod version
                bool continueStart = true;
                if (AmongUsClient.Instance.CBKCIKKEJHI) {
                    uint hostVersion = Convert.ToUInt32(TheOtherRolesPlugin.Version.Replace(".", string.Empty));
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                        if (!playerVersions.ContainsKey(player.PlayerId) || playerVersions[player.PlayerId] != hostVersion) continueStart = false;
                    }
                }
                return continueStart;
            }
        }
    }
}