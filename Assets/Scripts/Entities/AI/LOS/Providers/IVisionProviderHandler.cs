using Godot;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Entities.AI.LOS.Boards;

namespace JoyGodot.Assets.Scripts.Entities.AI.LOS.Providers
{
    public interface IVisionProviderHandler : IHandler<IVision, string>
    {
        bool AddVision(string name,
            Color darkColour,
            Color lightColour,
            IFOVHandler algorithm,
            int minimumLightLevel = 0,
            int minimumComfortLevel = 0,
            int maximumLightLevel = 32, int maximumComfortLevel = 32);

        bool AddVision(IVision vision);

        bool HasVision(string name);
    }
}