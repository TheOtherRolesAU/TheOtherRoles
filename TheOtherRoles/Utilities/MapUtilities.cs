using System.Collections.Generic;
using HarmonyLib;
using Il2CppSystem;

namespace TheOtherRoles.Utilities;

public static class MapUtilities
{
    public static ShipStatus CachedShipStatus;

    public static void MapDestroyed()
    {
        CachedShipStatus = null;
        _systems.Clear();
    }

    private static readonly Dictionary<SystemTypes, Object> _systems;
    public static Dictionary<SystemTypes, Object> Systems
    {
        get
        {
            if (_systems.Count == 0) GetSystems();
            return _systems;
        }
    }

    private static void GetSystems()
    {
        if (!CachedShipStatus) return;

        var systems = CachedShipStatus.Systems;
        if (systems.Count <= 0) return;
        
        foreach (Il2CppSystem.Collections.Generic.KeyValuePair<SystemTypes, ISystemType> keyValuePair in systems)
        {
            Systems[keyValuePair.Key] = keyValuePair.value.TryCast<Object>();
        }
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
public static class ShipStatus_Awake_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ShipStatus __instance)
    {
        MapUtilities.CachedShipStatus = __instance;
        SubmergedCompatibility.SetupMap(__instance);
    }
}
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnDestroy))]
public static class ShipStatus_OnDestroy_Patch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        MapUtilities.MapDestroyed();
        SubmergedCompatibility.SetupMap(null);
    }
}