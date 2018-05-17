using ConvenientChests.CategorizeChests;
using ConvenientChests.CraftFromChests;
using ConvenientChests.StackToNearbyChests;
using StardewModdingAPI;

namespace ConvenientChests
{
    public class ModEntry : Mod
    {
        public static Config config { get; internal set; }
        internal static IMonitor staticMonitor { get; set; }
        internal static IModHelper staticHelper { get; set; }

        public StashToNearbyChestsModule StashNearby;
        public CategorizeChestsModule  CategorizeChests;
        public CraftFromChestsModule CraftFromChests;
        
        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<Config>();

            staticMonitor = Monitor;
            staticHelper = Helper;

            LoadModules();
        }

        private void LoadModules()
        {
            StashNearby = new StashToNearbyChestsModule(this);
            if (config.StashToNearbyChests)
                StashNearby.activate();

            CategorizeChests = new CategorizeChestsModule(this);
            if (config.CategorizeChests)
                CategorizeChests.activate();

            CraftFromChests = new CraftFromChestsModule(this);
            if (config.CraftFromChests)
                CraftFromChests.activate();
        }

    }
}