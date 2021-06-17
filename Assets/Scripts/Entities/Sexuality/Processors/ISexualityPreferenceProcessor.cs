using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Entities.Relationships;

namespace JoyGodot.Assets.Scripts.Entities.Sexuality.Processors
{
    public interface ISexualityPreferenceProcessor
    {
        string Name { get; }
        
        bool WillMateWith(IEntity left, IEntity right, IEnumerable<IRelationship> relationships);
        
        bool Compatible(IEntity left, IEntity right);
    }
}