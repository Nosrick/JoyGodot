using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Entities;

namespace JoyGodot.Assets.Scripts.World.WorldInfo
{
    public abstract class AbstractLocalAreaProcessor : ILocalAreaProcessor
    {
        public IEnumerable<string> Tags
        {
            get => this.m_Tags;
            protected set => this.m_Tags = new HashSet<string>(value);
        }

        protected HashSet<string> m_Tags;
        
        public bool HasTag(string tag)
        {
            return this.Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasTags(IEnumerable<string> tags)
        {
            return tags.All(this.HasTag);
        }

        public bool AddTag(string tag)
        {
            return this.m_Tags.Add(tag);
        }

        public bool RemoveTag(string tag)
        {
            return this.m_Tags.Remove(tag);
        }

        public abstract string Get(IWorldInstance worldInstance, IEntity origin = null);
    }
}