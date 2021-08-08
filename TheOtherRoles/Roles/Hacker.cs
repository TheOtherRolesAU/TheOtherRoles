using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Hacker : Role<Hacker>
    {
        public static float cooldown = 30f;
        public static float duration = 10f;
        public static bool onlyColorType;

        public static float hackerTimer;

        private static Sprite buttonSprite;
        public override Color color => new Color32(117, 250, 76, byte.MaxValue);
        protected override RoleId roleId => RoleId.Hacker;

        public static Sprite GetButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.HackerButton.png", 115f);
            return buttonSprite;
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            hackerTimer = 0f;
            cooldown = CustomOptionHolder.hackerCooldown.GetFloat();
            duration = CustomOptionHolder.hackerHackeringDuration.GetFloat();
            onlyColorType = CustomOptionHolder.hackerOnlyColorType.GetBool();
        }
    }
}