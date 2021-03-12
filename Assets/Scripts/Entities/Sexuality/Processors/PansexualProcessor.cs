using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Entities.Relationships;

namespace JoyLib.Code.Entities.Sexuality
{
    public class PansexualProcessor : ISexualityPreferenceProcessor
    {
        public string Name => "pansexual";
        public bool WillMateWith(IEntity left, IEntity right, IEnumerable<IRelationship> relationships)
        {
            if (relationships.Any() == false)
            {
                return false;
            }
            
            int highestValue = relationships.Max(relationship => relationship.GetRelationshipValue(left.Guid, right.Guid));
            if(highestValue < left.Sexuality.MatingThreshold)
            {
                return false;
            }
            return left.Sentient == right.Sentient;
        }

        public bool Compatible(IEntity left, IEntity right)
        {
            return left.Sentient == right.Sentient;
        }
    }
}