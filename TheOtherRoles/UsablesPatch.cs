using HarmonyLib;
using System;
using Hazel;
using UnityEngine;
using System.Linq;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.GameHistory;
using static TheOtherRoles.MapOptions;
using System.Collections.Generic;


namespace TheOtherRoles
{

    [HarmonyPatch(typeof(Vent), "CanUse")]
    public static class VentCanUsePatch
    {
        public static bool Prefix(Vent __instance, ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            float num = float.MaxValue;
            PlayerControl @object = pc.Object;


            bool roleCouldUse = false;
            if (Engineer.engineer != null && Engineer.engineer == @object)
                roleCouldUse = true;
            else if (Jackal.canUseVents && Jackal.jackal != null && Jackal.jackal == @object)
                roleCouldUse = true;
            else if (Sidekick.canUseVents && Sidekick.sidekick != null && Sidekick.sidekick == @object)
                roleCouldUse = true;
            else if (pc.IsImpostor) {
                if (Janitor.janitor != null && Janitor.janitor == PlayerControl.LocalPlayer)
                    roleCouldUse = false;
                else if (Mafioso.mafioso != null && Mafioso.mafioso == PlayerControl.LocalPlayer && Godfather.godfather != null && !Godfather.godfather.Data.IsDead)
                    roleCouldUse = false;
                else
                    roleCouldUse = true;
            }

            var usableDistance = __instance.UsableDistance;
            if (__instance.name.StartsWith("JackInTheBoxVent_")) {
                if(Trickster.trickster != PlayerControl.LocalPlayer) {
                    // Only the Trickster can use the Jack-In-The-Boxes!
                    canUse = false;
                    couldUse = false;
                    __result = num;
                    return false; 
                } else {
                    // Reduce the usable distance to reduce the risk of gettings stuck while trying to jump into the box if it's placed near objects
                    usableDistance = 0.4f; 
                }
            }

            couldUse = ((@object.inVent || roleCouldUse) && !pc.IsDead && (@object.CanMove || @object.inVent));
            canUse = couldUse;
            if (canUse)
            {
                Vector2 truePosition = @object.GetTruePosition();
                Vector3 position = __instance.transform.position;
                num = Vector2.Distance(truePosition, position);
                
                canUse &= (num <= usableDistance && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false));
            }
            __result = num;
            return false;
        }
    }

    [HarmonyPatch(typeof(Vent), "Use")]
    public static class VentUsePatch {
        public static bool Prefix(Vent __instance) {
            bool flag;
            bool flag2;
            __instance.CanUse(PlayerControl.LocalPlayer.Data, out flag, out flag2);

            if (!flag || !__instance.name.StartsWith("JackInTheBoxVent_")) return true;  // Continue with default method

            bool isEnter = !PlayerControl.LocalPlayer.inVent;

            __instance.SetButtons(isEnter);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UseUncheckedVent, Hazel.SendOption.Reliable);
            writer.WritePacked(__instance.Id);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(isEnter ? byte.MaxValue : (byte)0);
            writer.EndMessage();
            RPCProcedure.useUncheckedVent(__instance.Id, PlayerControl.LocalPlayer.PlayerId, isEnter ? byte.MaxValue : (byte)0);

            return false;
        }
    }

    [HarmonyPatch(typeof(UseButtonManager), nameof(UseButtonManager.SetTarget))]
    class UseButtonSetTargetPatch {
        static void Postfix(UseButtonManager __instance) {
            // Trickster render special vent button
            if (__instance.currentTarget != null && Trickster.trickster != null && Trickster.trickster == PlayerControl.LocalPlayer) {
                Vent possibleVent =  __instance.currentTarget.TryCast<Vent>();
                if (possibleVent != null && possibleVent.gameObject != null && possibleVent.gameObject.name.StartsWith("JackInTheBoxVent_")) {
                    __instance.UseButton.sprite = Trickster.getTricksterVentButtonSprite();
                }
            }

            // Mafia sabotage button render patch
            bool blockSabotageJanitor = (Janitor.janitor != null && Janitor.janitor == PlayerControl.LocalPlayer);
            bool blockSabotageMafioso = (Mafioso.mafioso != null && Mafioso.mafioso == PlayerControl.LocalPlayer && Godfather.godfather != null && !Godfather.godfather.Data.IsDead);
            if (__instance.currentTarget == null && (blockSabotageJanitor || blockSabotageMafioso)) {
                __instance.UseButton.sprite = DestroyableSingleton<TranslationController>.Instance.GetImage(ImageNames.UseButton);
                __instance.UseButton.color = new Color(1f, 1f, 1f, 0.3f);
            }

        }
    }

    [HarmonyPatch(typeof(UseButtonManager), nameof(UseButtonManager.DoClick))]
    class UseButtonDoClickPatch {
        static bool Prefix(UseButtonManager __instance) { 
            if (__instance.currentTarget != null) return true;

            // Mafia sabotage button click patch
            bool blockSabotageJanitor = (Janitor.janitor != null && Janitor.janitor == PlayerControl.LocalPlayer);
            bool blockSabotageMafioso = (Mafioso.mafioso != null && Mafioso.mafioso == PlayerControl.LocalPlayer && Godfather.godfather != null && !Godfather.godfather.Data.IsDead);
            if (blockSabotageJanitor || blockSabotageMafioso) return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
    class EmergencyMinigameUpdatePatch {
        static void Postfix(EmergencyMinigame __instance) {
            var roleCanCallEmergency = true;
            var statusText = "";

            // Deactivate emergency button for Swapper
            if (Swapper.swapper != null && Swapper.swapper == PlayerControl.LocalPlayer && !Swapper.canCallEmergency) {
                roleCanCallEmergency = false;
                statusText = "The Swapper can't start an emergency meeting";
            }
            // Potentially deactivate emergency button for Jester
            if (Jester.jester != null && Jester.jester == PlayerControl.LocalPlayer && !Jester.canCallEmergency) {
                roleCanCallEmergency = false;
                statusText = "The Jester can't start an emergency meeting";
            }

            if (!roleCanCallEmergency) {
                __instance.StatusText.text = statusText;
                __instance.NumberText.text = string.Empty;
                __instance.ClosedLid.gameObject.SetActive(true);
                __instance.OpenLid.gameObject.SetActive(false);
                __instance.ButtonActive = false;
                return;
            }

            // Handle max number of meetings
            if (__instance.state == 1) {
                int localRemaining = PlayerControl.LocalPlayer.RemainingEmergencies;
                int teamRemaining = Mathf.Max(0, maxNumberOfMeetings - meetingsCount);
                int remaining = Mathf.Min(localRemaining, (Mayor.mayor != null && Mayor.mayor == PlayerControl.LocalPlayer) ? 1 : teamRemaining);
                __instance.NumberText.text = $"{localRemaining.ToString()} and the ship has {teamRemaining.ToString()}";
                __instance.ButtonActive = remaining > 0;
                __instance.ClosedLid.gameObject.SetActive(!__instance.ButtonActive);
                __instance.OpenLid.gameObject.SetActive(__instance.ButtonActive);
				return;
			}
        }
    }


    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    public static class ConsoleCanUsePatch {
        public static bool Prefix(ref float __result, Console __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse) {
            canUse = couldUse = false;
            if (Swapper.swapper != null && Swapper.swapper == PlayerControl.LocalPlayer)
                return !__instance.TaskTypes.Any(x => x == TaskTypes.FixLights || x == TaskTypes.FixComms);
            if (__instance.AllowImpostor) return true;
            if (!Helpers.hasFakeTasks(pc.Object)) return true;
            __result = float.MaxValue;
            return false;
        }
    }


    [HarmonyPatch(typeof(TuneRadioMinigame), nameof(TuneRadioMinigame.Begin))]
    class CommsMinigameBeginPatch {
        static void Postfix(TuneRadioMinigame __instance) {
            // Block Swapper from fixing comms. Still looking for a better way to do this, but deleting the task doesn't seem like a viable option since then the camera, admin table, ... work while comms are out
            if (Swapper.swapper != null && Swapper.swapper == PlayerControl.LocalPlayer) {
                __instance.Close();
            }
        }
    }

    [HarmonyPatch(typeof(SwitchMinigame), nameof(SwitchMinigame.Begin))]
    class LightsMinigameBeginPatch {
        static void Postfix(SwitchMinigame __instance) {
            // Block Swapper from fixing lights. One could also just delete the PlayerTask, but I wanted to do it the same way as with coms for now.
            if (Swapper.swapper != null && Swapper.swapper == PlayerControl.LocalPlayer) {
                __instance.Close();
            }
        }
    }

    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
    class VitalsMinigameBeginPatch {
        static void Postfix(VitalsMinigame __instance) {

            if (__instance.vitals.Length > 10) {
                for (int i = 0; i < __instance.vitals.Length; i++) {
                    var vitalsPanel = __instance.vitals[i];
                    var player = GameData.Instance.AllPlayers[i];
                    vitalsPanel.Text.text = player.PlayerName.Length >= 4 ? player.PlayerName.Substring(0, 4).ToUpper() : player.PlayerName.ToUpper();
                }
            }
        }
    }
    

    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
    class VitalsMinigameUpdatePatch {

        static void Postfix(VitalsMinigame __instance) {
            // Hacker show time since death
            bool showHackerInfo = Hacker.hacker != null && Hacker.hacker == PlayerControl.LocalPlayer && Hacker.hackerTimer > 0;
            for (int k = 0; k < __instance.vitals.Length; k++)
            {
                VitalsPanel vitalsPanel = __instance.vitals[k];
                GameData.PlayerInfo player = GameData.Instance.AllPlayers[k];

                // Crowded scaling
                float scale = 10f / Mathf.Max(10, __instance.vitals.Length);
                vitalsPanel.transform.localPosition = new Vector3((float)k * 0.6f * scale + -2.7f, 0.2f, -1f);
                vitalsPanel.transform.localScale = new Vector3(scale, scale, vitalsPanel.transform.localScale.z);

                // Hacker update
                if (vitalsPanel.IsDead) {
                    DeadPlayer deadPlayer = deadPlayers?.Where(x => x.player?.PlayerId == player?.PlayerId)?.FirstOrDefault();
                    if (deadPlayer != null && deadPlayer.timeOfDeath != null) {
                        float timeSinceDeath = ((float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds);

                        if (showHackerInfo)
                            vitalsPanel.Text.text = Math.Round(timeSinceDeath / 1000) + "s";
                        else if (__instance.vitals.Length > 10)
                            vitalsPanel.Text.text = player.PlayerName.Length >= 4 ? player.PlayerName.Substring(0, 4).ToUpper() : player.PlayerName.ToUpper();
                        else 
                            vitalsPanel.Text.text = DestroyableSingleton<TranslationController>.Instance.GetString(Palette.ShortColorNames[(int)player.ColorId], new UnhollowerBaseLib.Il2CppReferenceArray<Il2CppSystem.Object>(0));
                    }
                }
	    	}
        }
    }    

    [HarmonyPatch]
    class AdminPanelPatch {
        static Dictionary<SystemTypes, List<Color>> players = new Dictionary<SystemTypes, List<Color>>();

        [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
        class MapCountOverlayUpdatePatch {
            static bool Prefix(MapCountOverlay __instance) {
                // Save colors for the Hacker
                __instance.timer += Time.deltaTime;
                if (__instance.timer < 0.1f)
                {
                    return false;
                }
                __instance.timer = 0f;
                players = new Dictionary<SystemTypes, List<Color>>();
                bool commsActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                        if (task.TaskType == TaskTypes.FixComms) commsActive = true;       


                if (!__instance.isSab && commsActive)
                {
                    __instance.isSab = true;
                    __instance.BackgroundColor.SetColor(Palette.DisabledGrey);
                    __instance.SabotageText.gameObject.SetActive(true);
                    return false;
                }
                if (__instance.isSab && !commsActive)
                {
                    __instance.isSab = false;
                    __instance.BackgroundColor.SetColor(Color.green);
                    __instance.SabotageText.gameObject.SetActive(false);
                }

                for (int i = 0; i < __instance.CountAreas.Length; i++)
                {
                    CounterArea counterArea = __instance.CountAreas[i];
                    List<Color> roomColors = new List<Color>();
                    players.Add(counterArea.RoomType, roomColors);

                    if (!commsActive)
                    {
                        PlainShipRoom plainShipRoom = ShipStatus.Instance.FastRooms[counterArea.RoomType];

                        if (plainShipRoom != null && plainShipRoom.roomArea)
                        {
                            int num = plainShipRoom.roomArea.OverlapCollider(__instance.filter, __instance.buffer);
                            int num2 = num;
                            for (int j = 0; j < num; j++)
                            {
                                Collider2D collider2D = __instance.buffer[j];
                                if (!(collider2D.tag == "DeadBody"))
                                {
                                    PlayerControl component = collider2D.GetComponent<PlayerControl>();
                                    if (!component || component.Data == null || component.Data.Disconnected || component.Data.IsDead)
                                    {
                                        num2--;
                                    } else if (component?.myRend?.material != null) {
                                        Color color = component.myRend.material.GetColor("_BodyColor");
                                        if (Hacker.onlyColorType) {
                                            var id = Mathf.Max(0, Palette.PlayerColors.IndexOf(color));
                                            color = Helpers.isLighterColor((byte)id) ? Palette.PlayerColors[7] : Palette.PlayerColors[6];
                                        }
                                        roomColors.Add(color);
                                    }
                                } else {
                                    DeadBody component = collider2D.GetComponent<DeadBody>();
                                    if (component) {
                                        GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
                                        if (playerInfo != null) {
                                            var color = Palette.PlayerColors[playerInfo.ColorId];
                                            if (Hacker.onlyColorType)
                                                color = Helpers.isLighterColor(playerInfo.ColorId) ? Palette.PlayerColors[7] : Palette.PlayerColors[6];
                                            roomColors.Add(color);
                                        }
                                    }
                                }
                            }
                            counterArea.UpdateCount(num2);
                        }
                        else
                        {
                            Debug.LogWarning("Couldn't find counter for:" + counterArea.RoomType);
                        }
                    }
                    else
                    {
                        counterArea.UpdateCount(0);
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(CounterArea), nameof(CounterArea.UpdateCount))]
        class CounterAreaUpdateCountPatch {
            private static Sprite defaultIcon;

            static void Postfix(CounterArea __instance) {
                // Hacker display saved colors on the admin panel
                bool showHackerInfo = Hacker.hacker != null && Hacker.hacker == PlayerControl.LocalPlayer && Hacker.hackerTimer > 0;
                if (players.ContainsKey(__instance.RoomType)) {
                    List<Color> colors = players[__instance.RoomType];

                    for (int i = 0; i < __instance.myIcons.Count; i++) {
                        PoolableBehavior icon = __instance.myIcons[i];
                        SpriteRenderer renderer = icon.GetComponent<SpriteRenderer>();

                        if (renderer != null) {
                            if (defaultIcon == null) defaultIcon = renderer.sprite;
                            if (showHackerInfo && colors.Count > i && Hacker.getAdminTableIconSprite() != null) {
                                renderer.sprite = Hacker.getAdminTableIconSprite();
                                renderer.color = colors[i];
                            } else {
                                renderer.sprite = defaultIcon;
                                renderer.color = Color.white;
                            }
                        } 
                    }
                }
            }
        }
    }
}