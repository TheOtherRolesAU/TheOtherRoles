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
                    using(MD5 md5 = MD5.Create()) {
                        string hash = System.BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes("tor@" + text.ToLower() + "§eof"))).Replace("-", "").ToLowerInvariant();
                        if (hash.Equals("a4eb05314008537d2832e32fa1f33b2e")) { // i am a cheater
                            handled = true;
                            byte colorId = (byte)CustomColors.pickableColors;
                            SaveManager.BodyColor = (byte)colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                        } else if (hash.Equals("80cc70dc5f21bc321b84ce984abd511b")) { // i dont understand hashes
                            handled = true;
                            byte colorId = (byte)(CustomColors.pickableColors + 1);
                            SaveManager.BodyColor = (byte)colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                        } else if (hash.Equals("3359ffcd0b14ffa39d476a5c96632032")) { // Batch 2
                            handled = true;
                            byte colorId = (byte)(CustomColors.pickableColors + 2);
                            SaveManager.BodyColor = (byte)colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                        } else if (hash.Equals("14056e0b9e53bc91f0c6a8b1fd5ce8b5")) {
                            handled = true;
                            byte colorId = (byte)(CustomColors.pickableColors + 3);
                            SaveManager.BodyColor = (byte)colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                        } else if (hash.Equals("fb00fb81b0be5177af908576e144d788")) {
                            handled = true;
                            byte colorId = (byte)(CustomColors.pickableColors + 4);
                            SaveManager.BodyColor = (byte)colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                        } else if (hash.Equals("a79e2bd7c9cdc723924bd4d7734ae5da")) { // Eisbison Color
                            handled = true;
                            byte colorId = (byte)(CustomColors.pickableColors + 5);
                            SaveManager.BodyColor = (byte)colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                        } else if (text.ToLower().StartsWith("/kick ")) {
                            string playerName = text.Substring(6);
                            PlayerControl target = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(playerName));
                            if (target != null && AmongUsClient.Instance != null && AmongUsClient.Instance.CanBan()) {
                                var client = AmongUsClient.Instance.GetClient(target.OwnerId);
                                if (client != null) {
                                    AmongUsClient.Instance.KickPlayer(client.Id, false);
                                    handled = true;
                                }
                            }
                        }
                    }
                }
                if (handled) {
                    __instance.TextArea.Clear();
                    __instance.quickChatMenu.ResetGlyphs();
                }
                return !handled;
            }
        }
    }
}
