using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epic.OnlineServices.Presence;
using HarmonyLib;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using static UnityEngine.UI.Button;
using UnityEngine.Events;
using Hazel;
using TheOtherRoles.Players;

namespace TheOtherRoles.CustomGameModes {
    [HarmonyPatch]
    class GameModePatches {
        /* 
         Creates a button in the info pane in the lobby to cycle through the game modes of TOR.
         */
        [HarmonyPatch(typeof(LobbyInfoPane), nameof(LobbyInfoPane.Update))]
        class LobbyInfoPanePatch {

            private static GameObject gameModeButton = null;
            public static void Postfix(LobbyInfoPane __instance) {
                if (gameModeButton != null||  !AmongUsClient.Instance.AmHost) { return; }

                var template = GameObject.Find("PRIVATE BUTTON");
                var GameModeText = GameObject.Find("GameModeText");
                if (template == null || GameModeText== null) { return; }
                gameModeButton = GameObject.Instantiate(template, template.transform.parent); //, GameModeText.transform);
                gameModeButton.transform.localPosition = template.transform.localPosition + new Vector3(0f, 0.65f, -2f);
                gameModeButton.name = "TOR GameModeButton";

                var pButton = gameModeButton.GetComponent<PassiveButton>();
                pButton.buttonText.text = GameModeText.GetComponent<TextMeshPro>().text;
                pButton.OnClick.RemoveAllListeners();
                pButton.OnClick = new ButtonClickedEvent();
                __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>(p => { pButton.buttonText.text = Helpers.cs(Color.yellow, GameModeText.GetComponent<TextMeshPro>().text); })));
                gameModeButton.transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(0.1f, 0.1f, 0.1f);
                gameModeButton.transform.GetChild(2).GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f);
                pButton.OnClick.AddListener((Action)(() =>
                {
                    TORMapOptions.gameMode = (CustomGamemodes)((int)(TORMapOptions.gameMode + 1)  % Enum.GetNames(typeof(CustomGamemodes)).Length);
                    __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>(p => { pButton.buttonText.text = Helpers.cs(Color.yellow, GameModeText.GetComponent<TextMeshPro>().text); })));
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareGamemode, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)TORMapOptions.gameMode);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.shareGamemode((byte)TORMapOptions.gameMode);
                }));
                pButton.OnMouseOut = new UnityEvent();
                pButton.OnMouseOver = new UnityEvent();
                pButton.OnMouseOver.AddListener((Action)(() => {
                    gameModeButton.transform.GetChild(1).gameObject.SetActive(true);
                    gameModeButton.transform.GetChild(2).gameObject.SetActive(false);
                }));
                pButton.OnMouseOut.AddListener((Action)(() => {
                    gameModeButton.transform.GetChild(1).gameObject.SetActive(false);
                    gameModeButton.transform.GetChild(2).gameObject.SetActive(true);
                }));

            }
        }
    }
}
