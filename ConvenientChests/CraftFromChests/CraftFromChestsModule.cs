using System;
using System.Collections.Generic;
using System.Linq;
using ConvenientChests.StackToNearbyChests;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ConvenientChests.CraftFromChests
{
    public class CraftFromChestsModule : Module
    {
        private GameMenu GameMenu { get; set; }
        private IList<Item> UserInventory { get; set; }

        private bool isCraftingScreen => GameMenu?.currentTab == GameMenu.craftingTab;
        private bool isCookingScreen => isCraftingScreen && GameMenu.GetType() == Type.GetType("CookingSkill.NewCraftingPage, CookingSkill");
        private static bool hasFridge => Utility.getHomeOfFarmer(Game1.player).upgradeLevel > 0;

        public CraftFromChestsModule(ModEntry modEntry) : base(modEntry)
        {
        }

        public override void activate()
        {
            GameMenuExtension.Shown += OnGameMenuShown;
            MenuEvents.MenuClosed += OnMenuClosed;
            GameMenuExtension.TabChanged += OnGameMenuTabChanged;
        }
        
        private void Hijack()
        {
            try
            {
                CombineInventories();

                var tabs = GameMenu.getTabs();
                var page = (StardewValley.Menus.CraftingPage) tabs[GameMenu.craftingTab];
                page.inventory.actualInventory = UserInventory;

                InputEvents.ButtonReleased += OnButtonEvent;
            }
            catch
            {
                Monitor.Log("Something went wrong! Consider disabling CraftFromChests.", LogLevel.Alert);
                Restore();
            }
        }

        private void Restore()
        {
            if (UserInventory == null)
                return;

            // remove cleanup handlers
            InputEvents.ButtonReleased -= OnButtonEvent;

            // clean one final time
            CleanupInventories();

            // restore
            Game1.player.Items = UserInventory;
            UserInventory = null;
        }

        private void CleanupInventories()
        {
            foreach (var c in getInventories())
                for (var index = 0; index < c.Count; index++)
                {
                    var i = c[index];
                    if (i?.Stack < 1)
                        c[index] = null;
                }
        }

        private void CombineInventories()
        {
            if (UserInventory != null)
                return;

            // store original inventory...
            UserInventory = Game1.player.Items.ToList();

            // ... and replace with combined inventory
            var list = new List<Item>();

            foreach (var c in getInventories())
                list.AddRange(c.Where(i => i != null));

            Game1.player.Items = list;
        }

        private IEnumerable<IList<Item>> getInventories()
        {
            yield return UserInventory;

            foreach (var c in Game1.player.getNearbyChests(Config.CraftRadius))
                yield return c.items;

            // always add fridge when on cooking screen
            if (isCookingScreen && hasFridge)
                yield return Utility.getHomeOfFarmer(Game1.player).fridge.Value.items;
        }
        private void OnGameMenuShown(object sender, EventArgs e) => GameMenu = Game1.activeClickableMenu as GameMenu;

        private void OnGameMenuTabChanged(object sender, EventArgs e)
        {
            if (isCraftingScreen)
                Hijack();

            else
                Restore();
        }

        private void OnMenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            if (GameMenu == null)
                return;

            Restore();
        }

        private void OnButtonEvent(object sender, EventArgsInput e) => CleanupInventories();
    }
}