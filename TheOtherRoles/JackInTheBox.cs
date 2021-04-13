using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

namespace TheOtherRoles{
    
    public class JackInTheBox {
        public static System.Collections.Generic.List<JackInTheBox> AllJackInTheBoxes = new System.Collections.Generic.List<JackInTheBox>();
        public static int JackInTheBoxLimit = 3;
        private static Sprite jackInTheBoxSprite;
        public static bool boxesConvertedToVents = false;

        public static Sprite getJackInTheBoxSprite() {
            if (jackInTheBoxSprite) return jackInTheBoxSprite;
            jackInTheBoxSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.JackInTheBox.png", 175f);
            return jackInTheBoxSprite;
        }

        private GameObject gameObject;
        private Vent vent;

        public JackInTheBox(Vector2 p) {
            gameObject = new GameObject("JackInTheBox");
            Vector3 position = new Vector3(p.x, p.y, PlayerControl.LocalPlayer.transform.position.z + 1f); 
            
            // Create the marker
            gameObject.transform.position = position;
            var boxRenderer = gameObject.AddComponent<SpriteRenderer>();
            boxRenderer.sprite = getJackInTheBoxSprite();

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
            ventRenderer.sprite = getJackInTheBoxSprite();
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