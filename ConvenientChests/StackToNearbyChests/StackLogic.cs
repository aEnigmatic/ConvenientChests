using System;
using System.Collections.Generic;
using System.Linq;
using ConvenientChests.CategorizeChests.Framework;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;

namespace ConvenientChests.StackToNearbyChests
{
    public static class StackLogic
    {
        public static IEnumerable<Chest> getNearbyChests(this Farmer farmer, int radius)
            => getNearbyChests(farmer.currentLocation, farmer.getTileLocation(), radius);

        public static void StashToNearbyChests(int radius, Func<Chest, Item, bool> acceptingFunction)
        {
            if (Game1.player.currentLocation == null)
                return;

            var farmer = Game1.player;
            var items = farmer.Items
                .Where(i => i != null)
                .ToList();

            ModEntry.staticMonitor.Log("Stash to nearby");
//            ChestMod.staticMonitor.Log($"Trying to move: " + string.Join(", ", items.Select(i => i.Name)));

            
            var movedAtLeastOne = false;

            foreach (var chest in farmer.getNearbyChests(radius))
            {
//                ChestMod.staticMonitor.Log($"\tFound chest at {chest.TileLocation}");
                var moveItems = items
                    .Where(i => acceptingFunction(chest, i))
                    .ToList();

                if (!moveItems.Any())
                    continue;

//                ChestMod.staticMonitor.Log($"\t\tTrying to move: {string.Join(", ", moveItems.Select(i => i.Name))}");

                if (chest.dumpItemsToChest(moveItems).Any())
                    movedAtLeastOne = true;
            }

            if (!movedAtLeastOne)
                return;

            // List of sounds: https://gist.github.com/gasolinewaltz/46b1473415d412e220a21cb84dd9aad6
            Game1.playSound(Game1.soundBank.GetCue("pickUpItem").Name);
        }

        private static IEnumerable<Chest> getNearbyChests(GameLocation location, Vector2 point, int radius)
        {
            // chests
            foreach (var p in location.Objects.Pairs)
            {
                if (!(p.Value is Chest c))
                    continue;

                // c.TileLocation = p.Key * Game1.tileSize;
                if (inRadius(radius, point, p.Key))
                    yield return c;
            }

            // buildings
            if (!(location is BuildableGameLocation l))
                yield break;

            foreach (var building in l.buildings.Where(b => inRadius(radius, point, b.tileX.Value, b.tileY.Value)))
                if (building is JunimoHut junimoHut)
                    yield return junimoHut.output.Value;

                else if (building is Mill mill)
                    yield return mill.output.Value;
        }

        private static bool inRadius(int radius, Vector2 a, Vector2 b) => Math.Abs(a.X - b.X) < radius && Math.Abs(a.Y - b.Y) < radius;
        private static bool inRadius(int radius, Vector2 a, int x, int y) => Math.Abs(a.X - x) < radius && Math.Abs(a.Y - y) < radius;
    }
}