using System.Collections.Generic;
using JoyLib.Code.Entities.Relationships;

namespace JoyLib.Code.Entities.Sexuality
{
    public interface ISexualityPreferenceProcessor
    {
        string Name { get; }
        
        bool WillMateWith(IEntity left, IEntity right, IEnumerable<IRelationship> relationships);
        
        bool Compatible(IEntity left, IEntity right);
    }
}