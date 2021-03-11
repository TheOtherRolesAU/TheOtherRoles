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

namespace BonusRoles
{
    [HarmonyPatch]
    public static class BonusRoles
    {
        public static System.Random rnd = new System.Random();

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

            public static void clearAndReload() {
                janitor = null;
                cooldown = BonusRolesPlugin.janitorCooldown.GetValue();
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
                cooldown = BonusRolesPlugin.sheriffCooldown.GetValue();
                jesterCanDieToSheriff = BonusRolesPlugin.jesterCanDieToSheriff.GetValue();
            }
        }

        public static class Lighter {
            public static PlayerControl lighter;
            public static Color color = new Color(250f / 255f, 204f / 255f, 37f / 255f, 1);
            
            public static float lighterVision = 2f;

            public static void clearAndReload() {
                lighter = null;
                lighterVision = BonusRolesPlugin.lighterVision.GetValue();
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
                anonymousFootprints = BonusRolesPlugin.detectiveAnonymousFootprints.GetValue();
                footprintIntervall = BonusRolesPlugin.detectiveFootprintIntervall.GetValue();
                footprintDuration = BonusRolesPlugin.detectiveFootprintDuration.GetValue();
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

        public static void clearAndReload() {
            timeMaster = null;
            isRewinding = false;
            reviveDuringRewind = BonusRolesPlugin.timeMasterReviveDuringRewind.GetValue();
            rewindTime = BonusRolesPlugin.timeMasterRewindTime.GetValue();
            cooldown = BonusRolesPlugin.timeMasterCooldown.GetValue();
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

        public static void clearAndReload() {
            medic = null;
            shielded = null;
            currentTarget = null;
            usedShield = false;
            shieldedColor = new Color(0f / 255f, 221f / 255f, 255f / 255f, 1);
            reportNameDuration = BonusRolesPlugin.medicReportNameDuration.GetValue();
            reportColorDuration = BonusRolesPlugin.medicReportColorDuration.GetValue();
            showShielded = BonusRolesPlugin.medicShowShielded.GetValue();
            showAttemptToShielded = BonusRolesPlugin.medicShowAttemptToShielded.GetValue();
        }
    }

    public static class Shifter {
        public static PlayerControl shifter;
        public static Color color = new Color(90f / 255f, 90f / 255f, 90f / 255f, 1);
    
        public static float cooldown = float.MaxValue;

        public static PlayerControl currentTarget;

        public static void clearAndReload() {
            shifter = null;
            currentTarget = null;
            cooldown = BonusRolesPlugin.shifterCooldown.GetValue();
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
            spriteCheck = Helpers.loadSpriteFromResources("BonusRoles.Resources.SwapperCheck.png", 150f);
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
            return lover1 != null && lover2 != null && !lover1.Data.IsDead && !lover2.Data.IsDead && !notAckedExiledIsLover; // ADD NOT ACKED IS LOVER
        }

        public static bool existingWithImpLover() {
            return lover1 != null && lover2 != null && (lover1.Data.IsImpostor || lover2.Data.IsImpostor);
        }

        public static void clearAndReload() {
            lover1 = null;
            lover2 = null;
            notAckedExiledIsLover = false;
            bothDie = BonusRolesPlugin.loversBothDie.GetValue();
        }
    }

    public static class Seer {
        public static PlayerControl seer;
        public static Color color = new Color(60f / 255f, 181f / 255f, 100f / 255f, 1);
        public static List<PlayerControl> revealedPlayers = new List<PlayerControl>();

        public static float cooldown = float.MaxValue;
        public static int kindOfInfo = 0;
        public static int playersWithNotification = 0;

        public static PlayerControl currentTarget;
        
        public static void clearAndReload() {
            seer = null;
            revealedPlayers = new List<PlayerControl>();
            cooldown = BonusRolesPlugin.seerCooldown.GetValue();
            kindOfInfo = BonusRolesPlugin.seerKindOfInfo.GetValue();
            playersWithNotification = BonusRolesPlugin.seerPlayersWithNotification.GetValue();
        }
    }

    public static class Morphling {
        public static PlayerControl morphling;
        public static Color color = Palette.ImpostorRed;
        public static Sprite sampleSprite;
        public static Sprite morphSprite;
    
        public static float cooldown = float.MaxValue;

        public static PlayerControl currentTarget;
        public static PlayerControl sampledTarget;
        public static PlayerControl morphTarget;
        public static float morphTimer = 0f;


        public static void clearAndReload() {
            morphling = null;
            currentTarget = null;
            sampledTarget = null;
            morphTarget = null;
            morphTimer = 0f;
            cooldown = BonusRolesPlugin.morphlingCooldown.GetValue();
        }

        public static Sprite getSampleSprite() {
            if (sampleSprite) return sampleSprite;
            sampleSprite = Helpers.loadSpriteFromResources("BonusRoles.Resources.SampleButton.png", 100f);
            return sampleSprite;
        }

        public static Sprite getMorphSprite() {
            if (morphSprite) return morphSprite;
            morphSprite = Helpers.loadSpriteFromResources("BonusRoles.Resources.MorphButton.png", 100f);
            return morphSprite;
        }
    }

    public static class Camouflager {
        public static PlayerControl camouflager;
        public static Color color = Palette.ImpostorRed;
    
        public static float cooldown = float.MaxValue;
        public static float camouflageTimer = 0f;

        public static void clearAndReload() {
            camouflager = null;
            camouflageTimer = 0f;
            cooldown = BonusRolesPlugin.camouflagerCooldown.GetValue();
        }
    }

    public static class Spy {
        public static PlayerControl spy;
        public static Sprite adminTableIcon;
        public static Color color = new Color(252f / 255f, 90f / 255f, 30f / 255f, 1);

        public static Sprite getAdminTableIconSprite() {
            if (adminTableIcon) return adminTableIcon;
            adminTableIcon = Helpers.loadSpriteFromResources("BonusRoles.Resources.AdminTableIcon.png", 350f);
            return adminTableIcon;
        }

        public static void clearAndReload() {
            spy = null;
        }
    }

    public static class Child {
        public static PlayerControl child;
        public static Color color = Color.white;

        public static void clearAndReload() {
            child = null;
        }
    }
}