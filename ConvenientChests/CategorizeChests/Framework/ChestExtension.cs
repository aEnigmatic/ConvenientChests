using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Objects;

namespace ConvenientChests.CategorizeChests.Framework
{
    internal static class ChestExtension
    {
        public static bool containsItem(this Chest chest, Item i) => chest.items.Any(i.canStackWith);

        /// <summary>
        /// Attempt to move as much as possible of the player's inventory into the given chest
        /// </summary>
        ///
        /// <param name="chest">The chest to put the items in.</param>
        /// <param name="items">Items to put in</param>
        ///
        /// <returns>List of Items that were successfully moved into the chest</returns>
        public static List<Item> dumpItemsToChest(this Chest chest, IEnumerable<Item> items)
        {
            var changedItems = items.Where(item => item != null && tryPutItemInChest(chest, item)).ToList();
            
            return changedItems;
        }

        /// <summary>
        /// Attempt to move as much as possible of the given item stack into the chest.
        /// </summary>
        /// 
        /// <param name="chest">The chest to put the items in.</param>
        /// <param name="item">The items to put in the chest.</param>
        ///
        /// <returns>True if at least some of the stack was moved into the chest.</returns>
        public static bool tryPutItemInChest(Chest chest, Item item)
        {
            var remainder = chest.addItem(item);
            if (remainder == null)
            {
                Game1.player.removeItemFromInventory(item);
                return true;
            }

            if (remainder.Stack == item.Stack)
                return false;

            item.Stack = remainder.Stack;
            return true;
        }

        /// <summary>
        /// Check whether the given chest has any completely empty slots.
        /// </summary>
        /// <returns>Whether at least one slot is empty.</returns>
        /// <param name="chest">The chest to check.</param>
        public static bool hasEmptySlots(this Chest chest)
            => chest.items.Count < Chest.capacity || chest.items.Any(i => i == null);
    }
}