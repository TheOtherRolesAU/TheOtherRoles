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
            Mini.clearAndReload();
            Tracker.clearAndReload();
            Vampire.clearAndReload();
            Snitch.clearAndReload();
            Jackal.clearAndReload();
            Sidekick.clearAndReload();
            Eraser.clearAndReload();
            Spy.clearAndReload();
            Trickster.clearAndReload();
            Cleaner.clearAndReload();
            Warlock.clearAndReload();
            SecurityGuard.clearAndReload();
            Arsonist.clearAndReload();
            Guesser.clearAndReload();
            BountyHunter.clearAndReload();
        }

        public static class Jester {
            public static PlayerControl jester;
            public static Color color = new Color32(236, 98, 165, byte.MaxValue);

            public static bool triggerJesterWin = false;
            public static bool canCallEmergency = true;
            public static bool canSabotage = true;

            public static void clearAndReload() {
                jester = null;
                triggerJesterWin = false;
                canCallEmergency = CustomOptionHolder.jesterCanCallEmergency.getBool();
                canSabotage = CustomOptionHolder.jesterCanSabotage.getBool();
            }
        }

        public static class Mayor {
            public static PlayerControl mayor;
            public static Color color = new Color32(32, 77, 66, byte.MaxValue);

            public static void clearAndReload() {
                mayor = null;
            }
        }

        public static class Engineer {
            public static PlayerControl engineer;
            public static Color color = new Color32(0, 40, 245, byte.MaxValue);
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
            public static Color color = Palette.ImpostorRed;

            public static void clearAndReload() {
                godfather = null;
            }
        }

        public static class Mafioso {
            public static PlayerControl mafioso;
            public static Color color = Palette.ImpostorRed;

            public static void clearAndReload() {
                mafioso = null;
            }
        }


        public static class Janitor {
            public static PlayerControl janitor;
            public static Color color = Palette.ImpostorRed;

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
            public static Color color = new Color32(248, 205, 70, byte.MaxValue);

            public static float cooldown = 30f;
            public static bool canKillNeutrals = false;
            public static bool spyCanDieToSheriff = false;

            public static PlayerControl currentTarget;

            public static void clearAndReload() {
                sheriff = null;
                currentTarget = null;
                cooldown = CustomOptionHolder.sheriffCooldown.getFloat();
                canKillNeutrals = CustomOptionHolder.sheriffCanKillNeutrals.getBool();
                spyCanDieToSheriff = CustomOptionHolder.spyCanDieToSheriff.getBool();
            }
        }

        public static class Lighter {
            public static PlayerControl lighter;
            public static Color color = new Color32(238, 229, 190, byte.MaxValue);
            
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
            public static Color color = new Color32(45, 106, 165, byte.MaxValue);

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
        public static Color color = new Color32(112, 142, 239, byte.MaxValue);

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
        public static Color color = new Color32(126, 251, 194, byte.MaxValue);
        public static bool usedShield;

        public static int showShielded = 0;
        public static bool showAttemptToShielded = false;

        public static Color shieldedColor = new Color32(0, 221, 255, byte.MaxValue);
        public static PlayerControl currentTarget;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.ShieldButton.png", 115f);
            return buttonSprite;
        }

        public static void clearAndReload() {
            medic = null;
            shielded = null;
            currentTarget = null;
            usedShield = false;
            showShielded = CustomOptionHolder.medicShowShielded.getSelection();
            showAttemptToShielded = CustomOptionHolder.medicShowAttemptToShielded.getBool();
        }
    }

    public static class Shifter {
        public static PlayerControl shifter;
        public static Color color = new Color32(102, 102, 102, byte.MaxValue);

        public static PlayerControl futureShift;
        public static PlayerControl currentTarget;
        public static bool shiftModifiers = false;

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
            shiftModifiers = CustomOptionHolder.shifterShiftsModifiers.getBool();
        }
    }

    public static class Swapper {
        public static PlayerControl swapper;
        public static Color color = new Color32(134, 55, 86, byte.MaxValue);
        private static Sprite spriteCheck;
        public static bool canCallEmergency = false;
        public static bool canOnlySwapOthers = false;

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
            canOnlySwapOthers = CustomOptionHolder.swapperCanOnlySwapOthers.getBool();
        }
    }

    public static class Lovers {
        public static PlayerControl lover1;
        public static PlayerControl lover2;
        public static Color color = new Color32(232, 57, 185, byte.MaxValue);

        public static bool bothDie = true;
        // Lovers save if next to be exiled is a lover, because RPC of ending game comes before RPC of exiled
        public static bool notAckedExiledIsLover = false;

        public static bool existing() {
            return lover1 != null && lover2 != null && !lover1.Data.Disconnected && !lover2.Data.Disconnected;
        }

        public static bool existingAndAlive() {
            return existing() && !lover1.Data.IsDead && !lover2.Data.IsDead && !notAckedExiledIsLover; // ADD NOT ACKED IS LOVER
        }

        public static bool existingWithKiller() {
            return existing() && (lover1 == Jackal.jackal     || lover2 == Jackal.jackal
                               || lover1 == Sidekick.sidekick || lover2 == Sidekick.sidekick
                               || lover1.Data.IsImpostor      || lover2.Data.IsImpostor);
        }

        public static bool hasAliveKillingLover(this PlayerControl player) {
            if (!Lovers.existingAndAlive() || !existingWithKiller())
                return false;
            return (player != null && (player == lover1 || player == lover2));
        }

        public static void clearAndReload() {
            lover1 = null;
            lover2 = null;
            notAckedExiledIsLover = false;
            bothDie = CustomOptionHolder.loversBothDie.getBool();
        }

        public static PlayerControl getPartner(this PlayerControl player) {
            if (player == null)
                return null;
            if (lover1 == player)
                return lover2;
            if (lover2 == player)
                return lover1;
            return null;
        }
    }

    public static class Seer {
        public static PlayerControl seer;
        public static Color color = new Color32(97, 178, 108, byte.MaxValue);
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
            mode = CustomOptionHolder.seerMode.getSelection();
        }
    }

    public static class Morphling {
        public static PlayerControl morphling;
        public static Color color = Palette.ImpostorRed;
        private static Sprite sampleSprite;
        private static Sprite morphSprite;
    
        public static float cooldown = 30f;
        public static float duration = 10f;

        public static PlayerControl currentTarget;
        public static PlayerControl sampledTarget;
        public static PlayerControl morphTarget;
        public static float morphTimer = 0f;

        public static void resetMorph() {
            morphTarget = null;
            morphTimer = 0f;
            if (morphling == null) return;
            morphling.SetName(morphling.Data.PlayerName);
            morphling.SetHat(morphling.Data.HatId, (int)morphling.Data.ColorId);
            Helpers.setSkinWithAnim(morphling.MyPhysics, morphling.Data.SkinId);
            morphling.SetPet(morphling.Data.PetId);
            morphling.CurrentPet.Visible = morphling.Visible;
            morphling.SetColor(morphling.Data.ColorId);
        }

        public static void clearAndReload() {
            resetMorph();
            morphling = null;
            currentTarget = null;
            sampledTarget = null;
            morphTarget = null;
            morphTimer = 0f;
            cooldown = CustomOptionHolder.morphlingCooldown.getFloat();
            duration = CustomOptionHolder.morphlingDuration.getFloat();
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
        public static Color color = Palette.ImpostorRed;
    
        public static float cooldown = 30f;
        public static float duration = 10f;
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
                    p.SetName(p.Data.PlayerName);
                    p.SetHat(p.Data.HatId, (int)p.Data.ColorId);
                    Helpers.setSkinWithAnim(p.MyPhysics, p.Data.SkinId);
                    p.SetPet(p.Data.PetId);
                    p.CurrentPet.Visible = p.Visible;
                    p.SetColor(p.Data.ColorId);
                }
            }
        }

        public static void clearAndReload() {
            resetCamouflage();
            camouflager = null;
            camouflageTimer = 0f;
            cooldown = CustomOptionHolder.camouflagerCooldown.getFloat();
            duration = CustomOptionHolder.camouflagerDuration.getFloat();
        }
    }

    public static class Hacker {
        public static PlayerControl hacker;
        public static Color color = new Color32(117, 250, 76, byte.MaxValue);

        public static float cooldown = 30f;
        public static float duration = 10f;
        public static bool onlyColorType = false;
        public static float hackerTimer = 0f;

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

    public static class Mini {
        public static PlayerControl mini;
        public static Color color = Color.white;
        public const float defaultColliderRadius = 0.2233912f;
            public const float defaultColliderOffset = 0.3636057f;

        public static float growingUpDuration = 400f;
        public static DateTime timeOfGrowthStart = DateTime.UtcNow;
        public static bool triggerMiniLose = false;

        public static void clearAndReload() {
            mini = null;
            triggerMiniLose = false;
            growingUpDuration = CustomOptionHolder.miniGrowingUpDuration.getFloat();
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
        public static Color color = new Color32(100, 58, 220, byte.MaxValue);

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
        public static Color color = Palette.ImpostorRed;

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
        public static Color color = new Color32(184, 251, 79, byte.MaxValue);

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
        public static Color color = new Color32(0, 180, 235, byte.MaxValue);
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
        public static bool hasImpostorVision = false;

        public static Sprite getSidekickButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SidekickButton.png", 115f);
            return buttonSprite;
        }

        public static void removeCurrentJackal() {
            if (!formerJackals.Any(x => x.PlayerId == jackal.PlayerId)) formerJackals.Add(jackal);
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
            hasImpostorVision = CustomOptionHolder.jackalAndSidekickHaveImpostorVision.getBool();
        }
        
    }

    public static class Sidekick {
        public static PlayerControl sidekick;
        public static Color color = new Color32(0, 180, 235, byte.MaxValue);

        public static PlayerControl currentTarget;

        public static float cooldown = 30f;
        public static bool canUseVents = true;
        public static bool canKill = true;
        public static bool promotesToJackal = true;
        public static bool hasImpostorVision = false;

        public static void clearAndReload() {
            sidekick = null;
            currentTarget = null;
            cooldown = CustomOptionHolder.jackalKillCooldown.getFloat();
            canUseVents = CustomOptionHolder.sidekickCanUseVents.getBool();
            canKill = CustomOptionHolder.sidekickCanKill.getBool();
            promotesToJackal = CustomOptionHolder.sidekickPromotesToJackal.getBool();
            hasImpostorVision = CustomOptionHolder.jackalAndSidekickHaveImpostorVision.getBool();
        }
    }

    public static class Eraser {
        public static PlayerControl eraser;
        public static Color color = Palette.ImpostorRed;

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
        public static Color color = Palette.ImpostorRed;

        public static bool impostorsCanKillAnyone = true;
        public static bool canEnterVents = false;
        public static bool hasImpostorVision = false;

        public static void clearAndReload() {
            spy = null;
            impostorsCanKillAnyone = CustomOptionHolder.spyImpostorsCanKillAnyone.getBool();
            canEnterVents = CustomOptionHolder.spyCanEnterVents.getBool();
            hasImpostorVision = CustomOptionHolder.spyHasImpostorVision.getBool();
        }
    }

    public static class Trickster {
        public static PlayerControl trickster;
        public static Color color = Palette.ImpostorRed;
        public static float placeBoxCooldown = 30f;
        public static float lightsOutCooldown = 30f;
        public static float lightsOutDuration = 10f;
        public static float lightsOutTimer = 0f;

        private static Sprite placeBoxButtonSprite;
        private static Sprite lightOutButtonSprite;
        private static Sprite tricksterVentButtonSprite;

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

        public static Sprite getTricksterVentButtonSprite() {
            if (tricksterVentButtonSprite) return tricksterVentButtonSprite;
            tricksterVentButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.TricksterVentButton.png", 115f);
            return tricksterVentButtonSprite;
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

    public static class Cleaner {
        public static PlayerControl cleaner;
        public static Color color = Palette.ImpostorRed;

        public static float cooldown = 30f;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CleanButton.png", 115f);
            return buttonSprite;
        }

        public static void clearAndReload() {
            cleaner = null;
            cooldown = CustomOptionHolder.cleanerCooldown.getFloat();
        }
    }

    public static class Warlock {

        public static PlayerControl warlock;
        public static Color color = Palette.ImpostorRed;

        public static PlayerControl currentTarget;
        public static PlayerControl curseVictim;
        public static PlayerControl curseVictimTarget;
        public static PlayerControl curseKillTarget;

        public static float cooldown = 30f;
        public static float rootTime = 5f;

        private static Sprite curseButtonSprite;
        private static Sprite curseKillButtonSprite;

        public static Sprite getCurseButtonSprite() {
            if (curseButtonSprite) return curseButtonSprite;
            curseButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CurseButton.png", 115f);
            return curseButtonSprite;
        }

        public static Sprite getCurseKillButtonSprite() {
            if (curseKillButtonSprite) return curseKillButtonSprite;
            curseKillButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CurseKillButton.png", 115f);
            return curseKillButtonSprite;
        }

        public static void clearAndReload() {
            warlock = null;
            currentTarget = null;
            curseVictim = null;
            curseVictimTarget = null;
            curseKillTarget = null;
            cooldown = CustomOptionHolder.warlockCooldown.getFloat();
            rootTime = CustomOptionHolder.warlockRootTime.getFloat();
        }

        public static void resetCurse() {
            HudManagerStartPatch.warlockCurseButton.Timer = HudManagerStartPatch.warlockCurseButton.MaxTimer;
            HudManagerStartPatch.warlockCurseButton.Sprite = Warlock.getCurseButtonSprite();
            HudManagerStartPatch.warlockCurseButton.killButtonManager.TimerText.color = Palette.EnabledColor;
            currentTarget = null;
            curseVictim = null;
            curseVictimTarget = null;
            curseKillTarget = null;
        }
    }

    public static class SecurityGuard {
        public static PlayerControl securityGuard;
        public static Color color = new Color32(195, 178, 95, byte.MaxValue);

        public static float cooldown = 30f;
        public static int remainingScrews = 7;
        public static int totalScrews = 7;
        public static int ventPrice = 1;
        public static int camPrice = 2;
        public static int placedCameras = 0;
        public static Vent ventTarget = null;

        private static Sprite closeVentButtonSprite;
        public static Sprite getCloseVentButtonSprite() {
            if (closeVentButtonSprite) return closeVentButtonSprite;
            closeVentButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CloseVentButton.png", 115f);
            return closeVentButtonSprite;
        }

        private static Sprite placeCameraButtonSprite;
        public static Sprite getPlaceCameraButtonSprite() {
            if (placeCameraButtonSprite) return placeCameraButtonSprite;
            placeCameraButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.PlaceCameraButton.png", 115f);
            return placeCameraButtonSprite;
        }

        private static Sprite animatedVentSealedSprite;
        public static Sprite getAnimatedVentSealedSprite() {
            if (animatedVentSealedSprite) return animatedVentSealedSprite;
            animatedVentSealedSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.AnimatedVentSealed.png", 160f);
            return animatedVentSealedSprite;
        }

        private static Sprite staticVentSealedSprite;
        public static Sprite getStaticVentSealedSprite() {
            if (staticVentSealedSprite) return staticVentSealedSprite;
            staticVentSealedSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.StaticVentSealed.png", 160f);
            return staticVentSealedSprite;
        }

        public static void clearAndReload() {
            securityGuard = null;
            ventTarget = null;
            placedCameras = 0;
            cooldown = CustomOptionHolder.securityGuardCooldown.getFloat();
            totalScrews = remainingScrews = Mathf.RoundToInt(CustomOptionHolder.securityGuardTotalScrews.getFloat());
            camPrice = Mathf.RoundToInt(CustomOptionHolder.securityGuardCamPrice.getFloat());
            ventPrice = Mathf.RoundToInt(CustomOptionHolder.securityGuardVentPrice.getFloat());
        }
    }

    public static class Arsonist {
        public static PlayerControl arsonist;
        public static Color color = new Color32(238, 112, 46, byte.MaxValue);

        public static float cooldown = 30f;
        public static float duration = 3f;
        public static bool triggerArsonistWin = false;

        public static PlayerControl currentTarget;
        public static PlayerControl douseTarget;
        public static List<PlayerControl> dousedPlayers = new List<PlayerControl>();

        private static Sprite douseSprite;
        public static Sprite getDouseSprite() {
            if (douseSprite) return douseSprite;
            douseSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.DouseButton.png", 115f);
            return douseSprite;
        }

        private static Sprite igniteSprite;
        public static Sprite getIgniteSprite() {
            if (igniteSprite) return igniteSprite;
            igniteSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.IgniteButton.png", 115f);
            return igniteSprite;
        }

        public static bool dousedEveryoneAlive() {
            return PlayerControl.AllPlayerControls.ToArray().All(x => { return x == Arsonist.arsonist || x.Data.IsDead || x.Data.Disconnected || Arsonist.dousedPlayers.Any(y => y.PlayerId == x.PlayerId); });
        }

        public static void clearAndReload() {
            arsonist = null;
            currentTarget = null;
            douseTarget = null; 
            triggerArsonistWin = false;
            dousedPlayers = new List<PlayerControl>();
            foreach (PoolablePlayer p in MapOptions.playerIcons.Values) {
                if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
            }
            cooldown = CustomOptionHolder.arsonistCooldown.getFloat();
            duration = CustomOptionHolder.arsonistDuration.getFloat();
        }
    }

    public static class Guesser {
        public static PlayerControl guesser;
        public static Color color = new Color32(255, 255, 0, byte.MaxValue);
        private static Sprite targetSprite;

        public static int remainingShots = 2;

        public static Sprite getTargetSprite() {
            if (targetSprite) return targetSprite;
            targetSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.TargetIcon.png", 150f);
            return targetSprite;
        }

        public static void clearAndReload() {
            guesser = null;
            
            remainingShots = Mathf.RoundToInt(CustomOptionHolder.guesserNumberOfShots.getFloat());
        }
    }

    public static class BountyHunter {
        public static PlayerControl bountyHunter;
        public static Color color = Palette.ImpostorRed;

        public static Arrow arrow;
        public static float bountyDuration = 30f;
        public static bool showArrow = true;
        public static float bountyKillCooldown = 0f;
        public static float punishmentTime = 15f;
        public static float arrowUpdateIntervall = 10f;

        public static float arrowUpdateTimer = 0f;
        public static float bountyUpdateTimer = 0f;
        public static PlayerControl bounty;
        public static TMPro.TextMeshPro cooldownText;

        public static void clearAndReload() {
            arrow = new Arrow(color);
            bountyHunter = null;
            bounty = null;
            arrowUpdateTimer = 0f;
            bountyUpdateTimer = 0f;
            if (arrow != null && arrow.arrow != null) UnityEngine.Object.Destroy(arrow.arrow);
            arrow = null;
            if (cooldownText != null && cooldownText.gameObject != null) UnityEngine.Object.Destroy(cooldownText.gameObject);
            cooldownText = null;
            foreach (PoolablePlayer p in MapOptions.playerIcons.Values) {
                if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
            }


            bountyDuration = CustomOptionHolder.bountyHunterBountyDuration.getFloat();
            bountyKillCooldown = CustomOptionHolder.bountyHunterReducedCooldown.getFloat();
            punishmentTime = CustomOptionHolder.bountyHunterPunishmentTime.getFloat();
            showArrow = CustomOptionHolder.bountyHunterShowArrow.getBool();
            arrowUpdateIntervall = CustomOptionHolder.bountyHunterArrowUpdateIntervall.getFloat();
        }
    }
}