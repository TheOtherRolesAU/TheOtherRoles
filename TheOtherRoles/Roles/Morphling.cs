using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Morphling : Role<Morphling>
    {
        public static float cooldown = 30f;
        public static float duration = 10f;

        public static PlayerControl currentTarget;
        public static PlayerControl sampledTarget;
        public static PlayerControl morphTarget;

        public static float morphTimer;

        private static Sprite sampleSprite;
        private static Sprite morphSprite;
        public override Color color => Palette.ImpostorRed;
        protected override RoleId roleId => RoleId.Morphling;
        public override RoleType roleType => RoleType.Impostor;

        public static Sprite GetSampleSprite()
        {
            if (sampleSprite) return sampleSprite;
            sampleSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.SampleButton.png", 115f);
            return sampleSprite;
        }

        public static Sprite GetMorphSprite()
        {
            if (morphSprite) return morphSprite;
            morphSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.MorphButton.png", 115f);
            return morphSprite;
        }

        public static void ResetMorph()
        {
            morphTarget = null;
            morphTimer = 0f;
            if (Instance.player == null) return;
            Instance.player.SetName(Instance.player.Data.PlayerName);
            Instance.player.SetHat(Instance.player.Data.HatId, Instance.player.Data.ColorId);
            Helpers.SetSkinWithAnim(Instance.player.MyPhysics, Instance.player.Data.SkinId);
            Instance.player.SetPet(Instance.player.Data.PetId);
            Instance.player.CurrentPet.Visible = Instance.player.Visible;
            Instance.player.SetColor(Instance.player.Data.ColorId);
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            ResetMorph();
            currentTarget = null;
            sampledTarget = null;
            morphTarget = null;
            morphTimer = 0f;
            cooldown = CustomOptionHolder.morphlingCooldown.GetFloat();
            duration = CustomOptionHolder.morphlingDuration.GetFloat();
        }
    }
}