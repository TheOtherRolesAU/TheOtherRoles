using System;
using System.Security.Cryptography;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using UnityEngine;
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
                        string hash = System.BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(text.ToLower()))).Replace("-", "").ToLowerInvariant();
                        if (hash.Equals("f92af861c8b7aa5f2d05165abc9ba04f")) { // i am a cheater
                            handled = true;
                            byte colorId = (byte)CustomColors.pickableColors;
                            SaveManager.BodyColor = (byte)colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                        } else if (hash.Equals("5a2ee6de2e42d2b326556640f838b0d7")) { // i dont understand hashes
                            handled = true;
                            byte colorId = (byte)(CustomColors.pickableColors + 1);
                            SaveManager.BodyColor = (byte)colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                        }
                        // System.Console.WriteLine(hash);
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
