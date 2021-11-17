using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(InnerNet.InnerNetClient), nameof(InnerNet.InnerNetClient.GetConnectionData))]
    public static class InnerNetClientGetConnectionDataPatch {
        public static void Prefix(ref bool useDtlsLayout)
        {
            var serverManager = ServerManager.Instance;
            DnsRegionInfo region = serverManager.CurrentRegion.TryCast<DnsRegionInfo>();
            if (region == null || !region.Fqdn.EndsWith("among.us"))
                useDtlsLayout = false;
        }
    }

    [HarmonyPatch(typeof(AuthManager), nameof(AuthManager.CreateDtlsConnection))]
    public static class AuthManagerCreateDtlsConnection {
        public static bool Prefix(ref Hazel.Udp.UnityUdpClientConnection __result, string targetIp, ushort targetPort) {
            var serverManager = ServerManager.Instance;
            DnsRegionInfo region = serverManager.CurrentRegion.TryCast<DnsRegionInfo>();
            if (region == null || !region.Fqdn.EndsWith("among.us")) {
                var remoteEndPoint = new Il2CppSystem.Net.IPEndPoint(Il2CppSystem.Net.IPAddress.Parse(targetIp), targetPort);
                __result = new Hazel.Udp.UnityUdpClientConnection(remoteEndPoint);
                return false;
            }
            return true;
        }
    }
}
