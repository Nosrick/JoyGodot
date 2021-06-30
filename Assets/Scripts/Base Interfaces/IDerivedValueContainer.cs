using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Events;

namespace JoyGodot.Assets.Scripts.Base_Interfaces
{
    public interface IDerivedValueContainer
    {
        IDictionary<string, IDerivedValue> DerivedValues { get; }
        int DamageValue(string name, int value);
        int RestoreValue(string name, int value);
        int ModifyValue(string name, int value);
        int SetValue(string name, int value);
        int GetValue(string name);
        int GetMaximum(string name);
        int SetBase(string name, int value);
        int SetEnhancement(string name, int value);

        event ValueChangedEventHandler<int> OnDerivedValueChange;
        event ValueChangedEventHandler<int> OnMaximumChange;
    }
}