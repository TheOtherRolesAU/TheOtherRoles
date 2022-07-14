using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;

namespace TheOtherRoles.Modules {
    [HarmonyPatch]
    public class CustomHats { 
        private static bool LOADED = false;
        private static bool RUNNING = false;
        public static Material hatShader;

        public static Dictionary<string, HatExtension> CustomHatRegistry = new Dictionary<string, HatExtension>();
        public static HatExtension TestExt = null;

        public class HatExtension {
            public string author { get; set;}
            public string package { get; set;}
            public string condition { get; set;}
            public Sprite FlipImage { get; set;}
            public Sprite BackFlipImage { get; set;}
        }

        public class CustomHat { 
            public string author { get; set;}
            public string package { get; set;}
            public string condition { get; set;}
            public string name { get; set;}
            public string resource { get; set;}
            public string flipresource { get; set;}
            public string backflipresource { get; set;}
            public string backresource { get; set;}
            public string climbresource { get; set;}
            public bool bounce { get; set;}
            public bool adaptive { get; set;}
            public bool behind { get; set;}
        }

        private static List<CustomHat> createCustomHatDetails(string[] hats, bool fromDisk = false) {
            Dictionary<string, CustomHat> fronts = new Dictionary<string, CustomHat>();
            Dictionary<string, string> backs = new Dictionary<string, string>();
            Dictionary<string, string> flips = new Dictionary<string, string>();
            Dictionary<string, string> backflips = new Dictionary<string, string>();
            Dictionary<string, string> climbs = new Dictionary<string, string>();

            for (int i = 0; i < hats.Length; i++) {
                string s = fromDisk ? hats[i].Substring(hats[i].LastIndexOf("\\") + 1).Split('.')[0] : hats[i].Split('.')[3];
                string[] p = s.Split('_');

                HashSet<string> options = new HashSet<string>();
                for (int j = 1; j < p.Length; j++) 
                    options.Add(p[j]);

                if (options.Contains("back") && options.Contains("flip"))
                    backflips.Add(p[0], hats[i]);
                else if (options.Contains("climb")) 
                    climbs.Add(p[0], hats[i]);
                else if (options.Contains("back"))
                    backs.Add(p[0], hats[i]);
                else if (options.Contains("flip"))
                    flips.Add(p[0], hats[i]);
                else {
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
                string br, cr, fr, bfr;
                backs.TryGetValue(k, out br);
                climbs.TryGetValue(k, out cr);
                flips.TryGetValue(k, out fr);
                backflips.TryGetValue(k, out bfr);
                if (br != null)
                    hat.backresource = br;
                if (cr != null)
                    hat.climbresource = cr;
                if (fr != null)
                    hat.flipresource = fr;
                if (bfr != null)
                    hat.backflipresource = bfr;
                if (hat.backresource != null)
                    hat.behind = true;

                customhats.Add(hat);
            }

            return customhats;
        }

        private static Sprite CreateHatSprite(string path, bool fromDisk = false) {
            Texture2D texture = fromDisk ? Helpers.loadTextureFromDisk(path) : Helpers.loadTextureFromResources(path);
            if (texture == null)
                return null;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.53f, 0.575f), texture.width * 0.375f);
            if (sprite == null)
                return null;
            texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
            return sprite;
        }

        private static HatData CreateHatBehaviour(CustomHat ch, bool fromDisk = false, bool testOnly = false) {
            if (hatShader == null) {
                Material tmpShader = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
                hatShader = tmpShader;
            }

            HatData hat = ScriptableObject.CreateInstance<HatData>();
            hat.hatViewData.viewData = ScriptableObject.CreateInstance<HatViewData>();
            hat.hatViewData.viewData.MainImage = CreateHatSprite(ch.resource, fromDisk);
            if (ch.backresource != null) {
                hat.hatViewData.viewData.BackImage = CreateHatSprite(ch.backresource, fromDisk);
                ch.behind = true; // Required to view backresource
            }
            if (ch.climbresource != null)
                hat.hatViewData.viewData.ClimbImage = CreateHatSprite(ch.climbresource, fromDisk);
            hat.name = ch.name;
            hat.displayOrder = 99;
            hat.ProductId = "hat_" + ch.name.Replace(' ', '_');
            hat.InFront = !ch.behind;
            hat.NoBounce = !ch.bounce;
            hat.ChipOffset = new Vector2(0f, 0.2f);
            hat.Free = true;

            if (ch.adaptive && hatShader != null)
                hat.hatViewData.viewData.AltShader = hatShader;

            HatExtension extend = new HatExtension();
            extend.author = ch.author != null ? ch.author : "Unknown";
            extend.package = ch.package != null ? ch.package : "Misc.";
            extend.condition = ch.condition != null ? ch.condition : "none";

            if (ch.flipresource != null)
                 extend.FlipImage = CreateHatSprite(ch.flipresource, fromDisk);
            if (ch.backflipresource != null)
                 extend.BackFlipImage = CreateHatSprite(ch.backflipresource, fromDisk);

            if (testOnly) {
                TestExt = extend;
                TestExt.condition = hat.name;
            } else {
                CustomHatRegistry.Add(hat.name, extend);
            }

            return hat;
        }

        private static HatData CreateHatBehaviour(CustomHatLoader.CustomHatOnline chd) {
            string filePath = Path.GetDirectoryName(Application.dataPath) + @"\TheOtherHats\";
            chd.resource = filePath + chd.resource;
            if (chd.backresource != null)
                chd.backresource = filePath + chd.backresource;
            if (chd.climbresource != null)
                chd.climbresource = filePath + chd.climbresource;
            if (chd.flipresource != null)
                chd.flipresource = filePath + chd.flipresource;
            if (chd.backflipresource != null)
                chd.backflipresource = filePath + chd.backflipresource;
            return CreateHatBehaviour(chd, true);
        }

        [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
        private static class HatManagerPatch {
            static void Prefix(HatManager __instance) {
                if (RUNNING) return;
                RUNNING = true; // prevent simultanious execution
                try {
                    while (CustomHatLoader.hatdetails.Count > 0) {
                        __instance.allHats.Add(CreateHatBehaviour(CustomHatLoader.hatdetails[0]));
                        CustomHatLoader.hatdetails.RemoveAt(0);
                    }
                } catch (System.Exception e) {
                    if (!LOADED)
                        System.Console.WriteLine("Unable to add Custom Hats\n" + e);
                }
                LOADED = true;
            }
            static void Postfix(HatManager __instance) {
                RUNNING = false;
            }
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleAnimation))]
        private static class PlayerPhysicsHandleAnimationPatch {
            private static void Postfix(PlayerPhysics __instance) {
                AnimationClip currentAnimation = __instance.Animator.GetCurrentAnimation();
                if (currentAnimation == __instance.CurrentAnimationGroup.ClimbAnim || currentAnimation == __instance.CurrentAnimationGroup.ClimbDownAnim) return;
                HatParent hp = __instance.myPlayer.cosmetics.hat;
                if (hp.Hat == null) return;
                HatExtension extend = hp.Hat.getHatExtension();
                if (extend == null) return;
                if (extend.FlipImage != null) {
                    if (__instance.FlipX) {
                        hp.FrontLayer.sprite = extend.FlipImage;
                    } else {
                        hp.FrontLayer.sprite = hp.hatView.MainImage;
                    }
                }
                if (extend.BackFlipImage != null) {
                    if (__instance.FlipX) {
                        hp.BackLayer.sprite = extend.BackFlipImage;
                    } else {
                        hp.BackLayer.sprite = hp.hatView.BackImage;
                    }
                }
            }
        }

        // public void SetHat(HatData hat, HatViewData hatViewData, int color)
        // {
        //     this.Hat = hat;
        //     this.hatView = hatViewData;
        //     this.PopulateFromHatViewData();
        //     this.SetColor(color);
        // }

        [HarmonyPatch]
        private static class FreeplayHatTestingPatches
        {
            [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetHat), typeof(int))]
            private static class HatParentSetHatPatchColor {
                static void Prefix(HatParent __instance) {
                    if (DestroyableSingleton<TutorialManager>.InstanceExists) {
                        try {
                            string filePath = Path.GetDirectoryName(Application.dataPath) + @"\TheOtherHats\Test";
                            if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                            DirectoryInfo d = new DirectoryInfo(filePath);
                            string[] filePaths = d.GetFiles("*.png").Select(x => x.FullName).ToArray(); // Getting Text files
                            List<CustomHat> hats = createCustomHatDetails(filePaths, true);
                            if (hats.Count > 0) {
                                __instance.Hat = CreateHatBehaviour(hats[0], true, true);
                            }
                        } catch (System.Exception e) {
                            System.Console.WriteLine("Unable to create test hat\n" + e);
                        }
                    }
                }     
            }

            [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetHat), typeof(HatData), typeof(HatViewData), typeof(int))]
            private static class HatParentSetHatPatchExtra {
                static bool Prefix(HatParent __instance, HatData hat, HatViewData hatViewData, int color)
                {
                    if (!DestroyableSingleton<TutorialManager>.InstanceExists) return true;

                    try 
                    {
                        __instance.Hat = hat;
                        __instance.hatView = hatViewData;
                        string filePath = Path.GetDirectoryName(Application.dataPath) + @"\TheOtherHats\Test";
                        if (!Directory.Exists(filePath)) return true;
                        DirectoryInfo d = new DirectoryInfo(filePath);
                        string[] filePaths = d.GetFiles("*.png").Select(x => x.FullName).ToArray(); // Getting Text files
                        List<CustomHat> hats = createCustomHatDetails(filePaths, true);
                        if (hats.Count > 0) 
                        {
                            __instance.Hat = CreateHatBehaviour(hats[0], true, true);
                            __instance.hatView = __instance.Hat.hatViewData.viewData;
                        }
                    } 
                    catch (System.Exception e) 
                    {
                        System.Console.WriteLine("Unable to create test hat\n" + e);
                        return true;
                    }
                    
                    __instance.PopulateFromHatViewData();
                    __instance.SpriteColor = Palette.PlayerColors[color];
                    return false;
                }     
            }
        }

        [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.OnEnable))]
        public class HatsTabOnEnablePatch {
            public static string innerslothPackageName = "Innersloth Hats";
            private static TMPro.TMP_Text textTemplate;

            public static float createHatPackage(List<System.Tuple<HatData, HatExtension>> hats, string packageName, float YStart, HatsTab __instance) {
                bool isDefaultPackage = innerslothPackageName == packageName;
                if (!isDefaultPackage)
                    hats = hats.OrderBy(x => x.Item1.name).ToList();
                float offset = YStart;

                if (textTemplate != null) {
                    TMPro.TMP_Text title = UnityEngine.Object.Instantiate<TMPro.TMP_Text>(textTemplate, __instance.scroller.Inner);
                    title.transform.localPosition = new Vector3(2.25f, YStart, -1f);
                    title.transform.localScale = Vector3.one * 1.5f;
                    title.fontSize *= 0.5f;
                    title.enableAutoSizing = false;
                    __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) => { title.SetText(packageName); })));
                    offset -= 0.8f * __instance.YOffset;
                }
                for (int i = 0; i < hats.Count; i++) {
                    HatData hat = hats[i].Item1;
                    HatExtension ext = hats[i].Item2;

                    float xpos = __instance.XRange.Lerp((i % __instance.NumPerRow) / (__instance.NumPerRow - 1f));
                    float ypos = offset - (i / __instance.NumPerRow) * (isDefaultPackage ? 1f : 1.5f) * __instance.YOffset;
                    ColorChip colorChip = UnityEngine.Object.Instantiate<ColorChip>(__instance.ColorTabPrefab, __instance.scroller.Inner);
                    if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard) {
                        colorChip.Button.OnMouseOver.AddListener((System.Action)(() => __instance.SelectHat(hat)));
                        colorChip.Button.OnMouseOut.AddListener((System.Action)(() => __instance.SelectHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById(SaveManager.LastHat))));
                        colorChip.Button.OnClick.AddListener((System.Action)(() => __instance.ClickEquip()));
                    } else {
                        colorChip.Button.OnClick.AddListener((System.Action)(() => __instance.SelectHat(hat)));
                    }
                    colorChip.Button.ClickMask = __instance.scroller.Hitbox;
                    Transform background = colorChip.transform.FindChild("Background");
                    Transform foreground = colorChip.transform.FindChild("ForeGround");

                    if (ext != null) {
                        if (background != null) {
                            background.localPosition = Vector3.down * 0.243f;
                            background.localScale = new Vector3(background.localScale.x, 0.8f, background.localScale.y);
                        }
                        if (foreground != null) {
                            foreground.localPosition = Vector3.down * 0.243f;
                        }
                
                        if (textTemplate != null) {
                            TMPro.TMP_Text description = UnityEngine.Object.Instantiate<TMPro.TMP_Text>(textTemplate, colorChip.transform);
                            description.transform.localPosition = new Vector3(0f, -0.65f, -1f);
                            description.alignment = TMPro.TextAlignmentOptions.Center;
                            description.transform.localScale = Vector3.one * 0.65f;
                            __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) => { description.SetText($"{hat.name}\nby {ext.author}"); })));
                        }
                    }
                    
                    colorChip.transform.localPosition = new Vector3(xpos, ypos, -1f);
                    colorChip.Inner.SetHat(hat, __instance.HasLocalPlayer() ? CachedPlayer.LocalPlayer.Data.DefaultOutfit.ColorId : ((int)SaveManager.BodyColor));
                    colorChip.Inner.transform.localPosition = hat.ChipOffset;
                    colorChip.Tag = hat;
                    colorChip.SelectionHighlight.gameObject.SetActive(false);
                    __instance.ColorChips.Add(colorChip);
                }
                return offset - ((hats.Count - 1) / __instance.NumPerRow) * (isDefaultPackage ? 1f : 1.5f) * __instance.YOffset - 1.75f;
            }

            public static void Postfix(HatsTab __instance) {
                for (int i = 0; i < __instance.scroller.Inner.childCount; i++)
                    UnityEngine.Object.Destroy(__instance.scroller.Inner.GetChild(i).gameObject);
                __instance.ColorChips = new Il2CppSystem.Collections.Generic.List<ColorChip>();

                HatData[] unlockedHats = FastDestroyableSingleton<HatManager>.Instance.GetUnlockedHats();
                Dictionary<string, List<System.Tuple<HatData, HatExtension>>> packages = new Dictionary<string, List<System.Tuple<HatData, HatExtension>>>();

                foreach (HatData hatBehaviour in unlockedHats) {
                    HatExtension ext = hatBehaviour.getHatExtension();

                    if (ext != null) {
                        if (!packages.ContainsKey(ext.package)) 
                            packages[ext.package] = new List<System.Tuple<HatData, HatExtension>>();
                        packages[ext.package].Add(new System.Tuple<HatData, HatExtension>(hatBehaviour, ext));
                    } else {
                        if (!packages.ContainsKey(innerslothPackageName)) 
                            packages[innerslothPackageName] = new List<System.Tuple<HatData, HatExtension>>();
                        packages[innerslothPackageName].Add(new System.Tuple<HatData, HatExtension>(hatBehaviour, null));
                    }
                }

                float YOffset = __instance.YStart;
                textTemplate = GameObject.Find("HatsGroup").transform.FindChild("Text").GetComponent<TMPro.TMP_Text>();

                var orderedKeys = packages.Keys.OrderBy((string x) => {
                    if (x == innerslothPackageName) return 1000;
                    if (x == "Developer Hats") return 0;
                    return 500;
                });
                foreach (string key in orderedKeys) {
                    List<System.Tuple<HatData, HatExtension>> value = packages[key];
                    YOffset = createHatPackage(value, key, YOffset, __instance);
                }

                __instance.scroller.ContentYBounds.max = -(YOffset + 4.1f);
            }
        }

    }

    public class CustomHatLoader {
        public static bool running = false;
        private const string REPO = "https://raw.githubusercontent.com/JustASysAdmin/TheOtherHats/master";

        public static List<CustomHatOnline> hatdetails = new List<CustomHatOnline>();
        private static Task hatFetchTask = null;
        public static void LaunchHatFetcher() {
            if (running)
                return;
            running = true;
            hatFetchTask = LaunchHatFetcherAsync();
        }

        private static async Task LaunchHatFetcherAsync() {
            try {
                HttpStatusCode status = await FetchHats();
                if (status != HttpStatusCode.OK)
                    System.Console.WriteLine("Custom Hats could not be loaded\n");
            } catch (System.Exception e) {
                System.Console.WriteLine("Unable to fetch hats\n" + e.Message);
            }
            running = false;
        }

        private static string sanitizeResourcePath(string res) {
            if (res == null || !res.EndsWith(".png")) 
                return null;

            res = res.Replace("\\", "")
                     .Replace("/", "")
                     .Replace("*", "")
                     .Replace("..", "");   
            return res;
        }

        public static async Task<HttpStatusCode> FetchHats() {
            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue{ NoCache = true };
			var response = await http.GetAsync(new System.Uri($"{REPO}/CustomHats.json"), HttpCompletionOption.ResponseContentRead);
            try {
                if (response.StatusCode != HttpStatusCode.OK) return response.StatusCode;
                if (response.Content == null) {
                    System.Console.WriteLine("Server returned no data: " + response.StatusCode.ToString());
                    return HttpStatusCode.ExpectationFailed;
                }
                string json = await response.Content.ReadAsStringAsync();
                JToken jobj = JObject.Parse(json)["hats"];
                if (!jobj.HasValues) return HttpStatusCode.ExpectationFailed;

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
                        info.flipresource = sanitizeResourcePath(current["flipresource"]?.ToString());
                        info.reshashf = current["reshashf"]?.ToString();
                        info.backflipresource = sanitizeResourcePath(current["backflipresource"]?.ToString());
                        info.reshashbf = current["reshashbf"]?.ToString();

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
                if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                MD5 md5 = MD5.Create();
                foreach (CustomHatOnline data in hatdatas) {
    	            if (doesResourceRequireDownload(filePath + data.resource, data.reshasha, md5))
                        markedfordownload.Add(data.resource);
    	            if (data.backresource != null && doesResourceRequireDownload(filePath + data.backresource, data.reshashb, md5))
                        markedfordownload.Add(data.backresource);
    	            if (data.climbresource != null && doesResourceRequireDownload(filePath + data.climbresource, data.reshashc, md5))
                        markedfordownload.Add(data.climbresource);
    	            if (data.flipresource != null && doesResourceRequireDownload(filePath + data.flipresource, data.reshashf, md5))
                        markedfordownload.Add(data.flipresource);
    	            if (data.backflipresource != null && doesResourceRequireDownload(filePath + data.backflipresource, data.reshashbf, md5))
                        markedfordownload.Add(data.backflipresource);
                }
                
                foreach(var file in markedfordownload) {
                    
                    var hatFileResponse = await http.GetAsync($"{REPO}/hats/{file}", HttpCompletionOption.ResponseContentRead);
                    if (hatFileResponse.StatusCode != HttpStatusCode.OK) continue;
                    using (var responseStream = await hatFileResponse.Content.ReadAsStreamAsync()) {
                        using (var fileStream = File.Create($"{filePath}\\{file}")) {
                            responseStream.CopyTo(fileStream);
                        }
                    }
                }

                hatdetails = hatdatas;
            } catch (System.Exception ex) {
                TheOtherRolesPlugin.Instance.Log.LogError(ex.ToString());
                System.Console.WriteLine(ex);
            }
            return HttpStatusCode.OK;
        }

        private static bool doesResourceRequireDownload(string respath, string reshash, MD5 md5) {
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
            public string reshashf { get; set;}
            public string reshashbf { get; set;}
        }   
    }
    public static class CustomHatExtensions {
        public static CustomHats.HatExtension getHatExtension(this HatData hat) {
            CustomHats.HatExtension ret = null;
            if (CustomHats.TestExt != null && CustomHats.TestExt.condition.Equals(hat.name)) {
                return CustomHats.TestExt;
            }
            CustomHats.CustomHatRegistry.TryGetValue(hat.name, out ret);
            return ret;
        }
    }
}
