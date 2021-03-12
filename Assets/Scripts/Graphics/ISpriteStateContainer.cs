using System.Collections.Generic;

namespace JoyLib.Code.Graphics
{
    public interface ISpriteStateContainer
    {
        IEnumerable<ISpriteState> States { get; }
        ISpriteState GetState(string name);
        IEnumerable<ISpriteState> GetStatesByName(string name);
        void AddSpriteState(ISpriteState state, bool changeToNew);
        bool RemoveStatesByName(string name);
        void Clear();
    }
}