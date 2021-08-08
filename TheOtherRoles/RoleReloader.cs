using System;
using System.Collections.Generic;
using TheOtherRoles.Roles;

namespace TheOtherRoles
{
    public static class RoleReloader
    {
        public static readonly Random Rng = new();
        public static readonly Dictionary<RoleId, RoleBase> AllRoles = new();

        public static void ClearAndReloadRoles()
        {
            foreach (var role in AllRoles.Values) role.ClearAndReload();
        }
    }
}