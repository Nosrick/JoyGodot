using System;

namespace JoyGodot.Assets.Scripts.Events
{
    public delegate void ValueChangedEventHandler<T>(object sender, ValueChangedEventArgs<T> args);
    
    public class ValueChangedEventArgs<T> : EventArgs
    {
        public string Name { get; set; }
        public T NewValue { get; set; }
        public T Delta { get; set; }
    }
}