using System.Collections;
using System.Collections.Generic;
using Godot;

namespace JoyLib.Code.Unity.GUI
{
    public interface IValueItem<T>
    {
        ICollection<T> Values { get; set; }
        T Value { get; set; }
        int Minimum { get; set; }
        int Maximum { get; set; }
    }
}