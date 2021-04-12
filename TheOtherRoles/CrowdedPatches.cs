using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Hazel;

using Palette = GLNPIJPGGNJ;
using SaveManager = ALOOOIHKCAC;
using GameOptionsData = IGDMNKLDEPI;

namespace TheOtherRoles {
    [HarmonyPatch]
    static class CreateOptionsPickerPatch {
        public static List<SpriteRenderer> additionalButtons = new List<SpriteRenderer>();

        [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.Start))]
        public static class CreateOptionsPickerStartPatch {
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
                    text.Color = Helpers.isCustomServer() ? Color.white : Palette.JMELLHINKGM;

                    nextButton.transform.position = nextButton.transform.position + new Vector3(i * (maxPlayerButtons[1].transform.position.x - maxPlayerButtons[0].transform.position.x), 0, 0);
                    var passiveButton = nextButton.GetComponent<PassiveButton>();
                    passiveButton.OnClick.RemoveAllListeners();

                    void onClick() {
                        if (!Helpers.isCustomServer()) return;

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

        [HarmonyPatch(typeof(HostGameButton), nameof(HostGameButton.OnClick))]
        class HostGameButtonOnClickPatch {
            public static void Postfix() {
                bool isCustom = Helpers.isCustomServer();
                foreach (SpriteRenderer renderer in additionalButtons) {
                    if (renderer != null && renderer.gameObject != null) {
                        renderer.enabled = false;
                        renderer.gameObject.GetComponentInChildren<TextRenderer>().Color = isCustom ? Color.white : Palette.JMELLHINKGM;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.GJJLFHMDLGA), MethodType.Getter)]
    public static class SaveManagerGetHostOptions
    {
        public static bool Prefix(out GameOptionsData __result)
        {
            SaveManager.KEEEOKAJAFI ??= SaveManager.OODOOJLHAEC("gameHostOptions");

            SaveManager.KEEEOKAJAFI.PCBBPGNJPJN = Mathf.Clamp(SaveManager.KEEEOKAJAFI.PCBBPGNJPJN, 1, SaveManager.KEEEOKAJAFI.BKOLCIHDBPK - 1);
            SaveManager.KEEEOKAJAFI.MLLMFMOMIAC = Mathf.Clamp(SaveManager.KEEEOKAJAFI.MLLMFMOMIAC, 0, 2);

            __result = SaveManager.KEEEOKAJAFI;
            return false;
        }
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.GetAvailableId))]
    public static class GameDataGetAvailableIdPatch {
        public static bool Prefix(ref GameData __instance, out sbyte __result) {
            for (sbyte i = 0; i <= 15; i++)
                if (!__instance.AllPlayers.ToArray().Any(p => p.GMBAIPNOKLP == i)) {
                    __result = i;
                    return false;
                }
            __result = -1;
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckColor))]
    public static class PlayerControlCmdCheckColorPatch {
        public static bool Prefix(PlayerControl __instance, byte JAKOFFAIMMM) {
            if (!Helpers.isCustomServer()) return true;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetUncheckedColor, Hazel.SendOption.Reliable, -1);
            writer.Write(JAKOFFAIMMM);
            writer.Write(__instance.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.setUncheckedColor(JAKOFFAIMMM, __instance.PlayerId);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.UpdateAvailableColors))]
    public static class PlayerTabUpdateAvailableColorsPatch {
        public static bool Prefix(PlayerTab __instance) {
            if (!Helpers.isCustomServer()) return true;

            PlayerControl.SetPlayerMaterialColors(PlayerControl.LocalPlayer.IDOFAMCIJKE.JFHFMIKFHGG, __instance.DemoImage);
            for (int i = 0; i < Palette.CALCLMEEPGL.Length; i++)
                __instance.BENAMDPPABB.Add(i);
            return false;
        }
    }

    [HarmonyPatch(typeof(SecurityLogger), nameof(SecurityLogger.Awake))]
    public static class SecurityLoggerAwakePatch
    {
        public static void Postfix(SecurityLogger __instance) {
            __instance.AIECLALEDPN = new float[15];
        }
    }

    [HarmonyPatch(typeof(KeyMinigame),nameof(KeyMinigame.Start))]
    public static class KeyMinigameStartPatch
    {
        public static bool Prefix(KeyMinigame __instance)
        {
            __instance.ANIFEEMBMDA = (PlayerControl.LocalPlayer != null) ? PlayerControl.LocalPlayer.PlayerId % 10 : 0;
            __instance.Slots[__instance.ANIFEEMBMDA].Image.sprite = __instance.Slots[__instance.ANIFEEMBMDA].Highlit;
            return false;
        }
    }
}