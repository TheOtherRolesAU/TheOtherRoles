using HarmonyLib;
using System.Linq;
using System;
using System.Collections.Generic;
using static TheOtherRoles.TheOtherRoles;
using UnityEngine;

namespace TheOtherRoles
{
    class RoleInfo {
        public Color color;
        public string name;
        public string introDescription;
        public string shortDescription;
        public RoleId roleId;

        RoleInfo(string name, Color color, string introDescription, string shortDescription, RoleId roleId) {
            this.color = color;
            this.name = name;
            this.introDescription = introDescription;
            this.shortDescription = shortDescription;
            this.roleId = roleId;
        }

        public static RoleInfo jester = new RoleInfo("小丑", Jester.color, "被投出去", "被投出去", RoleId.Jester);
        public static RoleInfo mayor = new RoleInfo("市長", Mayor.color, "投票會變兩票", "投票會變兩票", RoleId.Mayor);
        public static RoleInfo engineer = new RoleInfo("工程師",  Engineer.color, "維護船上的重要系統 ", "修復船隻", RoleId.Engineer);
        public static RoleInfo sheriff = new RoleInfo("警長", Sheriff.color, "射擊 <color=#FF1919FF>偽裝者</color>", "射擊偽裝者", RoleId.Sheriff);
        public static RoleInfo lighter = new RoleInfo("點燈人", Lighter.color, "你的燈永不熄滅", "你的燈永不熄滅", RoleId.Lighter);
        public static RoleInfo godfather = new RoleInfo("教父", Godfather.color, "殺死所有船員", "殺死所有船員", RoleId.Godfather);
        public static RoleInfo mafioso = new RoleInfo("黑手黨員", Mafioso.color, "在<color=#FF1919FF>黑手黨</color>工作，教父死後殺死船員", "殺死所有船員", RoleId.Mafioso);
        public static RoleInfo janitor = new RoleInfo("守墓人", Janitor.color, "在<color=#FF1919FF>黑手黨</color>工作，隱藏屍體", "隱藏屍體", RoleId.Janitor);
        public static RoleInfo morphling = new RoleInfo("百變怪", Morphling.color, "改變你的樣子以免被抓到", "改變你的樣子", RoleId.Morphling);
        public static RoleInfo camouflager = new RoleInfo("魔術師", Camouflager.color, "偽裝並殺死船員", "隱藏船員其中", RoleId.Camouflager);
        public static RoleInfo vampire = new RoleInfo("吸血鬼", Vampire.color, "咬人來殺死船員", "咬你的敵人", RoleId.Vampire);
        public static RoleInfo eraser = new RoleInfo("抹除者", Eraser.color, "殺死船員並抹除他們的職業", "抹除敵人的職業", RoleId.Eraser);
        public static RoleInfo trickster = new RoleInfo("詭騙師", Trickster.color, "用你的詭騙箱讓別人大吃一驚", "讓你的敵人大吃一驚", RoleId.Trickster);
        public static RoleInfo cleaner = new RoleInfo("清除者", Cleaner.color, "殺人並不留痕跡", "清理屍體", RoleId.Cleaner);
        public static RoleInfo warlock = new RoleInfo("咒詛師", Warlock.color, "詛咒其他玩家並殺死所有人", "詛咒並殺死所有人", RoleId.Warlock);
        public static RoleInfo bountyHunter = new RoleInfo("賞金獵人", BountyHunter.color, "追捕你的懸賞", "追捕你的懸賞", RoleId.BountyHunter);
        public static RoleInfo detective = new RoleInfo("偵探", Detective.color, "通過檢查腳印來找到<color=#FF1919FF>偽裝者</color>", "檢查腳印", RoleId.Detective);
        public static RoleInfo timeMaster = new RoleInfo("時間管理大師", TimeMaster.color, "用你的時間之盾保護自己", "用你的時間之盾", RoleId.TimeMaster);
        public static RoleInfo medic = new RoleInfo("醫生", Medic.color, "用盾牌保護某人", "保護其他人", RoleId.Medic);
        public static RoleInfo shifter = new RoleInfo("轉職者", Shifter.color, "轉移你的職業", "轉移你的職業", RoleId.Shifter);
        public static RoleInfo swapper = new RoleInfo("掉包者", Swapper.color, "掉包得票來放逐<color=#FF1919FF>偽裝者</color>", "掉包得票", RoleId.Swapper);
        public static RoleInfo seer = new RoleInfo("靈媒", Seer.color, "你可以看到玩家的死亡", "你可以看到玩家的死亡", RoleId.Seer);
        public static RoleInfo hacker = new RoleInfo("駭客", Hacker.color, "駭入系統來找到<color=#FF1919FF>偽裝者</color>", "駭入來找到偽裝者", RoleId.Hacker);
        public static RoleInfo niceMini = new RoleInfo("好迷你", Mini.color, "在你長大之前沒有人可以傷害你", "沒有人可以傷害你", RoleId.Mini);
        public static RoleInfo evilMini = new RoleInfo("壞迷你", Palette.ImpostorRed, "在你長大之前沒有人可以傷害你", "沒有人可以傷害你", RoleId.Mini);
        public static RoleInfo tracker = new RoleInfo("追踪者", Tracker.color, "追踪<color=#FF1919FF>偽裝者</color>", "追踪偽裝者", RoleId.Tracker);
        public static RoleInfo snitch = new RoleInfo("密探", Snitch.color, "完成你的任務來找出<color=#FF1919FF>偽裝者</color>", "完成你的任務", RoleId.Snitch);
        public static RoleInfo jackal = new RoleInfo("豺狼", Jackal.color, "殺死所有船員與<color=#FF1919FF>偽裝者</color>來勝利", "殺死所有人", RoleId.Jackal);
        public static RoleInfo sidekick = new RoleInfo("跟班", Sidekick.color, "幫助你的豺狼殺死所有人", "幫助你的豺狼殺死所有人", RoleId.Sidekick);
        public static RoleInfo spy = new RoleInfo("間諜", Spy.color, "讓<color=#FF1919FF>偽裝者</color>混亂", "讓偽裝者混亂", RoleId.Spy);
        public static RoleInfo securityGuard = new RoleInfo("守衛", SecurityGuard.color, "封鎖通風口跟放置攝影機", "封鎖通風口跟放置攝影機", RoleId.SecurityGuard);
        public static RoleInfo arsonist = new RoleInfo("縱火狂", Arsonist.color, "燒了大家", "燒了大家", RoleId.Arsonist);
        public static RoleInfo goodGuesser = new RoleInfo("好賭徒", Guesser.color, "猜測並放逐", "猜測並放逐", RoleId.Guesser);
        public static RoleInfo badGuesser = new RoleInfo("壞賭徒", Palette.ImpostorRed, "猜測並放逐", "猜測並放逐", RoleId.Guesser);
        public static RoleInfo bait = new RoleInfo("誘餌", Bait.color, "引誘你的敵人", "引誘你的敵人", RoleId.Bait);
        public static RoleInfo impostor = new RoleInfo("偽裝者", Palette.ImpostorRed, Helpers.cs(Palette.ImpostorRed, "破壞跟殺死所有人"), "破壞跟殺死所有人", RoleId.Impostor);
        public static RoleInfo crewmate = new RoleInfo("船員", Color.white, "找到偽裝者", "找到偽裝者", RoleId.Crewmate);
        public static RoleInfo lover = new RoleInfo("戀人", Lovers.color, $"你正在戀愛中", $"你正在戀愛中", RoleId.Lover);

        public static List<RoleInfo> allRoleInfos = new List<RoleInfo>() {
            impostor,
            godfather,
            mafioso,
            janitor,
            morphling,
            camouflager,
            vampire,
            eraser,
            trickster,
            cleaner,
            warlock,
            bountyHunter,
            niceMini,
            evilMini,
            goodGuesser,
            badGuesser,
            lover,
            jester,
            arsonist,
            jackal,
            sidekick,
            crewmate,
            shifter,
            mayor,
            engineer,
            sheriff,
            lighter,
            detective,
            timeMaster,
            medic,
            swapper,
            seer,
            hacker,
            tracker,
            snitch,
            spy,
            securityGuard,
            bountyHunter,
            bait
        };

        public static List<RoleInfo> getRoleInfoForPlayer(PlayerControl p) {
            List<RoleInfo> infos = new List<RoleInfo>();
            if (p == null) return infos;

            // Special roles
            if (p == Jester.jester) infos.Add(jester);
            if (p == Mayor.mayor) infos.Add(mayor);
            if (p == Engineer.engineer) infos.Add(engineer);
            if (p == Sheriff.sheriff) infos.Add(sheriff);
            if (p == Lighter.lighter) infos.Add(lighter);
            if (p == Godfather.godfather) infos.Add(godfather);
            if (p == Mafioso.mafioso) infos.Add(mafioso);
            if (p == Janitor.janitor) infos.Add(janitor);
            if (p == Morphling.morphling) infos.Add(morphling);
            if (p == Camouflager.camouflager) infos.Add(camouflager);
            if (p == Vampire.vampire) infos.Add(vampire);
            if (p == Eraser.eraser) infos.Add(eraser);
            if (p == Trickster.trickster) infos.Add(trickster);
            if (p == Cleaner.cleaner) infos.Add(cleaner);
            if (p == Warlock.warlock) infos.Add(warlock);
            if (p == Detective.detective) infos.Add(detective);
            if (p == TimeMaster.timeMaster) infos.Add(timeMaster);
            if (p == Medic.medic) infos.Add(medic);
            if (p == Shifter.shifter) infos.Add(shifter);
            if (p == Swapper.swapper) infos.Add(swapper);
            if (p == Seer.seer) infos.Add(seer);
            if (p == Hacker.hacker) infos.Add(hacker);
            if (p == Mini.mini) infos.Add(p.Data.IsImpostor ? evilMini : niceMini);
            if (p == Tracker.tracker) infos.Add(tracker);
            if (p == Snitch.snitch) infos.Add(snitch);
            if (p == Jackal.jackal || (Jackal.formerJackals != null && Jackal.formerJackals.Any(x => x.PlayerId == p.PlayerId))) infos.Add(jackal);
            if (p == Sidekick.sidekick) infos.Add(sidekick);
            if (p == Spy.spy) infos.Add(spy);
            if (p == SecurityGuard.securityGuard) infos.Add(securityGuard);
            if (p == Arsonist.arsonist) infos.Add(arsonist);
            if (p == Guesser.guesser) infos.Add(p.Data.IsImpostor ? badGuesser : goodGuesser);
            if (p == BountyHunter.bountyHunter) infos.Add(bountyHunter);
            if (p == Bait.bait) infos.Add(bait);

            // Default roles
            if (infos.Count == 0 && p.Data.IsImpostor) infos.Add(impostor); // Just Impostor
            if (infos.Count == 0 && !p.Data.IsImpostor) infos.Add(crewmate); // Just Crewmate

            // Modifier
            if (p == Lovers.lover1|| p == Lovers.lover2) infos.Add(lover);

            return infos;
        }
    }
}
