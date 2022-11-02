using HarmonyLib;
using UnityEngine;

namespace TheOtherRoles.Patches;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.Awake))]
public static class PlayerPhysiscs_Awake_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerPhysics __instance)
    {
        if (!__instance.body) return;
        __instance.body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
public static class PlayerPhysicsFixedUpdatePatch
{
    public static void Postfix(PlayerPhysics __instance)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
        updateUndertakerMoveSpeed(__instance);
        removeGhostLordCollision(__instance);
    }

    static void updateUndertakerMoveSpeed(PlayerPhysics playerPhysics)
    {
        if (Undertaker.undertaker == null || Undertaker.undertaker != PlayerControl.LocalPlayer) return;
        if (Undertaker.deadBodyDraged != null)
        {
            if (playerPhysics.AmOwner && GameData.Instance && playerPhysics.myPlayer.CanMove)
                playerPhysics.body.velocity /= 2;
        }
    }

    static void removeGhostLordCollision(PlayerPhysics playerPhysics)
    {
        if (GhostLord.ghostLord == null || GhostLord.ghostLord != PlayerControl.LocalPlayer) return;
        if (GhostLord.isTurnIntoGhost())
        {
            playerPhysics.myPlayer.Collider.enabled = false;
        }
        else
        {
            playerPhysics.myPlayer.Collider.enabled = true;
        }
    }
}