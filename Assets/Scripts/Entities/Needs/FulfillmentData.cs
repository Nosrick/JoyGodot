using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

namespace JoyLib.Code.Entities.Needs
{
    public class FulfillmentData : ISerialisationHandler
    {
        public FulfillmentData(string name, int counter, IJoyObject[] targets)
        {
            this.Name = name;
            this.Counter = counter;
            this.Targets = targets;
        }

        public int DecrementCounter()
        {
            this.Counter -= 1;
            return this.Counter;
        }

        public string Name
        {
            get;
            protected set;
        }

        public int Counter
        {
            get;
            protected set;
        }

        public IEnumerable<IJoyObject> Targets
        {
            get;
            protected set;
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"Name", this.Name},
                {"Counter", this.Counter},
                {"Targets", new Array(this.Targets.Select(o => o.Guid.ToString()))}
            };

            return saveDict;
        }

        public void Load(string data)
        {
            throw new System.NotImplementedException();
        }
    }
}