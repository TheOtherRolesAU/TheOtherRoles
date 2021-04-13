  
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using Hazel;
using System;

using Palette = BLMBFIODBKL;

namespace TheOtherRoles {
    public class GameStartManagerPatch  {
        public static Dictionary<byte, Tuple<byte, byte, byte>> playerVersions = new Dictionary<byte, Tuple<byte, byte, byte>>();
        private static float timer = 600f;
        private static bool versionSent = false;

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch {
            public static void Postfix() {
                // Refresh version infos
                playerVersions = new Dictionary<byte, Tuple<byte, byte, byte>>();
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
                if (!AmongUsClient.Instance.HHBLOCGKFAB  || !GameData.Instance) return; // Not host or no instance
                update = GameData.Instance.MFDAIFHGKMG != __instance.OBFONKJNJFF;
            }

            public static void Postfix(GameStartManager __instance) {
                // Send version as soon as PlayerControl.LocalPlayer exists
                if (PlayerControl.LocalPlayer != null && !versionSent) {
                    versionSent = true;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VersionHandshake, Hazel.SendOption.Reliable, -1);
                    writer.Write(TheOtherRolesPlugin.Major);
                    writer.Write(TheOtherRolesPlugin.Minor);
                    writer.Write(TheOtherRolesPlugin.Patch);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.versionHandshake(TheOtherRolesPlugin.Major, TheOtherRolesPlugin.Minor, TheOtherRolesPlugin.Patch, PlayerControl.LocalPlayer.PlayerId);
                }

                // Host update with version handshake infos
                if (AmongUsClient.Instance.HHBLOCGKFAB) {
                    bool blockStart = false;
                    string message = "";
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                        var dummyComponent = player.GetComponent<DummyBehaviour>();
                        if (dummyComponent != null && dummyComponent.enabled)
                            continue;
                        else if (!playerVersions.ContainsKey(player.PlayerId))  {
                            blockStart = true;
                            message += $"<color=#FF0000FF>{player.PPMOEEPBHJO.PCLLABJCIPC} has an outdated or no version of The Other Roles\n</color>";
                        } else if (playerVersions[player.PlayerId].Item1 != TheOtherRolesPlugin.Major || playerVersions[player.PlayerId].Item2 != TheOtherRolesPlugin.Minor || playerVersions[player.PlayerId].Item3 != TheOtherRolesPlugin.Patch) {
                            blockStart = true;
                            message += $"<color=#FF0000FF>{player.PPMOEEPBHJO.PCLLABJCIPC} has an outdated version (v{playerVersions[player.PlayerId].Item1}.{playerVersions[player.PlayerId].Item2}.{playerVersions[player.PlayerId].Item3}) of The Other Roles\n</color>";
                        }
                    }
                    if (blockStart) {
                        // __instance.StartButton.color = Palette.ILFJLECIGDB; // Allow the start for this version to test the feature, blocking it with the next version
                        __instance.GameStartText.text = message;
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    } else {
                        // __instance.StartButton.color = ((__instance.OBFONKJNJFF >= __instance.MinPlayers) ? Palette.BJENLBHMKAI : Palette.ILFJLECIGDB); // Allow the start for this version to test the feature, blocking it with the next version
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                    }
                }

                // Lobby timer
                if (!AmongUsClient.Instance.HHBLOCGKFAB || !GameData.Instance) return; // Not host or no instance

                if (update) currentText = __instance.PlayerCounter.text;

                timer = Mathf.Max(0f, timer -= Time.deltaTime);
                int minutes = (int)timer / 60;
                int seconds = (int)timer % 60;
                string suffix = $" ({minutes:00}:{seconds:00})";

                __instance.PlayerCounter.text = currentText + suffix;
                __instance.PlayerCounter.autoSizeTextContainer = true;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        public class GameStartManagerBeginGame {
            public static bool Prefix(GameStartManager __instance) {
                // Block game start if not everyone has the same mod version
                bool continueStart = true;

                // Allow the start for this version to test the feature, blocking it with the next version
                // if (AmongUsClient.Instance.HHBLOCGKFAB) {
                //     uint hostVersion = Convert.ToUInt32(TheOtherRolesPlugin.Version.Replace(".", string.Empty));
                //     foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                //         var dummyComponent = player.GetComponent<DummyBehaviour>();
                //         if (dummyComponent != null && dummyComponent.enabled) continue;
                //         if (!playerVersions.ContainsKey(player.PlayerId) || (playerVersions[player.PlayerId].Item1 != TheOtherRolesPlugin.Major || playerVersions[player.PlayerId].Item2 != TheOtherRolesPlugin.Minor || playerVersions[player.PlayerId].Item3 != TheOtherRolesPlugin.Patch))
                //             continueStart = false;
                //     }
                // }
                return continueStart;
            }
        }
    }
}