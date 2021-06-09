using Godot.Collections;

namespace JoyLib.Code
{
    public interface ISerialisationHandler
    {
        Dictionary Save();

        void Load(string data);
    }
}