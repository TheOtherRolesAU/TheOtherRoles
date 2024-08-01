using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.Patches;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using static TheOtherRoles.Snitch;
using static UnityEngine.GraphicsBuffer;

namespace TheOtherRoles.CustomGameModes {
    [HarmonyPatch]
    class PropHunt {
        public static bool isPropHuntGM = false;

        public static Dictionary<byte, int> remainingShots = new Dictionary<byte, int>();
        public static float timer = 20f;
        public static bool timerRunning = false;
        public static float blackOutTimer = 0f;

        public static int numberOfHunters;
        public static float initialBlackoutTime;
        public static float killCooldownHit;
        public static float killCooldownMiss;
        public static float hunterVision;
        public static float propVision;
        public static float revealCooldown = 5f;
        public static float revealDuration = 5f;
        public static float unstuckDuration = 5f;
        public static float unstuckCooldown = 5f;
        public static float revealPunish = 0f;

        public static float invisCooldown = 0f;
        public static float invisDuration = 0f;
        public static float speedboostCooldown = 0f;
        public static float speedboostDuration = 0f;
        public static float speedboostRatio = 0f;

        public static float adminCooldown = 5f;
        public static float adminDuration = 10f;

        public static float findCooldown = 10f;
        public static float findDuration = 10f;

        public static bool enableSpeedboost = true;
        public static bool enableInvis = true;

        public static bool propBecomesHunterWhenFound;
        private static Sprite disguiseButtonSprite;
        private static Sprite unstuckButtonSprite;
        private static Sprite revealButtonSprite;
        private static Sprite invisButtonSprite;
        private static Sprite speedboostButtonSprite;
        private static Sprite findButtonSprite;
        private static Sprite poolablesBackgroundSprite;
        public static DateTime startTime = DateTime.UtcNow;
        public static TMP_Text timerText;
        public static List<string> whitelistedObjects = new List<string>() { };

        public static Dictionary<byte, Tuple<string, float>> currentObject = new();
        public static Dictionary<byte, float> isCurrentlyRevealed = new();
        private static Dictionary<byte, GameObject> revealRenderer = new();
        public static Dictionary<byte, float> invisPlayers = new();

        public static Dictionary<byte, float> speedboostActive = new();

        public static GameObject currentTarget = null;
        private static GameObject poolablesBackground;

        public static float dangerMeterActive = 0f;

        private static List<GameObject> duplicatedCollider = new();
        private static GameObject introObject;

        public static void clearAndReload() {
            remainingShots.Clear();
            isPropHuntGM = TORMapOptions.gameMode == CustomGamemodes.PropHunt;
            numberOfHunters = CustomOptionHolder.propHuntNumberOfHunters.getQuantity();
            initialBlackoutTime = CustomOptionHolder.hunterInitialBlackoutTime.getFloat();
            //maxMissesBeforeDeath = CustomOptionHolder.hunterMaxMissesBeforeDeath.getQuantity();
            propBecomesHunterWhenFound = CustomOptionHolder.propBecomesHunterWhenFound.getBool();
            killCooldownMiss = CustomOptionHolder.hunterMissCooldown.getFloat();
            killCooldownHit = CustomOptionHolder.hunterHitCooldown.getFloat();
            hunterVision = CustomOptionHolder.propHunterVision.getFloat();
            propVision = CustomOptionHolder.propVision.getFloat();
            timer = CustomOptionHolder.propHuntTimer.getFloat() * 60;
            revealDuration = CustomOptionHolder.propHuntRevealDuration.getFloat();
            revealCooldown = CustomOptionHolder.propHuntRevealCooldown.getFloat();
            unstuckDuration = CustomOptionHolder.propHuntUnstuckDuration.getFloat();
            unstuckCooldown = CustomOptionHolder.propHuntUnstuckCooldown.getFloat();
            revealPunish = CustomOptionHolder.propHuntRevealPunish.getFloat();
            invisCooldown = CustomOptionHolder.propHuntInvisCooldown.getFloat();
            invisDuration = CustomOptionHolder.propHuntInvisDuration.getFloat();
            speedboostCooldown = CustomOptionHolder.propHuntSpeedboostCooldown.getFloat();
            speedboostDuration = CustomOptionHolder.propHuntSpeedboostDuration.getFloat();
            speedboostRatio = CustomOptionHolder.propHuntSpeedboostSpeed.getFloat();
            enableSpeedboost = CustomOptionHolder.propHuntSpeedboostEnabled.getBool();
            enableInvis = CustomOptionHolder.propHuntInvisEnabled.getBool();
            adminCooldown = CustomOptionHolder.propHuntAdminCooldown.getFloat();
            findCooldown = CustomOptionHolder.propHuntFindCooldown.getFloat();
            findDuration = CustomOptionHolder.propHuntFindDuration.getFloat();
            timerRunning = false;
            timerText?.Destroy();
            timerText = null;
            currentObject = new();
            isCurrentlyRevealed = new();
            poolablesBackground?.Destroy();
            foreach (var go in revealRenderer.Values) { go.Destroy(); }
            revealRenderer = new();
            speedboostActive = new();
            invisPlayers = new();
            foreach (var go in duplicatedCollider) { go.Destroy(); }
            duplicatedCollider = new();
        }

        public static Sprite getIntroSprite(int index) {
            return Helpers.loadSpriteFromResources($"TheOtherRoles.Resources.IntroAnimation.intro_{index + 1000}.png", 150f, cache: false);
        }

        public static void updateWhitelistedObjects(bool debug=false) {
            string allNames = Helpers.readTextFromResources("TheOtherRoles.Resources.Txt.Props.txt");
            if (debug) {
                allNames = Helpers.readTextFromFile(System.IO.Directory.GetCurrentDirectory() + "\\Props.txt");
            }

            whitelistedObjects = allNames.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList();
        }


        public static void propTargetAndTimerDisplayUpdate() {
            
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor) currentTarget = FindClosestDisguiseObject(PlayerControl.LocalPlayer.gameObject, 1f);

            if (timerText == null) {
                RoomTracker roomTracker = FastDestroyableSingleton<HudManager>.Instance?.roomTracker;
                if (roomTracker != null) {
                    GameObject gameObject = UnityEngine.Object.Instantiate(roomTracker.gameObject);

                    gameObject.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
                    UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
                    timerText = gameObject.GetComponent<TMP_Text>();

                    // Use local position to place it in the player's view instead of the world location
                    gameObject.transform.localPosition = new Vector3(0, -1.8f, gameObject.transform.localPosition.z);
                    if (AmongUs.Data.DataManager.Settings.Gameplay.StreamerMode) gameObject.transform.localPosition = new Vector3(0, 2f, gameObject.transform.localPosition.z);
                }
            } else {
                if (timerRunning || blackOutTimer > 0f) {
                    var relevantTimer = timerRunning ? timer : blackOutTimer;
                    int minutes = (int)relevantTimer / 60;
                    int seconds = (int)relevantTimer % 60;
                    string suffix = $" {minutes:00}:{seconds:00}";
                    timerText.text = Helpers.cs(timerRunning ? Color.blue : Color.red, suffix);
                    timerText.outlineColor = Color.white;
                    timerText.outlineWidth = 0.1f;
                    timerText.color = timerRunning ? Color.blue : Color.red;
                }
            }
            if (HudManagerStartPatch.propDisguiseButton != null && HudManagerStartPatch.propDisguiseButton.Timer > HudManagerStartPatch.propDisguiseButton.MaxTimer) HudManagerStartPatch.propDisguiseButton.Timer = HudManagerStartPatch.propDisguiseButton.MaxTimer;
        }
        
        public static void poolablePlayerUpdate() {
            if (poolablesBackground == null) {
                poolablesBackground = new GameObject("poolablesBackground");
                poolablesBackground.AddComponent<SpriteRenderer>();
                poolablesBackground.layer = LayerMask.NameToLayer("UI");
                if (poolablesBackgroundSprite == null) poolablesBackgroundSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.poolablesBackground.jpg", 200f);
            }
            poolablesBackground.transform.SetParent(HudManager.Instance.transform);
            poolablesBackground.transform.localPosition = IntroCutsceneOnDestroyPatch.bottomLeft + new Vector3(-1.45f, -0.05f, 0) + Vector3.right * PlayerControl.AllPlayerControls.Count * 0.2f;
            var backgroundSizeX = PlayerControl.AllPlayerControls.Count * 0.4f + 0.2f;
            poolablesBackground.GetComponent<SpriteRenderer>().sprite = poolablesBackgroundSprite;
            poolablesBackground.transform.localScale = new Vector3(poolablesBackground.transform.localScale.x * backgroundSizeX / poolablesBackground.GetComponent<SpriteRenderer>().bounds.size.x, poolablesBackground.transform.localScale.y, poolablesBackground.transform.localScale.z);

            foreach (var pc in PlayerControl.AllPlayerControls) {
                if (!TORMapOptions.playerIcons.ContainsKey(pc.PlayerId)) continue;
                var poolablePlayer = TORMapOptions.playerIcons[pc.PlayerId];
                if (pc.Data.IsDead) {
                    poolablePlayer.setSemiTransparent(true, 0.25f);
                    poolablePlayer.cosmetics.nameText.text = Helpers.cs(Palette.DisabledGrey, pc.Data.PlayerName); ;
                } else if (pc.Data.Role.IsImpostor) {
                    poolablePlayer.cosmetics.nameText.text = Helpers.cs(Palette.ImpostorRed, pc.Data.PlayerName);
                    poolablePlayer.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 2f);
                    poolablePlayer.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", Palette.ImpostorRed);
                    poolablePlayer.cosmetics.nameText.fontSize = 4;
                } else {
                    // Display Prop
                    poolablePlayer.cosmetics.nameText.text = Helpers.cs(Palette.CrewmateBlue, pc.Data.PlayerName);
                }
                // update currently revealed:
                if (isCurrentlyRevealed.ContainsKey(pc.PlayerId)) {
                    if (!revealRenderer.ContainsKey(pc.PlayerId)) {
                        var go = new GameObject($"reveal_renderer_{pc.PlayerId}");
                        go.layer = LayerMask.NameToLayer("UI");
                        go.AddComponent<SpriteRenderer>();
                        go.transform.SetParent(poolablePlayer.transform.parent, false);
                        go.SetActive(true);
                        go.transform.localPosition = poolablePlayer.transform.localPosition + new Vector3(0, 0, -50f);
                        poolablePlayer.gameObject.SetActive(false);
                        revealRenderer.Add(pc.PlayerId, go);
                    }
                    float revealTimer = isCurrentlyRevealed[pc.PlayerId] - Time.deltaTime;
                    isCurrentlyRevealed[pc.PlayerId] = revealTimer;
                    if (revealTimer > 0) {
                        // get sprite:
                        if (currentObject.ContainsKey(pc.PlayerId)) {
                            revealRenderer[pc.PlayerId].GetComponent<SpriteRenderer>().sprite = pc.GetComponent<SpriteRenderer>().sprite;
                            revealRenderer[pc.PlayerId].transform.localScale *= 0.5f / revealRenderer[pc.PlayerId].GetComponent<SpriteRenderer>().bounds.size.magnitude;
                        }
                    } else {
                        revealRenderer[pc.PlayerId].Destroy();
                        isCurrentlyRevealed.Remove(pc.PlayerId);
                        revealRenderer.Remove(pc.PlayerId);
                        poolablePlayer.gameObject.SetActive(true);
                        SoundEffectsManager.play("morphlingMorph");
                    }
                }
            }
        }

        public static void invisUpdate() {
            foreach (var playerId in invisPlayers.Keys) {
                var pc = Helpers.playerById(playerId);
                if (pc == null || pc.Data.IsDead) continue;
                float timeLeft = invisPlayers[playerId] - Time.deltaTime;
                invisPlayers[playerId] = timeLeft;
                if (timeLeft > 0) {
                    pc.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, PlayerControl.LocalPlayer.Data.IsDead || PlayerControl.LocalPlayer.PlayerId == playerId ? 0.1f : 0f);

                } else {
                    pc.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                    invisPlayers.Remove(playerId);
                }
                if (isCurrentlyRevealed.ContainsKey(playerId)) {
                    revealRenderer[playerId].GetComponent<SpriteRenderer>().color = pc.GetComponent<SpriteRenderer>().color;
                }
            }
        }

        public static void speedboostUpdate() {
            foreach (var key in speedboostActive.Keys) {
                float speedboostTimer = speedboostActive[key] - Time.deltaTime;
                speedboostActive[key] = speedboostTimer;
                if (speedboostTimer < 0)
                    speedboostActive.Remove(key);
            }
        }

        public static void dangerMeterUpdate() {
            if (!HudManager.Instance || !HudManager.Instance.DangerMeter) return;
            if (HudManager.Instance.DangerMeter.gameObject.active) {
                float dist = 55f;
                float dist2 = 15f;
                float curr = float.MaxValue;
                foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Data.IsDead && (PlayerControl.LocalPlayer.Data.Role.IsImpostor ? !x.Data.Role.IsImpostor : x.Data.Role.IsImpostor))) {
                    if (invisPlayers.ContainsKey(playerControl.PlayerId)) continue;  // Dont light up for invisible players
                    if (!(playerControl == null)) {

                        float sqrMagnitude = (playerControl.transform.position - PlayerControl.LocalPlayer.transform.position).sqrMagnitude;
                        if (sqrMagnitude < dist && curr > sqrMagnitude) {
                            curr = sqrMagnitude;
                        }
                    }
                }
                float dangerLevel1 = Mathf.Clamp01((dist - curr) / (dist - dist2));
                float dangerLevel2 = Mathf.Clamp01((dist2 - curr) / dist2);
                HudManager.Instance.DangerMeter.SetDangerValue(dangerLevel1, dangerLevel2);
            }
            HudManager.Instance.DangerMeter?.gameObject.SetActive(!PlayerControl.LocalPlayer.Data.IsDead && (!PlayerControl.LocalPlayer.Data.Role.IsImpostor || HudManagerStartPatch.propHuntFindButton.isEffectActive));
        }


        public static void update() {            
            if (!isPropHuntGM) {
                // Make sure the DangerMeter is not displayed in TOR HideNSeek, Classic or Guesser Game mode.
                if (GameOptionsManager.Instance.currentGameOptions.GameMode != AmongUs.GameOptions.GameModes.HideNSeek) HudManager.Instance.DangerMeter?.gameObject.SetActive(false);
                return;
            }
            if (timerRunning) timer = Math.Clamp(timer -= Time.deltaTime, 0, timer >= 0 ? timer : 0);
            else if (blackOutTimer > 0f) blackOutTimer -= Time.deltaTime;

            // Local player find prop Target
            propTargetAndTimerDisplayUpdate();

            poolablePlayerUpdate();

            speedboostUpdate();

            invisUpdate();

            dangerMeterUpdate();
        }

        public static void transformLayers() {  // A bit of a hacky way to make sure that props as well as propable objects are not visible in the dark, while keeping collisions enabled.
            PlayerControl.LocalPlayer.clearAllTasks();
            foreach (Collider2D collider in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.transform.position, 500)) {
                bool whiteListed = false;
                foreach (var whiteListedWord in whitelistedObjects) if (collider.gameObject.name.Contains(whiteListedWord) && collider.gameObject.GetComponent<SpriteRenderer>() != null) whiteListed = true;
                if (collider.GetComponent<Console>() != null || whiteListed) {
                    if (whiteListed) {
                        var newgo = GameObject.Instantiate(collider.gameObject, collider.transform.parent);
                        newgo.name = "DONTUSE";
                        duplicatedCollider.Add(newgo);
                    }
                    
                    collider.gameObject.layer = PlayerControl.LocalPlayer.gameObject.layer;
                }
            }
        }

        public static Sprite getDisguiseButtonSprite() {
            if (disguiseButtonSprite) return disguiseButtonSprite;
            disguiseButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.DisguiseButton.png", 115f);
            return disguiseButtonSprite;
        }

        public static Sprite getUnstuckButtonSprite() {
            if (unstuckButtonSprite) return unstuckButtonSprite;
            unstuckButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.UnStuck.png", 115f);
            return unstuckButtonSprite;
        }
        public static Sprite getRevealButtonSprite() {
            if (revealButtonSprite) return revealButtonSprite;
            revealButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Reveal.png", 115f);
            return revealButtonSprite;
        }

        public static Sprite getInvisButtonSprite() {
            if (invisButtonSprite) return invisButtonSprite;
            invisButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.InvisButton.png", 115f);
            return invisButtonSprite;
        }

        public static Sprite getFindButtonSprite() {
            if (findButtonSprite) return findButtonSprite;
            findButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.FindButton.png", 115f);
            return findButtonSprite;
        }
        public static Sprite getSpeedboostButtonSprite() {
            if (speedboostButtonSprite) return speedboostButtonSprite;
            speedboostButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SpeedboostButton.png", 115f);
            return speedboostButtonSprite;
        }


        public static GameObject FindClosestDisguiseObject(GameObject origin, float radius, bool verbose=false) {
            // This throws errors if no console is nearby
            try {
                Collider2D bestCollider = null;
                float bestDist = 9999;
                if (whitelistedObjects == null || whitelistedObjects.Count == 0 || verbose) {
                    updateWhitelistedObjects(true);
                }
                foreach (Collider2D collider in Physics2D.OverlapCircleAll(origin.transform.position, radius)) {
                    if (verbose) {
                        TheOtherRolesPlugin.Logger.LogMessage($"Nearby Object: {collider.gameObject.name}");
                    }
                    bool whiteListed = false;
                    foreach (var whiteListedWord in whitelistedObjects) {
                        if (collider.gameObject.name.Contains(whiteListedWord)) whiteListed = true;
                    }
                    if (collider.GetComponent<Console>() != null || whiteListed) {
                        float dist = Vector2.Distance(origin.transform.position, collider.transform.position);
                        if (dist < bestDist) {
                            bestCollider = collider;
                            bestDist = dist;
                        }
                    }
                }
                return bestCollider.gameObject;
            } catch { return null; }
        }

        public static GameObject FindPropByNameAndPos(string propName, float posX) {
            var candidates = GameObject.FindObjectsOfType<GameObject>();
            GameObject prop = null;
            foreach (var candidate in candidates) {
                if (candidate.name == propName && candidate.transform.position.x == posX)
                    prop = candidate;
            }
            return prop;
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        [HarmonyPostfix]
        public static void SpeedbostPostfix(PlayerPhysics __instance) {
            if (!__instance.AmOwner || !speedboostActive.ContainsKey(__instance.myPlayer.PlayerId)) return;
            if (GameData.Instance && __instance.myPlayer.CanMove)
                __instance.body.velocity *= speedboostRatio;
        }

        [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.FixedUpdate))]
        [HarmonyPostfix]
        public static void PostfixNetworkSpeed(CustomNetworkTransform __instance) {
            if (__instance.AmOwner || !speedboostActive.ContainsKey(__instance.myPlayer.PlayerId)) return;
            if (GameData.Instance && __instance.myPlayer.CanMove)
                __instance.body.velocity *= speedboostRatio;
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
        [HarmonyPostfix]
        public static void IntroCutsceneDestroyPatch(IntroCutscene __instance) {
            if (!isPropHuntGM || !PlayerControl.LocalPlayer.Data.Role.IsImpostor) return;
            PlayerControl.LocalPlayer.moveable = false;
            MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.PropHuntStartTimer, Hazel.SendOption.Reliable, -1);
            writer2.Write(true);
            AmongUsClient.Instance.FinishRpcImmediately(writer2);
            RPCProcedure.propHuntStartTimer(true);
            introObject = new GameObject("introrenderer");
            introObject.layer = HudManager.Instance.FullScreen.gameObject.layer;
            introObject.transform.SetParent(HudManager.Instance.FullScreen.transform);
            introObject.transform.localPosition = new Vector3(0, 0, -1f);
            SpriteRenderer introRenderer = introObject.AddComponent<SpriteRenderer>();
            introObject.SetActive(true);
            introRenderer.enabled = true;
            int nFrames = 25 * (int)initialBlackoutTime + 10;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(initialBlackoutTime, new Action<float>((p) => {
                if (p == 1f) {
                    // start timer
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.PropHuntStartTimer, Hazel.SendOption.Reliable, -1);
                    writer.Write(false);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.propHuntStartTimer();
                    PlayerControl.LocalPlayer.moveable = true;
                    HudManager.Instance.FullScreen.enabled = false;
                    introObject.Destroy();
                } else {
                    HudManager.Instance.FullScreen.enabled = true;
                    HudManager.Instance.FullScreen.gameObject.SetActive(true);
                    HudManager.Instance.FullScreen.color = new Color(0, 0, 0, 1);
                    introRenderer.sprite?.Destroy();
                    introRenderer.sprite = getIntroSprite(510 - nFrames + (int)(p * nFrames));
                    Resources.UnloadUnusedAssets();  // Needed so that the last sprite gets unloaded
                }
            })));            
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        [HarmonyPostfix]
        public static void PlayerControlFixedUpdatePatch(PlayerControl __instance) {
            if (!PropHunt.isPropHuntGM) return;
                if (__instance.Data.Role.IsImpostor) {
                    __instance.GetComponent<CircleCollider2D>().radius = 0.2234f;
                    return;
                }
                if (__instance.GetComponent<SpriteRenderer>() != null || __instance.Data.IsDead) return;

                __instance.gameObject.AddComponent<SpriteRenderer>();
                __instance.GetComponent<CircleCollider2D>().radius = 0.00001f;
        }


        // Runs periodically, resets animation data for players
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleAnimation))]
        [HarmonyPostfix]
        public static void PlayerPhysicsAnimationPatch(PlayerPhysics __instance) {
            if (!AmongUsClient.Instance.IsGameStarted || !PropHunt.isPropHuntGM)
                return;
            try {
                if (__instance.GetComponent<SpriteRenderer>().sprite != null && !__instance.myPlayer.Data.Role.IsImpostor) {
                    __instance.myPlayer.Visible = false;
                    __instance.GetComponent<SpriteRenderer>().flipX = __instance.myPlayer.cosmetics.currentBodySprite.BodySprite.flipX;
                    __instance.myPlayer.cosmetics.currentPet?.Destroy();
                }
                if (__instance.myPlayer.Data.IsDead) {
                    __instance.myPlayer.Visible = PlayerControl.LocalPlayer.Data.IsDead;
                    GameObject.Destroy(__instance.GetComponent<SpriteRenderer>());
                }
            } catch { }
        }

        // Make prop impostor on death
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
        [HarmonyPostfix]
        public static void MakePropImpostorPatch(PlayerControl __instance) {
            if (!PropHunt.isPropHuntGM) return;
            __instance.transform.localScale = new Vector3(0.7f, 0.7f, 1);
            if (!__instance.Data.Role.IsImpostor && propBecomesHunterWhenFound) {
                __instance.Revive();
                DestroyableSingleton<RoleManager>.Instance.SetRole(__instance, RoleTypes.Impostor);
                if (__instance == PlayerControl.LocalPlayer) {
                    HudManagerStartPatch.propHuntRevealButton.Timer = revealCooldown;
                    HudManagerStartPatch.propHuntFindButton.Timer = findCooldown;
                    HudManagerStartPatch.propHuntAdminButton.Timer = adminCooldown;
                }
                __instance.MyPhysics.SetBodyType(PlayerBodyTypes.Seeker);
            } else {
                // Find correct dead body, set sprite to dead console object...
                Sprite currentPlayerSprite = __instance.GetComponent<SpriteRenderer>().sprite;
                foreach (var db in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                    if (db.ParentId == __instance.PlayerId && currentPlayerSprite != null) {
                        db.bodyRenderers[0].sprite = currentPlayerSprite;
                        db.bodyRenderers[0].color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    }
            }
            __instance.GetComponent<SpriteRenderer>().sprite = null;
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        [HarmonyPostfix]
        public static void MapSetPostfix() {  // Make sure the map in the settings is in sync with the map from li
            if (TORMapOptions.gameMode != CustomGamemodes.PropHunt && TORMapOptions.gameMode != CustomGamemodes.HideNSeek || AmongUsClient.Instance.IsGameStarted) return;
            int map = GameOptionsManager.Instance.currentGameOptions.MapId;
            if (map > 3) map--;
            if (TORMapOptions.gameMode == CustomGamemodes.HideNSeek)
                if (CustomOptionHolder.hideNSeekMap.selection != map)
                    CustomOptionHolder.hideNSeekMap.updateSelection(map);
            if (TORMapOptions.gameMode == CustomGamemodes.PropHunt)
                if (CustomOptionHolder.propHuntMap.selection != map)
                    CustomOptionHolder.propHuntMap.updateSelection(map);
        }


        [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.CanUse))]
        [HarmonyPostfix]
        public static void AdminCanUsePostfix(MapConsole __instance, NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse, ref float __result) {
            if (!PropHunt.isPropHuntGM || !PlayerControl.LocalPlayer.Data.Role.IsImpostor) return;
            if (canUse) {
                if (HudManagerStartPatch.propHuntAdminButton.Timer > 0) {
                    canUse = couldUse = false;
                    __result = float.MaxValue;
                }
            }
        }

        [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.Use))]
        [HarmonyPrefix]
        public static bool AdminUsePostfix(MapConsole __instance) {
            if (!PropHunt.isPropHuntGM || !PlayerControl.LocalPlayer.Data.Role.IsImpostor) return true;
            HudManagerStartPatch.propHuntAdminButton.onClickEvent();
            return false;
        }


        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        [HarmonyPostfix]
        public static void MurderPlayerPostfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target) {
            if (!PropHunt.isPropHuntGM || target != CachedPlayer.LocalPlayer.PlayerControl) return;
            try {
                target.NetTransform.RpcSnapTo(__instance.transform.position);
            } catch { }
        }

        // Make it so that the kill button doesn't light up when near a player
        [HarmonyPatch(typeof(VentButton), nameof(VentButton.SetTarget))]
        [HarmonyPatch(typeof(KillButton), nameof(KillButton.SetTarget))]
        [HarmonyPostfix]
        public static void KillButtonHighlightPatch(ActionButton __instance) {
            if (!PropHunt.isPropHuntGM) return;
            __instance.SetEnabled();
        }


        // Penalize the impostor if there is no prop killed
        [HarmonyPatch(typeof(KillButton), nameof(KillButton.CheckClick))]
        [HarmonyPrefix]
        public static bool CheckClickPatch(KillButton __instance) {
            if (!PropHunt.isPropHuntGM) return true;
            __instance.DoClick();
            return false;
        }

        // Penalize the impostor if there is no prop killed
        [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
        [HarmonyPrefix]
        public static bool KillButtonClickPatch(KillButton __instance) {
            if (!PropHunt.isPropHuntGM || __instance.isCoolingDown || PlayerControl.LocalPlayer.Data.IsDead || PlayerControl.LocalPlayer.inVent) return false;
            var targets = PlayerControl.LocalPlayer.Data.Role.GetPlayersInAbilityRangeSorted(RoleBehaviour.GetTempPlayerList(), true).ToArray();
            __instance.SetTarget(PlayerControl.LocalPlayer.Data.Role.GetPlayersInAbilityRangeSorted(RoleBehaviour.GetTempPlayerList(), true).ToArray().FirstOrDefault());

            if (__instance.currentTarget == null) {
                PlayerControl.LocalPlayer.SetKillTimer(killCooldownMiss);
            } else {  // There is a target, execute kill!
                MurderAttemptResult res = Helpers.checkMurderAttemptAndKill(CachedPlayer.LocalPlayer.PlayerControl, __instance.currentTarget);
                __instance.SetTarget(null);
                PlayerControl.LocalPlayer.SetKillTimer(killCooldownHit);
            }
            return false;
        }

        [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.IsValidTarget))]
        [HarmonyPrefix]
        public static bool IsValidTarget(RoleBehaviour __instance, NetworkedPlayerInfo target,  ref bool __result) {
            if (!PropHunt.isPropHuntGM) return true;
            __result = !(target == null) && !target.Disconnected && !target.IsDead && target.PlayerId != __instance.Player.PlayerId && !(target.Role == null) && !(target.Object == null) && !target.Object.inVent && !target.Object.inMovingPlat;
            return false;
        }

        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Show))]
        [HarmonyPrefix]
        public static void MapBehaviourShowPatch(MapBehaviour __instance, ref MapOptions opts) {
            if (!PropHunt.isPropHuntGM) return;
            if (opts.Mode == MapOptions.Modes.Sabotage) opts.Mode = MapOptions.Modes.Normal;
        }


        // Disable a lot of stuff
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
        [HarmonyPatch(typeof(Vent), nameof(Vent.SetOutline))]
        [HarmonyPrefix]
        public static bool DisableFunctions() {
            if (!PropHunt.isPropHuntGM) return true;
            return false;
        }
    }
}
