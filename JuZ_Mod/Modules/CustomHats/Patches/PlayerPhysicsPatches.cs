using TheOtherRoles.Modules.CustomHats.Extensions;
using HarmonyLib;

namespace TheOtherRoles.Modules.CustomHats.Patches;

[HarmonyPatch(typeof(PlayerPhysics))]
internal static class PlayerPhysicsPatches
{
    [HarmonyPatch(nameof(PlayerPhysics.HandleAnimation))]
    [HarmonyPostfix]
    private static void HandleAnimationPostfix(PlayerPhysics __instance)
    {
        var currentAnimation = __instance.Animations.Animator.GetCurrentAnimation();
        if (currentAnimation == __instance.Animations.group.ClimbUpAnim) return;
        if (currentAnimation == __instance.Animations.group.ClimbDownAnim) return;
        var hatParent = __instance.myPlayer.cosmetics.hat;
        if (hatParent == null || hatParent == null) return;
        if (!hatParent.TryGetCached(out var viewData)) return;
        var extend = hatParent.Hat.GetHatExtension();
        if (extend == null) return;
        if (extend.FlipImage != null)
        {
            if (__instance.FlipX)
            {
                hatParent.FrontLayer.sprite = extend.FlipImage;
            }
            else
            {
                hatParent.FrontLayer.sprite = viewData.MainImage;
            }
        }

        if (extend.BackFlipImage != null)
        {
            if (__instance.FlipX)
            {
                hatParent.BackLayer.sprite = extend.BackFlipImage;
            }
            else
            {
                hatParent.BackLayer.sprite = viewData.BackImage;
            }
        }
    }
}