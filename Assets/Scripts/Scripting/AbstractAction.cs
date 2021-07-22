using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Conversation.Subengines.Rumours;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Quests;

namespace JoyGodot.Assets.Scripts.Scripting
{
    public abstract class AbstractAction : IJoyAction
    {
        public virtual string Name => "abstractaction";
        public virtual string ActionString => "SOMEONE FORGOT TO OVERRIDE THE ACTIONSTRING";
        public IEnumerable<IJoyObject> LastParticipants { get; protected set; }
        public IEnumerable<string> LastTags { get; protected set; }
        public IDictionary<string, object> LastArgs { get; protected set; }
        public bool Successful { get; protected set; }
        public IQuestTracker QuestTracker { get; set; }
        public IRumourMill RumourMill { get; set; }
        protected AbstractAction(
            IQuestTracker questTracker = null,
            IRumourMill rumourMill = null)
        {
            this.QuestTracker = questTracker ?? GlobalConstants.GameManager?.QuestTracker;
            this.RumourMill = rumourMill ?? GlobalConstants.GameManager?.RumourMill;
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

            if (this.LastParticipants.Any(o => o.Equals(GlobalConstants.GameManager.Player) == false)
                && this.LastParticipants.Count() > 1)
            {
                this.RumourMill.PropagateRumour(this.LastParticipants, this.LastTags);
            }

            this.QuestTracker?.PerformQuestAction(this.LastParticipants.First() as IEntity, this);
        }
    }
}