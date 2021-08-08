using UnityEngine;

namespace TheOtherRoles.Roles
{
    // ? Is there a need to make config props in derived classes non-static? 
    public abstract class RoleBase
    {
        protected RoleBase()
        {
            RoleReloader.AllRoles.Add(roleId, this);
        }

        public virtual PlayerControl player { get; set; }
        public abstract Color color { get; }
        protected abstract RoleId roleId { get; }
        public virtual RoleType roleType => RoleType.Crewmate;

        public virtual void ClearAndReload()
        {
            player = null;
        }
    }

    // All roles are derived from Role<T>, so we can use reflection for everything
    // Saves quite a bit of lines
    // It's possible to even reset config values but it'll need some more work
    public abstract class Role<T> : RoleBase where T : Role<T>, new()
    {
        public static T Instance => new();
    }
}