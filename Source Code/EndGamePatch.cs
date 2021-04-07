  
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

using GameOverReason = JBJHCLOILBF;
using TempData = DIJKKMDMDCM;
using WinningPlayerData = KBEBEPNGKOD;
using DeathReason = KAPJFCMEBJE;
using SystemTypes = LGBKLKNAINN;
using ISystemType = CBFMKGACLNE;
using LifeSuppSystemType = MHEBFNFMKPH;
using ICriticalSabotage = IEGIFFLBFJG;

namespace TheOtherRoles {
    enum CustomGameOverReason {
        LoversWin = 10,
        TeamJackalWin = 11,
        ChildLose = 12,
        JesterWin = 13
    }

    enum WinCondition {
        Default,
        LoversTeamWin,
        LoversSoloWin,
        JesterWin,
        JackalWin,
        ChildLose
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

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.MDDLANONDOD))]
    public class OnGameEndPatch {
        private static GameOverReason gameOverReason;
        public static void Prefix(AmongUsClient __instance, ref GameOverReason FCBGPMEMOJB, bool GKFHPFPIHGA) {
            gameOverReason = FCBGPMEMOJB;
            if ((int)FCBGPMEMOJB >= 10) FCBGPMEMOJB = GameOverReason.ImpostorByKill;
        }

        public static void Postfix(AmongUsClient __instance, GameOverReason FCBGPMEMOJB, bool GKFHPFPIHGA) {
            AdditionalTempData.clear();

            // Remove Jester from winners (on Jester win he will be added again, see below)
            if (Jester.jester != null) {
                WinningPlayerData jesterWinner = null;
                foreach (WinningPlayerData winner in  TempData.BDGOKPKHCNB)
                    if (winner.GEDNCGBDPKC == Jester.jester.IDOFAMCIJKE.HGGCLJHCDBM) jesterWinner = winner;
                
                if (jesterWinner != null) TempData.BDGOKPKHCNB.Remove(jesterWinner);
            }
            // Remove Jackal and Sidekick from winners (on Jackal win he will be added again, see below)
            if (Jackal.jackal != null || Sidekick.sidekick != null) {
                List<WinningPlayerData> winnersToRemove = new List<WinningPlayerData>();
                foreach (WinningPlayerData winner in TempData.BDGOKPKHCNB) {
                    if (winner.GEDNCGBDPKC == Jackal.jackal?.IDOFAMCIJKE?.HGGCLJHCDBM) winnersToRemove.Add(winner);
                    if (winner.GEDNCGBDPKC == Sidekick.sidekick?.IDOFAMCIJKE?.HGGCLJHCDBM) winnersToRemove.Add(winner);
                    foreach(var player in Jackal.formerJackals) {
                        if (winner.GEDNCGBDPKC == player.IDOFAMCIJKE.HGGCLJHCDBM) {
                            winnersToRemove.Add(winner);
                        }
                    }
                }
                
                foreach (var winner in winnersToRemove) {
                    TempData.BDGOKPKHCNB.Remove(winner);
                }
            }

            bool childLose = Child.child != null && gameOverReason == (GameOverReason)CustomGameOverReason.ChildLose;
            bool jesterWin = Jester.jester != null && gameOverReason == (GameOverReason)CustomGameOverReason.JesterWin;

            // Child lose
            if (childLose) {
                TempData.BDGOKPKHCNB = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new WinningPlayerData(Child.child.IDOFAMCIJKE);
                wpd.KCEFGIGGLHM = false; // If "no one is the Child", it will display the Child, but also show defeat to everyone
                TempData.BDGOKPKHCNB.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.ChildLose;  
            }

            // Jester win
            else if (jesterWin) {
                TempData.BDGOKPKHCNB = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new WinningPlayerData(Jester.jester.IDOFAMCIJKE);
                TempData.BDGOKPKHCNB.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            }

            // Lovers win conditions
            else if (Lovers.existingAndAlive() && gameOverReason == (GameOverReason)CustomGameOverReason.LoversWin) {
                AdditionalTempData.localIsLover = (PlayerControl.LocalPlayer == Lovers.lover1 || PlayerControl.LocalPlayer == Lovers.lover2);
                // Double win for lovers, crewmates also win
                if (!Lovers.existingWithImpLover()) {
                    AdditionalTempData.winCondition = WinCondition.LoversTeamWin;
                    TempData.BDGOKPKHCNB = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                        if (p == null) continue;
                        if (p == Lovers.lover1 || p == Lovers.lover2)
                            TempData.BDGOKPKHCNB.Add(new WinningPlayerData(p.IDOFAMCIJKE));
                        else if (p != Jester.jester && p != Jackal.jackal && p != Sidekick.sidekick && !p.IDOFAMCIJKE.CIDDOFDJHJH)
                            TempData.BDGOKPKHCNB.Add(new WinningPlayerData(p.IDOFAMCIJKE));
                    }
                }
                // Lovers solo win
                else {
                    AdditionalTempData.winCondition = WinCondition.LoversSoloWin;
                    TempData.BDGOKPKHCNB = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    TempData.BDGOKPKHCNB.Add(new WinningPlayerData(Lovers.lover1.IDOFAMCIJKE));
                    TempData.BDGOKPKHCNB.Add(new WinningPlayerData(Lovers.lover2.IDOFAMCIJKE));
                }
            }
            
            // Jackal win condition (should be implemented using a proper GameOverReason in the future)
            else if (gameOverReason == (GameOverReason)CustomGameOverReason.TeamJackalWin && (Jackal.jackal != null && !Jackal.jackal.IDOFAMCIJKE.FGNJJFABIHJ || Sidekick.sidekick != null && !Sidekick.sidekick.IDOFAMCIJKE.FGNJJFABIHJ)) {
                // Jackal wins if nobody except jackal is alive
                AdditionalTempData.winCondition = WinCondition.JackalWin;
                TempData.BDGOKPKHCNB = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new WinningPlayerData(Jackal.jackal.IDOFAMCIJKE);
                wpd.CIDDOFDJHJH = false; 
                TempData.BDGOKPKHCNB.Add(wpd);
                // If there is a sidekick. The sidekick also wins
                if (Sidekick.sidekick != null) {
                    WinningPlayerData wpdSidekick = new WinningPlayerData(Sidekick.sidekick.IDOFAMCIJKE);
                    wpdSidekick.CIDDOFDJHJH = false; 
                    TempData.BDGOKPKHCNB.Add(wpdSidekick);
                }
                foreach(var player in Jackal.formerJackals) {
                    WinningPlayerData wpdFormerJackal = new WinningPlayerData(player.IDOFAMCIJKE);
                    wpdFormerJackal.CIDDOFDJHJH = false; 
                    TempData.BDGOKPKHCNB.Add(wpdFormerJackal);
                }
            }

            // Reset Settings
            RPCProcedure.resetVariables();
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.BIGANIAENKO))]
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
            else if (AdditionalTempData.winCondition == WinCondition.LoversTeamWin) {
                if (AdditionalTempData.localIsLover) {
                    __instance.WinText.Text = "Double Victory";
                } 
                textRenderer.Text = "Lovers And Crewmates Win";
                textRenderer.Color = Lovers.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
            } 
            else if (AdditionalTempData.winCondition == WinCondition.LoversSoloWin) {
                textRenderer.Text = "Lovers Win";
                textRenderer.Color = Lovers.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.JackalWin) {
                textRenderer.Text = "Team Jackal Wins";
                textRenderer.Color = Jackal.color;
            }
            else if (AdditionalTempData.winCondition == WinCondition.ChildLose) {
                textRenderer.Text = "Child died";
                textRenderer.Color = Child.color;
            }
            
            AdditionalTempData.clear();
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.NBEBPPBCGLJ))] 
    class CheckEndCriteriaPatch{

        public static bool Prefix(ShipStatus __instance) {
            if (!GameData.Instance) return false;
            var statistics = new PlayerStatistics(__instance);
            if (CheckAndEndGameForChildLose(__instance)) return false;
            if (CheckAndEndGameForJesterWin(__instance)) return false;
            if (CheckAndEndGameForSabotageWin(__instance)) return false;
            if (CheckAndEndGameForTaskWin(__instance)) return false;
            if (CheckAndEndGameForLoverWin(__instance, statistics)) return false;
            if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
            if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
            if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
            return false;
        }

        private static bool CheckAndEndGameForChildLose(ShipStatus __instance) {
            if (Child.triggerChildLose) {
                if (!DestroyableSingleton<TutorialManager>.JECNDKBIOFO)
                {
                    __instance.enabled = false;
                    ShipStatus.PBKIGLMJEDH((GameOverReason)CustomGameOverReason.ChildLose, false);
                }
                DestroyableSingleton<HudManager>.CMJOLNCMAPD.ShowPopUp(DestroyableSingleton<TranslationController>.CMJOLNCMAPD.GetString(StringNames.GameOverImpostorDead, new Il2CppReferenceArray<Il2CppSystem.Object>(0)));
                ReviveEveryone();
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForJesterWin(ShipStatus __instance) {
            if (Jester.triggerJesterWin) {
                if (!DestroyableSingleton<TutorialManager>.JECNDKBIOFO)
                {
                    __instance.enabled = false;
                    ShipStatus.PBKIGLMJEDH((GameOverReason)CustomGameOverReason.JesterWin, false);
                }
                DestroyableSingleton<HudManager>.CMJOLNCMAPD.ShowPopUp(DestroyableSingleton<TranslationController>.CMJOLNCMAPD.GetString(StringNames.GameOverImpostorDead, new Il2CppReferenceArray<Il2CppSystem.Object>(0)));
                ReviveEveryone();
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance) {
            if (__instance.Systems == null) return false;
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.LifeSupp) ? __instance.Systems[SystemTypes.LifeSupp] : null;
            if (systemType != null) {
                LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                if (lifeSuppSystemType != null && lifeSuppSystemType.DFAOAPHABEF < 0f) {
                    EndGameForSabotage(__instance);
                    lifeSuppSystemType.DFAOAPHABEF = 10000f;
                    return true;
                }
            }
            ISystemType systemType2 = __instance.Systems.ContainsKey(SystemTypes.Reactor) ? __instance.Systems[SystemTypes.Reactor] : null;
            if (systemType2 == null) {
                systemType2 = __instance.Systems.ContainsKey(SystemTypes.Laboratory) ? __instance.Systems[SystemTypes.Laboratory] : null;
            }
            if (systemType2 != null) {
                ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
                if (criticalSystem != null && criticalSystem.DFAOAPHABEF < 0f) {
                    EndGameForSabotage(__instance);
                    criticalSystem.ClearSabotage();
                    return true;
                }
            }
            return false;
        }

        private static bool CheckAndEndGameForTaskWin(ShipStatus __instance) {
            bool localCompletedAllTasks = true;
            foreach (PlayerTask t in PlayerControl.LocalPlayer.myTasks) {
                localCompletedAllTasks = localCompletedAllTasks && t.MOHGOCFFHLF;
            }

            if (!DestroyableSingleton<TutorialManager>.JECNDKBIOFO)
            {
                if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
                {
                    __instance.enabled = false;
                    ShipStatus.PBKIGLMJEDH(GameOverReason.HumansByTask, false);
                    return true;
                }
            }
            else if (localCompletedAllTasks)
            {
                DestroyableSingleton<HudManager>.CMJOLNCMAPD.ShowPopUp(DestroyableSingleton<TranslationController>.CMJOLNCMAPD.GetString(StringNames.GameOverTaskWin, new Il2CppReferenceArray<Il2CppSystem.Object>(0)));
                __instance.Begin();
            }
            return false;
        }

        private static bool CheckAndEndGameForLoverWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.TeamLoversAlive == 2 && statistics.TotalAlive <= 3) {
                if (!DestroyableSingleton<TutorialManager>.JECNDKBIOFO)
                {
                    __instance.enabled = false;
                    ShipStatus.PBKIGLMJEDH((GameOverReason)CustomGameOverReason.LoversWin, false);
                    return true;
                }
                DestroyableSingleton<HudManager>.CMJOLNCMAPD.ShowPopUp(DestroyableSingleton<TranslationController>.CMJOLNCMAPD.GetString(StringNames.GameOverImpostorDead, new Il2CppReferenceArray<Il2CppSystem.Object>(0)));
                ReviveEveryone();
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive && statistics.TeamImpostorsAlive == 0 && !(statistics.TeamJackalHasAliveLover && statistics.TeamLoversAlive == 2)) {
                if (!DestroyableSingleton<TutorialManager>.JECNDKBIOFO)
                {
                    __instance.enabled = false;
                    ShipStatus.PBKIGLMJEDH((GameOverReason)CustomGameOverReason.TeamJackalWin, false);
                    return true;
                }
                DestroyableSingleton<HudManager>.CMJOLNCMAPD.ShowPopUp(DestroyableSingleton<TranslationController>.CMJOLNCMAPD.GetString(StringNames.GameOverImpostorKills, new Il2CppReferenceArray<Il2CppSystem.Object>(0)));
                ReviveEveryone();
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive && statistics.TeamJackalAlive == 0 && !(statistics.TeamImpostorHasAliveLover && statistics.TeamLoversAlive == 2)) {
                if (!DestroyableSingleton<TutorialManager>.JECNDKBIOFO) {
                    __instance.enabled = false;
                    GameOverReason endReason;
                    switch (TempData.EBGJMGKCIFN) {
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
                    ShipStatus.PBKIGLMJEDH(endReason, false);
                    return true;
                }
                DestroyableSingleton<HudManager>.CMJOLNCMAPD.ShowPopUp(DestroyableSingleton<TranslationController>.CMJOLNCMAPD.GetString(StringNames.GameOverImpostorKills, new Il2CppReferenceArray<Il2CppSystem.Object>(0)));
                ReviveEveryone();
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0) {
                if (!DestroyableSingleton<TutorialManager>.JECNDKBIOFO) {
                    __instance.enabled = false;
                    ShipStatus.PBKIGLMJEDH(GameOverReason.HumansByVote, false);
                    return true;
                }
                DestroyableSingleton<HudManager>.CMJOLNCMAPD.ShowPopUp(DestroyableSingleton<TranslationController>.CMJOLNCMAPD.GetString(StringNames.GameOverImpostorDead, new Il2CppReferenceArray<Il2CppSystem.Object>(0)));
                ReviveEveryone();
                return true;
            }
            return false;
        }

        private static void ReviveEveryone() {
            for (int i = 0; i < GameData.Instance.BCFPPIDIMJK; i++)
                GameData.Instance.AllPlayers[i].GPBBCHGPABL.Revive();
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++) UnityEngine.Object.Destroy(array[i].gameObject);
        }

        private static void EndGameForSabotage(ShipStatus __instance)
        {
            if (!DestroyableSingleton<TutorialManager>.JECNDKBIOFO)
            {
                __instance.enabled = false;
                ShipStatus.PBKIGLMJEDH(GameOverReason.ImpostorBySabotage, false);
                return;
            }
            DestroyableSingleton<HudManager>.CMJOLNCMAPD.ShowPopUp(DestroyableSingleton<TranslationController>.CMJOLNCMAPD.GetString(StringNames.GameOverSabotage, new Il2CppReferenceArray<Il2CppSystem.Object>(0)));
            ReviveEveryone();
        }

    }

    internal class PlayerStatistics {
        public int TeamImpostorsAlive {get;set;}
        public int TeamJackalAlive {get;set;}
        public int TeamLoversAlive {get;set;}
        public int TotalAlive {get;set;}
        public bool TeamImpostorHasAliveLover {get;set;}
        public bool TeamJackalHasAliveLover {get;set;}

        public PlayerStatistics(ShipStatus __instance) {
            GetBCFPPIDIMJKs();
        }

        private bool isLover(GameData.OFKOJOKOOAK p) {
            return (Lovers.lover1 != null && Lovers.lover1.PlayerId == p.GMBAIPNOKLP) || (Lovers.lover2 != null && Lovers.lover2.PlayerId == p.GMBAIPNOKLP);
        }

        private void GetBCFPPIDIMJKs() {
            int numJackalAlive = 0;
            int numImpostorsAlive = 0;
            int numLoversAlive = 0;
            int numTotalAlive = 0;
            bool impLover = false;
            bool jackalLover = false;

            for (int i = 0; i < GameData.Instance.BCFPPIDIMJK; i++)
            {
                GameData.OFKOJOKOOAK OFKOJOKOOAK = GameData.Instance.AllPlayers[i];
                if (!OFKOJOKOOAK.GBPMEHJFECK)
                {
                    if (!OFKOJOKOOAK.FGNJJFABIHJ)
                    {
                        numTotalAlive++;

                        bool lover = isLover(OFKOJOKOOAK);
                        if (lover) numLoversAlive++;

                        if (OFKOJOKOOAK.CIDDOFDJHJH) {
                            numImpostorsAlive++;
                            if (lover) impLover = true;
                        }
                        if (Jackal.jackal != null && Jackal.jackal.PlayerId == OFKOJOKOOAK.GMBAIPNOKLP) {
                            numJackalAlive++;
                            if (lover) jackalLover = true;
                        }
                        if (Sidekick.sidekick != null && Sidekick.sidekick.PlayerId == OFKOJOKOOAK.GMBAIPNOKLP) {
                            numJackalAlive++;
                            if (lover) jackalLover = true;
                        }
                    }
                }
            }

            TeamJackalAlive = numJackalAlive;
            TeamImpostorsAlive = numImpostorsAlive;
            TeamLoversAlive = numLoversAlive;
            TotalAlive = numTotalAlive;
            TeamImpostorHasAliveLover = impLover;
            TeamJackalHasAliveLover = jackalLover;
        }
    }
}