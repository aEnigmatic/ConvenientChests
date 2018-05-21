using System;
using ConvenientChests.CategorizeChests.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace ConvenientChests.StackToNearbyChests {
    public class StashToNearbyChestsModule : Module {
        private bool UseCategories => ModEntry.CategorizeChests.IsActive;

        private Func<Chest, Item, bool> AcceptingFunction
            => UseCategories
                   ? (chest, i) => ModEntry.CategorizeChests.ChestAcceptsItem(chest, i) || chest.ContainsItem(i)
                   : (Func<Chest, Item, bool>) ((chest, i) => chest.ContainsItem(i));

        public StashToNearbyChestsModule(ModEntry modEntry) : base(modEntry) { }

        public override void Activate() {
            IsActive = true;

            // Events
            ControlEvents.ControllerButtonPressed += OnControllerButtonPressed;
            ControlEvents.KeyPressed              += OnKeyPressed;
        }

        private void TryStashNearby() {
            if (ModEntry.CategorizeChests.IsActive && Game1.activeClickableMenu is ItemGrabMenu)
                // Stash to current chest takes priority
                return;

            StackLogic.StashToNearbyChests(Config.StashRadius, AcceptingFunction);
        }

        private void OnKeyPressed(object sender, EventArgsKeyPressed e) {
            if (e.KeyPressed == Config.StashKey)
                TryStashNearby();
        }

        private void OnControllerButtonPressed(object sender, EventArgsControllerButtonPressed e) {
            if (e.ButtonPressed.Equals(Config.StashButton))
                TryStashNearby();
        }
    }
}