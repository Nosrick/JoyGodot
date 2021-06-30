namespace JoyGodot.Assets.Scripts.Conversation.Subengines.Rumours.Parameters
{
    public interface IParameterProcessorHandler
    {
        IParameterProcessor Get(string parameter);
    }
}