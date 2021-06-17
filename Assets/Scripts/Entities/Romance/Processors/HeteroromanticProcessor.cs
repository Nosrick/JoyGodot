using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Entities.Relationships;

namespace JoyGodot.Assets.Scripts.Entities.Romance.Processors
{
    public class HeteroromanticProcessor : IRomanceProcessor
    {
        public string Name => "heteroromantic";
        public bool WillRomance(IEntity me, IEntity them, IEnumerable<IRelationship> relationships)
        {
            if (relationships.Any() == false)
            {
                return false;
            }
            
            int highestValue = relationships.Max(relationship => relationship.GetRelationshipValue(me.Guid, them.Guid));
            if(highestValue < me.Romance.RomanceThreshold || me.Gender.Name.Equals(them.Gender.Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return me.Sentient == them.Sentient;
        }

        public bool Compatible(IEntity me, IEntity them)
        {
            return me.Sentient == them.Sentient
                   && me.Gender.Name.Equals(them.Gender.Name, StringComparison.OrdinalIgnoreCase) == false;
        }
    }
}