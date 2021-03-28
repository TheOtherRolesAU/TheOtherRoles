using HarmonyLib;
using System;
using System.IO;
using System.Net.Http;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;
using Reactor.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Essentials.Options;
using Essentials;

namespace TheOtherRoles
{
    [HarmonyPriority(Priority.Low)] 
    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    class GameOptionsMenuUpdatePatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            __instance.GetComponentInParent<Scroller>().YBounds.max = -0.5F + __instance.Children.Length * 0.5F;
        }
    }

    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class GameOptionsNextPagePatch
    {
        private static readonly System.Random random = new System.Random((int)DateTime.Now.Ticks);
        private static List<PlayerControl> bots = new List<PlayerControl>();

        public static void Postfix(KeyboardJoystick __instance)
        {
            if(Input.GetKeyDown(KeyCode.Tab)) {
                TheOtherRolesPlugin.optionsPage = (TheOtherRolesPlugin.optionsPage + 1) % 3;
            }
        }
    }

    [HarmonyPatch(typeof(GameOptionsData), "NHJLMAAHKJF")]
    class GameOptionsDataPatch
    {
        private static void Postfix(ref string __result)
        {
            StringBuilder stringBuilder = new StringBuilder(__result);
            foreach (CustomOption option in TheOtherRolesPlugin.options) {
                stringBuilder.AppendLine(string.Format("{0}[]:   {1}", option.Name, option));   
            }
            var hudString = stringBuilder.ToString();

            int defaultSettingsLines = 19;
            int roleSettingsLines = 19 + 24;
            int end1 = hudString.TakeWhile(c => (defaultSettingsLines -= (c == '\n' ? 1 : 0)) > 0).Count();
            int end2 = hudString.TakeWhile(c => (roleSettingsLines -= (c == '\n' ? 1 : 0)) > 0).Count();
            int counter = TheOtherRolesPlugin.optionsPage;
            if (counter == 0) {
                hudString = hudString.Substring(0, end1) + "\n";   
            } else if (counter == 1) {
                hudString = hudString.Substring(end1 + 1, end2 - end1);
                // Temporary fix, should add a new CustomOption for spaces
                int gap = 1;
                int index = hudString.TakeWhile(c => (gap -= (c == '\n' ? 1 : 0)) > 0).Count();
                hudString = hudString.Insert(index, "\n");
                gap = 4;
                index = hudString.TakeWhile(c => (gap -= (c == '\n' ? 1 : 0)) > 0).Count();
                hudString = hudString.Insert(index, "\n");
                gap = 9;
                index = hudString.TakeWhile(c => (gap -= (c == '\n' ? 1 : 0)) > 0).Count();
                hudString = hudString.Insert(index + 1, "\n");
                gap = 13;
                index = hudString.TakeWhile(c => (gap -= (c == '\n' ? 1 : 0)) > 0).Count();
                hudString = hudString.Insert(index + 1, "\n");
            } else if (counter == 2) {
                hudString = hudString.Substring(end2 + 1);
            }
            hudString += "\n Press tab for more...\n\n\n";
            __result = hudString;
        }
    }

    [HarmonyPatch(typeof(GameSettingMenu), "OnEnable")]
    class GameSettingMenuPatch {
        public static void Prefix(GameSettingMenu __instance) {
            __instance.HideForOnline = new Transform[]{};
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    public class GameOptionsMenuPatchUpdate
    {
        public static int previousPreset = -1;
        public static void Postfix(GameOptionsMenu __instance)
        {
            int newPreset = TheOtherRolesPlugin.presetSelection.GetValue();
            
            if (newPreset != previousPreset && AmongUsClient.Instance && PlayerControl.LocalPlayer && AmongUsClient.Instance.AmHost) {
                previousPreset = newPreset;
                System.Console.WriteLine("here");

                foreach (CustomOption option in TheOtherRolesPlugin.options) {
                    int outInt = 0;
                    bool isLastCharNumeric = int.TryParse(option.PluginID[option.PluginID.Length - 1].ToString(), out outInt);
                    if (isLastCharNumeric)
                        option.PluginID = option.PluginID.Remove(option.PluginID.Length - 1);
                    option.PluginID += newPreset.ToString();

                    if (option is CustomStringOption str) {
                        if (str != TheOtherRolesPlugin.presetSelection) {
                            str.ConfigEntry = option.SaveValue ? EssentialsPlugin.Instance.Config.Bind(option.PluginID, option.ConfigID, str.GetDefaultValue()) : null;
                            option.SetValue(str.ConfigEntry == null ? str.GetDefaultValue() : str.ConfigEntry.Value);
                        }
                    } else if (option is CustomToggleOption tgl) {
                        tgl.ConfigEntry = option.SaveValue ? EssentialsPlugin.Instance.Config.Bind(option.PluginID, option.ConfigID, tgl.GetDefaultValue()) : null;
                        option.SetValue(tgl.ConfigEntry == null ? tgl.GetDefaultValue() : tgl.ConfigEntry.Value);
                    } else if (option is CustomNumberOption nmb) {
                        nmb.ConfigEntry = option.SaveValue ? EssentialsPlugin.Instance.Config.Bind(option.PluginID, option.ConfigID, nmb.GetDefaultValue()) : null;
                        option.SetValue(nmb.ConfigEntry == null ? nmb.GetDefaultValue() : nmb.ConfigEntry.Value);
                    }
                }

            }
        }
    }
}