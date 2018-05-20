using System;
using System.Collections.Generic;
using System.Linq;
using ConvenientChests.CategorizeChests;
using ConvenientChests.StackToNearbyChests;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using Utility = StardewValley.Utility;

namespace ConvenientChests.CraftFromChests {
    public class CraftFromChestsModule : Module {
        private GameMenu           GameMenu                { get; set; }
        private int                UserSlots               { get; set; }
        private int                UserSlotsEmpty          { get; set; }
        private IList<Item>        UserInventory           { get; set; }
        private IList<IList<Item>> CraftInventories        { get; set; }
        private List<List<Item>>   CraftInventoriesInitial { get; set; }

        private static bool IsCraftingScreen => Game1.activeClickableMenu is GameMenu m && m.currentTab == GameMenu.craftingTab;
        private static bool IsCookingScreen  => Game1.activeClickableMenu is StardewValley.Menus.CraftingPage;
        private static bool HasFridge        => Utility.getHomeOfFarmer(Game1.MasterPlayer).upgradeLevel > 0;

        public CraftFromChestsModule(ModEntry modEntry) : base(modEntry) { }

        public override void activate() {
            MenuListener.RegisterEvents();

            MenuListener.CraftingMenuShown  += (sender, e) => Hijack();
            MenuListener.CraftingMenuClosed += (sender, e) => Restore();
        }

        private void Hijack() {
            var nearbyChests = Game1.player.GetNearbyChests(Config.CraftRadius).Where(c => c.items.Any(i => i != null)).ToList();
            if (!nearbyChests.Any())
                return;

            try {
                CombineInventories(nearbyChests);
                ReplaceCraftingInventory();

                GameEvents.HalfSecondTick += UpdateEvent;
            }
            catch (Exception e) {
                Monitor.Log("Something went wrong! Consider disabling CraftFromChests:", LogLevel.Alert);
                Monitor.Log(e.ToString(),                                                LogLevel.Error);
                Restore();
            }
        }

        private void ReplaceCraftingInventory() {
            switch (Game1.activeClickableMenu) {
                case GameMenu gameMenu:
                    var tabs = gameMenu.GetTabs(modEntry.Helper.Reflection);
                    var page = (StardewValley.Menus.CraftingPage) tabs[GameMenu.craftingTab];
                    page.inventory.showGrayedOutSlots = true;
                    page.inventory.actualInventory    = UserInventory;
                    break;

                case StardewValley.Menus.CraftingPage p:
                    p.inventory.showGrayedOutSlots = false;
                    p.inventory.actualInventory    = UserInventory;
                    break;

                default:
                    // How did we get here?
                    throw new Exception($"Unexpected menu: {Game1.activeClickableMenu?.GetType().ToString() ?? "null"}");
            }
        }

        private void Restore() {
            if (UserInventory == null)
                return;

            // remove cleanup handlers
            GameEvents.HalfSecondTick -= UpdateEvent;

            // clean one final time
            CleanupInventories();

            // restore inventory
            Game1.player.Items    = UserInventory;
            Game1.player.MaxItems = UserSlots;
            UserSlots             = 0;
            UserSlotsEmpty        = 0;
            UserInventory         = null;
            CraftInventories      = null;
        }

        private void CleanupInventories() {
            if (UserInventory == null || CraftInventories == null)
                return;

            // check for used up items
            foreach (var c in CraftInventories)
                for (var i = 0; i < c.Count; i++)
                    if (c[i]?.Stack < 1)
                        c[i] = null;

            // check for new items
            var emptySlots = UserSlotsEmpty;
            for (var i = 1; i <= emptySlots; i++) {
                var item = Game1.player.Items[Game1.player.MaxItems - i];
                if (item == null) continue;

                var index = UserInventory.IndexOf(null);
                if (index == -1)
                    // No more empty slots
                    // -> add to hand
                    return;

                // add a copy to user inventory
                UserInventory[index] = item;
                // and reduce empty slots for next check
                UserSlotsEmpty--;
            }

            // check for changed items
            if (!Config.CraftToChests)
                for (int i = 1; i < CraftInventories.Count; i++) {
                    var invNew = CraftInventories[i];
                    var invOld = CraftInventoriesInitial[i];

                    for (int j = 0; j < invNew.Count; j++) {
                        var iOld = invOld[j];
                        var iNew = invNew[j];

                        if (iNew == null)
                            continue;

                        if (iOld == null) {
                            invOld[j] = iNew.GetCopy();
                            continue;
                        }

                        if (iNew.Stack == iOld.Stack)
                            continue;

                        if (iNew.Stack < iOld.Stack) {
                            // Crafted from stack
                            // modEntry.Monitor.Log($"Stack decreased: {iOld.Name} {iOld.Stack} -> {iNew.Stack}");
                            iOld.Stack = iNew.Stack;
                        }
                        else {
                            // Crafted TO stack
                            // modEntry.Monitor.Log($"Stack increased: {iOld.Name} {iOld.Stack} -> {iNew.Stack}");

                            var o = iNew.GetCopy();
                            o.Stack -= iOld.Stack;

                            var item  = UserInventory.FirstOrDefault(x => x?.canStackWith(iNew) == true && (x.Stack + o.Stack) <= o.maximumStackSize());
                            var index = UserInventory.IndexOf(item);
                            if (index == -1) {
                                // Can not move stack
                                // -> leave it in chest
                                modEntry.Monitor.Log($"Inventory full! Item remains in chest");
                                iOld.Stack = iNew.Stack;
                                continue;
                            }

                            modEntry.Monitor.Log(
                                $"Moved stack: {o.Name} {o.Stack}/{iNew.Stack} to inventory ({index}: {UserInventory[index]?.Name ?? "null"})!");

                            if (UserInventory[index] == null) {
                                // modEntry.Monitor.Log($"Moved to new stack");

                                UserInventory[index] = o;
                                UserSlotsEmpty--;
                            }
                            else {
                                // modEntry.Monitor.Log($"Moved to old stack: {UserInventory[index].Stack} -> {UserInventory[index].Stack + o.Stack}");
                                UserInventory[index].Stack += o.Stack;
                            }

                            // update items
                            iNew.Stack = iOld.Stack;
                            // modEntry.Monitor.Log($"Set chest stack to {iNew.Stack}");
                        }
                    }
                }
        }

        private void CombineInventories(ICollection<Chest> chests) {
            if (UserInventory != null)
                return;

            // store original inventory...
            UserSlots               = Game1.player.MaxItems;
            UserSlotsEmpty          = Game1.player.freeSpotsInInventory();
            UserInventory           = Game1.player.Items.ToList();
            CraftInventories        = GetInventories(chests).ToList();
            CraftInventoriesInitial = CraftInventories.Select(l => l.Select(ItemHelper.GetCopy).ToList()).ToList();

            // ... and replace with combined inventory
            Game1.player.Items    = new List<Item>(CraftInventories.SelectMany(l => l).Where(i => i != null));
            Game1.player.MaxItems = Game1.player.Items.Count + UserSlotsEmpty;

            // ... add empty slots
            for (int i = 0; i < UserSlotsEmpty; i++)
                Game1.player.Items.Add(null);
        }

        private IEnumerable<IList<Item>> GetInventories(ICollection<Chest> chests) {
            yield return UserInventory;

            foreach (var c in chests)
                yield return c.items;

            // always add fridge when on cooking screen
            if (!IsCookingScreen || !HasFridge)
                yield break;

            Chest fridge = Utility.getHomeOfFarmer(Game1.MasterPlayer).fridge.Value;
            if (!chests.Contains(fridge))
                yield return fridge.items;
        }

        private void UpdateEvent(object sender, EventArgs e) => CleanupInventories();
    }
}