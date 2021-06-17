namespace JoyGodot.Assets.Scripts.GUI.WorldState
{
    public class Quickbar : GUIData
    {
        public void OpenInventory()
        {
            this.GUIManager.ToggleGUI(this, GUINames.INVENTORY);
        }

        public void OpenEquipment()
        {
            this.GUIManager.ToggleGUI(this, GUINames.EQUIPMENT);
        }

        public void OpenQuestJournal()
        {
            this.GUIManager.ToggleGUI(this, GUINames.QUEST_JOURNAL);
        }

        public void OpenCharacterSheet()
        {
            this.GUIManager.ToggleGUI(this, GUINames.CHARACTER_SHEET);
        }

        public void OpenJobManagement()
        {
            this.GUIManager.ToggleGUI(this, GUINames.JOB_MANAGEMENT);
        }
    }
}