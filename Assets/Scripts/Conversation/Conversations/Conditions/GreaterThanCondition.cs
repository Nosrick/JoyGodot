namespace JoyGodot.Assets.Scripts.Conversation.Conversations.Conditions
{
    public class GreaterThanCondition : AbstractCondition
    {
        public GreaterThanCondition(string criteria, int value)
        : base(criteria, ">", value)
        {
        }

        public override bool FulfillsCondition(int value)
        {
            return value > this.Value;
        }
    }
}
