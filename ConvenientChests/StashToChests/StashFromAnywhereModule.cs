using System.Linq;
using ConvenientChests.CategorizeChests.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using Utility = ConvenientChests.CategorizeChests.Utility;

namespace ConvenientChests.StashToChests
{
    public class StashFromAnywhereModule : Module
    {
        private StackLogic.AcceptingFunction CategorizedAcceptingFunction { get; set; }
        private StackLogic.AcceptingFunction ExistingStacksAcceptingFunction { get; set; }

        public StashFromAnywhereModule(ModEntry modEntry) : base(modEntry)
        {
        }

        private StackLogic.AcceptingFunction CreateCategorizedAcceptingFunction()
        {
            if (Config.StashAnywhere)
            {
                return (chest, item) => ModEntry.CategorizeChests.ChestAcceptsItem(chest, item);
            }

            return (chest, item) => false;
        }

        private StackLogic.AcceptingFunction CreateExistingStacksAcceptingFunction()
        {
            if (Config.StashAnywhere && Config.StashAnywhereToExistingStacks)
            {
                return (chest, item) => chest.ContainsItem(item);
            }
            
            return (chest, item) => false;
        }

        public override void Activate()
        {
            IsActive = true;

            CategorizedAcceptingFunction = CreateCategorizedAcceptingFunction();
            ExistingStacksAcceptingFunction = CreateExistingStacksAcceptingFunction();

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

            var locations = Utility.GetLocations().ToList();

            foreach (var pair in locations.SelectMany(location => location.Objects.Pairs))
            {
                if (pair.Value is Chest chest)
                {
                    StackLogic.StashToChest(chest, CategorizedAcceptingFunction);
                }
            }
            
            foreach (var pair in locations.SelectMany(location => location.Objects.Pairs))
            {
                if (pair.Value is Chest chest)
                {
                    StackLogic.StashToChest(chest, ExistingStacksAcceptingFunction);
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