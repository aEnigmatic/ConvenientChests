using System.Collections.Generic;

namespace ConvenientChests.CategorizeChests.Framework {
    /// <summary>
    /// A repository of item data that maps item keys to representative items
    /// and vice versa.
    /// </summary>
    internal interface IItemDataManager {
        Dictionary<string, IList<ItemKey>> Categories { get; }
    }
}