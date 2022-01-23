using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;

namespace TheOtherRoles
{
    // Class to automatically load all audio/sound effects that are contained in the embedded resources.
    // The effects are made available through the soundEffects Dict.
    public static class SoundEffectsHolder
        
    {
        private static Dictionary<string, AudioClip> soundEffects;

        static SoundEffectsHolder()
        {
            soundEffects = new Dictionary<string, AudioClip>();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();
            foreach (string resourceName in resourceNames)
            {
                if (resourceName.Contains("TheOtherRoles.Resources.SoundEffects.") && resourceName.Contains(".raw"))
                {
                    AudioClip tmpClip;
                    tmpClip = Helpers.loadAudioClipFromResources(resourceName);
                    soundEffects.Add(resourceName, tmpClip);
                }
            }
        }

        public static AudioClip get(String path)
        {
            // Convenience: As as SoundEffects are stored in the same folder, allow using just the name as well
            if (!path.Contains(".")) path = "TheOtherRoles.Resources.SoundEffects." + path + ".raw";
            AudioClip returnValue;
            return soundEffects.TryGetValue(path, out returnValue) ? returnValue : null;
        }
    }
}
