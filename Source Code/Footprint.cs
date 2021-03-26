using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles{
    class Footprint {
        private static List<Footprint> footprints = new List<Footprint>();
        private static Sprite sprite;
        private Color color;
        private GameObject footprint;
        private SpriteRenderer spriteRenderer;

        public static Sprite getFootprintSprite() {
            if (sprite) return sprite;
            sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Footprint.png", 600f);
            return sprite;
        }

        public Footprint(float footprintDuration, bool anonymousFootprints, PlayerControl player) {
            if (anonymousFootprints)
                this.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            else
                this.color = Palette.PlayerColors[(int) player.Data.ColorId];

            footprint = new GameObject("Footprint");
            Vector3 position = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z + 1f);
            footprint.transform.position = position;
            footprint.transform.localPosition = position;
            footprint.transform.SetParent(player.transform.parent);

            footprint.transform.Rotate(0.0f, 0.0f, UnityEngine.Random.Range(0.0f, 360.0f));


            spriteRenderer = footprint.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = getFootprintSprite();
            spriteRenderer.color = color;

            footprint.SetActive(true);
            footprints.Add(this);

            Reactor.Coroutines.Start(CoFadeOutAndDestroy(footprintDuration));
        }

        IEnumerator CoFadeOutAndDestroy(float duration)
        {
            for (float t = 0f; t < duration; t += Time.deltaTime) {
                if (spriteRenderer) spriteRenderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp(1f - t/duration, 0f, 1f));

                yield return null;
            }
            UnityEngine.Object.Destroy(footprint);
            footprints.Remove(this);
        }
    }
}