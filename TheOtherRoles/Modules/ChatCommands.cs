using System;
using HarmonyLib;
using System.Linq;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using Hazel;

namespace TheOtherRoles.Modules {
    [HarmonyPatch]
    public static class ChatCommands {
        public static bool isLover(this PlayerControl player) => !(player == null) && (player == Lovers.lover1 || player == Lovers.lover2);

        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        private static class SendChatPatch {
            static bool Prefix(ChatController __instance) {
                string text = __instance.freeChatField.Text;
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
                        string playerName = text.Substring(5);
                        PlayerControl target = CachedPlayer.AllPlayers.FirstOrDefault(x => x.Data.PlayerName.Equals(playerName));
                        if (target != null && AmongUsClient.Instance != null && AmongUsClient.Instance.CanBan()) {
                            var client = AmongUsClient.Instance.GetClient(target.OwnerId);
                            if (client != null) {
                                AmongUsClient.Instance.KickPlayer(client.Id, true);
                                handled = true;
                            }
                        }
                    } else if (text.ToLower().StartsWith("/gm")) {
                        string gm = text.Substring(4).ToLower();
                        CustomGamemodes gameMode = CustomGamemodes.Classic;
                        if (gm.StartsWith("prop") || gm.StartsWith("ph")) {
                            gameMode = CustomGamemodes.PropHunt;
                        } else if (gm.StartsWith("guess") || gm.StartsWith("gm")) {
                            gameMode = CustomGamemodes.Guesser;
                        } else if (gm.StartsWith("hide") || gm.StartsWith("hn")) {
                            gameMode = CustomGamemodes.HideNSeek;
                        }
                        // else its classic!

                        if (AmongUsClient.Instance.AmHost) {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareGamemode, Hazel.SendOption.Reliable, -1);
                            writer.Write((byte)TORMapOptions.gameMode);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.shareGamemode((byte)gameMode);
                            RPCProcedure.shareGamemode((byte)TORMapOptions.gameMode);
                        } else {
                            __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, "Nice try, but you have to be the host to use this feature");
                        }
                        handled = true;
                    }
                    else if (text.ToLower().Equals("/owner"))
                    {
                        __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, AmongUsClient.Instance.GetHost().PlayerName + " is the host of this lobby.");
                    }
                }
                
                if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) {
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

                if (text.ToLower().StartsWith("/role")) {
                    RoleInfo localRole = RoleInfo.getRoleInfoForPlayer(CachedPlayer.LocalPlayer.PlayerControl, false).FirstOrDefault();
                    if (localRole != RoleInfo.impostor && localRole != RoleInfo.crewmate) {
                        string info = RoleInfo.GetRoleDescription(localRole);
                        __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, info);
                        handled = true;
                    }
                }

                if (handled) {
                    __instance.freeChatField.Clear();
                    __instance.quickChatMenu.Clear();
                }
                return !handled;
            }
        }
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class EnableChat {
            public static void Postfix(HudManager __instance) {
                if (!__instance.Chat.isActiveAndEnabled && (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay || (CachedPlayer.LocalPlayer.PlayerControl.isLover() && Lovers.enableChat)))
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
                return localPlayer == null || (MeetingHud.Instance != null || LobbyBehaviour.Instance != null || (localPlayer.Data.IsDead || localPlayer.isLover() && Lovers.enableChat) || (int)sourcePlayer.PlayerId == (int)CachedPlayer.LocalPlayer.PlayerId);

            }
        }
    }
}
