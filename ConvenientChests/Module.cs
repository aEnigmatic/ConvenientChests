using StardewModdingAPI;

namespace ConvenientChests
{
    public abstract class Module
    {
        public bool isActive { get; protected set; } = false;
        public ModEntry modEntry { get; }
        public Config Config => ModEntry.config;
        public IMonitor Monitor => modEntry.Monitor;
        
        public Module(ModEntry modEntry) => this.modEntry = modEntry;

        public abstract void activate();
    }
}