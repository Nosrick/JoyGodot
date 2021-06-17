using System;
using JoyGodot.Assets.Scripts.Entities.Jobs;

namespace JoyGodot.Assets.Scripts.Events
{
    public delegate void JobChangedEventHandler(object sender, JobChangedEventArgs args);
    
    public class JobChangedEventArgs
    {
        public IJob NewJob { get; set; }
        public Guid GUID { get; set; }
    }
}