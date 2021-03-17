namespace JoyLib.Code.Conversation.Subengines.Rumours
{
    public interface IParameterProcessorHandler
    {
        IParameterProcessor Get(string parameter);
    }
}