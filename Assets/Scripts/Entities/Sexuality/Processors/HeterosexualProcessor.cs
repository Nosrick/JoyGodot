using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Entities.Relationships;

namespace JoyGodot.Assets.Scripts.Entities.Sexuality.Processors
{
    public class HeterosexualProcessor : ISexualityPreferenceProcessor
    {
        public string Name => "heterosexual";
        
        public bool WillMateWith(IEntity left, IEntity right, IEnumerable<IRelationship> relationships)
        {
            if (relationships.Any() == false)
            {
                return false;
            }
            
            int highestValue = relationships.Max(relationship => relationship.GetRelationshipValue(left.Guid, right.Guid));
            if(highestValue < left.Sexuality.MatingThreshold
               || left.Gender.Name.Equals(right.Gender.Name))
            {
                return false;
            }
            return left.Sentient == right.Sentient;
        }

        public bool Compatible(IEntity left, IEntity right)
        {
            return left.Sentient == right.Sentient
                   && left.Gender.Name.Equals(right.Gender.Name, StringComparison.OrdinalIgnoreCase) == false;
        }
    }
}