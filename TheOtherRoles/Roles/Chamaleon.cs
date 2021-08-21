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

namespace TheOtherRoles.Roles {
    public static class Chamaleon {
        public static PlayerControl chamaleon;
        public static Color color = Palette.ImpostorRed;

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static float chamaleonTimer = 0f;
        public static float rootTime = 10f;

        private static Sprite chamaleonSprite;
        public static Sprite getButtonSprite() {
            if(chamaleonSprite) return chamaleonSprite;
            chamaleonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.HideButton.png", 115f);
            return chamaleonSprite;
        }

        public static void resetChamaleon() {
            chamaleonTimer = 0f;
            if(chamaleon == null) return;
            chamaleon.myRend.enabled = true;
            chamaleon.SetName(chamaleon.Data.PlayerName);
            chamaleon.SetHat(chamaleon.Data.HatId, (int)chamaleon.Data.ColorId);
            Helpers.setSkinWithAnim(chamaleon.MyPhysics, chamaleon.Data.SkinId);
            chamaleon.SetPet(chamaleon.Data.PetId);
            chamaleon.CurrentPet.Visible = chamaleon.Visible;
            chamaleon.SetColor(chamaleon.Data.ColorId);
        }

        // Call in TheOtherRoles.cs
        public static void clearAndReload() {
            resetChamaleon();
            chamaleon = null;
            chamaleonTimer = 0f;
            cooldown = CustomOptionHolder.chamaleonCooldown.getFloat();
            duration = CustomOptionHolder.chamaleonDuration.getFloat();
            rootTime = CustomOptionHolder.chamaleonRootTime.getFloat();
        }

        // Call in UpdatePatch.cs
        public static void chamaleonAction() {
            float oldChamaleonTimer = Chamaleon.chamaleonTimer;

            Chamaleon.chamaleonTimer -= Time.deltaTime;

            // Set chamaleon look
            if(Chamaleon.chamaleonTimer > 0f) {
                Chamaleon.chamaleon.nameText.text = "";
                Chamaleon.chamaleon.myRend.enabled = false;
                Chamaleon.chamaleon.HatRenderer.SetHat(0, 0);
                Helpers.setSkinWithAnim(Chamaleon.chamaleon.MyPhysics, 0);
                bool spawnPet = false;
                if(Chamaleon.chamaleon.CurrentPet == null) spawnPet = true;
                else if(Chamaleon.chamaleon.CurrentPet.ProdId != DestroyableSingleton<HatManager>.Instance.AllPets[0].ProdId) {
                    UnityEngine.Object.Destroy(Chamaleon.chamaleon.CurrentPet.gameObject);
                    spawnPet = true;
                }

                if(spawnPet) {
                    Chamaleon.chamaleon.CurrentPet = UnityEngine.Object.Instantiate<PetBehaviour>(DestroyableSingleton<HatManager>.Instance.AllPets[0]);
                    Chamaleon.chamaleon.CurrentPet.transform.position = Chamaleon.chamaleon.transform.position;
                    Chamaleon.chamaleon.CurrentPet.Source = Chamaleon.chamaleon;
                }
            }

            if(oldChamaleonTimer > 0f && Chamaleon.chamaleonTimer <= 0f) {
                Chamaleon.resetChamaleon();
            }
        }

        // Call in RPC.cs
        public static void chamaleonHide() {
            if(Chamaleon.chamaleon == null) return;
            Chamaleon.chamaleonTimer = Chamaleon.duration;
        }
    }
}
