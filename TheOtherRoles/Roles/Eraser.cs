using System.Collections.Generic;
using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Eraser : Role<Eraser>
    {
        public static float cooldown = 30f;
        public static bool canEraseAnyone;

        public static PlayerControl currentTarget;
        public static List<PlayerControl> futureErased = new();

        private static Sprite buttonSprite;
        public override Color color => Palette.ImpostorRed;
        protected override RoleId roleId => RoleId.Eraser;
        public override RoleType roleType => RoleType.Impostor;

        public static Sprite GetButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.EraserButton.png", 115f);
            return buttonSprite;
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            futureErased.Clear();
            currentTarget = null;
            cooldown = CustomOptionHolder.eraserCooldown.GetFloat();
            canEraseAnyone = CustomOptionHolder.eraserCanEraseAnyone.GetBool();
        }
    }
}