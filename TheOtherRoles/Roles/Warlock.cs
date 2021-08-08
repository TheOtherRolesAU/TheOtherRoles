using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Warlock : Role<Warlock>
    {
        public static float cooldown = 30f;
        public static float rootTime = 5f;

        public static PlayerControl currentTarget;
        public static PlayerControl curseVictim;
        public static PlayerControl curseVictimTarget;
        public static PlayerControl curseKillTarget;

        private static Sprite curseButtonSprite;
        private static Sprite curseKillButtonSprite;
        public override Color color => Palette.ImpostorRed;
        protected override RoleId roleId => RoleId.Warlock;
        public override RoleType roleType => RoleType.Impostor;

        public static Sprite GetCurseButtonSprite()
        {
            if (curseButtonSprite) return curseButtonSprite;
            curseButtonSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.CurseButton.png", 115f);
            return curseButtonSprite;
        }

        public static Sprite GetCurseKillButtonSprite()
        {
            if (curseKillButtonSprite) return curseKillButtonSprite;
            curseKillButtonSprite =
                Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.CurseKillButton.png", 115f);
            return curseKillButtonSprite;
        }

        public static void ResetCurse()
        {
            HudManagerStartPatch.warlockCurseButton.timer = HudManagerStartPatch.warlockCurseButton.maxTimer;
            HudManagerStartPatch.warlockCurseButton.sprite = GetCurseButtonSprite();
            HudManagerStartPatch.warlockCurseButton.killButtonManager.TimerText.color = Palette.EnabledColor;
            currentTarget = null;
            curseVictim = null;
            curseVictimTarget = null;
            curseKillTarget = null;
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            currentTarget = null;
            curseVictim = null;
            curseVictimTarget = null;
            curseKillTarget = null;
            cooldown = CustomOptionHolder.warlockCooldown.GetFloat();
            rootTime = CustomOptionHolder.warlockRootTime.GetFloat();
        }
    }
}