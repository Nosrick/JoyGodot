﻿using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Entities.Statistics
{
    public interface IBasicValue<T> : 
        ITooltipHolder, 
        ISerialisationHandler
    {
        string Name
        {
            get;
        }

        T Value
        {
            get;
        }

        T ModifyValue(T value);
        T SetValue(T value);
    }
}
