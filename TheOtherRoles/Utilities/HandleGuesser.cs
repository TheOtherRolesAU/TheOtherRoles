using System;
using System.Collections.Generic;
using System.Text;
using TheOtherRoles.CustomGameModes;
using UnityEngine;

namespace TheOtherRoles.Utilities {
    public static class HandleGuesser {
        private static Sprite targetSprite;
        public static bool isGuesserGm = false;
        public static bool hasMultipleShotsPerMeeting = false;
        public static bool killsThroughShield = true;
        public static bool evilGuesserCanGuessSpy = true;
        public static bool guesserCantGuessSnitch = false;

        public static Sprite getTargetSprite() {
            if (targetSprite) return targetSprite;
            targetSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.TargetIcon.png", 150f);
            return targetSprite;
        }

        public static bool isGuesser(byte playerId) {
            if (isGuesserGm) return GuesserGM.isGuesser(playerId);
            return Guesser.isGuesser(playerId);
        }

        public static void clear(byte playerId) {
            if (isGuesserGm) GuesserGM.clear(playerId);
            else Guesser.clear(playerId);
        }

        public static int remainingShots(byte playerId, bool shoot = false) {
            if (isGuesserGm) return GuesserGM.remainingShots(playerId, shoot);
            return Guesser.remainingShots(playerId, shoot);
        }

        public static void clearAndReload() {
            Guesser.clearAndReload();
            GuesserGM.clearAndReload();
            isGuesserGm = MapOptions.gameMode == CustomGamemodes.Guesser;
            if (isGuesserGm) {
                guesserCantGuessSnitch = CustomOptionHolder.guesserGamemodeCantGuessSnitchIfTaksDone.getBool();
                hasMultipleShotsPerMeeting = CustomOptionHolder.guesserGamemodeHasMultipleShotsPerMeeting.getBool();
                killsThroughShield = CustomOptionHolder.guesserGamemodeKillsThroughShield.getBool();
                evilGuesserCanGuessSpy = CustomOptionHolder.guesserGamemodeEvilCanKillSpy.getBool();
            } else {
                guesserCantGuessSnitch = CustomOptionHolder.guesserCantGuessSnitchIfTaksDone.getBool();
                hasMultipleShotsPerMeeting = CustomOptionHolder.guesserHasMultipleShotsPerMeeting.getBool();
                killsThroughShield = CustomOptionHolder.guesserKillsThroughShield.getBool();
                evilGuesserCanGuessSpy = CustomOptionHolder.guesserEvilCanKillSpy.getBool();
            }

        }
    }
}
