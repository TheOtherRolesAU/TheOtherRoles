using System;
using UnityEngine;

namespace TheOtherRoles.Roles
{
    // TODO: split into evil and nice Mini
    public class Mini : Role<Mini>
    {
        public const float DefaultColliderRadius = 0.2233912f;
        public const float DefaultColliderOffset = 0.3636057f;

        private static float growingUpDuration = 400f;
        private static DateTime timeOfGrowthStart = DateTime.UtcNow;

        public static bool triggerMiniLose;

        public override Color color => Color.white;
        protected override RoleId roleId => RoleId.Mini;
        public override RoleType roleType => RoleType.Crewmate | RoleType.Impostor;

        public static float GrowingProgress()
        {
            var timeSinceStart = (float) (DateTime.UtcNow - timeOfGrowthStart).TotalMilliseconds;
            return Mathf.Clamp(timeSinceStart / (growingUpDuration * 1000), 0f, 1f);
        }

        public static bool IsGrownUp()
        {
            return Math.Abs(GrowingProgress() - 1f) < 0.1f;
        }

        public override void ClearAndReload()
        {
            triggerMiniLose = false;
            growingUpDuration = CustomOptionHolder.miniGrowingUpDuration.GetFloat();
            timeOfGrowthStart = DateTime.UtcNow;
        }
    }
}