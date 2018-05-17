using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ConvenientChests.CraftFromChests
{
    public static class GameMenuExtension
    {
        public static List<IClickableMenu> getTabs(this GameMenu m) =>
            ModEntry.staticHelper.Reflection.GetField<List<IClickableMenu>>(m, "pages").GetValue();


        private static EventHandler _shown;

        public static event EventHandler Shown
        {
            add
            {
                // subscribe, if first
                if (_shown == null || !_shown.GetInvocationList().Any())
                    RegisterEvents();

                _shown += value;
            }
            remove
            {
                _shown += value;

                // unsubscribe, if last
                if (_shown == null || !_shown.GetInvocationList().Any())
                    UnregisterEvents();
            }
        }

        private static void RegisterEvents()
        {
            ModEntry.staticMonitor.Log("Register");
            MenuEvents.MenuChanged += OnMenuChanged;
            MenuEvents.MenuClosed += OnMenuClosed;
        }

        private static void UnregisterEvents()
        {
            ModEntry.staticMonitor.Log("UnRegister");
            MenuEvents.MenuChanged -= OnMenuChanged;
            MenuEvents.MenuClosed -= OnMenuClosed;
        }

        private static void OnMenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (!(e.NewMenu is GameMenu))
                return;

            _shown?.Invoke(null, EventArgs.Empty);
            GraphicsEvents.OnPostRenderGuiEvent += OnPostRenderGuiEvent;
        }

        private static void OnMenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            if (!(e.PriorMenu is GameMenu))
                return;

            GraphicsEvents.OnPostRenderGuiEvent -= OnPostRenderGuiEvent;
        }


        private static int _previousTab = -1;

        public static event EventHandler TabChanged;
        private static void OnTabChanged() => TabChanged?.Invoke(null, EventArgs.Empty);

        private static void OnPostRenderGuiEvent(object sender, EventArgs e)
        {
            if (!(Game1.activeClickableMenu is GameMenu gameMenu)) 
            {
                ModEntry.staticMonitor.Log($"should not happen: Menu is {Game1.activeClickableMenu?.GetType().ToString() ?? "null"}");
                GraphicsEvents.OnPostRenderGuiEvent -= OnPostRenderGuiEvent;
                return;
            }
            
            if (gameMenu.currentTab == _previousTab)
                return;

            _previousTab = gameMenu.currentTab;
            OnTabChanged();
        }
    }
}