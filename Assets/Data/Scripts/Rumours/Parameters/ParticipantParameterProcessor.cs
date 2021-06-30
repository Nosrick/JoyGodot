using System;
using JoyGodot.Assets.Scripts.Conversation.Subengines.Rumours.Parameters;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Data.Scripts.Rumours.Parameters
{
    public class ParticipantParameterProcessor : IParameterProcessor
    {
        public bool CanParse(string parameter)
        {
            return parameter.Equals("participant", StringComparison.OrdinalIgnoreCase);
        }
        
        public string Parse(string parameter, IJoyObject participant)
        {
            if (parameter.Equals("participant", StringComparison.OrdinalIgnoreCase) == false)
            {
                return "";
            }

            return participant.JoyName;
        }
    }
}