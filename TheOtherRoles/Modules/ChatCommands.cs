using System;
using HarmonyLib;
using System.Linq;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;

namespace TheOtherRoles.Modules {
    [HarmonyPatch]
    public static class ChatCommands {
        public static bool isLover(this PlayerControl player) => !(player == null) && (player == Lovers.lover1 || player == Lovers.lover2);
        public static bool isTeamJackal(this PlayerControl player) => !(player == null) && (player == Jackal.jackal || player == Sidekick.sidekick);

        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        private static class SendChatPatch {
            static bool Prefix(ChatController __instance) {
                string text = __instance.TextArea.text;
                bool handled = false;
                if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) {
                    if (text.ToLower().StartsWith("/kick ")) {
                        string playerName = text.Substring(6);
                        PlayerControl target = CachedPlayer.AllPlayers.FirstOrDefault(x => x.Data.PlayerName.Equals(playerName));
                        if (target != null && AmongUsClient.Instance != null && AmongUsClient.Instance.CanBan()) {
                            var client = AmongUsClient.Instance.GetClient(target.OwnerId);
                            if (client != null) {
                                AmongUsClient.Instance.KickPlayer(client.Id, false);
                                handled = true;
                            }
                        }
                    } else if (text.ToLower().StartsWith("/ban ")) {
                        string playerName = text.Substring(6);
                        PlayerControl target = CachedPlayer.AllPlayers.FirstOrDefault(x => x.Data.PlayerName.Equals(playerName));
                        if (target != null && AmongUsClient.Instance != null && AmongUsClient.Instance.CanBan()) {
                            var client = AmongUsClient.Instance.GetClient(target.OwnerId);
                            if (client != null) {
                                AmongUsClient.Instance.KickPlayer(client.Id, true);
                                handled = true;
                            }
                        }
                    }
                }
                
                if (AmongUsClient.Instance.GameMode == GameModes.FreePlay) {
                    if (text.ToLower().Equals("/murder")) {
                        CachedPlayer.LocalPlayer.PlayerControl.Exiled();
                        FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(CachedPlayer.LocalPlayer.Data, CachedPlayer.LocalPlayer.Data);
                        handled = true;
                    } else if (text.ToLower().StartsWith("/color ")) {
                        handled = true;
                        int col;
                        if (!Int32.TryParse(text.Substring(7), out col)) {
                            __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, "Unable to parse color id\nUsage: /color {id}");
                        }
                        col = Math.Clamp(col, 0, Palette.PlayerColors.Length - 1);
                        CachedPlayer.LocalPlayer.PlayerControl.SetColor(col);
                        __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, "Changed color succesfully");;
                    } 
                }

                if (text.ToLower().StartsWith("/tp ") && CachedPlayer.LocalPlayer.Data.IsDead) {
                    string playerName = text.Substring(4).ToLower();
                    PlayerControl target = CachedPlayer.AllPlayers.FirstOrDefault(x => x.Data.PlayerName.ToLower().Equals(playerName));
                    if (target != null) {
                        CachedPlayer.LocalPlayer.transform.position = target.transform.position;
                        handled = true;
                    }
                }


                if (text.ToLower().StartsWith("/team")) {
                    if (CachedPlayer.LocalPlayer.PlayerControl.isLover() && CachedPlayer.LocalPlayer.PlayerControl.isTeamJackal() && Jackal.hasChat) {
                        if (Sidekick.sidekick == CachedPlayer.LocalPlayer.PlayerControl) Sidekick.chatTarget = Helpers.flipBitwise(Sidekick.chatTarget);
                        if (Jackal.jackal == CachedPlayer.LocalPlayer.PlayerControl) Jackal.chatTarget = Helpers.flipBitwise(Jackal.chatTarget);
                        handled = true;
                    }
                }

                if (handled) {
                    __instance.TextArea.Clear();
                    __instance.quickChatMenu.ResetGlyphs();
                }
                return !handled;
            }
        }
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class EnableChat {
            public static void Postfix(HudManager __instance) {
                PlayerControl player = CachedPlayer.LocalPlayer.PlayerControl;
                if (!__instance.Chat.isActiveAndEnabled && (AmongUsClient.Instance.GameMode == GameModes.FreePlay || (CachedPlayer.LocalPlayer.PlayerControl.isLover() && Lovers.enableChat)))
                    __instance.Chat.SetVisible(true);

                if (!__instance.Chat.isActiveAndEnabled && (AmongUsClient.Instance.GameMode == GameModes.FreePlay || (CachedPlayer.LocalPlayer.PlayerControl.isTeamJackal() && Jackal.hasChat)))
                    __instance.Chat.SetVisible(true);
            }
        }

        [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
        public static class SetBubbleName { 
            public static void Postfix(ChatBubble __instance, [HarmonyArgument(0)] string playerName) {
                PlayerControl sourcePlayer = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data != null && x.Data.PlayerName.Equals(playerName));
                if (CachedPlayer.LocalPlayer != null && CachedPlayer.LocalPlayer.Data.Role.IsImpostor && (Spy.spy != null && sourcePlayer.PlayerId == Spy.spy.PlayerId || Sidekick.sidekick != null && Sidekick.wasTeamRed && sourcePlayer.PlayerId == Sidekick.sidekick.PlayerId || Jackal.jackal != null && Jackal.wasTeamRed && sourcePlayer.PlayerId == Jackal.jackal.PlayerId) && __instance != null) __instance.NameText.color = Palette.ImpostorRed;
            }
        }

        [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
        public static class AddChat {
            public static bool Prefix(ChatController __instance, [HarmonyArgument(0)] PlayerControl sourcePlayer) {
                if (__instance != FastDestroyableSingleton<HudManager>.Instance.Chat)
                    return true;
                PlayerControl localPlayer = CachedPlayer.LocalPlayer.PlayerControl;
                bool isTeamJackalWithChat = CachedPlayer.LocalPlayer.PlayerControl.isTeamJackal() && Jackal.hasChat;
                return localPlayer == null || 
                    (MeetingHud.Instance != null || LobbyBehaviour.Instance != null ||
                    localPlayer.Data.IsDead || (int)sourcePlayer.PlayerId == (int)CachedPlayer.LocalPlayer.PlayerId ||
                    (localPlayer.isLover() && Lovers.enableChat && Helpers.getChatPartner(sourcePlayer) == localPlayer) || 
                    (isTeamJackalWithChat && Helpers.getChatPartner(sourcePlayer) == localPlayer));
            }
        }
    }
}
