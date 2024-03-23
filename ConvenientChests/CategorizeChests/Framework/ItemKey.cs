using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace ConvenientChests.CategorizeChests.Framework {
    internal struct ItemKey {
        public ItemType ItemType    { get; }
        public string   ItemId { get; }

        public ItemKey(ItemType itemType, string itemId) {
            ItemType    = itemType;
            ItemId      = itemId;
        }

        public override int GetHashCode() => ToString().GetHashCode();

        public override string ToString() => $"{ItemType}:{ItemId}";

        public override bool Equals(object obj) => obj is ItemKey itemKey       &&
                                                   itemKey.ItemType == ItemType &&
                                                   itemKey.ItemId   == ItemId;

        public Item GetOne() {
            switch (ItemType) {
                case ItemType.Boots:
                    return new Boots(ItemId);

                case ItemType.Furniture:
                    return new Furniture(ItemId, Vector2.Zero);

                case ItemType.Hat:
                    return new Hat(ItemId);

                case ItemType.Fish:
                case ItemType.Object:
                case ItemType.BigCraftable:
                    return new Object(ItemId, 1);

                case ItemType.Ring:
                    return new Ring(ItemId);

                case ItemType.Tool:
                    return ItemRegistry.Create(ItemId);

                case ItemType.Wallpaper:
                    return ItemRegistry.Create($"(WP){ItemId}");

                case ItemType.Flooring:
                    return ItemRegistry.Create($"(FL){ItemId}");

                case ItemType.Weapon:
                    return new MeleeWeapon(ItemId);

                case ItemType.Gate:
                    return new Fence(Vector2.Zero, ItemId, true);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string GetCategory() {
            // move scythe to tools
            if (ItemType == ItemType.Weapon && MeleeWeapon.IsScythe(ItemId))
                return Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14307");

            if (ItemType != ItemType.Object)
                return ItemType.ToString();

            var categoryName = GetOne().getCategoryName();
            return string.IsNullOrEmpty(categoryName) ? "Miscellaneous" : categoryName;
        }
    }
}