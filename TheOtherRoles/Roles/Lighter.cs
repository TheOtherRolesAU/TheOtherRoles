using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Lighter : Role<Lighter>
    {
        public static float lighterModeLightsOnVision = 2f;
        public static float lighterModeLightsOffVision = 0.75f;
        public static float cooldown = 30f;
        public static float duration = 5f;

        public static float lighterTimer;

        private static Sprite buttonSprite;
        public override Color color => new Color32(238, 229, 190, byte.MaxValue);
        protected override RoleId roleId => RoleId.Lighter;

        public static Sprite GetButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.LighterButton.png", 115f);
            return buttonSprite;
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            lighterTimer = 0f;
            cooldown = CustomOptionHolder.lighterCooldown.GetFloat();
            duration = CustomOptionHolder.lighterDuration.GetFloat();
            lighterModeLightsOnVision = CustomOptionHolder.lighterModeLightsOnVision.GetFloat();
            lighterModeLightsOffVision = CustomOptionHolder.lighterModeLightsOffVision.GetFloat();
        }
    }
}