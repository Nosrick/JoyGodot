using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Scripts.Entities.Relationships
{
    public interface IEntityRelationshipHandler : 
        IHandler<IRelationship, long>,
        ISerialisationHandler
    {
        IRelationship CreateRelationship(IEnumerable<Guid> participants, IEnumerable<string> tags);
        IRelationship CreateRelationshipWithValue(IEnumerable<Guid> participants, IEnumerable<string> tags, int value);
        IEnumerable<IRelationship> Get(IEnumerable<Guid> participants, IEnumerable<string> tags = null,
            bool createNewIfNone = false);
        int GetHighestRelationshipValue(Guid speaker, Guid listener, IEnumerable<string> tags = null);
        IRelationship GetBestRelationship(Guid speaker, Guid listener, IEnumerable<string> tags = null);
        IEnumerable<IRelationship> GetAllForObject(Guid actor);
        bool IsFamily(Guid speaker, Guid listener);
        List<IRelationship> RelationshipTypes { get; }
    }
}