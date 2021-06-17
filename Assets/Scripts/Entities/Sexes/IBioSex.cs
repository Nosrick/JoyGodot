//This identifies whether a creature can give birth or not, etc

using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Entities.Sexes
{
    public interface IBioSex: ISerialisationHandler
    {
        bool CanBirth
        {
            get;
        }

        bool CanFertilise
        {
            get;
        }

        string Name
        {
            get;
        }

        IEntity CreateChild(IEnumerable<IEntity> parents);
    }
}
