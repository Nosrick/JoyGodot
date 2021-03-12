using System.Collections.Generic;

namespace JoyLib.Code.Entities.Needs
{
    public class FulfillmentData
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
    }
}