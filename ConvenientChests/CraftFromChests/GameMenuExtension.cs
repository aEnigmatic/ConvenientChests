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

            UnregisterTabEvent();
        }


        private static int _previousTab = -1;

        public static event EventHandler TabChanged;
        private static void OnTabChanged() => TabChanged?.Invoke(null, EventArgs.Empty);

        private static void OnPostRenderGuiEvent(object sender, EventArgs e)
        {
            switch (Game1.activeClickableMenu)
            {
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
                    OnTabChanged();
                    break;

                default:
                    // How did we get here?
                    ModEntry.staticMonitor.Log($"Unexpected menu: {Game1.activeClickableMenu?.GetType().ToString() ?? "null"}");
                    UnregisterTabEvent();
                    return;
            }
        }

        private static void UnregisterTabEvent()
        {
            GraphicsEvents.OnPostRenderGuiEvent -= OnPostRenderGuiEvent;
            _previousTab = -1;
        }
    }
}