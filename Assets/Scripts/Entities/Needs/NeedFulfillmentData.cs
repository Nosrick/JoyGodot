using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.Entities.Needs
{
    public class NeedFulfillmentData : ISerialisationHandler
    {
        public NeedFulfillmentData()
        {
            this.Name = "none";
            this.Counter = 0;
            this.Targets = new IJoyObject[0];
            this.Value = 0;
            this.InitialCounter = 0;
        }
        
        public NeedFulfillmentData(
            string name, 
            int counter,
            int value,
            IEnumerable<IJoyObject> targets)
        {
            this.Name = name;
            this.Counter = counter;
            this.Targets = targets;
            this.Value = value;
            this.InitialCounter = counter;
        }

        public bool IsEmpty()
        {
            return (this.Name.IsNullOrEmpty() 
                    || this.Name?.Equals("none", StringComparison.OrdinalIgnoreCase) == true) 
                   && this.Counter == 0 
                   && this.Targets.IsNullOrEmpty();
        }

        public int DecrementCounter()
        {
            this.Counter -= 1;
            return this.Counter;
        }

        public string Name
        {
            get;
            private set;
        }

        public int Counter
        {
            get;
            private set;
        }
        
        public int Value { get; private set; }
        
        public int InitialCounter { get; private set; }

        public IEnumerable<IJoyObject> Targets
        {
            get;
            set;
        }

        public int ValuePerTick => (int) Math.Max(1, (float) this.Value / this.InitialCounter);

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"Name", this.Name},
                {"Counter", this.Counter},
                {"Targets", new Array(
                    this.Targets.IsNullOrEmpty()
                    ? new string[0]
                    : this.Targets.Select(o => o.Guid.ToString()))},
                {"InitialCounter", this.InitialCounter},
                {"Value", this.Value}
            };

            return saveDict;
        }

        public void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.ItemHandler.ValueExtractor;

            this.Name = valueExtractor.GetValueFromDictionary<string>(data, "Name");
            this.Counter = valueExtractor.GetValueFromDictionary<int>(data, "Counter");
            this.InitialCounter = valueExtractor.GetValueFromDictionary<int>(data, "InitialCounter");
            this.Value = valueExtractor.GetValueFromDictionary<int>(data, "Value");
            List<IJoyObject> targets = new List<IJoyObject>();
            Guid[] guids = valueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Targets")
                .Select(s => new Guid(s))
                .ToArray();

            foreach (Guid guid in guids)
            {
                IJoyObject target = GlobalConstants.GameManager.EntityHandler.Get(guid) 
                                    ?? (IJoyObject) GlobalConstants.GameManager.ItemHandler.Get(guid);

                if (target is null == false)
                {
                    targets.Add(target);
                }
            }

            this.Targets = targets;
        }
    }
}