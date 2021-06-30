using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Entities.Sexuality.Processors;

namespace JoyGodot.Assets.Scripts.Entities.Sexuality
{
    public interface IEntitySexualityHandler : IHandler<ISexuality, string>
    {
        ISexualityPreferenceProcessor GetProcessor(string name);
    }
}