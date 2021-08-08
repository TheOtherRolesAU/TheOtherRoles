using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Shifter : Role<Shifter>
    {
        public static bool shiftModifiers;

        public static PlayerControl futureShift;
        public static PlayerControl currentTarget;

        private static Sprite buttonSprite;
        public override Color color => new Color32(102, 102, 102, byte.MaxValue);
        protected override RoleId roleId => RoleId.Shifter;

        public static Sprite GetButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.ShiftButton.png", 115f);
            return buttonSprite;
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            currentTarget = null;
            futureShift = null;
            shiftModifiers = CustomOptionHolder.shifterShiftsModifiers.GetBool();
        }
    }
}