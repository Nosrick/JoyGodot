﻿namespace JoyLib.Code.Conversation.Conversations
{
    public class LessThanCondition : AbstractCondition
    {
        public LessThanCondition(string criteria, int condition)
        : base(criteria, "<", condition)
        {
        }

        public override bool FulfillsCondition(int value)
        {
            return value < this.Value;
        }
    }
}
