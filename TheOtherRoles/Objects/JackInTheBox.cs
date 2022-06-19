using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;

namespace TheOtherRoles.Objects {

    public class JackInTheBox {
        public static System.Collections.Generic.List<JackInTheBox> AllJackInTheBoxes = new System.Collections.Generic.List<JackInTheBox>();
        public static int JackInTheBoxLimit = 3;
        public static bool boxesConvertedToVents = false;
        public static Sprite[] boxAnimationSprites = new Sprite[18];

        public static Sprite getBoxAnimationSprite(int index) {
            if (boxAnimationSprites == null || boxAnimationSprites.Length == 0) return null;
            index = Mathf.Clamp(index, 0, boxAnimationSprites.Length - 1);
            if (boxAnimationSprites[index] == null)
                boxAnimationSprites[index] = (Helpers.loadSpriteFromResources($"TheOtherRoles.Resources.TricksterAnimation.trickster_box_00{(index + 1):00}.png", 175f));
            return boxAnimationSprites[index];
        }

        public static void startAnimation(int ventId) {
            JackInTheBox box = AllJackInTheBoxes.FirstOrDefault((x) => x?.vent != null && x.vent.Id == ventId);
            if (box == null) return;

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.6f, new Action<float>((p) => {
                if (box.boxRenderer != null) {
                    box.boxRenderer.sprite = getBoxAnimationSprite((int)(p * boxAnimationSprites.Length));
                    if (p == 1f) box.boxRenderer.sprite = getBoxAnimationSprite(0);
                }
            })));
        }

        private GameObject gameObject;
        public Vent vent;
        private SpriteRenderer boxRenderer;

        public JackInTheBox(Vector2 p) {
            gameObject = new GameObject("JackInTheBox"){layer = 11};
            gameObject.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
            Vector3 position = new Vector3(p.x, p.y,  p.y/1000f + 0.01f);
            position += (Vector3)CachedPlayer.LocalPlayer.PlayerControl.Collider.offset; // Add collider offset that DoMove moves the player up at a valid position
            // Create the marker
            gameObject.transform.position = position;
            boxRenderer = gameObject.AddComponent<SpriteRenderer>();
            boxRenderer.sprite = getBoxAnimationSprite(0);

            // Create the vent
            var referenceVent = UnityEngine.Object.FindObjectOfType<Vent>();
            vent = UnityEngine.Object.Instantiate<Vent>(referenceVent);
            vent.gameObject.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
            vent.transform.position = gameObject.transform.position;
            vent.Left = null;
            vent.Right = null;
            vent.Center = null;
            vent.EnterVentAnim = null;
            vent.ExitVentAnim = null;
            vent.Offset = new Vector3(0f, 0.25f, 0f);
            vent.GetComponent<PowerTools.SpriteAnim>()?.Stop();
            vent.Id = MapUtilities.CachedShipStatus.AllVents.Select(x => x.Id).Max() + 1; // Make sure we have a unique id
            var ventRenderer = vent.GetComponent<SpriteRenderer>();
            ventRenderer.sprite = null;  // Use the box.boxRenderer instead
            vent.myRend = ventRenderer;
            var allVentsList = MapUtilities.CachedShipStatus.AllVents.ToList();
            allVentsList.Add(vent);
            MapUtilities.CachedShipStatus.AllVents = allVentsList.ToArray();
            vent.gameObject.SetActive(false);
            vent.name = "JackInTheBoxVent_" + vent.Id;

            // Only render the box for the Trickster
            var playerIsTrickster = CachedPlayer.LocalPlayer.PlayerControl == Trickster.trickster;
            gameObject.SetActive(playerIsTrickster);

            AllJackInTheBoxes.Add(this);
        }

        public static void UpdateStates() {
            if (boxesConvertedToVents == true) return;
            foreach (var box in AllJackInTheBoxes) {
                var playerIsTrickster = CachedPlayer.LocalPlayer.PlayerControl == Trickster.trickster;
                box.gameObject.SetActive(playerIsTrickster);
            }
        }

        public void convertToVent() {
            gameObject.SetActive(true);
            vent.gameObject.SetActive(true);
            return;
        }

        public static void convertToVents() {
            foreach (var box in AllJackInTheBoxes) {
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
            for (var i = 0; i < AllJackInTheBoxes.Count - 1; i++) {
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
