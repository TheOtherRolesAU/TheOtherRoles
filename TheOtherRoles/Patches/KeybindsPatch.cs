using System;
using System.Collections.Generic;
using HarmonyLib;
using Rewired;

namespace TheOtherRoles.Patches;

internal class RewiredKeybindInfo
{
    internal int Id;
    
    internal string Name;
    internal string Description;
    internal int Category;
    internal InputActionType Type;
}

// Code from https://github.com/sinai-dev/Outward-SideLoader/blob/8f848532956eb5af31daf1d5293df31348d664f8/src/Managers/CustomKeybindings.cs
// Credits from original file:
// - Stian for the original version
// - Lasyan3 for helping port it over to BepInEx
// - johnathan-clause for logic fixes and improvements (https://github.com/johnathan-clause)

[HarmonyPatch(typeof(InputManager_Base), nameof(InputManager_Base.Awake))]
internal static class KeybindsPatch
{
    private static List<RewiredKeybindInfo> _keybindsToRegister = new List<RewiredKeybindInfo>()
    {
        new() 
        {
            Name = "ZoomIn",
            Description = "Zoom",
            Category = 0,
            Type = InputActionType.Button
        },
        new() 
        {
            Name = "HunterKill",
            Description = "Kill (hunter)",
            Category = 0,
            Type = InputActionType.Button
        },
        new() 
        {
            Name = "HunterArrow",
            Description = "Arrow (hunter)",
            Category = 0,
            Type = InputActionType.Button
        }
    };
    
    private static void Postfix(InputManager_Base __instance)
    {
        var userData = __instance.userData;
        var length = userData.actions.Count;
        
        foreach (var keybind in _keybindsToRegister)
        {
            userData.AddAction(keybind.Category);
            var action = userData.GetAction(length)!;

            action.name = keybind.Name;
            action.descriptiveName = keybind.Description;
            action.categoryId = keybind.Category;
            action.type = keybind.Type;
            action.userAssignable = true;

            keybind.Id = action.id;
            
            length++;
        }
    }
}

// [HarmonyPatch(typeof(ControlMapping), "InitMappings")]
// internal static class DefaultMappingPatch
// {
//     
// }
