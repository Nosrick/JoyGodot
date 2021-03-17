using System.Collections.Generic;
using JoyLib.Code.Entities;
using JoyLib.Code.World;

namespace JoyLib.Code.Quests
{
    public interface IQuestProvider
    {
        IQuest MakeRandomQuest(IEntity questor, IEntity provider, IWorldInstance overworldRef);

        IQuest MakeQuestOfType(IEntity questor, IEntity provider, IWorldInstance overworldRef, string[] tags);

        IEnumerable<IQuest> MakeOneOfEachType(IEntity questor, IEntity provider, IWorldInstance overworldRef);

        List<IQuestAction> Actions { get; }
    }
}