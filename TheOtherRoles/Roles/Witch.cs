using System.Net;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using UnhollowerBaseLib;


namespace TheOtherRoles.Roles {
    public static class Witch {

        public static PlayerControl witch;
        public static PlayerControl shielded;

        public static Color color = new Color32(108, 70, 117, byte.MaxValue);

        public static bool usedShield;
        public static bool showShielded = true;

        public static Color shieldedColor = new Color32(108, 70, 117, byte.MaxValue);

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if(buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.ShieldButton.png", 115f);
            return buttonSprite;
        }

        public static void clearAndReload() {
            witch = null;
            usedShield = false;
            shielded = null;
        }
    }
}
