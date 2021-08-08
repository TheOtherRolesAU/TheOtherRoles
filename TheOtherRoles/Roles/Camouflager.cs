using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Camouflager : Role<Camouflager>
    {
        public static float cooldown = 30f;
        public static float duration = 10f;

        public static float camouflageTimer;

        private static Sprite buttonSprite;
        public override Color color => Palette.ImpostorRed;
        protected override RoleId roleId => RoleId.Camouflager;
        public override RoleType roleType => RoleType.Impostor;

        public static Sprite GetButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.CamoButton.png", 115f);
            return buttonSprite;
        }

        public static void ResetCamouflage()
        {
            camouflageTimer = 0f;
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p == null) continue;
                if (Morphling.Instance.player != null && Morphling.Instance.player == p) continue;
                p.SetName(p.Data.PlayerName);
                p.SetHat(p.Data.HatId, p.Data.ColorId);
                Helpers.SetSkinWithAnim(p.MyPhysics, p.Data.SkinId);
                p.SetPet(p.Data.PetId);
                p.CurrentPet.Visible = p.Visible;
                p.SetColor(p.Data.ColorId);
            }
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            ResetCamouflage();
            camouflageTimer = 0f;
            cooldown = CustomOptionHolder.camouflagerCooldown.GetFloat();
            duration = CustomOptionHolder.camouflagerDuration.GetFloat();
        }
    }
}