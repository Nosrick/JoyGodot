namespace JoyLib.Code.Conversation.Conversations
{
    public interface ITopicCondition
    {
        string Criteria
        {
            get;
        }

        int Value
        {
            get;
        }

        string Operand
        {
            get;
        }

        bool FulfillsCondition(int value);
    }
}
