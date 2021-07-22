using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class PlayerPhysicsFixedUpdatePatch
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            updateUndertakerMoveSpeed(__instance);
        }

        static void updateUndertakerMoveSpeed(PlayerPhysics playerPhysics)
        {
            if (Undertaker.undertaker == null || Undertaker.undertaker != PlayerControl.LocalPlayer) return;
            if(Undertaker.deadBodyDraged != null )
            {
                if (playerPhysics.AmOwner && GameData.Instance && playerPhysics.myPlayer.CanMove)
                    playerPhysics.body.velocity /= 2;
            }
        }
    }
}
