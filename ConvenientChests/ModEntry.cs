using ConvenientChests.CategorizeChests;
using ConvenientChests.CraftFromChests;
using ConvenientChests.StackToNearbyChests;
using StardewModdingAPI;

namespace ConvenientChests {
    public class ModEntry : Mod {
        public static   Config     Config        { get; private set; }
        internal static IModHelper staticHelper  { get; set; }
        internal static IMonitor   staticMonitor { get; set; }
        internal static void       Log(string s, LogLevel l = LogLevel.Debug) => staticMonitor.Log(s, l);

        public StashToNearbyChestsModule StashNearby;
        public CategorizeChestsModule    CategorizeChests;
        public CraftFromChestsModule     CraftFromChests;

        public override void Entry(IModHelper helper) {
            Config = Helper.ReadConfig<Config>();

            staticMonitor = Monitor;
            staticHelper  = Helper;

            LoadModules();
        }

        private void LoadModules() {
            StashNearby = new StashToNearbyChestsModule(this);
            if (Config.StashToNearbyChests)
                StashNearby.activate();

            CategorizeChests = new CategorizeChestsModule(this);
            if (Config.CategorizeChests)
                CategorizeChests.activate();

            CraftFromChests = new CraftFromChestsModule(this);
            if (Config.CraftFromChests)
                CraftFromChests.activate();
        }
    }
}