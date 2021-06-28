  
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Hazel;
using System;
using UnhollowerBaseLib;

namespace TheOtherRoles.Patches {
    public class GameStartManagerPatch  {
        public static Dictionary<int, PlayerVersion> playerVersions = new Dictionary<int, PlayerVersion>();
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
            private static int kc = 0;
            private static KeyCode[] ks = new [] { KeyCode.UpArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.B, KeyCode.A, KeyCode.Return };
        
            public static void Prefix(GameStartManager __instance) {
                if (!AmongUsClient.Instance.AmHost  || !GameData.Instance) return; // Not host or no instance
                update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
            }

            public static void Postfix(GameStartManager __instance) {
                // Send version as soon as PlayerControl.LocalPlayer exists
                if (PlayerControl.LocalPlayer != null && !versionSent) {
                    versionSent = true;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VersionHandshake, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)TheOtherRolesPlugin.Version.Major);
                    writer.Write((byte)TheOtherRolesPlugin.Version.Minor);
                    writer.Write((byte)TheOtherRolesPlugin.Version.Build);
                    writer.WritePacked(AmongUsClient.Instance.ClientId);
                    writer.Write((byte)(TheOtherRolesPlugin.Version.Revision < 0 ? 0xFF : TheOtherRolesPlugin.Version.Revision));
                    writer.Write(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.versionHandshake(TheOtherRolesPlugin.Version.Major, TheOtherRolesPlugin.Version.Minor, TheOtherRolesPlugin.Version.Build, TheOtherRolesPlugin.Version.Revision, Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId, AmongUsClient.Instance.ClientId);
                }
                
                if(kc < ks.Length && Input.GetKeyDown(ks[kc])) {
                    kc++;
                } else if(Input.anyKeyDown) {
                    kc = 0;
                }

                if(kc == ks.Length) {
                    kc = 0;

                    // Random Color
                    byte colorId = (byte)TheOtherRoles.rnd.Next(0, Palette.PlayerColors.Length);
                    SaveManager.BodyColor = (byte)colorId;
                    if (PlayerControl.LocalPlayer) PlayerControl.LocalPlayer.CmdCheckColor(colorId);

                    // Random Hat
                    var hats = HatManager.Instance.GetUnlockedHats();
                    var unlockedHatIndex = TheOtherRoles.rnd.Next(0, hats.Length);
                    var hatId = (uint)HatManager.Instance.AllHats.IndexOf(hats[unlockedHatIndex]);
                    if (PlayerControl.LocalPlayer) PlayerControl.LocalPlayer.RpcSetHat(hatId);

                    // Random Skin
                    var skins = HatManager.Instance.GetUnlockedSkins();
                    var unlockedSkinIndex = TheOtherRoles.rnd.Next(0, skins.Length);
                    var skinId = (uint)HatManager.Instance.AllSkins.IndexOf(skins[unlockedSkinIndex]);
                    if (PlayerControl.LocalPlayer) PlayerControl.LocalPlayer.RpcSetSkin(skinId);
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
                            message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} 擁有不同版本的或沒有 The Other Roles\n</color>";
                        } else {
                            PlayerVersion PV = playerVersions[client.Id];
                            int diff = TheOtherRolesPlugin.Version.CompareTo(PV.version);
                            if (diff > 0) {
                                message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} 擁有舊的 The Other Roles (v{playerVersions[client.Id].version.ToString()})\n</color>";
                                blockStart = true;
                            } else if (diff < 0) {
                                message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} 擁有新的 The Other Roles (v{playerVersions[client.Id].version.ToString()})\n</color>";
                                blockStart = true;
                            } else if (!PV.GuidMatches()) { // version presumably matches, check if Guid matches
                                message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} 擁有修改過的 TOR v{playerVersions[client.Id].version.ToString()} <size=30%>({PV.guid.ToString()})</size>\n</color>";
                                blockStart = true;
                            }
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

        public class PlayerVersion {
            public readonly Version version;
            public readonly Guid guid;

            public PlayerVersion(Version version, Guid guid) {
                this.version = version;
                this.guid = guid;
            }

            public bool GuidMatches() {
                return Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.Equals(this.guid);
            }
        }
    }
}
