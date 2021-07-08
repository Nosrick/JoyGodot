using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.Quests.Actions;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Scripts.Quests
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

        bool CompleteQuest(IEntity questor, bool force = false);
    }
}