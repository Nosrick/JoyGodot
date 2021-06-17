using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Events;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Managed_Assets;
using JoyGodot.Assets.Scripts.Rollers;

namespace JoyGodot.Assets.Scripts.Entities.Needs
{
    public interface INeed : IBasicValue<int>
    {
        event ValueChangedEventHandler<int> ValueChanged; 
        bool FindFulfilmentObject(IEntity actor);

        bool Interact(IEntity actor, IJoyObject obj);

        INeed Copy();

        INeed Randomise();

        bool Tick(Entity actor);

        int Fulfill(int value);

        int Decay(int value);

        //Name and Value come from IBasicValue

        int Priority
        {
            get;
        }

        bool ContributingHappiness
        {
            get;
        }

        int AverageForDay
        {
            get;
        }

        int AverageForWeek
        {
            get;
        }

        int AverageForMonth
        {
            get;
        }

        ISpriteState FulfillingSprite
        {
            get;
            set;
        }

        int HappinessThreshold
        {
            get;
        }

        float PercentageFull
        {
            get;
        }
        
        RNG Roller { get; }
    }
}
