using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Scripts.Quests
{
    public interface IQuestTracker : ISerialisationHandler
    {
        List<IQuest> AllQuests { get; }

        List<IQuest> GetQuestsForEntity(Guid GUID);

        IQuest GetPrimaryQuestForEntity(Guid GUID);

        void AddQuest(Guid GUID, IQuest quest);

        void CompleteQuest(IEntity questor, IQuest quest, bool force = false);

        void FailQuest(IEntity questor, IQuest quest);

        void AbandonQuest(IEntity questor, IQuest quest);

        void PerformQuestAction(IEntity questor, IQuest quest, IJoyAction completedAction);

        void PerformQuestAction(IEntity questor, IJoyAction completedAction);
    }
}