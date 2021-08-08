using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class SecurityGuard : Role<SecurityGuard>
    {
        public static float cooldown = 30f;
        public static int totalScrews = 7;
        public static int ventPrice = 1;
        public static int camPrice = 2;

        public static int remainingScrews = 7;
        public static int placedCameras;
        public static Vent ventTarget;

        private static Sprite closeVentButtonSprite;
        private static Sprite placeCameraButtonSprite;
        private static Sprite animatedVentSealedSprite;
        private static Sprite staticVentSealedSprite;
        public override Color color => new Color32(195, 178, 95, byte.MaxValue);
        protected override RoleId roleId => RoleId.SecurityGuard;

        public static Sprite GetCloseVentButtonSprite()
        {
            if (closeVentButtonSprite) return closeVentButtonSprite;
            closeVentButtonSprite =
                Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.CloseVentButton.png", 115f);
            return closeVentButtonSprite;
        }

        public static Sprite GetPlaceCameraButtonSprite()
        {
            if (placeCameraButtonSprite) return placeCameraButtonSprite;
            placeCameraButtonSprite =
                Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.PlaceCameraButton.png", 115f);
            return placeCameraButtonSprite;
        }

        public static Sprite GetAnimatedVentSealedSprite()
        {
            if (animatedVentSealedSprite) return animatedVentSealedSprite;
            animatedVentSealedSprite =
                Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.AnimatedVentSealed.png", 160f);
            return animatedVentSealedSprite;
        }

        public static Sprite GetStaticVentSealedSprite()
        {
            if (staticVentSealedSprite) return staticVentSealedSprite;
            staticVentSealedSprite =
                Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.StaticVentSealed.png", 160f);
            return staticVentSealedSprite;
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            ventTarget = null;
            placedCameras = 0;
            cooldown = CustomOptionHolder.securityGuardCooldown.GetFloat();
            totalScrews = remainingScrews = Mathf.RoundToInt(CustomOptionHolder.securityGuardTotalScrews.GetFloat());
            camPrice = Mathf.RoundToInt(CustomOptionHolder.securityGuardCamPrice.GetFloat());
            ventPrice = Mathf.RoundToInt(CustomOptionHolder.securityGuardVentPrice.GetFloat());
        }
    }
}