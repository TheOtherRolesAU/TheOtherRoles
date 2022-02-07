using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TheOtherRoles.Objects {
    class NinjaTrace {
        public static List<NinjaTrace> traces = new List<NinjaTrace>();

        public GameObject trace;
        private float timeRemaining;
        
        private static Sprite TraceSprite;
        public static Sprite getTraceSprite() {
            if (TraceSprite) return TraceSprite;
            TraceSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.NinjaTrace.png", 300f);
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
                }
            }
        }
    }
}