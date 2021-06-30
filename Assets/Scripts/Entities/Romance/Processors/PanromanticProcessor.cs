using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Entities.Relationships;

namespace JoyGodot.Assets.Scripts.Entities.Romance.Processors
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