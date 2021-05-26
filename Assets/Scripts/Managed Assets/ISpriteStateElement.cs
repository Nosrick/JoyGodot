using Godot;
using JoyLib.Code.Graphics;

namespace JoyGodot.Assets.Scripts.GUI.Managed_Assets
{
    public interface ISpriteStateElement : IManagedElement
    {
        void AddSpriteState(ISpriteState state, bool changeToNew = true);
        bool RemoveStatesByName(string name);
        ISpriteState GetState(string name);
        bool ChangeState(string name);
        void Clear();
    }
}