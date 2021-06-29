namespace JoyGodot.Assets.Scripts.Entities.Statistics
{
    public interface IRollableValue<T> : IBasicValue<T> where T : struct
    {
        int SuccessThreshold
        {
            get;
        }

        int SetThreshold(int value);
    }
}
