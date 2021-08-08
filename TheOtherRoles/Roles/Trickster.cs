using TheOtherRoles.Objects;
using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Trickster : Role<Trickster>
    {
        public static float placeBoxCooldown = 30f;
        public static float lightsOutCooldown = 30f;
        public static float lightsOutDuration = 10f;

        public static float lightsOutTimer;

        private static Sprite placeBoxButtonSprite;
        private static Sprite lightOutButtonSprite;
        private static Sprite tricksterVentButtonSprite;
        public override Color color => Palette.ImpostorRed;
        protected override RoleId roleId => RoleId.Trickster;
        public override RoleType roleType => RoleType.Impostor;

        public static Sprite GetPlaceBoxButtonSprite()
        {
            if (placeBoxButtonSprite) return placeBoxButtonSprite;
            placeBoxButtonSprite =
                Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.PlaceJackInTheBoxButton.png", 115f);
            return placeBoxButtonSprite;
        }

        public static Sprite GetLightsOutButtonSprite()
        {
            if (lightOutButtonSprite) return lightOutButtonSprite;
            lightOutButtonSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.LightsOutButton.png", 115f);
            return lightOutButtonSprite;
        }

        public static Sprite GetTricksterVentButtonSprite()
        {
            if (tricksterVentButtonSprite) return tricksterVentButtonSprite;
            tricksterVentButtonSprite =
                Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.TricksterVentButton.png", 115f);
            return tricksterVentButtonSprite;
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            lightsOutTimer = 0f;
            placeBoxCooldown = CustomOptionHolder.tricksterPlaceBoxCooldown.GetFloat();
            lightsOutCooldown = CustomOptionHolder.tricksterLightsOutCooldown.GetFloat();
            lightsOutDuration = CustomOptionHolder.tricksterLightsOutDuration.GetFloat();
            JackInTheBox
                .UpdateStates(); // if the role is erased, we might have to update the state of the created objects
        }
    }
}