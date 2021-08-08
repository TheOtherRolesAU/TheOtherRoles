using System.Linq;
using TheOtherRoles.Objects;
using TMPro;
using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class BountyHunter : Role<BountyHunter>
    {
        public static float bountyDuration = 30f;
        public static bool showArrow = true;
        public static float bountyKillCooldown;
        public static float punishmentTime = 15f;
        public static float arrowUpdateInterval = 10f;

        public static float arrowUpdateTimer;
        public static float bountyUpdateTimer;

        public static PlayerControl bounty;
        public static TextMeshPro cooldownText;
        public static Arrow arrow;
        public override Color color => Palette.ImpostorRed;
        protected override RoleId roleId => RoleId.BountyHunter;
        public override RoleType roleType => RoleType.Impostor;

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            arrow = new Arrow(color);
            bounty = null;
            arrowUpdateTimer = 0f;
            bountyUpdateTimer = 0f;
            if (arrow != null && arrow.arrow != null) Object.Destroy(arrow.arrow);
            arrow = null;
            if (cooldownText != null && cooldownText.gameObject != null) Object.Destroy(cooldownText.gameObject);
            cooldownText = null;
            foreach (var p in MapOptions.playerIcons.Values.Where(p => p != null && p.gameObject != null))
                p.gameObject.SetActive(false);


            bountyDuration = CustomOptionHolder.bountyHunterBountyDuration.GetFloat();
            bountyKillCooldown = CustomOptionHolder.bountyHunterReducedCooldown.GetFloat();
            punishmentTime = CustomOptionHolder.bountyHunterPunishmentTime.GetFloat();
            showArrow = CustomOptionHolder.bountyHunterShowArrow.GetBool();
            arrowUpdateInterval = CustomOptionHolder.bountyHunterArrowUpdateInterval.GetFloat();
        }
    }
}