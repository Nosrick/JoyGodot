using System;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Scripts.Events
{
    public delegate void JoyObjectMouseOverHandler(object sender, JoyObjectMouseOverEventArgs args);
    public delegate void JoyObjectMouseExitHandler(object sender, EventArgs args);

    public class JoyObjectMouseOverEventArgs : EventArgs
    {
        public IJoyObject Actor { get; set; }
    }
}