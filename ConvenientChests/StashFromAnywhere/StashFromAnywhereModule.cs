using System.Collections.Generic;
using ConvenientChests.CategorizeChests.Framework;
using ConvenientChests.StackToNearbyChests;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using Utility = ConvenientChests.CategorizeChests.Utility;

namespace ConvenientChests.StashFromAnywhere
{
    public class StashFromAnywhereModule : Module
    {
        private StackLogic.AcceptingFunction AcceptingFunction { get; set; }

        public StashFromAnywhereModule(ModEntry modEntry) : base(modEntry)
        {
        }

        private StackLogic.AcceptingFunction CreateAcceptingFunction()
        {
            if (Config.StashAnywhere && Config.StashAnywhereToExistingStacks)
            {
                return (chest, item) => ModEntry.CategorizeChests.ChestAcceptsItem(chest, item) || chest.ContainsItem(item);
            }
            
            if (Config.StashAnywhere)
            {
                return (chest, item) => ModEntry.CategorizeChests.ChestAcceptsItem(chest, item);
            }

            return (chest, item) => false;
        }

        public override void Activate()
        {
            IsActive = true;

            AcceptingFunction = CreateAcceptingFunction();

            this.Events.Input.ButtonPressed += OnButtonPressed;
        }

        public override void Deactivate()
        {
            IsActive = false;
            this.Events.Input.ButtonPressed -= OnButtonPressed;
        }

        private void StashGlobally()
        {
            if (Game1.player.currentLocation == null)
                return;

            var locations = Utility.GetLocations();

            foreach (var location in locations)
            {
                foreach (KeyValuePair<Vector2, Object> pair in location.Objects.Pairs)
                {
                    if (pair.Value is Chest chest)
                    {
                        StackLogic.StashToChest(chest, AcceptingFunction);
                    }
                }
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == Config.StashAnywhereKey)
                StashGlobally();
        }
    }
}