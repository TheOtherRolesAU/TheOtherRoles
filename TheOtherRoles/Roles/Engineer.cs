using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Engineer : Role<Engineer>
    {
        public static bool usedRepair;

        private static Sprite buttonSprite;
        public override Color color => new Color32(0, 40, 245, byte.MaxValue);
        protected override RoleId roleId => RoleId.Engineer;

        public static Sprite GetButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.RepairButton.png", 115f);
            return buttonSprite;
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            usedRepair = false;
        }
    }
}