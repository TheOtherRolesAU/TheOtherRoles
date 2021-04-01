using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Reactor.Extensions;
using System.Collections;
using Reactor.Unstrip;
using UnhollowerBaseLib;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;
using HarmonyLib;
using Hazel;

namespace TheOtherRoles {
    public static class Helpers {

        public static Sprite LoadSpriteFromEmbeddedResources(string resource, float PixelPerUnit) {
            try {
                System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.IO.Stream myStream = myAssembly.GetManifestResourceStream(resource);
                byte[] image = new byte[myStream.Length];
                myStream.Read(image, 0, (int) myStream.Length);
                Texture2D myTexture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                LoadImage(myTexture, image, true);
                return Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height), new Vector2(0.5f, 0.5f), PixelPerUnit);
            } catch { }
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

        public static int a  = 0;

        public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit) {
            try {
            Texture2D texture = GUIExtensions.CreateEmptyTexture();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);
            byte[] byteTexture = Reactor.Extensions.Extensions.ReadFully(stream);
            ImageConversion.LoadImage(texture, byteTexture, false);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            } catch {
                System.Console.WriteLine("Error loading sprite from path: " + path);
            }
            return null;
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

        public static void setSkinWithAnim(PlayerPhysics playerPhysics, uint skinId) {
            SkinData nextSkin = DestroyableSingleton<HatManager>.Instance.AllSkins[(int)skinId];
            AnimationClip clip = null;
            var spriteAnim = playerPhysics.Skin.animator;
            var anim = spriteAnim.m_animator;
            var skinLayer = playerPhysics.Skin;

            var currentPhysicsAnim = playerPhysics.Animator.GetCurrentAnimation();
            if (currentPhysicsAnim == playerPhysics.RunAnim) clip = nextSkin.RunAnim;
            else if (currentPhysicsAnim == playerPhysics.SpawnAnim) clip = nextSkin.SpawnAnim;
            else if (currentPhysicsAnim == playerPhysics.EnterVentAnim) clip = nextSkin.EnterVentAnim;
            else if (currentPhysicsAnim == playerPhysics.ExitVentAnim) clip = nextSkin.ExitVentAnim;
            else if (currentPhysicsAnim == playerPhysics.IdleAnim) clip = nextSkin.IdleAnim;
            else clip = nextSkin.IdleAnim;

            float progress = playerPhysics.Animator.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
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


        public static bool handleMurderAttempt(PlayerControl target, bool localCall = false) {
            // Block impostor shielded kill
            if (Medic.shielded != null && Medic.shielded == target) {
                if (!localCall) { // Everyone calls the handleMurderAttempt locally, so don't notify others
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
                RPCProcedure.shieldedMurderAttempt();

                return false;
            }
            // Block impostor not fully grown child kill
            else if (Child.child != null && target == Child.child && !Child.isGrownUp()) {
                return false;
            }
            // Block Time Master with time shield kill
            else if (TimeMaster.shieldActive && TimeMaster.timeMaster != null && TimeMaster.timeMaster == target) {
                if (!localCall) { 
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TimeMasterRewindTime, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.timeMasterRewindTime();
                } else {
                    // Everyone calls the handleMurderAttempt locally as a meeting starts, the TimeMaster stays alive but the time won't be rewinded
                }
                return false;
            }
            return true;
        }

        public static void removeTasksFromPlayer(PlayerControl player, bool removeImportantTextTasks = false) {
            if (player == null) return;
            var toRemove = new List<PlayerTask>();
            foreach (PlayerTask task in player.myTasks) {
                if (!removeImportantTextTasks && task.gameObject.GetComponent<ImportantTextTask>() != null)
                    continue;
                if (task.TaskType != TaskTypes.FixComms && 
                    task.TaskType != TaskTypes.FixLights && 
                    task.TaskType != TaskTypes.ResetReactor && 
                    task.TaskType != TaskTypes.ResetSeismic && 
                    task.TaskType != TaskTypes.RestoreOxy) {
                    toRemove.Add(task);
                }
            }   
            foreach (PlayerTask task in toRemove) {
                player.RemoveTask(task);
            }
        }

        public static void refreshRoleDescription(PlayerControl player) {
            if (player == null) return;

            // Remove default ImportantTextTasks
            var toRemove = new List<PlayerTask>();
            foreach (PlayerTask t in player.myTasks) {
                if (t.gameObject.GetComponent<ImportantTextTask>() != null) {
                    toRemove.Add(t);
                }
            }   
            foreach (PlayerTask t in toRemove)
                player.RemoveTask(t);

            // Add description
            RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(player);        
            var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
            task.transform.SetParent(player.transform, false);

            if (player == Jackal.jackal) {
                var getSidekickText = Jackal.canCreateSidekick ? " and recruit a Sidekick" : "";
                task.Text = $"{roleInfo.colorHexString()}{roleInfo.name}: Kill everyone{getSidekickText}";  
            } else {
                task.Text = $"{roleInfo.colorHexString()}{roleInfo.name}: {roleInfo.shortDescription}";  
            }

            player.myTasks.Insert(0, task);
        }

        public static IEnumerator Slide2D(Transform target, Vector2 source, Vector2 dest, float duration = 0.75f)
        {
            Vector3 temp = default(Vector3);
            temp.z = target.localPosition.z;
            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                float num = time / duration;
                temp.x = Mathf.SmoothStep(source.x, dest.x, num);
                temp.y = Mathf.SmoothStep(source.y, dest.y, num);
                target.localPosition = temp;
                yield return null;
            }
            temp.x = dest.x;
            temp.y = dest.y;
            target.localPosition = temp;
            yield break;
        }

        private static List<byte> lighterColors = new List<byte>(){ 3, 4, 5, 7, 10, 11};
        public static bool isLighterColor(byte colorId) {
            return lighterColors.Contains(colorId);
        }
    }
}