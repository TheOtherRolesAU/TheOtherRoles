using System.Net;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using Reactor;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using Reactor.Unstrip;
using Reactor.Extensions;

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
            Spy.clearAndReload();
            Child.clearAndReload();
            BountyHunter.clearAndReload();
            Tracker.clearAndReload();
            Vampire.clearAndReload();
            Snitch.clearAndReload();
            Jackal.clearAndReload();
            Sidekick.clearAndReload();
        }

        public static class Jester {
            public static PlayerControl jester;
            public static Color color = new Color(255f / 255f, 84f / 255f, 167f / 255f, 1);

            public static void clearAndReload() {
                jester = null;
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
                buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.RepairButton.png", 100f);
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

            public static float cooldown = float.MaxValue;

            private static Sprite buttonSprite;
            public static Sprite getButtonSprite() {
                if (buttonSprite) return buttonSprite;
                buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CleanButton.png", 100f);
                return buttonSprite;
            }

            public static void clearAndReload() {
                janitor = null;
                cooldown = TheOtherRolesPlugin.janitorCooldown.GetValue();
            }
        }

        public static class Sheriff {
            public static PlayerControl sheriff;
            public static Color color = new Color(255f / 255f, 204f / 255f, 0f / 255f, 1);

            public static float cooldown = float.MaxValue;
            public static bool jesterCanDieToSheriff = false;

            public static PlayerControl currentTarget;

            public static void clearAndReload() {
                sheriff = null;
                currentTarget = null;
                cooldown = TheOtherRolesPlugin.sheriffCooldown.GetValue();
                jesterCanDieToSheriff = TheOtherRolesPlugin.jesterCanDieToSheriff.GetValue();
            }
        }

        public static class Lighter {
            public static PlayerControl lighter;
            public static Color color = new Color(250f / 255f, 204f / 255f, 37f / 255f, 1);
            
            public static float lighterVision = 2f;

            public static void clearAndReload() {
                lighter = null;
                lighterVision = TheOtherRolesPlugin.lighterVision.GetValue();
            }
        }

        public static class Detective {
            public static PlayerControl detective;
            public static Color color = new Color(2f / 255f, 61f / 255f, 156f / 255f, 1);

            public static float footprintIntervall = 1f;
            public static float footprintDuration = 1f;
            public static bool anonymousFootprints = false;

            public static float timer = 1f;

            public static void clearAndReload() {
                detective = null;
                anonymousFootprints = TheOtherRolesPlugin.detectiveAnonymousFootprints.GetValue();
                footprintIntervall = TheOtherRolesPlugin.detectiveFootprintIntervall.GetValue();
                footprintDuration = TheOtherRolesPlugin.detectiveFootprintDuration.GetValue();
                timer = footprintIntervall;
            }
        }
    }

    public static class TimeMaster {
        public static PlayerControl timeMaster;
        public static Color color = new Color(114f / 255f, 234f / 255f, 247f / 255f, 1);

        public static bool reviveDuringRewind = false;
        public static float rewindTime = 3f;
        public static float cooldown = float.MaxValue;

        public static bool isRewinding = false;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.RewindButton.png", 100f);
            return buttonSprite;
        }

        public static void clearAndReload() {
            timeMaster = null;
            isRewinding = false;
            reviveDuringRewind = TheOtherRolesPlugin.timeMasterReviveDuringRewind.GetValue();
            rewindTime = TheOtherRolesPlugin.timeMasterRewindTime.GetValue();
            cooldown = TheOtherRolesPlugin.timeMasterCooldown.GetValue();
        }
    }

    public static class Medic {
        public static PlayerControl medic;
        public static PlayerControl shielded;
        public static Color color = new Color(0f / 255f, 80f / 255f, 105f / 255f, 1);
        public static bool usedShield;

        public static float reportNameDuration = 10f;
        public static float reportColorDuration = 20f;
        public static int showShielded = 0;
        public static bool showAttemptToShielded = false;

        public static Color shieldedColor = new Color(0f / 255f, 221f / 255f, 255f / 255f, 1);
        public static PlayerControl currentTarget;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.ShieldButton.png", 100f);
            return buttonSprite;
        }

        public static void clearAndReload() {
            medic = null;
            if (shielded?.myRend?.material != null) shielded.myRend.material.SetFloat("_Outline", 0f);
            shielded = null;
            currentTarget = null;
            usedShield = false;
            shieldedColor = new Color(0f / 255f, 221f / 255f, 255f / 255f, 1);
            reportNameDuration = TheOtherRolesPlugin.medicReportNameDuration.GetValue();
            reportColorDuration = TheOtherRolesPlugin.medicReportColorDuration.GetValue();
            showShielded = TheOtherRolesPlugin.medicShowShielded.GetValue();
            showAttemptToShielded = TheOtherRolesPlugin.medicShowAttemptToShielded.GetValue();
        }
    }

    public static class Shifter {
        public static PlayerControl shifter;
        public static Color color = new Color(90f / 255f, 90f / 255f, 90f / 255f, 1);
    
        public static float cooldown = float.MaxValue;

        public static PlayerControl currentTarget;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.ShiftButton.png", 100f);
            return buttonSprite;
        }

        public static void clearAndReload() {
            shifter = null;
            currentTarget = null;
            cooldown = TheOtherRolesPlugin.shifterCooldown.GetValue();
        }
    }

    public static class Swapper {
        public static PlayerControl swapper;
        public static Color color = new Color(240f / 255f, 128f / 255f, 72f / 255f, 1);
        private static Sprite spriteCheck;

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
            return lover1 != null && lover2 != null && !lover1.Data.IsDead && !lover2.Data.IsDead && !lover1.Data.Disconnected && !lover2.Data.Disconnected && !notAckedExiledIsLover; // ADD NOT ACKED IS LOVER
        }

        public static bool existingWithImpLover() {
            return lover1 != null && lover2 != null && !lover1.Data.Disconnected && !lover2.Data.Disconnected && (lover1.Data.IsImpostor || lover2.Data.IsImpostor);
        }

        public static void clearAndReload() {
            lover1 = null;
            lover2 = null;
            notAckedExiledIsLover = false;
            bothDie = TheOtherRolesPlugin.loversBothDie.GetValue();
        }
    }

    public static class Seer {
        public static PlayerControl seer;
        public static Color color = new Color(60f / 255f, 181f / 255f, 100f / 255f, 1);
        public static Dictionary<PlayerControl, PlayerControl> revealedPlayers = new Dictionary<PlayerControl, PlayerControl>();

        public static float cooldown = float.MaxValue;
        public static int kindOfInfo = 0;
        public static int playersWithNotification = 0;
        public static float chanceOfSeeingRight = 100;

        public static PlayerControl currentTarget;
        
        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SeerButton.png", 100f);
            return buttonSprite;
        }

        public static void clearAndReload() {
            seer = null;
            revealedPlayers = new Dictionary<PlayerControl, PlayerControl>();
            cooldown = TheOtherRolesPlugin.seerCooldown.GetValue();
            kindOfInfo = TheOtherRolesPlugin.seerKindOfInfo.GetValue();
            playersWithNotification = TheOtherRolesPlugin.seerPlayersWithNotification.GetValue();
            chanceOfSeeingRight = TheOtherRolesPlugin.seerChanceOfSeeingRight.GetValue();
        }
    }

    public static class Morphling {
        public static PlayerControl morphling;
        public static Color color = Palette.ImpostorRed;
        private static Sprite sampleSprite;
        private static Sprite morphSprite;
    
        public static float cooldown = float.MaxValue;

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
            cooldown = TheOtherRolesPlugin.morphlingCooldown.GetValue();
        }

        public static Sprite getSampleSprite() {
            if (sampleSprite) return sampleSprite;
            sampleSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SampleButton.png", 100f);
            return sampleSprite;
        }

        public static Sprite getMorphSprite() {
            if (morphSprite) return morphSprite;
            morphSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.MorphButton.png", 100f);
            return morphSprite;
        }
    }

    public static class Camouflager {
        public static PlayerControl camouflager;
        public static Color color = Palette.ImpostorRed;
    
        public static float cooldown = float.MaxValue;
        public static float camouflageTimer = 0f;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CamoButton.png", 100f);
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
            cooldown = TheOtherRolesPlugin.camouflagerCooldown.GetValue();
        }
    }

    public static class Spy {
        public static PlayerControl spy;
        private static Sprite adminTableIcon;
        public static Color color = new Color(252f / 255f, 90f / 255f, 30f / 255f, 1);

        public static float cooldown = float.MaxValue;
        public static float duration = 10f;

        public static float spyTimer = 0f;

        public static Sprite getAdminTableIconSprite() {
            if (adminTableIcon) return adminTableIcon;
            adminTableIcon = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.AdminTableIcon.png", 350f);
            return adminTableIcon;
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SpyButton.png", 100f);
            return buttonSprite;
        }

        public static void clearAndReload() {
            spy = null;
            spyTimer = 0f;
            cooldown = TheOtherRolesPlugin.spyCooldown.GetValue();
            duration = TheOtherRolesPlugin.spySpyingDuration.GetValue();
        }
    }

    public static class Child {
        public static PlayerControl child;
        public static Color color = Color.white;
        
        public static float growingUpDuration = float.MaxValue;
        public static DateTime timeOfGrowthStart = DateTime.UtcNow;

        public static void clearAndReload() {
            child = null;
            growingUpDuration = TheOtherRolesPlugin.childGrowingUpDuration.GetValue();
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

    public static class BountyHunter {
        public static PlayerControl bountyHunter;
        public static Color color = new Color(237f / 255f, 101f / 255f, 59f / 255f, 1);

        public static bool notifyBounty = true;
        public static PlayerControl target;
    
        public static void clearAndReload() {
            bountyHunter = null;
            target = null;
            // notifyBounty = TheOtherRolesPlugin.bountyHunterNotifyBounty.GetValue();
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
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.TrackerButton.png", 100f);
            return buttonSprite;
        }

        public static void clearAndReload() {
            tracker = null;
            currentTarget = null;
            tracked = null;
            usedTracker = false;
            timeUntilUpdate = 0f;
            updateIntervall = TheOtherRolesPlugin.trackerUpdateIntervall.GetValue();
            if (arrow?.arrow != null) UnityEngine.Object.Destroy(arrow.arrow);
            arrow = new Arrow(Color.blue);
            if (arrow.arrow != null) arrow.arrow.SetActive(false);
        }
    }

    public static class Vampire {
        public static PlayerControl vampire;
        public static Color color = Palette.ImpostorRed;

        public static float delay = 10f;
        public static float cooldown = float.MaxValue;
        public static bool canKillNearGarlics = true;
        public static bool localPlacedGarlic = false;
        public static bool garlicsActive = true;

        public static PlayerControl currentTarget;
        public static PlayerControl bitten;
        public static bool targetNearGarlic = false;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.VampireButton.png", 100f);
            return buttonSprite;
        }

        private static Sprite garlicButtonSprite;
        public static Sprite getGarlicButtonSprite() {
            if (garlicButtonSprite) return garlicButtonSprite;
            garlicButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.GarlicButton.png", 100f);
            return garlicButtonSprite;
        }

        public static IEnumerator killWithDelay() {
            yield return new WaitForSeconds(delay);
            MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VampireTryKill, Hazel.SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(killWriter);
            RPCProcedure.vampireTryKill();
        }

        public static void clearAndReload() {
            vampire = null;
            bitten = null;
            targetNearGarlic = false;
            localPlacedGarlic = false;
            currentTarget = null;
            garlicsActive = TheOtherRolesPlugin.vampireSpawnRate.GetValue() > 0;
            delay = TheOtherRolesPlugin.vampireKillDelay.GetValue();
            cooldown = TheOtherRolesPlugin.vampireCooldown.GetValue();
            canKillNearGarlics = TheOtherRolesPlugin.vampireCanKillNearGarlics.GetValue();
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
            taskCountForImpostors = Mathf.RoundToInt(TheOtherRolesPlugin.snitchLeftTasksForImpostors.GetValue());
            snitch = null;
        }
    }

    public static class Jackal {
        public static PlayerControl jackal;
        public static Color color = new Color(0f / 255f, 180f / 255f, 235f / 255f, 1);
        public static PlayerControl fakeSidekick;

        public static PlayerControl currentTarget;
        public static List<PlayerControl> formerJackals = new List<PlayerControl>();
        
        public static float cooldown = float.MaxValue;
        public static float createSidekickCooldown = float.MaxValue;
        public static bool canUseVents = true;
        public static bool canCreateSidekick = true;
        public static Sprite buttonSprite;
        public static bool jackalPromotedFromSidekickCanCreateSidekick = true;
        public static bool canCreateSidekickFromImpostor = true;

        public static Sprite getSidekickButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SidekickButton.png", 100f);
            return buttonSprite;
        }

        public static void removeCurrentJackal() {
            formerJackals.Add(jackal);
            jackal = null;
            currentTarget = null;
            fakeSidekick = null;
            cooldown = TheOtherRolesPlugin.jackalKillCooldown.GetValue();
            createSidekickCooldown = TheOtherRolesPlugin.jackalCreateSidekickCooldown.GetValue();
        }

        public static void clearAndReload() {
            jackal = null;
            currentTarget = null;
            fakeSidekick = null;
            cooldown = TheOtherRolesPlugin.jackalKillCooldown.GetValue();
            createSidekickCooldown = TheOtherRolesPlugin.jackalCreateSidekickCooldown.GetValue();
            canUseVents = TheOtherRolesPlugin.jackalCanUseVents.GetValue();
            canCreateSidekick = TheOtherRolesPlugin.jackalCanCreateSidekick.GetValue();
            jackalPromotedFromSidekickCanCreateSidekick = TheOtherRolesPlugin.jackalPromotedFromSidekickCanCreateSidekick.GetValue();
            canCreateSidekickFromImpostor = TheOtherRolesPlugin.jackalCanCreateSidekickFromImpostor.GetValue();
            formerJackals.Clear();
        }
        
    }

    public static class Sidekick {
        public static PlayerControl sidekick;
        public static Color color = new Color(0f / 255f, 180f / 255f, 235f / 255f, 1);

        public static PlayerControl currentTarget;

        public static float cooldown = float.MaxValue;
        public static bool canUseVents = true;
        public static bool canKill = true;
        public static bool promotesToJackal = true;

        public static void clearAndReload() {
            sidekick = null;
            currentTarget = null;
            cooldown = TheOtherRolesPlugin.jackalKillCooldown.GetValue();
            canUseVents = TheOtherRolesPlugin.sidekickCanUseVents.GetValue();
            canKill = TheOtherRolesPlugin.sidekickCanKill.GetValue();
            promotesToJackal = TheOtherRolesPlugin.sidekickPromotesToJackal.GetValue();
        }
        
    }

}