using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Entities;
using JoyLib.Code.Quests;

namespace JoyLib.Code.Scripting
{
    public abstract class AbstractAction : IJoyAction
    {
        protected AbstractAction(IQuestTracker questTracker = null)
        {
            this.QuestTracker = questTracker ?? GlobalConstants.GameManager?.QuestTracker;
        }
        
        public abstract bool Execute(
            IJoyObject[] participants, 
            IEnumerable<string> tags = null,
            IDictionary<string, object> args = null);

        public void ClearLastParameters()
        {
            this.LastParticipants = null;
            this.LastTags = null;
            this.LastArgs = null;
            this.Successful = false;
        }

        public virtual void SetLastParameters(
            IEnumerable<IJoyObject> participants, 
            IEnumerable<string> tags = null,
            IDictionary<string, object> args = null)
        {
            this.LastParticipants = participants;
            this.LastTags = tags;
            this.LastArgs = args;
            this.Successful = true;

            this.QuestTracker?.PerformQuestAction(this.LastParticipants.First() as IEntity, this);
        }

        public virtual string Name => "abstractaction";
        public virtual string ActionString => "SOMEONE FORGOT TO OVERRIDE THE ACTIONSTRING";
        public IEnumerable<IJoyObject> LastParticipants { get; protected set; }
        public IEnumerable<string> LastTags { get; protected set; }
        public IDictionary<string, object> LastArgs { get; protected set; }
        public bool Successful { get; protected set; }
        public IQuestTracker QuestTracker { get; set; }
    }
}