using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.Quests.Actions;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.Scripting;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.Quests
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

            this.Actions = GlobalConstants.ScriptingEngine.FetchAndInitialiseChildren<IQuestAction>().ToList();
        }

        protected void Initialise()
        {
            this.EntityRelationshipHandler = GlobalConstants.GameManager.RelationshipHandler;

            this.Actions = GlobalConstants.ScriptingEngine.FetchAndInitialiseChildren<IQuestAction>().ToList();
        }

        public IQuest MakeRandomQuest(IEntity questor, IEntity provider, IWorldInstance overworldRef)
        {
            GlobalConstants.GameManager.ItemHandler.CleanUpRewards();
            
            List<IQuestAction> actions = new List<IQuestAction>();

            //int numberOfSteps = RNG.instance.Roll(1, 4);
            int numberOfSteps = 1;
            for (int i = 0; i < numberOfSteps; i++)
            {
                int result = this.Roller.Roll(0, this.Actions.Count);
                IQuestAction action = this.Actions[result].Create(questor, provider, overworldRef, new string[0]);
                actions.Add(action);
            }

            IEnumerable<string> tagsForAllSteps = actions.SelectMany(step => step.Tags);
            var rewards = this.GetRewards(questor, provider, actions);
            Quest quest = new Quest(
                actions, 
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
                List<IQuestAction> steps = new List<IQuestAction>{action.Create(questor, provider, overworldRef, new string[0])};
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

        private List<IItemInstance> GetRewards(IEntity questor, IEntity provider, List<IQuestAction> actions)
        {
            List<IItemInstance> rewards = new List<IItemInstance>();
            int reward = ((actions.Count * 100) + 
                          (this.EntityRelationshipHandler.GetHighestRelationshipValue(provider.Guid, questor.Guid)));
            rewards.Add(this.BagOfGoldHelper.GetBagOfGold(reward));
            foreach (IItemInstance item in rewards)
            {
                item.SetOwner(questor.Guid);
            }

            return rewards;
        }
    }
}
