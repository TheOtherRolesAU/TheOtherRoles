using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Swapper : Role<Swapper>
    {
        public static bool canCallEmergency;
        public static bool canOnlySwapOthers;

        public static byte playerId1 = byte.MaxValue;
        public static byte playerId2 = byte.MaxValue;

        private static Sprite spriteCheck;
        public override Color color => new Color32(134, 55, 86, byte.MaxValue);
        protected override RoleId roleId => RoleId.Swapper;

        public static Sprite GetCheckSprite()
        {
            if (spriteCheck) return spriteCheck;
            spriteCheck = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.SwapperCheck.png", 150f);
            return spriteCheck;
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            playerId1 = byte.MaxValue;
            playerId2 = byte.MaxValue;
            canCallEmergency = CustomOptionHolder.swapperCanCallEmergency.GetBool();
            canOnlySwapOthers = CustomOptionHolder.swapperCanOnlySwapOthers.GetBool();
        }
    }
}