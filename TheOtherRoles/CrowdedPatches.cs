using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;

namespace TheOtherRoles {
    static class CreateGameOptionsPatches
    {
        [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.Start))]
        public static class CreateOptionsPickerStartPatch {
            public static List<SpriteRenderer> additionalButtons = new List<SpriteRenderer>();

            public static void Postfix(CreateOptionsPicker __instance) {
                List<SpriteRenderer> maxPlayerButtons = __instance.MaxPlayerButtons.ToList();
                additionalButtons = new List<SpriteRenderer>();
                
                for (int i = 1; i < 6; i++) {
                    SpriteRenderer nextButton = Object.Instantiate(maxPlayerButtons.Last(), maxPlayerButtons.Last().transform.parent);
                    additionalButtons.Add(nextButton);
                    nextButton.enabled = false;
                    nextButton.gameObject.name = "1" + i;
                    TextRenderer text = nextButton.gameObject.GetComponentInChildren<TextRenderer>();
                    text.render = null;
                    nextButton.GetComponentInChildren<MeshFilter>().mesh = null;
                    text.Text = "1" + i;
                    text.RefreshMesh();
                
                    nextButton.transform.position = nextButton.transform.position + new Vector3(i * (maxPlayerButtons[1].transform.position.x - maxPlayerButtons[0].transform.position.x), 0, 0);
                    var passiveButton = nextButton.GetComponent<PassiveButton>();
                    passiveButton.OnClick.RemoveAllListeners();
                    void onClick() {
                        foreach (SpriteRenderer renderer in additionalButtons) renderer.enabled = false;
                        nextButton.enabled = true;

                        byte value = byte.Parse(nextButton.name);
                        var targetOptions = __instance.GetTargetOptions();
                        if (value <= targetOptions.PCBBPGNJPJN) {
                            targetOptions.PCBBPGNJPJN = value - 1;
                            __instance.ABNOBIOJDEH(targetOptions.PCBBPGNJPJN);
                        }
                        __instance.SetMaxPlayersButtons(value);
                    } 
                    passiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)onClick);
                }
            }
        }
    }
}