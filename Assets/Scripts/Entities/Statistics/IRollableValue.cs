namespace JoyLib.Code.Entities.Statistics
{
    public interface IRollableValue<T> : IBasicValue<T> where T : struct
    {
        int SuccessThreshold
        {
            get;
        }
    }
}
