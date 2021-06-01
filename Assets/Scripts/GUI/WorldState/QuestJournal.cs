using Godot;
using JoyLib.Code.Entities;
using JoyLib.Code.Quests;

namespace JoyLib.Code.Unity.GUI.WorldState
{
    public class QuestJournal : GUIData
    {
        protected VBoxContainer ItemList { get; set; }
        protected PackedScene ItemPrefab { get; set; }
        protected IQuestTracker QuestTracker { get; set; }
        protected IEntity Player { get; set; }

        public override void _Ready()
        {
            base._Ready();
            
            this.ItemList = this.FindNode("Quest List") as VBoxContainer;
            this.ItemPrefab = GD.Load<PackedScene>(
                GlobalConstants.GODOT_ASSETS_FOLDER
                + "Scenes/Parts/QuestMenuItem.tscn");
            this.QuestTracker = GlobalConstants.GameManager.QuestTracker;
            this.Player = GlobalConstants.GameManager.Player;
        }

        public override void Display()
        {
            this.SetUp();
            base.Display();
        }

        protected void SetUp()
        {
            var quests = this.QuestTracker.GetQuestsForEntity(this.Player.Guid);

            foreach (var item in this.ItemList.GetChildren())
            {
                if (item is QuestMenuItem menuItem)
                {
                    menuItem.Hide();
                }
            }

            bool addedChildren = false;
            if (this.ItemList.GetChildCount() < quests.Count)
            {
                for (int i = this.ItemList.GetChildCount(); i < quests.Count; i++)
                {
                    var instance = this.ItemPrefab.Instance() as QuestMenuItem;
                    instance.Hide();
                    instance.Player = this.Player;
                    instance.QuestTracker = this.QuestTracker;
                    instance.QuestAbandoned += this.SetUp;
                    this.ItemList.AddChild(instance);
                }

                addedChildren = true;
            }

            for (int i = 0; i < quests.Count; i++)
            {
                var item = this.ItemList.GetChild<QuestMenuItem>(i);
                item.MyQuest = quests[i];
                item.Show();
            }

            if (addedChildren)
            {
                GlobalConstants.GameManager.GUIManager.SetupManagedComponents(this);
            }
        }
    }
}