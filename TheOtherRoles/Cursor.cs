using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.IO;
using System.Collections.Generic;


namespace TheOtherRoles
{
    
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class MODCursor
    {
        public static Dictionary<string, Sprite> CachedSprites = new();
        private static void Prefix(MainMenuManager __instance)
        {
            Sprite sprite = LoadSprite("TheOtherRoles.Resources.Cursor.png");
            Cursor.SetCursor(sprite.texture, Vector2.zero, CursorMode.Auto);
        }
        
        public static Sprite? LoadSprite(string path, float pixelsPerUnit = 1f)
        {
            try
            {
                if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
                Texture2D texture = LoadTextureFromResources(path);
                sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
                sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
                return CachedSprites[path + pixelsPerUnit] = sprite;
            }
            catch
            {
                // ignored
            }

            return null;
        }
        public static Texture2D LoadTextureFromResources(string path)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using MemoryStream ms = new();
            stream.CopyTo(ms);
            ImageConversion.LoadImage(texture, ms.ToArray(), false);
            return texture;
        }
    }
}
