using System.Collections.Generic;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Entities.Statistics;

namespace JoyLib.Code.Helpers
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
            IItemMaterial itemMaterial = this.MaterialHandler.Get(material);
            BaseItemType baseItem = new BaseItemType(tags, "A claw, fist or psuedopod.", "A claw, fist or psuedopod.", "Natural Weapon", "Natural Weapon", new string[] { "Hand" }, 
                (wielderSize + 1) * 40.0f, itemMaterial, new [] {"Martial Arts"}, "strikes", 0, 0, "None");

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