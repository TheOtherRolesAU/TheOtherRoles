  
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Hazel;
using System;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using System.Linq;
using Reactor.Utilities.Extensions;

namespace TheOtherRoles.Patches {
    public class GameStartManagerPatch  {
        public static Dictionary<int, PlayerVersion> playerVersions = new Dictionary<int, PlayerVersion>();
        public static float timer = 600f;
        private static float kickingTimer = 0f;
        private static bool versionSent = false;
        private static string lobbyCodeText = "";

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public class AmongUsClientOnPlayerJoinedPatch {
            public static void Postfix(AmongUsClient __instance) {
                if (CachedPlayer.LocalPlayer != null) {
                    Helpers.shareGameVersion();
                }
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch {
            public static void Postfix(GameStartManager __instance) {
                // Trigger version refresh
                versionSent = false;
                // Reset lobby countdown timer
                timer = 600f; 
                // Reset kicking timer
                kickingTimer = 0f;
                // Copy lobby code
                string code = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
                GUIUtility.systemCopyBuffer = code;
                lobbyCodeText = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoomCode, new Il2CppReferenceArray<Il2CppSystem.Object>(0)) + "\r\n" + code;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch {
            public static float startingTimer = 0;
            private static bool update = false;
            private static string currentText = "";
            private static GameObject copiedStartButton;

            public static void Prefix(GameStartManager __instance) {
                if (!GameData.Instance ) return; // No instance
                __instance.MinPlayers = 1;
                update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
            }

            public static void Postfix(GameStartManager __instance) {
                // Send version as soon as CachedPlayer.LocalPlayer.PlayerControl exists
                if (PlayerControl.LocalPlayer != null && !versionSent) {
                    versionSent = true;
                    Helpers.shareGameVersion();
                }

                // Check version handshake infos

                bool versionMismatch = false;
                string message = "";
                foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.ToArray()) {
                    if (client.Character == null) continue;
                    var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                    if (dummyComponent != null && dummyComponent.enabled)
                        continue;
                    else if (!playerVersions.ContainsKey(client.Id))  {
                        versionMismatch = true;
                        message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} has a different or no version of The Other Roles\n</color>";
                    } else {
                        PlayerVersion PV = playerVersions[client.Id];
                        int diff = TheOtherRolesPlugin.Version.CompareTo(PV.version);
                        if (diff > 0) {
                            message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} has an older version of The Other Roles (v{playerVersions[client.Id].version.ToString()})\n</color>";
                            versionMismatch = true;
                        } else if (diff < 0) {
                            message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} has a newer version of The Other Roles (v{playerVersions[client.Id].version.ToString()})\n</color>";
                            versionMismatch = true;
                        } else if (!PV.GuidMatches()) { // version presumably matches, check if Guid matches
                            message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} has a modified version of TOR v{playerVersions[client.Id].version.ToString()} <size=30%>({PV.guid.ToString()})</size>\n</color>";
                            versionMismatch = true;
                        }
                    }
                }

                // Display message to the host
                if (AmongUsClient.Instance.AmHost) {
                    if (versionMismatch) {
                        __instance.StartButton.color = __instance.startLabelText.color = Palette.DisabledClear;
                        __instance.GameStartText.text = message;
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    } else {
                        __instance.StartButton.color = __instance.startLabelText.color = ((__instance.LastPlayerCount >= __instance.MinPlayers) ? Palette.EnabledColor : Palette.DisabledClear);
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                    }

                    if (__instance.startState != GameStartManager.StartingStates.Countdown)
                        copiedStartButton?.Destroy();

                    // Make starting info available to clients:
                    if (startingTimer <= 0 && __instance.startState == GameStartManager.StartingStates.Countdown) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.SetGameStarting, Hazel.SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.setGameStarting();

                        // Activate Stop-Button
                        copiedStartButton = GameObject.Instantiate(__instance.StartButton.gameObject, __instance.StartButton.gameObject.transform.parent);
                        copiedStartButton.transform.localPosition = __instance.StartButton.transform.localPosition;
                        copiedStartButton.GetComponent<SpriteRenderer>().sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.StopClean.png", 180f);
                        copiedStartButton.SetActive(true);
                        var startButtonText = copiedStartButton.GetComponentInChildren<TMPro.TextMeshPro>();
                        startButtonText.text = "STOP";
                        startButtonText.fontSize *= 0.8f;
                        startButtonText.fontSizeMax = startButtonText.fontSize;
                        startButtonText.gameObject.transform.localPosition = Vector3.zero;
                        PassiveButton startButtonPassiveButton = copiedStartButton.GetComponent<PassiveButton>();

                        void StopStartFunc() {
                            __instance.ResetStartState();
                            copiedStartButton.Destroy();
                            startingTimer = 0;
                        }
                        startButtonPassiveButton.OnClick.AddListener((Action)(() => StopStartFunc()));
                        __instance.StartCoroutine(Effects.Lerp(.1f, new System.Action<float>((p) => {
                            startButtonText.text = "STOP";
                        })));

                    }
                    if (__instance.startState == GameStartManager.StartingStates.Countdown)
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 0.6f;
                }

                // Client update with handshake infos
                else {
                    if (!playerVersions.ContainsKey(AmongUsClient.Instance.HostId) || TheOtherRolesPlugin.Version.CompareTo(playerVersions[AmongUsClient.Instance.HostId].version) != 0) {
                        kickingTimer += Time.deltaTime;
                        if (kickingTimer > 10) {
                            kickingTimer = 0;
			                AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
                            SceneChanger.ChangeScene("MainMenu");
                        }

                        __instance.GameStartText.text = $"<color=#FF0000FF>The host has no or a different version of The Other Roles\nYou will be kicked in {Math.Round(10 - kickingTimer)}s</color>";
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    } else if (versionMismatch) {
                        __instance.GameStartText.text = $"<color=#FF0000FF>Players With Different Versions:\n</color>" + message;
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    } else {
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                        if (!__instance.GameStartText.text.StartsWith("Starting"))
                            __instance.GameStartText.text = String.Empty;
                    }

                    if (!__instance.GameStartText.text.StartsWith("Starting") || !CustomOptionHolder.anyPlayerCanStopStart.getBool())
                        copiedStartButton?.Destroy();
                    if (CustomOptionHolder.anyPlayerCanStopStart.getBool() && copiedStartButton == null && __instance.GameStartText.text.StartsWith("Starting")) {

                        // Activate Stop-Button
                        copiedStartButton = GameObject.Instantiate(__instance.StartButton.gameObject, __instance.StartButton.gameObject.transform.parent);
                        copiedStartButton.transform.localPosition = __instance.StartButton.transform.localPosition;
                        copiedStartButton.GetComponent<SpriteRenderer>().sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.StopClean.png", 180f);
                        copiedStartButton.SetActive(true);
                        var startButtonText = copiedStartButton.GetComponentInChildren<TMPro.TextMeshPro>();
                        startButtonText.text = "STOP";
                        startButtonText.fontSize *= 0.8f;
                        startButtonText.fontSizeMax = startButtonText.fontSize;
                        startButtonText.gameObject.transform.localPosition = Vector3.zero;
                        PassiveButton startButtonPassiveButton = copiedStartButton.GetComponent<PassiveButton>();

                        void StopStartFunc() {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.StopStart, Hazel.SendOption.Reliable, AmongUsClient.Instance.HostId);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            copiedStartButton.Destroy();
                            __instance.GameStartText.text = String.Empty;
                            startingTimer = 0;
                        }
                        startButtonPassiveButton.OnClick.AddListener((Action)(() => StopStartFunc()));
                        __instance.StartCoroutine(Effects.Lerp(.1f, new System.Action<float>((p) => {
                            startButtonText.text = "STOP";
                        })));

                    }
                    if (__instance.GameStartText.text.StartsWith("Starting") && CustomOptionHolder.anyPlayerCanStopStart.getBool())
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 0.6f;
                }

                // Start Timer
                if (startingTimer > 0) {
                    startingTimer -= Time.deltaTime;
                }
                // Lobby timer
                if (!GameData.Instance) return; // No instance

                if (update) currentText = __instance.PlayerCounter.text;

                timer = Mathf.Max(0f, timer -= Time.deltaTime);
                int minutes = (int)timer / 60;
                int seconds = (int)timer % 60;
                string suffix = $" ({minutes:00}:{seconds:00})";

                __instance.PlayerCounter.text = currentText + suffix;
                __instance.PlayerCounter.autoSizeTextContainer = true;

                if (AmongUsClient.Instance.AmHost) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareGamemode, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte) TORMapOptions.gameMode);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.shareGamemode((byte) TORMapOptions.gameMode);
                }
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        public class GameStartManagerBeginGame {
            public static bool Prefix(GameStartManager __instance) {
                // Block game start if not everyone has the same mod version
                bool continueStart = true;

                if (AmongUsClient.Instance.AmHost) {
                    foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.GetFastEnumerator()) {
                        if (client.Character == null) continue;
                        var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                        if (dummyComponent != null && dummyComponent.enabled)
                            continue;
                        
                        if (!playerVersions.ContainsKey(client.Id)) {
                            continueStart = false;
                            break;
                        }
                        
                        PlayerVersion PV = playerVersions[client.Id];
                        int diff = TheOtherRolesPlugin.Version.CompareTo(PV.version);
                        if (diff != 0 || !PV.GuidMatches()) {
                            continueStart = false;
                            break;
                        }
                    }
                    if (continueStart && (TORMapOptions.gameMode == CustomGamemodes.HideNSeek || TORMapOptions.gameMode == CustomGamemodes.PropHunt) && GameOptionsManager.Instance.CurrentGameOptions.MapId != 6) {
                        byte mapId = 0;
                        if (TORMapOptions.gameMode == CustomGamemodes.HideNSeek) mapId = (byte)CustomOptionHolder.hideNSeekMap.getSelection();
                        else if (TORMapOptions.gameMode == CustomGamemodes.PropHunt) mapId = (byte)CustomOptionHolder.propHuntMap.getSelection();
                        if (mapId >= 3) mapId++;
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.DynamicMapOption, Hazel.SendOption.Reliable, -1);
                        writer.Write(mapId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.dynamicMapOption(mapId);
                    }            
                    else if (CustomOptionHolder.dynamicMap.getBool() && continueStart) {
                        // 0 = Skeld
                        // 1 = Mira HQ
                        // 2 = Polus
                        // 3 = Dleks - deactivated
                        // 4 = Airship
                        // 5 = Submerged
                        byte chosenMapId = 0;
                        List<float> probabilities = new List<float>();
                        probabilities.Add(CustomOptionHolder.dynamicMapEnableSkeld.getSelection() / 10f);
                        probabilities.Add(CustomOptionHolder.dynamicMapEnableMira.getSelection() / 10f);
                        probabilities.Add(CustomOptionHolder.dynamicMapEnablePolus.getSelection() / 10f);
                        probabilities.Add(CustomOptionHolder.dynamicMapEnableAirShip.getSelection() / 10f);
                        probabilities.Add(CustomOptionHolder.dynamicMapEnableFungle.getSelection() / 10f);
                        probabilities.Add(CustomOptionHolder.dynamicMapEnableSubmerged.getSelection() / 10f);

                        // if any map is at 100%, remove all maps that are not!
                        if (probabilities.Contains(1.0f)) {
                            for (int i=0; i < probabilities.Count; i++) {
                                if (probabilities[i] != 1.0) probabilities[i] = 0;
                            }
                        }

                        float sum = probabilities.Sum();
                        if (sum == 0) return continueStart;  // All maps set to 0, why are you doing this???
                        for (int i = 0; i < probabilities.Count; i++) {  // Normalize to [0,1]
                            probabilities[i] /= sum;
                        }
                        float selection = (float)TheOtherRoles.rnd.NextDouble();
                        float cumsum = 0;
                        for (byte i = 0; i < probabilities.Count; i++) {
                            cumsum += probabilities[i];
                            if (cumsum > selection) {
                                chosenMapId = i;
                                break;
                            }
                        }

                        // Translate chosen map to presets page and use that maps random map preset page
                        if (CustomOptionHolder.dynamicMapSeparateSettings.getBool()) {
                            CustomOptionHolder.presetSelection.updateSelection(chosenMapId + 2);
                        }
                        if (chosenMapId >= 3) chosenMapId++;  // Skip dlekS
                                                              
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.DynamicMapOption, Hazel.SendOption.Reliable, -1);
                        writer.Write(chosenMapId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.dynamicMapOption(chosenMapId);
                    }
                }
                return continueStart;
            }
        }

        public class PlayerVersion {
            public readonly Version version;
            public readonly Guid guid;

            public PlayerVersion(Version version, Guid guid) {
                this.version = version;
                this.guid = guid;
            }

            public bool GuidMatches() {
                return Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.Equals(this.guid);
            }
        }
    }
}
