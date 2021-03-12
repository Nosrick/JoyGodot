using System.Collections.Generic;
using JoyLib.Code.Entities.Relationships;

namespace JoyLib.Code.Entities.Sexuality
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