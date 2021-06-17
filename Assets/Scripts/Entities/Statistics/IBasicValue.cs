using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Entities.Statistics
{
    public interface IBasicValue<T> : ISerialisationHandler where T : struct
    {
        string Name
        {
            get;
        }

        T Value
        {
            get;
        }

        T ModifyValue(T value);
        T SetValue(T value);
    }
}
