using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Scripts.World
{
    public class WorldHandler : IWorldHandler
    {
        public IEnumerable<IWorldInstance> Values => this.Worlds.Values;
        public JSONValueExtractor ValueExtractor { get; protected set; }
        
        protected IDictionary<Guid, IWorldInstance> Worlds { get; set; }

        public WorldHandler()
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.Worlds = new Dictionary<Guid, IWorldInstance>();
        }
        
        public IWorldInstance Get(Guid name)
        {
            return this.Worlds.TryGetValue(name, out IWorldInstance world) ? world : null;
        }

        public bool Add(IWorldInstance value)
        {
            if (this.Worlds.ContainsKey(value.Guid))
            {
                return false;
            }

            this.Worlds.Add(value.Guid, value);
            foreach (IWorldInstance child in value.Areas.Values)
            {
                this.Add(child);
            }
            
            return true;
        }

        public bool Destroy(Guid key)
        {
            return this.Worlds.Remove(key);
        }

        public IEnumerable<IWorldInstance> Load()
        {
            return new List<IWorldInstance>();
        }
        
        public void Dispose()
        {
            GarbageMan.Dispose(this.Worlds);
            this.Worlds = null;
            this.ValueExtractor = null;
        }
    }
}