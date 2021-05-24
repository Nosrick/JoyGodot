using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyLib.Code.Collections;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Events;
using JoyLib.Code.Godot;
using JoyLib.Code.Graphics;
using JoyLib.Code.Rollers;
using JoyLib.Code.Scripting;
using JoyLib.Code.World;

namespace JoyLib.Code
{
    [Serializable]
    public class JoyObject : IComparable, IJoyObject
    {
        public event ValueChangedEventHandler<int> OnDerivedValueChange;
        public event ValueChangedEventHandler<int> OnMaximumChange;
        
        protected List<string> m_Tags;
        
        public IDictionary<string, IDerivedValue> DerivedValues { get; protected set; }
        
        public Vector2Int WorldPosition { get; protected set; }

        public IEnumerable<string> Tags
        {
            get => this.m_Tags;
            protected set => this.m_Tags = new List<string>(value);
        }

        public bool IsWall { get; protected set; }

        public bool IsDestructible { get; protected set; }
        
        public virtual IWorldInstance MyWorld { get; set; }

        public Guid Guid { get; protected set; }

        public string JoyName { get; protected set; }

        public string TileSet { get; protected set; }

        public virtual int HitPointsRemaining => this.GetValue("hitpoints");

        public virtual int HitPoints => this.GetMaximum("hitpoints");

        public bool Alive => this.HitPointsRemaining > (this.HitPoints * (-1));
        
        protected NonUniqueDictionary<object, object> Data { get; set; }

        public List<ISpriteState> States
        {
            get => this.m_States;
            protected set => this.m_States = value;
        }

        protected List<ISpriteState> m_States;

        public List<IJoyAction> CachedActions { get; protected set; }

        public JoyObjectNode MyNode
        {
            get => this.m_MyNode;
            set
            {
                this.m_MyNode = value;
                this.Move(this.WorldPosition);
            }
        }

        protected JoyObjectNode m_MyNode;

        public IRollable Roller { get; protected set; }

        public virtual ICollection<string> Tooltip
        {
            get => this.m_Tooltip;
            set => this.m_Tooltip = value.ToList();
        }
        
        protected List<string> m_Tooltip;

        public JoyObject()
        {
            this.Guid = Guid.Empty;
        }

        /// <summary>
        /// Creation of a JoyObject
        /// </summary>
        /// <param name="name"></param>
        /// <param name="hitPoints"></param>
        /// <param name="position"></param>
        /// <param name="sprites"></param>
        /// <param name="baseType"></param>
        /// <param name="isAnimated"></param>
        /// <param name="isWall"></param>
        public JoyObject(
            string name, 
            Guid guid,
            IDictionary<string, IDerivedValue> derivedValues, 
            Vector2Int position, 
            IEnumerable<string> actions,
            IEnumerable<ISpriteState> sprites, 
            string tileSet,
            RNG roller = null,
            params string[] tags)
        {
            this.TileSet = tileSet;
            this.Roller = roller is null ? new RNG() : roller; 
            List<IJoyAction> tempActions = new List<IJoyAction>(); 
            foreach(string action in actions)
            {
                tempActions.Add(ScriptingEngine.Instance.FetchAction(action));
            }

            this.Initialise(
                name, 
                guid,
                derivedValues,
                position,
                tempActions.ToArray(),
                sprites,
                tags);
        }

        public JoyObject(
            string name, 
            Guid guid,
            IDictionary<string, IDerivedValue> derivedValues, 
            Vector2Int position,
            IEnumerable<IJoyAction> actions,
            IEnumerable<ISpriteState> sprites,
            string tileSet,
            IRollable roller = null,
            params string[] tags)
        {
            this.TileSet = tileSet;
            this.Roller = roller is null ? new RNG() : roller; 
            this.Initialise(
                name,
                guid,
                derivedValues,
                position,
                actions,
                sprites,
                tags);
        }

        public void Initialise(
            string name, 
            Guid guid,
            IDictionary<string, IDerivedValue> derivedValues, 
            Vector2Int position, 
            IEnumerable<IJoyAction> actions,
            IEnumerable<ISpriteState> sprites, 
            params string[] tags)
        {
            this.Data = new NonUniqueDictionary<object, object>();
            
            this.JoyName = name;
            this.Guid = guid;

            this.DerivedValues = derivedValues;

            this.Tags = tags.ToList();

            this.States = sprites.ToList();

            this.WorldPosition = position;
            this.Move(this.WorldPosition);

            if (tags.Any(tag => tag.Equals("invulnerable", StringComparison.OrdinalIgnoreCase)))
            {
                this.IsDestructible = false;
            }

            if (tags.Any(tag => tag.Equals("wall", StringComparison.OrdinalIgnoreCase)))
            {
                this.IsWall = true;
            }

            this.CachedActions = new List<IJoyAction>(actions);

            this.Tooltip = new List<string>();
        }

        public IJoyAction FetchAction(string name)
        {
            return this.CachedActions.First(action => action.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        protected virtual void OnMaximumChanged(object sender, ValueChangedEventArgs<int> args)
        {
            this.OnMaximumChange?.Invoke(sender, args);
        }

        protected virtual void OnDerivedValueChanged(object sender, ValueChangedEventArgs<int> args)
        {
            this.OnDerivedValueChange?.Invoke(sender, args);
        }

        public bool AddTag(string tag)
        {
            if (this.HasTag(tag))
            {
                return false;
            }
            
            this.m_Tags.Add(tag);
            return true;
        }

        public bool RemoveTag(string tag)
        {
            if (!this.HasTag(tag))
            {
                return false;
            }
            
            this.m_Tags.Remove(tag);
            return true;
        }

        public bool HasTag(string tag)
        {
            return this.m_Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        }

        public void Move(Vector2Int newPosition)
        {
            this.WorldPosition = newPosition;

            this.MyNode?.Move(newPosition);
        }

        public int DamageValue(string name, int value)
        {
            return this.ModifyValue(name, -value);
        }

        public int RestoreValue(string name, int value)
        {
            return this.ModifyValue(name, value);
        }

        public virtual int GetValue(string name)
        {
            if (this.DerivedValues.Keys.Any(key => key.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return this.DerivedValues.First(pair => pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Value.Value;
            }
            
            throw new InvalidOperationException("Derived value of " + name + " not found on JoyObject " + this.ToString());
        }

        public virtual int GetMaximum(string name)
        {
            if (this.DerivedValues.Keys.Any(key => key.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return this.DerivedValues.First(pair => pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Value.Maximum;
            }
            
            throw new InvalidOperationException("Derived value of " + name + " not found on JoyObject " + this.ToString());
        }

        public int SetBase(string name, int value)
        {
            if (!this.DerivedValues.ContainsKey(name))
            {
                throw new InvalidOperationException("Derived value of " + name + " not found on JoyObject " + this);
            }

            int old = this.DerivedValues[name].Base;
            this.DerivedValues[name].SetBase(value);
            this.OnMaximumChange?.Invoke(this, new ValueChangedEventArgs<int>
            {
                Delta = value - old,
                Name = name,
                NewValue = this.DerivedValues[name].Base
            });
            return this.DerivedValues[name].Base;
        }
        
        public int SetEnhancement(string name, int value)
        {
            if (!this.DerivedValues.ContainsKey(name))
            {
                throw new InvalidOperationException("Derived value of " + name + " not found on JoyObject " + this);
            }

            int old = this.DerivedValues[name].Enhancement;
            this.DerivedValues[name].SetEnhancement(value);
            this.OnMaximumChange?.Invoke(this, new ValueChangedEventArgs<int>
            {
                Delta = value - old,
                Name = name,
                NewValue = this.DerivedValues[name].Enhancement
            });
            return this.DerivedValues[name].Enhancement;
        }

        public virtual int ModifyValue(string name, int value)
        {
            if (!this.DerivedValues.ContainsKey(name))
            {
                throw new InvalidOperationException("Derived value of " + name + " not found on JoyObject " + this);
            }
            this.DerivedValues[name].ModifyValue(value);
            this.OnDerivedValueChange?.Invoke(this, new ValueChangedEventArgs<int>
            {
                Delta = value,
                Name = name,
                NewValue = this.DerivedValues[name].Value
            });
            return this.DerivedValues[name].Value;
        }

        public virtual int SetValue(string name, int value)
        {
            if (!this.DerivedValues.ContainsKey(name))
            {
                throw new InvalidOperationException("Derived value of " + name + " not found on JoyObject " + this);
            }

            int old = this.DerivedValues[name].Value;
            this.DerivedValues[name].SetValue(value);
            this.OnDerivedValueChange?.Invoke(this, new ValueChangedEventArgs<int>
            {
                Delta = value - old,
                Name = name,
                NewValue = this.DerivedValues[name].Value
            });
            return this.DerivedValues[name].Value;
        }

        // Update is called once per frame
        public virtual void Update ()
        {
        }

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;
                
                case JoyObject joyObject:
                    return this.Guid.CompareTo(joyObject.Guid);
                
                default:
                    throw new ArgumentException("Object is not a JoyObject");
            }
        }

        public override string ToString()
        {
            return "{ " + this.JoyName + " : " + this.Guid + " }";
        }

        public virtual void Tick()
        {
        }

        public bool AddData(object key, object value)
        {
            this.Data.Add(key, value);
            return true;
        }

        public bool RemoveData(object key)
        {
            return this.Data.RemoveByKey(key) > 0;
        }

        public bool HasDataKey(object search)
        {
            return this.Data.ContainsKey(search);
        }

        public bool HasDataValue(object search)
        {
            return this.Data.ContainsValue(search);
        }

        public object[] GetDataValues(object key)
        {
            return this.Data.Where(tuple => tuple.Item1.Equals(key))
                .Select(tuple => tuple.Item2)
                .ToArray();
        }

        public object[] GetDataKeysForValue(object value)
        {
            return this.Data.Where(tuple => tuple.Item2.Equals(value))
                .Select(tuple => tuple.Item1)
                .ToArray();
        }

        public void SetStates(IEnumerable<ISpriteState> states)
        {
            this.m_States = states.ToList();
        }
    }    
}
