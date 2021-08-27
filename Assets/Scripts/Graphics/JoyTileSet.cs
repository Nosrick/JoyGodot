using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Helpers;
using MonoCustomResourceRegistry;

namespace JoyGodot.Assets.Scripts.Graphics
{
    [RegisteredType(nameof(JoyTileSet), "res://icon.png", nameof(TileSet))]
    public class JoyTileSet : TileSet, ITagged
    {
        public IEnumerable<Color> PossibleColours { get; set; }
        public IEnumerable<string> Tags => this.m_Tags;

        protected List<string> m_Tags;

        public JoyTileSet()
        {
            this.m_Tags = new List<string>();
        }
        
        public bool HasTag(string tag)
        {
            return this.Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasTags(IEnumerable<string> tags)
        {
            return tags.Aggregate(true, (current, t) => current & this.HasTag(t));
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
            string toRemove = this.m_Tags.FirstOrDefault(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));

            return toRemove.IsNullOrEmpty() == false && this.m_Tags.Remove(toRemove);
        }
    }
}