using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Collections;
using UnhollowerBaseLib;
using UnityEngine;
using System.Linq;
using static TheOtherRoles.TheOtherRoles;
using HarmonyLib;
using Hazel;

using TaskTypes = CBFIAGIGOFA;

namespace TheOtherRoles {
    public static class Helpers {

        public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit) {
            try {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);
            var byteTexture = new byte[stream.Length];
            var read = stream.Read(byteTexture, 0, (int) stream.Length);
            LoadImage(texture, byteTexture, false);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            } catch {
                System.Console.WriteLine("Error loading sprite from path: " + path);
            }
            return null;
        }

        internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
        internal static d_LoadImage iCall_LoadImage;
        private static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable) {
            if (iCall_LoadImage == null)
                iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");

            var il2cppArray = (Il2CppStructArray<byte>) data;

            return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
        }

        public static PlayerControl playerById(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == id)
                    return player;
            return null;
        }
        
        public static Dictionary<byte, PlayerControl> allPlayersById()
        {
            Dictionary<byte, PlayerControl> res = new Dictionary<byte, PlayerControl>();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                res.Add(player.PlayerId, player);
            return res;
        }

        public static void setSkinWithAnim(PlayerPhysics playerPhysics, uint LFDAHOFPIAM) {
            SkinData nextSkin = DestroyableSingleton<HatManager>.CMJOLNCMAPD.AllSkins[(int)LFDAHOFPIAM];
            AnimationClip clip = null;
            var spriteAnim = playerPhysics.Skin.animator;
            var anim = spriteAnim.m_animator;
            var skinLayer = playerPhysics.Skin;

            var currentPhysicsAnim = playerPhysics.NDIJGONKPMC.GetCurrentAnimation();
            if (currentPhysicsAnim == playerPhysics.RunAnim) clip = nextSkin.RunAnim;
            else if (currentPhysicsAnim == playerPhysics.SpawnAnim) clip = nextSkin.SpawnAnim;
            else if (currentPhysicsAnim == playerPhysics.EnterVentAnim) clip = nextSkin.EnterVentAnim;
            else if (currentPhysicsAnim == playerPhysics.ExitVentAnim) clip = nextSkin.ExitVentAnim;
            else if (currentPhysicsAnim == playerPhysics.IdleAnim) clip = nextSkin.IdleAnim;
            else clip = nextSkin.IdleAnim;

            float progress = playerPhysics.NDIJGONKPMC.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            skinLayer.skin = nextSkin;

            spriteAnim.Play(clip, 1f);
            anim.Play("a", 0, progress % 1);
            anim.Update(0f);
        }

        public static IEnumerator CoFadeOutAndDestroy(SpriteRenderer renderer, float duration) {
            for (float t = duration; t > 0; t -= Time.deltaTime) {
                if (renderer != null) {
                    var tmp = renderer.color;
                    tmp.a = Mathf.Clamp(t / duration, 0, 1);
                    renderer.color = tmp;
                }
                yield return null;
            }
            if (renderer?.gameObject != null) UnityEngine.Object.Destroy(renderer.gameObject);
        }

        public static IEnumerator CoFlashAndDisable(SpriteRenderer renderer, float duration, Color a, Color b) {
            float singleDuration = duration / 2;
            for (float t = 0f; t < singleDuration; t += Time.deltaTime) {
                if (renderer != null)
                    renderer.color = Color.Lerp(a, b, Mathf.Clamp(t / singleDuration, 0, 1));
                yield return null;
            }
            for (float t = singleDuration; t > 0f; t -= Time.deltaTime) {
                if (renderer != null)
                    renderer.color = Color.Lerp(a, b, Mathf.Clamp(t / singleDuration, 0, 1));
                yield return null;
            }
            
            if (renderer != null) renderer.enabled = false;
        }


        public static bool handleMurderAttempt(PlayerControl target, bool isMeetingStart = false) {
            // Block impostor shielded kill
            if (Medic.shielded != null && Medic.shielded == target) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shieldedMurderAttempt();

                return false;
            }
            // Block impostor not fully grown child kill
            else if (Child.child != null && target == Child.child && !Child.isGrownUp()) {
                return false;
            }
            // Block Time Master with time shield kill
            else if (TimeMaster.shieldActive && TimeMaster.timeMaster != null && TimeMaster.timeMaster == target) {
                if (!isMeetingStart) { // Only rewind the attempt was not called because a meeting startet 
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TimeMasterRewindTime, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.timeMasterRewindTime();
                }
                return false;
            }
            return true;
        }

        public static void removeTasksFromPlayer(PlayerControl player) {
            if (player == null) return;
            var toRemove = new List<PlayerTask>();
            foreach (PlayerTask task in player.myTasks) {
                if (task.gameObject.GetComponent<ImportantTextTask>() != null)
                    continue;
                if (task.TaskType != TaskTypes.FixComms && 
                    task.TaskType != TaskTypes.FixLights && 
                    task.TaskType != TaskTypes.ResetReactor && 
                    task.TaskType != TaskTypes.ResetSeismic && 
                    task.TaskType != TaskTypes.RestoreOxy &&
                    task.TaskType != TaskTypes.StopCharles) {
                    toRemove.Add(task);
                }
            }   
            foreach (PlayerTask task in toRemove) {
                player.RemoveTask(task);
            }
        }

        public static void refreshRoleDescription(PlayerControl player) {
            if (player == null) return;

            List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(player); 

            var toRemove = new List<PlayerTask>();
            foreach (PlayerTask t in player.myTasks) {
                var textTask = t.gameObject.GetComponent<ImportantTextTask>();
                if (textTask != null) {
                    var info = infos.FirstOrDefault(x => textTask.Text.StartsWith(x.name));
                    if (info != null)
                        infos.Remove(info); // TextTask for this RoleInfo does not have to be added, as it already exists
                    else
                        toRemove.Add(t); // TextTask does not have a corresponding RoleInfo and will hence be deleted
                }
            }   

            foreach (PlayerTask t in toRemove) {
                t.OnRemove();
                player.myTasks.Remove(t);
                UnityEngine.Object.Destroy(t.gameObject);
            }

            // Add TextTask for remaining RoleInfos
            foreach (RoleInfo roleInfo in infos) {
                var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
                task.transform.SetParent(player.transform, false);

                if (roleInfo.name == "Jackal") {
                    var getSidekickText = Jackal.canCreateSidekick ? " and recruit a Sidekick" : "";
                    task.Text = $"{roleInfo.colorHexString()}{roleInfo.name}: Kill everyone{getSidekickText}";  
                } else {
                    task.Text = $"{roleInfo.colorHexString()}{roleInfo.name}: {roleInfo.shortDescription}";  
                }

                player.myTasks.Insert(0, task);
            }
        }

        private static List<int> lighterColors = new List<int>(){ 3, 4, 5, 7, 10, 11};
        public static bool isLighterColor(int colorId) {
            return lighterColors.Contains(colorId);
        }
    }
}