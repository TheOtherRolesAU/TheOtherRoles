using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using TheOtherRoles.Modules.CustomHats.Extensions;
using HarmonyLib;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Modules.CustomHats.Patches;

[HarmonyPatch]
internal static class HatsTabPatches
{
    private static TextMeshPro textTemplate;


    [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.OnEnable))]
    [HarmonyPrefix]
    private static bool OnEnablePrefix(HatsTab __instance)
    {
        for (var i = 0; i < __instance.scroller.Inner.childCount; i++)
        {
            Object.Destroy(__instance.scroller.Inner.GetChild(i).gameObject);
        }

        __instance.ColorChips = new Il2CppSystem.Collections.Generic.List<ColorChip>();
        var unlockedHats = DestroyableSingleton<HatManager>.Instance.GetUnlockedHats();
        var packages = new Dictionary<string, List<Tuple<HatData, HatExtension>>>();

        foreach (var hatBehaviour in unlockedHats)
        {
            var ext = hatBehaviour.GetHatExtension();
            if (ext != null)
            {
                if (!packages.ContainsKey(ext.Package))
                {
                    packages[ext.Package] = new List<Tuple<HatData, HatExtension>>();
                }
                packages[ext.Package].Add(new Tuple<HatData, HatExtension>(hatBehaviour, ext));
            }
            else
            {
                if (!packages.ContainsKey(CustomHatManager.InnerslothPackageName))
                {
                    packages[CustomHatManager.InnerslothPackageName] = new List<Tuple<HatData, HatExtension>>();
                }
                packages[CustomHatManager.InnerslothPackageName].Add(new Tuple<HatData, HatExtension>(hatBehaviour, null));
            }
        }

        var yOffset = __instance.YStart;
        textTemplate = GameObject.Find("HatsGroup").transform.FindChild("Text").GetComponent<TextMeshPro>();

        var orderedKeys = packages.Keys.OrderBy(x =>
            x switch
            {
                CustomHatManager.InnerslothPackageName => 1000,
                CustomHatManager.DeveloperPackageName => 0,
                _ => 500
            });
        foreach (var key in orderedKeys)
        {
            var value = packages[key];
            yOffset = CreateHatPackage(value, key, yOffset, __instance);
        }
        
        __instance.scroller.ContentYBounds.max = -(yOffset + 4.1f);
        return false;
    }

    private static float CreateHatPackage(List<Tuple<HatData, HatExtension>> hats, string packageName, float yStart,
        HatsTab hatsTab)
    {
        var isDefaultPackage = CustomHatManager.InnerslothPackageName == packageName;
        if (!isDefaultPackage)
        {
            hats = hats.OrderBy(x => x.Item1.name).ToList();
        }

        var offset = yStart;
        if (textTemplate != null)
        {
            var title = Object.Instantiate(textTemplate, hatsTab.scroller.Inner);
            title.transform.localPosition = new Vector3(2.25f, yStart, -1f);
            title.transform.localScale = Vector3.one * 1.5f;
            title.fontSize *= 0.5f;
            title.enableAutoSizing = false;
            hatsTab.StartCoroutine(Effects.Lerp(0.1f, new Action<float>(p => { title.SetText(packageName); })));
            offset -= 0.8f * hatsTab.YOffset;
        }

        for (var i = 0; i < hats.Count; i++)
        {
            var (hat, ext) = hats[i];
            var xPos = hatsTab.XRange.Lerp(i % hatsTab.NumPerRow / (hatsTab.NumPerRow - 1f));
            var yPos = offset - i / hatsTab.NumPerRow * (isDefaultPackage ? 1f : 1.5f) * hatsTab.YOffset;
            var colorChip = Object.Instantiate(hatsTab.ColorTabPrefab, hatsTab.scroller.Inner);
            if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
            {
                colorChip.Button.OnMouseOver.AddListener((Action)(() => hatsTab.SelectHat(hat)));
                colorChip.Button.OnMouseOut.AddListener((Action)(() => hatsTab.SelectHat(DestroyableSingleton<HatManager>.Instance.GetHatById(DataManager.Player.Customization.Hat))));
                colorChip.Button.OnClick.AddListener((Action)hatsTab.ClickEquip);
            }
            else
            {
                colorChip.Button.OnClick.AddListener((Action)(() => hatsTab.SelectHat(hat)));
            }
            colorChip.Button.ClickMask = hatsTab.scroller.Hitbox;
            colorChip.Inner.SetMaskType(PlayerMaterial.MaskType.SimpleUI);
            hatsTab.UpdateMaterials(colorChip.Inner.FrontLayer, hat);
            var background = colorChip.transform.FindChild("Background");
            var foreground = colorChip.transform.FindChild("ForeGround");

            if (ext != null)
            {
                if (background != null) {
                    background.localPosition = Vector3.down * 0.243f;
                    background.localScale = new Vector3(background.localScale.x, 0.8f, background.localScale.y);
                }
                if (foreground != null) {
                    foreground.localPosition = Vector3.down * 0.243f;
                }
                
                if (textTemplate != null) {
                    var description = Object.Instantiate(textTemplate, colorChip.transform);
                    description.transform.localPosition = new Vector3(0f, -0.65f, -1f);
                    description.alignment = TextAlignmentOptions.Center;
                    description.transform.localScale = Vector3.one * 0.65f;
                    hatsTab.StartCoroutine(Effects.Lerp(0.1f, new Action<float>(p => { description.SetText($"{hat.name}\nby {ext.Author}"); })));
                }
            }
            
            colorChip.transform.localPosition = new Vector3(xPos, yPos, -1f);
            colorChip.Inner.SetHat(hat, hatsTab.HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
            colorChip.Inner.transform.localPosition = hat.ChipOffset;
            colorChip.Tag = hat;
            colorChip.SelectionHighlight.gameObject.SetActive(false);
            hatsTab.ColorChips.Add(colorChip);
        }

        return offset - (hats.Count - 1) / hatsTab.NumPerRow * (isDefaultPackage ? 1f : 1.5f) * hatsTab.YOffset -
               1.75f;
    }
}