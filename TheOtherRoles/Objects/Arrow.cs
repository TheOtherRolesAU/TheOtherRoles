using UnityEngine;

namespace TheOtherRoles.Objects {
    public class Arrow {
        public float perc = 0.925f;
        public SpriteRenderer image;
        public GameObject arrow;
        private Vector3 oldTarget;
        private ArrowBehaviour arrowBehaviour;

        private static Sprite sprite;
        public static Sprite getSprite() {
            if (sprite) return sprite;
            sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Arrow.png", 200f);
            return sprite;
        }


        public Arrow(Color color) {
            arrow = new GameObject("Arrow");
            arrow.layer = 5;
            image = arrow.AddComponent<SpriteRenderer>();
            image.sprite = getSprite();
            image.color = color;
            arrowBehaviour = arrow.AddComponent<ArrowBehaviour>();
            arrowBehaviour.image = image;
        }

        public void Update() {
            Vector3 target = oldTarget;
            Update(target);
        }

        public void Update(Vector3 target, Color? color = null)
        {
            if (arrow == null) return;
            oldTarget = target;

            if (color.HasValue) image.color = color.Value;

            arrowBehaviour.target = target;
            arrowBehaviour.Update();
        }

         public static void UpdateProximity(Vector3 position) {
            if (!GameManager.Instance.GameHasStarted) return;
            
            if (Tracker.DangerMeterParent == null) {
                Tracker.DangerMeterParent = GameObject.Instantiate(GameObject.Find("ImpostorDetector"), HudManager.Instance.transform);
                Tracker.Meter = Tracker.DangerMeterParent.transform.GetChild(0).GetComponent<DangerMeter>();
                Tracker.DangerMeterParent.transform.localPosition = new(3.7f, -1.6f, 0);
                var backgroundrend = Tracker.DangerMeterParent.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
                backgroundrend.color = backgroundrend.color.SetAlpha(0.5f);
            }
            Tracker.DangerMeterParent.SetActive(MeetingHud.Instance == null && LobbyBehaviour.Instance == null && !Tracker.tracker.Data.IsDead && Tracker.tracked != null);
            Tracker.Meter.gameObject.SetActive(MeetingHud.Instance == null && LobbyBehaviour.Instance == null && !Tracker.tracker.Data.IsDead && Tracker.tracked != null);
            if (Tracker.tracker.Data.IsDead) return;
            if (Tracker.tracked == null) {
                Tracker.Meter.SetDangerValue(0, 0);
                return;
            }
            if (Tracker.DangerMeterParent.transform.localPosition.x != 3.7f) Tracker.DangerMeterParent.transform.localPosition = new(3.7f, -1.6f, 0);
            float num = float.MaxValue;
            float dangerLevel1;
            float dangerLevel2;

            float sqrMagnitude = (position - Tracker.tracker.transform.position).sqrMagnitude;
            if (sqrMagnitude < (55 * GameOptionsManager.Instance.currentNormalGameOptions.PlayerSpeedMod) && num > sqrMagnitude)
            {
                num = sqrMagnitude;
            }

            dangerLevel1 = Mathf.Clamp01((55 - num) / (55 - 15 * GameOptionsManager.Instance.currentNormalGameOptions.PlayerSpeedMod));
            dangerLevel2 = Mathf.Clamp01((15 - num) / (15 * GameOptionsManager.Instance.currentNormalGameOptions.PlayerSpeedMod));

            Tracker.Meter.SetDangerValue(dangerLevel1, dangerLevel2);
        }
    }
}