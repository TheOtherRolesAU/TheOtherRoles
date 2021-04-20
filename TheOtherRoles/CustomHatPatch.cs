using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Collections;
using UnhollowerBaseLib;
using System.Linq;

namespace HatMod {
    [HarmonyPatch]
    public class CustomHatPatch { 
        private static bool LOADED = false;
        public static Material hatShader;

        protected internal struct CustomHat { 
            public string name;
            public string resource;
            public string backresource;
            public string climbresource;
            public bool bounce;
            public bool adaptive;
            public bool behind;
        }

        private static List<CustomHat> getAllCustomHats() {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string hatres = $"{assembly.GetName().Name}.Resources.CustomHats";

			string[] hats = (from r in assembly.GetManifestResourceNames()
			                     where r.StartsWith(hatres) && r.EndsWith(".png")
			                     select r).ToArray<string>();

            Dictionary<string, CustomHat> fronts = new Dictionary<string, CustomHat>();
            Dictionary<string, string> backs = new Dictionary<string, string>();
            Dictionary<string, string> climbs = new Dictionary<string, string>();

            for (int i = 0; i < hats.Length; i++) {
                string s = hats[i].Split('.')[3];
                string[] p = s.Split('_');

                HashSet<string> options = new HashSet<string>();
                for (int j = 1; j < p.Length; j++) 
                    options.Add(p[j]);

                if (options.Contains("climb")) 
                    climbs.Add(p[0], hats[i]);
                if (options.Contains("back"))
                    backs.Add(p[0], hats[i]);
                
                if (!options.Contains("back") && !options.Contains("climb")) {
                    CustomHat custom = new CustomHat { resource = hats[i] };
                    custom.name = p[0].Replace('-', ' ');
                    custom.bounce = options.Contains("bounce");
                    custom.adaptive = options.Contains("adaptive");
                    custom.behind = options.Contains("behind");
                    
                    fronts.Add(p[0], custom);
                }
            }

            List<CustomHat> customhats = new List<CustomHat>();

            foreach (string k in fronts.Keys) {
                CustomHat hat = fronts[k];
                backs.TryGetValue(k, out hat.backresource);
                climbs.TryGetValue(k, out hat.climbresource);
                customhats.Add(hat);
            }

            return customhats;
        }

        private static Sprite CreateHatSprite(string resource) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(resource);
            byte[] data = stream.ReadFully();
            Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            SpriteLoader.LoadImage(texture, data, true);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.8f), texture.width * 0.75f);
        }

        private static HatBehaviour CreateHat(CustomHat ch) {
            HatBehaviour hat = new HatBehaviour();
            hat.MainImage = CreateHatSprite(ch.resource);
            if (ch.backresource != null)
                hat.BackImage = CreateHatSprite(ch.backresource);
            if (ch.climbresource != null)
                hat.ClimbImage = CreateHatSprite(ch.climbresource);
            hat.name = ch.name;
            hat.Order = 99;
            hat.ProductId = "hat_" + ch.name.Replace(' ', '_');
            hat.InFront = !ch.behind;
            hat.NoBounce = !ch.bounce;
            hat.ChipOffset = new Vector2(0f, 0.35f);

            if (ch.adaptive && hatShader != null)
                hat.AltShader = hatShader;

            return hat;
        }

        [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
        private static class HatManagerPatch {
            static bool Prefix(HatManager __instance) {
                try {
                    if (!LOADED) {
						for (int i = 0; i < __instance.AllHats.Count && hatShader == null; i++)
							if (__instance.AllHats[i].AltShader != null)
								hatShader = __instance.AllHats[i].AltShader; // Grab Original Shader

                        List<CustomHat> customhats = getAllCustomHats();
                        // System.Console.WriteLine("Adding hats");
                        foreach (CustomHat ch in customhats)
                            __instance.AllHats.Add(CreateHat(ch));

                        LOADED = true;
                    }
                    return true;
                } catch (Exception e) {
                    System.Console.WriteLine("Unable to add Custom Hats\n" + e);
                    return false;
                }
            }
        } 
    }

    public static class SpriteLoader {
        private static DLoadImage _DLoadImage;
        private delegate bool DLoadImage(IntPtr tex, IntPtr data, bool markNonReadable);

		public static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable) {
            if (_DLoadImage == null)
                _DLoadImage = IL2CPP.ResolveICall<DLoadImage>("UnityEngine.ImageConversion::LoadImage");
			Il2CppStructArray<byte> il2CppStructArray = data;
			return _DLoadImage(tex.Pointer, il2CppStructArray.Pointer, markNonReadable);
		}

        public static byte[] ReadFully(this Stream s) {
            using (MemoryStream ms = new MemoryStream()) {
                s.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}