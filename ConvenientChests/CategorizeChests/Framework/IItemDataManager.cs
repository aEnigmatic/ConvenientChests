using System.Collections.Generic;
using StardewValley;

namespace ConvenientChests.CategorizeChests.Framework
{
    /// <summary>
    /// A repository of item data that maps item keys to representative items
    /// and vice versa.
    /// </summary>
    internal interface IItemDataManager
    {
        IDictionary<string, IList<ItemKey>> Categories { get; }

        ItemKey GetKey(Item item);
        Item    GetItem(ItemKey itemKey);
        bool    HasItem(ItemKey itemKey);
    }
}