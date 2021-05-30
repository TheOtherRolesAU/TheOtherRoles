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

namespace TheOtherRoles {
    public static class Helpers {

        public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit) {
            try {
                Texture2D texture = loadTextureFromResources(path);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            } catch {
                System.Console.WriteLine("Error loading sprite from path: " + path);
            }
            return null;
        }

        public static Texture2D loadTextureFromResources(string path) {
            try {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var byteTexture = new byte[stream.Length];
                var read = stream.Read(byteTexture, 0, (int) stream.Length);
                LoadImage(texture, byteTexture, false);
                return texture;
            } catch {
                System.Console.WriteLine("Error loading texture from resources: " + path);
            }
            return null;
        }

        public static Texture2D loadTextureFromDisk(string path) {
            try {          
                if (File.Exists(path))     {
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                    byte[] byteTexture = File.ReadAllBytes(path);
                    LoadImage(texture, byteTexture, false);
                    return texture;
                }
            } catch {
                System.Console.WriteLine("Error loading texture from disk: " + path);
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

        public static void setSkinWithAnim(PlayerPhysics playerPhysics, uint SkinId) {
            SkinData nextSkin = DestroyableSingleton<HatManager>.Instance.AllSkins[(int)SkinId];
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

        public static bool handleMurderAttempt(PlayerControl target, bool isMeetingStart = false) {
            // Block impostor shielded kill
            if (Medic.shielded != null && Medic.shielded == target) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shieldedMurderAttempt();

                return false;
            }
            // Block impostor not fully grown mini kill
            else if (Mini.mini != null && target == Mini.mini && !Mini.isGrownUp()) {
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
                    task.Text = cs(roleInfo.color, $"{roleInfo.name}: Kill everyone{getSidekickText}");  
                } else {
                    task.Text = cs(roleInfo.color, $"{roleInfo.name}: {roleInfo.shortDescription}");  
                }

                player.myTasks.Insert(0, task);
            }
        }

        public static bool isLighterColor(int colorId) {
            return CustomColors.lighterColors.Contains(colorId);
        }

        public static bool isCustomServer() {
            if (DestroyableSingleton<ServerManager>.Instance == null) return false;
            StringNames n = DestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName;
            return n != StringNames.ServerNA && n != StringNames.ServerEU && n != StringNames.ServerAS;
        }

        public static bool hasFakeTasks(this PlayerControl player) {
            return (player == Jester.jester || player == Jackal.jackal || player == Sidekick.sidekick || player == Arsonist.arsonist);
        }

        public static void clearAllTasks(this PlayerControl player) {
            if (player == null) return;
            for (int i = 0; i < player.myTasks.Count; i++) {
                PlayerTask playerTask = player.myTasks[i];
                playerTask.OnRemove();
                UnityEngine.Object.Destroy(playerTask.gameObject);
            }
            player.myTasks.Clear();
            
            if (player.Data != null && player.Data.Tasks != null)
                player.Data.Tasks.Clear();
        }

        public static void setSemiTransparent(this PoolablePlayer player, bool value) {
            float alpha = value ? 0.25f : 1f;
            foreach (SpriteRenderer r in player.gameObject.GetComponentsInChildren<SpriteRenderer>())
                r.color = new Color(r.color.r, r.color.g, r.color.b, alpha);
            player.NameText.color = new Color(player.NameText.color.r, player.NameText.color.g, player.NameText.color.b, alpha);
        }

        public static string cs(Color c, string s) {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
        }
 
        private static byte ToByte(float f) {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }
    }
}
