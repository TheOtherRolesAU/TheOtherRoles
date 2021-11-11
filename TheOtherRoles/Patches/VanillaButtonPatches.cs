using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheOtherRoles.Patches {
    [HarmonyPatch]
    class VentButtonPatch {
        [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.Initialize))]
        class RoleBehaviourInitializePatch {
            public static void Postfix(RoleBehaviour __instance, PlayerControl player) {
                if (player.roleCanUseVents())
                    DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.Show();
                else
                    DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.Hide();
            }
        }
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
        class HudManagerSetHudActivePatch {
            public static void Postfix(HudManager __instance, bool isActive) {
                __instance.ImpostorVentButton.gameObject.SetActive(isActive && PlayerControl.LocalPlayer.roleCanUseVents());
            }
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Revive))]
        class PlayerControlRevivePatch {
            public static void Postfix(PlayerControl __instance) {
                DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.ToggleVisible(__instance.roleCanUseVents());  
            }
        }
    }
}
