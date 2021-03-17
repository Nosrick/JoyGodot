namespace JoyLib.Code.Conversation.Conversations
{
    public class NotEqualToCondition : AbstractCondition
    {
        public NotEqualToCondition(string criteria, int value)
        : base(criteria, "!", value)
        {
        }
        
        public override bool FulfillsCondition(int value)
        {
            return this.Value != value;
        }
    }
}