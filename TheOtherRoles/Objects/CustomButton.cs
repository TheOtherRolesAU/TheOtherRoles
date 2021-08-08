using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Objects
{
    public class CustomButton
    {
        private static readonly List<CustomButton> Buttons = new();
        private static readonly int Desat = Shader.PropertyToID("_Desat");
        private readonly Func<bool> _couldUse;
        private readonly Func<bool> _hasButton;
        private readonly HudManager _hudManager;
        private readonly bool _mirror;
        private readonly Action _onClick;
        private readonly Action _onEffectEnds;
        private readonly Action _onMeetingEnds;
        private readonly Vector3 _positionOffset;
        private readonly bool _showButtonText;
        public readonly KillButtonManager killButtonManager;
        private KeyCode? _hotkey;
        public float effectDuration;
        public bool hasEffect;
        public bool isEffectActive;
        public float maxTimer = float.MaxValue;
        public Sprite sprite;
        public float timer;

        public CustomButton(Action onClick, Func<bool> hasButton, Func<bool> couldUse, Action onMeetingEnds,
            Sprite sprite, Vector3 positionOffset, HudManager hudManager, KeyCode? hotkey, bool hasEffect,
            float effectDuration, Action onEffectEnds, bool mirror = false)
        {
            _hudManager = hudManager;
            _onClick = onClick;
            _hasButton = hasButton;
            _couldUse = couldUse;
            _positionOffset = positionOffset;
            _onMeetingEnds = onMeetingEnds;
            this.hasEffect = hasEffect;
            this.effectDuration = effectDuration;
            _onEffectEnds = onEffectEnds;
            this.sprite = sprite;
            _mirror = mirror;
            _hotkey = hotkey;
            timer = 16.2f;
            Buttons.Add(this);
            killButtonManager = Object.Instantiate(hudManager.KillButton, hudManager.transform);
            _showButtonText = killButtonManager.renderer.sprite == sprite;
            var button = killButtonManager.GetComponent<PassiveButton>();
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnClick.AddListener((UnityAction) OnClickEvent);

            SetActive(false);
        }

        public CustomButton(Action onClick, Func<bool> hasButton, Func<bool> couldUse, Action onMeetingEnds,
            Sprite sprite, Vector3 positionOffset, HudManager hudManager, KeyCode? hotkey, bool mirror = false)
            : this(onClick, hasButton, couldUse, onMeetingEnds, sprite, positionOffset, hudManager, hotkey, false, 0f,
                () => { }, mirror)
        {
        }

        private void OnClickEvent()
        {
            if (!(timer < 0f) || !_hasButton() || !_couldUse()) return;
            killButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.3f);
            _onClick();

            if (!hasEffect || isEffectActive) return;
            timer = effectDuration;
            killButtonManager.TimerText.color = new Color(0F, 0.8F, 0F);
            isEffectActive = true;
        }

        public static void HudUpdate()
        {
            Buttons.RemoveAll(item => item.killButtonManager == null);

            foreach (var t in Buttons)
                try
                {
                    t.Update();
                }
                catch (NullReferenceException)
                {
                    System.Console.WriteLine(
                        "[WARNING] NullReferenceException from HudUpdate().HasButton(), if theres only one warning its fine");
                }
        }

        public static void MeetingEndedUpdate()
        {
            Buttons.RemoveAll(item => item.killButtonManager == null);
            foreach (var t in Buttons)
                try
                {
                    t._onMeetingEnds();
                    t.Update();
                }
                catch (NullReferenceException)
                {
                    System.Console.WriteLine(
                        "[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
                }
        }

        public static void ResetAllCooldowns()
        {
            foreach (var t in Buttons)
                try
                {
                    t.timer = t.maxTimer;
                    t.Update();
                }
                catch (NullReferenceException)
                {
                    System.Console.WriteLine(
                        "[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
                }
        }

        private void SetActive(bool isActive)
        {
            if (isActive)
            {
                killButtonManager.gameObject.SetActive(true);
                killButtonManager.renderer.enabled = true;
            }
            else
            {
                killButtonManager.gameObject.SetActive(false);
                killButtonManager.renderer.enabled = false;
            }
        }

        private void Update()
        {
            if (PlayerControl.LocalPlayer.Data == null || MeetingHud.Instance || ExileController.Instance ||
                !_hasButton())
            {
                SetActive(false);
                return;
            }

            SetActive(_hudManager.UseButton.isActiveAndEnabled);

            killButtonManager.renderer.sprite = sprite;
            killButtonManager.killText.enabled = _showButtonText; // Only show the text if it's a kill button
            if (_hudManager.UseButton != null)
            {
                var pos = _hudManager.UseButton.transform.localPosition;
                if (_mirror) pos = new Vector3(-pos.x, pos.y, pos.z);
                killButtonManager.transform.localPosition = pos + _positionOffset;
                if (_hudManager.KillButton != null)
                    _hudManager.KillButton.transform.localPosition =
                        _hudManager.UseButton.transform.localPosition -
                        new Vector3(1.3f, 0,
                            0); // Align the kill button (because it's on another position depending on the screen resolution)
            }

            if (_couldUse())
            {
                killButtonManager.renderer.color = killButtonManager.killText.color = Palette.EnabledColor;
                killButtonManager.renderer.material.SetFloat(Desat, 0f);
            }
            else
            {
                killButtonManager.renderer.color = killButtonManager.killText.color = Palette.DisabledClear;
                killButtonManager.renderer.material.SetFloat(Desat, 1f);
            }

            if (timer >= 0)
            {
                if (hasEffect && isEffectActive)
                    timer -= Time.deltaTime;
                else if (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable)
                    timer -= Time.deltaTime;
            }

            if (timer <= 0 && hasEffect && isEffectActive)
            {
                isEffectActive = false;
                killButtonManager.TimerText.color = Palette.EnabledColor;
                _onEffectEnds();
            }

            killButtonManager.SetCoolDown(timer, hasEffect && isEffectActive ? effectDuration : maxTimer);

            // Trigger OnClickEvent if the hotkey is being pressed down
            if (_hotkey.HasValue && Input.GetKeyDown(_hotkey.Value)) OnClickEvent();
        }
    }
}