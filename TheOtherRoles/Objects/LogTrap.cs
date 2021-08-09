using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TheOtherRoles.Objects
{
    class LogTrap
    {
        public static List<LogTrap> logTraps = new List<LogTrap>();
        public List<String> playersName = new List<String>();

        public static float logTrapLimit = CustomOptionHolder.loggerMaxTrap.getFloat();
        public static float recordIntervall = 0.10f;
        public static float recordTimer = 0.10f;
        public static float nbRecordPerTrap = CustomOptionHolder.loggerNbRecordPerTrap.getFloat();

        private List<String> playersNameRecordedLastTick = new List<String>();

        public static Dictionary<int, string> colorTrap = new Dictionary<int, string>()
        {
            {0,"Blue"},
            {1,"Red"},
            {2,"Yellow"}
        };

        public GameObject logTrap;
        private GameObject background;

        private static Sprite logTrapSprite;
        private Sprite backgroundSprite;
        private SpriteRenderer backgroundRendererComponent;

        // LogTrapSprite is set as the same sprite to Garlic to confuse imposteur. 
        public static Sprite getLogTrapSprite()
        {
            if (logTrapSprite) return logTrapSprite;
            logTrapSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Garlic.png", 300f);
            return logTrapSprite;
        }

        public Sprite getBackgroundSprite()
        {
            if (backgroundSprite) return backgroundSprite;
            if (Logger.logger != null && Logger.logger == PlayerControl.LocalPlayer)
            {
                int nbLogTrap = logTraps.Count;

                switch (nbLogTrap)
                {
                    case 0:
                    case 1:
                    case 2:
                        backgroundSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.LoggerBackground" + colorTrap[nbLogTrap] + ".png", 60f);
                        break;
                    default:
                        backgroundSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.GarlicBackground.png", 60f);
                        break;
                }
            }
            else
            {
                backgroundSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.GarlicBackground.png", 60f);
            }
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
            backgroundRendererComponent = backgroundRenderer;
            logTrap.SetActive(true);
            logTraps.Add(this);
        }

        public static void resetBackgroundImageForShifter()
        {
            if (Logger.logger != null && Logger.logger == PlayerControl.LocalPlayer)
            {
                for (int i = 0; i < logTraps.Count; i++)
                {
                    logTraps[i].backgroundRendererComponent.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.LoggerBackground" + colorTrap[i] + ".png", 60f);
                }
            }
        }


        public static void clearLogTraps()
        {
            logTraps = new List<LogTrap>();
        }

        public static void clearLogTrapsPlayerName()
        {
            foreach (LogTrap logTrap in logTraps)
            {
                logTrap.clearLoggedPlayersName();
            }
        }

        public void clearLoggedPlayersName()
        {
            playersName.Clear();
        }

        public static bool hasLogTrapLimitReached()
        {
            return (logTraps.Count >= logTrapLimit);
        }

        /**
         * for all trap, record player on it
         */
        public static void recordAllPlayerOnTraps()
        {
            if (Logger.logger == null || Logger.logger != PlayerControl.LocalPlayer) return;
            LogTrap.recordTimer -= Time.fixedDeltaTime;
            if(LogTrap.recordTimer <= 0f)
            {
                LogTrap.recordTimer = recordIntervall;
                foreach(LogTrap logtrap in logTraps)
                {
                    logtrap.recordPlayerOnTrap();
                }
            }
        }

        /**
         * The trap record who walk on it and save it into this.playersName                   
         */
        private void recordPlayerOnTrap()
        {
            List<String> playersNameCurrentlyRecorded = new List<String>();            
            float distanceRecord = 2f;
            if (!ShipStatus.Instance) return;
                        
            Vector2 logTrapTruePosition = logTrap.transform.position; 
            
            // Record every player name in trap area
            var allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];
                if (!playerInfo.Disconnected && !playerInfo.IsDead)
                {
                    PlayerControl currentPlayer = playerInfo.Object; 
                    if (currentPlayer && !currentPlayer.inVent)
                    {
                        Vector2 vector = currentPlayer.GetTruePosition() - logTrapTruePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= distanceRecord)
                        {
                            var commsActive = false;
                            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                                if (task.TaskType == TaskTypes.FixComms) commsActive = true;
                            //if walk during camouflage
                            if (Camouflager.camouflageTimer > 0 || commsActive)
                            {
                                playersNameCurrentlyRecorded.Add("Anonymous");
                            }
                            else if(Morphling.morphling != null && Morphling.morphling == currentPlayer &&  Morphling.morphTimer > 0 && Morphling.morphTarget != null )
                            {
                                playersNameCurrentlyRecorded.Add(Morphling.morphTarget.Data.PlayerName);
                            }
                            else
                            {
                                playersNameCurrentlyRecorded.Add(currentPlayer.Data.PlayerName);
                            }

                        } 
                    }
                }
            }

            List<String> playersNameCurrentlyRecordedToAdd;

            playersNameCurrentlyRecordedToAdd = new List<String>(playersNameCurrentlyRecorded);

            // Remove player that was already recorded last tick
            playersNameCurrentlyRecordedToAdd.RemoveAll(playerName => this.playersNameRecordedLastTick.Contains(playerName));

            // Add player Name that was not recorded last tick
            this.playersName.AddRange(playersNameCurrentlyRecordedToAdd);            
            
            // resize player recorded to nbRecordPerTrap
            if (this.playersName.Count > nbRecordPerTrap)
            {
                 this.playersName.RemoveRange(0, this.playersName.Count - (int)nbRecordPerTrap);
            }
            
            // save ppl recorded this tick
            this.playersNameRecordedLastTick = playersNameCurrentlyRecorded;
        }
       

    }
}
