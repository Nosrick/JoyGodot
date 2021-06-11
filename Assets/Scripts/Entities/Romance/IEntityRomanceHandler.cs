using JoyLib.Code.Entities.Romance.Processors;

namespace JoyLib.Code.Entities.Romance
{
    public interface IEntityRomanceHandler : IHandler<IRomance, string>
    {
        IRomanceProcessor GetProcessor(string name);
    }
}