using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Cleaner : Role<Cleaner>
    {
        public static float cooldown = 30f;

        private static Sprite buttonSprite;
        public override Color color => Palette.ImpostorRed;
        protected override RoleId roleId => RoleId.Cleaner;
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
            cooldown = CustomOptionHolder.cleanerCooldown.GetFloat();
        }
    }
}