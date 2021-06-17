using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.Scripting;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.Quests.Actions
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