//This identifies whether a creature can give birth or not, etc

using System.Collections.Generic;

namespace JoyLib.Code.Entities.Sexes
{
    public interface IBioSex
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
