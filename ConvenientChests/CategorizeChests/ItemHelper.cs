using System.Collections.Generic;
using System.Linq;
using ConvenientChests.CategorizeChests.Framework;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;

using Console = System.Console;

namespace ConvenientChests.CategorizeChests {
    internal static class ItemHelper {
        public static ItemKey ToItemKey(this Item item) => new ItemKey(GetItemType(item), item.ItemId);

        public static Item GetCopy(this Item item) {
            if (item == null)
                return null;

            var copy = item.getOne();
            copy.Stack = item.Stack;
            return copy;
        }

        public static IEnumerable<Item> GetWeapons() {
            foreach (var itemId in Game1.weaponData.Keys)
                yield return new MeleeWeapon(itemId);

            yield return new Slingshot(Slingshot.basicSlingshotId);
            yield return new Slingshot(Slingshot.masterSlingshotId);
            yield return new Slingshot(Slingshot.galaxySlingshotId); // unobtainable; filter out later
        }

        public static ItemType GetItemType(Item item) {
            switch (item) {
                case Boots _:
                    return ItemType.Boots;

                case Furniture _:
                    return ItemType.Furniture;

                case Hat _:
                    return ItemType.Hat;

                case Ring _:
                    return ItemType.Ring;

                case Wallpaper w:
                    return w.isFloor.Value
                               ? ItemType.Flooring
                               : ItemType.Wallpaper;

                case MeleeWeapon _:
                case Slingshot _:
                    return ItemType.Weapon;

                case Tool _:
                    return ItemType.Tool;

                case Fence f:
                    return f.isGate.Value
                               ? ItemType.Gate
                               : ItemType.Object;

                case Object _:
                    switch (item.Category) {
                        case Object.FishCategory:
                            return ItemType.Fish;

                        case Object.BigCraftableCategory:
                            return ItemType.BigCraftable;

                        default:
                            return ItemType.Object;
                    }
            }

            return ItemType.Object;
        }
    }
}