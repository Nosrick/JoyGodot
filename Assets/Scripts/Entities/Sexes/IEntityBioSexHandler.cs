using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Entities.Sexes.Processors;

namespace JoyGodot.Assets.Scripts.Entities.Sexes
{
    public interface IEntityBioSexHandler : IHandler<IBioSex, string>
    {
        IBioSexProcessor GetProcessor(string name);
    }
}