using System;
using UnityEngine;
using System.Collections.Generic;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Objects {

    public class Portal {
        public static Portal firstPortal = null;
        public static Portal secondPortal = null;
        public static bool bothPlacedAndEnabled = false;
        // public static Sprite[] portalBgAnimationSprites = new Sprite[109];
        public static Sprite[] portalFgAnimationSprites = new Sprite[205];
        public static Sprite portalSprite;
        public static bool isTeleporting = false;
        public static float teleportDuration = 3.4166666667f; 

        public struct tpLogEntry {
            public byte playerId;
            public string name;
            public DateTime time;
            public tpLogEntry(byte playerId, string name, DateTime time) {
                this.playerId = playerId;
                this.time = time;
                this.name = name;
            }
        }

        public static List<tpLogEntry> teleportedPlayers;

         /*public static Sprite getBgAnimationSprite(int index) {
            if (portalBgAnimationSprites == null || portalBgAnimationSprites.Length == 0) return null;
            index = Mathf.Clamp(index, 0, portalBgAnimationSprites.Length - 1);
            if (portalBgAnimationSprites[index] == null)
                portalBgAnimationSprites[index] = (Helpers.loadSpriteFromResources($"TheOtherRoles.Resources.PortalAnimation.plattform.png", 115f));
            return portalBgAnimationSprites[index];
        }*/

        public static Sprite getFgAnimationSprite(int index) {
            if (portalFgAnimationSprites == null || portalFgAnimationSprites.Length == 0) return null;
            index = Mathf.Clamp(index, 0, portalFgAnimationSprites.Length - 1);
            if (portalFgAnimationSprites[index] == null)
                portalFgAnimationSprites[index] = (Helpers.loadSpriteFromResources($"TheOtherRoles.Resources.PortalAnimation.portal_{(index):000}.png", 115f));
            return portalFgAnimationSprites[index];
        }

        public static void startTeleport(byte playerId) {
            if (firstPortal == null || secondPortal == null) return;
            isTeleporting = true;
            
            // Generate log info
            PlayerControl playerControl = Helpers.playerById(playerId);
            bool flip = playerControl.MyRend.flipX; // use the original player control here, not the morhpTarget.
            firstPortal.animationFgRenderer.flipX = flip;
            secondPortal.animationFgRenderer.flipX = flip;
            if (Morphling.morphling != null && Morphling.morphTimer > 0) playerControl = Morphling.morphTarget;  // Will output info of morph-target instead
            string playerNameDisplay = Portalmaker.logOnlyHasColors ? "A player (" + (Helpers.isLighterColor(playerControl.Data.DefaultOutfit.ColorId) ? "L" : "D") + ")" : playerControl.Data.PlayerName;

            int colorId = playerControl.Data.DefaultOutfit.ColorId;

            if (Camouflager.camouflageTimer > 0) {
                playerNameDisplay = "A camouflaged player";
                colorId = 6;
            }
            teleportedPlayers.Add(new tpLogEntry(playerId, playerNameDisplay, DateTime.UtcNow));
            
            HudManager.Instance.StartCoroutine(Effects.Lerp(teleportDuration, new Action<float>((p) => {
                if (firstPortal != null && firstPortal.animationFgRenderer != null && secondPortal != null && secondPortal.animationFgRenderer != null) {
                    firstPortal.animationFgRenderer.sprite = getFgAnimationSprite((int)(p * portalFgAnimationSprites.Length));
                    secondPortal.animationFgRenderer.sprite = getFgAnimationSprite((int)(p * portalFgAnimationSprites.Length));
                    PlayerControl.SetPlayerMaterialColors(colorId, firstPortal.animationFgRenderer);
                    PlayerControl.SetPlayerMaterialColors(colorId, secondPortal.animationFgRenderer);
                    /*firstPortal.animationBgRenderer.sprite = getBgAnimationSprite((int)(p * portalFgAnimationSprites.Length));
                    secondPortal.animationBgRenderer.sprite = getBgAnimationSprite((int)(p * portalFgAnimationSprites.Length));*/
                    if (p == 1f) {
                        firstPortal.animationFgRenderer.sprite = null;
                        secondPortal.animationFgRenderer.sprite = null;
                        firstPortal.animationBgRenderer.sprite = null;
                        secondPortal.animationBgRenderer.sprite = null;
                        isTeleporting = false;
                    }
                }
            })));
        }

        public GameObject portalFgAnimationGameObject;
        public GameObject portalBgAnimationGameObject;
        public GameObject portalGameObject;
        private SpriteRenderer animationFgRenderer;
        private SpriteRenderer animationBgRenderer;
        private SpriteRenderer portalRenderer;

        public Portal(Vector2 p) {
            portalGameObject = new GameObject("Portal");
            Vector3 position = new Vector3(p.x, p.y, PlayerControl.LocalPlayer.transform.position.z + 1f);


            // Create the portal            
            portalGameObject.transform.position = position;
            portalRenderer = portalGameObject.AddComponent<SpriteRenderer>();
            animationBgRenderer = portalGameObject.AddComponent<SpriteRenderer>();
            portalRenderer.sprite = portalSprite;

            portalBgAnimationGameObject = new GameObject("PortalAnimationBG");
            portalBgAnimationGameObject.transform.position = position;
            animationBgRenderer = portalBgAnimationGameObject.AddComponent<SpriteRenderer>();

            Vector3 fgPosition = new Vector3(p.x, p.y, PlayerControl.LocalPlayer.transform.position.z - 0.1f);
            portalFgAnimationGameObject = new GameObject("PortalAnimationFG");
            portalFgAnimationGameObject.transform.position = fgPosition;
            animationFgRenderer = portalFgAnimationGameObject.AddComponent<SpriteRenderer>();
            animationFgRenderer.material = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;

            // Only render the inactive portals for the Portalmaker
            bool playerIsPortalmaker = PlayerControl.LocalPlayer == TheOtherRoles.Portalmaker.portalmaker;
            portalGameObject.SetActive(playerIsPortalmaker);
            portalFgAnimationGameObject.SetActive(true);
            portalBgAnimationGameObject.SetActive(true);

            if (firstPortal == null) firstPortal = this;
            else if (secondPortal == null) {
                secondPortal = this;
            }
        }

        public static bool locationNearEntry(Vector2 p) {
            if (!bothPlacedAndEnabled) return false;
            float maxDist = 0.25f;

            var dist1 = Vector2.Distance(p, firstPortal.portalGameObject.transform.position);
            var dist2 = Vector2.Distance(p, secondPortal.portalGameObject.transform.position);
            if (dist1 > maxDist && dist2 > maxDist) return false;
            return true;
        }

        public static Vector2 findExit(Vector2 p) {
            var dist1 = Vector2.Distance(p, firstPortal.portalGameObject.transform.position);
            var dist2 = Vector2.Distance(p, secondPortal.portalGameObject.transform.position);
            return dist1 < dist2 ? secondPortal.portalGameObject.transform.position : firstPortal.portalGameObject.transform.position;
        }

        public static Vector2 findEntry(Vector2 p) {
            var dist1 = Vector2.Distance(p, firstPortal.portalGameObject.transform.position);
            var dist2 = Vector2.Distance(p, secondPortal.portalGameObject.transform.position);
            return dist1 > dist2 ? secondPortal.portalGameObject.transform.position : firstPortal.portalGameObject.transform.position;
        }

        public static void meetingEndsUpdate() {
            // checkAndEnable
            if (secondPortal != null) {
                firstPortal.portalGameObject.SetActive(true);
                secondPortal.portalGameObject.SetActive(true);
                bothPlacedAndEnabled = true;
            }

            // reset teleported players
            teleportedPlayers = new List<tpLogEntry>();
        }

        private static void preloadSprites() {
            for (int i = 0; i < portalFgAnimationSprites.Length; i++) {
                /*getBgAnimationSprite(i);*/
                getFgAnimationSprite(i);
            }
            portalSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.PortalAnimation.plattform.png", 115f);
        }

        public static void clearPortals() {
            preloadSprites();  // Force preload of sprites to avoid lag
            bothPlacedAndEnabled = false;
            firstPortal = null;
            secondPortal = null;
            isTeleporting = false;
            teleportedPlayers = new List<tpLogEntry>();
        }

    }

}