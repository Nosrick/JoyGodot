using System;

namespace JoyGodot.Assets.Scripts.Events
{
    public delegate void BooleanChangedEventHandler(object sender, BooleanChangeEventArgs args);
    
    public class BooleanChangeEventArgs : EventArgs
    {
        public bool Value { get; set; }
    }
}