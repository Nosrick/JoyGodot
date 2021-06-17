using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Scripts.Conversation.Subengines.Rumours.Parameters
{
    public interface IParameterProcessor
    {
        bool CanParse(string parameter);
        
        string Parse(string parameter, IJoyObject participant);
    }
}