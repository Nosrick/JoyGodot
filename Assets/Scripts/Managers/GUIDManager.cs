using System;
using System.Collections.Generic;

namespace JoyLib.Code.Managers
{
    [Serializable]
    public class GUIDManager : IDisposable
    {
        public Queue<Guid> RecycleList { get; protected set; }

        public GUIDManager()
        {
            this.RecycleList = new Queue<Guid>();
        }

        public void Deserialise(Queue<Guid> recycleList)
        {
            this.RecycleList = recycleList;
        }

        public Guid AssignGUID()
        {
            /*
            if (this.RecycleList.Count > 0)
            {
                return this.RecycleList.Dequeue();
            }
            */

            return Guid.NewGuid();
        }

        public void ReleaseGUID(Guid GUIDRef)
        {
            this.RecycleList.Enqueue(GUIDRef);
        }

        public void Dispose()
        {
            this.RecycleList = null;
        }
    }
}
