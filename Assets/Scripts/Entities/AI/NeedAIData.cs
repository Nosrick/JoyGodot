using System;

namespace JoyLib.Code.Entities.AI
{
    public class NeedAIData
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
    }

    public class NeedDataSerialisable
    {
        public Guid targetGuid;
        public string targetType;
        public Vector2Int targetPoint;
        public bool searching;
        public Intent intent;
        public bool idle;
        public string need;
    }
}
