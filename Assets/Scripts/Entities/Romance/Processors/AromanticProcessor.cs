using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Entities.Relationships;

namespace JoyGodot.Assets.Scripts.Entities.Romance.Processors
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