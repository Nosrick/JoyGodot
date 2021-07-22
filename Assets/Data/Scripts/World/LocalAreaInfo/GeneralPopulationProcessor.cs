using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.World;
using JoyGodot.Assets.Scripts.World.WorldInfo;

namespace JoyGodot.Assets.Data.Scripts.World.LocalAreaInfo
{
    public class GeneralPopulationProcessor : AbstractLocalAreaProcessor
    {
        public override string Get(IWorldInstance worldInstance, IEntity origin = null)
        {
            var entities = worldInstance.Entities;

            int count = entities.Count;
            if (count > 10)
            {
                int remainder = count % 5;
                count -= remainder;
            }

            bool plural = count > 1;

            return "There" +
                   (plural ? " are " : " is ") +
                   (plural ? "around " : "") +
                   count +
                   (plural ? " people " : " person ") +
                   "here.";
        }
    }
}