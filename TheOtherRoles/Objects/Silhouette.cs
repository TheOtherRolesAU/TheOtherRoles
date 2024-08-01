using Innersloth.DebugTool;
using LibCpp2IL.Elf;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Objects {
    public class Silhouette {
        public GameObject gameObject;
        public float timeRemaining;
        public bool permanent = false;
        private bool visibleForEveryOne = false;
        private SpriteRenderer renderer;

        public static List<Silhouette> silhouettes = new List<Silhouette>();


        private static Sprite SilhouetteSprite;
        public static Sprite getSilhouetteSprite() {
            if (SilhouetteSprite) return SilhouetteSprite;
            SilhouetteSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Silhouette.png", 225f);
            return SilhouetteSprite;
        }

        public Silhouette(Vector3 p, float duration = 1f, bool visibleForEveryOne = true) {
            if (duration <= 0f) {
                permanent = true;
            }
            this.visibleForEveryOne = visibleForEveryOne;
            gameObject = new GameObject("Silhouette");
            gameObject.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
            //Vector3 position = new Vector3(p.x, p.y, CachedPlayer.LocalPlayer.transform.localPosition.z + 0.001f); // just behind player
            Vector3 position = new Vector3(p.x, p.y, p.y / 1000f + 0.01f);
            gameObject.transform.position = position;
            gameObject.transform.localPosition = position;

            renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = getSilhouetteSprite();

            timeRemaining = duration;

            renderer.color = renderer.color.SetAlpha(Yoyo.SilhouetteVisibility);

            bool visible = visibleForEveryOne || PlayerControl.LocalPlayer == Yoyo.yoyo || PlayerControl.LocalPlayer.Data.IsDead;

            gameObject.SetActive(visible);
            silhouettes.Add(this);
        }

        public static void clearSilhouettes() {
            foreach (var sil in silhouettes)
                sil.gameObject.Destroy();
            silhouettes = new();
        }

        public static void UpdateAll() {
            foreach (Silhouette current in new List<Silhouette>(silhouettes)) {
                current.timeRemaining -= Time.fixedDeltaTime;
                bool visible = current.visibleForEveryOne || PlayerControl.LocalPlayer == Yoyo.yoyo || PlayerControl.LocalPlayer.Data.IsDead;
                current.gameObject.SetActive(visible);

                if (visible && current.timeRemaining > 0 && current.timeRemaining < 0.5) {
                    var alphaRatio = current.timeRemaining / 0.5f;
                    current.renderer.color = current.renderer.color.SetAlpha(Yoyo.SilhouetteVisibility * alphaRatio);
                }

                if (current.timeRemaining < 0 && !current.permanent) {
                    current.gameObject.SetActive(false);
                    UnityEngine.Object.Destroy(current.gameObject);
                    silhouettes.Remove(current);
                }
            }
        }
    }
}
