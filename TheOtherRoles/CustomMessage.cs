using UnityEngine;
using System.Collections.Generic;
using System;

using Effects = AEOEPNHOJDP;

namespace TheOtherRoles{

    public class CustomMessage {

        private TextRenderer text;
        private static List<CustomMessage> customMessages = new List<CustomMessage>();

        public CustomMessage(string message, float duration) {
            RoomTracker roomTracker =  HudManager.CHNDKKBEIDG?.roomTracker;
            if (roomTracker != null) {
                GameObject gameObject = UnityEngine.Object.Instantiate(roomTracker.gameObject);
                
                gameObject.transform.SetParent(HudManager.CHNDKKBEIDG.transform);
                UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
                text = gameObject.GetComponent<TextRenderer>();
                
                // Force TextRenderer to build a new Mesh
                text.render = null;
                gameObject.GetComponent<MeshFilter>().mesh = null;
                text.Text = message;
                text.RefreshMesh();

                // Use local position to place it in the player's view instead of the world location
                gameObject.transform.localPosition = new Vector3(0, -1.8f, gameObject.transform.localPosition.z);
                customMessages.Add(this);

                HudManager.CHNDKKBEIDG.StartCoroutine(Effects.LDACHPMFOIF(duration, new Action<float>((p) => {
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