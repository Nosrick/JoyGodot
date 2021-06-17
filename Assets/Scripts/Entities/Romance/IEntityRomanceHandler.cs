using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Entities.Romance.Processors;

namespace JoyGodot.Assets.Scripts.Entities.Romance
{
    public interface IEntityRomanceHandler : IHandler<IRomance, string>
    {
        IRomanceProcessor GetProcessor(string name);
    }
}