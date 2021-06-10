using Godot.Collections;

namespace JoyLib.Code
{
    public interface ISerialisationHandler
    {
        Dictionary Save();

        void Load(Dictionary data);
    }
}