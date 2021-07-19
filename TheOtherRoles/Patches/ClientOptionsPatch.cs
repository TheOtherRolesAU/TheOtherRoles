  
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using Hazel;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    public class OptionsMenuBehaviourStartPatch {
        private static Vector3? origin;
        private static ToggleButtonBehaviour streamerModeButton;
        private static ToggleButtonBehaviour ghostsSeeTasksButton;
        private static ToggleButtonBehaviour ghostsSeeRolesButton;
        private static ToggleButtonBehaviour ghostsSeeVotesButton;
        private static ToggleButtonBehaviour showRoleSummaryButton;

        public static float xOffset = 1.75f;
        public static float yOffset = -0.5f;

        private static void updateToggle(ToggleButtonBehaviour button, string text, bool on) {
            if (button == null || button.gameObject == null) return;

            Color color = on ? new Color(0f, 1f, 0.16470589f, 1f) : Color.white;
            button.Background.color = color;
            button.Text.text = $"{text}{(on ? "On" : "Off")}";
            if (button.Rollover) button.Rollover.ChangeOutColor(color);
        }

        private static ToggleButtonBehaviour createCustomToggle(string text, bool on, Vector3 offset, UnityEngine.Events.UnityAction onClick, OptionsMenuBehaviour __instance) {
            if (__instance.CensorChatButton != null) {
                var button = UnityEngine.Object.Instantiate(__instance.CensorChatButton, __instance.CensorChatButton.transform.parent);
                button.transform.localPosition = (origin ?? Vector3.zero) + offset;
                PassiveButton passiveButton = button.GetComponent<PassiveButton>();
                passiveButton.OnClick = new Button.ButtonClickedEvent();
                passiveButton.OnClick.AddListener(onClick);
                updateToggle(button, text, on);
                
                return button;
            }
            return null;
        }

        public static void Postfix(OptionsMenuBehaviour __instance) {
            if (__instance.CensorChatButton != null) {
                if (origin == null) origin = __instance.CensorChatButton.transform.localPosition + Vector3.up * 0.25f;
                __instance.CensorChatButton.transform.localPosition = origin.Value + Vector3.left * xOffset;
                __instance.CensorChatButton.transform.localScale = Vector3.one * 2f / 3f;
            }

            if ((streamerModeButton == null || streamerModeButton.gameObject == null)) {
                streamerModeButton = createCustomToggle("Streamer Mode: ", TheOtherRolesPlugin.StreamerMode.Value, Vector3.zero, (UnityEngine.Events.UnityAction)streamerModeToggle, __instance);

                void streamerModeToggle() {
                    TheOtherRolesPlugin.StreamerMode.Value = !TheOtherRolesPlugin.StreamerMode.Value;
                    updateToggle(streamerModeButton, "Streamer Mode: ", TheOtherRolesPlugin.StreamerMode.Value);
                }
            }

            if ((ghostsSeeTasksButton == null || ghostsSeeTasksButton.gameObject == null)) {
                ghostsSeeTasksButton = createCustomToggle("Ghosts See Remaining Tasks: ", TheOtherRolesPlugin.GhostsSeeTasks.Value, Vector3.right * xOffset, (UnityEngine.Events.UnityAction)ghostsSeeTaskToggle, __instance);

                void ghostsSeeTaskToggle() {
                    TheOtherRolesPlugin.GhostsSeeTasks.Value = !TheOtherRolesPlugin.GhostsSeeTasks.Value;
                    MapOptions.ghostsSeeTasks = TheOtherRolesPlugin.GhostsSeeTasks.Value; 
                    updateToggle(ghostsSeeTasksButton, "Ghosts See Remaining Tasks: ", TheOtherRolesPlugin.GhostsSeeTasks.Value);
                }
            }

            if ((ghostsSeeRolesButton == null || ghostsSeeRolesButton.gameObject == null)) {
                ghostsSeeRolesButton = createCustomToggle("Ghosts See Roles: ", TheOtherRolesPlugin.GhostsSeeRoles.Value, new Vector2(-xOffset, yOffset), (UnityEngine.Events.UnityAction)ghostsSeeRolesToggle, __instance);

                void ghostsSeeRolesToggle() {
                    TheOtherRolesPlugin.GhostsSeeRoles.Value = !TheOtherRolesPlugin.GhostsSeeRoles.Value;
                    MapOptions.ghostsSeeRoles = TheOtherRolesPlugin.GhostsSeeRoles.Value; 
                    updateToggle(ghostsSeeRolesButton, "Ghosts See Roles: ", TheOtherRolesPlugin.GhostsSeeRoles.Value);
                }
            }

            if ((ghostsSeeVotesButton == null || ghostsSeeVotesButton.gameObject == null)) {
                ghostsSeeVotesButton = createCustomToggle("Ghosts See Votes: ", TheOtherRolesPlugin.GhostsSeeVotes.Value, new Vector2(0, yOffset), (UnityEngine.Events.UnityAction)ghostsSeeVotesToggle, __instance);

                void ghostsSeeVotesToggle() {
                    TheOtherRolesPlugin.GhostsSeeVotes.Value = !TheOtherRolesPlugin.GhostsSeeVotes.Value;
                    MapOptions.ghostsSeeVotes = TheOtherRolesPlugin.GhostsSeeVotes.Value; 
                    updateToggle(ghostsSeeVotesButton, "Ghosts See Votes: ", TheOtherRolesPlugin.GhostsSeeVotes.Value);
                }
            }
            
            if ((showRoleSummaryButton == null || showRoleSummaryButton.gameObject == null)) {
                showRoleSummaryButton = createCustomToggle("Role Summary: ", TheOtherRolesPlugin.ShowRoleSummary.Value, new Vector2(xOffset, yOffset), (UnityEngine.Events.UnityAction)showRoleSummaryToggle, __instance);

                void showRoleSummaryToggle() {
                    TheOtherRolesPlugin.ShowRoleSummary.Value = !TheOtherRolesPlugin.ShowRoleSummary.Value;
                    MapOptions.showRoleSummary = TheOtherRolesPlugin.ShowRoleSummary.Value; 
                    updateToggle(showRoleSummaryButton, "Role Summary: ", TheOtherRolesPlugin.ShowRoleSummary.Value);
                }
            }
        }
    }

    [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
	public static class HiddenTextPatch
	{
		private static void Postfix(TextBoxTMP __instance)
		{
			bool flag = TheOtherRolesPlugin.StreamerMode.Value && (__instance.name == "GameIdText" || __instance.name == "IpTextBox" || __instance.name == "PortTextBox");
			if (flag) __instance.outputText.text = new string('*', __instance.text.Length);
		}
	}
}
