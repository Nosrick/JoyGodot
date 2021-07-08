using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Rollers;

namespace JoyGodot.Assets.Scripts.World.Generators.Interiors
{
    public class DungeonItemPlacer
    {
        protected IItemFactory ItemFactory { get; set; }

        protected ILiveItemHandler ItemHandler { get; set; }
        
        protected RNG Roller { get; set; }

        public DungeonItemPlacer(
            ILiveItemHandler itemHandler,
            RNG roller,
            IItemFactory itemFactory)
        {
            this.Roller = roller;
            this.ItemHandler = itemHandler;
            this.ItemFactory = itemFactory;
        }

        /// <summary>
        /// Places items in the dungeon
        /// </summary>
        /// <param name="worldRef">The world in which to place the items</param>
        /// <param name="prosperity">The prosperity of the world, the lower the better</param>
        /// <returns>The items placed</returns>
        public List<IItemInstance> PlaceItems(IWorldInstance worldRef, int prosperity = 50)
        {
            List<IItemInstance> placedItems = new List<IItemInstance>();

            int dungeonArea = worldRef.Tiles.GetLength(0) * worldRef.Tiles.GetLength(1);
            int itemsToPlace = dungeonArea / prosperity;

            List<Vector2Int> availablePoints = new List<Vector2Int>();

            for (int i = 0; i < worldRef.Tiles.GetLength(0); i++)
            {
                for (int j = 0; j < worldRef.Tiles.GetLength(1); j++)
                {
                    Vector2Int position = new Vector2Int(i, j);
                    if (worldRef.Walls.Contains(position) == false
                        && position != worldRef.SpawnPoint
                        && worldRef.Areas.ContainsKey(position) == false)
                    {
                        availablePoints.Add(position);
                    }
                }
            }

            for(int i = 0; i < itemsToPlace; i++)
            {
                Vector2Int point = availablePoints[this.Roller.Roll(0, availablePoints.Count)];

                IItemInstance item = this.ItemFactory.CreateRandomWeightedItem();
                worldRef.AddItem(item);
                item.Move(point);
                placedItems.Add(item);
            }

            return placedItems;
        }


    }
}
