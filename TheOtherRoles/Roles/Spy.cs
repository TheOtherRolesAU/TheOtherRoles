using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Spy : Role<Spy>
    {
        public static bool impostorsCanKillAnyone = true;
        public static bool canEnterVents;
        public static bool hasImpostorVision;
        public override Color color => Palette.ImpostorRed;
        protected override RoleId roleId => RoleId.Spy;

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            impostorsCanKillAnyone = CustomOptionHolder.spyImpostorsCanKillAnyone.GetBool();
            canEnterVents = CustomOptionHolder.spyCanEnterVents.GetBool();
            hasImpostorVision = CustomOptionHolder.spyHasImpostorVision.GetBool();
        }
    }
}