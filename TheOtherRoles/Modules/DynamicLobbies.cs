using System;
using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;

namespace TheOtherRoles.Modules {
    [HarmonyPatch]
    public static class DynamicLobbies {
        public static int LobbyLimit = 15;
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        private static class SendChatPatch {
            static bool Prefix(ChatController __instance) {
                string text = __instance.freeChatField.Text;
                bool handled = false;
                if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) {
                    if (text.ToLower().StartsWith("/size ")) { // Unfortunately server holds this - need to do more trickery
                            if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.CanBan()) { // checking both just cause
                                handled = true;
                                if (!Int32.TryParse(text.Substring(6), out LobbyLimit)) {
                                    __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, "Invalid Size\nUsage: /size {amount}");
                                } else {
                                    LobbyLimit = Math.Clamp(LobbyLimit, 4, 15);
                                    if (LobbyLimit != GameOptionsManager.Instance.currentNormalGameOptions.MaxPlayers) {
                                        GameOptionsManager.Instance.currentNormalGameOptions.MaxPlayers = LobbyLimit;
                                        FastDestroyableSingleton<GameStartManager>.Instance.LastPlayerCount = LobbyLimit;
                                        CachedPlayer.LocalPlayer.PlayerControl.RpcSyncSettings(GameOptionsManager.Instance.gameOptionsFactory.ToBytes(GameOptionsManager.Instance.currentGameOptions, false));  // TODO Maybe simpler?? 
                                        __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, $"Lobby Size changed to {LobbyLimit} players");
                                    } else {
                                        __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, $"Lobby Size is already {LobbyLimit}");
                                    }
                                }
                            }
                        }
                }
                if (handled) {
                    __instance.freeChatField.Clear();
                    __instance.quickChatMenu.Clear();
                }
                return !handled;
            }
        }
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HostGame))]
        public static class InnerNetClientHostPatch {
            public static void Prefix(InnerNet.InnerNetClient __instance, [HarmonyArgument(0)] GameOptionsData settings) {
                int maxPlayers;
                try {
                    maxPlayers = GameOptionsManager.Instance.currentNormalGameOptions.MaxPlayers;
                }
                catch {
                    maxPlayers = 15;
                }
                DynamicLobbies.LobbyLimit = maxPlayers;
                settings.MaxPlayers = 15; // Force 15 Player Lobby on Server
                DataManager.Settings.Multiplayer.ChatMode = InnerNet.QuickChatModes.FreeChatOrQuickChat;
            }
            public static void Postfix(InnerNet.InnerNetClient __instance, [HarmonyArgument(0)] GameOptionsData settings) {
                settings.MaxPlayers = DynamicLobbies.LobbyLimit;
            }
        }
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
        public static class InnerNetClientJoinPatch {
            public static void Prefix(InnerNet.InnerNetClient __instance) {
                DataManager.Settings.Multiplayer.ChatMode = InnerNet.QuickChatModes.FreeChatOrQuickChat;
            }
        }
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public static class AmongUsClientOnPlayerJoined {
            public static bool Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client) {
                if (LobbyLimit < __instance.allClients.Count) { // TODO: Fix this canceling start
                    DisconnectPlayer(__instance, client.Id);
                    return false;
                }
                return true;
            }

            private static void DisconnectPlayer(InnerNetClient _this, int clientId) {
			if (!_this.AmHost) {
				return;
			}
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(4);
			messageWriter.Write(_this.GameId);
			messageWriter.WritePacked(clientId);
			messageWriter.Write((byte)DisconnectReasons.GameFull);
			messageWriter.EndMessage();
			_this.SendOrDisconnect(messageWriter);
			messageWriter.Recycle();
            }
        }
    }
}
