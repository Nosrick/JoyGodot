using JoyLib.Code.Entities.Sexes.Processors;

namespace JoyLib.Code.Entities.Sexes
{
    public interface IEntityBioSexHandler : IHandler<IBioSex, string>
    {
        IBioSexProcessor GetProcessor(string name);
    }
}