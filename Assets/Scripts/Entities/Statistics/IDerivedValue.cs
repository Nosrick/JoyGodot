namespace JoyLib.Code.Entities.Statistics
{
    public interface IDerivedValue : IBasicValue<int>
    {
        int Maximum
        {
            get;
        }

        int Base
        {
            get;
        }

        int Enhancement
        {
            get;
        }
        int SetBase(int data);
        int SetEnhancement(int data, bool changeToMatch = true);
    }
}
