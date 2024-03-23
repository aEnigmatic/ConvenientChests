using StardewValley;

namespace ConvenientChests.CategorizeChests.Framework
{
    class DiscoveredItem
    {
        public readonly ItemKey ItemKey;
        public readonly Item Item;

        public DiscoveredItem(ItemType type, string itemId, Item item)
        {
            ItemKey = new ItemKey(type, itemId);
            Item = item;
        }
    }
}