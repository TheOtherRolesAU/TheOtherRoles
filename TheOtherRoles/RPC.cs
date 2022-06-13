using HarmonyLib;
using Hazel;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.HudManagerStartPatch;
using static TheOtherRoles.GameHistory;
using static TheOtherRoles.MapOptions;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using TheOtherRoles.Utilities;

namespace TheOtherRoles
{
    enum RoleId {
        Jester,
        Prosecutor,
        Mayor,
        Portalmaker,
        Engineer,
        Sheriff,
        Deputy,
        Lighter,
        Godfather,
        Mafioso,
        Janitor,
        Detective,
        TimeMaster,
        Swooper,
        Veteren,
        Amnisiac,
        Medic,
        Shifter,
        Swapper,
        Seer,
        Morphling,
        Camouflager,
        Hacker,
        Tracker,
        Vampire,
        Snitch,
        Jackal,
        Sidekick,
        Eraser,
        Spy,
        Trickster,
        Cleaner,
        Undertaker,
        Warlock,
        SecurityGuard,
        Arsonist,
        EvilGuesser,
        NiceGuesser,
        BountyHunter,
        Miner,
        Vulture,
        Medium,
        Lawyer,
        Pursuer,
        Witch,
        Ninja,
        Blackmailer,
        Crewmate,
        Impostor,
        // Modifier ---
        Lover,
        Bait,
        Bloody,
        AntiTeleport,
        Tiebreaker,
        Sunglasses,
        Mini,
        Vip,
        Invert
    }

    enum CustomRPC
    {
        // Main Controls

        ResetVaribles = 60,
        ShareOptions,
        ForceEnd,
        SetRole,
        SetModifier,
        VersionHandshake,
        UseUncheckedVent,
        UncheckedMurderPlayer,
        UncheckedCmdReportDeadBody,
        UncheckedExilePlayer,
        DynamicMapOption,

        // Role functionality

        EngineerFixLights = 91,
        EngineerFixSubmergedOxygen,
        EngineerUsedRepair,
        CleanBody,
        Mine,
        DragBody,
        DropBody,
        SheriffKill,
        MedicSetShielded,
        ShieldedMurderAttempt,
        TimeMasterShield,
        TimeMasterRewindTime,
        VeterenAlert,
        VeterenKill,
    ShifterShift,
        SwapperSwap,
        MorphlingMorph,
        CamouflagerCamouflage,
        TrackerUsedTracker,
        VampireSetBitten,
        PlaceGarlic,
        DeputyUsedHandcuffs,
        DeputyPromotes,
        JackalCreatesSidekick,
        SidekickPromotes,
        ErasePlayerRoles,
        SetFutureErased,
        SetFutureShifted,
        SetFutureShielded,
        SetFutureSpelled,
        PlaceNinjaTrace,
        PlacePortal,
    AmnisiacTakeRole,
        UsePortal,
        PlaceJackInTheBox,
        LightsOut,
        PlaceCamera,
        SealVent,
        ArsonistWin,
        GuesserShoot,
        VultureWin,
        LawyerWin,
        LawyerSetTarget,
        LawyerPromotesToPursuer,
        BlackmailPlayer,
        UseAdminTime,
        UseCameraTime,
        UseVitalsTime,
        UnblackmailPlayer,
        SetBlanked,
        Bloody,
        SetFirstKill,
        Invert,
        SetTiebreak,
        SetInvisible,
        SetInvisibleGen,
        SetSwoop,
    ProsecutorChangesRole,
    ProsecutorSetTarget,
        ProsecutorToPursuer
    }

    public static class RPCProcedure {

        // Main Controls

        public static void resetVariables() {
            Garlic.clearGarlics();
            JackInTheBox.clearJackInTheBoxes();
            NinjaTrace.clearTraces();
            Portal.clearPortals();
            Bloodytrail.resetSprites();
            clearAndReloadMapOptions();
            clearAndReloadRoles();
            clearGameHistory();
            setCustomButtonCooldowns();
        }

        public static void ShareOptions(int numberOfOptions, MessageReader reader) {            
            try {
                for (int i = 0; i < numberOfOptions; i++) {
                    uint optionId = reader.ReadPackedUInt32();
                    uint selection = reader.ReadPackedUInt32();
                    CustomOption option = CustomOption.options.FirstOrDefault(option => option.id == (int)optionId);
                    option.updateSelection((int)selection);
                }
            } catch (Exception e) {
                TheOtherRolesPlugin.Logger.LogError("Error while deserializing options: " + e.Message);
            }
        }

        public static void forceEnd() {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls.GetFastEnumerator())
            {
                if (!player.Data.Role.IsImpostor)
                {
                    player.RemoveInfected();
                    player.MurderPlayer(player);
                    player.Data.IsDead = true;
                }
            }
        }

        public static void setRole(byte roleId, byte playerId) {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls.GetFastEnumerator())
                if (player.PlayerId == playerId) {
                    switch((RoleId)roleId) {
                    case RoleId.Jester:
                        Jester.jester = player;
                        break;
                    case RoleId.Blackmailer:
                        Blackmailer.blackmailer = player;
                        break;
                    case RoleId.Miner:
                        Miner.miner = player;
                        break;
                    case RoleId.Prosecutor:
                        Prosecutor.prosecutor = player;
                        break;
                    case RoleId.Mayor:
                        Mayor.mayor = player;
                        break;
                    case RoleId.Portalmaker:
                        Portalmaker.portalmaker = player;
                        break;
                        case RoleId.Engineer:
                        Engineer.engineer = player;
                        break;
                    case RoleId.Sheriff:
                        Sheriff.sheriff = player;
                        break;
                    case RoleId.Deputy:
                        Deputy.deputy = player;
                        break;
                    case RoleId.Lighter:
                        Lighter.lighter = player;
                        break;
                    case RoleId.Godfather:
                        Godfather.godfather = player;
                        break;
                    case RoleId.Mafioso:
                        Mafioso.mafioso = player;
                        break;
                    case RoleId.Janitor:
                        Janitor.janitor = player;
                        break;
                    case RoleId.Detective:
                        Detective.detective = player;
                        break;
                    case RoleId.TimeMaster:
                        TimeMaster.timeMaster = player;
                        break;
                    case RoleId.Amnisiac:
                        Amnisiac.amnisiac = player;
                        break;
                    case RoleId.Veteren:
                        Veteren.veteren = player;
                        break;
                    case RoleId.Medic:
                        Medic.medic = player;
                        break;
                    case RoleId.Shifter:
                        Shifter.shifter = player;
                        break;
                    case RoleId.Swapper:
                        Swapper.swapper = player;
                        break;
                    case RoleId.Seer:
                        Seer.seer = player;
                        break;
                    case RoleId.Morphling:
                        Morphling.morphling = player;
                        break;
                    case RoleId.Camouflager:
                        Camouflager.camouflager = player;
                        break;
                    case RoleId.Hacker:
                        Hacker.hacker = player;
                        break;
                    case RoleId.Tracker:
                        Tracker.tracker = player;
                        break;
                    case RoleId.Vampire:
                        Vampire.vampire = player;
                        break;
                    case RoleId.Snitch:
                        Snitch.snitch = player;
                        break;
                    case RoleId.Jackal:
                        Jackal.jackal = player;
                        break;
                    case RoleId.Swooper:
                        Swooper.swooper = player;
                        break;
                    case RoleId.Sidekick:
                        Sidekick.sidekick = player;
                        break;
                    case RoleId.Eraser:
                        Eraser.eraser = player;
                        break;
                    case RoleId.Spy:
                        Spy.spy = player;
                        break;
                    case RoleId.Trickster:
                        Trickster.trickster = player;
                        break;
                    case RoleId.Cleaner:
                        Cleaner.cleaner = player;
                        break;
                    case RoleId.Undertaker:
                        Undertaker.undertaker= player;
                        break;
                    case RoleId.Warlock:
                        Warlock.warlock = player;
                        break;
                    case RoleId.SecurityGuard:
                        SecurityGuard.securityGuard = player;
                        break;
                    case RoleId.Arsonist:
                        Arsonist.arsonist = player;
                        break;
                    case RoleId.EvilGuesser:
                        Guesser.evilGuesser = player;
                        break;
                    case RoleId.NiceGuesser:
                        Guesser.niceGuesser = player;
                        break;
                    case RoleId.BountyHunter:
                        BountyHunter.bountyHunter = player;
                        break;
                    case RoleId.Vulture:
                        Vulture.vulture = player;
                        break;
                    case RoleId.Medium:
                        Medium.medium = player;
                        break;
                    case RoleId.Lawyer:
                        Lawyer.lawyer = player;
                        break;
                    case RoleId.Pursuer:
                        Pursuer.pursuer = player;
                        break;
                    case RoleId.Witch:
                        Witch.witch = player;
                        break;
                    case RoleId.Ninja:
                        Ninja.ninja = player;
                        break;
                    }
                }
        }

        public static void setModifier(byte modifierId, byte playerId, byte flag) {
            PlayerControl player = Helpers.playerById(playerId); 
            switch ((RoleId)modifierId) {
                case RoleId.Bait:
                    Bait.bait.Add(player);
                    break;
                case RoleId.Lover:
                    if (flag == 0) Lovers.lover1 = player;
                    else Lovers.lover2 = player;
                    break;
                case RoleId.Bloody:
                    Bloody.bloody.Add(player);
                    break;
                case RoleId.AntiTeleport:
                    AntiTeleport.antiTeleport.Add(player);
                    break;
                case RoleId.Tiebreaker:
                    Tiebreaker.tiebreaker = player;
                    break;
                case RoleId.Sunglasses:
                    Sunglasses.sunglasses.Add(player);
                    break;
                case RoleId.Mini:
                    Mini.mini = player;
                    break;
                case RoleId.Vip:
                    Vip.vip.Add(player);
                    break;
                case RoleId.Invert:
                    Invert.invert.Add(player);
                    break;
            }
        }

        public static void versionHandshake(int major, int minor, int build, int revision, Guid guid, int clientId) {
            System.Version ver;
            if (revision < 0) 
                ver = new System.Version(major, minor, build);
            else 
                ver = new System.Version(major, minor, build, revision);
            GameStartManagerPatch.playerVersions[clientId] = new GameStartManagerPatch.PlayerVersion(ver, guid);
        }

        public static void useUncheckedVent(int ventId, byte playerId, byte isEnter) {
            PlayerControl player = Helpers.playerById(playerId);
            if (player == null) return;
            // Fill dummy MessageReader and call MyPhysics.HandleRpc as the corountines cannot be accessed
            MessageReader reader = new MessageReader();
            byte[] bytes = BitConverter.GetBytes(ventId);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            reader.Buffer = bytes;
            reader.Length = bytes.Length;

            JackInTheBox.startAnimation(ventId);
            player.MyPhysics.HandleRpc(isEnter != 0 ? (byte)19 : (byte)20, reader);
        }

        public static void uncheckedMurderPlayer(byte sourceId, byte targetId, byte showAnimation) {
            PlayerControl source = Helpers.playerById(sourceId);
            PlayerControl target = Helpers.playerById(targetId);
            if (source != null && target != null) {
                if (showAnimation == 0) KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                source.MurderPlayer(target);
            }
        }

        public static void uncheckedCmdReportDeadBody(byte sourceId, byte targetId) {
            PlayerControl source = Helpers.playerById(sourceId);
            var t = targetId == Byte.MaxValue ? null : Helpers.playerById(targetId).Data;
            if (source != null) source.ReportDeadBody(t);
        }

        public static void uncheckedExilePlayer(byte targetId) {
            PlayerControl target = Helpers.playerById(targetId);
            if (target != null) target.Exiled();
        }

        public static void dynamicMapOption(byte mapId) {
            PlayerControl.GameOptions.MapId = mapId;
        }

        // Role functionality

        public static void engineerFixLights() {
            SwitchSystem switchSystem = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
            switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
        }

        public static void engineerFixSubmergedOxygen() {
            SubmergedCompatibility.RepairOxygen();
        }

        public static void engineerUsedRepair() {
            Engineer.remainingFixes--;
        }

        public static void cleanBody(byte playerId) {
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++) {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == playerId) {
                    UnityEngine.Object.Destroy(array[i].gameObject);
                }
            }
        }

        public static void dragBody(byte playerId)
        {
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++) {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == playerId) {
                    Undertaker.deadBodyDraged = array[i];
                }
            }
        }

        public static void dropBody(byte playerId)
        {
            if (Undertaker.undertaker == null || Undertaker.deadBodyDraged == null) return;
            var deadBody = Undertaker.deadBodyDraged;
            Undertaker.deadBodyDraged = null;
            deadBody.transform.position = new Vector3(Undertaker.undertaker.GetTruePosition().x, Undertaker.undertaker.GetTruePosition().y, Undertaker.undertaker.transform.position.z);
        }

        public static void sheriffKill(byte targetId) {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == targetId)
                {
                    Sheriff.sheriff.MurderPlayer(player);
                    return;
                }
            }
        }

        public static void timeMasterRewindTime() {
            TimeMaster.shieldActive = false; // Shield is no longer active when rewinding
            if(TimeMaster.timeMaster != null && TimeMaster.timeMaster == PlayerControl.LocalPlayer) {
                resetTimeMasterButton();
            }
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(TimeMaster.rewindTime / 2, new Action<float>((p) => {
                if (p == 1f) FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = false;
            })));

            if (TimeMaster.timeMaster == null || PlayerControl.LocalPlayer == TimeMaster.timeMaster) return; // Time Master himself does not rewind

            TimeMaster.isRewinding = true;

            if (MapBehaviour.Instance)
                MapBehaviour.Instance.Close();
            if (Minigame.Instance)
                Minigame.Instance.ForceClose();
            PlayerControl.LocalPlayer.moveable = false;
        }

        public static void timeMasterShield() {
            TimeMaster.shieldActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(TimeMaster.shieldDuration, new Action<float>((p) => {
                if (p == 1f) TimeMaster.shieldActive = false;
            })));
        }

    public static void amnisiacTakeRole(byte targetId) {
        PlayerControl target = Helpers.playerById(targetId);
        PlayerControl amnisiac = Amnisiac.amnisiac;
            if (target == null || amnisiac == null) return;
            List<RoleInfo> targetInfo = RoleInfo.getRoleInfoForPlayer(target);
            RoleInfo roleInfo = targetInfo.Where(info => !info.isModifier).FirstOrDefault();
            switch((RoleId)roleInfo.roleId) {
                case RoleId.Crewmate:
                    Amnisiac.clearAndReload();
                    break;
                case RoleId.Impostor:
                    Helpers.turnToImpostor(Amnisiac.amnisiac);
                    Amnisiac.clearAndReload();
                    break;
                case RoleId.Jester:
                    if (Amnisiac.resetRole) Jester.clearAndReload();
                    Jester.jester = amnisiac;
                    Amnisiac.clearAndReload();
            Amnisiac.amnisiac = target;
                    break;

                case RoleId.Prosecutor:
            // Never reload Prosecutor
                    Prosecutor.prosecutor = amnisiac;
                    Amnisiac.clearAndReload();
            Amnisiac.amnisiac = target;
                    break;

                case RoleId.Mayor:
                    if (Amnisiac.resetRole) Mayor.clearAndReload();
                    Mayor.mayor = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Portalmaker:
                    if (Amnisiac.resetRole) Portalmaker.clearAndReload();
                    Portalmaker.portalmaker = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Engineer:
                    if (Amnisiac.resetRole) Engineer.clearAndReload();
                    Engineer.engineer = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Sheriff:
            // Never reload Sheriff
                    if (Sheriff.formerDeputy != null && Sheriff.formerDeputy == Sheriff.sheriff) Sheriff.formerDeputy = amnisiac; // Ensure amni gets handcuffs
                    Sheriff.sheriff = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Deputy:
                    if (Amnisiac.resetRole) Deputy.clearAndReload();
                    Deputy.deputy = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Lighter:
                    if (Amnisiac.resetRole) Lighter.clearAndReload();
                    Lighter.lighter = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Godfather:
                    Helpers.turnToImpostor(Amnisiac.amnisiac);
                    if (Amnisiac.resetRole) Godfather.clearAndReload();
                    Godfather.godfather = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Mafioso:
                    Helpers.turnToImpostor(Amnisiac.amnisiac);
                    if (Amnisiac.resetRole) Mafioso.clearAndReload();
                    Mafioso.mafioso = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Janitor:
                    Helpers.turnToImpostor(Amnisiac.amnisiac);
                    if (Amnisiac.resetRole) Janitor.clearAndReload();
                    Janitor.janitor = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Detective:
                    if (Amnisiac.resetRole) Detective.clearAndReload();
                    Detective.detective = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.TimeMaster:
                    if (Amnisiac.resetRole) TimeMaster.clearAndReload();
                    TimeMaster.timeMaster = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Veteren:
                    if (Amnisiac.resetRole) Veteren.clearAndReload();
                    Veteren.veteren = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Medic:
                    if (Amnisiac.resetRole) Medic.clearAndReload();
                    Medic.medic = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Shifter:
                    if (Amnisiac.resetRole) Shifter.clearAndReload();
                    Shifter.shifter = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Swapper:
                    if (Amnisiac.resetRole) Swapper.clearAndReload();
                    Swapper.swapper = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Seer:
                    if (Amnisiac.resetRole) Seer.clearAndReload();
                    Seer.seer = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Morphling:
                    Helpers.turnToImpostor(Amnisiac.amnisiac);
                    if (Amnisiac.resetRole) Morphling.clearAndReload();
                    Morphling.morphling = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Camouflager:
                    Helpers.turnToImpostor(Amnisiac.amnisiac);
                    if (Amnisiac.resetRole) Camouflager.clearAndReload();
                    Camouflager.camouflager = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Hacker:
                    if (Amnisiac.resetRole) Hacker.clearAndReload();
                    Hacker.hacker = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Tracker:
                    if (Amnisiac.resetRole) Tracker.clearAndReload();
                    Tracker.tracker = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Vampire:
                    Helpers.turnToImpostor(Amnisiac.amnisiac);
                    if (Amnisiac.resetRole) Vampire.clearAndReload();
                    Vampire.vampire = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Snitch:
                    if (Amnisiac.resetRole) Snitch.clearAndReload();
                    Snitch.snitch = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Jackal:
                    Jackal.jackal = amnisiac;
            Jackal.formerJackals.Add(target);
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Sidekick:
            Jackal.formerJackals.Add(target);
                    if (Amnisiac.resetRole) Sidekick.clearAndReload();
                    Sidekick.sidekick = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Eraser:
                    Helpers.turnToImpostor(Amnisiac.amnisiac);
                    if (Amnisiac.resetRole) Eraser.clearAndReload();
                    Eraser.eraser = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Spy:
                    if (Amnisiac.resetRole) Spy.clearAndReload();
                    Spy.spy = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Trickster:
                    Helpers.turnToImpostor(Amnisiac.amnisiac);
                    if (Amnisiac.resetRole) Trickster.clearAndReload();
                    Trickster.trickster = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Cleaner:
                    Helpers.turnToImpostor(Amnisiac.amnisiac);
                    if (Amnisiac.resetRole) Cleaner.clearAndReload();
                    Cleaner.cleaner = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Warlock:
                    Helpers.turnToImpostor(Amnisiac.amnisiac);
                    if (Amnisiac.resetRole) Warlock.clearAndReload();
                    Warlock.warlock = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.SecurityGuard:
                    if (Amnisiac.resetRole) SecurityGuard.clearAndReload();
                    SecurityGuard.securityGuard = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Arsonist:
                    if (PlayerControl.LocalPlayer == Amnisiac.amnisiac) {
                       Helpers.showFlash(Palette.ImpostorRed, duration: 1f);
                    }
                    break;

                case RoleId.EvilGuesser:
                    Helpers.turnToImpostor(Amnisiac.amnisiac);
            // Never Reload Guesser
                    Guesser.evilGuesser = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.NiceGuesser:
            // Never Reload Guesser
                    Guesser.niceGuesser = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.BountyHunter:
                    Helpers.turnToImpostor(Amnisiac.amnisiac);
                    if (PlayerControl.LocalPlayer == Amnisiac.amnisiac) {
                       Helpers.showFlash(Palette.ImpostorRed, duration: 1f);
                    }
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Vulture:
                    if (Amnisiac.resetRole) Vulture.clearAndReload();
                    Vulture.vulture = amnisiac;
                    Amnisiac.clearAndReload();
            Amnisiac.amnisiac = target;
                    break;

                case RoleId.Medium:
                    if (Amnisiac.resetRole) Medium.clearAndReload();
                    Medium.medium = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Lawyer:
                    // Never reset Lawyer
                    Lawyer.lawyer = amnisiac;
                    Amnisiac.clearAndReload();
               Amnisiac.amnisiac = target;
                    break;

                case RoleId.Pursuer:
                    if (Amnisiac.resetRole) Pursuer.clearAndReload();
                    Pursuer.pursuer = amnisiac;
                    Amnisiac.clearAndReload();
                    Amnisiac.amnisiac = target;
                    break;

                case RoleId.Witch:
                    Helpers.turnToImpostor(Amnisiac.amnisiac);
                    if (Amnisiac.resetRole) Witch.clearAndReload();
                    Witch.witch = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Ninja:
                    Helpers.turnToImpostor(Amnisiac.amnisiac);
                    if (Amnisiac.resetRole) Ninja.clearAndReload();
                    Ninja.ninja = amnisiac;
                    Amnisiac.clearAndReload();
                    break;

                case RoleId.Blackmailer:
                    Helpers.turnToImpostor(Amnisiac.amnisiac);
                    if (Amnisiac.resetRole) Blackmailer.clearAndReload();
                    Blackmailer.blackmailer = amnisiac;
                    Amnisiac.clearAndReload();
                    break;
        }
    }

        public static void veterenAlert() {
            Veteren.alertActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Veteren.alertDuration, new Action<float>((p) => {
                if (p == 1f) Veteren.alertActive = false;
            })));
        }

        public static void veterenKill(byte targetId) {
      if (PlayerControl.LocalPlayer == Veteren.veteren) {
            PlayerControl player = Helpers.playerById(targetId);
          Helpers.checkMuderAttemptAndKill(Veteren.veteren, player);
          }
        }

        public static void medicSetShielded(byte shieldedId) {
            Medic.usedShield = true;
            Medic.shielded = Helpers.playerById(shieldedId);
            Medic.futureShielded = null;
        }

        public static void shieldedMurderAttempt(byte blank) {
            if (Medic.shielded == null || Medic.medic == null) return;
            if (!Medic.unbreakableShield && blank != 0) {
                Medic.shielded = null;
                Helpers.resetKill(blank);
                return;
            }
            bool isShieldedAndShow = Medic.shielded == PlayerControl.LocalPlayer && Medic.showAttemptToShielded;
            isShieldedAndShow = isShieldedAndShow && (Medic.meetingAfterShielding || !Medic.showShieldAfterMeeting);  // Dont show attempt, if shield is not shown yet
            bool isMedicAndShow = Medic.medic == PlayerControl.LocalPlayer && Medic.showAttemptToMedic;

            if (isShieldedAndShow || isMedicAndShow) Helpers.showFlash(Palette.ImpostorRed, duration: 0.5f);
        }

        public static void shifterShift(byte targetId) {
            PlayerControl oldShifter = Shifter.shifter;
            PlayerControl player = Helpers.playerById(targetId);
            if (player == null || oldShifter == null) return;

            Shifter.futureShift = null;
            Shifter.clearAndReload();

            // Suicide (exile) when impostor or impostor variants
            if (player.Data.Role.IsImpostor ||  // Don't shift on Imps
                player == Jackal.jackal || //  Don't shift on Jackal
                player == Swooper.swooper || // Don't shift on Swooper
                player == Sidekick.sidekick || // Don't shift on Sidekick 
                Jackal.formerJackals.Contains(player) || // Don't shift on former jackals
                player == Jester.jester || // Don't shift on Jester
                player == Arsonist.arsonist || // Don't shift on Arso
                player == Vulture.vulture || // Don't shift on Vulture
                player == Lawyer.lawyer || // Don't shift on Lawyer
                player == Prosecutor.prosecutor || // Don't shift on Prosecutor
                player == Amnisiac.amnisiac) { // Don't shift on Amnesiac
                    oldShifter.Exiled();
                    return;
            }

            if (Shifter.shiftModifiers) {
                // Switch shield
                if (Medic.shielded != null && Medic.shielded == player) {
                    Medic.shielded = oldShifter;
                } else if (Medic.shielded != null && Medic.shielded == oldShifter) {
                    Medic.shielded = player;
                }
                // Shift Lovers Role
                if (Lovers.lover1 != null && oldShifter == Lovers.lover1) Lovers.lover1 = player;
                else if (Lovers.lover1 != null && player == Lovers.lover1) Lovers.lover1 = oldShifter;

                if (Lovers.lover2 != null && oldShifter == Lovers.lover2) Lovers.lover2 = player;
                else if (Lovers.lover2 != null && player == Lovers.lover2) Lovers.lover2 = oldShifter;

                // TODO other Modifiers?
            }

            // Shift role
            if (Mayor.mayor != null && Mayor.mayor == player)
                Mayor.mayor = oldShifter;
            if (Portalmaker.portalmaker != null && Portalmaker.portalmaker == player)
                Portalmaker.portalmaker = oldShifter;
            if (Engineer.engineer != null && Engineer.engineer == player)
                Engineer.engineer = oldShifter;
            if (Sheriff.sheriff != null && Sheriff.sheriff == player) {
                if (Sheriff.formerDeputy != null && Sheriff.formerDeputy == Sheriff.sheriff) Sheriff.formerDeputy = oldShifter;  // Shifter also shifts info on promoted deputy (to get handcuffs)
                Sheriff.sheriff = oldShifter;
            }
            if (Deputy.deputy != null && Deputy.deputy == player)
                Deputy.deputy = oldShifter;
            if (Lighter.lighter != null && Lighter.lighter == player)
                Lighter.lighter = oldShifter;
            if (Detective.detective != null && Detective.detective == player)
                Detective.detective = oldShifter;
            if (TimeMaster.timeMaster != null && TimeMaster.timeMaster == player)
                TimeMaster.timeMaster = oldShifter;
            if (Amnisiac.amnisiac != null && Amnisiac.amnisiac == player)
                Amnisiac.amnisiac = oldShifter;
            if (Veteren.veteren != null && Veteren.veteren == player)
                Veteren.veteren = oldShifter;
            if (Medic.medic != null && Medic.medic == player)
                Medic.medic = oldShifter;
            if (Swapper.swapper != null && Swapper.swapper == player)
                Swapper.swapper = oldShifter;
            if (Seer.seer != null && Seer.seer == player)
                Seer.seer = oldShifter;
            if (Hacker.hacker != null && Hacker.hacker == player)
                Hacker.hacker = oldShifter;
            if (Tracker.tracker != null && Tracker.tracker == player)
                Tracker.tracker = oldShifter;
            if (Snitch.snitch != null && Snitch.snitch == player)
                Snitch.snitch = oldShifter;
            if (Spy.spy != null && Spy.spy == player)
                Spy.spy = oldShifter;
            if (SecurityGuard.securityGuard != null && SecurityGuard.securityGuard == player)
                SecurityGuard.securityGuard = oldShifter;
            if (Guesser.niceGuesser != null && Guesser.niceGuesser == player)
                Guesser.niceGuesser = oldShifter;
                
            if (Medium.medium != null && Medium.medium == player)
                Medium.medium = oldShifter;

            // Set cooldowns to max for both players
            if (PlayerControl.LocalPlayer == oldShifter || PlayerControl.LocalPlayer == player)
                CustomButton.ResetAllCooldowns();
        }

        public static void swapperSwap(byte playerId1, byte playerId2) {
            if (MeetingHud.Instance) {
                Swapper.playerId1 = playerId1;
                Swapper.playerId2 = playerId2;
            }
        }

        public static void morphlingMorph(byte playerId) {  
            PlayerControl target = Helpers.playerById(playerId);
            if (Morphling.morphling == null || target == null) return;

            Morphling.morphTimer = Morphling.duration;
            Morphling.morphTarget = target;
            if (Camouflager.camouflageTimer <= 0f)
                Morphling.morphling.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
        }

        public static void camouflagerCamouflage(byte setTimer) {
            if (Helpers.isActiveCamoComms() && setTimer != 2) return;
            if (Helpers.isCamoComms()) Camouflager.camoComms = true;
            if (Camouflager.camouflager == null && !Camouflager.camoComms) return;

            if (setTimer == 1) Camouflager.camouflageTimer = Camouflager.duration;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls.GetFastEnumerator())
                player.setLook("", 6, "", "", "", "");
        }

        public static void vampireSetBitten(byte targetId, byte performReset) {
            if (performReset != 0) {
                Vampire.bitten = null;
                return;
            }

            if (Vampire.vampire == null) return;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls.GetFastEnumerator()) {
                if (player.PlayerId == targetId && !player.Data.IsDead) {
                        Vampire.bitten = player;
                }
            }
        }

        public static void placeGarlic(byte[] buff) {
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0*sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1*sizeof(float));
            new Garlic(position);
        }

        public static void trackerUsedTracker(byte targetId) {
            Tracker.usedTracker = true;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls.GetFastEnumerator())
                if (player.PlayerId == targetId)
                    Tracker.tracked = player;
        }

        public static void deputyUsedHandcuffs(byte targetId)
        {
            Deputy.remainingHandcuffs--;
            Deputy.handcuffedPlayers.Add(targetId);
        }

        public static void deputyPromotes()
        {
            if (Deputy.deputy != null) {  // Deputy should never be null here, but there appeared to be a race condition during testing, which was removed.
                Sheriff.replaceCurrentSheriff(Deputy.deputy);
                Sheriff.formerDeputy = Deputy.deputy;
                Deputy.deputy = null;
                // No clear and reload, as we need to keep the number of handcuffs left etc
            }
        }

        public static void jackalCreatesSidekick(byte targetId) {
            PlayerControl player = Helpers.playerById(targetId);
            if (player == null) return;

            if (!Jackal.canCreateSidekickFromImpostor && player.Data.Role.IsImpostor) {
                Jackal.fakeSidekick = player;
            } else {
                bool wasSpy = Spy.spy != null && player == Spy.spy;
                bool wasImpostor = player.Data.Role.IsImpostor;  // This can only be reached if impostors can be sidekicked.
                FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
                if (player == Lawyer.lawyer && Lawyer.target != null)
                {
                    Transform playerInfoTransform = Lawyer.target.nameText.transform.parent.FindChild("Info");
                    TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (playerInfo != null) playerInfo.text = "";
                }
                erasePlayerRoles(player.PlayerId, true);
                Sidekick.sidekick = player;
                if (Prosecutor.prosecutor != null && Prosecutor.target == player) {
                          prosecutorChangesRole();
                }
                if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) PlayerControl.LocalPlayer.moveable = true;
                if (wasSpy || wasImpostor) Sidekick.wasTeamRed = true;
                Sidekick.wasSpy = wasSpy;
                Sidekick.wasImpostor = wasImpostor;
            }
            Jackal.canCreateSidekick = false;
        }

        public static void sidekickPromotes() {
            Jackal.removeCurrentJackal();
            Jackal.jackal = Sidekick.sidekick;
            Jackal.canCreateSidekick = Jackal.jackalPromotedFromSidekickCanCreateSidekick;
            Jackal.wasTeamRed = Sidekick.wasTeamRed;
            Jackal.wasSpy = Sidekick.wasSpy;
            Jackal.wasImpostor = Sidekick.wasImpostor;
            Sidekick.clearAndReload();
            return;
        }
        
        public static void erasePlayerRoles(byte playerId, bool ignoreModifier = false) {
            PlayerControl player = Helpers.playerById(playerId);
            if (player == null) return;

            // Crewmate roles
            if (player == Mayor.mayor) Mayor.clearAndReload();
            if (player == Portalmaker.portalmaker) Portalmaker.clearAndReload();
            if (player == Engineer.engineer) Engineer.clearAndReload();
            if (player == Sheriff.sheriff) Sheriff.clearAndReload();
            if (player == Deputy.deputy) Deputy.clearAndReload();
            if (player == Lighter.lighter) Lighter.clearAndReload();
            if (player == Detective.detective) Detective.clearAndReload();
            if (player == TimeMaster.timeMaster) TimeMaster.clearAndReload();
            if (player == Amnisiac.amnisiac) Amnisiac.clearAndReload();
            if (player == Veteren.veteren) Veteren.clearAndReload();
            if (player == Medic.medic) Medic.clearAndReload();
            if (player == Shifter.shifter) Shifter.clearAndReload();
            if (player == Seer.seer) Seer.clearAndReload();
            if (player == Hacker.hacker) Hacker.clearAndReload();
            if (player == Tracker.tracker) Tracker.clearAndReload();
            if (player == Snitch.snitch) Snitch.clearAndReload();
            if (player == Swapper.swapper) Swapper.clearAndReload();
            if (player == Spy.spy) Spy.clearAndReload();
            if (player == SecurityGuard.securityGuard) SecurityGuard.clearAndReload();
            if (player == Medium.medium) Medium.clearAndReload();

            // Impostor roles
            if (player == Morphling.morphling) Morphling.clearAndReload();
            if (player == Camouflager.camouflager) Camouflager.clearAndReload();
            if (player == Godfather.godfather) Godfather.clearAndReload();
            if (player == Mafioso.mafioso) Mafioso.clearAndReload();
            if (player == Janitor.janitor) Janitor.clearAndReload();
            if (player == Vampire.vampire) Vampire.clearAndReload();
            if (player == Eraser.eraser) Eraser.clearAndReload();
            if (player == Trickster.trickster) Trickster.clearAndReload();
            if (player == Cleaner.cleaner) Cleaner.clearAndReload();
            if (player == Undertaker.undertaker) Undertaker.clearAndReload();
            if (player == Warlock.warlock) Warlock.clearAndReload();
            if (player == Witch.witch) Witch.clearAndReload();
            if (player == Ninja.ninja) Ninja.clearAndReload();
            if (player == Blackmailer.blackmailer) Blackmailer.clearAndReload();

            // Other roles
            if (player == Jester.jester) Jester.clearAndReload();
            if (player == Prosecutor.prosecutor) Prosecutor.clearAndReload();
            if (player == Swooper.swooper) Swooper.clearAndReload();
            if (player == Miner.miner) Miner.clearAndReload();
            if (player == Arsonist.arsonist) Arsonist.clearAndReload();
            if (Guesser.isGuesser(player.PlayerId)) Guesser.clear(player.PlayerId);
            if (player == Jackal.jackal) { // Promote Sidekick and hence override the the Jackal or erase Jackal
                if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead) {
                    RPCProcedure.sidekickPromotes();
                } else {
                    Jackal.clearAndReload();
                }
            }
            if (player == Sidekick.sidekick) Sidekick.clearAndReload();
            if (player == BountyHunter.bountyHunter) BountyHunter.clearAndReload();
            if (player == Vulture.vulture) Vulture.clearAndReload();
            if (player == Lawyer.lawyer) Lawyer.clearAndReload();
            if (player == Pursuer.pursuer) Pursuer.clearAndReload();

            // Modifier
            if (!ignoreModifier)
            {
                if (player == Lovers.lover1 || player == Lovers.lover2) Lovers.clearAndReload(); // The whole Lover couple is being erased
                if (Bait.bait.Any(x => x.PlayerId == player.PlayerId)) Bait.bait.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (Bloody.bloody.Any(x => x.PlayerId == player.PlayerId)) Bloody.bloody.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (AntiTeleport.antiTeleport.Any(x => x.PlayerId == player.PlayerId)) AntiTeleport.antiTeleport.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (Sunglasses.sunglasses.Any(x => x.PlayerId == player.PlayerId)) Sunglasses.sunglasses.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (player == Tiebreaker.tiebreaker) Tiebreaker.clearAndReload();
                if (player == Mini.mini) Mini.clearAndReload();
                if (Vip.vip.Any(x => x.PlayerId == player.PlayerId)) Vip.vip.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (Invert.invert.Any(x => x.PlayerId == player.PlayerId)) Invert.invert.RemoveAll(x => x.PlayerId == player.PlayerId);
            }
        }

        public static void setFutureErased(byte playerId) {
            PlayerControl player = Helpers.playerById(playerId);
            if (Eraser.futureErased == null) 
                Eraser.futureErased = new List<PlayerControl>();
            if (player != null) {
                Eraser.futureErased.Add(player);
            }
        }

        public static void setFutureShifted(byte playerId) {
            Shifter.futureShift = Helpers.playerById(playerId);
        }

        public static void setFutureShielded(byte playerId) {
            Medic.futureShielded = Helpers.playerById(playerId);
            Medic.usedShield = true;
        }

        public static void setFutureSpelled(byte playerId) {
            PlayerControl player = Helpers.playerById(playerId);
            if (Witch.futureSpelled == null)
                Witch.futureSpelled = new List<PlayerControl>();
            if (player != null) {
                Witch.futureSpelled.Add(player);
            }
        }

        public static void placeNinjaTrace(byte[] buff) {
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            new NinjaTrace(position, Ninja.traceTime);
        }

        public static void setInvisible(byte playerId, byte flag)
        {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            if (flag == byte.MaxValue)
            {
                target.MyRend.color = Color.white;
                target.setDefaultLook();
                Ninja.isInvisble = false;
                return;
            }

            target.setLook("", 6, "", "", "", "");
            Color color = Color.clear;           
            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor || PlayerControl.LocalPlayer.Data.IsDead) color.a = 0.1f;
            target.MyRend.color = color;
            Ninja.invisibleTimer = Ninja.invisibleDuration;
            Ninja.isInvisble = true;
        }
        
        public static void Mine(int ventId, PlayerControl role, byte[] buff, float zAxis) {
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0*sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1*sizeof(float));
            
            var ventPrefab = UnityEngine.Object.FindObjectOfType<Vent>();
            var vent = UnityEngine.Object.Instantiate(ventPrefab, ventPrefab.transform.parent);
            vent.Id = ventId;
            vent.transform.position = new Vector3(position.x, position.y, zAxis);

            if (Miner.Vents.Count > 0) {
                var leftVent = Miner.Vents[^1];
                vent.Left = leftVent;
                leftVent.Right = vent;
            } else {
                vent.Left = null;
            }
            vent.Right = null;
            vent.Center = null;
            var allVents = ShipStatus.Instance.AllVents.ToList();
            allVents.Add(vent);
            ShipStatus.Instance.AllVents = allVents.ToArray();
            Miner.Vents.Add(vent);
            Miner.LastMined = DateTime.UtcNow;
            if (SubmergedCompatibility.IsSubmerged) {
                vent.gameObject.layer = 12;
                vent.gameObject.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover); // just in case elevator vent is not blocked
                if (vent.gameObject.transform.position.y > -7) vent.gameObject.transform.position = new Vector3(vent.gameObject.transform.position.x, vent.gameObject.transform.position.y, 0.03f);
                else {
                    vent.gameObject.transform.position = new Vector3(vent.gameObject.transform.position.x, vent.gameObject.transform.position.y, 0.0009f);
                    vent.gameObject.transform.localPosition = new Vector3(vent.gameObject.transform.localPosition.x, vent.gameObject.transform.localPosition.y, -0.003f);
                }
            }
        }

        public static void setSwoop(byte playerId, byte flag) {        
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            if (flag == byte.MaxValue) {
                if (Camouflager.camouflageTimer > 0f) {
                    camouflagerCamouflage((byte)2);
                } else {
                    target.MyRend.color = Color.white;
                    target.setDefaultLook();
                }
                Swooper.isInvisable = false;
                return;
            } else {
                target.setLook("", 6, "", "", "", "");
                Color color = Color.clear;           
                if (Swooper.swooper == PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data.IsDead || (Swooper.swooper == Jackal.jackal && Sidekick.sidekick == PlayerControl.LocalPlayer)) color.a = 0.1f;
                target.MyRend.color = color;
                Swooper.swoopTimer = Swooper.duration;
            Swooper.isInvisable = true;
            }
        }


        public static void setInvisibleGen(byte playerId, byte flag)
        {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            if (flag == byte.MaxValue)
            {
                target.MyRend.color = Color.white;
                target.setDefaultLook();
                return;
            }

            target.setLook("", 6, "", "", "", "");
            Color color = Color.clear;           
            if (PlayerControl.LocalPlayer.Data.IsDead) color.a = 0.1f;
            target.MyRend.color = color;
        }

        public static void placePortal(byte[] buff) {
            Vector3 position = Vector2.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            new Portal(position);
        }

        public static void usePortal(byte playerId) {
            Portal.startTeleport(playerId);
        }

        public static void placeJackInTheBox(byte[] buff) {
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0*sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1*sizeof(float));
            new JackInTheBox(position);
        }

        public static void lightsOut() {
            Trickster.lightsOutTimer = Trickster.lightsOutDuration;
            // If the local player is impostor indicate lights out
            if(PlayerControl.LocalPlayer.Data.Role.IsImpostor) {
                new CustomMessage("Lights are out", Trickster.lightsOutDuration);
            }
        }

        public static void placeCamera(byte[] buff) {
            var referenceCamera = UnityEngine.Object.FindObjectOfType<SurvCamera>(); 
            if (referenceCamera == null) return; // Mira HQ

            SecurityGuard.remainingScrews -= SecurityGuard.camPrice;
            SecurityGuard.placedCameras++;

            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0*sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1*sizeof(float));

            var camera = UnityEngine.Object.Instantiate<SurvCamera>(referenceCamera);
            camera.transform.position = new Vector3(position.x, position.y, referenceCamera.transform.position.z - 1f);
            camera.CamName = $"Security Camera {SecurityGuard.placedCameras}";
            camera.Offset = new Vector3(0f, 0f, camera.Offset.z);
            if (PlayerControl.GameOptions.MapId == 2 || PlayerControl.GameOptions.MapId == 4) camera.transform.localRotation = new Quaternion(0, 0, 1, 1); // Polus and Airship 

            if (SubmergedCompatibility.IsSubmerged) {
                // remove 2d box collider of console, so that no barrier can be created. (irrelevant for now, but who knows... maybe we need it later)
                var fixConsole = camera.transform.FindChild("FixConsole");
                if (fixConsole != null) {
                    var boxCollider = fixConsole.GetComponent<BoxCollider2D>();
                    if (boxCollider != null) UnityEngine.Object.Destroy(boxCollider);
                }
            }


            if (PlayerControl.LocalPlayer == SecurityGuard.securityGuard) {
                camera.gameObject.SetActive(true);
                camera.gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
            } else {
                camera.gameObject.SetActive(false);
            }
            MapOptions.camerasToAdd.Add(camera);
        }

        public static void sealVent(int ventId) {
            Vent vent = MapUtilities.CachedShipStatus.AllVents.FirstOrDefault((x) => x != null && x.Id == ventId);
            if (vent == null) return;

            SecurityGuard.remainingScrews -= SecurityGuard.ventPrice;
            if (PlayerControl.LocalPlayer == SecurityGuard.securityGuard) {
                PowerTools.SpriteAnim animator = vent.GetComponent<PowerTools.SpriteAnim>(); 
                animator?.Stop();
                vent.EnterVentAnim = vent.ExitVentAnim = null;
                vent.myRend.sprite = animator == null ? SecurityGuard.getStaticVentSealedSprite() : SecurityGuard.getAnimatedVentSealedSprite();
                if (SubmergedCompatibility.IsSubmerged && vent.Id == 0) vent.myRend.sprite = SecurityGuard.getSubmergedCentralUpperSealedSprite();
                if (SubmergedCompatibility.IsSubmerged && vent.Id == 14) vent.myRend.sprite = SecurityGuard.getSubmergedCentralLowerSealedSprite();
                vent.myRend.color = new Color(1f, 1f, 1f, 0.5f);
                vent.name = "FutureSealedVent_" + vent.name;
            }

            MapOptions.ventsToSeal.Add(vent);
        }

        public static void arsonistWin() {
            Arsonist.triggerArsonistWin = true;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls.GetFastEnumerator()) {
                if (p != Arsonist.arsonist) p.Exiled();
            }
        }

        public static void vultureWin() {
            Vulture.triggerVultureWin = true;
        }

        public static void lawyerWin() {
            Lawyer.triggerLawyerWin = true;
        }

        public static void lawyerSetTarget(byte playerId) {
            Lawyer.target = Helpers.playerById(playerId);
        }

        public static void lawyerPromotesToPursuer() {
            PlayerControl player = Lawyer.lawyer;
            PlayerControl client = Lawyer.target;
            Lawyer.clearAndReload(false);
            Pursuer.pursuer = player;

            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId && client != null) {
                    Transform playerInfoTransform = client.nameText.transform.parent.FindChild("Info");
                    TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (playerInfo != null) playerInfo.text = "";
            }
        }

        // Prosecutor
        public static void prosecutorSetTarget(byte playerId) {
            Prosecutor.target = Helpers.playerById(playerId);
        }

        public static void prosecutorChangesRole() {
            PlayerControl player = Prosecutor.prosecutor;
            PlayerControl target = Prosecutor.target;
            Prosecutor.clearAndReload();
            Lawyer.lawyer = player;
            Lawyer.target = target;
        }

        public static void prosecutorToPursuer() {
            if (Prosecutor.prosecutor == null) return;
            PlayerControl player = Prosecutor.prosecutor;
            Prosecutor.clearAndReload();
            Pursuer.pursuer = player;
            Pursuer.wasProsecutor = true;
        }

        public static void guesserShoot(byte killerId, byte dyingTargetId, byte guessedTargetId, byte guessedRoleId) {
            PlayerControl dyingTarget = Helpers.playerById(dyingTargetId);
            if (dyingTarget == null ) return;
            if (Lawyer.target != null && dyingTarget == Lawyer.target) Lawyer.targetWasGuessed = true;  // Lawyer shouldn't be exiled with the client for guesses
            if (Prosecutor.target != null && dyingTarget == Prosecutor.target) Prosecutor.targetWasGuessed = true;  // Prosecutor needs to turn into Pursuer if client is guessed

            dyingTarget.Exiled();
            PlayerControl dyingLoverPartner = Lovers.bothDie ? dyingTarget.getPartner() : null; // Lover check
            if (Lawyer.target != null && dyingLoverPartner == Lawyer.target) Lawyer.targetWasGuessed = true;  // Lawyer shouldn't be exiled with the client for guesses
            dyingTarget.Exiled();
            byte partnerId = dyingLoverPartner != null ? dyingLoverPartner.PlayerId : dyingTargetId;

            Guesser.remainingShots(killerId, true);
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(dyingTarget.KillSfx, false, 0.8f);
            if (MeetingHud.Instance) {
                foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates) {
                    if (pva.TargetPlayerId == dyingTargetId || pva.TargetPlayerId == partnerId) {
                        pva.SetDead(pva.DidReport, true);
                        pva.Overlay.gameObject.SetActive(true);
                    }

                    //Give players back their vote if target is shot dead
                    if (pva.VotedFor != dyingTargetId || pva.VotedFor != partnerId) continue;
                    pva.UnsetVote();
                    var voteAreaPlayer = Helpers.playerById(pva.TargetPlayerId);
                    if (!voteAreaPlayer.AmOwner) continue;
                    MeetingHud.Instance.ClearVote();

                }
                if (AmongUsClient.Instance.AmHost) 
                    MeetingHud.Instance.CheckForEndVoting();
            }
            PlayerControl guesser = Helpers.playerById(killerId);
            if (FastDestroyableSingleton<HudManager>.Instance != null && guesser != null)
                if (PlayerControl.LocalPlayer == dyingTarget) 
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(guesser.Data, dyingTarget.Data);
                else if (dyingLoverPartner != null && PlayerControl.LocalPlayer == dyingLoverPartner) 
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(dyingLoverPartner.Data, dyingLoverPartner.Data);
            
            PlayerControl guessedTarget = Helpers.playerById(guessedTargetId);
            if (Guesser.showInfoInGhostChat && PlayerControl.LocalPlayer.Data.IsDead && guessedTarget != null) {
                RoleInfo roleInfo = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == guessedRoleId);
                string msg = $"Guesser guessed the role {roleInfo?.name ?? ""} for {guessedTarget.Data.PlayerName}!";
                if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(guesser, msg);
                if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                    FastDestroyableSingleton<Assets.CoreScripts.Telemetry>.Instance.SendWho();
            }
        }

        public static void useAdminTime(float time)
        {
            MapOptions.restrictAdminTime -= time;
        }

        public static void useCameraTime(float time)
        {
            MapOptions.restrictCamerasTime -= time;
        }

        public static void useVitalsTime(float time)
        {
            MapOptions.restrictVitalsTime -= time;
        }


    public static void blackmailPlayer(byte playerId) {
      PlayerControl target = Helpers.playerById(playerId);
      Blackmailer.blackmailed = target;
      
    }

    public static void unblackmailPlayer() {
      Blackmailer.blackmailed = null;
      Blackmailer.alreadyShook = false;
    }

        public static void setBlanked(byte playerId, byte value) {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            Pursuer.blankedList.RemoveAll(x => x.PlayerId == playerId);
            if (value > 0) Pursuer.blankedList.Add(target);            
        }

        public static void bloody(byte killerPlayerId, byte bloodyPlayerId) {
            if (Bloody.active.ContainsKey(killerPlayerId)) return;
            Bloody.active.Add(killerPlayerId, Bloody.duration);
            Bloody.bloodyKillerMap.Add(killerPlayerId, bloodyPlayerId);
        }

        public static void setFirstKill(byte playerId) {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            MapOptions.firstKillPlayer = target;
        }

        public static void setTiebreak()
        {
            Tiebreaker.isTiebreak = true;
        }
    }   

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class RPCHandlerPatch
    {
        static void Postfix([HarmonyArgument(0)]byte callId, [HarmonyArgument(1)]MessageReader reader)
        {
            byte packetId = callId;
            switch (packetId) {

                // Main Controls

                case (byte)CustomRPC.ResetVaribles:
                    RPCProcedure.resetVariables();
                    break;
                case (byte)CustomRPC.ShareOptions:
                    RPCProcedure.ShareOptions((int)reader.ReadPackedUInt32(), reader);
                    break;
                case (byte)CustomRPC.ForceEnd:
                    RPCProcedure.forceEnd();
                    break; 
                case (byte)CustomRPC.SetRole:
                    byte roleId = reader.ReadByte();
                    byte playerId = reader.ReadByte();
                    RPCProcedure.setRole(roleId, playerId);
                    break;
                case (byte)CustomRPC.SetModifier:
                    byte modifierId = reader.ReadByte();
                    byte pId = reader.ReadByte();
                    byte flag = reader.ReadByte();
                    RPCProcedure.setModifier(modifierId, pId, flag);
                    break;
                case (byte)CustomRPC.VersionHandshake:
                    byte major = reader.ReadByte();
                    byte minor = reader.ReadByte();
                    byte patch = reader.ReadByte();
                    int versionOwnerId = reader.ReadPackedInt32();
                    byte revision = 0xFF;
                    Guid guid;
                    if (reader.Length - reader.Position >= 17) { // enough bytes left to read
                        revision = reader.ReadByte();
                        // GUID
                        byte[] gbytes = reader.ReadBytes(16);
                        guid = new Guid(gbytes);
                    } else {
                        guid = new Guid(new byte[16]);
                    }
                    RPCProcedure.versionHandshake(major, minor, patch, revision == 0xFF ? -1 : revision, guid, versionOwnerId);
                    break;
                case (byte)CustomRPC.UseUncheckedVent:
                    int ventId = reader.ReadPackedInt32();
                    byte ventingPlayer = reader.ReadByte();
                    byte isEnter = reader.ReadByte();
                    RPCProcedure.useUncheckedVent(ventId, ventingPlayer, isEnter);
                    break;
                case (byte)CustomRPC.UncheckedMurderPlayer:
                    byte source = reader.ReadByte();
                    byte target = reader.ReadByte();
                    byte showAnimation = reader.ReadByte();
                    RPCProcedure.uncheckedMurderPlayer(source, target, showAnimation);
                    break;
                case (byte)CustomRPC.UncheckedExilePlayer:
                    byte exileTarget = reader.ReadByte();
                    RPCProcedure.uncheckedExilePlayer(exileTarget);
                    break;
                case (byte)CustomRPC.UncheckedCmdReportDeadBody:
                    byte reportSource = reader.ReadByte();
                    byte reportTarget = reader.ReadByte();
                    RPCProcedure.uncheckedCmdReportDeadBody(reportSource, reportTarget);
                    break;
                case (byte)CustomRPC.DynamicMapOption:
                    byte mapId = reader.ReadByte();
                    RPCProcedure.dynamicMapOption(mapId);
                    break;

                // Role functionality

                case (byte)CustomRPC.EngineerFixLights:
                    RPCProcedure.engineerFixLights();
                    break;
                case (byte)CustomRPC.EngineerFixSubmergedOxygen:
                    RPCProcedure.engineerFixSubmergedOxygen();
                    break;
                case (byte)CustomRPC.EngineerUsedRepair:
                    RPCProcedure.engineerUsedRepair();
                    break;
                case (byte)CustomRPC.CleanBody:
                    RPCProcedure.cleanBody(reader.ReadByte());
                    break;
                case (byte)CustomRPC.UseAdminTime:
                    RPCProcedure.useAdminTime(reader.ReadSingle());
                    break;
                case (byte)CustomRPC.UseCameraTime:
                    RPCProcedure.useCameraTime(reader.ReadSingle());
                    break;
                case (byte)CustomRPC.UseVitalsTime:
                    RPCProcedure.useVitalsTime(reader.ReadSingle());
                    break;
                case (byte)CustomRPC.BlackmailPlayer:
                    RPCProcedure.blackmailPlayer(reader.ReadByte());
                    break;
                case (byte)CustomRPC.UnblackmailPlayer:
                    RPCProcedure.unblackmailPlayer();
                    break;
                case (byte)CustomRPC.DragBody:
                    RPCProcedure.dragBody(reader.ReadByte());
                    break;
                case (byte)CustomRPC.DropBody:
                    RPCProcedure.dropBody(reader.ReadByte());
                    break;
                case (byte)CustomRPC.SheriffKill:
                    RPCProcedure.sheriffKill(reader.ReadByte());
                    break;
                case (byte)CustomRPC.TimeMasterRewindTime:
                    RPCProcedure.timeMasterRewindTime();
                    break;
                case (byte)CustomRPC.TimeMasterShield:
                    RPCProcedure.timeMasterShield();
                    break;
                case (byte)CustomRPC.AmnisiacTakeRole:
                    RPCProcedure.amnisiacTakeRole(reader.ReadByte());
                    break;
                case (byte)CustomRPC.VeterenAlert:
                    RPCProcedure.veterenAlert();
                    break;
                case (byte)CustomRPC.VeterenKill:
                    RPCProcedure.veterenKill(reader.ReadByte());
                    break;
                case (byte)CustomRPC.MedicSetShielded:
                    RPCProcedure.medicSetShielded(reader.ReadByte());
                    break;
                case (byte)CustomRPC.ShieldedMurderAttempt:
                    RPCProcedure.shieldedMurderAttempt(reader.ReadByte());
                    break;
                case (byte)CustomRPC.ShifterShift:
                    RPCProcedure.shifterShift(reader.ReadByte());
                    break;
                case (byte)CustomRPC.SwapperSwap:
                    byte playerId1 = reader.ReadByte();
                    byte playerId2 = reader.ReadByte();
                    RPCProcedure.swapperSwap(playerId1, playerId2);
                    break;
                case (byte)CustomRPC.MorphlingMorph:
                    RPCProcedure.morphlingMorph(reader.ReadByte());
                    break;
                case (byte)CustomRPC.CamouflagerCamouflage:
                    byte setTimer = reader.ReadByte();
                    RPCProcedure.camouflagerCamouflage(setTimer);
                break;
                case (byte)CustomRPC.VampireSetBitten:
                    byte bittenId = reader.ReadByte();
                    byte reset = reader.ReadByte();
                    RPCProcedure.vampireSetBitten(bittenId, reset);
                    break;
                case (byte)CustomRPC.PlaceGarlic:
                    RPCProcedure.placeGarlic(reader.ReadBytesAndSize());
                    break;
                case (byte)CustomRPC.TrackerUsedTracker:
                    RPCProcedure.trackerUsedTracker(reader.ReadByte());
                    break;               
                case (byte)CustomRPC.DeputyUsedHandcuffs:
                    RPCProcedure.deputyUsedHandcuffs(reader.ReadByte());
                    break;
                case (byte)CustomRPC.DeputyPromotes:
                    RPCProcedure.deputyPromotes();
                    break;
                case (byte)CustomRPC.JackalCreatesSidekick:
                    RPCProcedure.jackalCreatesSidekick(reader.ReadByte());
                    break;
                case (byte)CustomRPC.SidekickPromotes:
                    RPCProcedure.sidekickPromotes();
                    break;
                case (byte)CustomRPC.ErasePlayerRoles:
                    RPCProcedure.erasePlayerRoles(reader.ReadByte());
                    break;
                case (byte)CustomRPC.SetFutureErased:
                    RPCProcedure.setFutureErased(reader.ReadByte());
                    break;
                case (byte)CustomRPC.SetFutureShifted:
                    RPCProcedure.setFutureShifted(reader.ReadByte());
                    break;
                case (byte)CustomRPC.SetFutureShielded:
                    RPCProcedure.setFutureShielded(reader.ReadByte());
                    break;
                case (byte)CustomRPC.PlaceNinjaTrace:
                    RPCProcedure.placeNinjaTrace(reader.ReadBytesAndSize());
                    break;
                case (byte)CustomRPC.PlacePortal:
                    RPCProcedure.placePortal(reader.ReadBytesAndSize());
                    break;
                case (byte)CustomRPC.UsePortal:
                    RPCProcedure.usePortal(reader.ReadByte());
                    break;
                case (byte)CustomRPC.PlaceJackInTheBox:
                    RPCProcedure.placeJackInTheBox(reader.ReadBytesAndSize());
                    break;
                case (byte)CustomRPC.LightsOut:
                    RPCProcedure.lightsOut();
                    break;
                case (byte)CustomRPC.PlaceCamera:
                    RPCProcedure.placeCamera(reader.ReadBytesAndSize());
                    break;
                case (byte)CustomRPC.SealVent:
                    RPCProcedure.sealVent(reader.ReadPackedInt32());
                    break;
                case (byte)CustomRPC.ArsonistWin:
                    RPCProcedure.arsonistWin();
                    break;
                case (byte)CustomRPC.GuesserShoot:
                    byte killerId = reader.ReadByte();
                    byte dyingTarget = reader.ReadByte();
                    byte guessedTarget = reader.ReadByte();
                    byte guessedRoleId = reader.ReadByte();
                    RPCProcedure.guesserShoot(killerId, dyingTarget, guessedTarget, guessedRoleId);
                    break;
                case (byte)CustomRPC.VultureWin:
                    RPCProcedure.vultureWin();
                    break;
                case (byte)CustomRPC.LawyerWin:
                    RPCProcedure.lawyerWin();
                    break; 
                case (byte)CustomRPC.LawyerSetTarget:
                    RPCProcedure.lawyerSetTarget(reader.ReadByte()); 
                    break;
                case (byte)CustomRPC.LawyerPromotesToPursuer:
                    RPCProcedure.lawyerPromotesToPursuer();
                    break;
                case (byte)CustomRPC.ProsecutorSetTarget:
                    RPCProcedure.prosecutorSetTarget(reader.ReadByte());
                    break;
                case (byte)CustomRPC.ProsecutorChangesRole:
                    RPCProcedure.prosecutorChangesRole();
                    break;
                case (byte)CustomRPC.ProsecutorToPursuer:
                    RPCProcedure.prosecutorToPursuer();
                    break;
                case (byte)CustomRPC.SetBlanked:
                    var pid = reader.ReadByte();
                    var blankedValue = reader.ReadByte();
                    RPCProcedure.setBlanked(pid, blankedValue);
                    break;
                case (byte)CustomRPC.SetFutureSpelled:
                    RPCProcedure.setFutureSpelled(reader.ReadByte());
                    break;
                case (byte)CustomRPC.Bloody:
                    byte bloodyKiller = reader.ReadByte();
                    byte bloodyDead = reader.ReadByte();
                    RPCProcedure.bloody(bloodyKiller, bloodyDead);
                    break;
                case (byte)CustomRPC.SetFirstKill:
                    byte firstKill = reader.ReadByte();
                    RPCProcedure.setFirstKill(firstKill);
                    break;
                case (byte)CustomRPC.SetTiebreak:
                    RPCProcedure.setTiebreak();
                    break;
                case (byte)CustomRPC.SetInvisible:
                    byte invisiblePlayer = reader.ReadByte();
                    byte invisibleFlag = reader.ReadByte();
                    RPCProcedure.setInvisible(invisiblePlayer, invisibleFlag);
                    break; 
                case (byte)CustomRPC.SetSwoop:
                    byte invisiblePlayer2 = reader.ReadByte();
                    byte invisibleFlag2 = reader.ReadByte();
                    RPCProcedure.setSwoop(invisiblePlayer2, invisibleFlag2);
                    break;  
                case (byte)CustomRPC.Mine:
                    var newVentId = reader.ReadInt32();
                    var role = Helpers.playerById(reader.ReadByte());
                    var pos = reader.ReadBytesAndSize();
                    var zAxis = reader.ReadSingle();            
                    RPCProcedure.Mine(newVentId, role, pos, zAxis);
                    break;
                case (byte)CustomRPC.SetInvisibleGen:
                    byte invisiblePlayer3 = reader.ReadByte();
                    byte invisibleFlag3 = reader.ReadByte();
                    RPCProcedure.setInvisibleGen(invisiblePlayer3, invisibleFlag3);
                    break;  
            }
        }
    }
}
