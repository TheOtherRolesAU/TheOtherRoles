using HarmonyLib;
using Hazel;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.GameHistory;
using System.Reflection;
using TheOtherRoles.Players;

namespace TheOtherRoles.Patches
{
    [Harmony]
    public class VitalsPatch
    {
        static float vitalsTimer = 0f;
        static TMPro.TextMeshPro TimeRemaining;
        private static List<TMPro.TextMeshPro> hackerTexts = new List<TMPro.TextMeshPro>();

        public static void ResetData()
        {
            vitalsTimer = 0f;
            if (TimeRemaining != null)
            {
                UnityEngine.Object.Destroy(TimeRemaining);
                TimeRemaining = null;
            }
        }

        static void UseVitalsTime()
        {
            // Don't waste network traffic if we're out of time.
            if (MapOptions.restrictDevices > 0 && MapOptions.restrictVitalsTime > 0f && CachedPlayer.LocalPlayer.PlayerControl.isAlive() && CachedPlayer.LocalPlayer.PlayerControl != Hacker.hacker)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.UseVitalsTime, Hazel.SendOption.Reliable, -1);
                writer.Write(vitalsTimer);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.useVitalsTime(vitalsTimer);
            }
            vitalsTimer = 0f;
        }

        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
        class VitalsMinigameStartPatch
        {
            static void Postfix(VitalsMinigame __instance)
            {
                vitalsTimer = 0f;

                if (Hacker.hacker != null && CachedPlayer.LocalPlayer.PlayerControl == Hacker.hacker)
                {
                    hackerTexts = new List<TMPro.TextMeshPro>();
                    foreach (VitalsPanel panel in __instance.vitals)
                    {
                        TMPro.TextMeshPro text = UnityEngine.Object.Instantiate(__instance.SabText, panel.transform);
                        hackerTexts.Add(text);
                        UnityEngine.Object.DestroyImmediate(text.GetComponent<AlphaBlink>());
                        text.gameObject.SetActive(false);
                        text.transform.localScale = Vector3.one * 0.75f;
                        text.transform.localPosition = new Vector3(-0.75f, -0.23f, 0f);

                    }
                }
            }
        }

        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
        class VitalsMinigameUpdatePatch
        {
            static bool Prefix(VitalsMinigame __instance)
            {
                vitalsTimer += Time.deltaTime;
                if (vitalsTimer > 0.1f)
                    UseVitalsTime();

                if (MapOptions.restrictDevices > 0)
                {
                    if (TimeRemaining == null)
                    {
                        TimeRemaining = UnityEngine.Object.Instantiate(HudManager.Instance.TaskText, __instance.transform);
                        TimeRemaining.alignment = TMPro.TextAlignmentOptions.BottomRight;
                        TimeRemaining.transform.position = Vector3.zero;
                        TimeRemaining.transform.localPosition = new Vector3(1.7f, 4.45f);
                        TimeRemaining.transform.localScale *= 1.8f;
                        TimeRemaining.color = Palette.White;
                    }

                    if (MapOptions.restrictVitalsTime <= 0f && CachedPlayer.LocalPlayer.PlayerControl != Hacker.hacker && !CachedPlayer.LocalPlayer.Data.IsDead)
                    {
                        __instance.Close();
                        return false;
                    }

                    string timeString = TimeSpan.FromSeconds(MapOptions.restrictVitalsTime).ToString(@"mm\:ss\.ff");
                    TimeRemaining.text = String.Format("Remaining: {0}", timeString);
                    TimeRemaining.gameObject.SetActive(true);
                }

                return true;
            }

            static void Postfix(VitalsMinigame __instance)
            {

                // Hacker show time since death
                if (Hacker.hacker != null && Hacker.hacker == CachedPlayer.LocalPlayer.PlayerControl && Hacker.hackerTimer > 0)
                {
                    for (int k = 0; k < __instance.vitals.Length; k++)
                    {
                        VitalsPanel vitalsPanel = __instance.vitals[k];
                        GameData.PlayerInfo player = GameData.Instance.AllPlayers[k];

                        // Hacker update
                        if (vitalsPanel.IsDead)
                        {
                            DeadPlayer deadPlayer = deadPlayers?.Where(x => x.player?.PlayerId == player?.PlayerId)?.FirstOrDefault();
                            if (deadPlayer != null && deadPlayer.timeOfDeath != null && k < hackerTexts.Count && hackerTexts[k] != null)
                            {
                                float timeSinceDeath = ((float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds);
                                hackerTexts[k].gameObject.SetActive(true);
                                hackerTexts[k].text = Math.Round(timeSinceDeath / 1000) + "s";
                            }
                        }
                    }
                }
                else
                {
                    foreach (TMPro.TextMeshPro text in hackerTexts)
                        if (text != null && text.gameObject != null)
                            text.gameObject.SetActive(false);
                }
            }
        }
    }
}