using System;
using System.Collections.Generic;
using TheOtherRoles.Players;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Objects {
    public class CustomButton
    {
        public static List<CustomButton> buttons = new List<CustomButton>();
        public ActionButton actionButton;
        public GameObject actionButtonGameObject;
        public SpriteRenderer actionButtonRenderer;
        public Material actionButtonMat;
        public TextMeshPro actionButtonLabelText;
        public Vector3 PositionOffset;
        public float MaxTimer = float.MaxValue;
        public float Timer = 0f;
        public float DeputyTimer = 0f;
        private Action OnClick;
        private Action InitialOnClick;
        private Action OnMeetingEnds;
        public Func<bool> HasButton;
        public Func<bool> CouldUse;
        private Action OnEffectEnds;
        public bool HasEffect;
        public bool isEffectActive = false;
        public bool showButtonText = false;
        public float EffectDuration;
        public Sprite Sprite;
        public HudManager hudManager;
        public bool mirror;
        public KeyCode? hotkey;
        private string buttonText;
        public bool isHandcuffed = false;
        private static readonly int Desat = Shader.PropertyToID("_Desat");

        public CustomButton(Action OnClick, Func<bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, Vector3 PositionOffset, HudManager hudManager, KeyCode? hotkey, bool HasEffect, float EffectDuration, Action OnEffectEnds, bool mirror = false, string buttonText = "")
        {
            this.hudManager = hudManager;
            this.OnClick = OnClick;
            this.InitialOnClick = OnClick;
            this.HasButton = HasButton;
            this.CouldUse = CouldUse;
            this.PositionOffset = PositionOffset;
            this.OnMeetingEnds = OnMeetingEnds;
            this.HasEffect = HasEffect;
            this.EffectDuration = EffectDuration;
            this.OnEffectEnds = OnEffectEnds;
            this.Sprite = Sprite;
            this.mirror = mirror;
            this.hotkey = hotkey;
            this.buttonText = buttonText;
            Timer = 16.2f;
            buttons.Add(this);
            actionButton = UnityEngine.Object.Instantiate(hudManager.KillButton, hudManager.KillButton.transform.parent);
            actionButtonGameObject = actionButton.gameObject;
            actionButtonRenderer = actionButton.graphic;
            actionButtonMat = actionButtonRenderer.material;
            actionButtonLabelText = actionButton.buttonLabelText;
            PassiveButton button = actionButton.GetComponent<PassiveButton>();
            this.showButtonText = (actionButtonRenderer.sprite == Sprite || buttonText != "");
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnClick.AddListener((UnityEngine.Events.UnityAction)onClickEvent);

            setActive(false);
        }

        public CustomButton(Action OnClick, Func<bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, Vector3 PositionOffset, HudManager hudManager, KeyCode? hotkey, bool mirror = false, string buttonText = "")
        : this(OnClick, HasButton, CouldUse, OnMeetingEnds, Sprite, PositionOffset, hudManager, hotkey, false, 0f, () => {}, mirror, buttonText) { }

        public void onClickEvent()
        {
            if (this.Timer < 0f && HasButton() && CouldUse())
            {
                actionButtonRenderer.color = new Color(1f, 1f, 1f, 0.3f);
                this.OnClick();

                // Deputy skip onClickEvent if handcuffed
                if (Deputy.handcuffedKnows.ContainsKey(CachedPlayer.LocalPlayer.PlayerId) && Deputy.handcuffedKnows[CachedPlayer.LocalPlayer.PlayerId] > 0f) return;

                if (this.HasEffect && !this.isEffectActive) {
                    this.DeputyTimer = this.EffectDuration;
                    this.Timer = this.EffectDuration;
                    actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    this.isEffectActive = true;
                }
            }
        }

        public static void HudUpdate()
        {
            buttons.RemoveAll(item => item.actionButton == null);
        
            for (int i = 0; i < buttons.Count; i++)
            {
                try
                {
                    buttons[i].Update();
                }
                catch (NullReferenceException)
                {
                    System.Console.WriteLine("[WARNING] NullReferenceException from HudUpdate().HasButton(), if theres only one warning its fine");
                }
            }
        }

        public static void MeetingEndedUpdate() {
            buttons.RemoveAll(item => item.actionButton == null);
            for (int i = 0; i < buttons.Count; i++)
            {
                try
                {
                    buttons[i].OnMeetingEnds();
                    buttons[i].Update();
                }
                catch (NullReferenceException)
                {
                    System.Console.WriteLine("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
                }
            }
        }

        public static void ResetAllCooldowns() {
            for (int i = 0; i < buttons.Count; i++)
            {
                try
                {
                    buttons[i].Timer = buttons[i].MaxTimer;
                    buttons[i].DeputyTimer = buttons[i].MaxTimer;
                    buttons[i].Update();
                }
                catch (NullReferenceException)
                {
                    System.Console.WriteLine("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
                }
            }
        }

        public void setActive(bool isActive) {
            if (isActive) {
                actionButtonGameObject.SetActive(true);
                actionButtonRenderer.enabled = true;
            } else {
                actionButtonGameObject.SetActive(false);
                actionButtonRenderer.enabled = false;
            }
        }

        public void Update()
        {
            var localPlayer = CachedPlayer.LocalPlayer;
            var moveable = localPlayer.PlayerControl.moveable;
            
            if (localPlayer.Data == null || MeetingHud.Instance || ExileController.Instance || !HasButton()) {
                setActive(false);
                return;
            }
            setActive(hudManager.UseButton.isActiveAndEnabled);

            if (DeputyTimer >= 0) { // This had to be reordered, so that the handcuffs do not stop the underlying timers from running
                if (HasEffect && isEffectActive)
                    DeputyTimer -= Time.deltaTime;
                else if (!localPlayer.PlayerControl.inVent && moveable)
                    DeputyTimer -= Time.deltaTime;
            }

            if (DeputyTimer <= 0 && HasEffect && isEffectActive) {
                isEffectActive = false;
                actionButton.cooldownTimerText.color = Palette.EnabledColor;
                OnEffectEnds();
            }

            if (isHandcuffed) {
                setActive(false);
                return;
            }

            actionButtonRenderer.sprite = Sprite;
            if (showButtonText && buttonText != ""){
                actionButton.OverrideText(buttonText);
            }
            actionButtonLabelText.enabled = showButtonText; // Only show the text if it's a kill button
            if (hudManager.UseButton != null) {
                Vector3 pos = hudManager.UseButton.transform.localPosition;
                if (mirror) pos = new Vector3(-pos.x, pos.y, pos.z);
                actionButton.transform.localPosition = pos + PositionOffset;
            }
            if (CouldUse()) {
                actionButtonRenderer.color = actionButtonLabelText.color = Palette.EnabledColor;
                actionButtonMat.SetFloat(Desat, 0f);
            } else {
                actionButtonRenderer.color = actionButtonLabelText.color = Palette.DisabledClear;
                actionButtonMat.SetFloat(Desat, 1f);
            }
        
            if (Timer >= 0) {
                if (HasEffect && isEffectActive)
                    Timer -= Time.deltaTime;
                else if (!localPlayer.PlayerControl.inVent && moveable)
                    Timer -= Time.deltaTime;
            }
            
            if (Timer <= 0 && HasEffect && isEffectActive) {
                isEffectActive = false;
                actionButton.cooldownTimerText.color = Palette.EnabledColor;
                OnEffectEnds();
            }
        
            actionButton.SetCoolDown(Timer, (HasEffect && isEffectActive) ? EffectDuration : MaxTimer);

            // Trigger OnClickEvent if the hotkey is being pressed down
            if (hotkey.HasValue && Input.GetKeyDown(hotkey.Value)) onClickEvent();

            // Deputy disable the button and display Handcuffs instead...
            if (Deputy.handcuffedPlayers.Contains(localPlayer.PlayerId)) {
                OnClick = () => {
                    Deputy.setHandcuffedKnows();
                };
            } else // Reset.
            {
                OnClick = InitialOnClick;
            }
        }
    }
}
