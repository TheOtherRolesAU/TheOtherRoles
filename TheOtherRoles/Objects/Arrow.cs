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
            if (target == null) target = Vector3.zero;
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
    }
}