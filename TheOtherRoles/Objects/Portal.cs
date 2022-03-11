using System;
using UnityEngine;
using System.Collections.Generic;

namespace TheOtherRoles.Objects {

    public class Portal {
        public static Portal firstPortal = null;
        public static Portal secondPortal = null;
        public static bool bothPlacedAndEnabled = false;
        public static Sprite[] portalAnimationSprites = new Sprite[81];
        public static Sprite portalSprite;
        public static bool isTeleporting = false;

        public static List<byte> teleportedPlayers;

        public static Sprite getBoxAnimationSprite(int index) {
            if (portalAnimationSprites == null || portalAnimationSprites.Length == 0) return null;
            index = Mathf.Clamp(index, 0, portalAnimationSprites.Length - 1);
            if (portalAnimationSprites[index] == null)
                portalAnimationSprites[index] = (Helpers.loadSpriteFromResources($"TheOtherRoles.Resources.PortalAnimation.portal_animation_00{(index+1):00}.png", 225f));
            return portalAnimationSprites[index];
        }

        public static void startTeleport(byte playerId) {
            if (firstPortal == null || secondPortal == null) return;
            isTeleporting = true;
            teleportedPlayers.Add(playerId);
            HudManager.Instance.StartCoroutine(Effects.Lerp(2.5f, new Action<float>((p) => {
                if (firstPortal != null && firstPortal.animationRenderer != null) {
                    firstPortal.animationRenderer.sprite = getBoxAnimationSprite((int)(p * portalAnimationSprites.Length));
                    if (p == 1f) firstPortal.animationRenderer.sprite = null;
                }
            })));
            HudManager.Instance.StartCoroutine(Effects.Lerp(2.5f, new Action<float>((p) => {
                if (secondPortal != null && secondPortal.animationRenderer != null) {
                    secondPortal.animationRenderer.sprite = getBoxAnimationSprite((int)(p * portalAnimationSprites.Length));
                    if (p == 1f) {
                        secondPortal.animationRenderer.sprite = null;
                        isTeleporting = false;
                    }
                }
            })));
        }

        public GameObject portalAnimationGameObject;
        public GameObject portalGameObject;
        private SpriteRenderer animationRenderer;
        private SpriteRenderer portalRenderer;

        public Portal(Vector2 p) {
            portalGameObject = new GameObject("Portal");
            Vector3 position = new Vector3(p.x, p.y, PlayerControl.LocalPlayer.transform.position.z + 1f);

            // Create the portal            
            portalGameObject.transform.position = position;
            portalRenderer = portalGameObject.AddComponent<SpriteRenderer>();
            portalRenderer.sprite = portalSprite;

            portalAnimationGameObject = new GameObject("PortalAnimation");
            position.z -= 1.1f; // infront of the player;
            portalAnimationGameObject.transform.position = position;
            animationRenderer = portalAnimationGameObject.AddComponent<SpriteRenderer>();
            animationRenderer.sprite = null;


            // Only render the inactive portals for the Portalmaker
            bool playerIsPortalmaker = PlayerControl.LocalPlayer == TheOtherRoles.Portalmaker.portalmaker;
            portalGameObject.SetActive(playerIsPortalmaker);
            portalAnimationGameObject.SetActive(true);

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
            TheOtherRolesPlugin.Logger.LogWarning(p);
            TheOtherRolesPlugin.Logger.LogWarning(firstPortal.portalGameObject.transform.position);
            TheOtherRolesPlugin.Logger.LogWarning(p.x - firstPortal.portalGameObject.transform.position.x);
            TheOtherRolesPlugin.Logger.LogWarning(p.y - firstPortal.portalGameObject.transform.position.y);
            var dist1 = Vector2.Distance(p, firstPortal.portalGameObject.transform.position);
            var dist2 = Vector2.Distance(p, secondPortal.portalGameObject.transform.position);
            return dist1 < dist2 ? secondPortal.portalGameObject.transform.position : firstPortal.portalGameObject.transform.position;
        }

        public static void meetingEndsUpdate() {
            // checkAndEnable
            if (secondPortal != null) {
                firstPortal.portalGameObject.SetActive(true);
                secondPortal.portalGameObject.SetActive(true);
                bothPlacedAndEnabled = true;
            }

            // reset teleported players
            teleportedPlayers = new List<byte>();
        }

        private static void preloadSprites() {
            for (int i = 0; i < portalAnimationSprites.Length; i++) getBoxAnimationSprite(i);
            portalSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.PortalAnimation.portal_0001.png", 115f);
        }

        public static void clearPortals() {
            preloadSprites();  // Force preload of sprites to avoid lag
            bothPlacedAndEnabled = false;
            firstPortal = null;
            secondPortal = null;
            isTeleporting = false;
            teleportedPlayers = new List<byte>();
        }

    }

}