using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Entities.Relationships;
using JoyLib.Code.Helpers;
using JoyLib.Code.Rollers;
using JoyLib.Code.Scripting;
using JoyLib.Code.World;

namespace JoyLib.Code.Quests
{
    public class QuestProvider : IQuestProvider
    {
        protected IEntityRelationshipHandler EntityRelationshipHandler { get; set; }
        
        protected RNG Roller { get; set; }

        public List<IQuestAction> Actions { get; protected set; }
        
        protected BagOfGoldHelper BagOfGoldHelper { get; set; }

        public QuestProvider(
            IEntityRelationshipHandler entityRelationshipHandler,
            ILiveItemHandler itemHandler,
            IItemFactory itemFactory,
            RNG roller)
        {
            this.BagOfGoldHelper = new BagOfGoldHelper(itemHandler, itemFactory);
            this.Roller = roller;
            this.EntityRelationshipHandler = entityRelationshipHandler;

            this.Actions = ScriptingEngine.Instance.FetchAndInitialiseChildren<IQuestAction>().ToList();
        }

        protected void Initialise()
        {
            this.EntityRelationshipHandler = GlobalConstants.GameManager.RelationshipHandler;

            this.Actions = ScriptingEngine.Instance.FetchAndInitialiseChildren<IQuestAction>().ToList();
        }

        public IQuest MakeRandomQuest(IEntity questor, IEntity provider, IWorldInstance overworldRef)
        {
            GlobalConstants.GameManager.ItemHandler.CleanUpRewards();
            
            List<IQuestStep> steps = new List<IQuestStep>();

            //int numberOfSteps = RNG.instance.Roll(1, 4);
            int numberOfSteps = 1;
            for (int i = 0; i < numberOfSteps; i++)
            {
                int result = this.Roller.Roll(0, this.Actions.Count);
                IQuestAction action = this.Actions[result].Create(
                    new string[0],
                    new List<IItemInstance>(),
                    new List<IJoyObject>(),
                    new List<IWorldInstance>());
                steps.Add(action.Make(questor, provider, overworldRef, action.Tags));
            }

            IEnumerable<string> tagsForAllSteps = steps.SelectMany(step => step.Tags);
            var rewards = this.GetRewards(questor, provider, steps);
            Quest quest = new Quest(
                steps, 
                QuestMorality.Neutral, 
                rewards, 
                provider.Guid, 
                questor.Guid, 
                tagsForAllSteps);

            return quest;
        }

        public IEnumerable<IQuest> MakeOneOfEachType(IEntity questor, IEntity provider, IWorldInstance overworldRef)
        {
            List<IQuest> quests = new List<IQuest>();

            foreach (IQuestAction action in this.Actions)
            {
                IQuestAction newAction = action.Create(
                    new string[0],
                    new List<IItemInstance>(),
                    new List<IJoyObject>(),
                    new List<IWorldInstance>());
                List<IQuestStep> steps = new List<IQuestStep>{newAction.Make(questor, provider, overworldRef, newAction.Tags)};
                quests.Add(new Quest(
                    steps, 
                    QuestMorality.Neutral, 
                    this.GetRewards(questor, provider, steps), 
                    provider.Guid, 
                    questor.Guid, 
                    new string[0]));
            }

            return quests;
        }

        public IQuest MakeQuestOfType(IEntity questor, IEntity provider, IWorldInstance overworldRef, string[] tags)
        {
            throw new System.NotImplementedException();
        }

        private List<IItemInstance> GetRewards(IEntity questor, IEntity provider, List<IQuestStep> steps)
        {
            List<IItemInstance> rewards = new List<IItemInstance>();
            int reward = ((steps.Count * 100) + (this.EntityRelationshipHandler.GetHighestRelationshipValue(provider, questor)));
            rewards.Add(this.BagOfGoldHelper.GetBagOfGold(reward));
            foreach (IItemInstance item in rewards)
            {
                item.SetOwner(questor.Guid);
            }

            return rewards;
        }
    }
}
