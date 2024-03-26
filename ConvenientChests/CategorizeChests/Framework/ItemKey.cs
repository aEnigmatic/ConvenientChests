using StardewValley;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace ConvenientChests.CategorizeChests.Framework {
    public readonly struct ItemKey {
        public string ItemId { get; }
        public string TypeDefinition { get; }

        public string QualifiedItemId => $"{TypeDefinition}{ItemId}";

        public ItemKey(string typeDefinition, string itemId) {
            TypeDefinition = typeDefinition;
            ItemId = itemId;
        }

        public ItemKey(string qualifiedItemId) {
            var item = ItemRegistry.Create(qualifiedItemId);
            TypeDefinition = item.TypeDefinitionId;
            ItemId = item.ItemId;
        }

        public override int GetHashCode() => QualifiedItemId.GetHashCode();
        public override string ToString() => QualifiedItemId;

        public override bool Equals(object obj)
            => obj is ItemKey itemKey &&
               itemKey.TypeDefinition == TypeDefinition &&
               itemKey.ItemId == ItemId;

        public Item GetOne() => ItemRegistry.Create(QualifiedItemId);
        public T GetOne<T>() where T : Item => ItemRegistry.Create<T>(QualifiedItemId);


        public string GetCategory() {
            switch (TypeDefinition) {
                case "(T)":
                case "(W)" when MeleeWeapon.IsScythe(QualifiedItemId):
                    // move scythes to tools
                    return Object.GetCategoryDisplayName(Object.toolCategory);

                case "(W)":
                    // weapon subgroups
                    // return GetOne() switch {
                    //            MeleeWeapon w => w.type.Value switch {
                    //                                 1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14304"),
                    //                                 2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14305"),
                    //                                 _ => Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14306"),
                    //                             },
                    //            Slingshot => new Slingshot().DisplayName,
                    //            _ => "Weapon",
                    //        };
                    return "Weapon";

                case "(FL)":
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13203");

                case "(WP)":
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13204");

                default:
                    var categoryName = GetOne().getCategoryName();
                    return string.IsNullOrEmpty(categoryName)
                               ? "Miscellaneous"
                               : categoryName;
            }
        }
    }
}