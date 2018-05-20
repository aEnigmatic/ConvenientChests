using System;
using System.Collections.Generic;
using System.Linq;
using ConvenientChests.StackToNearbyChests;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace ConvenientChests.CraftFromChests
{
    public class CraftingPage : IClickableMenu
    {
//        private const int HowManyRecipesFitOnPage = 40;
        private const int NumInRow = 4;
        private const int NumInCol = 10;
        private const int RegionUpArrow = 88;
        private const int RegionDownArrow = 89;
        private const int RegionCraftingSelectionArea = 8000;
        private const int RegionCraftingModifier = 200;

        public readonly InventoryMenu Inventory;

        private string _hoverText = "";
        private string _hoverTitle = "";

        protected Item HeldItem;
        protected Item HoveredItem;
        protected CraftingRecipe HoveredRecipe;


        private List<Dictionary<ClickableTextureComponent, CraftingRecipe>> pages =
            new List<Dictionary<ClickableTextureComponent, CraftingRecipe>>();

        protected int PageIndex;
        protected Dictionary<ClickableTextureComponent, CraftingRecipe> currentPage => pages[PageIndex];

        private readonly ClickableTextureComponent _downButton;
        private readonly ClickableTextureComponent _upButton;
        private readonly ClickableTextureComponent _trashCan;
        private float _trashCanLidRotation;

        private List<CraftingRecipe> recipes { get; }

        public bool isCookingScreen { get; }

        public IEnumerable<Chest> nearbyChests { get; }

        public CraftingPage(IClickableMenu m) : this(m.xPositionOnScreen, m.yPositionOnScreen, m.width, m.height)
        {
        }

        public CraftingPage(int x, int y, int width, int height, bool isCookingScreen = false) : base(x, y, width, height)
        {
            this.isCookingScreen = isCookingScreen;
            Inventory = createInventoryMenu();
            _trashCan = createTrashCan(width, height);

            // recipes
            var recipeKeys = isCookingScreen
                ? CraftingRecipe.cookingRecipes.Keys.Select(s => s)
                : Game1.player.craftingRecipes.Keys;

            recipes = recipeKeys.Select(r => new CraftingRecipe(r, isCookingScreen)).ToList();
            nearbyChests = this.isCookingScreen
                ? new List<Chest> {StardewValley.Utility.getHomeOfFarmer(Game1.player).fridge.Value}
                : Game1.player.GetNearbyChests(ModEntry.Config.CraftRadius);

//            var playerRecipes = sourceRecipes.Select(x => x.)
//            if (!IsCooking)
//            {
//                foreach (string key in Game1.player.craftingRecipes.Keys)
//                    playerRecipes.Add(new string(key.ToCharArray()));
//            }
//            else
//            {
//                Game1.playSound("bigSelect");
//                foreach (string key in CraftingRecipe.IsCookingRecipes.Keys)
//                    playerRecipes.Add(new string(key.ToCharArray()));
//            }

            layoutRecipes(recipes);

            if (pages.Count <= 1)
                return;

            _upButton = createUpButton();
            _downButton = createDownButton();
        }

        private ClickableTextureComponent createDownButton()
        {
            return new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + 768 + 32, craftingPageY() + 192 + 32, 64, 64),
                Game1.mouseCursors,
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 11),
                0.8f)
            {
                myID = RegionDownArrow,
                upNeighborID = RegionUpArrow,
                rightNeighborID = 106
            };
        }


        private ClickableTextureComponent createUpButton()
        {
            return new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + 768 + 32, craftingPageY(), 64, 64),
                Game1.mouseCursors,
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 12),
                0.8f)
            {
                myID = RegionUpArrow,
                downNeighborID = RegionDownArrow,
                rightNeighborID = 106
            };
        }

        private ClickableTextureComponent createTrashCan(int w, int h)
        {
            return new ClickableTextureComponent(
                    new Rectangle(
                        xPositionOnScreen + w + 4,
                        yPositionOnScreen + h - 192 - 32 - borderWidth - 104,
                        64,
                        104
                    ),
                    Game1.mouseCursors,
                    new Rectangle(669, 261, 16, 26), 4f)
                {myID = 106};
        }

        private InventoryMenu createInventoryMenu()
        {
            return new InventoryMenu(
                    xPositionOnScreen + spaceToClearSideBorder + borderWidth,
                    craftingPageY() + 320,
                    false)
                {showGrayedOutSlots = true};
        }

        private int craftingPageY()
        {
            return yPositionOnScreen + spaceToClearTopBorder + borderWidth - 16;
        }

        private static ClickableTextureComponent[,] createNewPageLayout()
        {
            return new ClickableTextureComponent[NumInCol, NumInRow];
        }

        private Dictionary<ClickableTextureComponent, CraftingRecipe> createNewPage()
        {
            var dictionary = new Dictionary<ClickableTextureComponent, CraftingRecipe>();
            pages.Add(dictionary);
            return dictionary;
        }

        private bool IsSpaceOccupied(ClickableTextureComponent[,] pageLayout, int column, int row, CraftingRecipe recipe)
        {
            if (pageLayout[column, row] != null)
                return true;

            if (!recipe.bigCraftable)
                return false;

            if (row + 1 < 4)
                return pageLayout[column, row + 1] != null;

            return true;
        }

        private void layoutRecipes(List<CraftingRecipe> playerRecipes)
        {
            var i = 0;
            var col = 0;
            var row = 0;

            var page = createNewPage();
            var layout = createNewPageLayout();
            foreach (var recipe in playerRecipes)
            {
                // skip occupied slots
                while (IsSpaceOccupied(layout, col, row, recipe))
                {
                    // next col
                    ++col;
                    if (col < 10)
                        continue;

                    // new row
                    col = 0;
                    ++row;
                    if (row < 4)
                        continue;

                    // new page
                    page = createNewPage();
                    layout = createNewPageLayout();
                    col = 0;
                    row = 0;
                }

                // add entry
                var recipeEntry = createRecipeEntry(recipe, row, col);
                recipeEntry.myID = RegionCraftingModifier + i++;
                recipeEntry.leftNeighborID = recipeEntry.myID - 1;
                recipeEntry.rightNeighborID = recipeEntry.myID + 1;
                recipeEntry.upNeighborID = recipeEntry.myID - 10;
                recipeEntry.downNeighborID = recipeEntry.myID + 10;

                page.Add(recipeEntry, recipe);
                layout[col, row] = recipeEntry;

                if (recipe.bigCraftable)
                    layout[col, row + 1] = recipeEntry;
            }
        }

        private ClickableTextureComponent createRecipeEntry(CraftingRecipe recipe, int row, int col)
        {
            var left = xPositionOnScreen + spaceToClearSideBorder + borderWidth - 16;
            const int space = 8;

            var bounds = new Rectangle(
                left + col * (64 + space),
                craftingPageY() + row * 72,
                64,
                recipe.bigCraftable ? 128 : 64
            );

            var spriteSheet = recipe.bigCraftable
                ? Game1.bigCraftableSpriteSheet
                : Game1.objectSpriteSheet;

            var sourceRect = recipe.bigCraftable
                ? Game1.getArbitrarySourceRect(Game1.bigCraftableSpriteSheet, 16, 32, recipe.getIndexOfMenuView())
                : Game1.getArbitrarySourceRect(Game1.objectSpriteSheet, 16, 16, recipe.getIndexOfMenuView());

            return new ClickableTextureComponent("",
                bounds,
                null,
                !isCookingScreen || Game1.player.cookingRecipes.ContainsKey(recipe.name) ? "" : "ghosted",
                spriteSheet,
                sourceRect,
                4f
            );
        }

        protected override void noSnappedComponentFound(int direction, int oldRegion, int oldID)
        {
            base.noSnappedComponentFound(direction, oldRegion, oldID);
            if (oldRegion != RegionCraftingSelectionArea || direction != 2)
                return;
            currentlySnappedComponent = getComponentWithID(oldID % 10);
            currentlySnappedComponent.upNeighborID = oldID;
        }

        public override void snapToDefaultClickableComponent()
        {
            currentlySnappedComponent = PageIndex < pages.Count
                ? currentPage.First().Key
                : null;
            
            snapCursorToCurrentSnappedComponent();
        }

        protected override void actionOnRegionChange(int oldRegion, int newRegion)
        {
            base.actionOnRegionChange(oldRegion, newRegion);
            if (newRegion != 9000 || oldRegion == 0)
                return;
            for (var index = 0; index < 10; ++index)
                if (Inventory.inventory.Count > index)
                    Inventory.inventory[index].upNeighborID = currentlySnappedComponent.upNeighborID;
        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
            if (!key.Equals(Keys.Delete) || HeldItem == null || !HeldItem.canBeTrashed())
                return;

            if (HeldItem is Object o && Game1.player.specialItems.Contains(o.ParentSheetIndex))
                Game1.player.specialItems.Remove(o.ParentSheetIndex);

            HeldItem = null;
            Game1.playSound("trashcan");
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);

            if (direction == 0)
                return;

            if (direction > 0 && PageIndex < 1)
                return;

            if (direction < 0 && PageIndex >= pages.Count - 1)
                return;

            PageIndex -= direction;
            Game1.playSound("shwip");
            if (!Game1.options.SnappyMenus)
                return;

            setCurrentlySnappedComponentTo(direction > 0 ? RegionUpArrow : RegionDownArrow);
            snapCursorToCurrentSnappedComponent();

            var lastID = currentPage.Last().Key.myID;
            _downButton.leftNeighborID = lastID;
            _upButton.leftNeighborID = lastID;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            // inventory??
            HeldItem = Inventory.leftClick(x, y, HeldItem);

            // navigation
            if (_upButton != null && _upButton.containsPoint(x, y) && PageIndex > 0)
            {
                Game1.playSound("coin");
                PageIndex = Math.Max(0, PageIndex - 1);
                _upButton.scale = _upButton.baseScale;
                _upButton.leftNeighborID = currentPage.Last().Key.myID;
            }

            if (_downButton != null && _downButton.containsPoint(x, y) && PageIndex < pages.Count - 1)
            {
                Game1.playSound("coin");
                PageIndex = Math.Min(pages.Count - 1, PageIndex + 1);
                _downButton.scale = _downButton.baseScale;
                _downButton.leftNeighborID = currentPage.Last().Key.myID;
            }

            // handle recipe clicks
            foreach (var clickItem in currentPage.Keys)
            {
                var num = Game1.oldKBState.IsKeyDown(Keys.LeftShift) ? 5 : 1;
                for (var index = 0; index < num; ++index)
                    if (clickItem.containsPoint(x, y) && !clickItem.hoverText.Equals("ghosted") && playerHasMaterials(currentPage[clickItem]))
                        clickCraftingRecipe(currentPage[clickItem], index == 0);
            }

            // handle thrashing / throwing away
            if (_trashCan != null && _trashCan.containsPoint(x, y) && HeldItem != null && HeldItem.canBeTrashed())
            {
                if (HeldItem is Object o && Game1.player.specialItems.Contains(o.ParentSheetIndex))
                    Game1.player.specialItems.Remove(o.ParentSheetIndex);

                HeldItem = null;
                Game1.playSound("trashcan");
            }
            else
            {
                if (HeldItem == null || isWithinBounds(x, y) || !HeldItem.canBeTrashed())
                    return;

                Game1.playSound("throwDownITem");
                Game1.createItemDebris(HeldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
                HeldItem = null;
            }
        }

        public virtual bool playerHasMaterials(CraftingRecipe craftingRecipe) =>
            craftingRecipe.doesFarmerHaveIngredientsInInventory(nearbyChests.SelectMany(c => c.items).ToList());

        protected virtual void clickCraftingRecipe(CraftingRecipe recipe, bool playSound = true)
        {
            var newItem = recipe.createItem();
            // check if hand is empty or can merge with hand
            if (HeldItem != null && !HeldItem.canStackWith(newItem) &&
                HeldItem.getRemainingStackSpace() < recipe.numberProducedPerCraft)
            {
                // otherwise, try to stash held item
                if (!Game1.player.couldInventoryAcceptThisItem(HeldItem))
                    return;

                Game1.player.addItemToInventoryBool(HeldItem);
                HeldItem = null;
            }

            recipe.consumeIngredients(nearbyChests.Select(c => c.items.ToList()).ToList());

            if (HeldItem == null)
                HeldItem = newItem;

            else
                HeldItem.Stack += newItem.Stack;

            if (playSound)
                Game1.playSound("coin");

            // item triggers
            Game1.player.checkForQuestComplete(null, -1, -1, newItem, null, 2);
            if (isCookingScreen)
            {
                Game1.player.cookedRecipe(HeldItem.ParentSheetIndex);
                Game1.stats.checkForCookingAchievements();
            }
            else
            {
                Game1.player.craftingRecipes[recipe.name] += recipe.numberProducedPerCraft;
                Game1.stats.checkForCraftingAchievements();
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            HeldItem = Inventory.rightClick(x, y, HeldItem);
            foreach (var key in currentPage.Keys)
            {
                if (!key.containsPoint(x, y) || key.hoverText.Equals("ghosted"))
                    continue;

                var recipe = currentPage[key];
                clickCraftingRecipe(recipe);
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            HoveredRecipe = null;

            // inventory
            HoveredItem = Inventory.hover(x, y, HoveredItem);
            if (HoveredItem != null)
            {
                _hoverTitle = Inventory.hoverTitle;
                _hoverText = Inventory.hoverText;
                return;
            }

            // recipes
            foreach (var key in currentPage.Keys)
                // shrink element again
                if (!key.containsPoint(x, y))
                {
                    key.scale = Math.Max(key.scale - 0.02f, key.baseScale);
                }

                // hover element
                else
                {
                    if (key.hoverText.Equals("ghosted"))
                        _hoverText = "???";

                    else
                        HoveredRecipe = currentPage[key];

                    key.scale = Math.Min(key.scale + 0.02f, key.baseScale + 0.1f);
                }

            if (_upButton != null)
                _upButton.scale = _upButton.containsPoint(x, y)
                    ? Math.Min(_upButton.scale + 0.02f, _upButton.baseScale + 0.1f)
                    : Math.Max(_upButton.scale - 0.02f, _upButton.baseScale);

            if (_downButton != null)
                _downButton.scale = _downButton.containsPoint(x, y)
                    ? Math.Min(_downButton.scale + 0.02f, _downButton.baseScale + 0.1f)
                    : Math.Max(_downButton.scale - 0.02f, _downButton.baseScale);

            if (_trashCan == null)
                return;

            if (_trashCan.containsPoint(x, y))
            {
                if (_trashCanLidRotation <= 0.0)
                    Game1.playSound("trashcanlid");

                _trashCanLidRotation = Math.Min(_trashCanLidRotation + (float) Math.PI / 48f, 1.570796f);
            }
            else
            {
                _trashCanLidRotation = Math.Max(_trashCanLidRotation - (float) Math.PI / 48f, 0.0f);
            }
        }

        public override bool readyToClose()
        {
            return HeldItem == null;
        }

        public override void draw(SpriteBatch b)
        {
            if (isCookingScreen)
                Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);

            drawHorizontalPartition(b, craftingPageY() + 240);

            Inventory.draw(b);

            if (_trashCan != null)
            {
                _trashCan.draw(b);
                b.Draw(Game1.mouseCursors,
                    new Vector2(_trashCan.bounds.X + 60, _trashCan.bounds.Y + 40),
                    new Rectangle(686, 256, 18, 10), Color.White, _trashCanLidRotation,
                    new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
            }

            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp,
                null, null);

            foreach (var e in currentPage)
                if (e.Key.hoverText.Equals("ghosted"))
                    e.Key.draw(b, Color.Black * 0.35f, 0.89f);
                
                else if (!playerHasMaterials(e.Value))
                    e.Key.draw(b, Color.LightGray * 0.4f, 0.89f);
                
                else
                {
                    e.Key.draw(b);

                    if (e.Value.numberProducedPerCraft > 1)
                        NumberSprite.draw(e.Value.numberProducedPerCraft, b,
                            new Vector2(e.Key.bounds.X + 64 - 2, e.Key.bounds.Y + 64 - 2),
                            Color.Red, (float) (0.5 * (e.Key.scale / 4.0)), 0.97f, 1f, 0);
                }

            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            if (HoveredItem != null)
                drawToolTip(b, _hoverText, _hoverTitle, HoveredItem, HeldItem != null);

            else if (!string.IsNullOrEmpty(_hoverText))
                drawHoverText(b, _hoverText, Game1.smallFont, HeldItem != null ? 64 : 0, HeldItem != null ? 64 : 0);

            HeldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);

            base.draw(b);

            if (_downButton != null && PageIndex < pages.Count - 1)
                _downButton.draw(b);

            if (_upButton != null && PageIndex > 0)
                _upButton.draw(b);


            if (isCookingScreen)
                drawMouse(b);

            if (HoveredRecipe == null)
                return;

            // DrawRecipeTooltip
            var offset = HeldItem != null ? 48 : 0;
            var buffs = getBuffsForCookingRecipe(HoveredRecipe);
            
            drawHoverText(b, " ", Game1.smallFont, offset, offset, -1, HoveredRecipe.DisplayName, -1, buffs, HoveredItem, 0, -1, -1, -1, -1, 1, HoveredRecipe);
        }

        private string[] getBuffsForCookingRecipe(CraftingRecipe recipe)
        {
            if (!isCookingScreen || HoveredRecipe == null)
                return null;

            var objInfo = Game1.objectInformation[recipe.createItem().ParentSheetIndex];
            return objInfo.Split('/').Length < 8
                ? null
                : objInfo.Split('/')[7].Split(' ');
        }
    }
}