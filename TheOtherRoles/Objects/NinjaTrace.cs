using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TheOtherRoles.Objects {
    class NinjaTrace {
        public static List<NinjaTrace> traces = new List<NinjaTrace>();

        private GameObject trace;
        private float timeRemaining;
        
        private static Sprite TraceSprite;
        public static Sprite getTraceSprite() {
            if (TraceSprite) return TraceSprite;
            TraceSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.NinjaTraceW.png", 300f);
            return TraceSprite;
        }

        public NinjaTrace(Vector2 p, float duration=1f) {
            trace = new GameObject("NinjaTrace");
            Vector3 position = new Vector3(p.x, p.y, PlayerControl.LocalPlayer.transform.localPosition.z + 0.001f); // just behind player
            trace.transform.position = position;
            trace.transform.localPosition = position;
            
            var traceRenderer = trace.AddComponent<SpriteRenderer>();
            traceRenderer.sprite = getTraceSprite();

            timeRemaining = duration;

            // display the ninjas color in the trace for duration / 4 seconds, could be an extra setting.
            HudManager.Instance.StartCoroutine(Effects.Lerp(duration / 4, new Action<float>((p) => {  // time could be fixed as a setting manually etc.
                Color c = Palette.PlayerColors[(int)Ninja.ninja.Data.DefaultOutfit.ColorId];
                if (Camouflager.camouflageTimer > 0) {
                    c = Palette.PlayerColors[6];
                }

                Color g = Palette.PlayerColors[6];  // default grey

                Color combinedColor = Mathf.Clamp01(p) * g + Mathf.Clamp01(1 - p) * c;

                if (traceRenderer) traceRenderer.color = combinedColor;
            })));


            trace.SetActive(true);
            traces.Add(this);
        }

        public static void clearTraces() {
            traces = new List<NinjaTrace>();
        }

        public static void UpdateAll() {
            foreach (NinjaTrace traceCurrent in traces)
            {
                traceCurrent.timeRemaining -= Time.fixedDeltaTime;
                if (traceCurrent.timeRemaining < 0)
                {
                    traceCurrent.trace.SetActive(false);
                    UnityEngine.Object.Destroy(traceCurrent.trace);
                    traces.Remove(traceCurrent);
                }
            }
        }
    }
}