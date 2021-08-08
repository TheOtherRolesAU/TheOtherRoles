using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Modules
{
    [HarmonyPatch]
    public class CustomHats
    {
        private static bool LOADED;
        private static bool RUNNING;
        private static Material hatShader;

        public static readonly Dictionary<string, HatExtension> CustomHatRegistry =
            new();

        public static HatExtension testExt;

        private static List<TMP_Text> hatsTabCustomTexts = new();

        private static List<CustomHat> CreateCustomHatDetails(IEnumerable<string> hats, bool fromDisk = false)
        {
            var fronts = new Dictionary<string, CustomHat>();
            var backs = new Dictionary<string, string>();
            var flips = new Dictionary<string, string>();
            var backflips = new Dictionary<string, string>();
            var climbs = new Dictionary<string, string>();

            foreach (var t in hats)
            {
                var s = fromDisk
                    ? t.Substring(t.LastIndexOf("\\", StringComparison.Ordinal) + 1).Split('.')[0]
                    : t.Split('.')[3];
                var p = s.Split('_');

                var options = new HashSet<string>();
                for (var j = 1; j < p.Length; j++)
                    options.Add(p[j]);

                if (options.Contains("back") && options.Contains("flip"))
                {
                    backflips.Add(p[0], t);
                }
                else if (options.Contains("climb"))
                {
                    climbs.Add(p[0], t);
                }
                else if (options.Contains("back"))
                {
                    backs.Add(p[0], t);
                }
                else if (options.Contains("flip"))
                {
                    flips.Add(p[0], t);
                }
                else
                {
                    var custom = new CustomHat
                    {
                        resource = t,
                        name = p[0].Replace('-', ' '),
                        bounce = options.Contains("bounce"),
                        adaptive = options.Contains("adaptive"),
                        behind = options.Contains("behind")
                    };

                    fronts.Add(p[0], custom);
                }
            }

            var customHats = new List<CustomHat>();

            foreach (var k in fronts.Keys)
            {
                var hat = fronts[k];
                backs.TryGetValue(k, out var br);
                climbs.TryGetValue(k, out var cr);
                flips.TryGetValue(k, out var fr);
                backflips.TryGetValue(k, out var bfr);
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

                customHats.Add(hat);
            }

            return customHats;
        }

        private static Sprite CreateHatSprite(string path, bool fromDisk = false)
        {
            var texture = fromDisk ? Helpers.LoadTextureFromDisk(path) : Helpers.LoadTextureFromResources(path);
            if (texture == null)
                return null;
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.53f, 0.575f), texture.width * 0.375f);
            if (sprite == null)
                return null;
            texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
            return sprite;
        }

        private static HatBehaviour CreateHatBehaviour(CustomHat ch, bool fromDisk = false, bool testOnly = false)
        {
            if (hatShader == null && DestroyableSingleton<HatManager>.InstanceExists)
                foreach (var h in DestroyableSingleton<HatManager>.Instance.AllHats)
                    if (h.AltShader != null)
                    {
                        hatShader = h.AltShader;
                        break;
                    }

            var hat = ScriptableObject.CreateInstance<HatBehaviour>();
            hat.MainImage = CreateHatSprite(ch.resource, fromDisk);
            if (ch.backresource != null)
            {
                hat.BackImage = CreateHatSprite(ch.backresource, fromDisk);
                ch.behind = true; // Required to view backresource
            }

            if (ch.climbresource != null)
                hat.ClimbImage = CreateHatSprite(ch.climbresource, fromDisk);
            hat.name = ch.name;
            hat.Order = 99;
            hat.ProductId = "hat_" + ch.name.Replace(' ', '_');
            hat.InFront = !ch.behind;
            hat.NoBounce = !ch.bounce;
            hat.ChipOffset = new Vector2(0f, 0.2f);

            if (ch.adaptive && hatShader != null)
                hat.AltShader = hatShader;

            var extend = new HatExtension
            {
                author = ch.author ?? "Unknown",
                package = ch.package ?? "Misc.",
                condition = ch.condition ?? "none"
            };

            if (ch.flipresource != null)
                extend.FlipImage = CreateHatSprite(ch.flipresource, fromDisk);
            if (ch.backflipresource != null)
                extend.BackFlipImage = CreateHatSprite(ch.backflipresource, fromDisk);

            if (testOnly)
            {
                testExt = extend;
                testExt.condition = hat.name;
            }
            else
            {
                CustomHatRegistry.Add(hat.name, extend);
            }

            return hat;
        }

        private static HatBehaviour CreateHatBehaviour(CustomHatLoader.CustomHatOnline chd)
        {
            var filePath = Path.GetDirectoryName(Application.dataPath) + @"\TheOtherHats\";
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

        public class HatExtension
        {
            public string author { get; set; }
            public string package { get; set; }
            public string condition { get; set; }
            public Sprite FlipImage { get; set; }
            public Sprite BackFlipImage { get; set; }

            public bool IsUnlocked()
            {
                return condition == null || condition.ToLower() == "none";
            }
        }

        public class CustomHat
        {
            public string author { get; set; }
            public string package { get; set; }
            public string condition { get; set; }
            public string name { get; set; }
            public string resource { get; set; }
            public string flipresource { get; set; }
            public string backflipresource { get; set; }
            public string backresource { get; set; }
            public string climbresource { get; set; }
            public bool bounce { get; set; }
            public bool adaptive { get; set; }
            public bool behind { get; set; }
        }

        [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
        private static class HatManagerPatch
        {
            private static void Prefix(HatManager __instance)
            {
                if (RUNNING) return;
                RUNNING = true; // prevent simultaneous execution
                try
                {
                    if (!LOADED)
                    {
                        var assembly = Assembly.GetExecutingAssembly();
                        var hatRes = $"{assembly.GetName().Name}.Resources.CustomHats";
                        var hats = (from r in assembly.GetManifestResourceNames()
                            where r.StartsWith(hatRes) && r.EndsWith(".png")
                            select r).ToArray();

                        var customHats = CreateCustomHatDetails(hats);
                        foreach (var ch in customHats)
                            __instance.AllHats.Add(CreateHatBehaviour(ch));
                    }

                    while (CustomHatLoader.hatdetails.Count > 0)
                    {
                        __instance.AllHats.Add(CreateHatBehaviour(CustomHatLoader.hatdetails[0]));
                        CustomHatLoader.hatdetails.RemoveAt(0);
                    }
                }
                catch (Exception e)
                {
                    if (!LOADED)
                        System.Console.WriteLine("Unable to add Custom Hats\n" + e);
                }

                LOADED = true;
            }

            private static void Postfix()
            {
                RUNNING = false;
            }
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleAnimation))]
        private static class PlayerPhysicsHandleAnimationPatch
        {
            private static void Postfix(PlayerPhysics __instance)
            {
                var currentAnimation = __instance.Animator.GetCurrentAnimation();
                if (currentAnimation == __instance.ClimbAnim || currentAnimation == __instance.ClimbDownAnim) return;
                var hp = __instance.myPlayer.HatRenderer;
                if (hp.Hat == null) return;
                var extend = hp.Hat.GetHatExtension();
                if (extend == null) return;
                if (extend.FlipImage != null)
                    hp.FrontLayer.sprite = __instance.rend.flipX ? extend.FlipImage : hp.Hat.MainImage;

                if (extend.BackFlipImage == null) return;
                hp.BackLayer.sprite = __instance.rend.flipX ? extend.BackFlipImage : hp.Hat.BackImage;
            }
        }

        [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetHat), typeof(uint), typeof(int))]
        private static class HatParentSetHatPatch
        {
            private static void Postfix(HatParent __instance, [HarmonyArgument(1)] int color)
            {
                if (!DestroyableSingleton<TutorialManager>.InstanceExists) return;
                try
                {
                    var filePath = Path.GetDirectoryName(Application.dataPath) + @"\TheOtherHats\Test";
                    var d = new DirectoryInfo(filePath);
                    var filePaths = d.GetFiles("*.png").Select(x => x.FullName).ToArray(); // Getting Text files
                    var hats = CreateCustomHatDetails(filePaths, true);
                    if (hats.Count <= 0) return;
                    __instance.Hat = CreateHatBehaviour(hats[0], true, true);
                    __instance.SetHat(color);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("Unable to create test hat\n" + e);
                }
            }
        }

        [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.OnEnable))]
        public class HatsTabOnEnablePatch
        {
            private static readonly string innerslothPackageName = "Innersloth Hats";
            private static TMP_Text textTemplate;

            private static float CreateHatPackage(IReadOnlyList<Tuple<HatBehaviour, HatExtension>> hats,
                string packageName,
                float yStart, HatsTab __instance)
            {
                var isDefaultPackage = innerslothPackageName == packageName;
                var offset = yStart;

                if (textTemplate != null)
                {
                    var title = Object.Instantiate(textTemplate, __instance.scroller.Inner);
                    title.transform.localPosition = new Vector3(2.25f, yStart, -1f);
                    title.transform.localScale = Vector3.one * 1.5f;
                    // title.currentFontSize
                    title.fontSize *= 0.5f;
                    title.enableAutoSizing = false;
                    __instance.StartCoroutine(Effects.Lerp(0.1f,
                        new Action<float>(p => { title.SetText(packageName); })));
                    offset -= 0.8f * __instance.YOffset;
                    hatsTabCustomTexts.Add(title);
                }

                for (var i = 0; i < hats.Count; i++)
                {
                    var hat = hats[i].Item1;
                    var ext = hats[i].Item2;

                    var xPos = __instance.XRange.Lerp(i % __instance.NumPerRow / (__instance.NumPerRow - 1f));
                    var yPos = offset - i / __instance.NumPerRow * (isDefaultPackage ? 1f : 1.5f) * __instance.YOffset;
                    var colorChip = Object.Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
                    if (ext != null)
                    {
                        var background = colorChip.transform.FindChild("Background");
                        var foreground = colorChip.transform.FindChild("ForeGround");

                        if (background != null)
                        {
                            background.localScale = new Vector3(1, 1.5f, 1);
                            background.localPosition = Vector3.down * 0.243f;
                        }

                        if (foreground != null) foreground.localPosition = Vector3.down * 0.243f;

                        if (textTemplate != null)
                        {
                            var description = Object.Instantiate(textTemplate, colorChip.transform);
                            description.transform.localPosition = new Vector3(0f, -0.75f, -1f);
                            description.transform.localScale = Vector3.one * 0.7f;
                            __instance.StartCoroutine(Effects.Lerp(0.1f,
                                new Action<float>(p => { description.SetText($"{hat.name}\nby {ext.author}"); })));
                            hatsTabCustomTexts.Add(description);
                        }

                        if (!ext.IsUnlocked())
                        {
                            // Hat is locked
                            Object.Destroy(colorChip.Button);
                            var overlay = Object.Instantiate(colorChip.InUseForeground, colorChip.transform);
                            overlay.SetActive(true);
                        }
                    }

                    void SelectHatCallback()
                    {
                        __instance.SelectHat(hat);
                    }

                    colorChip.transform.localPosition = new Vector3(xPos, yPos, -1f);
                    colorChip.Button.OnClick.AddListener((UnityAction) SelectHatCallback);
                    colorChip.Inner.SetHat(hat, PlayerControl.LocalPlayer.Data.ColorId);
                    colorChip.Inner.transform.localPosition = hat.ChipOffset;
                    colorChip.Tag = hat;
                    __instance.ColorChips.Add(colorChip);
                }

                return offset - (hats.Count - 1) / __instance.NumPerRow * (isDefaultPackage ? 1f : 1.5f) *
                    __instance.YOffset - 0.85f;
            }

            public static bool Prefix(HatsTab __instance)
            {
                PlayerControl.SetPlayerMaterialColors(PlayerControl.LocalPlayer.Data.ColorId, __instance.DemoImage);
                __instance.HatImage.SetHat(SaveManager.LastHat, PlayerControl.LocalPlayer.Data.ColorId);
                PlayerControl.SetSkinImage(SaveManager.LastSkin, __instance.SkinImage);
                PlayerControl.SetPetImage(SaveManager.LastPet, PlayerControl.LocalPlayer.Data.ColorId,
                    __instance.PetImage);

                HatBehaviour[] unlockedHats = DestroyableSingleton<HatManager>.Instance.GetUnlockedHats();
                var packages = new Dictionary<string, List<Tuple<HatBehaviour, HatExtension>>>();
                hatsTabCustomTexts = new List<TMP_Text>();

                foreach (var hatBehaviour in unlockedHats)
                {
                    var ext = hatBehaviour.GetHatExtension();

                    if (ext != null)
                    {
                        if (!packages.ContainsKey(ext.package))
                            packages[ext.package] = new List<Tuple<HatBehaviour, HatExtension>>();
                        packages[ext.package].Add(new Tuple<HatBehaviour, HatExtension>(hatBehaviour, ext));
                    }
                    else
                    {
                        if (!packages.ContainsKey(innerslothPackageName))
                            packages[innerslothPackageName] = new List<Tuple<HatBehaviour, HatExtension>>();
                        packages[innerslothPackageName].Add(new Tuple<HatBehaviour, HatExtension>(hatBehaviour, null));
                    }
                }

                var yOffset = __instance.YStart;

                var hatButton = GameObject.Find("HatButton");

                if (hatButton != null && hatButton.transform.FindChild("ButtonText_TMP") != null)
                    textTemplate = hatButton.transform.FindChild("ButtonText_TMP").GetComponent<TMP_Text>();

                var orderedKeys = packages.Keys.OrderBy(x =>
                    x == innerslothPackageName ? 5000 : x == "Developer Hats" ? 0 : 500);
                foreach (var key in orderedKeys)
                {
                    var value = packages[key];
                    yOffset = CreateHatPackage(value, key, yOffset, __instance);
                }

                // __instance.scroller.YBounds.max = -(__instance.YStart - (float)(unlockedHats.Length / this.NumPerRow) * this.YOffset) - 3f;
                // __instance.scroller.YBounds.max = YOffset * -0.875f; // probably needs to fix up the entire messed math to solve this correctly
                __instance.scroller.YBounds.max = -(yOffset + 4.1f);
                return false;
            }
        }

        [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.Update))]
        public class HatsTabUpdatePatch
        {
            public static void Postfix()
            {
                // Manually hide all custom TMPro.TMP_Text objects that are outside the ScrollRect
                foreach (var customText in hatsTabCustomTexts)
                    if (customText != null && customText.transform != null && customText.gameObject != null)
                    {
                        var position = customText.transform.position;
                        var active = position.y is <= 3.75f and >= 0.3f;
                        var epsilon = Mathf.Min(Mathf.Abs(position.y - 3.75f),
                            Mathf.Abs(position.y - 0.35f));
                        if (active != customText.gameObject.active && epsilon > 0.1f)
                            customText.gameObject.SetActive(active);
                    }
            }
        }
    }

    public static class CustomHatExtensions
    {
        public static CustomHats.HatExtension GetHatExtension(this HatBehaviour hat)
        {
            if (CustomHats.testExt != null && CustomHats.testExt.condition.Equals(hat.name)) return CustomHats.testExt;
            CustomHats.CustomHatRegistry.TryGetValue(hat.name, out var ret);
            return ret;
        }
    }
}