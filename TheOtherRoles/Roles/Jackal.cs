using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Jackal : Role<Jackal>
    {
        public static float cooldown = 30f;
        public static float createSidekickCooldown = 30f;
        public static bool canUseVents = true;
        public static bool canCreateSidekick = true;
        public static bool recursiveSidekicks = true;
        public static bool canCreateSidekickFromImpostor = true;
        public static bool hasImpostorVision;

        public static PlayerControl fakeSidekick;
        public static PlayerControl currentTarget;
        public static readonly List<PlayerControl> FormerJackals = new();

        private static Sprite buttonSprite;
        public override Color color => new Color32(0, 180, 235, byte.MaxValue);
        protected override RoleId roleId => RoleId.Jackal;
        public override RoleType roleType => RoleType.Neutral;

        public static Sprite GetSidekickButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.SidekickButton.png", 115f);
            return buttonSprite;
        }

        public static void RemoveCurrentJackal()
        {
            if (FormerJackals.All(x => x.PlayerId != Instance.player.PlayerId)) FormerJackals.Add(Instance.player);
            Instance.player = null;
            currentTarget = null;
            fakeSidekick = null;
            cooldown = CustomOptionHolder.jackalKillCooldown.GetFloat();
            createSidekickCooldown = CustomOptionHolder.jackalCreateSidekickCooldown.GetFloat();
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            currentTarget = null;
            fakeSidekick = null;
            cooldown = CustomOptionHolder.jackalKillCooldown.GetFloat();
            createSidekickCooldown = CustomOptionHolder.jackalCreateSidekickCooldown.GetFloat();
            canUseVents = CustomOptionHolder.jackalCanUseVents.GetBool();
            canCreateSidekick = CustomOptionHolder.jackalCanCreateSidekick.GetBool();
            recursiveSidekicks = CustomOptionHolder.recursiveSidekicks.GetBool();
            canCreateSidekickFromImpostor = CustomOptionHolder.jackalCanCreateSidekickFromImpostor.GetBool();
            hasImpostorVision = CustomOptionHolder.jackalAndSidekickHaveImpostorVision.GetBool();
            FormerJackals.Clear();
        }
    }
}