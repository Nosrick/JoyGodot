using System;
using System.Collections.Generic;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Rollers;
using JoyLib.Code.Scripting;
using JoyLib.Code.World;

namespace JoyLib.Code.Quests
{
    public interface IQuestAction : ISerialisationHandler
    {
        string[] Tags { get; }
        string Description { get; }
        List<Guid> Items { get; }
        List<Guid> Actors { get; }
        List<Guid> Areas { get; }
        
        RNG Roller { get; }
        bool ExecutedSuccessfully(IJoyAction action);

        string AssembleDescription();

        void ExecutePrerequisites(IEntity questor);

        IQuestAction Create(
            IEntity questor, 
            IEntity provider, 
            IWorldInstance overworld, 
            IEnumerable<string> tags);
    }
}