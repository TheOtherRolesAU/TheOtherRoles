using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Bait : Role<Bait>
    {
        public static bool highlightAllVents;
        public static float reportDelay;

        public static bool reported;
        public override Color color => new Color32(0, 247, 255, byte.MaxValue);
        protected override RoleId roleId => RoleId.Bait;

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            reported = false;
            highlightAllVents = CustomOptionHolder.baitHighlightAllVents.GetBool();
            reportDelay = CustomOptionHolder.baitReportDelay.GetFloat();
        }
    }
}