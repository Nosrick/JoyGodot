using System.Collections.Generic;
using JoyLib.Code.Quests;

namespace JoyLib.Code.Scripting
{
    public interface IJoyAction
    {
        bool Execute(
            IJoyObject[] participants, 
            IEnumerable<string> tags = null, 
            IDictionary<string, object> args = null);
        void SetLastParameters(
            IEnumerable<IJoyObject> participants, 
            IEnumerable<string> tags = null, 
            IDictionary<string, object> args = null);

        void ClearLastParameters();

        string Name
        {
            get;
        }

        string ActionString
        {
            get;
        }

        IEnumerable<IJoyObject> LastParticipants
        {
            get;
        }

        IEnumerable<string> LastTags
        {
            get;
        }

        IDictionary<string, object> LastArgs
        {
            get;
        }

        bool Successful
        {
            get;
        }
        
        IQuestTracker QuestTracker { get; set; }
    }
}