using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Reactor.Extensions;
using System.Linq;

namespace TheOtherRoles{
    
    public class JackInTheBox {
        public static System.Collections.Generic.List<JackInTheBox> AllJackInTheBoxes = new System.Collections.Generic.List<JackInTheBox>();
        public static int JackInTheBoxLimit = 3;
        private static Sprite jackInTheBoxSprite;
        public static bool boxesConvertedToVents = false;

        public static Sprite getJackInTheBoxSprite() {
            if (jackInTheBoxSprite) return jackInTheBoxSprite;
            jackInTheBoxSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.JackInTheBox.png", 300f);
            return jackInTheBoxSprite;
        }

        private GameObject JackInTheBoxGameObject;
        private GameObject JackInTheBoxVentObject;
        public Vent JackInTheBoxVent;

        public JackInTheBox(Vector2 p) {
            JackInTheBoxGameObject = new GameObject("JackInTheBox");
            Vector3 position = new Vector3(p.x, p.y, PlayerControl.LocalPlayer.transform.position.z + 1f);
            JackInTheBoxGameObject.transform.position = position;
            JackInTheBoxGameObject.transform.localPosition = position;

            if (PlayerControl.LocalPlayer == JackInTheBox.jackinthebox) {
                // Only render the box for the Jack-In-The-Box player
                var renderer = JackInTheBoxGameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = getJackInTheBoxSprite();
            }
            JackInTheBoxGameObject.SetActive(true);

            AllJackInTheBoxes.Add(this);
        }

        public void convertToVent() {
            var referenceVent = ShipStatus.Instance.AllVents.FirstOrDefault();
            JackInTheBoxGameObject.SetActive(false);
            
            JackInTheBoxVent = UnityEngine.Object.Instantiate(referenceVent);
            JackInTheBoxVent.gameObject.SetActive(false);
            var position = new Vector3(JackInTheBoxGameObject.transform.position.x, JackInTheBoxGameObject.transform.position.y, referenceVent.transform.position.z);

            JackInTheBoxVent.transform.position = position;
            JackInTheBoxVent.Left = null;
            JackInTheBoxVent.Right = null;
            JackInTheBoxVent.Center = null;
            JackInTheBoxVent.EnterVentAnim = null;
            JackInTheBoxVent.ExitVentAnim = null;

            JackInTheBoxVent.Id = ShipStatus.Instance.AllVents.Select(x => x.Id).Max() + 1; // Make sure we have a unique id

            var renderer = JackInTheBoxVent.GetComponent<SpriteRenderer>();
            renderer.sprite = getJackInTheBoxSprite();

            JackInTheBoxVent.gameObject.SetActive(true);
            
            var allVentsList = ShipStatus.Instance.AllVents.ToList();
            allVentsList.Add(JackInTheBoxVent);
            ShipStatus.Instance.AllVents = allVentsList.ToArray();
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
                a.JackInTheBoxVent.Right = b.JackInTheBoxVent;
                b.JackInTheBoxVent.Left = a.JackInTheBoxVent;
            }
            // Connect first with last
            AllJackInTheBoxes.First().JackInTheBoxVent.Left = AllJackInTheBoxes.Last().JackInTheBoxVent;
            AllJackInTheBoxes.Last().JackInTheBoxVent.Right = AllJackInTheBoxes.First().JackInTheBoxVent;
        }

        public static void clearJackInTheBoxes() {
            boxesConvertedToVents = false;
            AllJackInTheBoxes = new List<JackInTheBox>();
        }
        
    }

}