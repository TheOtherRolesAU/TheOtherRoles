using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Sheriff : Role<Sheriff>
    {
        public static float cooldown = 30f;
        public static bool canKillNeutrals;
        public static bool spyCanDieToSheriff;

        public static PlayerControl currentTarget;
        public override Color color => new Color32(248, 205, 70, byte.MaxValue);
        protected override RoleId roleId => RoleId.Sheriff;

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            currentTarget = null;
            cooldown = CustomOptionHolder.sheriffCooldown.GetFloat();
            canKillNeutrals = CustomOptionHolder.sheriffCanKillNeutrals.GetBool();
            spyCanDieToSheriff = CustomOptionHolder.spyCanDieToSheriff.GetBool();
        }
    }
}