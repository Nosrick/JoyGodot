using System.Collections.Generic;
using JoyLib.Code.Entities.Relationships;

namespace JoyLib.Code.Entities.Romance.Processors
{
    public interface IRomanceProcessor
    {
        string Name { get; }
        
        bool WillRomance(IEntity me, IEntity them, IEnumerable<IRelationship> relationships);
        bool Compatible(IEntity me, IEntity them);
    }
}