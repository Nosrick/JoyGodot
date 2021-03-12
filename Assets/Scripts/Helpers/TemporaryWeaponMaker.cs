using JoyLib.Code.Entities.Items;

namespace JoyLib.Code.Helpers
{
    public static class TemporaryWeaponMaker
    {
        /*
        private static IItemFactory ItemFactory { get; set; }

        private static void Initialise()
        {
            ItemFactory = GlobalConstants.GameManager.ItemFactory;
        }

        //Meant for making things like magic blasts that will never actually appear in the world.
        public static IItemInstance Make(int size, string actionString, string skill, string weaponName = "temporary weapon", string materialName = "magic", params string[] tags)
        {
            ItemMaterial material = new ItemMaterial(materialName, 1, 0, 1, 0.0f);

            BaseItemType tempItem = new BaseItemType(
                tags, 
                "", 
                weaponName, 
                weaponName,
                weaponName,
                new string[] { "None" }, 
                size, 
                material, 
                new []{skill}, 
                actionString, 
                0, 
                0, 
                "None");

            IItemInstance temporary = ItemFactory.CreateFromTemplate(tempItem, true);

            return temporary;
        }
        */
    }
}
