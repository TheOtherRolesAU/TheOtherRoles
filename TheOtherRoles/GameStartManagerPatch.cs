  
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using Hazel;
using System;
using UnhollowerBaseLib;

namespace TheOtherRoles {
    public class GameStartManagerPatch  {
        public static Dictionary<int, Tuple<byte, byte, byte>> playerVersions = new Dictionary<int, Tuple<byte, byte, byte>>();
        private static float timer = 600f;
        private static bool versionSent = false;
        private static string lobbyCodeText = "";

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch {
            public static void Postfix(GameStartManager __instance) {
                // Trigger version refresh
                versionSent = false;
                // Reset lobby countdown timer
                timer = 600f; 
                // Copy lobby code
                string code = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
                GUIUtility.systemCopyBuffer = code;
                lobbyCodeText = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoomCode, new Il2CppReferenceArray<Il2CppSystem.Object>(0)) + "\r\n" + code;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch {
            private static bool update = false;
            private static string currentText = "";

            public static void Prefix(GameStartManager __instance) {
                if (!AmongUsClient.Instance.AmHost  || !GameData.Instance) return; // Not host or no instance
                update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
            }

            public static void Postfix(GameStartManager __instance) {
                // Send version as soon as PlayerControl.LocalPlayer exists
                if (PlayerControl.LocalPlayer != null && !versionSent) {
                    versionSent = true;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VersionHandshake, Hazel.SendOption.Reliable, -1);
                    writer.Write(TheOtherRolesPlugin.Major);
                    writer.Write(TheOtherRolesPlugin.Minor);
                    writer.Write(TheOtherRolesPlugin.Patch);
                    writer.WritePacked(AmongUsClient.Instance.ClientId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.versionHandshake(TheOtherRolesPlugin.Major, TheOtherRolesPlugin.Minor, TheOtherRolesPlugin.Patch, AmongUsClient.Instance.ClientId);
                }

                // Host update with version handshake infos
                if (AmongUsClient.Instance.AmHost) {
                    bool blockStart = false;
                    string message = "";
                    foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.ToArray()) {
                        if (client.Character == null) continue;
                        var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                        if (dummyComponent != null && dummyComponent.enabled)
                            continue;
                        else if (!playerVersions.ContainsKey(client.Id))  {
                            blockStart = true;
                            message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} has an outdated or no version of The Other Roles\n</color>";
                        } else if (playerVersions[client.Id].Item1 != TheOtherRolesPlugin.Major || playerVersions[client.Id].Item2 != TheOtherRolesPlugin.Minor || playerVersions[client.Id].Item3 != TheOtherRolesPlugin.Patch) {
                            blockStart = true;
                            message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} has an outdated version (v{playerVersions[client.Id].Item1}.{playerVersions[client.Id].Item2}.{playerVersions[client.Id].Item3}) of The Other Roles\n</color>";
                        }
                    }
                    if (blockStart) {
                        // __instance.StartButton.color = Palette.DisabledClear; // Allow the start for this version to test the feature, blocking it with the next version
                        __instance.GameStartText.text = message;
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    } else {
                        // __instance.StartButton.color = ((__instance.LastPlayerCount >= __instance.MinPlayers) ? Palette.EnabledColor : Palette.DisabledClear); // Allow the start for this version to test the feature, blocking it with the next version
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                    }
                }

                // Lobby code replacement
                __instance.GameRoomName.text = TheOtherRolesPlugin.StreamerMode.Value ? $"<color={TheOtherRolesPlugin.StreamerModeReplacementColor.Value}>{TheOtherRolesPlugin.StreamerModeReplacementText.Value}</color>" : lobbyCodeText;

                // Lobby timer
                if (!AmongUsClient.Instance.AmHost || !GameData.Instance) return; // Not host or no instance

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
                // if (AmongUsClient.Instance.AmHost) {
                //     foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients) {
                //         if (client.Character == null) continue;
                //         var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                //         if (dummyComponent != null && dummyComponent.enabled) continue;
                //         if (!playerVersions.ContainsKey(client.Id) || (playerVersions[client.Id].Item1 != TheOtherRolesPlugin.Major || playerVersions[client.Id].Item2 != TheOtherRolesPlugin.Minor || playerVersions[client.Id].Item3 != TheOtherRolesPlugin.Patch))
                //             continueStart = false;
                //     }
                // }
                return continueStart;
            }
        }
    }
}
