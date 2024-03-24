using System.Collections.Generic;

namespace ConvenientChests.CategorizeChests.Framework
{
    /// <summary>
    /// Maintains the list of items that should be excluded from the available
    /// items to use for categorization, e.g. unobtainable items and bug items.
    /// </summary>
    static class ItemBlacklist
    {
        /// <summary>
        /// Check whether a given item key is blacklisted.
        /// </summary>
        /// <returns>Whether the key is blacklisted.</returns>
        /// <param name="itemKey">Item key to check.</param>
        public static bool Includes(ItemKey itemKey) => 
            itemKey.ItemType == ItemType.BigCraftable ||itemKey.ItemType == ItemType.Furniture || BlacklistedItemKeys.Contains(itemKey);

        private static readonly HashSet<ItemKey> BlacklistedItemKeys = new HashSet<ItemKey> {
            // stones
            new ItemKey(ItemType.Object, "2"),
            new ItemKey(ItemType.Object, "4"),
            new ItemKey(ItemType.Object, "6"),
            new ItemKey(ItemType.Object, "8"),
            new ItemKey(ItemType.Object, "10"),
            new ItemKey(ItemType.Object, "12"),
            new ItemKey(ItemType.Object, "14"),
            new ItemKey(ItemType.Object, "25"),
            new ItemKey(ItemType.Object, "32"),
            new ItemKey(ItemType.Object, "34"),
            new ItemKey(ItemType.Object, "36"),
            new ItemKey(ItemType.Object, "38"),
            new ItemKey(ItemType.Object, "40"),
            new ItemKey(ItemType.Object, "42"),
            new ItemKey(ItemType.Object, "44"),
            new ItemKey(ItemType.Object, "46"),
            new ItemKey(ItemType.Object, "48"),
            new ItemKey(ItemType.Object, "50"),
            new ItemKey(ItemType.Object, "52"),
            new ItemKey(ItemType.Object, "54"),
            new ItemKey(ItemType.Object, "56"),
            new ItemKey(ItemType.Object, "58"),
            new ItemKey(ItemType.Object, "75"),
            new ItemKey(ItemType.Object, "76"),
            new ItemKey(ItemType.Object, "77"),
            new ItemKey(ItemType.Object, "95"),
            new ItemKey(ItemType.Object, "290"),
            new ItemKey(ItemType.Object, "343"),
            new ItemKey(ItemType.Object, "450"),
            new ItemKey(ItemType.Object, "668"),
            new ItemKey(ItemType.Object, "670"),
            new ItemKey(ItemType.Object, "751"),
            new ItemKey(ItemType.Object, "760"),
            new ItemKey(ItemType.Object, "762"),
            new ItemKey(ItemType.Object, "764"),
            new ItemKey(ItemType.Object, "765"),
            new ItemKey(ItemType.Object, "816"),
            new ItemKey(ItemType.Object, "817"),
            new ItemKey(ItemType.Object, "818"),
            new ItemKey(ItemType.Object, "819"),
            new ItemKey(ItemType.Object, "843"),
            new ItemKey(ItemType.Object, "844"),
            new ItemKey(ItemType.Object, "845"),
            new ItemKey(ItemType.Object, "846"),
            new ItemKey(ItemType.Object, "847"),
            new ItemKey(ItemType.Object, "849"),
            new ItemKey(ItemType.Object, "850"),
            new ItemKey(ItemType.Object, "BasicCoalNode0"),
            new ItemKey(ItemType.Object, "BasicCoalNode1"),
            new ItemKey(ItemType.Object, "CalicoEggStone_0"),
            new ItemKey(ItemType.Object, "CalicoEggStone_1"),
            new ItemKey(ItemType.Object, "CalicoEggStone_2"),
            new ItemKey(ItemType.Object, "PotOfGold"),
            new ItemKey(ItemType.Object, "VolcanoGoldNode"),
            new ItemKey(ItemType.Object, "VolcanoCoalNode0"),
            new ItemKey(ItemType.Object, "VolcanoCoalNode1"),

            // weeds
            new ItemKey(ItemType.Object, "0"),
            new ItemKey(ItemType.Object, "313"),
            new ItemKey(ItemType.Object, "314"),
            new ItemKey(ItemType.Object, "315"),
            new ItemKey(ItemType.Object, "316"),
            new ItemKey(ItemType.Object, "317"),
            new ItemKey(ItemType.Object, "318"),
            new ItemKey(ItemType.Object, "319"),
            new ItemKey(ItemType.Object, "320"),
            new ItemKey(ItemType.Object, "321"),
            new ItemKey(ItemType.Object, "452"),
            new ItemKey(ItemType.Object, "674"),
            new ItemKey(ItemType.Object, "675"),
            new ItemKey(ItemType.Object, "676"),
            new ItemKey(ItemType.Object, "677"),
            new ItemKey(ItemType.Object, "678"),
            new ItemKey(ItemType.Object, "679"),
            new ItemKey(ItemType.Object, "750"),
            new ItemKey(ItemType.Object, "784"),
            new ItemKey(ItemType.Object, "785"),
            new ItemKey(ItemType.Object, "786"),
            new ItemKey(ItemType.Object, "792"),
            new ItemKey(ItemType.Object, "793"),
            new ItemKey(ItemType.Object, "794"),
            new ItemKey(ItemType.Object, "882"),
            new ItemKey(ItemType.Object, "883"),
            new ItemKey(ItemType.Object, "884"),
            new ItemKey(ItemType.Object, "GreenRainWeeds0"),
            new ItemKey(ItemType.Object, "GreenRainWeeds1"),
            new ItemKey(ItemType.Object, "GreenRainWeeds2"),
            new ItemKey(ItemType.Object, "GreenRainWeeds3"),
            new ItemKey(ItemType.Object, "GreenRainWeeds4"),
            new ItemKey(ItemType.Object, "GreenRainWeeds5"),
            new ItemKey(ItemType.Object, "GreenRainWeeds6"),
            new ItemKey(ItemType.Object, "GreenRainWeeds7"),

            // twigs
            new ItemKey(ItemType.Object, "294"),
            new ItemKey(ItemType.Object, "295"),

            // quest items
            new ItemKey(ItemType.Object, "71"), // Trimmed Lucky Purple Shorts
            new ItemKey(ItemType.Object, "191"), // Ornate Necklace
            new ItemKey(ItemType.Object, "742"), // Haley's Lost Bracelet
            new ItemKey(ItemType.Object, "788"), // Lost Axe
            new ItemKey(ItemType.Object, "789"), // Lucky Purple Shorts
            new ItemKey(ItemType.Object, "790"), // Berry Basket
            new ItemKey(ItemType.Object, "864"), // War Memento
            new ItemKey(ItemType.Object, "865"), // Gourmet Tomato Salt
            new ItemKey(ItemType.Object, "866"), // Stardew Valley Rose
            new ItemKey(ItemType.Object, "867"), // Advanced TV Remote
            new ItemKey(ItemType.Object, "868"), // Arctic Shard
            new ItemKey(ItemType.Object, "869"), // Wriggling Worm
            new ItemKey(ItemType.Object, "870"), // Pirate's Locket
            new ItemKey(ItemType.Object, "875"), // Ectoplasm
            new ItemKey(ItemType.Object, "876"), // Prismatic Jelly
            new ItemKey(ItemType.Object, "870"), // Pirate's Locket
            new ItemKey(ItemType.Object, "897"), // Pierre's Missing Stocklist
            new ItemKey(ItemType.Object, "GoldenBobber"),

            // unstorable items (used on pickup)
            new ItemKey(ItemType.Object, "73"), // Golden Walnut
            new ItemKey(ItemType.Object, "434"), // Stardrop
            new ItemKey(ItemType.Object, "858"), // Qi Gem

            // supply crates
            new ItemKey(ItemType.Object, "922"),
            new ItemKey(ItemType.Object, "923"),
            new ItemKey(ItemType.Object, "924"),
            new ItemKey(ItemType.Object, "925"), // Slime Crate

            // unobtainable
            new ItemKey(ItemType.Object, "30"), // Lumber
            new ItemKey(ItemType.Object, "94"), // Spirit Torch
            new ItemKey(ItemType.Object, "102"), // Lost Book
            new ItemKey(ItemType.Object, "449"), // Stone Base
            new ItemKey(ItemType.Object, "461"), // Decorative Pot
            new ItemKey(ItemType.Object, "528"), // Jukebox Ring
            new ItemKey(ItemType.Object, "590"), // Artifact Spot
            new ItemKey(ItemType.Object, "892"), // Warp Totem: Qi's Arena
            new ItemKey(ItemType.Object, "927"), // Camping Stove
            new ItemKey(ItemType.Object, "929"), // Hedge
            
            new ItemKey(ItemType.Weapon, "25"), // Alex's Bat
            new ItemKey(ItemType.Weapon, "30"), // Sam's Old Guitar
            new ItemKey(ItemType.Weapon, "35"), // Elliott's Pencil
            new ItemKey(ItemType.Weapon, "36"), // Maru's Wrench
            new ItemKey(ItemType.Weapon, "37"), // Harvey's Mallet
            new ItemKey(ItemType.Weapon, "38"), // Penny's Fryer
            new ItemKey(ItemType.Weapon, "39"), // Leah's Whittler
            new ItemKey(ItemType.Weapon, "40"), // Abby's Planchette
            new ItemKey(ItemType.Weapon, "41"), // Seb's Lost Mace
            new ItemKey(ItemType.Weapon, "42"), // Haley's Iron
            new ItemKey(ItemType.Weapon, "20"), // Elf Blade
            new ItemKey(ItemType.Weapon, "34"), // Galaxy Slingshot
            new ItemKey(ItemType.Weapon, "46"), // Kudgel
            new ItemKey(ItemType.Weapon, "49"), // Rapier
            new ItemKey(ItemType.Weapon, "19"), // Shadow Dagger
            new ItemKey(ItemType.Weapon, "48"), // Yeti Tooth

            new ItemKey(ItemType.Boots, "515"), // Cowboy Boots

            new ItemKey(ItemType.Object, "SeedSpot"),
        };
    }
}
