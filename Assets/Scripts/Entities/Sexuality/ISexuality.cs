using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Entities.Relationships;

namespace JoyGodot.Assets.Scripts.Entities.Sexuality
{
    public interface ISexuality : ITagged, ISerialisationHandler
    {
        bool WillMateWith(IEntity me, IEntity them, IEnumerable<IRelationship> relationships);
        bool Compatible(IEntity me, IEntity them);

        string Name
        {
            get;
        }

        bool DecaysNeed
        {
            get;
        }

        int MatingThreshold
        {
            get;
            set;
        }
    }
}
