using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

using Effects = AEOEPNHOJDP;

namespace TheOtherRoles{
    
    public class JackInTheBox {
        public static System.Collections.Generic.List<JackInTheBox> AllJackInTheBoxes = new System.Collections.Generic.List<JackInTheBox>();
        public static int JackInTheBoxLimit = 3;
        public static bool boxesConvertedToVents = false;
        private static List<Sprite> boxAnimationSprites;

        public static Sprite getBoxSprite() {
            List<Sprite> s = getBoxAnimationSprites();
            if (s.Count > 0) return s.First();
            return null;
        }

        public static List<Sprite> getBoxAnimationSprites() {
            if (boxAnimationSprites != null) return boxAnimationSprites;
            boxAnimationSprites = new List<Sprite>();
            for (int i = 1; i < 19; i++) boxAnimationSprites.Add(Helpers.loadSpriteFromResources($"TheOtherRoles.Resources.TricksterAnimation.trickster_box_00{i:00}.png", 115f));
            return boxAnimationSprites;
        }

        public static void startAnimation(int ventId, bool isOpen) {
            JackInTheBox box = AllJackInTheBoxes.FirstOrDefault((x) => x?.vent != null && x.vent.Id == ventId);
            if (box == null) return;
            Vent vent = box.vent;

            HudManager.CHNDKKBEIDG.StartCoroutine(Effects.DCHLMIDMBHG(0.6f, new Action<float>((p) => {
                var sprites = getBoxAnimationSprites();
                if (vent != null && vent.KJAENOGGEOK != null && sprites != null && sprites.Count > 0) {
                    int index = Mathf.Max(sprites.Count - 1, (int)(p * sprites.Count));
                    vent.KJAENOGGEOK.sprite = sprites[index];
                    if (p == 1f) vent.KJAENOGGEOK.sprite = getBoxSprite();
                }
            })));
        }

        private GameObject gameObject;
        public Vent vent;

        public JackInTheBox(Vector2 p) {
            gameObject = new GameObject("JackInTheBox");
            Vector3 position = new Vector3(p.x, p.y, PlayerControl.LocalPlayer.transform.position.z + 1f); 
            position += (Vector3)PlayerControl.LocalPlayer.Collider.offset; // add Player Collider offset as this gets subtracted again on Vent.DoMove
            // Create the marker
            gameObject.transform.position = position;
            var boxRenderer = gameObject.AddComponent<SpriteRenderer>();
            boxRenderer.sprite = getBoxSprite();

            // Create the vent
            var referenceVent = UnityEngine.Object.FindObjectOfType<Vent>(); 
            vent = UnityEngine.Object.Instantiate<Vent>(referenceVent);
            vent.transform.position = gameObject.transform.position;
            vent.Left = null;
            vent.Right = null;
            vent.Center = null;
            vent.EnterVentAnim = null;
            vent.ExitVentAnim = null;
            vent.GetComponent<PowerTools.SpriteAnim>()?.Stop();
            vent.Id = ShipStatus.Instance.GJHKPDGJHJN.Select(x => x.Id).Max() + 1; // Make sure we have a unique id
            var ventRenderer = vent.GetComponent<SpriteRenderer>();
            ventRenderer.sprite = getBoxSprite();
            vent.KJAENOGGEOK = ventRenderer;
            var allVentsList = ShipStatus.Instance.GJHKPDGJHJN.ToList();
            allVentsList.Add(vent);
            ShipStatus.Instance.GJHKPDGJHJN = allVentsList.ToArray();
            vent.gameObject.SetActive(false);
            vent.name = "JackInTheBoxVent_" + vent.Id;

            // Only render the box for the Trickster
            var playerIsTrickster = PlayerControl.LocalPlayer == Trickster.trickster;
            gameObject.SetActive(playerIsTrickster);

            AllJackInTheBoxes.Add(this);
        }

        public static void UpdateStates() {
            if (boxesConvertedToVents == true) return;
            foreach(var box in AllJackInTheBoxes) {
                var playerIsTrickster = PlayerControl.LocalPlayer == Trickster.trickster;
                box.gameObject.SetActive(playerIsTrickster);
            }
        }

        public void convertToVent() {
            gameObject.SetActive(false);
            vent.gameObject.SetActive(true);
            return;
        }

        public static void convertToVents() {
            foreach(var box in AllJackInTheBoxes) {
                box.convertToVent();
            }
            connectVents();
            boxesConvertedToVents = true;
            return;
        }

        public static bool hasJackInTheBoxLimitReached() {
            return (AllJackInTheBoxes.Count >= JackInTheBoxLimit);
        }

        private static void connectVents() {
            for(var i = 0;i < AllJackInTheBoxes.Count - 1;i++) {
                var a = AllJackInTheBoxes[i];
                var b = AllJackInTheBoxes[i + 1];
                a.vent.Right = b.vent;
                b.vent.Left = a.vent;
            }
            // Connect first with last
            AllJackInTheBoxes.First().vent.Left = AllJackInTheBoxes.Last().vent;
            AllJackInTheBoxes.Last().vent.Right = AllJackInTheBoxes.First().vent;
        }

        public static void clearJackInTheBoxes() {
            boxesConvertedToVents = false;
            AllJackInTheBoxes = new List<JackInTheBox>();
        }
        
    }

}