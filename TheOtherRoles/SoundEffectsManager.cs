using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;

namespace TheOtherRoles
{
    // Class to preload all audio/sound effects that are contained in the embedded resources.
    // The effects are made available through the soundEffects Dict / the get and the play methods.
    public static class SoundEffectsManager
        
    {
        private static Dictionary<string, AudioClip> soundEffects;

        public static void Load()
        {
            soundEffects = new Dictionary<string, AudioClip>();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();
            foreach (string resourceName in resourceNames)
            {
                if (resourceName.Contains("TheOtherRoles.Resources.SoundEffects.") && resourceName.Contains(".raw"))
                {
                    soundEffects.Add(resourceName, Helpers.loadAudioClipFromResources(resourceName));
                }
            }
        }

        public static AudioClip get(string path)
        {
            // Convenience: As as SoundEffects are stored in the same folder, allow using just the name as well
            if (!path.Contains(".")) path = "TheOtherRoles.Resources.SoundEffects." + path + ".raw";
            AudioClip returnValue;
            return soundEffects.TryGetValue(path, out returnValue) ? returnValue : null;
        }


        public static void play(string path, float volume=0.8f)
        {
            if (!MapOptions.enableSoundEffects) return;
            AudioClip clipToPlay = get(path);
            // if (false) clipToPlay = get("exampleClip"); for april fools?
            stop(path);
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(clipToPlay, false, volume);
        }

        public static void stop(string path) {
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.StopSound(get(path));
        }

        public static void stopAll() {
            if (soundEffects == null) return;
            foreach (var path in soundEffects.Keys) stop(path);
        }
    }
}
