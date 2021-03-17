using System;
using System.Collections.Generic;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Rollers;
using JoyLib.Code.Scripting;
using JoyLib.Code.World;

namespace JoyLib.Code.Quests
{
    public interface IQuestAction
    {
        string[] Tags { get; }
        string Description { get; }
        List<Guid> Items { get; }
        List<Guid> Actors { get; }
        List<Guid> Areas { get; }
        
        RNG Roller { get; }

        IQuestStep Make(IEntity questor, IEntity provider, IWorldInstance overworld, IEnumerable<string> tags);
        bool ExecutedSuccessfully(IJoyAction action);

        string AssembleDescription();

        void ExecutePrerequisites(IEntity questor);

        IQuestAction Create(
            IEnumerable<string> tags,
            IEnumerable<IItemInstance> items,
            IEnumerable<IJoyObject> actors,
            IEnumerable<IWorldInstance> areas,
            IItemFactory itemFactory = null);
    }
}