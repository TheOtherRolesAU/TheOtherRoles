using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Detective : Role<Detective>
    {
        public static float footprintInterval = 1f;
        public static float footprintDuration = 1f;
        public static bool anonymousFootprints;
        public static float reportNameDuration;
        public static float reportColorDuration = 20f;

        public static float timer = 6.2f;
        public override Color color => new Color32(45, 106, 165, byte.MaxValue);
        protected override RoleId roleId => RoleId.Detective;

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            anonymousFootprints = CustomOptionHolder.detectiveAnonymousFootprints.GetBool();
            footprintInterval = CustomOptionHolder.detectiveFootprintInterval.GetFloat();
            footprintDuration = CustomOptionHolder.detectiveFootprintDuration.GetFloat();
            reportNameDuration = CustomOptionHolder.detectiveReportNameDuration.GetFloat();
            reportColorDuration = CustomOptionHolder.detectiveReportColorDuration.GetFloat();
            timer = 6.2f;
        }
    }
}