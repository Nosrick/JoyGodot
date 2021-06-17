using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Entities.Relationships;

namespace JoyGodot.Assets.Scripts.Entities.Romance.Processors
{
    public interface IRomanceProcessor
    {
        string Name { get; }
        
        bool WillRomance(IEntity me, IEntity them, IEnumerable<IRelationship> relationships);
        bool Compatible(IEntity me, IEntity them);
    }
}