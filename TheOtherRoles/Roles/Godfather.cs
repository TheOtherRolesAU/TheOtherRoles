using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Godfather : Role<Godfather>
    {
        public override Color color => Palette.ImpostorRed;
        protected override RoleId roleId => RoleId.Godfather;
        public override RoleType roleType => RoleType.Impostor;
    }
}