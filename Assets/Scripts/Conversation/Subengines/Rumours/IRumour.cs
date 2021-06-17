using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Scripts.Conversation.Subengines.Rumours
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

        bool FulfilsConditions(IEnumerable<Tuple<string, object>> values);
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
