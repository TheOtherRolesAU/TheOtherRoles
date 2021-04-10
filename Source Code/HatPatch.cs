using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnhollowerBaseLib;

namespace TheOtherRoles
{
    public class HatPatch
    {
        static bool modded = false;
        [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
        public static class HatManagerHatsPatch
        {
            internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
            internal static d_LoadImage iCall_LoadImage;


            public static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
            {
                if (iCall_LoadImage == null)
                    iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");

                var il2cppArray = (Il2CppStructArray<byte>)data;

                return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
            }

            private static HatBehaviour CreateHat(Stream texture, string id)
            {
                System.Console.WriteLine($"Creating Hat: {id}");
                HatBehaviour newHat = new HatBehaviour();
                Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                float pixelsPerUnit = 225f;

                byte[] hatTexture = new byte[texture.Length];
                texture.Read(hatTexture, 0, (int)texture.Length);
                LoadImage(tex, hatTexture, false);
                newHat.MainImage = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.8f),
                    pixelsPerUnit
                );
                newHat.ProductId = $"+{id}";
                newHat.InFront = true;
                newHat.NoBounce = true;

                return newHat;
            }

            private static HatBehaviour CreateFilesystemHat(string filePath)
            {
                try
                {
                    using (var hatFileStream = new StreamReader(filePath))
                    {
                        return CreateHat(hatFileStream.BaseStream, Path.GetFileNameWithoutExtension(filePath));
                    }
                }
                catch (IOException e)
                {
                    throw e;
                }

            }

            public static IEnumerable<HatBehaviour> CreateFilesystemHats()
            {
                var hatPath = Path.Combine(Directory.GetCurrentDirectory(), "CustomHats");

                System.Console.WriteLine($"Looking for hats in path: {hatPath}");
                Directory.CreateDirectory(hatPath);
                var hatFileNames = Directory.GetFiles(hatPath, "*.hat.png");
                if (hatFileNames.Count() == 0)
                {
                    foreach (var name in Directory.GetFiles(hatPath))
                    {
                    }
                }
                return hatFileNames.Select(CreateFilesystemHat);

            }

            static void Finalizer(Exception __exception)
            {
            }

            static void Postfix()
            {
            }

            public static bool Prefix(HatManager __instance)
            {
                try
                {
                    if (!modded)
                    {
                        System.Console.WriteLine("Adding Hats from System");
                        modded = true;
                        var hatsFromFilesystem = CreateFilesystemHats();
                        foreach (var hat in hatsFromFilesystem)
                        {
                            if (__instance.AllHats.Contains(hat))
                            {
                                __instance.AllHats.Remove(hat);
                                System.Console.WriteLine("Hats reloaded");
                            }
                            __instance.AllHats.Add(hat);
                        }

                        // 
                        __instance.AllHats.Sort((Il2CppSystem.Comparison<HatBehaviour>)((h1, h2) => h2.ProductId.CompareTo(h1.ProductId)));

                    }
                    else
                    {
                    }
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}
