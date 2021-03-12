using System.Collections.Generic;
using JoyLib.Code.Entities.Relationships;

namespace JoyLib.Code.Entities.Romance.Processors
{
    public class PanromanticProcessor : IRomanceProcessor
    {
        public string Name => "panromantic";
        
        public bool WillRomance(IEntity me, IEntity them, IEnumerable<IRelationship> relationships)
        {
            return true;
        }

        public bool Compatible(IEntity me, IEntity them)
        {
            return true;
        }
    }
}