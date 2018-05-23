using System.Collections.Generic;
using System.Linq;
using ConvenientChests.CategorizeChests.Framework.Persistence;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace ConvenientChests.CategorizeChests.Framework
{
    /// <summary>
    /// The class responsible for producing data to be saved.
    /// </summary>
    class Saver
    {
        private readonly ISemanticVersion Version;
        private readonly IChestDataManager ChestDataManager;

        public Saver(ISemanticVersion version, IChestDataManager chestDataManager)
        {
            Version = version;
            ChestDataManager = chestDataManager;
        }

        /// <summary>
        /// Build save data for the current game state.
        /// </summary>
        public SaveData GetSerializableData()
        {
            return new SaveData
            {
                Version = Version.ToString(),
                ChestEntries = BuildChestEntries()
            };
        }

        private IEnumerable<ChestEntry> BuildChestEntries()
        {
            foreach (var location in Game1.locations)
            {
                // chests
                foreach (var pair in GetLocationChests(location))
                    yield return new ChestEntry(
                        ChestDataManager.GetChestData(pair.Value),
                        new ChestAddress(location.Name, pair.Key)
                    );

                switch (location)
                {
                    // buildings
                    case BuildableGameLocation buildableLocation:
                        foreach (var building in buildableLocation.buildings.Where(b => b.indoors != null))
                        foreach (var pair in GetLocationChests(building.indoors))
                            yield return new ChestEntry(
                                ChestDataManager.GetChestData(pair.Value),
                                new ChestAddress(location.Name, pair.Key, ChestLocationType.Building, building.nameOfIndoors)
                            );
                        break;

                    // fridges
                    case FarmHouse farmHouse when farmHouse.upgradeLevel >= 1:
                        yield return new ChestEntry(
                            ChestDataManager.GetChestData(farmHouse.fridge),
                            new ChestAddress {LocationName = farmHouse.Name, LocationType = ChestLocationType.Refrigerator}
                        );
                        break;
                }
            }
        }

        /// <summary>
        /// Retrieve a collection of the chest objects present in the given
        /// location, keyed by their tile location.
        /// </summary>
        private static IDictionary<Vector2, Chest> GetLocationChests(GameLocation location) =>
            location.Objects
                .Where(pair => pair.Value is Chest c && c.playerChest)
                .ToDictionary(
                    pair => pair.Key,
                    pair => (Chest) pair.Value
                );
    }
}