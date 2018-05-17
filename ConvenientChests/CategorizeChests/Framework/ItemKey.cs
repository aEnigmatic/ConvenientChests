namespace ConvenientChests.CategorizeChests.Framework
{
    internal class ItemKey
    {
        public readonly ItemType ItemType;
        public readonly int ObjectIndex;

        public ItemKey(ItemType itemType, int parentSheetIndex)
        {
            ItemType = itemType;
            ObjectIndex = parentSheetIndex;
        }

        public override int GetHashCode() => (int) ItemType * 10000 + ObjectIndex;

        public override string ToString() => $"{ItemType}:{ObjectIndex}";

        public override bool Equals(object obj) => obj is ItemKey itemKey &&
                                                   itemKey.ItemType == ItemType &&
                                                   itemKey.ObjectIndex == ObjectIndex;
    }
}