using System;
using System.Collections.Generic;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Scripting;

namespace JoyLib.Code.Quests
{
    public interface IQuest : ITagged, ISerialisationHandler
    {
        List<IQuestAction> Actions { get; }
        QuestMorality Morality { get; }
        
        List<Guid> RewardGUIDs { get; }
        
        List<IItemInstance> Rewards { get; }
        int CurrentStep { get; }
        
        Guid Instigator { get; }
        
        Guid Questor { get; }
        
        Guid ID { get; }
        
        bool IsComplete { get; }

        bool BelongsToThis(object searchTerm);
        bool AdvanceStep();

        bool FulfilsRequirements(IEntity questor, IJoyAction action);

        void StartQuest(IEntity questor);

        bool CompleteQuest(IEntity questor);
    }
}