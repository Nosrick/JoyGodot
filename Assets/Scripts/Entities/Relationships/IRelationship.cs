using System;
using System.Collections.Generic;

namespace JoyLib.Code.Entities.Relationships
{
    public interface IRelationship : 
        ITagged,
        ISerialisationHandler
    {
        IDictionary<Guid, int> GetValuesOfParticipant(Guid GUID);

        int GetRelationshipValue(Guid left, Guid right);

        int GetHighestRelationshipValue(Guid GUID);
        
        IJoyObject GetParticipant(Guid GUID);
        IEnumerable<IJoyObject> GetParticipants();

        int ModifyValueOfParticipant(Guid actor, Guid observer, int value);

        int ModifyValueOfOtherParticipants(Guid actor, int value);

        int ModifyValueOfAllParticipants(int value);

        bool AddParticipant(Guid newParticipant);
        bool AddParticipants(IEnumerable<Guid> participants);
        
        bool RemoveParticipant(Guid currentGUID);

        long GenerateHashFromInstance();

        IRelationship Create(IEnumerable<IJoyObject> participants);
        
        IRelationship CreateWithValue(IEnumerable<IJoyObject> participants, int value);

        string Name { get; }

        string DisplayName { get; }
        
        HashSet<string> UniqueTags { get; }
        
        int MaxParticipants { get; }
    }
}
