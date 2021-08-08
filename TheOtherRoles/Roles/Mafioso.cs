using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Mafioso : Role<Mafioso>
    {
        public override Color color => Palette.ImpostorRed;
        protected override RoleId roleId => RoleId.Mafioso;
        public override RoleType roleType => RoleType.Impostor;
    }
}