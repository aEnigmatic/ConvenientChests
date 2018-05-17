using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;

namespace ConvenientChests.CraftFromChests
{
    public static class CraftingRecipeExtension
    {
        public static bool consumeIngredients(this CraftingRecipe recipe, List<List<Item>> extraInventories)
        {
            foreach (var i in recipe.getIngredients())
            {
                var itemKey = i.Key;
                var count = i.Value;

                // start with player inventory
                count = consumeFromInventory(Game1.player.Items, itemKey, count);
                if (count <= 0) goto NEXT_ITEM;

                // check extra inventories
                foreach (var extraInventory in extraInventories)
                {
                    count = consumeFromInventory(extraInventory, itemKey, count);
                    if (count <= 0) goto NEXT_ITEM;
                }

                if (count > 0)
                    ModEntry.staticMonitor.Log($"\tOnly found {i.Value - count} / {count} of {i.Key}", LogLevel.Warn);

                NEXT_ITEM:;
            }

            return true;
        }

        private static int consumeFromInventory(IList<Item> items, int itemKey, int count)
        {
            // start from the end
            for (var index = items.Count - 1; index >= 0; --index)
            {
                var item = items[index];
                if (!matchesItemKey(item, itemKey))
                    continue;

                var actualValue = item.Stack;

                if (item.Stack > count)
                    item.Stack -= count;

                else
                    items[index] = null;

                count -= actualValue;

                if (count <= 0)
                    return 0;
            }

            return count;
        }

        private static bool matchesItemKey(Item item, int itemKey) =>
            item != null &&
            item is Object o &&
            !o.bigCraftable.Value &&
            (o.ParentSheetIndex == itemKey || o.Category == itemKey);


        public static Dictionary<int, int> getIngredients(this CraftingRecipe recipe) =>
            ModEntry.staticHelper.Reflection.GetField<Dictionary<int, int>>(recipe, "recipeList").GetValue();
    }
}