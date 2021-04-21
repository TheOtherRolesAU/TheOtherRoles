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

namespace TheOtherRoles {
    [HarmonyPatch]
    public class CustomHats { 
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

        private static List<CustomHat> createCustomHatDetails(string[] hats, bool fromDisk = false) {
            Dictionary<string, CustomHat> fronts = new Dictionary<string, CustomHat>();
            Dictionary<string, string> backs = new Dictionary<string, string>();
            Dictionary<string, string> climbs = new Dictionary<string, string>();

            for (int i = 0; i < hats.Length; i++) {
                string s = fromDisk ? hats[i].Substring(hats[i].LastIndexOf("\\") + 1).Split('.')[0] : hats[i].Split('.')[3];
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
                if (hat.backresource != null)
                    hat.behind = true;
                customhats.Add(hat);
            }

            return customhats;
        }

        private static Sprite CreateHatSprite(string path, bool fromDisk = false) {
            Texture2D texture = fromDisk ? Helpers.loadTextureFromDisk(path) : Helpers.loadTextureFromResources(path);
            return texture == null ? null : Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.8f), texture.width * 0.75f);
        }

        private static HatBehaviour CreateHatBehaviour(CustomHat ch, bool fromDisk = false) {
            HatBehaviour hat = new HatBehaviour();
            hat.MainImage = CreateHatSprite(ch.resource, fromDisk);
            if (ch.backresource != null)
                hat.BackImage = CreateHatSprite(ch.backresource, fromDisk);
            if (ch.climbresource != null)
                hat.ClimbImage = CreateHatSprite(ch.climbresource, fromDisk);
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

        private static void loadHatShader() {
            if (hatShader != null || !DestroyableSingleton<HatManager>.InstanceExists) return;

            foreach (HatBehaviour hat in DestroyableSingleton<HatManager>.Instance.AllHats) {
                if (hat.AltShader != null) {
                    hatShader = hat.AltShader;
                    break;
                }
            }
        }

        [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
        private static class HatManagerPatch {
            static bool Prefix(HatManager __instance) {
                try {
                    if (!LOADED) {
                        loadHatShader();

                        Assembly assembly = Assembly.GetExecutingAssembly();
                        string hatres = $"{assembly.GetName().Name}.Resources.CustomHats";
                        string[] hats = (from r in assembly.GetManifestResourceNames()
                                            where r.StartsWith(hatres) && r.EndsWith(".png")
                                            select r).ToArray<string>();

                        List<CustomHat> customhats = createCustomHatDetails(hats);
                        foreach (CustomHat ch in customhats)
                            __instance.AllHats.Add(CreateHatBehaviour(ch));

                        LOADED = true;
                    }
                    return true;
                } catch (Exception e) {
                    System.Console.WriteLine("Unable to add Custom Hats\n" + e);
                    return false;
                }
            }
        }

        [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetHat), new Type[] { typeof(uint), typeof(int) })]
        private static class HatParentSetHatPatch {
            static void Postfix(HatParent __instance, [HarmonyArgument(0)]uint hatId, [HarmonyArgument(1)]int color) {
                if (DestroyableSingleton<TutorialManager>.InstanceExists) {
                    try {
                        string filePath = Path.GetDirectoryName(Application.dataPath) + @"\TheOtherHats\Test";
                        DirectoryInfo d = new DirectoryInfo(filePath);
                        string[] filePaths = d.GetFiles("*.png").Select(x => x.FullName).ToArray(); // Getting Text files
                        List<CustomHat> customHats = createCustomHatDetails(filePaths, true);
                        if (customHats.Count > 0) {
                            loadHatShader();
                            __instance.Hat = CreateHatBehaviour(customHats[0], true);
                            __instance.SetHat(color);
                        }
                    } catch (Exception e) {
                        System.Console.WriteLine("Unable to create test hat\n" + e);
                    }
                }
            }     
        }
    }
}