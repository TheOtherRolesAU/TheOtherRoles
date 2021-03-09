using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Reactor.Extensions;
using System.Collections;
using Reactor.Unstrip;
using UnityEngine;
using static BonusRoles.BonusRoles;

namespace BonusRoles {
    public static class Helpers {
        public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit) {
            Texture2D texture = GUIExtensions.CreateEmptyTexture();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);
            byte[] byteTexture = Reactor.Extensions.Extensions.ReadFully(stream);
            ImageConversion.LoadImage(texture, byteTexture, false);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
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

            if (spriteAnim.GetCurrentAnimation().name == skinLayer.skin.RunAnim.name) clip = nextSkin.RunAnim;
            else if (spriteAnim.GetCurrentAnimation().name == skinLayer.skin.SpawnAnim.name) clip = nextSkin.SpawnAnim;
            else if (spriteAnim.GetCurrentAnimation().name == skinLayer.skin.ExitVentAnim.name) clip = nextSkin.ExitVentAnim;
            else if (spriteAnim.GetCurrentAnimation().name == skinLayer.skin.EnterVentAnim.name) clip = nextSkin.EnterVentAnim;
            else if (spriteAnim.GetCurrentAnimation().name == skinLayer.skin.IdleAnim.name) clip = nextSkin.IdleAnim;

            float progress = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
            skinLayer.skin = nextSkin;

            spriteAnim.Play(clip, 1f);
            anim.Play("a", 0, progress % 1);
            anim.Update(0f);
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

        public static string colorToHex(Color c) {
            return string.Format("[{0:X2}{1:X2}{2:X2}{3:X2}]", (int)c.r, (int)c.g, (int)c.b, (int)c.a);
        }
    }

    public class SeerInfo {
        public Color color;
        public string roleName;
        public bool isGood;

        public static SeerInfo getSeerInfoForPlayer(PlayerControl p) {
            string r = "";
            bool g = true;
            Color c = Color.white;

            if (Jester.jester != null && p == Jester.jester) {
                r = "Jester";
                c = Jester.color;
                g = false;
            }
            else if (Mayor.mayor != null && p == Mayor.mayor) {
                r = "Mayor";   
                c = Mayor.color;
            }
            else if (Engineer.engineer != null && p == Engineer.engineer) {
                r = "Engineer";   
                c = Engineer.color;
            }
            else if (Sheriff.sheriff != null && p == Sheriff.sheriff) {
                r = "Sheriff";   
                c = Sheriff.color;
            }
            else if (Lighter.lighter != null && p == Lighter.lighter) {
                r = "Lighter";   
                c = Lighter.color;
            }
            else if (Godfather.godfather != null && p == Godfather.godfather) {
                r = "Godfather";   
                c = Godfather.color;
                g = false;
            }
            else if (Mafioso.mafioso != null && p == Mafioso.mafioso) {
                r = "Mafioso";   
                c = Mafioso.color;
                g = false;
            }
            else if (Janitor.janitor != null && p == Janitor.janitor) {
                r = "Janitor";
                c = Janitor.color;
                g = false;
            }
            else if (Morphling.morphling != null && p == Morphling.morphling) {
                r = "Morphling";
                c = Morphling.color;
                g = false;
            }
            else if (Camouflager.camouflager != null && p == Camouflager.camouflager) {
                r = "Camouflager";
                c = Camouflager.color;
                g = false;
            }
            else if (Detective.detective != null && p == Detective.detective) {
                r = "Detective";
                c = Detective.color;
            }
            else if (TimeMaster.timeMaster != null && p == TimeMaster.timeMaster) {
                r = "Time Master";
                c = TimeMaster.color;
            }
            else if (Medic.medic != null && p == Medic.medic) {
                r = "Medic";
                c = Medic.color;
            }
            else if (Shifter.shifter != null && p == Shifter.shifter) {
                r = "Shifter";
                c = Shifter.color;
                g = false;
            }
            else if (Swapper.swapper != null && p == Swapper.swapper) {
                r = "Swapper";
                c = Swapper.color;
            }
            else if (Lovers.lover1 != null && p == Lovers.lover1) {
                r = Lovers.lover1.Data.IsImpostor ? "ImpLover" : "Lover";
                c = Lovers.lover1.Data.IsImpostor ? Palette.ImpostorRed : Lovers.color;
                g = !Lovers.lover1.Data.IsImpostor;
            }
            else if (Lovers.lover2 != null && p == Lovers.lover2) {
                r = Lovers.lover2.Data.IsImpostor ? "ImpLover" : "Lover";
                c = Lovers.lover2.Data.IsImpostor ? Palette.ImpostorRed : Lovers.color;
                g = !Lovers.lover2.Data.IsImpostor;
            }
            else if (Seer.seer != null && p == Seer.seer) { 
                r = "Seer";
                c = Seer.color;
            }
            else if (Spy.spy != null && p == Spy.spy) { 
                r = "Spy";
                c = Spy.color;
            }
            else if (Child.child != null && p == Child.child) { 
                r = "Child";
                c = Child.color;
            }
            else if (p.Data.IsImpostor) { // Just Impostor
                r = "Impostor";
                c = Palette.ImpostorRed;
                g = false;
            }
            else if (!p.Data.IsImpostor) { // Just Crewmate
                r = "Crewmate";
                c = Color.white;
            }

            SeerInfo result = new SeerInfo();
            result.roleName = r;
            result.color = c;
            result.isGood = g;

            return result;
        }
    }
}