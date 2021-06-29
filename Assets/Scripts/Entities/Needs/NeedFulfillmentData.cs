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
    public struct NeedFulfillmentData : ISerialisationHandler
    {
        public NeedFulfillmentData(string name, int counter, IEnumerable<IJoyObject> targets)
        {
            this.Name = name;
            this.Counter = counter;
            this.Targets = targets;
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
            set;
        }

        public int Counter
        {
            get;
            set;
        }

        public IEnumerable<IJoyObject> Targets
        {
            get;
            set;
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"Name", this.Name},
                {"Counter", this.Counter},
                {"Targets", new Array(
                    this.Targets.IsNullOrEmpty()
                    ? new string[0]
                    : this.Targets.Select(o => o.Guid.ToString()))}
            };

            return saveDict;
        }

        public void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.ItemHandler.ValueExtractor;

            this.Name = valueExtractor.GetValueFromDictionary<string>(data, "Name");
            this.Counter = valueExtractor.GetValueFromDictionary<int>(data, "Counter");
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