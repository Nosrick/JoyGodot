namespace JoyLib.Code.Conversation.Conversations
{
    public class EqualToCondition : AbstractCondition
    {
        public EqualToCondition(string criteria, int value)
        : base(criteria, "=", value)
        {
        }

        public override bool FulfillsCondition(int value)
        {
            return value == this.Value;
        }
    }
}
