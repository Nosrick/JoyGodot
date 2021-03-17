using System;
using System.Collections.Generic;
using JoyLib.Code.Conversation.Conversations;

namespace JoyLib.Code.Conversation.Subengines.Rumours
{
    public interface IRumour
    {
        IJoyObject[] Participants
        {
            get;
        }

        string[] Tags
        {
            get;
        }

        float ViralPotential
        {
            get;
        }

        ITopicCondition[] Conditions
        {
            get;
        }

        string[] Parameters
        {
            get;
        }

        string Words
        {
            get;
        }

        bool Baseless
        {
            get;
        }

        float LifetimeMultiplier
        {
            get;
        }

        int Lifetime
        {
            get;
        }

        bool IsAlive
        {
            get;
        }

        bool FulfilsConditions(IEnumerable<Tuple<string, int>> values);
        bool FulfilsConditions(IEnumerable<IJoyObject> participants);

        int Tick();

        string ConstructString();

        IRumour Create(
            IEnumerable<IJoyObject> participants,
            IEnumerable<string> tags,
            float viralPotential,
            IEnumerable<ITopicCondition> conditions,
            IEnumerable<string> parameters,
            string words,
            float lifetimeMultiplier = 1F,
            int lifetime = 5000,
            bool baseless = false);
    }
}
