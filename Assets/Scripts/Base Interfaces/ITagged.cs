using System.Collections.Generic;

namespace JoyGodot.Assets.Scripts.Base_Interfaces
{
    public interface ITagged
    {
        IEnumerable<string> Tags { get; }
        bool HasTag(string tag);
        bool AddTag(string tag);
        bool RemoveTag(string tag);
    }
}