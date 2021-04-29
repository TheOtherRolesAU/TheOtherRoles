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

        private static List<TMPro.TMP_Text> hatsTabCustomTexts = new List<TMPro.TMP_Text>();

        [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.OnEnable))]
        public class HatsTabOnEnablePatch {
            public static string innerslothPackageName = "Innersloth Hats";
            private static TMPro.TMP_Text textTemplate;

            public static float createHatPackage(List<System.Tuple<HatBehaviour, HatExtension>> hats, string packageName, float YStart, HatsTab __instance) {
                bool isDefaultPackage = innerslothPackageName == packageName;
                float offset = YStart;

                if (textTemplate != null) {
                    TMPro.TMP_Text title = UnityEngine.Object.Instantiate<TMPro.TMP_Text>(textTemplate, __instance.scroller.Inner);
                    title.transform.localPosition = new Vector3(2.25f, YStart, -1f);
                    title.transform.localScale = Vector3.one * 1.5f;
                    // title.currentFontSize
                    title.fontSize *= 0.5f;
                    title.enableAutoSizing = false;
                    __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) => { title.SetText(packageName); })));
                    offset -= 0.8f * __instance.YOffset;
                    hatsTabCustomTexts.Add(title);
                }
                for (int i = 0; i < hats.Count; i++) {
                    HatBehaviour hat = hats[i].Item1;
                    HatExtension ext = hats[i].Item2;

                    float xpos = __instance.XRange.Lerp((i % __instance.NumPerRow) / (__instance.NumPerRow - 1f));
                    float ypos = offset - i / __instance.NumPerRow * (isDefaultPackage ? 1f : 1.5f) * __instance.YOffset;
                    ColorChip colorChip = UnityEngine.Object.Instantiate<ColorChip>(__instance.ColorTabPrefab, __instance.scroller.Inner);
                    if (ext != null) {
                        Transform background = colorChip.transform.FindChild("Background");
                        Transform foreground = colorChip.transform.FindChild("ForeGround");

                        if (background != null) {
                            background.localScale = new Vector3(1, 1.5f, 1);
                            background.localPosition = Vector3.down * 0.243f;
                        }
                        if (foreground != null) {
                            foreground.localPosition = Vector3.down * 0.243f;
                        }
                
                        if (textTemplate != null) {
                            TMPro.TMP_Text description = UnityEngine.Object.Instantiate<TMPro.TMP_Text>(textTemplate, colorChip.transform);
                            description.transform.localPosition = new Vector3(0f, -0.75f, -1f);
                            description.transform.localScale = Vector3.one * 0.7f;
                            __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) => { description.SetText($"{hat.name}\nby {ext.author}"); })));
                            hatsTabCustomTexts.Add(description);
                        }

                        if (!ext.isUnlocked()) { // Hat is locked
                            UnityEngine.Object.Destroy(colorChip.Button);
                            var overlay = UnityEngine.Object.Instantiate(colorChip.InUseForeground, colorChip.transform);
                            overlay.SetActive(true);
                        }
                    }
                    
                    colorChip.transform.localPosition = new Vector3(xpos, ypos, -1f);
                    colorChip.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => { __instance.SelectHat(hat); }));
                    colorChip.Inner.SetHat(hat, PlayerControl.LocalPlayer.Data.ColorId);
                    colorChip.Inner.transform.localPosition = hat.ChipOffset;
                    colorChip.Tag = hat;
                    __instance.ColorChips.Add(colorChip);
                }
                return offset - ((hats.Count - 1) / __instance.NumPerRow * (isDefaultPackage ? 1f : 1.5f) + 0.7f) * __instance.YOffset;
            }

            public static bool Prefix(HatsTab __instance) {
                PlayerControl.SetPlayerMaterialColors(PlayerControl.LocalPlayer.Data.ColorId, __instance.DemoImage);
                __instance.HatImage.SetHat(SaveManager.LastHat, PlayerControl.LocalPlayer.Data.ColorId);
                PlayerControl.SetSkinImage(SaveManager.LastSkin, __instance.SkinImage);
                PlayerControl.SetPetImage(SaveManager.LastPet, PlayerControl.LocalPlayer.Data.ColorId, __instance.PetImage);

                HatBehaviour[] unlockedHats = DestroyableSingleton<HatManager>.Instance.GetUnlockedHats();
                Dictionary<string, List<System.Tuple<HatBehaviour, HatExtension>>> packages = new Dictionary<string, List<System.Tuple<HatBehaviour, HatExtension>>>();
                hatsTabCustomTexts = new List<TMPro.TMP_Text>();

                foreach (HatBehaviour hatBehaviour in unlockedHats) {
                    HatExtension ext = hatBehaviour.getHatExtension();

                    if (ext != null) {
                        if (!packages.ContainsKey(ext.package)) 
                            packages[ext.package] = new List<System.Tuple<HatBehaviour, HatExtension>>();
                        packages[ext.package].Add(new System.Tuple<HatBehaviour, HatExtension>(hatBehaviour, ext));
                    } else {
                        if (!packages.ContainsKey(innerslothPackageName)) 
                            packages[innerslothPackageName] = new List<System.Tuple<HatBehaviour, HatExtension>>();
                        packages[innerslothPackageName].Add(new System.Tuple<HatBehaviour, HatExtension>(hatBehaviour, null));
                    }
                }

                float YOffset = __instance.YStart;

                var hatButton = GameObject.Find("HatButton");

                if (hatButton != null && hatButton.transform.FindChild("ButtonText_TMP") != null) {
                    textTemplate = hatButton.transform.FindChild("ButtonText_TMP").GetComponent<TMPro.TMP_Text>();
                }

                var orderedKeys = packages.Keys.OrderBy((string x) => x == innerslothPackageName ? 1000 : 0);
                foreach (string key in orderedKeys) {
                    List<System.Tuple<HatBehaviour, HatExtension>> value = packages[key];
                    YOffset += createHatPackage(value, key, YOffset, __instance);
                }
                __instance.scroller.YBounds.max = (YOffset - 0.6f) * -0.75f;
                return false;
            }
        }

        [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.Update))]
        public class HatsTabUpdatePatch {
            public static void Postfix(HatsTab __instance) {
                // Manually hide all custom TMPro.TMP_Text objects that are outside the ScrollRect
                foreach (TMPro.TMP_Text customText in hatsTabCustomTexts) {
                    if (customText != null && customText.transform != null && customText.gameObject != null) {
                        bool active = customText.transform.position.y <= 3.75f && customText.transform.position.y >= 0.3f;
                        float epsilon = Mathf.Min(Mathf.Abs(customText.transform.position.y - 3.75f), Mathf.Abs(customText.transform.position.y - 0.35f));
                        if (active != customText.gameObject.active && epsilon > 0.1f) customText.gameObject.SetActive(active);
                    }
                }
            }
        }
    }

    public class CustomHatLoader {
        public bool running = false;
        private const string REPO = "https://raw.githubusercontent.com/thunderstorm584/TheOtherHats/master";

        public static List<CustomHatOnline> hatdetails = new List<CustomHatOnline>();

        public void LaunchHatFetcher() {
            if (this.running)
                return;
            this.running = true;
            this.LaunchHatFetcherAsync();
        }

        private async void LaunchHatFetcherAsync() {
            try {
                HttpStatusCode status = await this.FetchHats();
                if (status != HttpStatusCode.OK)
                    System.Console.WriteLine("Custom Hats could not be loaded\n");
            } catch (System.Exception e) {
                System.Console.WriteLine("Unable to fetch hats\n" + e.Message);
            }
            this.running = false;
        }

        private string sanitizeResourcePath(string res) {
            if (res == null || !res.EndsWith(".png")) 
                return null;

            res = res.Replace("\\", "")
                     .Replace("/", "")
                     .Replace("*", "")
                     .Replace("..", "");   
            return res;
        }

        public async Task<HttpStatusCode> FetchHats() {
            using (HttpClient http = new HttpClient()) {
                http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue{ NoCache = true };
                using (HttpRequestMessage msg = new HttpRequestMessage()) {
                    msg.Method = HttpMethod.Get;
                    msg.RequestUri = new System.Uri($"{REPO}/CustomHats.json");
                    HttpResponseMessage httpResponseMessage = await http.SendAsync(msg);
                    using (HttpResponseMessage res = httpResponseMessage) {
                        try {
                            if (res.Content == null) {
                                System.Console.WriteLine("Server returned no data: " + res.StatusCode.ToString());
                                return HttpStatusCode.ExpectationFailed;
                            }
                            if (res.StatusCode != HttpStatusCode.OK) 
                                return res.StatusCode;

                            string json = await res.Content.ReadAsStringAsync();
                            // System.Console.WriteLine(json);

                            JToken jobj = JObject.Parse(json)["hats"];
                            if (!jobj.HasValues) 
                                return HttpStatusCode.ExpectationFailed;

                            List<CustomHatOnline> hatdatas = new List<CustomHatOnline>();

                            for (JToken current = jobj.First; current != null; current = current.Next) {
                                if (current.HasValues) {
                                    CustomHatOnline info = new CustomHatOnline();

                                    info.name = current["name"]?.ToString();
                                    info.resource = sanitizeResourcePath(current["resource"]?.ToString());
                                    if (info.resource == null || info.name == null) // required
                                        continue;
                                    info.reshasha = current["reshasha"]?.ToString();
                                    info.backresource = sanitizeResourcePath(current["backresource"]?.ToString());
                                    info.reshashb = current["reshashb"]?.ToString();
                                    info.climbresource = sanitizeResourcePath(current["climbresource"]?.ToString());
                                    info.reshashc = current["reshashc"]?.ToString();
                                    info.author = current["author"]?.ToString();
                                    info.package = current["package"]?.ToString();
                                    info.condition = current["condition"]?.ToString();
                                    info.bounce = current["bounce"] != null;
                                    info.adaptive = current["adaptive"] != null;
                                    info.behind = current["behind"] != null;
                                    hatdatas.Add(info);
                                }
                            }

                            List<string> markedfordownload = new List<string>();

                            string filePath = Path.GetDirectoryName(Application.dataPath) + @"\TheOtherHats\";
                            using (MD5 md5 = MD5.Create()) {
                                foreach (CustomHatOnline data in hatdatas) {
    	                            if (doesResourceRequireDownload(filePath + data.resource, data.reshasha, md5))
                                        markedfordownload.Add(data.resource);
    	                            if (data.backresource != null && doesResourceRequireDownload(filePath + data.backresource, data.reshashb, md5))
                                        markedfordownload.Add(data.backresource);
    	                            if (data.climbresource != null && doesResourceRequireDownload(filePath + data.climbresource, data.reshashc, md5))
                                        markedfordownload.Add(data.climbresource);
                                }
                            }
                            markedfordownload.AsParallel().ForAll(file => {
                                System.Uri url = new System.Uri($"{REPO}/hats/{file}");
                                // System.Console.WriteLine($"Downloading {file}");
                                var fres = http.GetAsync(url, HttpCompletionOption.ResponseContentRead);
                                if (fres.Result.StatusCode != HttpStatusCode.OK)
                                    return;
                                using (var resStream = fres.Result.Content.ReadAsStreamAsync().Result) {
                                    using (var fileStream = File.Create($"{filePath}\\{file}")) {
                                        resStream.CopyTo(fileStream);
                                    }
                                }
                            });

                            hatdetails = hatdatas;
                        } catch (System.Exception ex) {
                            System.Console.WriteLine(ex);
                        }
                    }
                }
            }
            return HttpStatusCode.OK;
        }

        private bool doesResourceRequireDownload(string respath, string reshash, MD5 md5) {
            if (reshash == null || !File.Exists(respath))
                return true;

            using (var stream = File.OpenRead(respath)) {
                var hash = System.BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                return !reshash.Equals(hash);
            }
        }

        public class CustomHatOnline : CustomHats.CustomHat { 
            public string reshasha { get; set;}
            public string reshashb { get; set;}
            public string reshashc { get; set;}
        }   
    }
    public static class CustomHatExtensions {
        public static CustomHats.HatExtension getHatExtension(this HatBehaviour hat) {
            CustomHats.HatExtension ret = null;
            CustomHats.CustomHatRegistry.TryGetValue(hat.name, out ret);
            return ret;
        }
    }
}