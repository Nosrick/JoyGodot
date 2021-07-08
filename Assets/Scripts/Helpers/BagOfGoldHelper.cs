using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Items;

namespace JoyGodot.Assets.Scripts.Helpers
{
    public class BagOfGoldHelper
    {
        private static IItemFactory ItemFactory { get; set; }
        
        private static ILiveItemHandler ItemHandler { get; set; }

        public BagOfGoldHelper(ILiveItemHandler itemHandler, IItemFactory itemFactory)
        {
            ItemFactory = itemFactory;
            ItemHandler = itemHandler;
        }

        public IItemInstance GetBagOfGold(int count)
        {
            if (GlobalConstants.GameManager is null == false && ItemHandler is null)
            {
                ItemHandler = GlobalConstants.GameManager.ItemHandler;
                ItemFactory = GlobalConstants.GameManager.ItemFactory;
            }
            
            IItemInstance bag = ItemFactory.CreateRandomItemOfType(new[] { "container"}, true);
            List<IItemInstance> coins = new List<IItemInstance>();
            int gold = count / 100;
            int silver = (count - (gold * 100))  / 10;
            int copper = (count - (gold * 100) - (silver * 10));

            if (gold > 0)
            {
                IItemInstance goldCoin = ItemFactory.CreateSpecificType("gold coin", new[] {"currency"}, true);
                for (int i = 0; i < gold; i++)
                {
                    coins.Add(goldCoin.Copy(goldCoin));
                }
            }
            
            if (silver > 0)
            {
                IItemInstance silverCoin = ItemFactory.CreateSpecificType("silver coin", new[] {"currency"}, true);
                for (int i = 0; i < silver; i++)
                {
                    coins.Add(silverCoin.Copy(silverCoin));
                }
            }

            if (copper > 0)
            {
                IItemInstance copperCoin = ItemFactory.CreateSpecificType("copper coin", new[] {"currency"}, true);
                for (int i = 0; i < copper; i++)
                {
                    coins.Add(copperCoin.Copy(copperCoin));
                }
            }
            
            bag.AddContents(coins);

            return bag;
        }
    }
}
