using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Sidekick : Role<Sidekick>
    {
        public static PlayerControl currentTarget;

        public static float cooldown = 30f;
        public static bool canUseVents = true;
        public static bool canKill = true;
        public static bool promotesToJackal = true;
        public static bool hasImpostorVision;
        public override Color color => new Color32(0, 180, 235, byte.MaxValue);
        protected override RoleId roleId => RoleId.Sidekick;
        public override RoleType roleType => RoleType.Neutral;

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            currentTarget = null;
            cooldown = CustomOptionHolder.jackalKillCooldown.GetFloat();
            canUseVents = CustomOptionHolder.sidekickCanUseVents.GetBool();
            canKill = CustomOptionHolder.sidekickCanKill.GetBool();
            promotesToJackal = CustomOptionHolder.sidekickPromotesToJackal.GetBool();
            hasImpostorVision = CustomOptionHolder.jackalAndSidekickHaveImpostorVision.GetBool();
        }
    }
}