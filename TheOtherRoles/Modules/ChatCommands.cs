using System;
using System.Security.Cryptography;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using UnhollowerBaseLib;

namespace TheOtherRoles.Modules {
    [HarmonyPatch]
    public static class ChatCommands {
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        private static class SendChatPatch {
            static bool Prefix(ChatController __instance) {
                string text = __instance.TextArea.text;
                bool handled = false;
                if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) {
                    if (text.ToLower().StartsWith("/kick ")) {
                        string playerName = text.Substring(6);
                        PlayerControl target = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(playerName));
                        if (target != null && AmongUsClient.Instance != null && AmongUsClient.Instance.CanBan()) {
                            var client = AmongUsClient.Instance.GetClient(target.OwnerId);
                            if (client != null) {
                                AmongUsClient.Instance.KickPlayer(client.Id, false);
                                handled = true;
                            }
                        }
                    } else if (text.ToLower().StartsWith("/ban ")) {
                        string playerName = text.Substring(6);
                        PlayerControl target = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(playerName));
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
                        PlayerControl.LocalPlayer.Exiled();
                        HudManager.Instance.KillOverlay.ShowKillAnimation(PlayerControl.LocalPlayer.Data, PlayerControl.LocalPlayer.Data);
                        handled = true;
                    } else if (text.ToLower().StartsWith("/color ")) {
                        handled = true;
                        int col;
                        if (!Int32.TryParse(text.Substring(7), out col)) {
                            __instance.AddChat(PlayerControl.LocalPlayer, "Unable to parse color id\nUsage: /color {id}");
                        }
                        col = Math.Clamp(col, 0, Palette.PlayerColors.Length - 1);
                        PlayerControl.LocalPlayer.SetColor(col);
                        __instance.AddChat(PlayerControl.LocalPlayer, "Changed color succesfully");;
                    } 
                }

                if (text.ToLower().StartsWith("/tp ") && PlayerControl.LocalPlayer.Data.IsDead) {
                    string playerName = text.Substring(4).ToLower();
                    PlayerControl target = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.ToLower().Equals(playerName));
                    if (target != null) {
                        PlayerControl.LocalPlayer.transform.position = target.transform.position;
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
                if (!__instance.Chat.isActiveAndEnabled && AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                    __instance.Chat.SetVisible(true);
            }
        }
    }
}
