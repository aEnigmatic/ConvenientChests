using System;
using System.IO;
using ConvenientChests.CategorizeChests.Framework;
using ConvenientChests.CategorizeChests.Framework.Persistence;
using ConvenientChests.CategorizeChests.Interface;
using ConvenientChests.CategorizeChests.Interface.Widgets;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace ConvenientChests.CategorizeChests
{
    public class CategorizeChestsModule : Module
    {
        internal IItemDataManager ItemDataManager { get; } = new ItemDataManager();
        internal IChestDataManager ChestDataManager { get; } = new ChestDataManager();
        internal ChestFinder ChestFinder { get; } = new ChestFinder();

        internal static string SaveDirectory => Path.Combine(ModEntry.staticHelper.DirectoryPath, "savedata");
        internal static string SavePath => Path.Combine(SaveDirectory, Constants.SaveFolderName + ".json");
        internal SaveManager SaveManager { get; set; }


        private WidgetHost WidgetHost { get; set; }

//        public bool ChestAcceptsItem(Chest chest, Item item) => ChestAcceptsItem(chest, item.ToItemKey());
        public bool ChestAcceptsItem(Chest chest, Item item) => item != null && ChestDataManager.GetChestData(chest).Accepts(ItemDataManager.GetKey(item));
        internal bool ChestAcceptsItem(Chest chest, ItemKey itemKey) => itemKey != null && ChestDataManager.GetChestData(chest).Accepts(itemKey);

        public CategorizeChestsModule(ModEntry modEntry) : base(modEntry)
        {
            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);
        }

        public override void activate()
        {
            isActive = true;
            SaveManager = new SaveManager(modEntry.ModManifest.Version, this);

            // Events
            MenuEvents.MenuChanged += onMenuChanged;
            MenuEvents.MenuClosed += onMenuClosed;
            SaveEvents.BeforeSave += OnGameSaving;
            SaveEvents.AfterLoad += onGameLoaded;
        }

        private void OnGameSaving(object sender, EventArgs e)
        {
            try
            {
                SaveManager.Save(SavePath);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error saving chest data to {SavePath}", LogLevel.Error);
                Monitor.Log(ex.ToString());
            }
        }

        private void onGameLoaded(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(SavePath))
                    SaveManager.Load(SavePath);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error loading chest data from {SavePath}", LogLevel.Error);
                Monitor.Log(ex.ToString());
            }
        }

        private void onMenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (e.PriorMenu is ItemGrabMenu)
                if (e.NewMenu is ItemGrabMenu)
                    return;
                else
                    clearMenu();

            if (e.NewMenu is ItemGrabMenu itemGrabMenu)
                createMenu(itemGrabMenu);
        }

        private void onMenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            if (e.PriorMenu is ItemGrabMenu)
                clearMenu();
        }

        private void createMenu(ItemGrabMenu itemGrabMenu)
        {
            if (!(itemGrabMenu.behaviorOnItemGrab?.Target is Chest chest))
                return;

            WidgetHost = new WidgetHost();
            var overlay = new ChestOverlay(this, chest, itemGrabMenu, WidgetHost.TooltipManager);
            WidgetHost.RootWidget.AddChild(overlay);
        }

        private void clearMenu()
        {
            WidgetHost?.Dispose();
            WidgetHost = null;
        }
    }
}