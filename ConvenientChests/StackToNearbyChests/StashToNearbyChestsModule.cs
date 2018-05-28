using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace ConvenientChests.StackToNearbyChests {
    public class StashToNearbyChestsModule : Module {
        public StashToNearbyChestsModule(ModEntry modEntry) : base(modEntry) { }

        public override void Activate() {
            IsActive = true;

            // Events
            ControlEvents.ControllerButtonPressed += OnControllerButtonPressed;
            ControlEvents.KeyPressed              += OnKeyPressed;
        }

        public override void Deactivate() {
            IsActive = false;

            // Events
            ControlEvents.ControllerButtonPressed -= OnControllerButtonPressed;
            ControlEvents.KeyPressed              -= OnKeyPressed;
        }

        private void TryStashNearby() {
            if (Game1.player.currentLocation == null)
                return;

            if (Game1.activeClickableMenu is ItemGrabMenu m && m.behaviorOnItemGrab?.Target is Chest c)
                StackLogic.StashToChest(c);
            
            else
                StackLogic.StashToNearbyChests(Config.StashRadius);
        }

        private void OnKeyPressed(object sender, EventArgsKeyPressed e) {
            if (e.KeyPressed == Config.StashKey) TryStashNearby();
        }

        private void OnControllerButtonPressed(object sender, EventArgsControllerButtonPressed e) {
            if (e.ButtonPressed.Equals(Config.StashButton)) TryStashNearby();
        }
    }
}