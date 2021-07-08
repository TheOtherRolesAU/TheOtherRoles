using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TheOtherRoles.Objects {
    class Garlic {
        public static List<Garlic> garlics = new List<Garlic>();

        public GameObject garlic;
        private GameObject background;

        private static Sprite garlicSprite;
        public static Sprite getGarlicSprite() {
            if (garlicSprite) return garlicSprite;
            garlicSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Garlic.png", 300f);
            return garlicSprite;
        }

        private static Sprite backgroundSprite;
        public static Sprite getBackgroundSprite() {
            if (backgroundSprite) return backgroundSprite;
            backgroundSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.GarlicBackground.png", 60f);
            return backgroundSprite;
        }

        public Garlic(Vector2 p) {
            garlic = new GameObject("Garlic");
            background = new GameObject("Background");
            background.transform.SetParent(garlic.transform);
            Vector3 position = new Vector3(p.x, p.y, PlayerControl.LocalPlayer.transform.localPosition.z + 0.001f); // just behind player
            garlic.transform.position = position;
            garlic.transform.localPosition = position;
            background.transform.localPosition = new Vector3(0 , 0, -0.01f); // before player

            var garlicRenderer = garlic.AddComponent<SpriteRenderer>();
            garlicRenderer.sprite = getGarlicSprite();
            var backgroundRenderer = background.AddComponent<SpriteRenderer>();
            backgroundRenderer.sprite = getBackgroundSprite();


            garlic.SetActive(true);
            garlics.Add(this);
        }

        public static void clearGarlics() {
            garlics = new List<Garlic>();
        }

        public static void UpdateAll() {
            foreach (Garlic garlic in garlics) {
                if (garlic != null)
                    garlic.Update();
            }
        }

        public void Update() {
            if (background != null)
                background.transform.Rotate(Vector3.forward * 6 * Time.fixedDeltaTime);
        }
    }
}