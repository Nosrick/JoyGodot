using System.Collections;
using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyLib.Code.Unity.GUI
{
    public interface IValueItem<T> : ITooltipComponent
    {
        ICollection<T> Values { get; set; }
        T Value { get; set; }
        int Minimum { get; set; }
        int Maximum { get; set; }
    }
}