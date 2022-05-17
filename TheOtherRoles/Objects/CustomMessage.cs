using UnityEngine;
using System.Collections.Generic;
using System;
using TheOtherRoles.Utilities;

namespace TheOtherRoles.Objects {

    public class CustomMessage {

        private TMPro.TMP_Text text;
        private static List<CustomMessage> customMessages = new List<CustomMessage>();

        public CustomMessage(string message, float duration) {
            RoomTracker roomTracker =  FastDestroyableSingleton<HudManager>.Instance?.roomTracker;
            if (roomTracker != null) {
                GameObject gameObject = UnityEngine.Object.Instantiate(roomTracker.gameObject);
                
                gameObject.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
                UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
                text = gameObject.GetComponent<TMPro.TMP_Text>();
                text.text = message;

                // Use local position to place it in the player's view instead of the world location
                gameObject.transform.localPosition = new Vector3(0, -1.8f, gameObject.transform.localPosition.z);
                customMessages.Add(this);

                FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) => {
                    bool even = ((int)(p * duration / 0.25f)) % 2 == 0; // Bool flips every 0.25 seconds
                    string prefix = (even ? "<color=#FCBA03FF>" : "<color=#FF0000FF>");
                    text.text = prefix + message + "</color>";
                    if (text != null) text.color = even ? Color.yellow : Color.red;
                    if (p == 1f && text != null && text.gameObject != null) {
                        UnityEngine.Object.Destroy(text.gameObject);
                        customMessages.Remove(this);
                    }
                })));
            }
        }
    }
}