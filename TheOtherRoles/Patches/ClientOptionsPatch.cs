using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    public class OptionsMenuBehaviourStartPatch
    {
        private const float XOffset = 1.75f;
        private const float YOffset = -0.5f;
        private static Vector3? origin;
        private static ToggleButtonBehaviour streamerModeButton;
        private static ToggleButtonBehaviour ghostsSeeTasksButton;
        private static ToggleButtonBehaviour ghostsSeeRolesButton;
        private static ToggleButtonBehaviour ghostsSeeVotesButton;
        private static ToggleButtonBehaviour showRoleSummaryButton;

        private static void UpdateToggle(ToggleButtonBehaviour button, string text, bool on)
        {
            if (button == null || button.gameObject == null) return;

            var color = on ? new Color(0f, 1f, 0.16470589f, 1f) : Color.white;
            button.Background.color = color;
            button.Text.text = $"{text}{(on ? "On" : "Off")}";
            if (button.Rollover) button.Rollover.ChangeOutColor(color);
        }

        private static ToggleButtonBehaviour CreateCustomToggle(string text, bool on, Vector3 offset,
            UnityAction onClick, OptionsMenuBehaviour __instance)
        {
            if (__instance.CensorChatButton == null) return null;
            var button = Object.Instantiate(__instance.CensorChatButton,
                __instance.CensorChatButton.transform.parent);
            button.transform.localPosition = (origin ?? Vector3.zero) + offset;
            var passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener(onClick);
            UpdateToggle(button, text, on);

            return button;
        }

        public static void Postfix(OptionsMenuBehaviour __instance)
        {
            if (__instance.CensorChatButton != null)
            {
                var transform = __instance.CensorChatButton.transform;
                origin ??= transform.localPosition + Vector3.up * 0.25f;
                transform.localPosition = origin.Value + Vector3.left * XOffset;
                transform.localScale = Vector3.one * 2f / 3f;
            }

            if (streamerModeButton == null || streamerModeButton.gameObject == null)
            {
                streamerModeButton = CreateCustomToggle("Streamer Mode: ", TheOtherRolesPlugin.StreamerMode.Value,
                    Vector3.zero, (UnityAction) StreamerModeToggle, __instance);

                static void StreamerModeToggle()
                {
                    TheOtherRolesPlugin.StreamerMode.Value = !TheOtherRolesPlugin.StreamerMode.Value;
                    UpdateToggle(streamerModeButton, "Streamer Mode: ", TheOtherRolesPlugin.StreamerMode.Value);
                }
            }

            if (ghostsSeeTasksButton == null || ghostsSeeTasksButton.gameObject == null)
            {
                ghostsSeeTasksButton = CreateCustomToggle("Ghosts See Remaining Tasks: ",
                    TheOtherRolesPlugin.GhostsSeeTasks.Value, Vector3.right * XOffset,
                    (UnityAction) GhostsSeeTaskToggle, __instance);

                static void GhostsSeeTaskToggle()
                {
                    TheOtherRolesPlugin.GhostsSeeTasks.Value = !TheOtherRolesPlugin.GhostsSeeTasks.Value;
                    MapOptions.ghostsSeeTasks = TheOtherRolesPlugin.GhostsSeeTasks.Value;
                    UpdateToggle(ghostsSeeTasksButton, "Ghosts See Remaining Tasks: ",
                        TheOtherRolesPlugin.GhostsSeeTasks.Value);
                }
            }

            if (ghostsSeeRolesButton == null || ghostsSeeRolesButton.gameObject == null)
            {
                ghostsSeeRolesButton = CreateCustomToggle("Ghosts See Roles: ",
                    TheOtherRolesPlugin.GhostsSeeRoles.Value, new Vector2(-XOffset, YOffset),
                    (UnityAction) GhostsSeeRolesToggle, __instance);

                static void GhostsSeeRolesToggle()
                {
                    TheOtherRolesPlugin.GhostsSeeRoles.Value = !TheOtherRolesPlugin.GhostsSeeRoles.Value;
                    MapOptions.ghostsSeeRoles = TheOtherRolesPlugin.GhostsSeeRoles.Value;
                    UpdateToggle(ghostsSeeRolesButton, "Ghosts See Roles: ", TheOtherRolesPlugin.GhostsSeeRoles.Value);
                }
            }

            if (ghostsSeeVotesButton == null || ghostsSeeVotesButton.gameObject == null)
            {
                ghostsSeeVotesButton = CreateCustomToggle("Ghosts See Votes: ",
                    TheOtherRolesPlugin.GhostsSeeVotes.Value, new Vector2(0, YOffset),
                    (UnityAction) GhostsSeeVotesToggle, __instance);

                static void GhostsSeeVotesToggle()
                {
                    TheOtherRolesPlugin.GhostsSeeVotes.Value = !TheOtherRolesPlugin.GhostsSeeVotes.Value;
                    MapOptions.ghostsSeeVotes = TheOtherRolesPlugin.GhostsSeeVotes.Value;
                    UpdateToggle(ghostsSeeVotesButton, "Ghosts See Votes: ", TheOtherRolesPlugin.GhostsSeeVotes.Value);
                }
            }

            if (showRoleSummaryButton != null && showRoleSummaryButton.gameObject != null) return;
            showRoleSummaryButton = CreateCustomToggle("Role Summary: ", TheOtherRolesPlugin.ShowRoleSummary.Value,
                new Vector2(XOffset, YOffset), (UnityAction) ShowRoleSummaryToggle, __instance);

            static void ShowRoleSummaryToggle()
            {
                TheOtherRolesPlugin.ShowRoleSummary.Value = !TheOtherRolesPlugin.ShowRoleSummary.Value;
                MapOptions.showRoleSummary = TheOtherRolesPlugin.ShowRoleSummary.Value;
                UpdateToggle(showRoleSummaryButton, "Role Summary: ", TheOtherRolesPlugin.ShowRoleSummary.Value);
            }
        }
    }

    [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
    public static class HiddenTextPatch
    {
        private static void Postfix(TextBoxTMP __instance)
        {
            if (TheOtherRolesPlugin.StreamerMode.Value &&
                __instance.name is "GameIdText" or "IpTextBox" or "PortTextBox")
                __instance.outputText.text = new string('*', __instance.text.Length);
        }
    }
}