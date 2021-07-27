using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TheOtherRoles.Objects
{
    public class LogTrap
    {       
        public List<String> playersName;

        public GameObject logTrap;
        private GameObject background;

        private static Sprite logTrapSprite;
        private static Sprite backgroundSprite;

        // LogTrapSprite is set as the same sprite to Garlic to confuse imposteur. 
        public static Sprite getLogTrapSprite()
        {
            if (logTrapSprite) return logTrapSprite;
            logTrapSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Garlic.png", 300f);
            return logTrapSprite;
        }

        public static Sprite getBackgroundSprite()
        {
            if (backgroundSprite) return backgroundSprite;
            backgroundSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.GarlicBackground.png", 60f);
            return backgroundSprite;
        }

        public LogTrap(Vector2 p)
        {
            logTrap = new GameObject("LogTrap");
            background = new GameObject("LogTrapBackground");
            background.transform.SetParent(logTrap.transform);
            Vector3 position = new Vector3(p.x, p.y, PlayerControl.LocalPlayer.transform.localPosition.z + 0.001f); // just behind player
            logTrap.transform.position = position;
            logTrap.transform.localPosition = position;
            background.transform.localPosition = new Vector3(0, 0, -0.01f); // before player

            var logTrapRenderer = logTrap.AddComponent<SpriteRenderer>();
            logTrapRenderer.sprite = getLogTrapSprite();
            var backgroundRenderer = background.AddComponent<SpriteRenderer>();
            backgroundRenderer.sprite = getBackgroundSprite();
            playersName.Add("player1");
            playersName.Add("player2");
            playersName.Add("player3");
            logTrap.SetActive(true);
            Logger.logTraps.Add(this);
        }

        public void clearLoggedPlayersName()
        {
            playersName.Clear();
        }

    }
}
