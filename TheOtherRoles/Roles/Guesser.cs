using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Guesser : Role<Guesser>
    {
        public static int remainingShots = 2;

        private static Sprite targetSprite;

        // TODO: split into evil and nice Guesser
        public override Color color => new Color32(255, 255, 0, byte.MaxValue);
        protected override RoleId roleId => RoleId.Guesser;
        public override RoleType roleType => RoleType.Crewmate | RoleType.Impostor;

        public static Sprite GetTargetSprite()
        {
            if (targetSprite) return targetSprite;
            targetSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.TargetIcon.png", 150f);
            return targetSprite;
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();

            remainingShots = Mathf.RoundToInt(CustomOptionHolder.guesserNumberOfShots.GetFloat());
        }
    }
}