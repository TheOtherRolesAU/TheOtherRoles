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
    public static class Ninja {
        public static PlayerControl ninja;
        public static Color color = Palette.ImpostorRed;
        public static float cooldown = 30f;
        public static float duration = 10f;
        public static float ninjaTimer = 0f;
        public static int killDistance = 0;
        public static bool moveToPlayer = true;

        private static Sprite ninjaSprite;
        public static Sprite getButtonSprite() {
            if(ninjaSprite) return ninjaSprite;
            ninjaSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.NinjaButton.png", 115f);
            return ninjaSprite;
        }

        public static void resetNinja() {
            ninjaTimer = 0f;
            if(ninja == null) return;
            PlayerControl.GameOptions.KillDistance = 0;
            moveToPlayer = true;
        }

        // Call in TheOtherRoles.cs
        public static void clearAndReload() {
            resetNinja();
            ninja = null;
            ninjaTimer = 0f;
            cooldown = CustomOptionHolder.ninjaCooldown.getFloat();
            duration = CustomOptionHolder.ninjaDuration.getFloat();
            killDistance = Convert.ToInt32(CustomOptionHolder.ninjaKillDistance.getFloat()); 
        }

        // Call in UpdatePatch.cs
        public static void ninjaAction() {
            float oldNinjaTimer = Ninja.ninjaTimer;

            Ninja.ninjaTimer -= Time.deltaTime;

            // Set Ninja effect
            if(Ninja.ninjaTimer > 0f) {
                PlayerControl.GameOptions.KillDistance = killDistance;
                moveToPlayer = false;
            }

            if(oldNinjaTimer > 0f && Ninja.ninjaTimer <= 0f) {
                Ninja.resetNinja();
            }
        }

        // Call in RPC.cs
        public static void ninjaExpand() {
            if(Ninja.ninja == null) return;
            Ninja.ninjaTimer = Ninja.duration;
        }
    }
}
