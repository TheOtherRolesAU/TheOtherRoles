
using AmongUs.Data;
using HarmonyLib;
using InnerNet;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TheOtherRoles.Patches {
    [HarmonyPatch]

    public sealed class LobbyJoinBind {
        static int GameId;

        static GameObject LobbyText;

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
        [HarmonyPostfix]

        public static void Postfix(InnerNetClient __instance) {
            GameId = __instance.GameId;
        }

        [HarmonyPatch(typeof(MMOnlineManager), nameof(MMOnlineManager.Start))]
        [HarmonyPostfix]

        public static void Postfix() {
            if (!LobbyText) {
                LobbyText = new("lobbycode");
                var comp = LobbyText.AddComponent<TMPro.TextMeshPro>();
                comp.fontSize = 2.5f;
                LobbyText.transform.localPosition = new(10.3f, -3.9f, 0);
                LobbyText.SetActive(true);
            }
        }

        [HarmonyPatch(typeof(MMOnlineManager), nameof(MMOnlineManager.Update))]
        [HarmonyPostfix]

        public static void Postfix(MMOnlineManager __instance) {

            string code2 = GUIUtility.systemCopyBuffer;

            if (code2.Length != 6 || !Regex.IsMatch(code2, @"^[a-zA-Z]+$"))
                code2 = "";
            string code2Disp = DataManager.Settings.Gameplay.StreamerMode ? "****" : code2.ToUpper();
            if (GameId != 0 && Input.GetKeyDown(KeyCode.LeftShift)) {
                __instance.StartCoroutine(AmongUsClient.Instance.CoJoinOnlineGameFromCode(GameId));
            } else if (Input.GetKeyDown(KeyCode.RightShift) && code2 != "") {
                __instance.StartCoroutine(AmongUsClient.Instance.CoJoinOnlineGameFromCode(GameCode.GameNameToInt(code2)));
            }

            if (LobbyText) {
                LobbyText.GetComponent<TMPro.TextMeshPro>().text = "";
                if (GameId != 0 && GameId != 32) {
                    string code = GameCode.IntToGameName(GameId);
                    if (code != "") {
                        code = DataManager.Settings.Gameplay.StreamerMode ? "****" : code;
                        LobbyText.GetComponent<TMPro.TextMeshPro>().text = $"Prev Lobby: {code}   [LShift]";

                    }
                }
                if (code2 != "") {
                    LobbyText.GetComponent<TMPro.TextMeshPro>().text += $"\nClipboard: {code2Disp}  [RShift]";
                }
            }
        }
    }
}