using System;
using HarmonyLib;
using Il2CppSystem.Collections;
using Rewired;
using Rewired.Data;
using UnityEngine;

namespace TheOtherRoles.Patches;

[HarmonyPatch(typeof(InputManager_Base), nameof(InputManager_Base.Awake))]
static class ControllerPatch
{
    private static void Prefix(InputManager_Base __instance)
    {
        __instance.userData.RegisterBind("HunterAdmin", "Hunter admin button", KeyboardKeyCode.G);
        __instance.userData.RegisterBind("UsePortal", "Use a portal", KeyboardKeyCode.H);
    }
    private static int RegisterBind(this UserData self, string name, string description, KeyboardKeyCode keycode, int elementIdentifierId = -1, int category = 0, InputActionType type = InputActionType.Button)
    {
        self.AddAction(category);
        var action = self.GetAction(self.actions.Count - 1)!;

        action.name = name;
        action.descriptiveName = description;
        action.categoryId = category;
        action.type = type;
        action.userAssignable = true;

        var a = new ActionElementMap();
        a._elementIdentifierId = elementIdentifierId;
        a._actionId = action.id;
        a._elementType = ControllerElementType.Button;
        a._axisContribution = Pole.Positive;
        a._keyboardKeyCode = keycode;
        a._modifierKey1 = ModifierKey.None;
        a._modifierKey2 = ModifierKey.None;
        a._modifierKey3 = ModifierKey.None;
        self.keyboardMaps[0].actionElementMaps.Add(a);
        self.joystickMaps[0].actionElementMaps.Add(a);
            
        return action.id;
    }
}
