using System;
using ConvenientChests.CategorizeChests.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace ConvenientChests.StackToNearbyChests
{
    public class StashToNearbyChestsModule : Module
    {
        private bool useCategories => modEntry.CategorizeChests.isActive;

        private Func<Chest, Item, bool> acceptingFunction => useCategories
            ? (chest, i) => modEntry.CategorizeChests.ChestAcceptsItem(chest, i) || chest.containsItem(i)
            : (Func<Chest, Item, bool>) ((chest, i) => chest.containsItem(i));

        public StashToNearbyChestsModule(ModEntry modEntry) : base(modEntry)
        {
        }

        public override void activate()
        {
            isActive = true;

            // Events
            ControlEvents.ControllerButtonPressed += onControllerButtonPressed;
            ControlEvents.KeyPressed += onKeyPressed;
        }

        private void TryStashNearby()
        {
            if (Game1.activeClickableMenu is ItemGrabMenu)
                // Stash to current chest takes priority
                return;

            StackLogic.StashToNearbyChests(Config.StashRadius, acceptingFunction);
        }

        private void onKeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (e.KeyPressed == Config.StashKey) TryStashNearby();
        }

        private void onControllerButtonPressed(object sender, EventArgsControllerButtonPressed e)
        {
            if (e.ButtonPressed.Equals(Config.StashButton)) TryStashNearby();
        }
    }
}