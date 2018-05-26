using System.Collections.Generic;
using System.Linq;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;


namespace ConvenientChests.CraftFromChests {
    public class RecipeTooltextReplacer {
        internal static void Register(HarmonyInstance harmony) {
            harmony.Patch(AccessTools.Method(typeof(CraftingRecipe), "drawRecipeDescription"),
                          new HarmonyMethod(typeof(RecipeTooltextReplacer).GetMethod("DrawRecipeDescription")), null);
        }

        public static List<Item> ActiveInventory { get; set; }

        public static bool DrawRecipeDescription(CraftingRecipe __instance, SpriteBatch b, Vector2 position, int width) {
            if (ActiveInventory == null)
                return true;

            b.Draw(Game1.staminaRect,
                   new Rectangle((int) (position.X + 8.0),
                                 (int) (position.Y + 32.0 + Game1.smallFont.MeasureString("Ing").Y) - 4 - 2, width - 32, 2),
                   Game1.textColor * 0.35f);

            Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.567"), Game1.smallFont,
                                       position + new Vector2(8f, 28f), Game1.textColor * 0.75f);

            var ingredients = __instance.GetIngredients();
            for (int index = 0; index < ingredients.Count; ++index) {
                var item   = ingredients.Keys.ElementAt(index);
                var amount = ingredients.Values.ElementAt(index);

                Color color = Game1.player.hasItemInList(ActiveInventory, item, amount, 8)
                                  ? Game1.textColor
                                  : Color.Red;

                var rect = Game1.getSourceRectForStandardTileSheet(
                                                                   Game1.objectSpriteSheet,
                                                                   __instance.getSpriteIndexFromRawIndex(item), 16,
                                                                   16);
                b.Draw(
                       Game1.objectSpriteSheet,
                       new Vector2(position.X, position.Y + 64f + index * 32 + index * 4),
                       rect,
                       Color.White, 0.0f,
                       Vector2.Zero, 2f,
                       SpriteEffects.None, 0.86f
                      );

                var w = Game1.tinyFont.MeasureString(string.Concat(amount)).X;
                Utility.drawTinyDigits(amount, b,
                                       new Vector2(position.X + 32f - w, position.Y + 64 + index * 32 + index * 4 + 21), 2f, 0.87f,
                                       Color.AntiqueWhite);

                Utility.drawTextWithShadow(b, __instance.getNameFromIndex(item), Game1.smallFont,
                                           new Vector2((float) (position.X + 32.0                          + 8.0),
                                                       (float) (position.Y + 64.0 + index * 32 + index * 4 + 4.0)), color);
            }

            b.Draw(Game1.staminaRect,
                   new Rectangle((int) position.X + 8, (int) position.Y + 64 + 4 + ingredients.Count * 36, width - 32, 2),
                   Game1.textColor * 0.35f);

            Utility.drawTextWithShadow(b, Game1.parseText(__instance.GetDescription(), Game1.smallFont, width - 8), Game1.smallFont,
                                       position + new Vector2(0.0f, 76 + ingredients.Count * 36), Game1.textColor * 0.75f);

            return false;
        }
    }
}