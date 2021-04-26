using System;
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
                    if (text.ToLower().Equals("i'm a panda")) {
                        handled = true;
                        byte colorId = (byte)CustomColors.pickableColors;
                        SaveManager.BodyColor = (byte)colorId;
                        if (PlayerControl.LocalPlayer)
                            PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                    } else if (text.ToLower().Equals("nightsky")) {
                        handled = true;
                        byte colorId = (byte)(CustomColors.pickableColors + 1);
                        SaveManager.BodyColor = (byte)colorId;
                        if (PlayerControl.LocalPlayer)
                            PlayerControl.LocalPlayer.CmdCheckColor(colorId);
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
