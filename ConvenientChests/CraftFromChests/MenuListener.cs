using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ConvenientChests.CraftFromChests {
    public static class MenuListener {
        public static List<IClickableMenu> GetTabs(this GameMenu m, IReflectionHelper h) => h.GetField<List<IClickableMenu>>(m, "pages").GetValue();

        public static EventHandler GameMenuShown;
        public static EventHandler GameMenuClosed;
        public static EventHandler CraftingMenuShown;
        public static EventHandler CraftingMenuClosed;

        public static void RegisterEvents() {
            ModEntry.staticMonitor.Log("Register");
            MenuEvents.MenuChanged += OnMenuChanged;
            MenuEvents.MenuClosed  += OnMenuClosed;
        }

        public static void UnregisterEvents() {
            ModEntry.staticMonitor.Log("UnRegister");
            MenuEvents.MenuChanged -= OnMenuChanged;
            MenuEvents.MenuClosed  -= OnMenuClosed;
        }

        private static void OnMenuChanged(object sender, EventArgsClickableMenuChanged e) {
            ModEntry.staticMonitor.Log($"{e.PriorMenu?.GetType().ToString() ?? "null"} -> {e.NewMenu?.GetType().ToString() ?? "null"}");

            if (e.NewMenu == e.PriorMenu)
                return;
            
            switch (e.PriorMenu) {
                case GameMenu _:
                    GameMenuClosed?.Invoke(sender, e);
                    UnregisterTabEvent();
                    break;

                case StardewValley.Menus.CraftingPage _:
                    CraftingMenuClosed?.Invoke(sender, e);
                    break;
            }
            
            switch (e.NewMenu) {
                case GameMenu _:
                    GameMenuShown?.Invoke(sender, e);
                    GraphicsEvents.OnPostRenderGuiEvent += OnPostRenderGuiEvent;
                    break;

                case StardewValley.Menus.CraftingPage _:
                    CraftingMenuShown?.Invoke(sender, e);
                    break;
            }
        }

        private static void OnMenuClosed(object sender, EventArgsClickableMenuClosed e) {          
            switch (e.PriorMenu) {
                case GameMenu _:
                    GameMenuClosed?.Invoke(sender, e);
                    UnregisterTabEvent();
                    break;

                case StardewValley.Menus.CraftingPage _:
                    CraftingMenuClosed?.Invoke(sender, e);
                    break;
            }
        }


        private static int _previousTab = -1;

        public static event EventHandler GameMenuTabChanged;

        private static void OnPostRenderGuiEvent(object sender, EventArgs e) {
            switch (Game1.activeClickableMenu) {
                case TitleMenu _:
                    // Quit to title
                    // -> unregister silently
                    UnregisterTabEvent();
                    return;

                case GameMenu gameMenu when gameMenu.currentTab == _previousTab:
                    // Nothing changed
                    return;

                case GameMenu gameMenu:
                    // Tab changed!
                    _previousTab = gameMenu.currentTab;
                    GameMenuTabChanged?.Invoke(null, EventArgs.Empty);
                    if (_previousTab == GameMenu.craftingTab)
                        CraftingMenuClosed?.Invoke(sender, EventArgs.Empty);

                    else if (gameMenu.currentTab == GameMenu.craftingTab)
                        CraftingMenuShown?.Invoke(sender, EventArgs.Empty);
                        break;

                default:
                    // How did we get here?
                    ModEntry.staticMonitor.Log($"Unexpected menu: {Game1.activeClickableMenu?.GetType().ToString() ?? "null"}");
                    UnregisterTabEvent();
                    return;
            }
        }

        private static void UnregisterTabEvent() {
            GraphicsEvents.OnPostRenderGuiEvent -= OnPostRenderGuiEvent;
            _previousTab                        =  -1;
        }
        
    }
}