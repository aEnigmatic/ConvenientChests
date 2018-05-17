using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using StardewObject = StardewValley.Object;

namespace ConvenientChests.CategorizeChests.Framework
{
    internal class ItemDataManager : IItemDataManager
    {
        private const int CustomIdOffset = 1000;

        /// <summary>
        /// A mapping of category names to the item keys belonging to that category.
        /// </summary>
        public IDictionary<string, IList<ItemKey>> Categories { get; }

        /// <summary>
        /// A mapping of item keys to a representative instance of the item they correspond to.
        /// </summary>
        public Dictionary<ItemKey, Item> Prototypes { get; }

        public ItemDataManager()
        {
            Prototypes = DiscoverItems()
                .Where(d => d.ItemKey.ItemType != ItemType.BigCraftable && !ItemBlacklist.Includes(d.ItemKey))
                .ToDictionary(d => d.ItemKey, d => d.Item);

            Categories = Prototypes
                .GroupBy(p => ChooseCategoryName(p.Key))
                .ToDictionary(
                    g => g.Key,
                    g => (IList<ItemKey>) g.Select(p => p.Key).ToList()
                );
        }

        /// <summary>
        /// Retrieve the item key appropriate for representing the given item.
        /// </summary>
        public ItemKey GetKey(Item item)
        {
            if (item == null)
                return null;

            // find matching prototype
            var result = Prototypes.FirstOrDefault(p => MatchesPrototype(item, p.Value));
            if (result.Key is ItemKey k)
                return k;

            // build one 
            return CreatePrototype(item);
        }

        /// <summary>
        /// Retrieve the representative item corresponding to the given key.
        /// </summary>
        public Item GetItem(ItemKey itemKey) => Prototypes[itemKey];

        /// <summary>
        /// Check whether the repository contains an item corresponding to the
        /// given key.
        /// </summary>
        public bool HasItem(ItemKey itemKey) => Prototypes.ContainsKey(itemKey);

        /// <summary>
        /// Generate every item known to man, or at least those we're interested
        /// in using for categorization.
        /// </summary>
        /// <remarks>
        /// Substantially based on code from Pathoschild's LookupAnything mod.
        /// </remarks>
        /// <returns>A collection of all of the item entries.</returns>
        private IEnumerable<DiscoveredItem> DiscoverItems()
        {
            // get tools
            yield return new DiscoveredItem(ItemType.Tool, ToolFactory.axe,
                ToolFactory.getToolFromDescription(ToolFactory.axe, Tool.stone));
            yield return new DiscoveredItem(ItemType.Tool, ToolFactory.hoe,
                ToolFactory.getToolFromDescription(ToolFactory.hoe, Tool.stone));
            yield return new DiscoveredItem(ItemType.Tool, ToolFactory.pickAxe,
                ToolFactory.getToolFromDescription(ToolFactory.pickAxe, Tool.stone));
            yield return new DiscoveredItem(ItemType.Tool, ToolFactory.wateringCan,
                ToolFactory.getToolFromDescription(ToolFactory.wateringCan, Tool.stone));
            yield return new DiscoveredItem(ItemType.Tool, ToolFactory.fishingRod,
                ToolFactory.getToolFromDescription(ToolFactory.fishingRod, Tool.stone));
            yield return new DiscoveredItem(ItemType.Tool, CustomIdOffset, new MilkPail());

            // these don't have any sort of ID, so we'll just assign some arbitrary ones
            yield return new DiscoveredItem(ItemType.Tool, CustomIdOffset + 1, new Shears());
            yield return new DiscoveredItem(ItemType.Tool, CustomIdOffset + 2, new Pan());

            // equipment
            foreach (int id in Game1.content.Load<Dictionary<int, string>>("Data\\Boots").Keys)
                yield return new DiscoveredItem(ItemType.Boots, id, new Boots(id));
            foreach (int id in Game1.content.Load<Dictionary<int, string>>("Data\\hats").Keys)
                yield return new DiscoveredItem(ItemType.Hat, id, new Hat(id));
            foreach (int id in Game1.objectInformation.Keys)
                if (id >= Ring.ringLowerIndexRange && id <= Ring.ringUpperIndexRange)
                    yield return new DiscoveredItem(ItemType.Ring, id, new Ring(id));

            // weapons
            foreach (int id in Game1.content.Load<Dictionary<int, string>>("Data\\weapons").Keys)
            {
                var weapon = (id >= 32 && id <= 34)
                    ? (Item) new Slingshot(id)
                    : new MeleeWeapon(id);
                yield return new DiscoveredItem(ItemType.Weapon, id, weapon);
            }

            // furniture
            /*foreach (int id in Game1.content.Load<Dictionary<int, string>>("Data\\Furniture").Keys)
            {
                if (id == 1466 || id == 1468)
                    yield return new DiscoveredItem(ItemType.Furniture, id, new TV(id, Vector2.Zero));
                else
                    yield return new DiscoveredItem(ItemType.Furniture, id, new Furniture(id, Vector2.Zero));
            }*/

            // craftables
            /*foreach (int id in Game1.bigCraftablesInformation.Keys)
                yield return new DiscoveredItem(ItemType.BigCraftable, id, new StardewObject(Vector2.Zero, id));*/

            // objects
            foreach (int id in Game1.objectInformation.Keys)
            {
                if (id >= Ring.ringLowerIndexRange && id <= Ring.ringUpperIndexRange)
                    continue; // handled separated

                var item = new StardewObject(id, 1);
                yield return new DiscoveredItem(ItemType.Object, id, item);
            }
        }

        public ItemKey CreatePrototype(Item item)
        {
            var p = new ItemKey(ItemHelper.GetItemType(item), ItemHelper.GetItemID(item));

            ModEntry.staticMonitor.Log($"Created prototype {p} for {item.Name}");

            Prototypes.Add(p, item);
            if (p.ItemType == ItemType.BigCraftable || ItemBlacklist.Includes(p))
                return p;

            // Try to categorize
            ModEntry.staticMonitor.Log($"Added prototype {p} to category {ChooseCategoryName(p)}");
            Categories[ChooseCategoryName(p)].Add(p);

            return p;
        }

        /// <summary>
        /// Check whether a given item should be classified as the same as the
        /// given representative item for the purposes of categorization.
        /// </summary>
        /// <param name="item">The item being checked.</param>
        /// <param name="prototype">The representative prototype item to check against.</param>
        public static bool MatchesPrototype(Item item, Item prototype)
        {
            return
                // same generic item type
                (
                    item.GetType() == prototype.GetType()
                    || prototype.GetType() == typeof(StardewObject) && item.GetType() == typeof(ColoredObject)
                )
                && item.Category == prototype.Category
                && item.ParentSheetIndex == prototype.ParentSheetIndex

                // same discriminators
                && (item as Boots)?.indexInTileSheet == (prototype as Boots)?.indexInTileSheet
                && (item as BreakableContainer)?.Type == (prototype as BreakableContainer)?.Type
                && (item as Fence)?.isGate == (prototype as Fence)?.isGate
                && (item as Fence)?.whichType == (prototype as Fence)?.whichType
                && (item as Hat)?.which == (prototype as Hat)?.which
                && (item as Ring)?.indexInTileSheet == (prototype as Ring)?.indexInTileSheet
                && (item as MeleeWeapon)?.type == (prototype as MeleeWeapon)?.type
                && (item as MeleeWeapon)?.InitialParentTileIndex == (prototype as MeleeWeapon)?.InitialParentTileIndex
                ;
        }

        /// <summary>
        /// Decide what category name the given item key should belong to.
        /// </summary>
        /// <returns>The chosen category name.</returns>
        /// <param name="itemKey">The item key to categorize.</param>
        private string ChooseCategoryName(ItemKey itemKey)
        {
            if (itemKey.ItemType != ItemType.Object)
                return Enum.GetName(typeof(ItemType), itemKey.ItemType);

            var categoryName = GetItem(itemKey).getCategoryName();
            return string.IsNullOrEmpty(categoryName) ? "Miscellaneous" : categoryName;
        }
    }
}