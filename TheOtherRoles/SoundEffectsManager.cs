using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;
using Reactor.Utilities.Extensions;

namespace TheOtherRoles
{
    // Class to preload all audio/sound effects that are contained in the embedded resources.
    // The effects are made available through the soundEffects Dict / the get and the play methods.
    public static class SoundEffectsManager
        
    {
        private static Dictionary<string, AudioClip> soundEffects = new();

        public static void Load()
        {
            soundEffects = new Dictionary<string, AudioClip>();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();

            /* Old way of loading .raw files. Left here for reference -Gendelo
            foreach (string resourceName in resourceNames)
            {
                if (resourceName.Contains("TheOtherRoles.Resources.SoundEffects.") && (resourceName.Contains(".raw") || resourceName.Contains(".ogg")))
                {
                    soundEffects.Add(resourceName, Helpers.loadAudioClipFromResources(resourceName));
                }
            }*/

            var resourceBundle = assembly.GetManifestResourceStream("TheOtherRoles.Resources.SoundEffects.toraudio");
            var assetBundle = AssetBundle.LoadFromMemory(resourceBundle.ReadFully());
            foreach (var f in assetBundle.GetAllAssetNames()) {
                soundEffects.Add(f, assetBundle.LoadAsset<AudioClip>(f).DontUnload());
            }
            assetBundle.Unload(false);

        }

        public static AudioClip get(string path)
        {
            // Convenience: As as SoundEffects are stored in the same folder, allow using just the name as well
            //if (!path.Contains(".")) path = "TheOtherRoles.Resources.SoundEffects." + path + ".raw";
            path = "assets/audio/" + path.ToLower() + ".ogg";
            AudioClip returnValue;
            return soundEffects.TryGetValue(path, out returnValue) ? returnValue : null;
        }


        public static void play(string path, float volume=0.8f, bool loop = false)
        {
            if (!TORMapOptions.enableSoundEffects) return;
            AudioClip clipToPlay = get(path);
            stop(path);
            if (Constants.ShouldPlaySfx() && clipToPlay != null) {
                AudioSource source = SoundManager.Instance.PlaySound(clipToPlay, false, volume);
                source.loop = loop;
            }
        }
        public static void playAtPosition(string path, Vector2 position, float maxDuration = 15f, float range = 5f, bool loop = false) {
            if (!TORMapOptions.enableSoundEffects || !Constants.ShouldPlaySfx()) return;
            AudioClip clipToPlay = get(path);

            AudioSource source = SoundManager.Instance.PlaySound(clipToPlay, false, 1f);
            source.loop = loop;
            HudManager.Instance.StartCoroutine(Effects.Lerp(maxDuration, new Action<float>((p) => {
                if (source != null) {
                    if (p == 1) {
                        source.Stop();
                    }
                    float distance, volume;
                    distance = Vector2.Distance(position, PlayerControl.LocalPlayer.GetTruePosition());
                    if (distance < range)
                        volume = (1f - distance / range);
                    else
                        volume = 0f;
                    source.volume = volume;
                }
            })));
        }

        public static void stop(string path) {
            var soundToStop = get(path);
            if (soundToStop != null)
                if (Constants.ShouldPlaySfx()) SoundManager.Instance.StopSound(soundToStop);
        }

        public static void stopAll() {
            if (soundEffects == null) return;
            foreach (var path in soundEffects.Keys) stop(path);
        }
    }
}
