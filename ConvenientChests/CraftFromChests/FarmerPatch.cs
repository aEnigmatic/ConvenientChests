using Harmony;

namespace ConvenientChests.CraftFromChests {
    public static class FarmerPatch {
        internal static void Register(HarmonyInstance harmony) {
            var original = AccessTools.Method(typeof(StardewValley.Farmer), "hasItemInInventory");
            var method   = typeof(FarmerPatch).GetMethod("HasItemInInventory");
            var postfix  = new HarmonyMethod(method);
            harmony.Patch(original, null, postfix);
        }

        public static bool HasItemInInventory(bool __result, StardewValley.Farmer __instance, int itemIndex, int quantity, int minPrice = 0) =>
            __result ||
            CraftFromChestsModule.NearbyItems != null && __instance.hasItemInList(CraftFromChestsModule.NearbyItems, itemIndex, quantity, minPrice);
    }
}