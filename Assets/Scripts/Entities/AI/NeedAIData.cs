using System;
using Godot.Collections;

namespace JoyLib.Code.Entities.AI
{
    public class NeedAIData : ISerialisationHandler
    {
        public IJoyObject target;
        public Vector2Int targetPoint;
        public bool searching;
        public Intent intent;
        public bool idle;
        public string need;

        public static NeedAIData IdleState()
        {
            return new NeedAIData
            {
                target = null,
                targetPoint = GlobalConstants.NO_TARGET,
                searching = false,
                idle = true,
                intent = Intent.Interact,
                need = "none"
            };
        }

        public static NeedAIData SearchingState()
        {
            return new NeedAIData
            {
                target = null,
                targetPoint = GlobalConstants.NO_TARGET,
                searching = true,
                idle = false,
                intent = Intent.Interact,
                need = "none"
            };
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"TargetPoint", this.targetPoint.Save()},
                {"Target", this.target?.Guid.ToString()},
                {"Searching", this.searching},
                {"Intent", this.intent.ToString()},
                {"Idle", this.idle},
                {"Need", this.need}
            };
            
            return saveDict;
        }

        public void Load(string data)
        {
            throw new NotImplementedException();
        }
    }
}
