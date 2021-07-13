using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Scripts.Items
{
    public struct IdentifiedItem
    {
        public string name;
        public string description;
        public int value;
        public IEnumerable<IAbility> abilities;
        public int weighting;
        public int lightLevel;
        public IEnumerable<string> skills;
        public IDictionary<string, int> materials;
        public IEnumerable<string> tags;
        public float size;
        public IEnumerable<string> slots;
        public string spriteSheet;
        public int range;
        public IEnumerable<BaseItemType> components;

        public IdentifiedItem(
            string nameRef, 
            IEnumerable<string> tagsRef, 
            string descriptionRef, 
            int valueRef, 
            IEnumerable<IAbility> abilitiesRef, 
            int weightingRef,
            IEnumerable<string> skills, 
            IDictionary<string, int> materialsRef, 
            float sizeRef, 
            IEnumerable<string> slotsRef, 
            string spriteSheetRef, 
            int range = 1,
            IEnumerable<BaseItemType> componentsRef = null,
            int lightLevelRef = 0)
        {
            this.name = nameRef;
            this.tags = tagsRef;
            this.components = componentsRef.IsNullOrEmpty() ? new BaseItemType[0] : componentsRef;
            this.description = descriptionRef;
            this.value = valueRef;
            this.abilities = abilitiesRef;
            this.weighting = weightingRef;
            this.skills = skills;
            this.materials = materialsRef;
            this.size = sizeRef;
            this.slots = slotsRef;
            this.spriteSheet = spriteSheetRef;
            this.lightLevel = lightLevelRef;
            this.range = range;
        }
    }

    public struct UnidentifiedItem
    {
        public string name;
        public string description;
        public string identifiedName;

        public UnidentifiedItem(string nameRef, string descriptionRef,
             string identifiedName)
        {
            this.name = nameRef;
            this.description = descriptionRef;
            this.identifiedName = identifiedName;
        }
    }
}