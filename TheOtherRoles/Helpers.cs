using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Hazel;
using TheOtherRoles.Modules;
using TheOtherRoles.Roles;
using UnhollowerBaseLib;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace TheOtherRoles
{
    public static class Helpers
    {
        private static DLoadImage iCallLoadImage;

        public static Sprite LoadSpriteFromResources(string path, float pixelsPerUnit)
        {
            try
            {
                var texture = LoadTextureFromResources(path);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f),
                    pixelsPerUnit);
            }
            catch
            {
                System.Console.WriteLine("Error loading sprite from path: " + path);
            }

            return null;
        }

        public static Texture2D LoadTextureFromResources(string path)
        {
            try
            {
                var texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                var assembly = Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream(path);
                if (stream == null) return texture;
                var byteTexture = new byte[stream.Length];
                stream.Read(byteTexture, 0, (int) stream.Length);
                LoadImage(texture, byteTexture, false);

                return texture;
            }
            catch
            {
                System.Console.WriteLine("Error loading texture from resources: " + path);
            }

            return null;
        }

        public static Texture2D LoadTextureFromDisk(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                    var byteTexture = File.ReadAllBytes(path);
                    LoadImage(texture, byteTexture, false);
                    return texture;
                }
            }
            catch
            {
                System.Console.WriteLine("Error loading texture from disk: " + path);
            }

            return null;
        }

        private static void LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
        {
            iCallLoadImage ??= IL2CPP.ResolveICall<DLoadImage>("UnityEngine.ImageConversion::LoadImage");
            var il2CPPArray = (Il2CppStructArray<byte>) data;
            iCallLoadImage.Invoke(tex.Pointer, il2CPPArray.Pointer, markNonReadable);
        }

        public static PlayerControl PlayerById(byte id)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == id)
                    return player;
            return null;
        }

        public static Dictionary<byte, PlayerControl> AllPlayersById()
        {
            var res = new Dictionary<byte, PlayerControl>();
            foreach (var player in PlayerControl.AllPlayerControls)
                res.Add(player.PlayerId, player);
            return res;
        }

        public static void SetSkinWithAnim(PlayerPhysics playerPhysics, uint skinId)
        {
            var nextSkin = DestroyableSingleton<HatManager>.Instance.AllSkins.ToArray()[(int) skinId];
            AnimationClip clip;
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

            var progress = playerPhysics.Animator.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            skinLayer.skin = nextSkin;

            spriteAnim.Play(clip, 1f);
            anim.Play("a", 0, progress % 1);
            anim.Update(0f);
        }

        public static bool HandleMurderAttempt(PlayerControl target, bool isMeetingStart = false)
        {
            // Block impostor shielded kill
            if (Medic.shielded != null && Medic.shielded == target)
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.ShieldedMurderAttempt, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.ShieldedMurderAttempt();

                return false;
            }
            // Block impostor not fully grown mini kill

            if (Mini.Instance.player != null && target == Mini.Instance.player && !Mini.IsGrownUp()) return false;
            // Block Time Master with time shield kill

            if (!TimeMaster.shieldActive || TimeMaster.Instance.player == null || TimeMaster.Instance.player != target)
                return true;
            {
                if (isMeetingStart) return false;
                // Only rewind the attempt was not called because a meeting started 
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.TimeMasterRewindTime, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.TimeMasterRewindTime();

                return false;
            }
        }

        public static void HandleVampireBiteOnBodyReport()
        {
            // Murder the bitten player before the meeting starts or reset the bitten player
            if (Vampire.bitten != null && !Vampire.bitten.Data.IsDead &&
                HandleMurderAttempt(Vampire.bitten, true))
            {
                var killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.VampireTryKill, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                RPCProcedure.VampireTryKill();
            }
            else
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.VampireSetBitten, SendOption.Reliable, -1);
                writer.Write(byte.MaxValue);
                writer.Write(byte.MaxValue);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.VampireSetBitten(byte.MaxValue, byte.MaxValue);
            }
        }

        public static void RefreshRoleDescription(PlayerControl player)
        {
            if (player == null) return;

            var infos = RoleInfo.GetRoleInfoForPlayer(player);

            var toRemove = new List<PlayerTask>();
            foreach (var t in player.myTasks)
            {
                var textTask = t.gameObject.GetComponent<ImportantTextTask>();
                if (textTask == null) continue;
                var info = infos.FirstOrDefault(x => textTask.Text.StartsWith(x.name));
                if (info != null)
                    infos.Remove(
                        info); // TextTask for this RoleInfo does not have to be added, as it already exists
                else
                    toRemove.Add(t); // TextTask does not have a corresponding RoleInfo and will hence be deleted
            }

            foreach (var t in toRemove)
            {
                t.OnRemove();
                player.myTasks.Remove(t);
                UnityEngine.Object.Destroy(t.gameObject);
            }

            // Add TextTask for remaining RoleInfos
            foreach (var roleInfo in infos)
            {
                var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
                task.transform.SetParent(player.transform, false);

                if (roleInfo.name == "Jackal")
                {
                    var getSidekickText = Jackal.canCreateSidekick ? " and recruit a Sidekick" : "";
                    task.Text = Cs(roleInfo.color, $"{roleInfo.name}: Kill everyone{getSidekickText}");
                }
                else
                {
                    task.Text = Cs(roleInfo.color, $"{roleInfo.name}: {roleInfo.shortDescription}");
                }

                player.myTasks.Insert(0, task);
            }
        }

        public static bool IsLighterColor(int colorId)
        {
            return CustomColors.LighterColors.Contains(colorId);
        }

        public static bool IsCustomServer()
        {
            if (DestroyableSingleton<ServerManager>.Instance == null) return false;
            var n = DestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName;
            return n != StringNames.ServerNA && n != StringNames.ServerEU && n != StringNames.ServerAS;
        }

        public static bool HasFakeTasks(this PlayerControl player)
        {
            return player == Jester.Instance.player || player == Jackal.Instance.player ||
                   player == Sidekick.Instance.player ||
                   player == Arsonist.Instance.player || Jackal.FormerJackals.Contains(player);
        }

        public static bool CanBeErased(this PlayerControl player)
        {
            return player != Jackal.Instance.player && player != Sidekick.Instance.player &&
                   !Jackal.FormerJackals.Contains(player);
        }

        public static void ClearAllTasks(this PlayerControl player)
        {
            if (player == null) return;
            foreach (var task in player.myTasks)
            {
                task.OnRemove();
                UnityEngine.Object.Destroy(task.gameObject);
            }

            player.myTasks.Clear();

            player.Data?.Tasks?.Clear();
        }

        public static void SetSemiTransparent(this PoolablePlayer player, bool value)
        {
            var alpha = value ? 0.25f : 1f;
            foreach (var r in player.gameObject.GetComponentsInChildren<SpriteRenderer>())
                r.color = new Color(r.color.r, r.color.g, r.color.b, alpha);
            player.NameText.color = new Color(player.NameText.color.r, player.NameText.color.g, player.NameText.color.b,
                alpha);
        }

        public static string GetString(this TranslationController t, StringNames key, params Object[] parts)
        {
            return t.GetString(key, parts);
        }

        public static string Cs(Color c, string s)
        {
            return $"<color=#{ToByte(c.r):X2}{ToByte(c.g):X2}{ToByte(c.b):X2}{ToByte(c.a):X2}>{s}</color>";
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte) (f * 255);
        }

        public static KeyValuePair<byte, int> MaxPair(this Dictionary<byte, int> self, out bool tie)
        {
            tie = true;
            var result = new KeyValuePair<byte, int>(byte.MaxValue, int.MinValue);
            foreach (var keyValuePair in self)
                if (keyValuePair.Value > result.Value)
                {
                    result = keyValuePair;
                    tie = false;
                }
                else if (keyValuePair.Value == result.Value)
                {
                    tie = true;
                }

            return result;
        }

        private delegate bool DLoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
    }
}