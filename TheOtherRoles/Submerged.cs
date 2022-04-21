using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.IL2CPP;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace TheOtherRoles
{
    public static class Submerged
    {
        public const string SUBMERGED_GUID = "Submerged";

        public static SemanticVersioning.Version Version { get; private set; }
        public static bool Loaded { get; private set; }
        public static bool Debug { get; private set; }
        public static BasePlugin Plugin { get; private set; }
        public static Assembly Assembly { get; private set; }
        public static Type[] Types { get; private set; }
        public static Dictionary<string, Type> InjectedTypes { get; private set; }
        
        public static void Initialize()
        {
            Loaded = IL2CPPChainloader.Instance.Plugins.TryGetValue(SUBMERGED_GUID, out var plugin);
            if (!Loaded) return;
            
            Plugin = plugin!.Instance as BasePlugin;
            Version = plugin.Metadata.Version;

            var pluginType = Plugin!.GetType()!;
            Debug = (bool) pluginType.GetProperty("IsDebugBuild", AccessTools.all)?.GetMethod.Invoke(null, Array.Empty<object>())!;
            
            Assembly = Plugin!.GetType().Assembly;
            Types = AccessTools.GetTypesFromAssembly(Assembly);
            
            InjectedTypes = (Dictionary<string, Type>) AccessTools.PropertyGetter(Types.FirstOrDefault(t => t.Name == "RegisterInIl2CppAttribute"), "RegisteredTypes")
                .Invoke(null, Array.Empty<object>());

        }

        public static MonoBehaviour AddSubmergedComponent(this GameObject obj, string typeName)
        {
            var validType = InjectedTypes.TryGetValue(typeName, out var type);
            return validType ? obj.AddComponent(Il2CppType.From(type)).TryCast<MonoBehaviour>() : obj.AddComponent<DummySubmergedBehaviour>();
        }
    }

    public class DummySubmergedBehaviour : MonoBehaviour
    {
        static DummySubmergedBehaviour() => ClassInjector.IsTypeRegisteredInIl2Cpp<DummySubmergedBehaviour>();
        public DummySubmergedBehaviour(IntPtr ptr) : base(ptr) { }
    }
}