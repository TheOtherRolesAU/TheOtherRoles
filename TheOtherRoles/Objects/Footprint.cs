using System;
using TheOtherRoles.Roles;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TheOtherRoles.Objects
{
    internal class Footprint
    {
        private static Sprite sprite;

        public Footprint(float footprintDuration, bool anonymousFootprints, PlayerControl player)
        {
            var owner = player;
            Color color = Palette.PlayerColors[anonymousFootprints ? 6 : player.Data.ColorId];

            var footprint = new GameObject("Footprint");
            footprint.transform.position = player.transform.position + new Vector3(0, 0, 1f);
            footprint.transform.localPosition = footprint.transform.position;
            footprint.transform.SetParent(player.transform.parent);

            footprint.transform.Rotate(0.0f, 0.0f, Random.Range(0.0f, 360.0f));


            var spriteRenderer = footprint.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetFootprintSprite();
            spriteRenderer.color = color;

            footprint.SetActive(true);

            HudManager.Instance.StartCoroutine(Effects.Lerp(footprintDuration, new Action<float>(p =>
            {
                var c = color;
                if (!anonymousFootprints && owner != null)
                {
                    if (owner == Morphling.Instance.player && Morphling.morphTimer > 0 && Morphling.morphTarget &&
                        Morphling.morphTarget.Data != null)
                        c = Palette.ShadowColors[Morphling.morphTarget.Data.ColorId];
                    else if (Camouflager.camouflageTimer > 0)
                        c = Palette.PlayerColors[6];
                }

                if (spriteRenderer) spriteRenderer.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(1 - p));

                if (!(Math.Abs(p - 1f) < 0.1f) || footprint == null) return;
                Object.Destroy(footprint);
            })));
        }

        private static Sprite GetFootprintSprite()
        {
            if (sprite) return sprite;
            sprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.Footprint.png", 600f);
            return sprite;
        }
    }
}