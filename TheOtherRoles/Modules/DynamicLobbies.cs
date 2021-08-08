using System;
using HarmonyLib;
using Hazel;
using InnerNet;

namespace TheOtherRoles.Modules
{
    [HarmonyPatch]
    public static class DynamicLobbies
    {
        private static int lobbyLimit = 15;

        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        private static class SendChatPatch
        {
            private static bool Prefix(ChatController __instance)
            {
                var text = __instance.TextArea.text;
                var handled = false;
                if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
                    if (text.ToLower()
                        .StartsWith("/size ")) // Unfortunately server holds this - need to do more trickery
                        if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.CanBan())
                        {
                            // checking both just cause
                            handled = true;
                            if (!int.TryParse(text.Substring(6), out lobbyLimit))
                            {
                                __instance.AddChat(PlayerControl.LocalPlayer, "Invalid Size\nUsage: /size {amount}");
                            }
                            else
                            {
                                lobbyLimit = Math.Clamp(lobbyLimit, 4, 15);
                                if (lobbyLimit != PlayerControl.GameOptions.MaxPlayers)
                                {
                                    PlayerControl.GameOptions.MaxPlayers = lobbyLimit;
                                    DestroyableSingleton<GameStartManager>.Instance.LastPlayerCount = lobbyLimit;
                                    PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
                                    __instance.AddChat(PlayerControl.LocalPlayer,
                                        $"Lobby Size changed to {lobbyLimit} players");
                                }
                                else
                                {
                                    __instance.AddChat(PlayerControl.LocalPlayer,
                                        $"Lobby Size is already {lobbyLimit}");
                                }
                            }
                        }

                if (!handled) return true;
                __instance.TextArea.Clear();
                __instance.quickChatMenu.ResetGlyphs();

                return false;
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HostGame))]
        public static class InnerNetClientHostPatch
        {
            public static void Prefix([HarmonyArgument(0)] GameOptionsData settings)
            {
                lobbyLimit = settings.MaxPlayers;
                settings.MaxPlayers = 15; // Force 15 Player Lobby on Server
                SaveManager.ChatModeType = QuickChatModes.FreeChatOrQuickChat;
            }

            public static void Postfix([HarmonyArgument(0)] GameOptionsData settings)
            {
                settings.MaxPlayers = lobbyLimit;
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
        public static class InnerNetClientJoinPatch
        {
            public static void Prefix()
            {
                SaveManager.ChatModeType = QuickChatModes.FreeChatOrQuickChat;
            }
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public static class AmongUsClientOnPlayerJoined
        {
            public static bool Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
            {
                if (lobbyLimit >= __instance.allClients.Count) return true;
                // TODO: Fix this canceling start
                DisconnectPlayer(__instance, client.Id);
                return false;
            }

            private static void DisconnectPlayer(InnerNetClient @this, int clientId)
            {
                if (!@this.AmHost) return;
                var messageWriter = MessageWriter.Get(SendOption.Reliable);
                messageWriter.StartMessage(4);
                messageWriter.Write(@this.GameId);
                messageWriter.WritePacked(clientId);
                messageWriter.Write((byte) DisconnectReasons.GameFull);
                messageWriter.EndMessage();
                @this.SendOrDisconnect(messageWriter);
                messageWriter.Recycle();
            }
        }
    }
}