using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

namespace TheOtherRoles.Objects
{
    internal class Gift
    {
        public static List<Gift> gifts = new List<Gift>();

        public GameObject gift;
        public byte id;

        private static Sprite giftSprite;
        public static Sprite getGiftSprite()
        {
            if (giftSprite) return giftSprite;
            giftSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Garlic.png", 300f);
            return giftSprite;
        }

        public Gift(Vector2 p)
        {
            gift = new GameObject("Gift");
            Vector3 position = new Vector3(p.x, p.y, PlayerControl.LocalPlayer.transform.localPosition.z + 0.001f); // just behind player
            gift.transform.position = position;
            gift.transform.localPosition = position;

            id = (byte)(gifts.Select(x => x.id).DefaultIfEmpty((byte)0).Max() + 1);

            var giftRenderer = gift.AddComponent<SpriteRenderer>();
            giftRenderer.sprite = getGiftSprite();
            gift.SetActive(true);
            gifts.Add(this);
        }

        public static Gift playerIsNearGift(PlayerControl player)
        {
            foreach (Gift gift in gifts)
            {
                // if player is near the gift, return the gift, else nothing
                if (Vector2.Distance(PlayerControl.LocalPlayer.transform.localPosition, gift.gift.transform.position) <= 1.91f) return gift;
            }
            return null;
        }

        public static void collectGift(byte giftId)
        {
            foreach (Gift gift in gifts)
                if (giftId == gift.id)
                {
                    gifts.Remove(gift);
                    gift.gift.SetActive(false);
                    return;
                }
        }
        public static void clearGifts()
        {
            gifts = new List<Gift>();
        }
    }
}
