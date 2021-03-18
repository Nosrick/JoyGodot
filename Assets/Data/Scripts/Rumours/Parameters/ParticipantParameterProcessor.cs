using System;

namespace JoyLib.Code.Conversation.Subengines.Rumours
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