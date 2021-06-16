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

namespace TheOtherRoles {
    [HarmonyPatch]
    public static class ChatCommands {
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        private static class SendChatPatch {
            static bool Prefix(ChatController __instance) {
                string text = __instance.TextArea.text;
                bool handled = false;
                if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) {
                    //using(MD5 md5 = MD5.Create()) {
                        // string hash = System.BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes("tor@" + text.ToLower() + "§eof"))).Replace("-", "").ToLowerInvariant();
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
                //}
                if (handled) {
                    __instance.TextArea.Clear();
                    __instance.quickChatMenu.ResetGlyphs();
                }
                return !handled;
            }
        }
    }
}
