using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Collections;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Events;
using JoyGodot.Assets.Scripts.Godot;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Managed_Assets;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.Scripting;
using JoyGodot.Assets.Scripts.World;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.Entities.Items
{
    public class ItemInstance : JoyObject.JoyObject, IItemInstance
    {
        protected const string DURABILITY = "durability";

        protected IEntity User { get; set; }
        
         
        protected bool m_Identified;

         
        protected List<Guid> m_Contents;
        
         
        protected BaseItemType m_Type;

         
        protected Guid m_OwnerGUID;
        
         
        protected string m_OwnerString;
        
        protected int StateIndex { get; set; }

         
        protected int m_Value;

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
                return this.m_OwnerGUID;
            }
            protected set
            {
                this.m_OwnerGUID = value;
                this.m_OwnerString = this.EntityHandler?.Get(this.m_OwnerGUID)?.JoyName;
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

        public int Efficiency => (int)(this.m_Type.Material.Bonus * (this.GetValue(DURABILITY) / (float)this.GetMaximum(DURABILITY)));

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

        public IEnumerable<IItemInstance> Contents
        {
            get => this.ItemHandler?.GetItems(this.m_Contents);
        }

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

        public int Value => this.m_Value;

         
        public IEnumerable<IAbility> UniqueAbilities { get; protected set; }

        public IEnumerable<ISpriteState> Sprites => this.States;
        
        public ILiveItemHandler ItemHandler { get; set; }
        
        public ILiveEntityHandler EntityHandler { get; set; }
        
        public ItemInstance()
        {
            this.Data = new NonUniqueDictionary<object, object>();
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
            List<IItemInstance> contents = null,
            bool active = false)
            : base(
                type.UnidentifiedName, 
                guid,
                derivedValues,
                position,
                actions,
                sprites,
                type.SpriteSheet,
                roller,
                type.Tags)
        {
            this.Initialise();
                
            this.m_Type = type;
            
            this.Identified = identified;

            this.m_Contents = contents is null ? new List<Guid>() : contents.Select(instance => instance.Guid).ToList();

            this.UniqueAbilities = uniqueAbilities is null == false ? new List<IAbility>(uniqueAbilities) : new List<IAbility>();

            this.ItemHandler = GlobalConstants.GameManager.ItemHandler;
            this.EntityHandler = GlobalConstants.GameManager.EntityHandler;

            this.CalculateValue();

            if (this.States.Count > 1)
            {
                this.StateIndex = this.Roller.Roll(0, this.States.Count);
            }
            else
            {
                this.StateIndex = 0;
            }
        }

        public void Deserialise()
        {
            /*
            this.EntityHandler = GlobalConstants.GameManager?.EntityHandler;
            this.ItemHandler = GlobalConstants.GameManager?.ItemHandler;
            */
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
            this.Initialise();

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
            
            this.CalculateValue();
        }

        protected void Initialise()
        {
            /*
            if (GlobalConstants.GameManager is null)
            {
                return;
            }
            this.Data = new NonUniqueDictionary<object, object>();
            ItemHandler = GlobalConstants.GameManager.ItemHandler;
            EntityHandler = GlobalConstants.GameManager.EntityHandler;
            */
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
            if (actor.Guid == this.Guid 
            || this.Contains(actor) 
            || actor.Contains(this))
            {
                return false;
            }

            return true;
        }

        public bool AddContents(IItemInstance actor)
        {
            if(this.CanAddContents(actor))
            {
                this.m_Contents.Add(actor.Guid);

                this.ContentsDirty = true;

                this.CalculateValue();

                actor.InWorld = false;
                
                this.ItemAdded?.Invoke(this, new ItemChangedEventArgs { Item = actor });
                return true;
            }

            return false;
        }

        public bool AddContents(IEnumerable<IItemInstance> actors)
        {
            IEnumerable<IItemInstance> itemInstances = actors as IItemInstance[] ?? actors.ToArray();
            this.m_Contents.AddRange(
                itemInstances.Where(this.CanAddContents)
                .Select(instance => instance.Guid));

            this.ContentsDirty = true;
            
            this.CalculateValue();
            foreach (IItemInstance actor in itemInstances)
            {
                this.ItemAdded?.Invoke(this, new ItemChangedEventArgs { Item = actor });
            }

            return true;
        }

        public bool RemoveContents(IItemInstance actor)
        {
            if (!this.m_Contents.Remove(actor.Guid))
            {
                return false;
            }

            this.ContentsDirty = true;

            this.CalculateValue();
            this.ItemRemoved?.Invoke(this, new ItemChangedEventArgs { Item = actor });

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

            this.CalculateValue();
        }

        protected void CalculateValue()
        {
            this.m_Value = (int)(this.m_Type.Value * this.m_Type.Material.ValueMod);
            foreach (IItemInstance item in this.Contents)
            {
                this.m_Value += item.Value;
            }
        }

        public override Dictionary Save()
        {
            Dictionary saveDict = base.Save();
            
            saveDict.Add("ItemType", this.ItemType.IdentifiedName);
            saveDict.Add("Contents", new Array(this.m_Contents.Select(guid => guid.ToString())));
            saveDict.Add("Owner", this.OwnerGUID.ToString());
            saveDict.Add("Value", this.Value);
            saveDict.Add("InWorld", this.InWorld);
            saveDict.Add("UniqueAbilities", new Array(this.UniqueAbilities.Select(ability => ability.Save())));

            return saveDict;
        }

        public override void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.ItemHandler.ValueExtractor;
            
            base.Load(data);
            
            string itemType = valueExtractor.GetValueFromDictionary<string>(data, "ItemType");
            this.m_Type = GlobalConstants.GameManager.ItemDatabase.Get(itemType);
            this.m_Contents = new List<Guid>(
                valueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Contents")
                    .Select(s => new Guid(s)));
            string owner = valueExtractor.GetValueFromDictionary<string>(data, "Owner");
            this.OwnerGUID = owner is null ? Guid.Empty : new Guid(owner);
            this.m_Value = valueExtractor.GetValueFromDictionary<int>(data, "Value");
            this.InWorld = valueExtractor.GetValueFromDictionary<bool>(data, "InWorld");
            this.UniqueAbilities = new List<IAbility>(
                valueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "UniqueAbilities")
                    .Select(s => GlobalConstants.GameManager.AbilityHandler.Get(s)));

            this.ContentsDirty = true;
        }
    }
}
