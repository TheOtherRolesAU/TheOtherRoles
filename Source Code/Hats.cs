using System;
using System.Collections.Generic;
using HarmonyLib;
using UnhollowerBaseLib;
using UnityEngine;
using System.Reflection;

namespace TheOtherRoles.Hats
{
    public class HatCreation
    {
        private static bool modded = false;


        protected internal struct HatData
        {
            public bool bounce;
            public string name;
            public bool highUp;
            public Vector2 offset;
            public string author;
        }

        private static List<HatData> _hatDatas = new List<HatData>()
        {
            new HatData {name = "corpse", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.2f), author="Idk"},
            new HatData {name = "bretman", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.2f), author="Idk"},
            new HatData {name = "poki", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.2f), author="Idk"},
            new HatData {name = "sykkunno", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.2f), author="Idk"},
            new HatData {name = "sykunno", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.2f), author="Idk"},
            new HatData {name = "toast", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.2f), author="MrFawkes1337"},

        };

        public static List<uint> TallIds = new List<uint>();

        protected internal static Dictionary<uint, HatData> IdToData = new Dictionary<uint, HatData>();

        private static HatBehaviour CreateHat(HatData hat, int id)
        {
            System.Console.WriteLine($"Creating Hat {hat.name}");
            var sprite = CreateSprite($"TheOtherRoles.Resources.Hats.{hat.name}.png", true);
            var newHat = ScriptableObject.CreateInstance<HatBehaviour>();
            newHat.MainImage = sprite;
            newHat.ProductId = hat.name;
            newHat.Order = 99 + id;
            newHat.InFront = true;
            newHat.NoBounce = true;

            return newHat;
        }
        public static Texture2D CreateEmptyTexture(int width = 0, int height = 0)
        {
            return new Texture2D(width, height, TextureFormat.RGBA32, Texture.GenerateAllMips, false, IntPtr.Zero);
        }
        public static Sprite CreateSprite(string name, bool hat = false)
        {
            var pixelsPerUnit = 225f;

            var assembly = Assembly.GetExecutingAssembly();
            Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            var imageStream = assembly.GetManifestResourceStream(name);
            var img = Helpers.ReadFully(imageStream);
            LoadImage(tex, img, true);
            tex = Helpers.DontDestroy(tex);
            var sprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.8f),
                    pixelsPerUnit
                );
            Helpers.DontDestroy(sprite);
            return sprite;
        }
        private delegate bool DLoadImage(IntPtr tex, IntPtr data, bool markNonReadable);

        private static DLoadImage _iCallLoadImage;
        private static void LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
        {
            _iCallLoadImage ??= IL2CPP.ResolveICall<DLoadImage>("UnityEngine.ImageConversion::LoadImage");
            var il2CPPArray = (Il2CppStructArray<byte>)data;
            _iCallLoadImage.Invoke(tex.Pointer, il2CPPArray.Pointer, markNonReadable);
        }
        private static IEnumerable<HatBehaviour> CreateAllHats()
        {

            var i = 0;
            foreach (var hat in _hatDatas)
            {
                yield return CreateHat(hat, ++i);
            }
        }

        [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
        public static class HatManagerPatch
        {
            static bool Prefix(HatManager __instance)
            {
                try
                {
                    if (!modded)
                    {
                        System.Console.WriteLine("Adding hats");
                        modded = true;
                        var id = 0;
                        foreach (var hatData in _hatDatas)
                        {
                            var hat = CreateHat(hatData, id++);
                            __instance.AllHats.Add(hat);
                            if (hatData.highUp)
                            {
                                TallIds.Add((uint)(__instance.AllHats.Count - 1));
                            }
                            IdToData.Add((uint)__instance.AllHats.Count - 1, hatData);
                        }
                    }
                    return true;
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("During Prefix, an exception occured");
                    System.Console.WriteLine("------------------------------------------------");
                    System.Console.WriteLine(e);
                    System.Console.WriteLine("------------------------------------------------");
                    throw;
                }
            }
        }



    }
}
