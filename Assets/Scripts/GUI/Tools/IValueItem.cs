using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyGodot.Assets.Scripts.GUI.Tools
{
    public interface IValueItem<T> : ITooltipComponent
    {
        ICollection<T> Values { get; set; }
        T Value { get; set; }
        int Minimum { get; set; }
        int Maximum { get; set; }
    }
}