namespace JoyLib.Code.Entities.Statistics
{
    public interface IBasicValue<T> where T : struct 
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
