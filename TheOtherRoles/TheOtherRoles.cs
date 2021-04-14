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

using Palette = BLMBFIODBKL;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public static class TheOtherRoles
    {
        public static System.Random rnd = new System.Random((int)DateTime.Now.Ticks);

        public static void clearAndReloadRoles() {
            Jester.clearAndReload();
            Mayor.clearAndReload();
            Engineer.clearAndReload();
            Sheriff.clearAndReload();
            Lighter.clearAndReload();
            Godfather.clearAndReload();
            Mafioso.clearAndReload();
            Janitor.clearAndReload();
            Detective.clearAndReload();
            TimeMaster.clearAndReload();
            Medic.clearAndReload();
            Shifter.clearAndReload();
            Swapper.clearAndReload();
            Lovers.clearAndReload();
            Seer.clearAndReload();
            Morphling.clearAndReload();
            Camouflager.clearAndReload();
            Hacker.clearAndReload();
            Child.clearAndReload();
            Tracker.clearAndReload();
            Vampire.clearAndReload();
            Snitch.clearAndReload();
            Jackal.clearAndReload();
            Sidekick.clearAndReload();
            Eraser.clearAndReload();
            Spy.clearAndReload();
            Trickster.clearAndReload();
        }

        public static class Jester {
            public static PlayerControl jester;
            public static Color color = new Color(255f / 255f, 84f / 255f, 167f / 255f, 1);

            public static bool triggerJesterWin = false;
            public static bool canCallEmergency = true;

            public static void clearAndReload() {
                jester = null;
                triggerJesterWin = false;
                canCallEmergency = CustomOptionHolder.jesterCanCallEmergency.getBool();
            }
        }

        public static class Mayor {
            public static PlayerControl mayor;
            public static Color color = new Color(105f / 255f, 58f / 255f, 58f / 255f, 1);

            public static void clearAndReload() {
                mayor = null;
            }
        }

        public static class Engineer {
            public static PlayerControl engineer;
            public static Color color = new Color(98f / 255f, 216f / 255f, 240f / 255f, 1);
            public static bool usedRepair;
            private static Sprite buttonSprite;

            public static Sprite getButtonSprite() {
                if (buttonSprite) return buttonSprite;
                buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.RepairButton.png", 115f);
                return buttonSprite;
            }

            public static void clearAndReload() {
                engineer = null;
                usedRepair = false;
            }
        }

        public static class Godfather {
            public static PlayerControl godfather;
            public static Color color = Palette.JPCHLLEJNEH;

            public static void clearAndReload() {
                godfather = null;
            }
        }

        public static class Mafioso {
            public static PlayerControl mafioso;
            public static Color color = Palette.JPCHLLEJNEH;

            public static void clearAndReload() {
                mafioso = null;
            }
        }


        public static class Janitor {
            public static PlayerControl janitor;
            public static Color color = Palette.JPCHLLEJNEH;

            public static float cooldown = 30f;

            private static Sprite buttonSprite;
            public static Sprite getButtonSprite() {
                if (buttonSprite) return buttonSprite;
                buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CleanButton.png", 115f);
                return buttonSprite;
            }

            public static void clearAndReload() {
                janitor = null;
                cooldown = CustomOptionHolder.janitorCooldown.getFloat();
            }
        }

        public static class Sheriff {
            public static PlayerControl sheriff;
            public static Color color = new Color(255f / 255f, 204f / 255f, 0f / 255f, 1);

            public static float cooldown = 30f;
            public static bool jesterCanDieToSheriff = false;
            public static bool spyCanDieToSheriff = false;

            public static PlayerControl currentTarget;

            public static void clearAndReload() {
                sheriff = null;
                currentTarget = null;
                cooldown = CustomOptionHolder.sheriffCooldown.getFloat();
                jesterCanDieToSheriff = CustomOptionHolder.jesterCanDieToSheriff.getBool();
                spyCanDieToSheriff = CustomOptionHolder.spyCanDieToSheriff.getBool();
            }
        }

        public static class Lighter {
            public static PlayerControl lighter;
            public static Color color = new Color(250f / 255f, 204f / 255f, 37f / 255f, 1);
            
            public static float lighterModeLightsOnVision = 2f;
            public static float lighterModeLightsOffVision = 0.75f;

            public static float cooldown = 30f;
            public static float duration = 5f;

            public static float lighterTimer = 0f;

            private static Sprite buttonSprite;
            public static Sprite getButtonSprite() {
                if (buttonSprite) return buttonSprite;
                buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.LighterButton.png", 115f);
                return buttonSprite;
            }

            public static void clearAndReload() {
                lighter = null;
                lighterTimer = 0f;
                cooldown = CustomOptionHolder.lighterCooldown.getFloat();
                duration = CustomOptionHolder.lighterDuration.getFloat();
                lighterModeLightsOnVision = CustomOptionHolder.lighterModeLightsOnVision.getFloat();
                lighterModeLightsOffVision = CustomOptionHolder.lighterModeLightsOffVision.getFloat();
            }
        }

        public static class Detective {
            public static PlayerControl detective;
            public static Color color = new Color(2f / 255f, 61f / 255f, 156f / 255f, 1);

            public static float footprintIntervall = 1f;
            public static float footprintDuration = 1f;
            public static bool anonymousFootprints = false;
            public static float reportNameDuration = 0f;
            public static float reportColorDuration = 20f;
            public static float timer = 6.2f;

            public static void clearAndReload() {
                detective = null;
                anonymousFootprints = CustomOptionHolder.detectiveAnonymousFootprints.getBool();
                footprintIntervall = CustomOptionHolder.detectiveFootprintIntervall.getFloat();
                footprintDuration = CustomOptionHolder.detectiveFootprintDuration.getFloat();
                reportNameDuration = CustomOptionHolder.detectiveReportNameDuration.getFloat();
                reportColorDuration = CustomOptionHolder.detectiveReportColorDuration.getFloat();
                timer = 6.2f;
            }
        }
    }

    public static class TimeMaster {
        public static PlayerControl timeMaster;
        public static Color color = new Color(114f / 255f, 234f / 255f, 247f / 255f, 1);

        public static bool reviveDuringRewind = false;
        public static float rewindTime = 3f;
        public static float shieldDuration = 3f;
        public static float cooldown = 30f;

        public static bool shieldActive = false;
        public static bool isRewinding = false;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.TimeShieldButton.png", 115f);
            return buttonSprite;
        }

        public static void clearAndReload() {
            timeMaster = null;
            isRewinding = false;
            shieldActive = false;
            rewindTime = CustomOptionHolder.timeMasterRewindTime.getFloat();
            shieldDuration = CustomOptionHolder.timeMasterShieldDuration.getFloat();
            cooldown = CustomOptionHolder.timeMasterCooldown.getFloat();
        }
    }

    public static class Medic {
        public static PlayerControl medic;
        public static PlayerControl shielded;
        public static Color color = new Color(0f / 255f, 80f / 255f, 105f / 255f, 1);
        public static bool usedShield;

        public static int showShielded = 0;
        public static bool showAttemptToShielded = false;

        public static Color shieldedColor = new Color(0f / 255f, 221f / 255f, 255f / 255f, 1);
        public static PlayerControl currentTarget;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.ShieldButton.png", 115f);
            return buttonSprite;
        }

        public static void clearAndReload() {
            medic = null;
            if (shielded?.KJAENOGGEOK?.material != null) shielded.KJAENOGGEOK.material.SetFloat("_Outline", 0f);
            shielded = null;
            currentTarget = null;
            usedShield = false;
            shieldedColor = new Color(0f / 255f, 221f / 255f, 255f / 255f, 1);
            showShielded = CustomOptionHolder.medicShowShielded.getSelection();
            showAttemptToShielded = CustomOptionHolder.medicShowAttemptToShielded.getBool();
        }
    }

    public static class Shifter {
        public static PlayerControl shifter;
        public static Color color = new Color(90f / 255f, 90f / 255f, 90f / 255f, 1);

        public static PlayerControl futureShift;
        public static PlayerControl currentTarget;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.ShiftButton.png", 115f);
            return buttonSprite;
        }

        public static void clearAndReload() {
            shifter = null;
            currentTarget = null;
            futureShift = null;
        }
    }

    public static class Swapper {
        public static PlayerControl swapper;
        public static Color color = new Color(240f / 255f, 128f / 255f, 72f / 255f, 1);
        private static Sprite spriteCheck;
        public static bool canCallEmergency = false;

        public static byte playerId1 = Byte.MaxValue;
        public static byte playerId2 = Byte.MaxValue;

        public static Sprite getCheckSprite() {
            if (spriteCheck) return spriteCheck;
            spriteCheck = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SwapperCheck.png", 150f);
            return spriteCheck;
        }

        public static void clearAndReload() {
            swapper = null;
            playerId1 = Byte.MaxValue;
            playerId2 = Byte.MaxValue;
            canCallEmergency = CustomOptionHolder.swapperCanCallEmergency.getBool();
        }
    }

    public static class Lovers {
        public static PlayerControl lover1;
        public static PlayerControl lover2;
        public static Color color = new Color(252f / 255f, 3f / 255f, 190f / 255f, 1);

        public static bool bothDie = true;
        // Lovers save if next to be exiled is a lover, because RPC of ending game comes before RPC of exiled
        public static bool notAckedExiledIsLover = false;

        public static bool existingAndAlive() {
            return lover1 != null && lover2 != null && !lover1.PPMOEEPBHJO.IAGJEKLJCCI && !lover2.PPMOEEPBHJO.IAGJEKLJCCI && !lover1.PPMOEEPBHJO.MFFAGDHDHLO && !lover2.PPMOEEPBHJO.MFFAGDHDHLO && !notAckedExiledIsLover; // ADD NOT ACKED IS LOVER
        }

        public static bool existingAndCrewLovers() {
            if (lover1 == null || lover2 == null || lover1.PPMOEEPBHJO.MFFAGDHDHLO || lover2.PPMOEEPBHJO.MFFAGDHDHLO) return false; // Null or disconnected
            return !(lover1.PPMOEEPBHJO.FDNMBJOAPFL || lover2.PPMOEEPBHJO.FDNMBJOAPFL || lover1 == Jackal.jackal || lover2 == Jackal.jackal || lover1 == Sidekick.sidekick || lover2 == Sidekick.sidekick); // Not Impostor, Sidekick or Jackal
        }

        public static void clearAndReload() {
            lover1 = null;
            lover2 = null;
            notAckedExiledIsLover = false;
            bothDie = CustomOptionHolder.loversBothDie.getBool();
        }
    }

    public static class Seer {
        public static PlayerControl seer;
        public static Color color = new Color(60f / 255f, 181f / 255f, 100f / 255f, 1);
        public static List<Vector3> deadBodyPositions = new List<Vector3>();

        public static float soulDuration = 15f;
        public static bool limitSoulDuration = false;
        public static int mode = 0;

        private static Sprite soulSprite;
        public static Sprite getSoulSprite() {
            if (soulSprite) return soulSprite;
            soulSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Soul.png", 500f);
            return soulSprite;
        }

        public static void clearAndReload() {
            seer = null;
            deadBodyPositions = new List<Vector3>();
            limitSoulDuration = CustomOptionHolder.seerLimitSoulDuration.getBool();
            soulDuration = CustomOptionHolder.seerSoulDuration.getFloat();
            mode = CustomOptionHolder.medicShowShielded.getSelection();
        }
    }

    public static class Morphling {
        public static PlayerControl morphling;
        public static Color color = Palette.JPCHLLEJNEH;
        private static Sprite sampleSprite;
        private static Sprite morphSprite;
    
        public static float cooldown = 30f;

        public static PlayerControl currentTarget;
        public static PlayerControl sampledTarget;
        public static PlayerControl morphTarget;
        public static float morphTimer = 0f;

        public static void resetMorph() {
            morphTarget = null;
            morphTimer = 0f;
            if (morphling == null) return;
            morphling.SetName(morphling.PPMOEEPBHJO.PCLLABJCIPC);
            morphling.SetHat(morphling.PPMOEEPBHJO.CPGFLBANALE, (int)morphling.PPMOEEPBHJO.IMMNCAGJJJC);
            Helpers.setSkinWithAnim(morphling.MyPhysics, morphling.PPMOEEPBHJO.CGNMKICGLOG);
            morphling.SetPet(morphling.PPMOEEPBHJO.LBHODBKCBKA);
            morphling.CurrentPet.BDBDGFDELMB = morphling.BDBDGFDELMB;
            morphling.SetColor(morphling.PPMOEEPBHJO.IMMNCAGJJJC);
            morphling.KJAENOGGEOK.material.SetFloat("_Outline", 0f);
        }

        public static void clearAndReload() {
            resetMorph();
            morphling = null;
            currentTarget = null;
            sampledTarget = null;
            morphTarget = null;
            morphTimer = 0f;
            cooldown = CustomOptionHolder.morphlingCooldown.getFloat();
        }

        public static Sprite getSampleSprite() {
            if (sampleSprite) return sampleSprite;
            sampleSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SampleButton.png", 115f);
            return sampleSprite;
        }

        public static Sprite getMorphSprite() {
            if (morphSprite) return morphSprite;
            morphSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.MorphButton.png", 115f);
            return morphSprite;
        }
    }

    public static class Camouflager {
        public static PlayerControl camouflager;
        public static Color color = Palette.JPCHLLEJNEH;
    
        public static float cooldown = 30f;
        public static float camouflageTimer = 0f;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CamoButton.png", 115f);
            return buttonSprite;
        }

        public static void resetCamouflage() {
            camouflageTimer = 0f;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                if (p == null) continue;
                if (Morphling.morphling == null || Morphling.morphling != p) {
                    p.SetName(p.PPMOEEPBHJO.PCLLABJCIPC);
                    p.SetHat(p.PPMOEEPBHJO.CPGFLBANALE, (int)p.PPMOEEPBHJO.IMMNCAGJJJC);
                    Helpers.setSkinWithAnim(p.MyPhysics, p.PPMOEEPBHJO.CGNMKICGLOG);
                    p.SetPet(p.PPMOEEPBHJO.LBHODBKCBKA);
                    p.CurrentPet.BDBDGFDELMB = p.BDBDGFDELMB;
                    p.SetColor(p.PPMOEEPBHJO.IMMNCAGJJJC);
                }
            }
        }

        public static void clearAndReload() {
            resetCamouflage();
            camouflager = null;
            camouflageTimer = 0f;
            cooldown = CustomOptionHolder.camouflagerCooldown.getFloat();
        }
    }

    public static class Hacker {
        public static PlayerControl hacker;
        private static Sprite adminTableIcon;
        public static Color color = new Color(252f / 255f, 90f / 255f, 30f / 255f, 1);

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static bool onlyColorType = false;

        public static float hackerTimer = 0f;

        public static Sprite getAdminTableIconSprite() {
            if (adminTableIcon) return adminTableIcon;
            adminTableIcon = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.AdminTableIcon.png", 350f);
            return adminTableIcon;
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.HackerButton.png", 115f);
            return buttonSprite;
        }

        public static void clearAndReload() {
            hacker = null;
            hackerTimer = 0f;
            cooldown = CustomOptionHolder.hackerCooldown.getFloat();
            duration = CustomOptionHolder.hackerHackeringDuration.getFloat();
            onlyColorType = CustomOptionHolder.hackerOnlyColorType.getBool();
        }
    }

    public static class Child {
        public static PlayerControl child;
        public static Color color = Color.white;
        public const float defaultColliderRadius = 0.2233912f;
            public const float defaultColliderOffset = 0.3636057f;

        public static float growingUpDuration = 400f;
        public static DateTime timeOfGrowthStart = DateTime.UtcNow;
        public static bool triggerChildLose = false;

        public static void clearAndReload() {
            child = null;
            triggerChildLose = false;
            growingUpDuration = CustomOptionHolder.childGrowingUpDuration.getFloat();
            timeOfGrowthStart = DateTime.UtcNow;
        }

        public static float growingProgress() {
            if (timeOfGrowthStart == null) return 0f;

            float timeSinceStart = (float)(DateTime.UtcNow - timeOfGrowthStart).TotalMilliseconds;
            return Mathf.Clamp(timeSinceStart/(growingUpDuration*1000), 0f, 1f);
        }

        public static bool isGrownUp() {
            return growingProgress() == 1f;
        }
    }

    public static class Tracker {
        public static PlayerControl tracker;
        public static Color color = new Color(117f / 255f, 209f / 255f, 255f / 255f, 1);

        public static float updateIntervall = 5f;

        public static PlayerControl currentTarget;
        public static PlayerControl tracked;
        public static bool usedTracker = false;
        public static float timeUntilUpdate = 0f;
        public static Arrow arrow = new Arrow(Color.blue);

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.TrackerButton.png", 115f);
            return buttonSprite;
        }

        public static void clearAndReload() {
            tracker = null;
            currentTarget = null;
            tracked = null;
            usedTracker = false;
            timeUntilUpdate = 0f;
            updateIntervall = CustomOptionHolder.trackerUpdateIntervall.getFloat();
            if (arrow?.arrow != null) UnityEngine.Object.Destroy(arrow.arrow);
            arrow = new Arrow(Color.blue);
            if (arrow.arrow != null) arrow.arrow.SetActive(false);
        }
    }

    public static class Vampire {
        public static PlayerControl vampire;
        public static Color color = Palette.JPCHLLEJNEH;

        public static float delay = 10f;
        public static float cooldown = 30f;
        public static bool canKillNearGarlics = true;
        public static bool localPlacedGarlic = false;
        public static bool garlicsActive = true;

        public static PlayerControl currentTarget;
        public static PlayerControl bitten; 
        public static bool targetNearGarlic = false;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.VampireButton.png", 115f);
            return buttonSprite;
        }

        private static Sprite garlicButtonSprite;
        public static Sprite getGarlicButtonSprite() {
            if (garlicButtonSprite) return garlicButtonSprite;
            garlicButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.GarlicButton.png", 115f);
            return garlicButtonSprite;
        }

        public static void clearAndReload() {
            vampire = null;
            bitten = null;
            targetNearGarlic = false;
            localPlacedGarlic = false;
            currentTarget = null;
            garlicsActive = CustomOptionHolder.vampireSpawnRate.getSelection() > 0;
            delay = CustomOptionHolder.vampireKillDelay.getFloat();
            cooldown = CustomOptionHolder.vampireCooldown.getFloat();
            canKillNearGarlics = CustomOptionHolder.vampireCanKillNearGarlics.getBool();
        }
    }

    public static class Snitch {
        public static PlayerControl snitch;
        public static Color color = new Color(227f / 255f, 251f / 255f, 47f / 255f, 1);

        public static List<Arrow> localArrows = new List<Arrow>();
        public static int taskCountForImpostors = 1;

        public static void clearAndReload() {
            if (localArrows != null) {
                foreach (Arrow arrow in localArrows)
                    if (arrow?.arrow != null)
                    UnityEngine.Object.Destroy(arrow.arrow);
            }
            localArrows = new List<Arrow>();
            taskCountForImpostors = Mathf.RoundToInt(CustomOptionHolder.snitchLeftTasksForImpostors.getFloat());
            snitch = null;
        }
    }

    public static class Jackal {
        public static PlayerControl jackal;
        public static Color color = new Color(0f / 255f, 180f / 255f, 235f / 255f, 1);
        public static PlayerControl fakeSidekick;

        public static PlayerControl currentTarget;
        public static List<PlayerControl> formerJackals = new List<PlayerControl>();
        
        public static float cooldown = 30f;
        public static float createSidekickCooldown = 30f;
        public static bool canUseVents = true;
        public static bool canCreateSidekick = true;
        public static Sprite buttonSprite;
        public static bool jackalPromotedFromSidekickCanCreateSidekick = true;
        public static bool canCreateSidekickFromImpostor = true;

        public static Sprite getSidekickButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SidekickButton.png", 115f);
            return buttonSprite;
        }

        public static void removeCurrentJackal() {
            formerJackals.Add(jackal);
            jackal = null;
            currentTarget = null;
            fakeSidekick = null;
            cooldown = CustomOptionHolder.jackalKillCooldown.getFloat();
            createSidekickCooldown = CustomOptionHolder.jackalCreateSidekickCooldown.getFloat();
        }

        public static void clearAndReload() {
            jackal = null;
            currentTarget = null;
            fakeSidekick = null;
            cooldown = CustomOptionHolder.jackalKillCooldown.getFloat();
            createSidekickCooldown = CustomOptionHolder.jackalCreateSidekickCooldown.getFloat();
            canUseVents = CustomOptionHolder.jackalCanUseVents.getBool();
            canCreateSidekick = CustomOptionHolder.jackalCanCreateSidekick.getBool();
            jackalPromotedFromSidekickCanCreateSidekick = CustomOptionHolder.jackalPromotedFromSidekickCanCreateSidekick.getBool();
            canCreateSidekickFromImpostor = CustomOptionHolder.jackalCanCreateSidekickFromImpostor.getBool();
            formerJackals.Clear();
        }
        
    }

    public static class Sidekick {
        public static PlayerControl sidekick;
        public static Color color = new Color(0f / 255f, 180f / 255f, 235f / 255f, 1);

        public static PlayerControl currentTarget;

        public static float cooldown = 30f;
        public static bool canUseVents = true;
        public static bool canKill = true;
        public static bool promotesToJackal = true;

        public static void clearAndReload() {
            sidekick = null;
            currentTarget = null;
            cooldown = CustomOptionHolder.jackalKillCooldown.getFloat();
            canUseVents = CustomOptionHolder.sidekickCanUseVents.getBool();
            canKill = CustomOptionHolder.sidekickCanKill.getBool();
            promotesToJackal = CustomOptionHolder.sidekickPromotesToJackal.getBool();
        }
    }

    public static class Eraser {
        public static PlayerControl eraser;
        public static Color color = Palette.JPCHLLEJNEH;

        public static List<PlayerControl> futureErased = new List<PlayerControl>();
        public static PlayerControl currentTarget;
        public static float cooldown = 30f;
        public static bool canEraseAnyone = false; 

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.EraserButton.png", 115f);
            return buttonSprite;
        }

        public static void clearAndReload() {
            eraser = null;
            futureErased = new List<PlayerControl>();
            currentTarget = null;
            cooldown = CustomOptionHolder.eraserCooldown.getFloat();
            canEraseAnyone = CustomOptionHolder.eraserCanEraseAnyone.getBool();
        }
    }
    
    public static class Spy {
        public static PlayerControl spy;
        public static Color color = Palette.JPCHLLEJNEH;

        public static bool impostorsCanKillAnyone = true;

        public static void clearAndReload() {
            spy = null;
            impostorsCanKillAnyone = CustomOptionHolder.spyImpostorsCanKillAnyone.getBool();
        }
    }

    public static class Trickster {
        public static PlayerControl trickster;
        public static Color color = Palette.JPCHLLEJNEH;
        public static float placeBoxCooldown = 30f;
        public static float lightsOutCooldown = 30f;
        public static float lightsOutDuration = 10f;
        public static float lightsOutTimer = 0f;

        private static Sprite placeBoxButtonSprite;
        private static Sprite lightOutButtonSprite;
        public static Sprite getPlaceBoxButtonSprite() {
            if (placeBoxButtonSprite) return placeBoxButtonSprite;
            placeBoxButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.PlaceJackInTheBoxButton.png", 115f);
            return placeBoxButtonSprite;
        }
        public static Sprite getLightsOutButtonSprite() {
            if (lightOutButtonSprite) return lightOutButtonSprite;
            lightOutButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.LightsOutButton.png", 115f);
            return lightOutButtonSprite;
        }

        public static void clearAndReload() {
            trickster = null;
            lightsOutTimer = 0f;
            placeBoxCooldown = CustomOptionHolder.tricksterPlaceBoxCooldown.getFloat();
            lightsOutCooldown = CustomOptionHolder.tricksterLightsOutCooldown.getFloat();
            lightsOutDuration = CustomOptionHolder.tricksterLightsOutDuration.getFloat();
            JackInTheBox.UpdateStates(); // if the role is erased, we might have to update the state of the created objects
        }

    }
}