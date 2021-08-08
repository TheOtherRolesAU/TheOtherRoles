using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Medic : Role<Medic>
    {
        public static int showShielded;
        public static bool showAttemptToShielded;
        public static bool setShieldAfterMeeting;

        public static bool usedShield;

        public static PlayerControl shielded;
        public static PlayerControl futureShielded;
        public static PlayerControl currentTarget;
        public static Color shieldedColor = new Color32(0, 221, 255, byte.MaxValue);

        private static Sprite buttonSprite;
        public override Color color => new Color32(126, 251, 194, byte.MaxValue);
        protected override RoleId roleId => RoleId.Medic;

        public static Sprite GetButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.ShieldButton.png", 115f);
            return buttonSprite;
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            shielded = null;
            futureShielded = null;
            currentTarget = null;
            usedShield = false;
            showShielded = CustomOptionHolder.medicShowShielded.GetSelection();
            showAttemptToShielded = CustomOptionHolder.medicShowAttemptToShielded.GetBool();
            setShieldAfterMeeting = CustomOptionHolder.medicSetShieldAfterMeeting.GetBool();
        }
    }
}