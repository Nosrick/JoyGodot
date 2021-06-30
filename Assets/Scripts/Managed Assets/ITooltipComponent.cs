using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Managed_Assets
{
    public interface ITooltipComponent : ITooltipHolder
    {
        bool MouseOver { get; }
        void OnPointerEnter();
        void OnPointerExit();
    }
}