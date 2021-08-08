using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class TimeMaster : Role<TimeMaster>
    {
        public static bool reviveDuringRewind = false;
        public static float rewindTime = 3f;
        public static float shieldDuration = 3f;
        public static float cooldown = 30f;

        public static bool shieldActive;
        public static bool isRewinding;

        private static Sprite buttonSprite;
        public override Color color => new Color32(112, 142, 239, byte.MaxValue);
        protected override RoleId roleId => RoleId.TimeMaster;

        public static Sprite GetButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.TimeShieldButton.png", 115f);
            return buttonSprite;
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            isRewinding = false;
            shieldActive = false;
            rewindTime = CustomOptionHolder.timeMasterRewindTime.GetFloat();
            shieldDuration = CustomOptionHolder.timeMasterShieldDuration.GetFloat();
            cooldown = CustomOptionHolder.timeMasterCooldown.GetFloat();
        }
    }
}