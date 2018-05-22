using StardewModdingAPI;

namespace ConvenientChests.CategorizeChests.Framework.Persistence {
    /// <summary>
    /// The class responsible for saving and loading the mod state.
    /// </summary>
    class SaveManager : ISaveManager {
        private readonly CategorizeChestsModule Module;
        private readonly ISemanticVersion       Version;

        public SaveManager(ISemanticVersion version, CategorizeChestsModule module) {
            Version = version;
            Module  = module;
        }

        /// <summary>
        /// Generate save data and write it to the given file path.
        /// </summary>
        /// <param name="path">The full path of the save file.</param>
        public void Save(string path) {
            var saver = new Saver(Version, Module.ChestDataManager);
            Module.ModEntry.Helper.WriteJsonFile(path, saver.GetSerializableData());
        }

        /// <summary>
        /// Load save data from the given file path.
        /// </summary>
        public void Load(string path) {
            var model = Module.ModEntry.Helper.ReadJsonFile<SaveData>(path) ?? new SaveData();

            foreach (var entry in model.ChestEntries) {
                var chest     = Module.ChestFinder.GetChestByAddress(entry.Address);
                var chestData = Module.ChestDataManager.GetChestData(chest);

                chestData.AcceptedItemKinds = entry.GetItemSet();
            }
        }
    }
}