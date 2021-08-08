using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Mayor : Role<Mayor>
    {
        public override Color color => new Color32(32, 77, 66, byte.MaxValue);
        protected override RoleId roleId => RoleId.Mayor;
    }
}