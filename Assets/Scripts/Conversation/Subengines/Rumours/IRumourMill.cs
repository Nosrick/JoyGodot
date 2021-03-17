using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JoyLib.Code.Rollers;
using JoyLib.Code.World;

namespace JoyLib.Code.Conversation.Subengines.Rumours
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

        IRumour GenerateRandomRumour(IJoyObject[] participants);

        IRumour GenerateRumourFromTags(IJoyObject[] participants, string[] tags);

        IRumour[] GenerateOneRumourOfEachType(IJoyObject[] participants);

        IRumour GetRandom(IWorldInstance overworldRef);
    }
}
