using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Vampire : Role<Vampire>
    {
        public static float delay = 10f;
        public static float cooldown = 30f;
        public static bool canKillNearGarlics = true;
        public static bool garlicsActive = true;

        public static PlayerControl currentTarget;
        public static PlayerControl bitten;
        public static bool targetNearGarlic;
        public static bool localPlacedGarlic;

        private static Sprite buttonSprite;
        private static Sprite garlicButtonSprite;
        public override Color color => Palette.ImpostorRed;
        protected override RoleId roleId => RoleId.Vampire;
        public override RoleType roleType => RoleType.Impostor;

        public static Sprite GetButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.VampireButton.png", 115f);
            return buttonSprite;
        }

        public static Sprite GetGarlicButtonSprite()
        {
            if (garlicButtonSprite) return garlicButtonSprite;
            garlicButtonSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.GarlicButton.png", 115f);
            return garlicButtonSprite;
        }

        public override void ClearAndReload()
        {
            player = null;
            bitten = null;
            targetNearGarlic = false;
            localPlacedGarlic = false;
            currentTarget = null;
            garlicsActive = CustomOptionHolder.vampireSpawnRate.GetSelection() > 0;
            delay = CustomOptionHolder.vampireKillDelay.GetFloat();
            cooldown = CustomOptionHolder.vampireCooldown.GetFloat();
            canKillNearGarlics = CustomOptionHolder.vampireCanKillNearGarlics.GetBool();
        }
    }
}