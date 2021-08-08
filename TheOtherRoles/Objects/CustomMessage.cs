using System;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Objects
{
    public class CustomMessage
    {
        public CustomMessage(string message, float duration)
        {
            var roomTracker = HudManager.Instance.roomTracker;
            if (!roomTracker) return;
            var gameObject = Object.Instantiate(roomTracker.gameObject, HudManager.Instance.transform, true);

            Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
            var text = gameObject.GetComponent<TMP_Text>();
            text.text = message;

            // Use local position to place it in the player's view instead of the world location
            gameObject.transform.localPosition = new Vector3(0, -1.8f, gameObject.transform.localPosition.z);

            HudManager.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>(p =>
            {
                var even = (int) (p * duration / 0.25f) % 2 == 0; // Bool flips every 0.25 seconds
                var prefix = even ? "<color=#FCBA03FF>" : "<color=#FF0000FF>";
                text.text = prefix + message + "</color>";
                if (text != null) text.color = even ? Color.yellow : Color.red;
                if (Math.Abs(p - 1f) > 0.1 || text == null || text.gameObject == null) return;
                Object.Destroy(text.gameObject);
            })));
        }
    }
}