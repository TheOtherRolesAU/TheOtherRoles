using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using TheOtherRoles.Roles;
using TMPro;
using UnityEngine;
using static TheOtherRoles.GameHistory;
using static TheOtherRoles.MapOptions;
using Object = UnityEngine.Object;


namespace TheOtherRoles.Patches
{
    [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
    public static class VentCanUsePatch
    {
        public static bool Prefix(Vent __instance, ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo pc,
            [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            var num = float.MaxValue;
            var @object = pc.Object;


            var roleCouldUse = false;
            if (Engineer.Instance.player == @object)
                roleCouldUse = true;
            else if (Jackal.canUseVents && Jackal.Instance.player == @object)
                roleCouldUse = true;
            else if (Sidekick.canUseVents && Sidekick.Instance.player == @object)
                roleCouldUse = true;
            else if (Spy.canEnterVents && Spy.Instance.player == @object)
                roleCouldUse = true;
            else if (pc.IsImpostor) roleCouldUse = true;

            var usableDistance = __instance.UsableDistance;
            if (__instance.name.StartsWith("JackInTheBoxVent_"))
            {
                if (Trickster.Instance.player != PlayerControl.LocalPlayer)
                {
                    // Only the Trickster can use the Jack-In-The-Boxes!
                    canUse = false;
                    couldUse = false;
                    __result = num;
                    return false;
                }

                // Reduce the usable distance to reduce the risk of getting stuck while trying to jump into the box if it's placed near objects
                usableDistance = 0.4f;
            }
            else if (__instance.name.StartsWith("SealedVent_"))
            {
                canUse = couldUse = false;
                __result = num;
                return false;
            }

            couldUse = (@object.inVent || roleCouldUse) && !pc.IsDead && (@object.CanMove || @object.inVent);
            canUse = couldUse;
            if (canUse)
            {
                var truePosition = @object.GetTruePosition();
                var position = __instance.transform.position;
                num = Vector2.Distance(truePosition, position);

                canUse &= num <= usableDistance &&
                          !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false);
            }

            __result = num;
            return false;
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.Use))]
    public static class VentUsePatch
    {
        public static bool Prefix(Vent __instance)
        {
            __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out _);
            var canMoveInVents = true;
            if (!canUse) return false; // No need to execute the native method as using is disallowed anyways
            if (Spy.Instance.player == PlayerControl.LocalPlayer) canMoveInVents = false;
            var isEnter = !PlayerControl.LocalPlayer.inVent;

            if (__instance.name.StartsWith("JackInTheBoxVent_"))
            {
                __instance.SetButtons(isEnter && canMoveInVents);
                var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.UseUncheckedVent, SendOption.Reliable);
                writer.WritePacked(__instance.Id);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write(isEnter ? byte.MaxValue : (byte) 0);
                writer.EndMessage();
                RPCProcedure.UseUncheckedVent(__instance.Id, PlayerControl.LocalPlayer.PlayerId,
                    isEnter ? byte.MaxValue : (byte) 0);
                return false;
            }

            if (isEnter)
                PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(__instance.Id);
            else
                PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(__instance.Id);
            __instance.SetButtons(isEnter && canMoveInVents);
            return false;
        }
    }

    [HarmonyPatch(typeof(UseButtonManager), nameof(UseButtonManager.SetTarget))]
    internal class UseButtonSetTargetPatch
    {
        private static void Postfix(UseButtonManager __instance)
        {
            // Trickster render special vent button
            UseButton useButton;
            if (__instance.currentTarget != null && Trickster.Instance.player != null &&
                Trickster.Instance.player == PlayerControl.LocalPlayer)
            {
                var possibleVent = __instance.currentTarget.TryCast<Vent>();
                if (possibleVent && possibleVent.gameObject)
                {
                    useButton = __instance.currentButtonShown;
                    if (possibleVent.gameObject.name.StartsWith("JackInTheBoxVent_"))
                    {
                        useButton.graphic.sprite = Trickster.GetTricksterVentButtonSprite();
                        useButton.text.enabled = false; // clear text;
                    }
                    else
                    {
                        useButton.graphic.sprite =
                            DestroyableSingleton<TranslationController>.Instance.GetImage(ImageNames.VentButton);
                        useButton.text.enabled = false;
                    }
                }
            }

            // Jester sabotage
            if (Jester.canSabotage && Jester.Instance.player == PlayerControl.LocalPlayer &&
                PlayerControl.LocalPlayer.CanMove)
            {
                useButton = __instance.currentButtonShown;
                if (!Jester.Instance.player.Data.IsDead && __instance.currentTarget == null)
                {
                    // no target, so sabotage
                    useButton.graphic.sprite =
                        DestroyableSingleton<TranslationController>.Instance.GetImage(ImageNames.SabotageButton);
                    useButton.graphic.color = UseButtonManager.EnabledColor;
                    useButton.text.enabled = false;
                }
                else
                {
                    useButton.graphic.sprite =
                        DestroyableSingleton<TranslationController>.Instance.GetImage(ImageNames.UseButton);
                    useButton.text.enabled = false;
                }
            }

            // Mafia sabotage button render patch
            var blockSabotageJanitor = Janitor.Instance.player == PlayerControl.LocalPlayer;
            var blockSabotageMafioso = Mafioso.Instance.player == PlayerControl.LocalPlayer &&
                                       Godfather.Instance.player != null && !Godfather.Instance.player.Data.IsDead;
            if (__instance.currentTarget != null || !blockSabotageJanitor && !blockSabotageMafioso) return;

            useButton = __instance.currentButtonShown;
            useButton.graphic.sprite =
                DestroyableSingleton<TranslationController>.Instance.GetImage(ImageNames.UseButton);
            useButton.graphic.color = UseButtonManager.DisabledColor;
            useButton.text.enabled = false;
        }
    }

    [HarmonyPatch(typeof(UseButtonManager), nameof(UseButtonManager.DoClick))]
    internal class UseButtonDoClickPatch
    {
        private static bool Prefix(UseButtonManager __instance)
        {
            if (__instance.currentTarget != null) return true;
            // Jester sabotage
            if (Jester.canSabotage && Jester.Instance.player != null &&
                Jester.Instance.player == PlayerControl.LocalPlayer &&
                !PlayerControl.LocalPlayer.Data.IsDead)
            {
                Action<MapBehaviour> action = m => m.ShowInfectedMap();
                DestroyableSingleton<HudManager>.Instance.ShowMap(action);
                return false;
            }

            // Mafia sabotage button click patch
            var blockSabotageJanitor = Janitor.Instance.player == PlayerControl.LocalPlayer;
            var blockSabotageMafioso = Mafioso.Instance.player == PlayerControl.LocalPlayer &&
                                       Godfather.Instance.player != null && !Godfather.Instance.player.Data.IsDead;
            return !blockSabotageJanitor && !blockSabotageMafioso;
        }
    }

    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
    internal class EmergencyMinigameUpdatePatch
    {
        private static void Postfix(EmergencyMinigame __instance)
        {
            var roleCanCallEmergency = true;
            var statusText = "";

            // Deactivate emergency button for Swapper
            if (Swapper.Instance.player != null && Swapper.Instance.player == PlayerControl.LocalPlayer &&
                !Swapper.canCallEmergency)
            {
                roleCanCallEmergency = false;
                statusText = "The Swapper can't start an emergency meeting";
            }

            // Potentially deactivate emergency button for Jester
            if (Jester.Instance.player != null && Jester.Instance.player == PlayerControl.LocalPlayer &&
                !Jester.canCallEmergency)
            {
                roleCanCallEmergency = false;
                statusText = "The Jester can't start an emergency meeting";
            }

            if (!roleCanCallEmergency)
            {
                __instance.StatusText.text = statusText;
                __instance.NumberText.text = string.Empty;
                __instance.ClosedLid.gameObject.SetActive(true);
                __instance.OpenLid.gameObject.SetActive(false);
                __instance.ButtonActive = false;
                return;
            }

            // Handle max number of meetings
            if (__instance.state != 1) return;
            var localRemaining = PlayerControl.LocalPlayer.RemainingEmergencies;
            var teamRemaining = Mathf.Max(0, maxNumberOfMeetings - meetingsCount);
            var remaining = Mathf.Min(localRemaining,
                Mayor.Instance.player != null && Mayor.Instance.player == PlayerControl.LocalPlayer
                    ? 1
                    : teamRemaining);
            __instance.NumberText.text = $"{localRemaining.ToString()} and the ship has {teamRemaining.ToString()}";
            __instance.ButtonActive = remaining > 0;
            __instance.ClosedLid.gameObject.SetActive(!__instance.ButtonActive);
            __instance.OpenLid.gameObject.SetActive(__instance.ButtonActive);
        }
    }


    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    public static class ConsoleCanUsePatch
    {
        public static bool Prefix(ref float __result, Console __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc,
            [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            canUse = couldUse = false;
            if (Swapper.Instance.player == PlayerControl.LocalPlayer)
                return !__instance.TaskTypes.Any(x => x is TaskTypes.FixLights or TaskTypes.FixComms);
            if (__instance.AllowImpostor) return true;
            if (!pc.Object.HasFakeTasks()) return true;
            __result = float.MaxValue;
            return false;
        }
    }

    [HarmonyPatch(typeof(TuneRadioMinigame), nameof(TuneRadioMinigame.Begin))]
    internal class CommsMinigameBeginPatch
    {
        private static void Postfix(TuneRadioMinigame __instance)
        {
            // Block Swapper from fixing comms. Still looking for a better way to do this, but deleting the task doesn't seem like a viable option since then the camera, admin table, ... work while comms are out
            if (Swapper.Instance.player != null && Swapper.Instance.player == PlayerControl.LocalPlayer)
                __instance.Close();
        }
    }

    [HarmonyPatch(typeof(SwitchMinigame), nameof(SwitchMinigame.Begin))]
    internal class LightsMinigameBeginPatch
    {
        private static void Postfix(SwitchMinigame __instance)
        {
            // Block Swapper from fixing lights. One could also just delete the PlayerTask, but I wanted to do it the same way as with comms for now.
            if (Swapper.Instance.player != null && Swapper.Instance.player == PlayerControl.LocalPlayer)
                __instance.Close();
        }
    }

    [HarmonyPatch]
    internal class VitalsMinigamePatch
    {
        private static List<TextMeshPro> hackerTexts = new();

        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
        private class VitalsMinigameStartPatch
        {
            private static void Postfix(VitalsMinigame __instance)
            {
                if (!Hacker.Instance.player || Hacker.Instance.player != PlayerControl.LocalPlayer) return;
                hackerTexts = new List<TextMeshPro>();
                foreach (var panel in __instance.vitals)
                {
                    var text = Object.Instantiate(__instance.SabText, panel.transform);
                    hackerTexts.Add(text);
                    Object.DestroyImmediate(text.GetComponent<AlphaBlink>());
                    text.gameObject.SetActive(false);
                    text.transform.localScale = Vector3.one * 0.75f;
                    text.transform.localPosition = new Vector3(-0.75f, -0.23f, 0f);
                }
            }
        }

        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
        private class VitalsMinigameUpdatePatch
        {
            private static void Postfix(VitalsMinigame __instance)
            {
                // Hacker show time since death

                if (Hacker.Instance.player != null && Hacker.Instance.player == PlayerControl.LocalPlayer &&
                    Hacker.hackerTimer > 0)
                    for (var k = 0; k < __instance.vitals.Length; k++)
                    {
                        var vitalsPanel = __instance.vitals[k];
                        var player = GameData.Instance.AllPlayers.ToArray()[k];

                        // Hacker update
                        if (!vitalsPanel.IsDead) continue;
                        var deadPlayer = deadPlayers?.Where(x => x.player && x.player.PlayerId == player?.PlayerId)
                            .FirstOrDefault();
                        if (deadPlayer == null || k >= hackerTexts.Count || hackerTexts[k] == null) continue;
                        var timeSinceDeath =
                            (float) (DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds;
                        hackerTexts[k].gameObject.SetActive(true);
                        hackerTexts[k].text = Math.Round(timeSinceDeath / 1000) + "s";
                    }
                else
                    foreach (var text in hackerTexts.Where(text => text != null && text.gameObject != null))
                        text.gameObject.SetActive(false);
            }
        }
    }

    [HarmonyPatch]
    internal class AdminPanelPatch
    {
        private static Dictionary<SystemTypes, List<Color>> players = new();
        private static readonly int VisorColor = Shader.PropertyToID("_VisorColor");
        private static readonly int BackColor = Shader.PropertyToID("_BackColor");
        private static readonly int BodyColor = Shader.PropertyToID("_BodyColor");

        [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
        private class MapCountOverlayUpdatePatch
        {
            private static bool Prefix(MapCountOverlay __instance)
            {
                // Save colors for the Hacker
                __instance.timer += Time.deltaTime;
                if (__instance.timer < 0.1f) return false;
                __instance.timer = 0f;
                players = new Dictionary<SystemTypes, List<Color>>();
                var commsActive = false;
                foreach (var task in PlayerControl.LocalPlayer.myTasks)
                    if (task.TaskType == TaskTypes.FixComms)
                        commsActive = true;


                switch (__instance.isSab)
                {
                    case false when commsActive:
                        __instance.isSab = true;
                        __instance.BackgroundColor.SetColor(Palette.DisabledGrey);
                        __instance.SabotageText.gameObject.SetActive(true);
                        return false;
                    case true when !commsActive:
                        __instance.isSab = false;
                        __instance.BackgroundColor.SetColor(Color.green);
                        __instance.SabotageText.gameObject.SetActive(false);
                        break;
                }

                foreach (var counterArea in __instance.CountAreas)
                {
                    var roomColors = new List<Color>();
                    players.Add(counterArea.RoomType, roomColors);

                    if (!commsActive)
                    {
                        var plainShipRoom = ShipStatus.Instance.FastRooms[counterArea.RoomType];

                        if (plainShipRoom && plainShipRoom.roomArea)
                        {
                            var num = plainShipRoom.roomArea.OverlapCollider(__instance.filter, __instance.buffer);
                            var num2 = num;
                            for (var j = 0; j < num; j++)
                            {
                                var collider2D = __instance.buffer[j];
                                if (!collider2D.CompareTag("DeadBody"))
                                {
                                    var component = collider2D.GetComponent<PlayerControl>();
                                    if (!component || component.Data == null || component.Data.Disconnected ||
                                        component.Data.IsDead)
                                    {
                                        num2--;
                                    }
                                    else if (component.myRend && component.myRend.material)
                                    {
                                        var color = component.myRend.material.GetColor(BodyColor);
                                        if (Hacker.onlyColorType)
                                        {
                                            var id = Mathf.Max(0, Palette.PlayerColors.IndexOf(color));
                                            color = Helpers.IsLighterColor((byte) id)
                                                ? Palette.PlayerColors[7]
                                                : Palette.PlayerColors[6];
                                        }

                                        roomColors.Add(color);
                                    }
                                }
                                else
                                {
                                    var component = collider2D.GetComponent<DeadBody>();
                                    if (!component) continue;
                                    var playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
                                    if (playerInfo == null) continue;
                                    var color = Palette.PlayerColors[playerInfo.ColorId];
                                    if (Hacker.onlyColorType)
                                        color = Helpers.IsLighterColor(playerInfo.ColorId)
                                            ? Palette.PlayerColors[7]
                                            : Palette.PlayerColors[6];
                                    roomColors.Add(color);
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
        private class CounterAreaUpdateCountPatch
        {
            private static Material defaultMat;
            private static Material newMat;

            private static void Postfix(CounterArea __instance)
            {
                // Hacker display saved colors on the admin panel
                var showHackerInfo = Hacker.Instance.player != null &&
                                     Hacker.Instance.player == PlayerControl.LocalPlayer &&
                                     Hacker.hackerTimer > 0;
                if (!players.ContainsKey(__instance.RoomType)) return;
                var colors = players[__instance.RoomType];

                for (var i = 0; i < __instance.myIcons.Count; i++)
                {
                    var icon = __instance.myIcons.ToArray()[i];
                    var renderer = icon.GetComponent<SpriteRenderer>();

                    if (renderer == null) continue;
                    if (defaultMat == null) defaultMat = renderer.material;
                    if (newMat == null) newMat = Object.Instantiate(defaultMat);
                    if (showHackerInfo && colors.Count > i)
                    {
                        renderer.material = newMat;
                        var color = colors[i];
                        renderer.material.SetColor(BodyColor, color);
                        var id = Palette.PlayerColors.IndexOf(color);
                        if (id < 0)
                            renderer.material.SetColor(BackColor, color);
                        else
                            renderer.material.SetColor(BackColor, Palette.ShadowColors[id]);
                        renderer.material.SetColor(VisorColor, Palette.VisorColor);
                    }
                    else
                    {
                        renderer.material = defaultMat;
                    }
                }
            }
        }
    }

    [HarmonyPatch]
    internal class SurveillanceMinigamePatch
    {
        private static int page;
        private static float timer;
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
        private class SurveillanceMinigameBeginPatch
        {
            public static void Postfix(SurveillanceMinigame __instance)
            {
                // Add securityGuard cameras
                page = 0;
                timer = 0;
                if (ShipStatus.Instance.AllCameras.Length <= 4 || __instance.FilteredRooms.Length <= 0) return;

                __instance.textures = __instance.textures.ToList()
                    .Concat(new RenderTexture[ShipStatus.Instance.AllCameras.Length - 4]).ToArray();
                for (var i = 4; i < ShipStatus.Instance.AllCameras.Length; i++)
                {
                    var surv = ShipStatus.Instance.AllCameras[i];
                    var camera = Object.Instantiate(__instance.CameraPrefab);
                    camera.transform.SetParent(__instance.transform);
                    camera.transform.position =
                        new Vector3(surv.transform.position.x, surv.transform.position.y, 8f);
                    camera.orthographicSize = 2.35f;
                    var temporary = RenderTexture.GetTemporary(256, 256, 16, (RenderTextureFormat) 0);
                    __instance.textures[i] = temporary;
                    camera.targetTexture = temporary;
                }
            }
        }

        [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update))]
        private class SurveillanceMinigameUpdatePatch
        {
            public static bool Prefix(SurveillanceMinigame __instance)
            {
                // Update normal and securityGuard cameras
                timer += Time.deltaTime;
                var numberOfPages = Mathf.CeilToInt(ShipStatus.Instance.AllCameras.Length / 4f);

                if (timer > 3f || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    timer = 0f;
                    page = (page + 1) % numberOfPages;
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    page = (page + numberOfPages - 1) % numberOfPages;
                    timer = 0f;
                }

                switch (__instance.isStatic)
                {
                    case true or true when !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer):
                    {
                        __instance.isStatic = false;
                        for (var i = 0; i < __instance.ViewPorts.Length; i++)
                        {
                            __instance.ViewPorts[i].sharedMaterial = __instance.DefaultMaterial;
                            __instance.SabText[i].gameObject.SetActive(false);
                            if (page * 4 + i < __instance.textures.Length)
                                __instance.ViewPorts[i].material.SetTexture(MainTex, __instance.textures[page * 4 + i]);
                            else
                                __instance.ViewPorts[i].sharedMaterial = __instance.StaticMaterial;
                        }

                        break;
                    }
                    case false when PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer):
                    {
                        __instance.isStatic = true;
                        for (var j = 0; j < __instance.ViewPorts.Length; j++)
                        {
                            __instance.ViewPorts[j].sharedMaterial = __instance.StaticMaterial;
                            __instance.SabText[j].gameObject.SetActive(true);
                        }

                        break;
                    }
                }

                return false;
            }
        }
    }
}