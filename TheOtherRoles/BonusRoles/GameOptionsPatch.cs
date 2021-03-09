using HarmonyLib;
using System;
using System.IO;
using System.Net.Http;
using UnityEngine;
using static BonusRoles.BonusRoles;
using Reactor.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace BonusRoles
{
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.FixedUpdate))]
    class LobbyBehaviourUpdatePatch
    {
        private static float timer = 5f;
        private static float duration = 5f;
        private static bool firstPart = true;

        public static bool Prefix(LobbyBehaviour __instance)
        {
            DestroyableSingleton<HudManager>.Instance.GameSettings.scale = 0.5f;

            timer -= Time.deltaTime;
            if (timer <= 0) {
                timer = duration;
                firstPart = !firstPart;
            }
            if (PlayerControl.GameOptions != null)
		    {
                string text = PlayerControl.GameOptions.Method_24(GameData.Instance ? GameData.Instance.PlayerCount : 10);
                int n = 30;
                int index = text.TakeWhile(c => (n -= (c == '\n' ? 1 : 0)) > 0).Count();
                
                if (firstPart) {
                    DestroyableSingleton<HudManager>.Instance.GameSettings.Text = text.Substring(0, index);
                } else {
                    DestroyableSingleton<HudManager>.Instance.GameSettings.Text = text.Substring(index + 1);
                }
                DestroyableSingleton<HudManager>.Instance.GameSettings.gameObject.SetActive(true);
            }
            return false;
        }
    }

    [HarmonyPriority(Priority.Low)] 
    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    class GameOptionsMenuUpdatePatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            __instance.GetComponentInParent<Scroller>().YBounds.max = -0.5F + __instance.Children.Length * 0.5F;
        }
    }
}