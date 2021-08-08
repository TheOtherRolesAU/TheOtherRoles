using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Janitor : Role<Janitor>
    {
        public static float cooldown = 30f;

        private static Sprite buttonSprite;
        public override Color color => Palette.ImpostorRed;
        protected override RoleId roleId => RoleId.Janitor;
        public override RoleType roleType => RoleType.Impostor;

        public static Sprite GetButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.CleanButton.png", 115f);
            return buttonSprite;
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            cooldown = CustomOptionHolder.janitorCooldown.GetFloat();
        }
    }
}