// Adapted from https://github.com/NuclearPowered/Reactor/blob/master/Reactor/Patches/ServerInfoFixes.cs 
// Orginial code was written by 6pak and miniduikboot as part of the Reactor api for Among Us (https://github.com/NuclearPowered/Reactor)

using HarmonyLib;
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
                var remoteEndPoint = new Il2CppSystem.Net.IPEndPoint(Il2CppSystem.Net.IPAddress.Parse(targetIp), (int)(targetPort - 3));
                __result = new Hazel.Udp.UnityUdpClientConnection(remoteEndPoint);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(InnerNet.InnerNetClient), nameof(InnerNet.InnerNetClient.SetEndpoint))]
	public static class InnerNetClientSetEndPointPatch
	{
		public static void Prefix(InnerNet.InnerNetClient __instance, ref string addr, ref ushort port) {
			ServerManager mgr = DestroyableSingleton<ServerManager>.Instance;
			if (string.Equals(addr, mgr.OnlineNetAddress, System.StringComparison.Ordinal) && __instance.GameMode == GameModes.OnlineGame) {
				port = mgr.OnlineNetPort;
			}
		}
	}
}
