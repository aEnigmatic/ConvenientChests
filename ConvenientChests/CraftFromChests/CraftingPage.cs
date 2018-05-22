using System.Collections.Generic;
using System.Linq;
using ConvenientChests.CategorizeChests.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace ConvenientChests.CraftFromChests {
    public class CraftingPage : StardewValley.Menus.CraftingPage {
        public static IReflectionHelper Reflection => ModEntry.StaticHelper.Reflection;

        public List<IList<Item>> Inventories { get; }
        public List<Item>        Inventory   => Inventories.SelectMany(l => l.Where(i => i != null)).ToList();

        public bool Cooking {
            get => Reflection.GetField<bool>(this, "cooking").GetValue();
            set => Reflection.GetField<bool>(this, "cooking").SetValue(value);
        }

        public Item HeldItem {
            get => Reflection.GetField<Item>(this, "heldItem").GetValue();
            set => Reflection.GetField<Item>(this, "heldItem").SetValue(value);
        }

        public Item HoveredItem {
            get => Reflection.GetField<Item>(this, Cooking ? "lastCookingHover" : "hoverItem").GetValue();
            set => Reflection.GetField<Item>(this, Cooking ? "lastCookingHover" : "hoverItem").SetValue(value);
        }

        public string HoverTitle {
            get => Reflection.GetField<string>(this, "hoverTitle").GetValue();
            set => Reflection.GetField<string>(this, "hoverTitle").SetValue(value);
        }

        public string HoverText {
            get => Reflection.GetField<string>(this, "hoverTitle").GetValue();
            set => Reflection.GetField<string>(this, "hoverTitle").SetValue(value);
        }

        public CraftingRecipe HoveredRecipe {
            get => Reflection.GetField<CraftingRecipe>(this, "hoverRecipe").GetValue();
            set => Reflection.GetField<CraftingRecipe>(this, "hoverRecipe").SetValue(value);
        }

        public int CurrentPageIndex {
            get => Reflection.GetField<int>(this, "currentCraftingPage").GetValue();
            set => Reflection.GetField<int>(this, "currentCraftingPage").SetValue(value);
        }

        public List<Dictionary<ClickableTextureComponent, CraftingRecipe>> CraftingPages {
            get => Reflection.GetField<List<Dictionary<ClickableTextureComponent, CraftingRecipe>>>(this, "pagesOfCraftingRecipes").GetValue();
            set => Reflection.GetField<List<Dictionary<ClickableTextureComponent, CraftingRecipe>>>(this, "pagesOfCraftingRecipes").SetValue(value);
        }

        public Dictionary<ClickableTextureComponent, CraftingRecipe> CurrentPage => CraftingPages[CurrentPageIndex];


        public CraftingPage(int x, int y, int width, int height, bool cooking, List<IList<Item>> inventories) : base(x, y, width, height, cooking) {
            Cooking     = cooking;
            Inventories = inventories;
        }

        public CraftingPage(StardewValley.Menus.CraftingPage craftingPage, bool cooking, List<IList<Item>> inventories) : this(
                                                                                                                               craftingPage.xPositionOnScreen,
                                                                                                                               craftingPage.yPositionOnScreen,
                                                                                                                               craftingPage.width,
                                                                                                                               craftingPage.height,
                                                                                                                               cooking, inventories
                                                                                                                              ) { }

        private bool PlayerHasMaterials(CraftingRecipe craftingRecipe) => craftingRecipe.doesFarmerHaveIngredientsInInventory(Inventory);

        public override void receiveLeftClick(int x, int y, bool playSound = true) {
            if (!TryClick(x, y))
                base.receiveLeftClick(x, y, playSound);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true) {
            if (!TryClick(x, y))
                base.receiveRightClick(x, y, playSound);
        }

        private bool TryClick(int x, int y) {
            foreach (var clickItem in CurrentPage.Keys) {
                if (clickItem.hoverText.Equals("ghosted") || !clickItem.containsPoint(x, y))
                    continue;

                var num = Game1.oldKBState.IsKeyDown(Keys.LeftShift) ? 5 : 1;
                for (var index = 0; index < num; ++index)
                    if (PlayerHasMaterials(CurrentPage[clickItem]))
                        ClickCraftingRecipe(CurrentPage[clickItem], index == 0);

                return true;
            }

            return false;
        }

        protected virtual void ClickCraftingRecipe(CraftingRecipe recipe, bool playSound = true) {
            var newItem = recipe.createItem();

            // check if hand is empty
            if (HeldItem == null)
                HeldItem = newItem;

            // or item can be added to hand
            else if (HeldItem.canStackWith(newItem) && HeldItem.getRemainingStackSpace() <= recipe.numberProducedPerCraft)
                HeldItem.Stack += recipe.numberProducedPerCraft;

            // or stashed to inventory
            else if (Game1.player.couldInventoryAcceptThisItem(HeldItem)) {
                Game1.player.addItemToInventoryBool(HeldItem);
                HeldItem = newItem;
            }

            // otherwise abort
            else return;

            // craft item
            recipe.ConsumeIngredients(Inventories);

            if (playSound)
                Game1.playSound("coin");

            // item triggers
            Game1.player.checkForQuestComplete(null, -1, -1, newItem, null, 2);
            if (Cooking) {
                Game1.player.cookedRecipe(HeldItem.ParentSheetIndex);
                Game1.stats.checkForCookingAchievements();
            }
            else {
                Game1.player.craftingRecipes[recipe.name] += recipe.numberProducedPerCraft;
                Game1.stats.checkForCraftingAchievements();
            }

            // Handle gamepad
            if (!Game1.options.gamepadControls || !Game1.player.couldInventoryAcceptThisItem(HeldItem))
                return;

            Game1.player.addItemToInventoryBool(HeldItem);
            HeldItem = null;
        }

        public override void draw(SpriteBatch b) {
            if (Cooking)
                Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);

            drawHorizontalPartition(b, yPositionOnScreen + borderWidth + spaceToClearTopBorder + 256);

            inventory.draw(b);

            if (trashCan != null) {
                trashCan.draw(b);
                b.Draw(Game1.mouseCursors,
                       new Vector2(trashCan.bounds.X + 60, trashCan.bounds.Y + 40),
                       new Rectangle(686, 256, 18, 10), Color.White, trashCanLidRotation,
                       new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
            }

            b.End();
            DrawRecipeItems(b);
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            DrawHoverTooltip(b);

            HeldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);

            // base.draw(b);

            if (downButton != null && CurrentPageIndex < CraftingPages.Count - 1)
                downButton.draw(b);

            if (upButton != null && CurrentPageIndex > 0)
                upButton.draw(b);

            if (Cooking)
                drawMouse(b);

            DrawRecipeTooltip(b);
        }

        private void DrawRecipeItems(SpriteBatch b) {
            b.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null);

            foreach (var e in CurrentPage)
                if (e.Key.hoverText == "ghosted")
                    e.Key.draw(b, Color.Black * 0.35f, 0.89f);

                else if (!PlayerHasMaterials(e.Value))
                    e.Key.draw(b, Color.LightGray * 0.4f, 0.89f);

                else {
                    e.Key.draw(b);

                    if (e.Value.numberProducedPerCraft == 1)
                        continue;

                    NumberSprite.draw(e.Value.numberProducedPerCraft, b, new Vector2(e.Key.bounds.X + 62, e.Key.bounds.Y + 62),
                                      Color.AntiqueWhite, e.Key.scale / 8, 0.97f, 1, 0);
                }

            b.End();
        }

        protected virtual void DrawRecipeTooltip(SpriteBatch b) {
            if (HoveredRecipe == null)
                return;

            var offset = HeldItem != null ? 48 : 0;
            var buffs  = Cooking ? HoveredRecipe.GetBuffsForCookingRecipe() : null;

            // ugly tooltip workaround <_<
            var fridge = ChestExtension.GetFridge(Game1.player);
            if (fridge.items.Dirty)
                return;

            var items   = fridge.items.ToList();
            var cooking = Cooking;

            // set
            Cooking = true;
            fridge.items.Set(Inventory);
            fridge.items.MarkClean();

            // draw
            drawHoverText(b, " ", Game1.smallFont, offset, offset, -1, HoveredRecipe.DisplayName, -1, buffs, HoveredItem, 0, -1, -1, -1, -1, 1, HoveredRecipe);

            // revert
            Cooking = cooking;
            fridge.items.Set(items);
            fridge.items.MarkClean();
        }

        protected virtual void DrawHoverTooltip(SpriteBatch b) {
            if (HoveredItem != null)
                drawToolTip(b, HoverText, HoverTitle, HoveredItem, HeldItem != null);

            else if (!string.IsNullOrEmpty(HoverText))
                drawHoverText(b, HoverText, Game1.smallFont, HeldItem != null ? 64 : 0, HeldItem != null ? 64 : 0);
        }
    }
}