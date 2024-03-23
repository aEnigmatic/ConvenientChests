using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using StardewObject = StardewValley.Object;

namespace ConvenientChests.CategorizeChests.Framework {
    internal class ItemDataManager : IItemDataManager {
        /// <summary>
        /// A mapping of category names to the item keys belonging to that category.
        /// </summary>
        public Dictionary<string, IList<ItemKey>> Categories { get; }

        /// <summary>
        /// A mapping of item keys to a representative instance of the item they correspond to.
        /// </summary>
        public Dictionary<ItemKey, Item> Prototypes { get; } = new Dictionary<ItemKey, Item>();

        public ItemDataManager() {
            // Load standard items
            foreach (var item in DiscoverItems()) {
                var key = CreateItemKey(item);
                if (ItemBlacklist.Includes(key))
                    continue;

                if (Prototypes.ContainsKey(key))
                    continue;

                Prototypes.Add(key, item);
            }

            // Create Categories
            Categories = Prototypes.Keys
                                   .GroupBy(GetCategoryName)
                                   .ToDictionary(
                                        g => g.Key,
                                        g => (IList<ItemKey>) g.ToList()
                                    );
        }

        public ItemKey GetItemKey(Item item) {
            if (item == null)
                throw new Exception();

            var key = CreateItemKey(item);
            if (Prototypes.ContainsKey(key))
                return key;

            // Add to prototypes
            Prototypes.Add(key, item);
            
            var category = GetCategoryName(key);
            ModEntry.Log($"Added prototype for '{item.DisplayName}' ({key}) to category '{category}'", LogLevel.Debug);

            // Add to categories, if not blacklisted
            if (ItemBlacklist.Includes(key))
                return key;

            if (!Categories.ContainsKey(category))
                Categories.Add(category, new List<ItemKey>());

            if (!Categories[category].Contains(key))
                Categories[category].Add(key);


            return key;
        }

        protected ItemKey CreateItemKey(Item item) {
            switch (item) {
                // Tool family overrides
                case Axe _:
                    return new Axe().ToItemKey();
                case Pickaxe _:
                    return new Pickaxe().ToItemKey();
                case Hoe _:
                    return new Hoe().ToItemKey();
                case WateringCan _:
                    return new WateringCan().ToItemKey();
                case FishingRod _:
                    return new FishingRod().ToItemKey();
                case Pan _:
                    return new Pan().ToItemKey();

                default:
                    return item.ToItemKey();
            }
        }

        public Item GetItem(ItemKey itemKey) => Prototypes.ContainsKey(itemKey)
                                                    ? Prototypes[itemKey]
                                                    : itemKey.GetOne();

        /// <summary>
        /// Generate every item known to man, or at least those we're interested
        /// in using for categorization.
        /// </summary>
        /// 
        /// <remarks>
        /// Substantially based on code from Pathoschild's LookupAnything mod.
        /// </remarks>
        /// 
        /// <returns>A collection of all of the item entries.</returns>
        private IEnumerable<Item> DiscoverItems() {
            // upgradable tools
            yield return new Axe();
            yield return new Hoe();
            yield return new Pickaxe();
            yield return new Pan();
            yield return new WateringCan();
            yield return new FishingRod();

            // other tools
            yield return new MilkPail();
            yield return new Shears();
            yield return new Wand();

            // equipment
            foreach (string id in DataLoader.Boots(Game1.content).Keys)
                yield return new Boots(id);

            foreach (string id in DataLoader.Hats(Game1.content).Keys)
                yield return new Hat(id);

            // rings handled under objects

            // weapons
            foreach (var item in ItemHelper.GetWeapons())
                yield return item;

            // objects
            foreach (var (id, objectData) in Game1.objectData) {
                if (objectData.Type.Equals("Ring"))
                    yield return new Ring(id);
                else
                    yield return new StardewObject(id, 1);
            }
        }


        /// <summary>
        /// Decide what category name the given item key should belong to.
        /// </summary>
        /// <returns>The chosen category name.</returns>
        /// <param name="itemKey">The item key to categorize.</param>
        public string GetCategoryName(ItemKey itemKey) {
            // move scythe to tools
            if (itemKey.ItemType == ItemType.Weapon && MeleeWeapon.IsScythe(itemKey.ItemId))
                return "Tool";

            if (itemKey.ItemType != ItemType.Object)
                return itemKey.ItemType.ToString();


            var categoryName = GetItem(itemKey).getCategoryName();
            return string.IsNullOrEmpty(categoryName) ? "Miscellaneous" : categoryName;
        }
    }
}