using Godot.Collections;

namespace JoyGodot.Assets.Scripts.Base_Interfaces
{
    public interface ISerialisationHandler
    {
        Dictionary Save();

        void Load(Dictionary data);
    }
}