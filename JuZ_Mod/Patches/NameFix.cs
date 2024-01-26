using HarmonyLib;
using AmongUs.Data;
using AmongUs.Data.Legacy;

namespace TheOtherRoles.Patches {
    [Harmony]
    public class AccountManagerPatch {
        [HarmonyPatch(typeof(AccountManager), nameof(AccountManager.RandomizeName))]
        public static class RandomizeNamePatch {
            static bool Prefix(AccountManager __instance) {  
                if (LegacySaveManager.lastPlayerName == null)
                    return true;
                DataManager.Player.Customization.Name = LegacySaveManager.lastPlayerName;
		        __instance.accountTab.UpdateNameDisplay();
                return false; // Don't execute original
            }
        }
    }
}