using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Events;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Managed_Assets;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Scripts.Entities.Needs
{
    public abstract class AbstractNeed : INeed
    {
        public virtual string Name
        {
            get => this.m_Name;
            protected set => this.m_Name = value;
        }

        protected string m_Name;

        public ISpriteState FulfillingSprite { get; set; }

        public RNG Roller { get; protected set; }

        protected bool Initialised { get; set; }

        public float PercentageFull => this.HappinessThreshold == 0
            ? 1f
            : Mathf.Min(1f, this.Value / (float) this.HappinessThreshold);

        protected System.Collections.Generic.Dictionary<string, IJoyAction> m_CachedActions;

        //How quickly the need decays
        //The higher the number, the slower it decays

        protected int m_Decay;
        protected int m_DecayCounter;
        protected bool m_DoesDecay;


        //How much of an impact this need has on overall happiness
        protected int m_Priority;


        //How high the value has to be before it contributes to happiness
        protected int m_HappinessThreshold;

        //Current value
        protected int m_Value;
        protected int m_MaximumValue;


        //Average for the day
        //Will be calculated by adding the value every hour, then dividing by 24 when the day is up
        protected int m_AverageForDay;


        //Average for the week
        //Calculated by adding value for the day every day, then dividing by 7 when the week is up
        protected int m_AverageForWeek;

        protected int m_AverageForMonth;

        public AbstractNeed(
            int decayRef,
            int decayCounterRef,
            bool doesDecayRef,
            int priorityRef,
            int happinessThresholdRef,
            int valueRef,
            int maxValueRef,
            IEnumerable<string> actions,
            ISpriteState fulfillingSprite = null,
            int averageForDayRef = 0,
            int averageForWeekRef = 0,
            RNG roller = null)
        {
            this.Roller = roller is null ? new RNG() : roller;
            this.m_CachedActions = new System.Collections.Generic.Dictionary<string, IJoyAction>();

            IJoyAction[] standardActions = this.FetchStandardActions();

            foreach (IJoyAction action in standardActions)
            {
                this.m_CachedActions.Add(action.Name, action);
            }

            this.m_Decay = decayRef;
            this.m_DecayCounter = decayCounterRef;
            this.m_DoesDecay = doesDecayRef;

            this.m_Priority = priorityRef;

            this.m_HappinessThreshold = happinessThresholdRef;

            this.m_Value = valueRef;
            this.m_MaximumValue = maxValueRef;

            this.m_AverageForDay = averageForDayRef;
            this.m_AverageForWeek = averageForWeekRef;

            this.FulfillingSprite = fulfillingSprite;
            if (this.FulfillingSprite is null)
            {
                SpriteData data = GlobalConstants.GameManager.ObjectIconHandler?.GetManagedSprites(
                    "needs",
                    this.Name)
                    .FirstOrDefault();
                if (data is null == false)
                {
                    this.FulfillingSprite = new SpriteState(this.Name, "needs", data);
                }
            }

            foreach (string action in actions)
            {
                this.m_CachedActions.Add(action, GlobalConstants.ScriptingEngine.FetchAction(action));
            }
        }

        public event ValueChangedEventHandler<int> ValueChanged;
        public abstract bool FindFulfilmentObject(IEntity actor);

        public abstract INeed Copy();

        public abstract INeed Randomise();

        //This will be called once per in-game minute
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actor"></param>
        /// <returns>True if there has been a change</returns>
        public virtual bool Tick(Entity actor)
        {
            this.m_DecayCounter -= 1;
            if (this.m_DecayCounter == 0 && this.m_DoesDecay)
            {
                this.m_DecayCounter = this.m_Decay;
                this.Decay(1);
                return true;
            }

            return false;
        }

        public virtual int Fulfill(int value)
        {
            return this.ModifyValue(value);
        }

        public virtual int Decay(int value)
        {
            return this.ModifyValue(-value);
        }

        public int ModifyValue(int value)
        {
            this.m_Value = Math.Max(0, Math.Min(this.m_MaximumValue, this.m_Value + value));
            this.ValueChanged?.Invoke(this, new ValueChangedEventArgs<int>
            {
                Delta = value,
                Name = this.Name,
                NewValue = this.m_Value
            });
            return this.m_Value;
        }

        public abstract bool Interact(IEntity actor, IJoyObject obj);

        public int SetValue(int value)
        {
            int delta = this.m_Value - value;
            this.m_Value = Mathf.Clamp(value, 0, this.m_MaximumValue);
            this.ValueChanged?.Invoke(this, new ValueChangedEventArgs<int>
            {
                Delta = delta,
                Name = this.Name,
                NewValue = this.m_Value
            });
            return this.m_Value;
        }

        protected IJoyAction[] FetchStandardActions()
        {
            List<IJoyAction> actions = new List<IJoyAction>();
            actions.Add(GlobalConstants.ScriptingEngine.FetchAction("seekaction"));
            actions.Add(GlobalConstants.ScriptingEngine.FetchAction("wanderaction"));
            actions.Add(GlobalConstants.ScriptingEngine.FetchAction("fulfillneedaction"));

            return actions.ToArray();
        }

        public int Priority
        {
            get { return this.m_Priority; }
            protected set { this.m_Priority = value; }
        }

        public bool ContributingHappiness
        {
            get { return this.m_Value >= this.m_HappinessThreshold; }
        }

        public int Value
        {
            get { return this.m_Value; }
            set { this.m_Value = value; }
        }

        public int AverageForDay
        {
            get { return this.m_AverageForDay; }
            protected set { this.m_AverageForDay = value; }
        }

        public int AverageForWeek
        {
            get { return this.m_AverageForWeek; }
            protected set { this.m_AverageForWeek = value; }
        }

        public int AverageForMonth
        {
            get { return this.m_AverageForMonth; }
            protected set { this.m_AverageForMonth = value; }
        }

        public int HappinessThreshold => this.m_HappinessThreshold;

        public virtual Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"Name", this.Name},
                {"Decay", this.m_Decay},
                {"DecayCounter", this.m_DecayCounter},
                {"DoesDecay", this.m_DoesDecay},
                {"Priority", this.m_Priority},
                {"HappinessThreshold", this.m_HappinessThreshold},
                {"Value", this.m_Value},
                {"MaximumValue", this.m_MaximumValue},
                {"AverageForDay", this.m_AverageForDay},
                {"AverageForWeek", this.m_AverageForWeek},
                {"AverageForMonth", this.m_AverageForMonth}
            };

            return saveDict;
        }

        public virtual void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.ItemHandler.ValueExtractor;

            this.Name = valueExtractor.GetValueFromDictionary<string>(data, "Name");
            this.m_Decay = valueExtractor.GetValueFromDictionary<int>(data, "Decay");
            this.m_DecayCounter = valueExtractor.GetValueFromDictionary<int>(data, "DecayCounter");
            this.m_DoesDecay = valueExtractor.GetValueFromDictionary<bool>(data, "DoesDecay");
            this.m_Priority = valueExtractor.GetValueFromDictionary<int>(data, "Priority");
            this.m_HappinessThreshold = valueExtractor.GetValueFromDictionary<int>(data, "HappinessThreshold");
            this.m_Value = valueExtractor.GetValueFromDictionary<int>(data, "Value");
            this.m_MaximumValue = valueExtractor.GetValueFromDictionary<int>(data, "MaximumValue");
            this.m_AverageForDay = valueExtractor.GetValueFromDictionary<int>(data, "AverageForDay");
            this.m_AverageForWeek = valueExtractor.GetValueFromDictionary<int>(data, "AverageForWeek");
            this.m_AverageForMonth = valueExtractor.GetValueFromDictionary<int>(data, "AverageForMonth");
        }
    }
}