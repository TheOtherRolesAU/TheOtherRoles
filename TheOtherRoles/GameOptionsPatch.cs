using HarmonyLib;
using System;
using System.IO;
using System.Net.Http;
using UnityEngine;
using static BonusRoles.BonusRoles;
using Reactor.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BonusRoles
{
    [HarmonyPriority(Priority.Low)] 
    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    class GameOptionsMenuUpdatePatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            __instance.GetComponentInParent<Scroller>().YBounds.max = -0.5F + __instance.Children.Length * 0.5F;
        }
    }
}