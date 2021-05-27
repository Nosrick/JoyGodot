using System.Collections.Generic;

namespace JoyGodot.Assets.Scripts.Managed_Assets
{
    public interface ITooltipComponent
    {
        ICollection<string> Tooltip { get; set; }
        
        bool MouseOver { get; }

        void OnPointerEnter();
        void OnPointerExit();
    }
}