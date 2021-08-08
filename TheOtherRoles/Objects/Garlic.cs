using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheOtherRoles.Objects
{
    internal class Garlic
    {
        public static List<Garlic> garlics = new();

        private static Sprite garlicSprite;

        private static Sprite backgroundSprite;
        private readonly GameObject _background;

        public readonly GameObject garlic;

        public Garlic(Vector2 p)
        {
            garlic = new GameObject("Garlic");
            _background = new GameObject("Background");
            _background.transform.SetParent(garlic.transform);
            var position =
                new Vector3(p.x, p.y,
                    PlayerControl.LocalPlayer.transform.localPosition.z + 0.001f); // just behind player
            garlic.transform.position = position;
            garlic.transform.localPosition = position;
            _background.transform.localPosition = new Vector3(0, 0, -0.01f); // before player

            var garlicRenderer = garlic.AddComponent<SpriteRenderer>();
            garlicRenderer.sprite = GetGarlicSprite();
            var backgroundRenderer = _background.AddComponent<SpriteRenderer>();
            backgroundRenderer.sprite = GetBackgroundSprite();


            garlic.SetActive(true);
            garlics.Add(this);
        }

        private static Sprite GetGarlicSprite()
        {
            if (garlicSprite) return garlicSprite;
            garlicSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.Garlic.png", 300f);
            return garlicSprite;
        }

        private static Sprite GetBackgroundSprite()
        {
            if (backgroundSprite) return backgroundSprite;
            backgroundSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.GarlicBackground.png", 60f);
            return backgroundSprite;
        }

        public static void ClearGarlics()
        {
            garlics = new List<Garlic>();
        }

        public static void UpdateAll()
        {
            foreach (var garlic in garlics.Where(garlic => garlic != null))
                garlic.Update();
        }

        private void Update()
        {
            if (_background != null)
                _background.transform.Rotate(Vector3.forward * 6 * Time.fixedDeltaTime);
        }
    }
}