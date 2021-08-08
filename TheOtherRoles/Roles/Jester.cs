using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Jester : Role<Jester>
    {
        public static bool canCallEmergency = true;
        public static bool canSabotage = true;

        public static bool triggerJesterWin;
        public override Color color => new Color32(236, 98, 165, byte.MaxValue);
        protected override RoleId roleId => RoleId.Jester;
        public override RoleType roleType => RoleType.Neutral;

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            triggerJesterWin = false;
            canCallEmergency = CustomOptionHolder.jesterCanCallEmergency.GetBool();
            canSabotage = CustomOptionHolder.jesterCanSabotage.GetBool();
        }
    }
}