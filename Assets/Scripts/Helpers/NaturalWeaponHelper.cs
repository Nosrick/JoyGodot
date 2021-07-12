using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Items;

namespace JoyGodot.Assets.Scripts.Helpers
{
    public class NaturalWeaponHelper
    {
        protected IMaterialHandler MaterialHandler { get; set; }
        
        protected IItemFactory ItemFactory { get; set; }

        public NaturalWeaponHelper(
            IMaterialHandler materialHandler,
            IItemFactory itemFactory)
        {
            this.MaterialHandler = materialHandler;
            this.ItemFactory = itemFactory;
        }

        public IItemInstance MakeNaturalWeapon(int wielderSize, string material = "flesh", params string[] tags)
        {
            float size = (wielderSize + 1) * 40.0f;

            var m = GlobalConstants.GameManager.MaterialHandler.Get(material);
            
            BaseItemType baseItem = new BaseItemType(
                tags, 
                "A claw, fist or psuedopod.", 
                "A claw, fist or psuedopod.", 
                "Natural Weapon", 
                "Natural Weapon", 
                new string[] { "Hand" }, 
                size, 
                new Dictionary<IItemMaterial, int>
                {
                    {m, (int) size}
                }, 
                new [] {"Martial Arts"}, 
                "strikes", 
                0, 
                "None");

            List<IBasicValue<float>> values = new List<IBasicValue<float>>
            {
                new ConcreteBasicFloatValue("weight", baseItem.Weight),
                new ConcreteBasicFloatValue("size", baseItem.Size),
                new ConcreteBasicFloatValue("hardness", baseItem.Materials.First().Key.Hardness),
                new ConcreteBasicFloatValue("bonus", baseItem.BaseEfficiency),
                new ConcreteBasicFloatValue("density", baseItem.Materials.First().Key.Density)
            };

            IItemInstance naturalWeapon = this.ItemFactory.CreateFromTemplate(baseItem, true);
            return naturalWeapon;
        }
    }
}