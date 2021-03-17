namespace JoyLib.Code.Conversation.Subengines.Rumours
{
    public interface IParameterProcessor
    {
        bool CanParse(string parameter);
        
        string Parse(string parameter, IJoyObject participant);
    }
}