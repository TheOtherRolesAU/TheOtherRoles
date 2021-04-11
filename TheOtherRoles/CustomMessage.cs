using UnityEngine;
using System.Collections.Generic;
using System;

using Effects = HLPCBNMDEHF;

namespace TheOtherRoles{
    class CustomMessage {
        private TextRenderer text;
        private static List<CustomMessage> customMessages = new List<CustomMessage>();

        public CustomMessage(string message, float duration) {
            RoomTracker roomTracker =  HudManager.CMJOLNCMAPD?.roomTracker;
            if (roomTracker != null) {
                GameObject gameObject = UnityEngine.Object.Instantiate(roomTracker.gameObject);
                gameObject.transform.SetParent(roomTracker.gameObject.transform.parent);
                UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
                text = gameObject.GetComponent<TextRenderer>();
                
                // Force TextRenderer to build a new Mesh
                text.render = null;
                gameObject.GetComponent<MeshFilter>().mesh = null;
                text.Text = message;
                text.RefreshMesh();

                gameObject.transform.position = new Vector3(0, 0, gameObject.transform.position.z);
                customMessages.Add(this);

                HudManager.CMJOLNCMAPD.StartCoroutine(Effects.LDACHPMFOIF(duration, new Action<float>((p) => {
                    bool even = ((int)(p * duration / 0.25f)) % 2 == 0; // Bool flips every 0.25 seconds
                    string prefix = "[" + (even ? "FCBA03FF" : "FF0000FF") + "]";
                    text.Text = prefix + message;
                    if (text != null) text.Color = even ? Color.yellow : Color.red;
                    if (p == 1f && text?.gameObject != null) {
                        UnityEngine.Object.Destroy(text.gameObject);
                        customMessages.Remove(this);
                    }
                })));
            }
        }
    }
}