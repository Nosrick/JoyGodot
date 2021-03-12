using System.Collections.Generic;
using JoyLib.Code.Entities.Relationships;

namespace JoyLib.Code.Entities.Romance.Processors
{
    public class AromanticProcessor : IRomanceProcessor
    {
        public string Name => "aromantic";
        
        public bool WillRomance(IEntity me, IEntity them, IEnumerable<IRelationship> relationships)
        {
            return false;
        }

        public bool Compatible(IEntity me, IEntity them)
        {
            return false;
        }
    }
}