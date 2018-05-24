using System;
using System.Collections.Generic;
using System.Linq;
using ConvenientChests.StackToNearbyChests;
using Harmony;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace ConvenientChests.CraftFromChests {
    public class CraftFromChestsModule : Module {
        public HarmonyInstance Harmony { get; set; }
        
        public CraftFromChestsModule(ModEntry modEntry) : base(modEntry) { }

        public override void Activate() {
            MenuListener.RegisterEvents();
            MenuListener.CraftingMenuShown += (sender, e) => ReplaceCraftingScreen();
            
            Harmony = HarmonyInstance.Create("aEnigma.convenientchests");
            RecipeTooltextReplacer.Register(Harmony);
        }


        private void ReplaceCraftingScreen() {
            switch (Game1.activeClickableMenu) {
                case GameMenu gameMenu:
                    var tabs    = gameMenu.GetTabs(ModEntry.Helper.Reflection);
                    var oldPage = (StardewValley.Menus.CraftingPage) tabs[GameMenu.craftingTab];
                    var newPage = new CraftingPage(oldPage, false, GetInventories(false).ToList());
                    tabs[GameMenu.craftingTab] = newPage;
                    gameMenu.changeTab(GameMenu.craftingTab);
                    break;

                case StardewValley.Menus.CraftingPage p:
                    if (p is CraftingPage)
                        return;

                    Game1.activeClickableMenu = new CraftingPage(p, true, GetInventories(true).ToList());
                    break;

                default:
                    // How did we get here?
                    throw new Exception($"Unexpected menu: {Game1.activeClickableMenu?.GetType().ToString() ?? "null"}");
            }
        }

        private IEnumerable<IList<Item>> GetInventories(bool isCookingScreen) {
            // nearby chests
            var chests = Game1.player.GetNearbyChests(Config.CraftRadius).Where(c => c.items.Any(i => i != null)).ToList();
            foreach (var c in chests)
                yield return c.items;

            // always add fridge when on cooking screen
            if (!isCookingScreen)
                yield break;

            var house = Game1.player.currentLocation as FarmHouse ?? Utility.getHomeOfFarmer(Game1.player) ?? null;
            if (house == null || house.upgradeLevel == 0)
                yield break;

            var fridge = house.fridge.Value;
            if (!chests.Contains(fridge))
                yield return fridge.items;
        }
    }
}