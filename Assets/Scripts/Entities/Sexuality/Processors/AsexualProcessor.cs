using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Entities.Relationships;

namespace JoyGodot.Assets.Scripts.Entities.Sexuality.Processors
{
    public class AsexualProcessor : ISexualityPreferenceProcessor
    {
        public string Name => "asexual";
        
        public bool WillMateWith(IEntity left, IEntity right, IEnumerable<IRelationship> relationships)
        {
            return false;
        }

        public bool Compatible(IEntity left, IEntity right)
        {
            return false;
        }
    }
}