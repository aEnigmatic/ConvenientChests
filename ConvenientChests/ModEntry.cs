using ConvenientChests.CategorizeChests;
using ConvenientChests.CraftFromChests;
using ConvenientChests.StackToNearbyChests;
using StardewModdingAPI;

namespace ConvenientChests {
    public class ModEntry : Mod {
        public static   Config     Config        { get; private set; }
        internal static IModHelper StaticHelper  { get; set; }
        internal static IMonitor   StaticMonitor { get; set; }

        internal static void Log(string s, LogLevel l = LogLevel.Trace) => StaticMonitor.Log(s, l);

        public StashToNearbyChestsModule StashNearby;
        public CategorizeChestsModule    CategorizeChests;
        public CraftFromChestsModule     CraftFromChests;

        public override void Entry(IModHelper helper) {
            Config = helper.ReadConfig<Config>();

            StaticMonitor = Monitor;
            StaticHelper  = Helper;

            LoadModules();
        }

        private void LoadModules() {
            StashNearby = new StashToNearbyChestsModule(this);
            if (Config.StashToNearbyChests)
                StashNearby.Activate();

            CategorizeChests = new CategorizeChestsModule(this);
            if (Config.CategorizeChests)
                CategorizeChests.Activate();

            CraftFromChests = new CraftFromChestsModule(this);
            if (Config.CraftFromChests)
                CraftFromChests.Activate();
        }
    }
}