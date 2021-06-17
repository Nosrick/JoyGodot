using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Quests.Actions;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.Quests
{
    public interface IQuestProvider
    {
        IQuest MakeRandomQuest(IEntity questor, IEntity provider, IWorldInstance overworldRef);

        IQuest MakeQuestOfType(IEntity questor, IEntity provider, IWorldInstance overworldRef, string[] tags);

        IEnumerable<IQuest> MakeOneOfEachType(IEntity questor, IEntity provider, IWorldInstance overworldRef);

        List<IQuestAction> Actions { get; }
    }
}