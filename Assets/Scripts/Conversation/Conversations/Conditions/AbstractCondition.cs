namespace JoyGodot.Assets.Scripts.Conversation.Conversations.Conditions
{
    public abstract class AbstractCondition : ITopicCondition
    {
        public string Criteria { get; protected set; }
        public int Value { get; protected set; }
        public string Operand { get; protected set; }

        public AbstractCondition()
        {
            this.Criteria = "NONE";
            this.Operand = "NONE";
            this.Value = int.MinValue;
        }

        public AbstractCondition(string criteria, string operand, int value)
        {
            this.Criteria = criteria;
            this.Operand = operand;
            this.Value = value;
        }

        public abstract bool FulfillsCondition(int value);
    }
}