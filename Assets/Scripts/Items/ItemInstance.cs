using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Collections;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Events;
using JoyGodot.Assets.Scripts.Godot;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Managed_Assets;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.Scripting;
using JoyGodot.Assets.Scripts.World;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.Items
{
    public class ItemInstance : JoyObject.JoyObject, IItemInstance
    {
        protected const string DURABILITY = "durability";

        protected IEntity User { get; set; }
        
         
        protected bool m_Identified;

         
        protected List<Guid> m_Contents;
        
         
        protected BaseItemType m_Type;

         
        protected Guid m_OwnerGuid;
        
         
        protected string m_OwnerString;
        
        protected int StateIndex { get; set; }

        public override IEnumerable<string> Tags
        {
            get
            {
                List<string> tags = new List<string>(this.m_Tags);
                tags.AddRange(this.ItemType.Tags);
                tags.AddRange(this.ItemType.Materials.SelectMany(pair => pair.Item1.Tags));
                tags = tags.Distinct().ToList();
                
                return tags;
            }

            protected set => this.m_Tags = new List<string>(value);
        }

        public override IWorldInstance MyWorld
        {
            get => this.m_World;
            set
            {
                this.m_World = value;
                foreach (IItemInstance content in this.Contents)
                {
                    content.MyWorld = value;
                }
            }
        }

        protected IWorldInstance m_World;

        public Guid OwnerGUID
        {
            get
            {
                return this.m_OwnerGuid;
            }
            protected set
            {
                this.m_OwnerGuid = value;
                this.m_OwnerString = this.EntityHandler?.Get(this.m_OwnerGuid)?.JoyName;
            }
        }

        public string OwnerString
        {
            get => this.m_OwnerString;
        }

        public bool Identified
        {
            get => this.m_Identified;
            protected set => this.m_Identified = value;
        }

        public bool Broken => this.GetValue(DerivedValueName.DURABILITY) <= 0;

        public override ICollection<string> Tooltip => this.ConstructDescription();

        public int Efficiency => (int)(this.m_Type.BaseEfficiency * (this.GetValue(DURABILITY) / (float)this.GetMaximum(DURABILITY)));

        public string ConditionString
        {
            get
            {
                float durability = this.GetValue(DURABILITY);
                float maximum = this.GetMaximum(DURABILITY);
                if (durability == 0)
                {
                    return "Broken";
                }
                if (durability / maximum < 0.25)
                {
                    return "In poor condition";
                }

                if (durability / maximum < 0.5)
                {
                    return "In fair condition";
                }
                if (durability / maximum < 0.75)
                {
                    return "In good condition";
                }

                return "In great condition";
            }
        }

        public string SlotString
        {
            get
            {
                if (this.ItemType.Slots.Contains("None") || this.ItemType.Slots.IsNullOrEmpty())
                {
                    return "This item can be thrown";
                }
                return "This is equipped to " + string.Join(", ", this.ItemType.Slots);
            }
        }

        public string DisplayName => this.Identified ? this.m_Type.IdentifiedName : this.m_Type.UnidentifiedName;

        public string IdentifiedName => this.m_Type.IdentifiedName;

        public string DisplayDescription => this.Identified ? this.m_Type.Description : this.m_Type.UnidentifiedDescription;

        public float Weight
        {
            get
            {
                return this.m_Type.Weight + this.Contents.Sum(item => item.Weight);
            }
        }

         
        public bool InWorld
        {
            get;
            set;
        }

        public string WeightString 
        {
            get
            {
                const string weight = "Weighs ";
                if (this.Weight < 1000)
                {
                    return weight + this.Weight + " grams";
                }
                return weight + (this.Weight / 1000f) + " kilograms";
            }
        }

        public IEnumerable<IItemInstance> Contents => this.ItemHandler?.GetItems(this.m_Contents) ?? new IItemInstance[0];

        public string ContentString
        {
            get
            {
                if (this.ContentsDirty == false)
                {
                    return this.CachedContentString;
                }
                
                string contentString = "It contains ";

                List<IItemInstance> items = this.Contents.ToList();
                if (items.Any() == false)
                {
                    contentString = "";
                    return contentString;
                }

                IDictionary<string, int> occurrences = new System.Collections.Generic.Dictionary<string, int>();
                foreach (IItemInstance item in items)
                {
                    if (occurrences.ContainsKey(item.JoyName))
                    {
                        occurrences[item.JoyName] += 1;
                    }
                    else
                    {
                        occurrences.Add(item.JoyName, 1);
                    }
                }

                IEnumerable<string> itemNames = occurrences.Select(pair => pair.Value > 1 ? pair.Key + "s" : pair.Key);
                contentString += string.Join(", ", itemNames);

                this.CachedContentString = contentString;
                this.ContentsDirty = false;
                return contentString;
            }
        }
        
        protected bool ContentsDirty { get; set; }
        
        protected string CachedContentString { get; set; }

        public event ItemRemovedEventHandler ItemRemoved;
        public event ItemAddedEventHandler ItemAdded;

        public IEnumerable<IAbility> AllAbilities
        {
            get
            {
                List<IAbility> abilities = new List<IAbility>();
                abilities.AddRange(this.UniqueAbilities);
                abilities.AddRange(this.ItemType.Abilities);
                return abilities;
            }
        }

        public BaseItemType ItemType => this.m_Type;

        public int Value
        {
            get
            {
                if (this.ContentsDirty)
                {
                    this.m_CachedValue = this.ItemType.Value + this.Contents.Sum(item => item.Value);
                }

                return this.m_CachedValue;
            }
        }
        protected int m_CachedValue;
        
        public IEnumerable<IAbility> UniqueAbilities { get; protected set; }
        
        public ILiveItemHandler ItemHandler { get; set; }
        
        public ILiveEntityHandler EntityHandler { get; set; }
        
        public ItemInstance()
        {
            this.Data = new NonUniqueDictionary<string, object>();
            this.ItemHandler = GlobalConstants.GameManager.ItemHandler;
            this.EntityHandler = GlobalConstants.GameManager.EntityHandler; 
        }

        public ItemInstance(
            Guid guid,
            BaseItemType type, 
            IDictionary<string, IDerivedValue> derivedValues,
            Vector2Int position, 
            bool identified, 
            IEnumerable<ISpriteState> sprites,
            IRollable roller = null,
            IEnumerable<IAbility> uniqueAbilities = null,
            IEnumerable<IJoyAction> actions = null,
            List<IItemInstance> contents = null)
            : base(
                type.UnidentifiedName, 
                guid,
                derivedValues,
                position,
                actions,
                sprites,
                type.SpriteSheet,
                roller,
                type.Tags.ToArray())
        {
            this.m_Type = type;
            
            this.Identified = identified;

            this.m_Contents = contents is null ? new List<Guid>() : contents.Select(instance => instance.Guid).ToList();

            this.UniqueAbilities = uniqueAbilities is null == false ? new List<IAbility>(uniqueAbilities) : new List<IAbility>();

            this.ItemHandler = GlobalConstants.GameManager.ItemHandler;
            this.EntityHandler = GlobalConstants.GameManager.EntityHandler;

            this.SetMaterialColours();

            if (this.States.Count > 1)
            {
                this.StateIndex = this.Roller.Roll(0, this.States.Count);
            }
            else
            {
                this.StateIndex = 0;
            }
        }

        protected void SetMaterialColours()
        {
            IDictionary<string, Color> parts =
                new System.Collections.Generic.Dictionary<string, Color>();
            foreach (var component in this.ItemType.Components)
            {
                foreach (ISpriteState state in this.States)
                {
                    var part = state.SpriteData.Parts.FirstOrDefault(p =>
                        p.m_Name.Equals(component.UnidentifiedName, StringComparison.OrdinalIgnoreCase));
                    if (part is null)
                    {
                        continue;
                    }

                    if (component.Materials.Keys.Any(material => material.Colours.Any()) == false)
                    {
                        continue;
                    }

                    if (parts.ContainsKey(component.UnidentifiedName) == false)
                    {
                        part.m_PossibleColours = new List<Color>
                        {
                            component.Materials.Keys
                                .SelectMany(material => material.Colours)
                                .ToArray()
                                .GetRandom()

                        };
                        part.m_SelectedColour = this.Roller.Roll(0, part.m_PossibleColours.Count);
                        parts.Add(component.UnidentifiedName, part.SelectedColour);
                    }
                    else
                    {
                        part.m_PossibleColours = new List<Color> { parts[component.UnidentifiedName] };
                        part.m_SelectedColour = 0;
                    }
                }
            }

            foreach (var material in this.ItemType.MyMaterials.Keys)
            {
                foreach (ISpriteState state in this.States)
                {
                    var part = state.SpriteData.Parts.FirstOrDefault(p =>
                        material.HasTag(p.m_Name)
                        || material.Name.Equals(p.m_Name, StringComparison.OrdinalIgnoreCase)
                        || p.m_Name.Equals(this.ItemType.UnidentifiedName, StringComparison.OrdinalIgnoreCase));
                    if (part is null)
                    {
                        continue;
                    }

                    if (parts.ContainsKey(material.Name))
                    {
                        part.m_PossibleColours = new List<Color> { parts[material.Name] };
                        part.m_SelectedColour = 0;
                    }
                    else
                    {
                        part.m_PossibleColours = new List<Color>(material.Colours);
                        part.m_SelectedColour = this.Roller.Roll(0, part.m_PossibleColours.Count);
                        parts.Add(material.Name, part.SelectedColour);
                    }
                }
            }
        }

        public void Instantiate(bool recursive = true, JoyObjectNode gameObject = null, bool active = false)
        {
            var joyObjectNode = gameObject ?? GlobalConstants.GameManager.ItemPool.Get();
            joyObjectNode.AttachJoyObject(this);
            this.MyNode = joyObjectNode;
            
            this.MyNode.Clear();
            this.MyNode.AddSpriteState(this.States[this.StateIndex]);
            if (active)
            {
                this.MyNode.Show();
            }
            else
            {
                this.MyNode.Hide();
            }

            if (!recursive)
            {
                return;
            }
            
            foreach (IItemInstance item in this.Contents)
            {
                if (item is ItemInstance instance)
                {
                    instance.Instantiate(true, GlobalConstants.GameManager.ItemPool.Get(), active);
                }
            }
        }

        public IItemInstance Copy(IItemInstance copy)
        {
            ItemInstance newItem = new ItemInstance(
                copy.Guid,
                copy.ItemType,
                copy.DerivedValues,
                copy.WorldPosition,
                copy.Identified,
                copy.States,
                copy.Roller,
                copy.UniqueAbilities,
                copy.CachedActions.ToArray());

            this.ItemHandler.Add(newItem);
            return newItem;
        }

        protected List<string> ConstructDescription()
        {
            List<string> data = new List<string>
            {
                this.ConditionString,
                this.WeightString,
                this.ItemType.MaterialDescription,
                this.SlotString
            };
            if (this.ContentString.IsNullOrEmpty() == false)
            {
                data.Add(this.ContentString);
            }
            
            if (this.OwnerString.IsNullOrEmpty() == false)
            {
                data.Add("Owned by " + this.OwnerString);
            }
            else
            {
                data.Add("Not owned");
            }
            
            data.Add("Worth " + this.Value);

            return data;
        }
        
        public void SetUser(IEntity user)
        {
            this.User = user;
        }

        public void Use(string abilityName)
        {
            var ability = this.AllAbilities.FirstOrDefault(a =>
                a.Name.Equals(abilityName, StringComparison.OrdinalIgnoreCase)
                || a.InternalName.Equals(abilityName, StringComparison.OrdinalIgnoreCase));

            if (ability is null)
            {
                return;
            }

            ability.OnUse(this.User, this);

            foreach (IAbility userAbility in this.User.Abilities)
            {
                userAbility.OnUse(this.User, this);
            }
        }
        
        public void SetOwner(Guid newOwner, bool recursive = false)
        {
            this.OwnerGUID = newOwner;

            if (recursive)
            {
                foreach (IItemInstance item in this.Contents)
                {
                    item.SetOwner(newOwner, true);
                }
            }
        }

        public new bool HasTag(string tag)
        {
            return this.Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        }

        public override void Move(Vector2Int newPosition)
        {
            base.Move(newPosition);

            foreach (IItemInstance item in this.Contents)
            {
                item.Move(newPosition);
            }
        }

        public void Interact(IEntity user, string ability)
        {
            this.SetUser(user);

            this.Use(ability);

            if(!this.Identified)
            {
                this.IdentifyMe();
                user.AddIdentifiedItem(this.DisplayName);
            }
        }

        public void IdentifyMe()
        {
            this.Identified = true;
            this.JoyName = this.IdentifiedName;

            var others = this.ItemHandler.Values.Where(instance =>
                instance.IdentifiedName.Equals(this.IdentifiedName, StringComparison.OrdinalIgnoreCase)
                && instance.Identified == false);

            foreach (IItemInstance other in others)
            {
                other.IdentifyMe();
            }
        }

        public bool Contains(IItemInstance actor)
        {
            if (this.m_Contents.Contains(actor.Guid))
            {
                return true;
            }

            bool result = false;
            IEnumerable<IItemInstance> items = this.Contents.Where(instance => instance.HasTag("container"));
            foreach (IItemInstance c in items)
            {
                result |= c.Contains(actor);
                if (result)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanAddContents(IItemInstance actor)
        {
            if (actor is null)
            {
                return true;
            }
            
            return actor.Guid != this.Guid && !this.Contains(actor) && !actor.Contains(this);
        }

        public bool AddContents(IItemInstance actor)
        {
            if (actor is null)
            {
                return true;
            }
            
            if (!this.CanAddContents(actor))
            {
                return false;
            }
            this.m_Contents.Add(actor.Guid);

            this.ContentsDirty = true;

            actor.InWorld = false;
                
            this.ItemAdded?.Invoke(this, actor);
            return true;
        }

        public bool AddContents(IEnumerable<IItemInstance> actors)
        {
            return actors.Aggregate(true, (current, actor) => current & this.AddContents(actor));
        }

        public bool CanRemoveContents(IItemInstance actor)
        {
            return actor is null || this.Contains(actor);
        }

        public bool CanRemoveContents(IEnumerable<IItemInstance> actors)
        {
            return actors.Aggregate(true, (current, actor) => current & this.CanRemoveContents(actor));
        }

        public bool RemoveContents(IItemInstance actor)
        {
            if (actor is null)
            {
                return true;
            }
            
            if (!this.m_Contents.Remove(actor.Guid))
            {
                return false;
            }

            this.ContentsDirty = true;
            this.ItemRemoved?.Invoke(this, actor);

            return true;
        }

        public virtual bool RemoveContents(IEnumerable<IItemInstance> actors)
        {
            return actors.Aggregate(true, (current, actor) => current & this.RemoveContents(actor));
        }

        public void Clear()
        {
            List<IItemInstance> copy = new List<IItemInstance>(this.Contents);
            foreach (IItemInstance item in copy)
            {
                this.RemoveContents(item);
            }
        }

        public override Dictionary Save()
        {
            Dictionary saveDict = base.Save();
            
            saveDict.Add("ItemType", this.ItemType.Save());
            saveDict.Add("Contents", new Array(this.m_Contents.Select(guid => guid.ToString())));
            saveDict.Add("Owner", this.OwnerGUID.ToString());
            saveDict.Add("InWorld", this.InWorld);
            saveDict.Add("UniqueAbilities", new Array(this.UniqueAbilities.Select(ability => ability.Save())));

            return saveDict;
        }

        public override void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.ItemHandler.ValueExtractor;
            
            base.Load(data);
            
            Dictionary itemType = valueExtractor.GetValueFromDictionary<Dictionary>(data, "ItemType");
            this.m_Type = new BaseItemType();
            this.m_Type.Load(itemType);
            
            this.m_Contents = new List<Guid>(
                valueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Contents")
                    .Select(s => new Guid(s)));
            string owner = valueExtractor.GetValueFromDictionary<string>(data, "Owner");
            this.OwnerGUID = owner is null ? Guid.Empty : new Guid(owner);
            this.InWorld = valueExtractor.GetValueFromDictionary<bool>(data, "InWorld");
            this.UniqueAbilities = new List<IAbility>(
                valueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "UniqueAbilities")
                    .Select(s => GlobalConstants.GameManager.AbilityHandler.Get(s)));

            this.ContentsDirty = true;

            this.SetMaterialColours();
        }
    }
}
