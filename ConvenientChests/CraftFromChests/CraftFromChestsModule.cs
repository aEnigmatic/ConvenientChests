using System;
using System.Collections.Generic;
using System.Linq;
using ConvenientChests.StackToNearbyChests;
using Harmony;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using Object = System.Object;

namespace ConvenientChests.CraftFromChests {
    public class CraftFromChestsModule : Module {
        public HarmonyInstance Harmony { get; set; }

        public static List<Chest> NearbyChests { get; set; }
        public static List<Item>  NearbyItems  { get; private set; }

        private static IList<IList<Item>> _nearbyInventories;

        public static IList<IList<Item>> NearbyInventories {
            get => _nearbyInventories;
            set {
                _nearbyInventories = value;
                NearbyItems        = value?.SelectMany(l => l.Where(i => i != null)).ToList();
            }
        }

        public CraftFromChestsModule(ModEntry modEntry) : base(modEntry) { }

        public override void Activate() {
            IsActive = true;
            
            // Register Events
            MenuListener.RegisterEvents();
            MenuListener.CraftingMenuShown  += CraftingMenuShown;
            MenuListener.CraftingMenuClosed += CraftingMenuClosed;

            // Apply method patches
            Harmony = HarmonyInstance.Create("aEnigma.convenientchests");
            CraftingRecipePatch.Register(Harmony);
            FarmerPatch.Register(Harmony);
        }

        public override void Deactivate() {
            IsActive = false;
            
            // Unregister Events
            MenuListener.CraftingMenuShown  -= CraftingMenuShown;
            MenuListener.CraftingMenuClosed -= CraftingMenuClosed;
            MenuListener.UnregisterEvents();

            // Remove method patches
            CraftingRecipePatch.Remove(Harmony);
            FarmerPatch.Remove(Harmony);
        }

        private void CraftingMenuShown(object sender, EventArgs e) {
            NearbyInventories = GetInventories(Game1.activeClickableMenu is CraftingPage).ToList();
        }

        private void CraftingMenuClosed(object sender, EventArgs e) {
            NearbyInventories = null;
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