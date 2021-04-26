using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class CustomButton
{
    public static List<CustomButton> buttons = new List<CustomButton>();
    public KillButtonManager killButtonManager;
    public Vector3 PositionOffset;
    public float MaxTimer = float.MaxValue;
    public float Timer = 0f;
    private Action OnClick;
    private Action OnMeetingEnds;
    private Func<bool> HasButton;
    private Func<bool> CouldUse;
    private Action OnEffectEnds;
    public bool HasEffect;
    public bool isEffectActive = false;
    public float EffectDuration;
    public Sprite Sprite;
    private HudManager hudManager;
    private bool mirror;
    private KeyCode? hotkey;

    public CustomButton(Action OnClick, Func<bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, Vector3 PositionOffset, HudManager hudManager, KeyCode? hotkey, bool HasEffect, float EffectDuration, Action OnEffectEnds, bool mirror = false)
    {
        this.hudManager = hudManager;
        this.OnClick = OnClick;
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
        killButtonManager = UnityEngine.Object.Instantiate(hudManager.KillButton, hudManager.transform);
        PassiveButton button = killButtonManager.GetComponent<PassiveButton>();
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
            killButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.3f);
            this.OnClick();

            if (this.HasEffect && !this.isEffectActive) {
                this.Timer = this.EffectDuration;
                killButtonManager.TimerText.color = new Color(0F, 0.8F, 0F);
                this.isEffectActive = true;
            }
        }
    }

    public static void HudUpdate()
    {
        buttons.RemoveAll(item => item.killButtonManager == null);
    
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
        buttons.RemoveAll(item => item.killButtonManager == null);
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
            killButtonManager.gameObject.SetActive(true);
            killButtonManager.renderer.enabled = true;
        } else {
            killButtonManager.gameObject.SetActive(false);
            killButtonManager.renderer.enabled = false;
        }
    }

    private void Update()
    {
        if (PlayerControl.LocalPlayer.Data == null || MeetingHud.Instance || ExileController.Instance || !HasButton()) {
            setActive(false);
            return;
        }
        setActive(hudManager.UseButton.isActiveAndEnabled);

        killButtonManager.renderer.sprite = Sprite;
        if (hudManager.UseButton != null) {
            Vector3 pos = hudManager.UseButton.transform.localPosition;
            if (mirror) pos = new Vector3(-pos.x, pos.y, pos.z);
            killButtonManager.transform.localPosition = pos + PositionOffset;
            if (hudManager.KillButton != null) hudManager.KillButton.transform.localPosition = hudManager.UseButton.transform.localPosition - new Vector3(1.3f, 0, 0); // Align the kill button (because it's on another position depending on the screen resolution)
        }
        if (CouldUse()) {
            killButtonManager.renderer.color = Palette.EnabledColor;
            killButtonManager.renderer.material.SetFloat("_Desat", 0f);
        } else {
            killButtonManager.renderer.color = Palette.DisabledClear;
            killButtonManager.renderer.material.SetFloat("_Desat", 1f);
        }

        if (Timer >= 0) {
            if (HasEffect && isEffectActive)
                Timer -= Time.deltaTime;
            else if (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable)
                Timer -= Time.deltaTime;
        }
        
        if (Timer <= 0 && HasEffect && isEffectActive) {
            isEffectActive = false;
            killButtonManager.TimerText.color = Palette.EnabledColor;
            OnEffectEnds();
        }
    
        killButtonManager.SetCoolDown(Timer, (HasEffect && isEffectActive) ? EffectDuration : MaxTimer);

        // Trigger OnClickEvent if the hotkey is being pressed down
        if (hotkey.HasValue && Input.GetKeyDown(hotkey.Value)) onClickEvent();
    }
}