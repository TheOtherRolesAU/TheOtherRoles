using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Objects {
    public class CustomButton
    {
        public static List<CustomButton> buttons = new List<CustomButton>();
        public static List<CustomButton> handcuffedButtons = null;
        public ActionButton actionButton;
        public Vector3 PositionOffset;
        public float MaxTimer = float.MaxValue;
        public float Timer = 0f;
        private Action OnClick;
        private Action InitialOnClick;
        private Action OnMeetingEnds;
        private Func<bool> HasButton;
        private Func<bool> CouldUse;
        private Action OnEffectEnds;
        public bool HasEffect;
        public bool isEffectActive = false;
        public bool showButtonText = false;
        public float EffectDuration;
        public Sprite Sprite;
        private HudManager hudManager;
        private bool mirror;
        private KeyCode? hotkey;
        private bool cuffed = false;

        public CustomButton(Action OnClick, Func<bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, Vector3 PositionOffset, HudManager hudManager, KeyCode? hotkey, bool HasEffect, float EffectDuration, Action OnEffectEnds, bool mirror = false)
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
            Timer = 16.2f;
            buttons.Add(this);
            actionButton = UnityEngine.Object.Instantiate(hudManager.KillButton, hudManager.KillButton.transform.parent);
            PassiveButton button = actionButton.GetComponent<PassiveButton>();
            this.showButtonText = actionButton.graphic.sprite == Sprite;
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnClick.AddListener((UnityEngine.Events.UnityAction)onClickEvent);

            setActive(false);
        }

        public CustomButton(Action OnClick, Func<bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, Vector3 PositionOffset, HudManager hudManager, KeyCode? hotkey, bool mirror = false)
        : this(OnClick, HasButton, CouldUse, OnMeetingEnds, Sprite, PositionOffset, hudManager, hotkey, false, 0f, () => {}, mirror) { }

        void onClickEvent()
        {
            if (this.Timer < 0f && HasButton() && CouldUse())
            {
                actionButton.graphic.color = new Color(1f, 1f, 1f, 0.3f);
                this.OnClick();

                // Deputy skip onClickEvent if handcuffed
                if (Deputy.handcuffedKnows > 0f) return;

                if (this.HasEffect && !this.isEffectActive) {
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
                actionButton.gameObject.SetActive(true);
                actionButton.graphic.enabled = true;
            } else {
                actionButton.gameObject.SetActive(false);
                actionButton.graphic.enabled = false;
            }
        }

        // Disables all Buttons (except the ones disabled in the Deputy class), and replaces them with new buttons.
        public static void setAllButtonsHandcuffed(bool handcuffed) {
            if (handcuffed && handcuffedButtons == null)
            {
                handcuffedButtons = new List<CustomButton>();
                int maxI = buttons.Count;
                for (int i = 0; i < maxI; i++)
                {
                    try
                    {
                        if (buttons[i].HasButton())  // For each custombutton the player has
                        {
                            addReplacementHandcuffedButton(buttons[i]);
                        }
                        buttons[i].cuffed = true;
                    }
                    catch (NullReferenceException)
                    {
                        System.Console.WriteLine("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");  // Note: idk what this is good for, but i copied it from above /gendelo
                    }
                }

                // Non Custom Buttons. The Originals are disabled / hidden in UpdatePatch.cs already, just need to replace them. Can use any button, as we replace onclick etc anyways.
                // Kill Button if enabled for the Role
                if (HudManager.Instance.KillButton.isActiveAndEnabled) addReplacementHandcuffedButton(HudManagerStartPatch.arsonistButton, new Vector3(0, 1f, 0), couldUse: () => { return HudManager.Instance.KillButton.currentTarget != null; });  // Could use any kill button here, as the position is always the same.
                // Vent Button if enabled
                if (Deputy.disablesVents && PlayerControl.LocalPlayer.roleCanUseVents()) addReplacementHandcuffedButton(HudManagerStartPatch.arsonistButton, new Vector3(-1.8f, 1f, 0), () => { return true; });
                // Sabotage Button if enabled
                if (Deputy.disablesSabotage && PlayerControl.LocalPlayer.Data.Role.IsImpostor) addReplacementHandcuffedButton(HudManagerStartPatch.arsonistButton, new Vector3(-0.9f, 1f, 0), () => { return true; });
                // Use Button if enabled
                if (Deputy.disablesUse && HudManager.Instance.UseButton.isActiveAndEnabled) addReplacementHandcuffedButton(HudManagerStartPatch.arsonistButton, HudManager.Instance.UseButton.transform.localPosition);
            }
            else if (!handcuffed && handcuffedButtons != null)  // Reset to original. Disables the replacements, enables the original buttons.
            {
                foreach (var button in handcuffedButtons)
                {
                    button.HasButton = () => { return false; };
                    button.Update(); // To make it disappear properly.
                }
                buttons.RemoveRange(buttons.Count - handcuffedButtons.Count, handcuffedButtons.Count);
                handcuffedButtons = null;

                foreach (var button in buttons)
                {
                    button.cuffed = false;
                }
            }
        }

        private static void addReplacementHandcuffedButton(CustomButton button, Vector3? positionOffset=null, Func<bool> couldUse=null)
        {
            positionOffset = positionOffset ?? button.PositionOffset;  // For non custom buttons, we can set these manually.
            couldUse = couldUse ?? button.CouldUse;
            CustomButton replacementHandcuffedButton = new CustomButton(() => { }, () => { return true; }, couldUse, () => { }, Deputy.getHandcuffedButtonSprite(), (Vector3)positionOffset, button.hudManager, button.hotkey,
                                                                                        true, Deputy.handcuffTimeRemaining, () => { }, button.mirror);
            replacementHandcuffedButton.Timer = replacementHandcuffedButton.EffectDuration;
            replacementHandcuffedButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
            replacementHandcuffedButton.isEffectActive = true;
            handcuffedButtons.Add(replacementHandcuffedButton);
        }

        private void Update()
        {
            if (PlayerControl.LocalPlayer.Data == null || MeetingHud.Instance || ExileController.Instance || !HasButton()) {
                setActive(false);
                return;
            }
            setActive(hudManager.UseButton.isActiveAndEnabled);

            if (Timer >= 0)  // This had to be reordered, so that the handcuffs do not stop the underlying timers from running
            {
                if (HasEffect && isEffectActive)
                    Timer -= Time.deltaTime;
                else if (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable)
                    Timer -= Time.deltaTime;
            }

            if (Timer <= 0 && HasEffect && isEffectActive)
            {
                isEffectActive = false;
                actionButton.cooldownTimerText.color = Palette.EnabledColor;
                OnEffectEnds();
            }

            if (cuffed)
            {
                setActive(false);
                return;
            }

            actionButton.graphic.sprite = Sprite;
            actionButton.buttonLabelText.enabled = showButtonText; // Only show the text if it's a kill button
            if (hudManager.UseButton != null) {
                Vector3 pos = hudManager.UseButton.transform.localPosition;
                if (mirror) pos = new Vector3(-pos.x, pos.y, pos.z);
                actionButton.transform.localPosition = pos + PositionOffset;
            }
            if (CouldUse()) {
                actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.EnabledColor;
                actionButton.graphic.material.SetFloat("_Desat", 0f);
            } else {
                actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.DisabledClear;
                actionButton.graphic.material.SetFloat("_Desat", 1f);
            }


        
            actionButton.SetCoolDown(Timer, (HasEffect && isEffectActive) ? EffectDuration : MaxTimer);

            // Trigger OnClickEvent if the hotkey is being pressed down
            if (hotkey.HasValue && Input.GetKeyDown(hotkey.Value)) onClickEvent();

            // Deputy disable the button and display Handcuffs instead...
            if (PlayerControl.LocalPlayer == Deputy.handcuffedPlayer) {
                OnClick = () => {
                    Deputy.setHandcuffedKnows();
                    setAllButtonsHandcuffed(true);
                };
            } else // Reset.
            {
                OnClick = InitialOnClick;
            }
         }
    }
}