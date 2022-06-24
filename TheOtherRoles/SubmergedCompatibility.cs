using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using TheOtherRoles.Patches;
using TheOtherRoles.Players;
using UnityEngine;
using Object = UnityEngine.Object;

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
        public static bool LoadedExternally { get; private set; }
        public static BasePlugin Plugin { get; private set; }
        public static Assembly Assembly { get; private set; }
        public static Type[] Types { get; private set; }
        public static Dictionary<string, Type> InjectedTypes { get; private set; }

        public static MonoBehaviour SubmarineStatus { get; private set; }

        public static bool IsSubmerged { get; private set; }
        
        public static bool DisableO2MaskCheckForEmergency
        {
            set
            {
                if (!Loaded) return;
                DisableO2MaskCheckField.SetValue(null, value);
            }
        }
        
        public static void SetupMap(ShipStatus map)
        {
            if (map == null)
            {
                IsSubmerged = false;
                SubmarineStatus = null;
                return;
            }
            
            IsSubmerged = map.Type == SubmergedCompatibility.SUBMERGED_MAP_TYPE;
            if (!IsSubmerged) return;
            
            SubmarineStatus = map.GetComponent(Il2CppType.From(SubmarineStatusType))?.TryCast(SubmarineStatusType) as MonoBehaviour;
        }

        private static Type SubmarineStatusType;
        private static MethodInfo CalculateLightRadiusMethod;
        
        private static Type TaskIsEmergencyPatchType;
        private static FieldInfo DisableO2MaskCheckField;

        private static MethodInfo RpcRequestChangeFloorMethod;
        private static Type FloorHandlerType;
        private static MethodInfo GetFloorHandlerMethod;

        private static Type Vent_MoveToVent_PatchType;
        private static FieldInfo InTransitionField;

        private static Type CustomTaskTypesType;
        private static FieldInfo RetrieveOxigenMaskField;
        public static TaskTypes RetrieveOxygenMask;
        private static Type SubmarineOxygenSystemType;
        private static MethodInfo SubmarineOxygenSystemInstanceField;
        private static MethodInfo RepairDamageMethod;

        public static bool TryLoadSubmerged()
        {
            try
            {
                TheOtherRolesPlugin.Logger.LogMessage("Trying to load Submerged...");
                var thisAsm = Assembly.GetCallingAssembly();
                var resourceName = thisAsm.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith("Submerged.dll"));
                if (resourceName == default) return false;

                using var submergedStream = thisAsm.GetManifestResourceStream(resourceName)!;
                byte[] assemblyBuffer = new byte[submergedStream.Length];
                submergedStream.Read(assemblyBuffer, 0, assemblyBuffer.Length);
                Assembly = Assembly.Load(assemblyBuffer);

                var pluginType = Assembly.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(BasePlugin)));
                Plugin = (BasePlugin) Activator.CreateInstance(pluginType!);
                Plugin.Load();

                Version = pluginType.GetCustomAttribute<BepInPlugin>().Version.BaseVersion();;

                IL2CPPChainloader.Instance.Plugins[SUBMERGED_GUID] = new();
                return true;
            }
            catch (Exception e)
            {
                TheOtherRolesPlugin.Logger.LogError(e);
            }
            return false;
        }
        

        public static void Initialize()
        {
            Loaded = IL2CPPChainloader.Instance.Plugins.TryGetValue(SUBMERGED_GUID, out PluginInfo plugin);
            if (!Loaded)
            {
                if (TryLoadSubmerged()) Loaded = true;
                else return;
            }
            else
            {
                LoadedExternally = true;
                Plugin = plugin!.Instance as BasePlugin;
                Version = plugin.Metadata.Version.BaseVersion();
                Assembly = Plugin!.GetType().Assembly;
            }

            CredentialsPatch.PingTrackerPatch.modStamp = new GameObject();
            Object.DontDestroyOnLoad(CredentialsPatch.PingTrackerPatch.modStamp);
            
            Types = AccessTools.GetTypesFromAssembly(Assembly);
            
            InjectedTypes = (Dictionary<string, Type>) AccessTools.PropertyGetter(Types.FirstOrDefault(t => t.Name == "RegisterInIl2CppAttribute"), "RegisteredTypes")
                .Invoke(null, Array.Empty<object>());
            
            SubmarineStatusType = Types.First(t => t.Name == "SubmarineStatus");
            CalculateLightRadiusMethod = AccessTools.Method(SubmarineStatusType, "CalculateLightRadius");
            
            TaskIsEmergencyPatchType = Types.First(t => t.Name == "PlayerTask_TaskIsEmergency_Patch");
            DisableO2MaskCheckField = AccessTools.Field(TaskIsEmergencyPatchType, "DisableO2MaskCheck");

            FloorHandlerType = Types.First(t => t.Name == "FloorHandler");
            GetFloorHandlerMethod = AccessTools.Method(FloorHandlerType, "GetFloorHandler", new Type[] {typeof(PlayerControl)});
            RpcRequestChangeFloorMethod = AccessTools.Method(FloorHandlerType, "RpcRequestChangeFloor");

            Vent_MoveToVent_PatchType = Types.First(t => t.Name == "Vent_MoveToVent_Patch");
            InTransitionField = AccessTools.Field(Vent_MoveToVent_PatchType, "InTransition");

            CustomTaskTypesType = Types.First(t => t.Name == "CustomTaskTypes");
            RetrieveOxigenMaskField = AccessTools.Field(CustomTaskTypesType, "RetrieveOxygenMask");
            RetrieveOxygenMask = (TaskTypes)RetrieveOxigenMaskField.GetValue(null);

            SubmarineOxygenSystemType = Types.First(t => t.Name == "SubmarineOxygenSystem" && t.Namespace == "Submerged.Systems.CustomSystems.Oxygen");
            SubmarineOxygenSystemInstanceField = AccessTools.PropertyGetter(SubmarineOxygenSystemType, "Instance");
            RepairDamageMethod = AccessTools.Method(SubmarineOxygenSystemType, "RepairDamage");
        }

        public static MonoBehaviour AddSubmergedComponent(this GameObject obj, string typeName)
        {
            if (!Loaded) return obj.AddComponent<MissingSubmergedBehaviour>();
            bool validType = InjectedTypes.TryGetValue(typeName, out Type type);
            return validType ? obj.AddComponent(Il2CppType.From(type)).TryCast<MonoBehaviour>() : obj.AddComponent<MissingSubmergedBehaviour>();
        }

        public static float GetSubmergedNeutralLightRadius(bool isImpostor) {
            if (!Loaded) return 0;
            return (float)CalculateLightRadiusMethod.Invoke(SubmarineStatus, new object[] { null, true, isImpostor });
        }


        public static void ChangeFloor(bool toUpper) {
            if (!Loaded) return;
            MonoBehaviour _floorHandler = ((Component)GetFloorHandlerMethod.Invoke(null, new object[] { CachedPlayer.LocalPlayer.PlayerControl })).TryCast(FloorHandlerType) as MonoBehaviour;
            RpcRequestChangeFloorMethod.Invoke(_floorHandler, new object[] { toUpper });
        }

        public static bool getInTransition() {
            if (!Loaded) return false;
            return (bool)InTransitionField.GetValue(null);
        }

        public static void RepairOxygen() {
            if (!Loaded) return;
            try {
                ShipStatus.Instance.RpcRepairSystem((SystemTypes)130, 64);
                RepairDamageMethod.Invoke(SubmarineOxygenSystemInstanceField.Invoke(null, Array.Empty<object>()), new object[] { CachedPlayer.LocalPlayer.PlayerControl, 64 });
            }
            catch (System.NullReferenceException) {
                TheOtherRolesPlugin.Logger.LogMessage("null reference in engineer oxygen fix");
            }

        }
    }

    public class MissingSubmergedBehaviour : MonoBehaviour
    {
        static MissingSubmergedBehaviour() => ClassInjector.RegisterTypeInIl2Cpp<MissingSubmergedBehaviour>();
        public MissingSubmergedBehaviour(IntPtr ptr) : base(ptr) { }
    }
}
