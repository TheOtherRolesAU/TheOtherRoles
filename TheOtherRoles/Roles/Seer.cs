using System.Collections.Generic;
using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Seer : Role<Seer>
    {
        public static float soulDuration = 15f;
        public static bool limitSoulDuration;
        public static int mode;

        public static List<Vector3> deadBodyPositions = new();

        private static Sprite soulSprite;
        public override Color color => new Color32(97, 178, 108, byte.MaxValue);
        protected override RoleId roleId => RoleId.Seer;

        public static Sprite GetSoulSprite()
        {
            if (soulSprite) return soulSprite;
            soulSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.Soul.png", 500f);
            return soulSprite;
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            deadBodyPositions.Clear();
            limitSoulDuration = CustomOptionHolder.seerLimitSoulDuration.GetBool();
            soulDuration = CustomOptionHolder.seerSoulDuration.GetFloat();
            mode = CustomOptionHolder.seerMode.GetSelection();
        }
    }
}