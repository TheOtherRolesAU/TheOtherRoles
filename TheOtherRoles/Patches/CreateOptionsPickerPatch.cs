using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.UI.Button;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(CreateOptionsPicker))]
    class CreateOptionsPickerPatch {
        private static List<SpriteRenderer> renderers = new List<SpriteRenderer>();

        [HarmonyPatch(typeof(CreateOptionsPicker), "Awake")]
        [HarmonyPrefix]
        public static void Prefix(CreateOptionsPicker __instance) {
            if (__instance.mode == SettingsMode.Host) {
                renderers = new List<SpriteRenderer>();

                // space for max 5 buttons
                addGamemodeButton(__instance, "Classic", "TheOtherRoles.Resources.TabIconClassicMode.png", CustomGamemodes.Classic);
                addGamemodeButton(__instance, "Guesser", "TheOtherRoles.Resources.TabIconGuesserMode.png", CustomGamemodes.Guesser);
                addGamemodeButton(__instance, "Hide 'N Seek", "TheOtherRoles.Resources.TabIconHideNSeekMode.png", CustomGamemodes.HideNSeek);

                switch (MapOptions.gameMode) {
                    case CustomGamemodes.Classic: renderers.FindLast(x => x.name == "Classic").color = Color.white; break;
                    case CustomGamemodes.Guesser: renderers.FindLast(x => x.name == "Guesser").color = Color.white; break;
                    case CustomGamemodes.HideNSeek: renderers.FindLast(x => x.name == "Hide 'N Seek").color = Color.white; break;
                }
            }
            else {
                //
            }
        }

        private static void addGamemodeButton(CreateOptionsPicker __instance, string name, string spritePath, CustomGamemodes gamemode) {
            Vector3 position1 = __instance.MapButtons[3].transform.position;
            Vector3 position2 = __instance.transform.Find("Max Players").position;
            float p = -5.8f + (renderers.Count * 1.4f);

            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.MapButtons[2].gameObject, __instance.MapButtons[2].transform.parent);
            gameObject.name = name;
            gameObject.transform.position = new Vector3(position1.x + p, position2.y - 0.6f, position1.z);
            SpriteRenderer component = gameObject.transform.Find("MapIcon2").GetComponent<SpriteRenderer>();
            component.gameObject.name = "gm" + name;
            component.sprite = Helpers.loadSpriteFromResources(spritePath, 150f);
            PassiveButton passiveButton = gameObject.GetComponent<PassiveButton>();
            SpriteRenderer buttonSprite = gameObject.GetComponent<SpriteRenderer>();
            buttonSprite.color *= 0;

            renderers.Add(buttonSprite);

            passiveButton.OnClick = new ButtonClickedEvent();
            passiveButton.OnClick.AddListener((System.Action)(() => setListener(buttonSprite, gamemode)));
        }

        private static void setListener(SpriteRenderer renderer, CustomGamemodes gameMode) {
            MapOptions.gameMode = gameMode;
            foreach (SpriteRenderer r in renderers) r.color *= 0;
            renderer.color = Color.white;
        }
    }
}
