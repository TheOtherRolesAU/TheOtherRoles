using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

using Palette = BLMBFIODBKL;
using Effects = AEOEPNHOJDP;

namespace TheOtherRoles{
    class Footprint {
        private static List<Footprint> footprints = new List<Footprint>();
        private static Sprite sprite;
        private Color color;
        private GameObject footprint;
        private SpriteRenderer spriteRenderer;
        private PlayerControl owner;
        private bool anonymousFootprints;

        public static Sprite getFootprintSprite() {
            if (sprite) return sprite;
            sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Footprint.png", 600f);
            return sprite;
        }

        public Footprint(float footprintDuration, bool anonymousFootprints, PlayerControl player) {
            this.owner = player;
            this.anonymousFootprints = anonymousFootprints;
            if (anonymousFootprints)
                this.color = Palette.CALCLMEEPGL[6];
            else
                this.color = Palette.CALCLMEEPGL[(int) player.IDOFAMCIJKE.JFHFMIKFHGG];

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

            PlayerControl.LocalPlayer.StartCoroutine(Effects.LDACHPMFOIF(footprintDuration, new Action<float>((p) => {
            Color c = color;
            if (!anonymousFootprints && owner != null) {
                if (owner == Morphling.morphling && Morphling.morphTimer > 0 && Morphling.morphTarget?.IDOFAMCIJKE != null)
                    c = Palette.CHIIBPFJACF[Morphling.morphTarget.IDOFAMCIJKE.JFHFMIKFHGG];
                else if (Camouflager.camouflageTimer > 0)
                    c = Palette.CALCLMEEPGL[6];
            }

            if (spriteRenderer) spriteRenderer.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(1 - p));

            if (p == 1f && footprint != null) {
                UnityEngine.Object.Destroy(footprint);
                footprints.Remove(this);
            }
            })));
        }
    }
}