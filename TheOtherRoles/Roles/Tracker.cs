using TheOtherRoles.Objects;
using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Tracker : Role<Tracker>
    {
        public static float updateInterval = 5f;
        public static bool resetTargetAfterMeeting;

        public static Arrow arrow = new(Color.blue);
        public static PlayerControl currentTarget;
        public static PlayerControl tracked;
        public static bool usedTracker;

        public static float timeUntilUpdate;

        private static Sprite buttonSprite;
        public override Color color => new Color32(100, 58, 220, byte.MaxValue);
        protected override RoleId roleId => RoleId.Tracker;

        public static Sprite GetButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.TrackerButton.png", 115f);
            return buttonSprite;
        }

        public static void ResetTracked()
        {
            currentTarget = tracked = null;
            usedTracker = false;
            if (arrow?.arrow != null) Object.Destroy(arrow.arrow);
            arrow = new Arrow(Color.blue);
            if (arrow.arrow != null) arrow.arrow.SetActive(false);
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            ResetTracked();
            timeUntilUpdate = 0f;
            updateInterval = CustomOptionHolder.trackerUpdateInterval.GetFloat();
            resetTargetAfterMeeting = CustomOptionHolder.trackerResetTargetAfterMeeting.GetBool();
        }
    }
}