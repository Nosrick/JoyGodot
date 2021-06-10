using System.Collections.Generic;
using JoyLib.Code.Collections;

namespace JoyLib.Code.Entities.Relationships
{
    public interface IEntityRelationshipHandler : 
        IHandler<IRelationship, long>,
        ISerialisationHandler
    {
        bool Add(IRelationship relationship);
        bool Destroy(long ID);
        
        IRelationship CreateRelationship(IEnumerable<IJoyObject> participants, IEnumerable<string> tags);
        IRelationship CreateRelationshipWithValue(IEnumerable<IJoyObject> participants, IEnumerable<string> tags, int value);
        IEnumerable<IRelationship> Get(IEnumerable<IJoyObject> participants, IEnumerable<string> tags = null, bool createNewIfNone = false);
        int GetHighestRelationshipValue(IJoyObject speaker, IJoyObject listener, IEnumerable<string> tags = null);
        IRelationship GetBestRelationship(IJoyObject speaker, IJoyObject listener, IEnumerable<string> tags = null);
        IEnumerable<IRelationship> GetAllForObject(IJoyObject actor);
        bool IsFamily(IJoyObject speaker, IJoyObject listener);
        
        List<IRelationship> RelationshipTypes { get; }
    }
}