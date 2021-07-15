using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Managed_Assets;
using JoyGodot.Assets.Scripts.Scripting;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.Entities.Abilities
{
    public abstract class AbstractAbility : IAbility
    {
        protected readonly System.Collections.Generic.Dictionary<string, IJoyAction> m_CachedActions;

        protected List<string> m_Tags;

        public string Name { get; protected set; }

        public string InternalName { get; protected set; }

        public string Description { get; protected set; }

        public bool Stacking { get; protected set; }

        public int Counter { get; protected set; }

        public int Magnitude { get; protected set; }

        public int Priority { get; protected set; }

        public bool ReadyForRemoval
        {
            get
            {
                if (this.Permanent == true)
                {
                    return false;
                }

                if (this.Counter <= 0 || this.Magnitude <= 0)
                {
                    return true;
                }

                return false;
            }
        }

        public bool Permanent { get; protected set; }

        public IEnumerable<Tuple<string, int>> Costs { get; protected set; }

        public IDictionary<string, int> Prerequisites { get; protected set; }

        public AbilityTarget TargetType { get; protected set; }

        public IEnumerable<string> Tags
        {
            get => this.m_Tags;
            protected set => this.m_Tags = new List<string>(value);
        }

        public int Range
        {
            get
            {
                return this.TargetType switch
                {
                    AbilityTarget.Ranged => this.m_Range,
                    AbilityTarget.Adjacent => 1,
                    AbilityTarget.Self => 0,
                    AbilityTarget.WeaponRange => 3,
                    _ => 0
                };
            }
            set => this.m_Range = value;
        }

        protected int m_Range;

        public SpriteData SpriteData { get; protected set; }
        public Texture UsingIcon { get; protected set; }

        public AbstractAbility()
        { }

        public AbstractAbility(
            string name,
            string internalName,
            string description,
            bool stacking,
            int counter,
            int magnitude,
            int priority,
            bool permanent,
            string[] actions,
            Tuple<string, int>[] costs, System.Collections.Generic.Dictionary<string, int> prerequisites,
            AbilityTarget target,
            int range = 0,
            SpriteData usingSprite = null,
            params string[] tags)
        {
            this.Name = name;
            this.InternalName = internalName;
            this.Description = description;
            this.Stacking = stacking;
            this.Counter = counter;
            this.Magnitude = magnitude;
            this.Priority = priority;
            this.Permanent = permanent;
            this.Costs = costs;
            this.TargetType = target;
            this.Prerequisites = prerequisites;
            this.Tags = tags;
            this.Range = range;
            this.SpriteData = usingSprite;
            this.UsingIcon = this.SpriteData?.Parts
                .FirstOrDefault(part => part.m_Name.Equals("icon", StringComparison.OrdinalIgnoreCase))?
                .m_FrameSprite.FirstOrDefault();

            this.m_CachedActions = new System.Collections.Generic.Dictionary<string, IJoyAction>(actions.Length);

            foreach (string action in actions)
            {
                this.m_CachedActions.Add(action, GlobalConstants.ScriptingEngine.FetchAction(action));
            }
        }

        protected static SpriteData GetSprite(string name)
        {
            SpriteData data = null; //GlobalConstants.GameManager?.ObjectIconHandler?.GetFrame("abilities", name);
            return data;
        }

        //When the entity attacks, before any resolution occurs
        //Returns false to denote no effect took place
        public virtual bool OnAttack(IEntity attacker, IEntity target, IEnumerable<string> attackerTags,
            IEnumerable<string> defenderTags)
        {
            return false;
        }

        //When the entity is hit, after resolution
        //Triggers even when no damage happens
        //Returns the damage by default
        public virtual int OnTakeHit(
            IEntity attacker,
            IEntity defender,
            int damage,
            IEnumerable<string> attackerTags,
            IEnumerable<string> defenderTags)
        {
            return damage;
        }

        //When the entity is healed
        //Returns the amount healed
        public virtual int OnHeal(IEntity receiver, IEntity healer, int value, IEnumerable<string> receiverTags,
            IEnumerable<string> healerTags)
        {
            return value;
        }

        //When the entity picks up an item
        //Return true to denote an effect took place
        //Thus returns false by default
        public virtual bool OnPickup(IEntity entity, IItemInstance item)
        {
            return false;
        }

        //Triggered when the entity ticks
        //Returns true when an effect triggers
        //Thus returns false by default
        public virtual bool OnTick(IEntity entity)
        {
            return false;
        }

        //Triggered when an entity reduces another entity to zero of a Derived Value
        //Returns true when the effect takes place
        public virtual bool OnReduceToZero(IEntity attacker, IEntity target, IDerivedValue value)
        {
            return false;
        }

        //Triggered when an entity reduces another entity to the "disabled" of a Derived Value
        //Returns true when the effect takes place
        public virtual bool OnDisable(IEntity attacker, IEntity target, IDerivedValue value)
        {
            return false;
        }

        //Triggered when an entity uses the JoyObject this ability is attached to
        //This is typically an item
        //Returns true when the ability triggers
        public virtual bool OnUse(IEntity user, IJoyObject target)
        {
            return false;
        }

        //Triggered when an entity interacts with the JoyObject (could be an item or an entity or whatever)
        //Returns true when the effect triggers
        public virtual bool OnInteract(IEntity actor, IJoyObject observer)
        {
            return false;
        }

        //Triggered when the ability is added to the entity
        //Returns true if an effect was added (other than this one)
        public virtual bool OnAdd(IEntity entity)
        {
            return false;
        }

        //Triggered when the ability is removed from the entity
        //Return true if an effect was removed (other than this one)
        public virtual bool OnRemove(IEntity entity)
        {
            return false;
        }

        //When the entity uses a skill
        //This returns the success threshold modification for the roll
        //The second parameter is for checking against other possible stat/skill values
        public virtual int OnCheckRollModifyThreshold(int successThreshold, IEnumerable<IBasicValue<int>> values,
            IEnumerable<string> attackerTags, IEnumerable<string> defenderTags)
        {
            return successThreshold;
        }

        //This returns bonus/penalty dice for the roll
        //The second parameter is for checking against other possible stat/skill values
        public virtual int OnCheckRollModifyDice(int dicePool, IEnumerable<IBasicValue<int>> values,
            IEnumerable<string> attackerTags, IEnumerable<string> defenderTags)
        {
            return dicePool;
        }

        //This is used for directly modifying the successes of the check
        //And should return the new successes
        //The second parameter is for checking against other possible stat/skill values
        public virtual int OnCheckSuccess(int successes, IEnumerable<IBasicValue<int>> values,
            IEnumerable<string> attackerTags, IEnumerable<string> defenderTags)
        {
            return successes;
        }

        public bool HasTag(string tag)
        {
            return this.Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        }

        public bool AddTag(string tag)
        {
            if (this.HasTag(tag))
            {
                return true;
            }

            this.m_Tags.Add(tag);
            return true;
        }

        public bool RemoveTag(string tag)
        {
            if (this.HasTag(tag))
            {
                this.m_Tags.RemoveAt(this.m_Tags.FindIndex(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)));
                return true;
            }

            return false;
        }

        public bool DecrementCounter(int value)
        {
            this.Counter = Math.Max(0, this.Counter - value);
            return this.ReadyForRemoval;
        }

        public bool DecrementMagnitude(int value)
        {
            this.Magnitude = Math.Max(0, this.Magnitude - value);
            return this.ReadyForRemoval;
        }

        public void IncrementMagnitude(int value)
        {
            if (this.Stacking == true)
            {
                this.Magnitude += value;
            }
        }

        public void IncrementCounter(int value)
        {
            if (this.Stacking == true)
            {
                this.Counter += value;
            }
        }

        public bool HasResourcesForUse(IEntity caster)
        {
            IEnumerable<string> costs = this.Costs.Select(cost => cost.Item1);
            IEnumerable<Tuple<string, int>> returnData = caster.GetData(costs)
                .Where(tuple => tuple.Item2 is int)
                .Select(tuple => new Tuple<string, int>(tuple.Item1, (int) tuple.Item2));

            return returnData.Count() >= this.Costs.Count()
                && returnData.All(x => 
                this.Costs.Any(cost => 
                    cost.Item1.Equals(
                        x.Item1, StringComparison.OrdinalIgnoreCase) 
                    && x.Item2 >= cost.Item2));
        }

        public bool EnactToll(IEntity caster)
        {
            foreach (var pair in this.Costs)
            {
                caster.ModifyValue(pair.Item1, -pair.Item2);
            }
            return true;
        }

        public bool MeetsPrerequisites(IEntity actor)
        {
            bool meetsPrereqs = false;

            IEnumerable<string> prereqs = this.Prerequisites.Select(pair => pair.Key);
            IEnumerable<Tuple<string, int>> returnData = actor.GetData(prereqs)
                .Where(tuple => tuple.Item2 is int)
                .Select(tuple => new Tuple<string, int>(tuple.Item1, (int) tuple.Item2));

            meetsPrereqs = returnData.All(x => this.Prerequisites.Any(prereq =>
                prereq.Key.Equals(x.Item1, StringComparison.OrdinalIgnoreCase) && x.Item2 >= prereq.Value));

            return meetsPrereqs;
        }

        public bool MeetsPrerequisites(IEnumerable<IBasicValue<int>> data)
        {
            bool meetsValueRequirements = this.Prerequisites.IsNullOrEmpty()
                                          || this.Prerequisites.All(prereq => data.Any(
                                              d => d.Name.Equals(prereq.Key)
                                                   && d.Value >= prereq.Value));
            return meetsValueRequirements;
        }

        public bool IsInRange(IEntity left, IJoyObject right)
        {
            Vector2Int leftPos = left.WorldPosition;
            Vector2Int rightPos = right.WorldPosition;

            int longestRange = 1;
            if (left.Equipment.Contents.IsNullOrEmpty() == false)
            {
                longestRange = left.Equipment.Contents.Max(instance => instance.ItemType.Range);
            }

            switch (this.TargetType)
            {
                case AbilityTarget.Adjacent:
                    return AdjacencyHelper.IsAdjacent(leftPos, rightPos) || leftPos == rightPos;

                case AbilityTarget.Ranged:
                    return AdjacencyHelper.IsInRange(leftPos, rightPos, this.Range);

                case AbilityTarget.WeaponRange:
                    return AdjacencyHelper.IsInRange(leftPos, rightPos, longestRange);

                case AbilityTarget.Self:
                    return leftPos == rightPos;

                default:
                    return false;
            }
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary();

            saveDict.Add("Name", this.Name);
            saveDict.Add("InternalName", this.InternalName);
            saveDict.Add("Description", this.Description);
            saveDict.Add("Stacking", this.Stacking);
            saveDict.Add("Counter", this.Counter);
            saveDict.Add("Magnitude", this.Magnitude);
            saveDict.Add("Priority", this.Priority);
            saveDict.Add("Permanent", this.Permanent);

            Dictionary tempDict = new Dictionary();
            foreach ((string key, int value) in this.Costs)
            {
                tempDict.Add(key, value);
            }

            saveDict.Add("Costs", tempDict);

            tempDict = new Dictionary();
            foreach (var pair in this.Prerequisites)
            {
                tempDict.Add(pair.Key, pair.Value);
            }

            saveDict.Add("Prerequisites", tempDict);

            saveDict.Add("TargetType", this.TargetType.ToString());
            saveDict.Add("Tags", new Array(this.Tags));
            saveDict.Add("Range", this.Range);

            return saveDict;
        }

        public void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.ItemHandler.ValueExtractor;

            this.Name = valueExtractor.GetValueFromDictionary<string>(data, "Name");
            this.InternalName = valueExtractor.GetValueFromDictionary<string>(data, "InternalName");
            this.Description = valueExtractor.GetValueFromDictionary<string>(data, "Description");
            this.Stacking = valueExtractor.GetValueFromDictionary<bool>(data, "Stacking");
            this.Counter = valueExtractor.GetValueFromDictionary<int>(data, "Counter");
            this.Magnitude = valueExtractor.GetValueFromDictionary<int>(data, "Magnitude");
            this.Priority = valueExtractor.GetValueFromDictionary<int>(data, "Priority");
            this.Permanent = valueExtractor.GetValueFromDictionary<bool>(data, "Permanent");

            List<Tuple<string, int>> costs = new List<Tuple<string, int>>();
            Dictionary tempDict = valueExtractor.GetValueFromDictionary<Dictionary>(data, "Costs");
            foreach (DictionaryEntry cost in tempDict)
            {
                costs.Add(new Tuple<string, int>(
                    cost.Key.ToString(),
                    int.Parse(cost.Value.ToString())));
            }

            this.Costs = costs;

            this.Prerequisites = new System.Collections.Generic.Dictionary<string, int>();
            tempDict = valueExtractor.GetValueFromDictionary<Dictionary>(data, "Prerequisites");
            foreach (DictionaryEntry prereq in tempDict)
            {
                this.Prerequisites.Add(
                    prereq.Key.ToString(),
                    int.Parse(prereq.Value.ToString()));
            }

            this.TargetType = (AbilityTarget) Enum.Parse(
                typeof(AbilityTarget),
                valueExtractor.GetValueFromDictionary<string>(
                    data,
                    "TargetType"));

            this.Tags = valueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Tags");
            this.Range = valueExtractor.GetValueFromDictionary<int>(data, "Range");
        }
    }
}