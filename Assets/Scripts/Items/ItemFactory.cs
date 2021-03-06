using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using Godot;
using JoyGodot.Assets.Scripts.Collections;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Exceptions;
using JoyGodot.Assets.Scripts.Godot;
using JoyGodot.Assets.Scripts.Graphics;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Managed_Assets;
using JoyGodot.Assets.Scripts.Managers;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Scripts.Items
{
    public class ItemFactory : IItemFactory
    {
        protected IGameManager GameManager { get; set; }

        protected IItemDatabase ItemDatabase { get; set; }
        protected ILiveItemHandler ItemHandler { get; set; }

        protected IObjectIconHandler ObjectIcons { get; set; }

        protected IDerivedValueHandler DerivedValueHandler { get; set; }
        
        protected GUIDManager GuidManager { get; set; }

        protected GameObjectPool<JoyObjectNode> ItemPool { get; set; }

        protected RNG Roller { get; set; }

        public ItemFactory(
            GUIDManager guidManager,
            IItemDatabase itemDatabase,
            ILiveItemHandler itemHandler,
            IObjectIconHandler objectIconHandler,
            IDerivedValueHandler derivedValueHandler,
            GameObjectPool<JoyObjectNode> itemPool,
            RNG roller = null)
        {
            this.GuidManager = guidManager;
            this.ItemDatabase = itemDatabase;
            this.ItemHandler = itemHandler;
            this.ObjectIcons = objectIconHandler;
            this.DerivedValueHandler = derivedValueHandler;
            this.ItemPool = itemPool;
            this.Roller = roller is null ? new RNG() : roller;
        }

        public IItemInstance CreateRandomItemOfType(string[] tags, bool identified = false)
        {
            BaseItemType[] matchingTypes = this.ItemDatabase.FindItemsOfType(tags, tags.Length).ToArray();
            if (matchingTypes.Length > 0)
            {
                int result = this.Roller.Roll(0, matchingTypes.Length);
                BaseItemType itemType = matchingTypes[result];

                IItemInstance itemInstance = this.CreateFromTemplate(itemType, identified);

                this.ItemHandler.Add(itemInstance);
                return itemInstance;
            }

            return null;
        }

        public IItemInstance CreateSpecificType(string name, string[] tags, bool identified = false)
        {
            BaseItemType[] matchingTypes = this.ItemDatabase.FindItemsOfType(tags).ToArray();
            List<BaseItemType> secondRound = new List<BaseItemType>();
            foreach (BaseItemType itemType in matchingTypes)
            {
                if (identified == false)
                {
                    if (itemType.UnidentifiedName.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                    {
                        secondRound.Add(itemType);
                    }
                }
                else
                {
                    if (itemType.IdentifiedName.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                    {
                        secondRound.Add(itemType);
                    }
                }
            }

            if (secondRound.Count > 0)
            {
                int result = this.Roller.Roll(0, secondRound.Count);
                BaseItemType itemType = secondRound[result];

                IItemInstance itemInstance = this.CreateFromTemplate(itemType, identified);

                this.ItemHandler.Add(itemInstance);
                return itemInstance;
            }

            throw new ItemTypeNotFoundException(name, "Could not find an item type by the name of " + name);
        }

        public IItemInstance CreateRandomWeightedItem(
            bool identified = false,
            bool withAbility = false)
        {
            var weights = this.ItemDatabase.ItemWeights;

            int totalWeight = weights.Values.Sum();
            int result = this.Roller.Roll(0, totalWeight);

            int total = 0;
            BaseItemType chosenType = null;
            foreach (var pair in weights)
            {
                total += pair.Value;

                if (total > result)
                {
                    chosenType = this.ItemDatabase
                        .GetAllForName(pair.Key)
                        .ToArray()
                        .GetRandom();
                    break;
                }
            }

            if (chosenType is null)
            {
                return null;
            }

            IItemInstance itemInstance = this.CreateFromTemplate(chosenType, identified);

            this.ItemHandler.Add(itemInstance);

            if (identified)
            {
                itemInstance.IdentifyMe();
            }
            return itemInstance;
        }

        public IItemInstance CreateFromTemplate(BaseItemType itemType, bool identified = false)
        {
            List<IBasicValue<float>> values = new List<IBasicValue<float>>
            {
                new ConcreteBasicFloatValue(
                    "weight", itemType.Weight),
                new ConcreteBasicFloatValue(
                    "bonus", itemType.BaseEfficiency),
                new ConcreteBasicFloatValue(
                    "size", itemType.Size),
                new ConcreteBasicFloatValue(
                    "hardness", itemType.Materials.Average(pair => pair.Item1.Hardness)),
                new ConcreteBasicFloatValue(
                    "density", itemType.Materials.Average(pair => pair.Item1.Density))
            };

            List<SpriteState> states = (from sprite in this.ObjectIcons.GetManagedSprites(
                        itemType.SpriteSheet,
                        itemType.UnidentifiedName)
                    select new SpriteState(
                        sprite.Name, 
                        itemType.SpriteSheet,
                        sprite))
                .ToList();

            ISpriteState chosenState = this.Roller.SelectFromCollection(states);
            IDictionary<string, Color> colours = itemType.Materials
                .Select(tuple => tuple.Item1)
                .Distinct()
                .ToDictionary(
                itemMaterial => itemMaterial.Name, 
                itemMaterial => itemMaterial.Colours.GetRandom());
            foreach (var component in itemType.Components)
            {
                if (colours.ContainsKey(component.IdentifiedName) == false)
                {
                    colours.Add(component.IdentifiedName, component.Materials.Keys.GetRandom().Colours.GetRandom());
                }
            }
            chosenState.OverrideColours(colours);

            ItemInstance itemInstance = new ItemInstance(
                this.GuidManager.AssignGUID(),
                itemType,
                this.DerivedValueHandler.GetItemStandardBlock(values),
                new Vector2Int(-1, -1),
                identified, 
                new[] { chosenState },
                new RNG(),
                new List<IAbility>(),
                new List<IJoyAction>());

            return itemInstance;
        }
    }
}