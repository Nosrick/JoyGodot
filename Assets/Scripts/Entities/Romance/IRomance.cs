using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Entities.Relationships;

namespace JoyGodot.Assets.Scripts.Entities.Romance
{
    public interface IRomance : ITagged, ISerialisationHandler
    {
        bool WillRomance(IEntity me, IEntity them, IEnumerable<IRelationship> relationships);
        bool Compatible(IEntity me, IEntity them);

        string Name { get; }
        
        bool DecaysNeed { get; }
        
        int RomanceThreshold { get; set; }
        
        int BondingThreshold { get; set; }
    }
}