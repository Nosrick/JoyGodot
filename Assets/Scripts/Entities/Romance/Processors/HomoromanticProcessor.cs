﻿using System;
using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Entities.Relationships;

namespace JoyLib.Code.Entities.Romance.Processors
{
    public class HomoromanticProcessor : IRomanceProcessor
    {
        public string Name => "homoromantic";
        public bool WillRomance(IEntity me, IEntity them, IEnumerable<IRelationship> relationships)
        {
            if (relationships.Any() == false)
            {
                return false;
            }

            int highestValue = relationships.Max(relationship => relationship.GetRelationshipValue(me.Guid, them.Guid));
            if(highestValue < me.Romance.RomanceThreshold 
               || me.Gender.Name.Equals(them.Gender.Name, StringComparison.OrdinalIgnoreCase) == false)
            {
                return false;
            }
            return me.Sentient == them.Sentient;
        }

        public bool Compatible(IEntity me, IEntity them)
        {
            return me.Sentient == them.Sentient
                   && me.Gender.Name.Equals(them.Gender.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}