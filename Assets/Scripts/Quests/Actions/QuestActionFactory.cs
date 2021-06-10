using JoyLib.Code.Scripting;

namespace JoyLib.Code.Quests
{
    public static class QuestActionFactory
    {
        public static IQuestAction Create(string name)
        {
            IQuestAction action = ScriptingEngine.Instance.FetchAndInitialise(name) as IQuestAction;
            return action;
        }
    }
}