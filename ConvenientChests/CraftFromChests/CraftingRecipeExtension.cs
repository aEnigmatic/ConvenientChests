using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace ConvenientChests.CraftFromChests {
    public static class CraftingRecipeExtension {
        public static bool ConsumeIngredients(this CraftingRecipe recipe, List<IList<Item>> extraInventories) {
            foreach (var i in recipe.GetIngredients()) {
                var itemKey = i.Key;
                var count   = i.Value;

                // start with player inventory
                count = ConsumeFromInventory(Game1.player.Items, itemKey, count);
                if (count <= 0) goto NEXT_ITEM;

                // check extra inventories
                foreach (var extraInventory in extraInventories) {
                    count = ConsumeFromInventory(extraInventory, itemKey, count);
                    if (count <= 0) goto NEXT_ITEM;
                }

                if (count > 0)
                    ModEntry.Log($"\tOnly found {i.Value - count} / {count} of {i.Key}", LogLevel.Warn);

                NEXT_ITEM:;
            }

            return true;
        }

        private static int ConsumeFromInventory(IList<Item> items, int itemKey, int count) {
            // start from the end
            for (var index = items.Count - 1; index >= 0; --index) {
                var item = items[index];
                if (!MatchesItemKey(item, itemKey))
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

        private static bool MatchesItemKey(Item item, int itemKey) =>
            item != null          &&
            item is Object o      &&
            !o.bigCraftable &&
            (o.parentSheetIndex == itemKey || o.Category == itemKey);


        public static Dictionary<int, int> GetIngredients(this CraftingRecipe recipe)
            => ModEntry.StaticHelper.Reflection.GetField<Dictionary<int, int>>(recipe, "recipeList").GetValue();

        public static string GetDescription(this CraftingRecipe recipe)
            => ModEntry.StaticHelper.Reflection.GetField<string>(recipe, "description").GetValue();

        public static void DrawRecipeDescription(this CraftingRecipe r, List<Item> inventory, SpriteBatch b, Vector2 position, int width) {
            b.Draw(Game1.staminaRect,
                   new Rectangle((int) (position.X + 8.0),
                                 (int) (position.Y + 32.0 + Game1.smallFont.MeasureString("Ing").Y) - 4 - 2, width - 32, 2),
                   Game1.textColor * 0.35f);

            Utility.drawTextWithShadow(b,
                                       Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.567"),
                                       Game1.smallFont,
                                       position + new Vector2(8f, 28f),
                                       Game1.textColor * 0.75f);

            var ingredients = r.GetIngredients();
            for (int index = 0; index < ingredients.Count; ++index) {
                Color color = Game1.player.hasItemInList(inventory, ingredients.Keys.ElementAt(index), ingredients.Values.ElementAt(index), 8)
                                  ? Game1.textColor
                                  : Color.Red;

                b.Draw(Game1.objectSpriteSheet,
                       new Vector2(position.X, position.Y + 64f + index * 32 + index * 4),
                       Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet,
                                                               r.getSpriteIndexFromRawIndex(ingredients.Keys.ElementAt(index)), 16,
                                                               16), Color.White, 0.0f, Vector2.Zero, 2f, SpriteEffects.None, 0.86f);


                Utility.drawTinyDigits(ingredients.Values.ElementAt(index), b,
                                       new Vector2(
                                           position.X + 32f - Game1.tinyFont.MeasureString(string.Concat(ingredients.Values.ElementAt(index))).X,
                                           (float) (position.Y + 64.0 + index * 32 + index * 4 + 21.0)
                                       ),
                                       2f, 0.87f, Color.AntiqueWhite);

                Utility.drawTextWithShadow(b,
                                           r.getNameFromIndex(ingredients.Keys.ElementAt(index)),
                                           Game1.smallFont,
                                           new Vector2(position.X + 32 + 8, position.Y + 64 + index * 32 + (index * 4) + 4),
                                           color);
            }

            b.Draw(Game1.staminaRect, new Rectangle((int) position.X + 8, (int) position.Y + 64 + 4 + ingredients.Count * 36, width - 32, 2),
                   Game1.textColor * 0.35f);

            Utility.drawTextWithShadow(b,
                                       Game1.parseText(r.GetDescription(), Game1.smallFont, width - 8),
                                       Game1.smallFont,
                                       position + new Vector2(0.0f, 76 + ingredients.Count * 36),
                                       Game1.textColor * 0.75f);
        }


        public static string[] GetBuffsForCookingRecipe(this CraftingRecipe recipe) {
            var objInfo = Game1.objectInformation[recipe.createItem().parentSheetIndex];
            return objInfo.Split('/').Length < 8
                       ? null
                       : objInfo.Split('/')[7].Split(' ');
        }
    }
}