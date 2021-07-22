using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.Conversation.Subengines.Rumours
{
    public interface IRumourMill : IDisposable
    {
        List<IRumour> Rumours
        {
            get;
        }

        List<IRumour> RumourTypes
        {
            get;
        }
        
        RNG Roller { get; }

        IRumour GenerateRandomRumour(IEnumerable<IJoyObject> participants);

        IRumour GenerateRumourFromTags(IEnumerable<IJoyObject> participants, IEnumerable<string> tags, bool includeBaseless = false);

        IRumour[] GenerateOneRumourOfEachType(IJoyObject[] participants);

        IRumour GetRandom(IWorldInstance overworldRef);

        bool PropagateRumour(IEnumerable<IJoyObject> participants, IEnumerable<string> tags, bool force = false);
    }
}
