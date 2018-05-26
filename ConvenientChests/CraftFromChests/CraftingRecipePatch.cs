using System.Collections.Generic;
using Harmony;
using StardewValley;

namespace ConvenientChests.CraftFromChests {
    public class CraftingRecipePatch {
        internal static void Register(HarmonyInstance harmony) {
            // Fix ingredient consumption
            var original = AccessTools.Method(typeof(CraftingRecipe), "consumeIngredients");
            var target   = typeof(CraftingRecipePatch).GetMethod("ConsumeIngredients");
            harmony.Patch(original, new HarmonyMethod(target), null);
        }

        public static bool ConsumeIngredients(CraftingRecipe __instance, Dictionary<int, int> ___recipeList) {
            if (CraftFromChestsModule.NearbyItems == null)
                return true;
            
            __instance.ConsumeIngredients(CraftFromChestsModule.NearbyInventories);
            return false;
        }
    }
}