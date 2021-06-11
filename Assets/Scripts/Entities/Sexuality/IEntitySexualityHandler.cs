namespace JoyLib.Code.Entities.Sexuality
{
    public interface IEntitySexualityHandler : IHandler<ISexuality, string>
    {
        ISexualityPreferenceProcessor GetProcessor(string name);
    }
}