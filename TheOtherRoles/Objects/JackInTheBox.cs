using System;
using System.Collections.Generic;
using System.Linq;
using PowerTools;
using TheOtherRoles.Roles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Objects
{
    public class JackInTheBox
    {
        private const int JackInTheBoxLimit = 3;
        private static List<JackInTheBox> allJackInTheBoxes = new();
        public static bool boxesConvertedToVents;
        private static readonly Sprite[] BoxAnimationSprites = new Sprite[18];

        private readonly GameObject _gameObject;
        private readonly Vent _vent;

        public JackInTheBox(Vector2 p)
        {
            _gameObject = new GameObject("JackInTheBox");
            var position = new Vector3(p.x, p.y, PlayerControl.LocalPlayer.transform.position.z + 1f);
            position += (Vector3) PlayerControl.LocalPlayer.Collider
                .offset; // Add collider offset that DoMove moves the player up at a valid position
            // Create the marker
            _gameObject.transform.position = position;
            var boxRenderer = _gameObject.AddComponent<SpriteRenderer>();
            boxRenderer.sprite = GetBoxAnimationSprite(0);

            // Create the vent
            var referenceVent = Object.FindObjectOfType<Vent>();
            _vent = Object.Instantiate(referenceVent);
            _vent.transform.position = _gameObject.transform.position;
            _vent.Left = null;
            _vent.Right = null;
            _vent.Center = null;
            _vent.EnterVentAnim = null;
            _vent.ExitVentAnim = null;
            _vent.Offset = new Vector3(0f, 0.25f, 0f);
            _vent.GetComponent<SpriteAnim>()?.Stop();
            _vent.Id = ShipStatus.Instance.AllVents.Select(x => x.Id).Max() + 1; // Make sure we have a unique id
            var ventRenderer = _vent.GetComponent<SpriteRenderer>();
            ventRenderer.sprite = GetBoxAnimationSprite(0);
            _vent.myRend = ventRenderer;
            var allVentsList = ShipStatus.Instance.AllVents.ToList();
            allVentsList.Add(_vent);
            ShipStatus.Instance.AllVents = allVentsList.ToArray();
            _vent.gameObject.SetActive(false);
            _vent.name = "JackInTheBoxVent_" + _vent.Id;

            // Only render the box for the Trickster
            var playerIsTrickster = PlayerControl.LocalPlayer == Trickster.Instance.player;
            _gameObject.SetActive(playerIsTrickster);

            allJackInTheBoxes.Add(this);
        }

        private static Sprite GetBoxAnimationSprite(int index)
        {
            if (BoxAnimationSprites == null || BoxAnimationSprites.Length == 0) return null;
            index = Mathf.Clamp(index, 0, BoxAnimationSprites.Length - 1);
            if (BoxAnimationSprites[index] == null)
                BoxAnimationSprites[index] = Helpers.LoadSpriteFromResources(
                    $"TheOtherRoles.Resources.TricksterAnimation.trickster_box_00{index + 1:00}.png", 175f);
            return BoxAnimationSprites[index];
        }

        public static void StartAnimation(int ventId)
        {
            var box = allJackInTheBoxes.FirstOrDefault(x => x?._vent != null && x._vent.Id == ventId);
            if (box == null) return;
            var vent = box._vent;

            HudManager.Instance.StartCoroutine(Effects.Lerp(0.6f, new Action<float>(p =>
            {
                if (vent == null || vent.myRend == null) return;
                vent.myRend.sprite = GetBoxAnimationSprite((int) (p * BoxAnimationSprites.Length));
                if (Math.Abs(p - 1f) < 0.1f) vent.myRend.sprite = GetBoxAnimationSprite(0);
            })));
        }

        public static void UpdateStates()
        {
            if (boxesConvertedToVents) return;
            foreach (var box in allJackInTheBoxes)
            {
                var playerIsTrickster = PlayerControl.LocalPlayer == Trickster.Instance.player;
                box._gameObject.SetActive(playerIsTrickster);
            }
        }

        private void ConvertToVent()
        {
            _gameObject.SetActive(false);
            _vent.gameObject.SetActive(true);
        }

        public static void ConvertToVents()
        {
            foreach (var box in allJackInTheBoxes) box.ConvertToVent();
            ConnectVents();
            boxesConvertedToVents = true;
        }

        public static bool HasJackInTheBoxLimitReached()
        {
            return allJackInTheBoxes.Count >= JackInTheBoxLimit;
        }

        private static void ConnectVents()
        {
            for (var i = 0; i < allJackInTheBoxes.Count - 1; i++)
            {
                var a = allJackInTheBoxes[i];
                var b = allJackInTheBoxes[i + 1];
                a._vent.Right = b._vent;
                b._vent.Left = a._vent;
            }

            // Connect first with last
            allJackInTheBoxes.First()._vent.Left = allJackInTheBoxes.Last()._vent;
            allJackInTheBoxes.Last()._vent.Right = allJackInTheBoxes.First()._vent;
        }

        public static void ClearJackInTheBoxes()
        {
            boxesConvertedToVents = false;
            allJackInTheBoxes = new List<JackInTheBox>();
        }
    }
}