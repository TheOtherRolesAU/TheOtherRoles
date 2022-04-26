using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace TheOtherRoles
{
    public static class SubmergedCompatibility
    {
        public static class Classes
        {
            public const string ElevatorMover = "ElevatorMover";
        }
        
        public const string SUBMERGED_GUID = "Submerged";
        public const ShipStatus.MapType SUBMERGED_MAP_TYPE = (ShipStatus.MapType) 5;
        
        public static SemanticVersioning.Version Version { get; private set; }
        public static bool Loaded { get; private set; }
        public static BasePlugin Plugin { get; private set; }
        public static Assembly Assembly { get; private set; }
        public static Type[] Types { get; private set; }
        public static Dictionary<string, Type> InjectedTypes { get; private set; }

        private static MonoBehaviour _submarineStatus;
        public static MonoBehaviour SubmarineStatus
        {
            get
            {
                if (!Loaded) return null;
                
                if (_submarineStatus is null || _submarineStatus.WasCollected || !_submarineStatus || _submarineStatus == null)
                {
                    if (ShipStatus.Instance is null || ShipStatus.Instance.WasCollected || !ShipStatus.Instance || ShipStatus.Instance == null)
                    {
                        return _submarineStatus = null;
                    }
                    else
                    {
                        if (ShipStatus.Instance.Type == SUBMERGED_MAP_TYPE)
                        {
                            return _submarineStatus = ShipStatus.Instance.GetComponent(Il2CppType.From(SubmarineStatusType))?.TryCast(SubmarineStatusType) as MonoBehaviour;
                        }
                        else
                        {
                            return _submarineStatus = null;
                        }
                    }
                }
                else
                {
                    return _submarineStatus;
                }
            }
        }

        public static bool DisableO2MaskCheckForEmergency
        {
            set
            {
                if (!Loaded) return;
                DisableO2MaskCheckField.SetValue(null, value);
            }
        }

        private static Type SubmarineStatusType;
        private static MethodInfo CalculateLightRadiusMethod;
        
        private static Type TaskIsEmergencyPatchType;
        private static FieldInfo DisableO2MaskCheckField;
        
        public static void Initialize()
        {
            Loaded = IL2CPPChainloader.Instance.Plugins.TryGetValue(SUBMERGED_GUID, out PluginInfo plugin);
            if (!Loaded) return;
            
            Plugin = plugin!.Instance as BasePlugin;
            Version = plugin.Metadata.Version;

            Assembly = Plugin!.GetType().Assembly;
            Types = AccessTools.GetTypesFromAssembly(Assembly);
            
            InjectedTypes = (Dictionary<string, Type>) AccessTools.PropertyGetter(Types.FirstOrDefault(t => t.Name == "RegisterInIl2CppAttribute"), "RegisteredTypes")
                .Invoke(null, Array.Empty<object>());
            
            SubmarineStatusType = Types.First(t => t.Name == "SubmarineStatus");
            CalculateLightRadiusMethod = AccessTools.Method(SubmarineStatusType, "CalculateLightRadius");
            
            TaskIsEmergencyPatchType = Types.First(t => t.Namespace == "PlayerTask_TaskIsEmergency_Patch");
            DisableO2MaskCheckField = AccessTools.Field(TaskIsEmergencyPatchType, "DisableO2MaskCheck");
        }

        public static MonoBehaviour AddSubmergedComponent(this GameObject obj, string typeName)
        {
            if (!Loaded) return obj.AddComponent<MissingSubmergedBehaviour>();
            bool validType = InjectedTypes.TryGetValue(typeName, out Type type);
            return validType ? obj.AddComponent(Il2CppType.From(type)).TryCast<MonoBehaviour>() : obj.AddComponent<MissingSubmergedBehaviour>();
        }

        public static float GetSubmergedNeutralLightRadius()
        {
            if (!Loaded) return 0;
            return (float) CalculateLightRadiusMethod.Invoke(SubmarineStatus, new object[] {null, true});
        }
    }

    public class MissingSubmergedBehaviour : MonoBehaviour
    {
        static MissingSubmergedBehaviour() => ClassInjector.RegisterTypeInIl2Cpp<MissingSubmergedBehaviour>();
        public MissingSubmergedBehaviour(IntPtr ptr) : base(ptr) { }
    }
}