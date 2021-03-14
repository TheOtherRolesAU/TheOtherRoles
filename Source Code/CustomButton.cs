using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Reactor.Extensions;
using Reactor.Unstrip;
using UnityEngine;

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

    public CustomButton(Action OnClick, Func<bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, Vector3 PositionOffset, HudManager hudManager, bool HasEffect, float EffectDuration, Action OnEffectEnds)
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
        Timer = 16.2f;
        buttons.Add(this);
        killButtonManager = UnityEngine.Object.Instantiate(hudManager.KillButton, hudManager.transform);
        PassiveButton button = killButtonManager.GetComponent<PassiveButton>();
        button.OnClick.RemoveAllListeners();
        button.OnClick.AddListener((UnityEngine.Events.UnityAction)listener);

        void listener()
        {
            if (this.Timer < 0f && HasButton() && CouldUse())
            {
                killButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.3f);
                this.OnClick();

                if (this.HasEffect && !this.isEffectActive) {
                    this.Timer = this.EffectDuration;
                    killButtonManager.TimerText.Color = new Color(0F, 0.8F, 0F);
                    this.isEffectActive = true;
                }
            }
        }
        setActive(false);
    }

    public CustomButton(Action OnClick, Func<bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, Vector3 PositionOffset, HudManager hudManager)
    : this(OnClick, HasButton, CouldUse, OnMeetingEnds, Sprite, PositionOffset, hudManager, false, 0f, () => {}) { }

    public static void HudUpdate()
    {
        buttons.RemoveAll(item => item.killButtonManager == null);

        if (MeetingHud.Instance || ExileController.Instance) return;
    
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
        if (PlayerControl.LocalPlayer.Data == null || !HasButton()) {
            setActive(false);
            return;
        }
        setActive(hudManager.UseButton.isActiveAndEnabled);

        killButtonManager.renderer.sprite = Sprite;

        if (killButtonManager.transform.position == hudManager.KillButton.transform.position)
        {
            Vector3 vector = killButtonManager.transform.localPosition;
            vector += new Vector3(PositionOffset.x, PositionOffset.y);
            killButtonManager.transform.localPosition = vector;
        }


        if (CouldUse()) {
            killButtonManager.renderer.color = Palette.EnabledColor;
            killButtonManager.renderer.material.SetFloat("_Desat", 0f);
        } else {
            killButtonManager.renderer.color = Palette.DisabledColor;
            killButtonManager.renderer.material.SetFloat("_Desat", 1f);
        }

        if (Timer >= 0) {
            Timer -= Time.deltaTime;
        }
        
        if (Timer <= 0 && HasEffect && isEffectActive) {
            isEffectActive = false;
            killButtonManager.TimerText.Color = Palette.EnabledColor;
            OnEffectEnds();
        }
    
        killButtonManager.SetCoolDown(Timer, (HasEffect && isEffectActive) ? EffectDuration : MaxTimer);
    }
}