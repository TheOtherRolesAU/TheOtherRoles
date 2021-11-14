using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TheOtherRoles.Objects {
    public class Arrow {
        public float perc = 0.925f;
        public SpriteRenderer image;
        public GameObject arrow;
        private Vector3 oldTarget;

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
        }

        public void Update() {
            Vector3 target = oldTarget;
            if (target == null) target = Vector3.zero;
            Update(target);
        }

        public void Update(Vector3 target, Color? color = null)
        {
            if (arrow == null) return;
            oldTarget = target;

            if (color.HasValue) image.color = color.Value;

            Camera main = Camera.main;
            Vector2 vector = target - main.transform.position;
            float num = vector.magnitude / (main.orthographicSize * perc);
            image.enabled = ((double)num > 0.3);
            Vector2 vector2 = main.WorldToViewportPoint(target);
            if (Between(vector2.x, 0f, 1f) && Between(vector2.y, 0f, 1f))
            {
                arrow.transform.position = target - (Vector3)vector.normalized * 0.6f;
                float num2 = Mathf.Clamp(num, 0f, 1f);
                arrow.transform.localScale = new Vector3(num2, num2, num2);
            }
            else
            {
                Vector2 vector3 = new Vector2(Mathf.Clamp(vector2.x * 2f - 1f, -1f, 1f), Mathf.Clamp(vector2.y * 2f - 1f, -1f, 1f));
                float orthographicSize = main.orthographicSize;
                float num3 = main.orthographicSize * main.aspect;
                Vector3 vector4 = new Vector3(Mathf.LerpUnclamped(0f, num3 * 0.88f, vector3.x), Mathf.LerpUnclamped(0f, orthographicSize * 0.79f, vector3.y), 0f);
                arrow.transform.position = main.transform.position + vector4;
                arrow.transform.localScale = Vector3.one;
            }

            LookAt2d(arrow.transform, target);
        }

        private void LookAt2d(Transform transform, Vector3 target) {
            Vector3 vector = target - transform.position;
            vector.Normalize();
            float num = Mathf.Atan2(vector.y, vector.x);
            if (transform.lossyScale.x < 0f)
                num += 3.1415927f;
            transform.rotation = Quaternion.Euler(0f, 0f, num * 57.29578f);
        }

        private bool Between(float value, float min, float max) {
            return value > min && value < max;
        }
    }
}