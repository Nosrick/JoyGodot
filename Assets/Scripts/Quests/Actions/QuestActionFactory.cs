using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Scripts.Quests.Actions
{
    public static class QuestActionFactory
    {
        public static IQuestAction Create(string name)
        {
            IQuestAction action = GlobalConstants.ScriptingEngine.FetchAndInitialise(name) as IQuestAction;
            return action;
        }
    }
}