  
using HarmonyLib;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.GameHistory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Hazel;
using UnhollowerBaseLib;
using System;
using System.Text;

namespace TheOtherRoles {
    enum WinCondition {
        Default,
        LoversTeamWin,
        LoversSoloWin,
        JesterWin,
        BountyHunterWin,
        JesterAndBountyHunterWin
    }

    static class AdditionalTempData {
        // Should be implemented using a proper GameOverReason in the future
        public static WinCondition winCondition = WinCondition.Default;
        public static bool localIsLover = false;


        public static void clear() {
            winCondition = WinCondition.Default;
            localIsLover = false;

        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class OnGameEndPatch {
        public static void Postfix(AmongUsClient __instance, GameOverReason OFLKLGMHBEL, bool JFFPAKGPNJA) {
            AdditionalTempData.clear();

            // Remove shifter from winners
            if (Shifter.shifter != null) {
                WinningPlayerData shifterWinner = null;
                foreach (WinningPlayerData winner in  TempData.winners)
                    if (winner.Name == Shifter.shifter.Data.PlayerName) shifterWinner = winner;
                
                if (shifterWinner != null) TempData.winners.Remove(shifterWinner);
            }
            // Remove Jester from winners (on Jester win he will be added again, see below)
            if (Jester.jester != null) {
                WinningPlayerData jesterWinner = null;
                foreach (WinningPlayerData winner in  TempData.winners)
                    if (winner.Name == Jester.jester.Data.PlayerName) jesterWinner = winner;
                
                if (jesterWinner != null) TempData.winners.Remove(jesterWinner);
            }

            // Jester and Bounty Hunter win condition (should be implemented using a proper GameOverReason in the future)
            bool jesterWin = Jester.jester != null && Jester.jester.Data.IsImpostor;
            bool bountyHunterWin = BountyHunter.bountyHunter != null && BountyHunter.bountyHunter.Data.IsImpostor;
            if (jesterWin || bountyHunterWin) {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                if (jesterWin) {
                    WinningPlayerData wpd = new WinningPlayerData(Jester.jester.Data);
                    wpd.IsImpostor = false; 
                    TempData.winners.Add(wpd);
                    AdditionalTempData.winCondition = WinCondition.JesterWin;
                }
                if (bountyHunterWin) {
                    WinningPlayerData wpd = new WinningPlayerData(BountyHunter.bountyHunter.Data);
                    wpd.IsImpostor = false; 
                    TempData.winners.Add(wpd);
                    if (AdditionalTempData.winCondition == WinCondition.JesterWin)
                        AdditionalTempData.winCondition = WinCondition.JesterAndBountyHunterWin;
                    else
                        AdditionalTempData.winCondition = WinCondition.BountyHunterWin;  
                }
            }

            // Lovers win conditions (should be implemented using a proper GameOverReason in the future)
            else if (Lovers.existingAndAlive()) {
                AdditionalTempData.localIsLover = (PlayerControl.LocalPlayer == Lovers.lover1 || PlayerControl.LocalPlayer == Lovers.lover2);
                // Double win for lovers, crewmates also win
                if (TempData.DidHumansWin(OFLKLGMHBEL)) {
                    AdditionalTempData.winCondition = WinCondition.LoversTeamWin;
                }
                // Lovers solo win
                else if (Lovers.existingWithImpLover()){
                    AdditionalTempData.winCondition = WinCondition.LoversSoloWin;
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    TempData.winners.Add(new WinningPlayerData(Lovers.lover1.Data));
                    TempData.winners.Add(new WinningPlayerData(Lovers.lover2.Data));
                }
            }

            // Reset Bonus Roles Settings
            clearAndReloadRoles();
            clearGameHistory();
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public class EndGameManagerSetUpPatch {
        public static void Postfix(EndGameManager __instance) {
            GameObject bonusText = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            bonusText.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.8f, __instance.WinText.transform.position.z);
            bonusText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            TextRenderer textRenderer = bonusText.GetComponent<TextRenderer>();
            textRenderer.Text = "";

            if (AdditionalTempData.winCondition == WinCondition.JesterWin) {
                textRenderer.Text = "Jester Wins";
                textRenderer.Color = Jester.color;
            }
            else if (AdditionalTempData.winCondition == WinCondition.BountyHunterWin) {
                textRenderer.Text = "Bounty Hunter Wins";
                textRenderer.Color = BountyHunter.color;
            }
            else if (AdditionalTempData.winCondition == WinCondition.JesterAndBountyHunterWin) {
                textRenderer.Text = "[AD653BFF]Bounty Hunter[FFFFFFFF] and [FF54A7FF]Jester[FFFFFFFF] Win";
                textRenderer.Color = Color.white;
            }
            else if (AdditionalTempData.winCondition == WinCondition.LoversTeamWin) {
                if (AdditionalTempData.localIsLover) {
                    __instance.WinText.Text = "Double Victory";
                } 
                textRenderer.Text = "Lovers And Crewmates Win";
                textRenderer.Color = Lovers.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
            } else if (AdditionalTempData.winCondition == WinCondition.LoversSoloWin) {
                textRenderer.Text = "Lovers Win";
                textRenderer.Color = Lovers.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
            }
            
            AdditionalTempData.clear();
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))] 
    class CheckEndCriteriaPatch{
        private static void ReviveEveryone() {
            for (int i = 0; i < GameData.Instance.PlayerCount; i++)
                GameData.Instance.AllPlayers[i].Object.Revive();
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++) UnityEngine.Object.Destroy(array[i].gameObject);
        }

        private static void EndGameForSabotage(ShipStatus __instance)
        {
            if (!DestroyableSingleton<TutorialManager>.InstanceExists)
            {
                __instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.ImpostorBySabotage, false);
                return;
            }
            DestroyableSingleton<HudManager>.Instance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverSabotage, new Il2CppReferenceArray<Il2CppSystem.Object>(0)));
        }

        public static bool Prefix(ShipStatus __instance) {
            if (!GameData.Instance)
            {
                return false;
            }
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.LifeSupp) ? __instance.Systems[SystemTypes.LifeSupp] : null;
            if (systemType != null)
            {
                LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                if (lifeSuppSystemType.Countdown < 0f)
                {
                    EndGameForSabotage(__instance);
                    lifeSuppSystemType.Countdown = 10000f;
                }
            }
            ISystemType systemType2 = __instance.Systems.ContainsKey(SystemTypes.Reactor) ? __instance.Systems[SystemTypes.Reactor] : null;
            if (systemType2 == null) systemType2 = __instance.Systems.ContainsKey(SystemTypes.Laboratory) ? __instance.Systems[SystemTypes.Laboratory] : null;
            if (systemType2 != null)
            {
                ReactorSystemType reactorSystemType = systemType2.TryCast<ReactorSystemType>();
                if (reactorSystemType.Countdown < 0f)
                {
                    EndGameForSabotage(__instance);
                    reactorSystemType.Countdown = 10000f;
                }
            }
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            for (int i = 0; i < GameData.Instance.PlayerCount; i++)
            {
                GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                if (!playerInfo.Disconnected)
                {
                    if (playerInfo.IsImpostor)
                    {
                        num3++;
                    }
                    if (!playerInfo.IsDead)
                    {
                        if (playerInfo.IsImpostor)
                        {
                            num2++;
                        }
                        else
                        {
                            num++;
                        }
                    }
                }
            }
            if (num2 <= 0 && (!DestroyableSingleton<TutorialManager>.InstanceExists || num3 > 0))
            {
                if (!DestroyableSingleton<TutorialManager>.InstanceExists)
                {
                    __instance.enabled = false;
                    ShipStatus.RpcEndGame(GameOverReason.HumansByVote, false);
                    return false;
                }
                DestroyableSingleton<HudManager>.Instance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverImpostorDead, new Il2CppReferenceArray<Il2CppSystem.Object>(0)));
                ReviveEveryone();
                return false;
            }
            else
            {
                if (num > num2)
                {
                    bool localCompletedAllTasks = true;
                    foreach (PlayerTask t in PlayerControl.LocalPlayer.myTasks) {
                        localCompletedAllTasks = localCompletedAllTasks && t.IsComplete;
                    }

                    if (!DestroyableSingleton<TutorialManager>.InstanceExists)
                    {
                        if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
                        {
                            __instance.enabled = false;
                            ShipStatus.RpcEndGame(GameOverReason.HumansByTask, false);
                            return false;
                        }
                    }
                    else if (localCompletedAllTasks)
                    {
                        DestroyableSingleton<HudManager>.Instance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverTaskWin, new Il2CppReferenceArray<Il2CppSystem.Object>(0)));
                        __instance.Begin();
                    }
                    if (num + num2 == 3 && Lovers.existingAndAlive()) { // 3 players with 2 lovers is always a lover win (either shared with crewmates or solo for lovers, marked as impostor win)
                        __instance.enabled = false;
                        ShipStatus.RpcEndGame(Lovers.existingWithImpLover() ? GameOverReason.ImpostorByKill : GameOverReason.HumansByVote, false); // should be implemented using a proper GameOverReason in the future
                        return false;
                    }
                    return false;
                }
                if (num == num2 && Lovers.existingAndAlive() && Lovers.existingWithImpLover()) { // 3 vs 3 or 2 vs 2 is not win if both lovers are alive and one is an impostor
                    return false;
                }
                if (!DestroyableSingleton<TutorialManager>.InstanceExists)
                {
                    __instance.enabled = false;
                    GameOverReason endReason;
                    switch (TempData.LastDeathReason)
                    {
                    case DeathReason.Exile:
                        endReason = GameOverReason.ImpostorByVote;
                        break;
                    case DeathReason.Kill:
                        endReason = GameOverReason.ImpostorByKill;
                        break;
                    default:
                        endReason = GameOverReason.ImpostorByVote;
                        break;
                    }
                    ShipStatus.RpcEndGame(endReason, false);
                    return false;
                }
                DestroyableSingleton<HudManager>.Instance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverImpostorKills, new Il2CppReferenceArray<Il2CppSystem.Object>(0)));
                
                ReviveEveryone();
                return false;
            }
        }
    }
}