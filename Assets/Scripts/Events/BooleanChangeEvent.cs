using System;

namespace JoyLib.Code.Events
{
    public delegate void BooleanChangedEventHandler(object sender, BooleanChangeEventArgs args);
    
    public class BooleanChangeEventArgs : EventArgs
    {
        public bool Value { get; set; }
    }
}