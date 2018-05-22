using System.Collections.Generic;
using System.Linq;
using ConvenientChests.CategorizeChests.Framework;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;

namespace ConvenientChests.CategorizeChests {
    internal static class ItemHelper {
        public static ItemKey ToItemKey(this Item item) => new ItemKey(GetItemType(item), GetItemID(item));

        public static Item GetCopy(this Item item) {
            if (item == null)
                return null;

            var copy = item.getOne();
            copy.Stack = item.Stack;
            return copy;
        }

        public static IEnumerable<Item> GetAllItems() {
            foreach (var i in GetTools())
                yield return i;

            foreach (var i in GetEquipment())
                yield return i;

            // wallpapers
            for (var id = 0; id < 112; id++)
                yield return new Wallpaper(id) {Category = Object.furnitureCategory};

            // flooring
            for (var id = 0; id < 40; id++)
                yield return new Wallpaper(id, true) {Category = Object.furnitureCategory};

            // furniture
            foreach (var id in Game1.content.Load<Dictionary<int, string>>("Data\\Furniture").Keys) {
                Item item = new Furniture(id, Vector2.Zero);
                // if (id == 1466 || id == 1468)
                //    item = new TV(id, Vector2.Zero);

                yield return item;
            }

            // craftables
            foreach (var id in Game1.bigCraftablesInformation.Keys)
                yield return new Object(Vector2.Zero, id);

            // objects
            foreach (var item1 in GetObjects()) yield return item1;
        }

        private static IEnumerable<Item> GetObjects() {
            foreach (var id in Game1.objectInformation.Keys) {
                if (id >= Ring.ringLowerIndexRange && id <= Ring.ringUpperIndexRange)
                    continue;

                // if (Game1.bigCraftablesInformation.ContainsKey(id))
                //     continue;

                // object
                var item = new Object(id, 1);
                yield return item;

                switch (item.Category) {
                    case Object.FruitsCategory:
                        yield return GenerateWine(item);
                        yield return GenerateJelly(item);
                        break;

                    case Object.VegetableCategory:
                        yield return GenerateJuice(item);
                        yield return GeneratePickles(item);
                        break;

                    case Object.flowersCategory:
                        Object.HoneyType type;
                        switch (item.ParentSheetIndex) {
                            case 376:
                                type = Object.HoneyType.Poppy;
                                break;
                            case 591:
                                type = Object.HoneyType.Tulip;
                                break;
                            case 593:
                                type = Object.HoneyType.SummerSpangle;
                                break;
                            case 595:
                                type = Object.HoneyType.FairyRose;
                                break;
                            case 597:
                                type = Object.HoneyType.BlueJazz;
                                break;
                            case 421: // sunflower standing in for all other flowers
                                type = Object.HoneyType.Wild;
                                break;

                            default:
                                continue;
                        }

                        yield return GenerateHoney(item, type);
                        break;
                }
            }
        }

        private static Item GenerateHoney(Object item, Object.HoneyType type) {
            var honey = new Object(Vector2.Zero, 340, item.Name + " Honey", false, true, false, false) {
                                                                                                           name      = "Wild Honey",
                                                                                                           honeyType = {Value = type}
                                                                                                       };

            if (type == Object.HoneyType.Wild)
                return honey;

            honey.name  =  $"{item.Name} Honey";
            honey.Price += item.Price * 2;

            return honey;
        }

        private static Item GeneratePickles(Object item) {
            return new Object(342, 1) {
                                          name                      = $"Pickled {item.Name}",
                                          Price                     = 50 + item.Price * 2,
                                          preserve                  = {Value = Object.PreserveType.Pickle},
                                          preservedParentSheetIndex = {Value = item.ParentSheetIndex},
                                      };
        }

        private static Item GenerateJuice(Object item) {
            return new Object(350, 1) {
                                          name                      = $"{item.Name} Juice",
                                          Price                     = (int) (item.Price * 2.25d),
                                          preserve                  = {Value = Object.PreserveType.Juice},
                                          preservedParentSheetIndex = {Value = item.ParentSheetIndex},
                                      };
        }

        private static Item GenerateJelly(Object item) {
            return new Object(344, 1) {
                                          name                      = $"{item.Name} Jelly",
                                          Price                     = 50 + item.Price * 2,
                                          preserve                  = {Value = Object.PreserveType.Jelly},
                                          preservedParentSheetIndex = {Value = item.ParentSheetIndex}
                                      };
        }

        private static Item GenerateWine(Object item) {
            return new Object(348, 1) {
                                          name                      = $"{item.Name} Wine",
                                          Price                     = item.Price * 3,
                                          preserve                  = {Value = Object.PreserveType.Wine},
                                          preservedParentSheetIndex = {Value = item.ParentSheetIndex}
                                      };
        }

        private static IEnumerable<Item> GetEquipment() {
            foreach (var item in GetWeapons()) yield return item;
            foreach (var item in GetBoots()) yield return item;
            foreach (var item in GetHats()) yield return item;
            foreach (var item in GetRings()) yield return item;
        }

        public static IEnumerable<Item> GetWeapons() {
            foreach (var e in Game1.content.Load<Dictionary<int, string>>("Data\\weapons"))
                if (e.Value.Split('/')[8] == "4")
                    yield return new Slingshot(e.Key);
                
                else
                    yield return new MeleeWeapon(e.Key);
        }


        private static IEnumerable<Item> GetRings() {
            for (var id = Ring.ringLowerIndexRange; id <= Ring.ringUpperIndexRange; id++)
                yield return new Ring(id);
        }

        private static IEnumerable<Item> GetHats()
            => Game1.content.Load<Dictionary<int, string>>("Data\\hats").Keys.Select(id => new Hat(id));

        private static IEnumerable<Item> GetBoots()
            => Game1.content.Load<Dictionary<int, string>>("Data\\hats").Keys.Select(id => new Boots(id));

        private static IEnumerable<Tool> GetTools() {
            for (var quality = Tool.stone; quality <= Tool.iridium; quality++) {
                yield return ToolFactory.getToolFromDescription(ToolFactory.axe,         quality);
                yield return ToolFactory.getToolFromDescription(ToolFactory.hoe,         quality);
                yield return ToolFactory.getToolFromDescription(ToolFactory.pickAxe,     quality);
                yield return ToolFactory.getToolFromDescription(ToolFactory.wateringCan, quality);

                if (quality != Tool.iridium)
                    yield return ToolFactory.getToolFromDescription(ToolFactory.fishingRod, quality);
            }

            yield return new MilkPail();
            yield return new Shears();
            yield return new Pan();
            yield return new Wand();
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

        public static int GetItemID(Item item) {
            switch (item) {
                case Boots a:
                    return a.indexInTileSheet.Value;

                case Ring a:
                    return a.indexInTileSheet.Value;

                case Hat a:
                    return a.which.Value;

                case Tool a:
                    return a.InitialParentTileIndex;

                case Fence a:
                    if (a.isGate.Value)
                        return 0;

                    return a.whichType.Value;

                default:
                    return item.ParentSheetIndex;
            }
        }
    }
}