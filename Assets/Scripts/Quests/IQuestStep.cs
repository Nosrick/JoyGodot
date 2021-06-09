using System;
using System.Collections.Generic;
using JoyLib.Code.Entities;

namespace JoyLib.Code.Quests
{
    public interface IQuestStep : ITagged, ISerialisationHandler
    {
        IQuestAction Action { get; }
        List<Guid> Items { get; }
        List<Guid> Actors { get; }
        List<Guid> Areas { get; }

        void StartQuest(IEntity questor);
    }
}