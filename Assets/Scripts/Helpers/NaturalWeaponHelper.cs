using System.Collections.Generic;
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
            BaseItemType baseItem = new BaseItemType(
                tags, 
                "A claw, fist or psuedopod.", 
                "A claw, fist or psuedopod.", 
                "Natural Weapon", 
                "Natural Weapon", 
                new string[] { "Hand" }, 
                (wielderSize + 1) * 40.0f, 
                material, 
                new [] {"Martial Arts"}, 
                "strikes", 
                0, 
                0, 
                "None");

            List<IBasicValue<float>> values = new List<IBasicValue<float>>
            {
                new ConcreteBasicFloatValue("weight", baseItem.Weight),
                new ConcreteBasicFloatValue("size", baseItem.Size),
                new ConcreteBasicFloatValue("hardness", baseItem.Material.Hardness),
                new ConcreteBasicFloatValue("bonus", baseItem.Material.Bonus),
                new ConcreteBasicFloatValue("density", baseItem.Material.Density)
            };

            IItemInstance naturalWeapon = this.ItemFactory.CreateFromTemplate(baseItem, true);
            return naturalWeapon;
        }
    }
}