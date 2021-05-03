using System.Collections.Generic;

namespace JoyGodot.Assets.Scripts.Managed_Assets
{
    public interface ITooltipComponent
    {
        ICollection<string> Tooltip { get; set; }

        void OnPointerEnter();
        void OnPointerExit();
    }
}