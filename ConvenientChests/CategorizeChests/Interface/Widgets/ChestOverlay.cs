using System;
using System.Linq;
using ConvenientChests.CategorizeChests.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace ConvenientChests.CategorizeChests.Interface.Widgets
{
    internal class ChestOverlay : Widget
    {
        private ItemGrabMenu ItemGrabMenu { get; }
        private CategorizeChestsModule Module { get; }
        private Chest Chest { get; }
        private ITooltipManager TooltipManager { get; }
        
        private readonly InventoryMenu InventoryMenu;
        private readonly InventoryMenu.highlightThisItem DefaultChestHighlighter;
        private readonly InventoryMenu.highlightThisItem DefaultInventoryHighlighter;


        private TextButton OpenButton;
        private TextButton StashButton;
        private CategoryMenu CategoryMenu;

        public ChestOverlay(CategorizeChestsModule module, Chest chest, ItemGrabMenu menu, ITooltipManager tooltipManager)
        {
            Module = module;
            Chest = chest;
            ItemGrabMenu = menu;
            InventoryMenu = menu.ItemsToGrabMenu;
            TooltipManager = tooltipManager;

            DefaultChestHighlighter = ItemGrabMenu.inventory.highlightMethod;
            DefaultInventoryHighlighter = InventoryMenu.highlightMethod;

            AddButtons();
            ControlEvents.ControllerButtonPressed += ControlEventsOnControllerButtonPressed;
        }


        protected override void OnParent(Widget parent)
        {
            base.OnParent(parent);

            if (parent == null) return;
            Width = parent.Width;
            Height = parent.Height;
        }

        private void AddButtons()
        {
            OpenButton = new TextButton("Categorize", Sprites.LeftProtrudingTab);
            OpenButton.OnPress += ToggleMenu;
            AddChild(OpenButton);

            StashButton = new TextButton(ChooseStashButtonLabel(), Sprites.LeftProtrudingTab);
            StashButton.OnPress += StashItems;
            AddChild(StashButton);

            PositionButtons();
        }

        private void PositionButtons()
        {
            StashButton.Width = OpenButton.Width = Math.Max(StashButton.Width, OpenButton.Width);

            OpenButton.Position = new Point(
                ItemGrabMenu.xPositionOnScreen + ItemGrabMenu.width / 2 - OpenButton.Width - 112 * Game1.pixelZoom,
                ItemGrabMenu.yPositionOnScreen + 22 * Game1.pixelZoom
            );

            StashButton.Position = new Point(
                OpenButton.Position.X + OpenButton.Width - StashButton.Width,
                OpenButton.Position.Y + OpenButton.Height
            );
        }

        private string ChooseStashButtonLabel()
        {
            var stashKey = Module.Config.StashKey;
            if (stashKey == Keys.None)
                return "Stash";

            var keyName = Enum.GetName(typeof(Keys), stashKey);
            return $"Stash ({keyName})";
        }

        private void ToggleMenu()
        {
            if (CategoryMenu == null)
                OpenCategoryMenu();

            else
                CloseCategoryMenu();
        }

        private void OpenCategoryMenu()
        {
            var chestData = Module.ChestDataManager.GetChestData(Chest);
            CategoryMenu = new CategoryMenu(chestData, Module.ItemDataManager, TooltipManager);
            CategoryMenu.Position = new Point(
                ItemGrabMenu.xPositionOnScreen + ItemGrabMenu.width / 2 - CategoryMenu.Width / 2 - 6 * Game1.pixelZoom,
                ItemGrabMenu.yPositionOnScreen - 10 * Game1.pixelZoom
            );
            CategoryMenu.OnClose += CloseCategoryMenu;
            AddChild(CategoryMenu);

            SetItemsClickable(false);
        }

        private void CloseCategoryMenu()
        {
            RemoveChild(CategoryMenu);
            CategoryMenu = null;

            SetItemsClickable(true);
        }

        private void StashItems()
        {
            ModEntry.staticMonitor.Log("Stash to this chest");

            if (!GoodTimeToStash())
                return;

            var acceptedItems = Game1.player.Items.Where(i => Module.ChestAcceptsItem(Chest, i));
            Chest.dumpItemsToChest(acceptedItems);
        }

        public override bool ReceiveKeyPress(Keys input)
        {
            if (input != Module.Config.StashKey)
                return PropagateKeyPress(input);

            StashItems();
            return true;
        }

        private void ControlEventsOnControllerButtonPressed(object sender, EventArgsControllerButtonPressed e)
        {
            if (!e.ButtonPressed.Equals(Module.Config.StashButton))
                return;

            StashItems();
        }


        public override bool ReceiveLeftClick(Point point)
        {
            var hit = PropagateLeftClick(point);

            if (!hit && CategoryMenu != null) // Are they clicking outside the menu to try to exit it?
                CloseCategoryMenu();

            return hit;
        }

        private void SetItemsClickable(bool clickable)
        {
            if (clickable)
            {
                ItemGrabMenu.inventory.highlightMethod = DefaultChestHighlighter;
                InventoryMenu.highlightMethod = DefaultInventoryHighlighter;
            }
            else
            {
                ItemGrabMenu.inventory.highlightMethod = item => false;
                InventoryMenu.highlightMethod = item => false;
            }
        }

        private bool GoodTimeToStash()
        {
            return ItemsAreClickable() && ItemGrabMenu.heldItem == null;
        }

        private bool ItemsAreClickable()
        {
            var items = ItemGrabMenu.inventory.actualInventory;
            var highlighter = ItemGrabMenu.inventory.highlightMethod;
            return items.Any(item => highlighter(item));
        }
    }
}