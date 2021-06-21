using System.Collections.Generic;
using System.Linq;

using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Data.Scripts.Actions
{
    public class ModifyRelationshipPointsAction : AbstractAction
    {
        public override string Name => "modifyrelationshippointsaction";

        public override string ActionString => "modification of relationship points";

        protected IEntityRelationshipHandler RelationshipHandler { get; set; }

        public ModifyRelationshipPointsAction()
        {
            if (GlobalConstants.GameManager is null == false)
            {
                this.RelationshipHandler = GlobalConstants.GameManager.RelationshipHandler;
            }
        }

        public override bool Execute(IJoyObject[] participants, IEnumerable<string> tags = null,
            IDictionary<string, object> args = null)
        {
            this.ClearLastParameters();
            
            if (args.IsNullOrEmpty())
            {
                return false;
            }

            if (participants.Distinct().Count() != participants.Length)
            {
                return false;
            }

            int relationshipMod = args.TryGetValue("value", out object arg) ? (int) arg : 0;

            if (this.RelationshipHandler is null)
            {
                return false;
            }

            IEnumerable<IRelationship> relationships = this.RelationshipHandler?.Get(
                participants.Select(o => o.Guid), 
                tags, 
                true);

            bool doAll = args.TryGetValue("doAll", out arg) && (bool) arg;

            if(relationships.Any())
            {
                foreach(IRelationship relationship in relationships)
                {
                    if (doAll)
                    {
                        relationship.ModifyValueOfAllParticipants(relationshipMod);
                    }
                    else
                    {
                        relationship.ModifyValueOfOtherParticipants(participants[0].Guid, relationshipMod);
                    }
                }

                this.SetLastParameters(participants, tags, args);

                return true;
            }

            return false;
        }
    }
}