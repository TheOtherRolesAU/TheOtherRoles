using System;

namespace TheOtherRoles.Roles
{
    [Flags]
    public enum RoleType
    {
        Impostor,
        Crewmate,
        Neutral,
        Secondary
    }
}