using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Entities.Abilities;

namespace JoyGodot.Assets.Scripts.Entities.Items
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
        public IEnumerable<string> materials;
        public IEnumerable<string> tags;
        public float size;
        public IEnumerable<string> slots;
        public string spriteSheet;
        public int range;

        public IdentifiedItem(
            string nameRef, 
            IEnumerable<string> tagsRef, 
            string descriptionRef, 
            int valueRef, 
            IEnumerable<IAbility> abilitiesRef, 
            int weightingRef,
            IEnumerable<string> skills, 
            IEnumerable<string> materialsRef, 
            float sizeRef, 
            IEnumerable<string> slotsRef, 
            string spriteSheetRef, 
            int range = 1,
            int lightLevelRef = 0)
        {
            this.name = nameRef;
            this.tags = tagsRef;
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