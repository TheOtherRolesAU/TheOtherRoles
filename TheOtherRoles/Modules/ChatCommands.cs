using System;
using System.Linq;
using HarmonyLib;
using InnerNet;

namespace TheOtherRoles.Modules
{
    [HarmonyPatch]
    public static class ChatCommands
    {
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        private static class SendChatPatch
        {
            private static bool Prefix(ChatController __instance)
            {
                var text = __instance.TextArea.text;
                var handled = false;
                if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
                {
                    if (text.ToLower().StartsWith("/kick "))
                    {
                        var playerName = text.Substring(6);
                        var target = PlayerControl.AllPlayerControls.ToArray().ToList()
                            .FirstOrDefault(x => x.Data.PlayerName.Equals(playerName));
                        if (target != null && AmongUsClient.Instance != null && AmongUsClient.Instance.CanBan())
                        {
                            var client = AmongUsClient.Instance.GetClient(target.OwnerId);
                            if (client != null)
                            {
                                AmongUsClient.Instance.KickPlayer(client.Id, false);
                                handled = true;
                            }
                        }
                    }
                    else if (text.ToLower().StartsWith("/ban "))
                    {
                        var playerName = text.Substring(5);
                        var target = PlayerControl.AllPlayerControls.ToArray().ToList()
                            .FirstOrDefault(x => x.Data.PlayerName.Equals(playerName));
                        if (target != null && AmongUsClient.Instance != null && AmongUsClient.Instance.CanBan())
                        {
                            var client = AmongUsClient.Instance.GetClient(target.OwnerId);
                            if (client != null)
                            {
                                AmongUsClient.Instance.KickPlayer(client.Id, true);
                                handled = true;
                            }
                        }
                    }
                }

                if (AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                {
                    if (text.ToLower().Equals("/murder"))
                    {
                        PlayerControl.LocalPlayer.Exiled();
                        HudManager.Instance.KillOverlay.ShowKillAnimation(PlayerControl.LocalPlayer.Data,
                            PlayerControl.LocalPlayer.Data);
                        handled = true;
                    }
                    else if (text.ToLower().StartsWith("/color "))
                    {
                        handled = true;
                        if (!int.TryParse(text.Substring(7), out var col))
                            __instance.AddChat(PlayerControl.LocalPlayer,
                                "Unable to parse color id\nUsage: /color {id}");
                        col = Math.Clamp(col, 0, Palette.PlayerColors.Length - 1);
                        PlayerControl.LocalPlayer.SetColor(col);
                        __instance.AddChat(PlayerControl.LocalPlayer, "Changed color successfully");
                    }
                }

                if (text.ToLower().StartsWith("/tp ") && PlayerControl.LocalPlayer.Data.IsDead)
                {
                    var playerName = text.Substring(4).ToLower();
                    var target = PlayerControl.AllPlayerControls.ToArray().ToList()
                        .FirstOrDefault(x => x.Data.PlayerName.ToLower().Equals(playerName));
                    if (target != null)
                    {
                        PlayerControl.LocalPlayer.transform.position = target.transform.position;
                        handled = true;
                    }
                }

                if (!handled) return true;
                __instance.TextArea.Clear();
                __instance.quickChatMenu.ResetGlyphs();

                return false;
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class EnableChat
        {
            public static void Postfix(HudManager __instance)
            {
                if (!__instance.Chat.isActiveAndEnabled && AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                    __instance.Chat.SetVisible(true);
            }
        }
    }
}