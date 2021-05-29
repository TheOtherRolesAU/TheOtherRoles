using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;

namespace TheOtherRoles
{
    [BepInPlugin(Id, "The Other Roles", VersionString)]
    [BepInProcess("Among Us.exe")]
    public class TheOtherRolesPlugin : BasePlugin
    {
        public const string Id = "me.eisbison.theotherroles";

        public Harmony Harmony { get; } = new Harmony(Id);
        public static TheOtherRolesPlugin Instance;


        public override void Load() {
			
        }
    }
}
