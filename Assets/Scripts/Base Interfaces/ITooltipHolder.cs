using System.Collections.Generic;

namespace JoyGodot.Assets.Scripts.Base_Interfaces
{
    public interface ITooltipHolder
    {
        ICollection<string> Tooltip { get; set; }
    }
}