using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code.Entities;
using JoyLib.Code.Events;
using JoyLib.Code.Quests;

namespace JoyLib.Code.Unity.GUI
{
    #if TOOLS
    [Tool]
    #endif
    public class QuestMenuItem : ManagedLabel
    {
        public IQuestTracker QuestTracker { get; set; }
        public IEntity Player { get; set; }
        
        public event EmptyEventHandler QuestAbandoned;

        public IQuest MyQuest
        {
            get => this.m_MyQuest;
            set
            {
                this.m_MyQuest = value;
                if (value is null == false)
                {
                    this.Text = this.m_MyQuest.ToString();
                }
            }
        }

        protected IQuest m_MyQuest;

        public override void _GuiInput(InputEvent @event)
        {
            base._GuiInput(@event);

            if (@event.IsActionPressed("open context menu")
                && this.MyQuest is null == false)
            {
                var contextMenu = GlobalConstants.GameManager.GUIManager.ContextMenu;
                
                contextMenu.Clear();
                contextMenu.AddItem("Abandon", delegate
                {
                    this.QuestTracker?.AbandonQuest(this.Player, this.MyQuest);
                    this.MyQuest = null;
                    this.QuestAbandoned?.Invoke();
                });
                GlobalConstants.GameManager.GUIManager.OpenGUI(this, GUINames.CONTEXT_MENU);
            }
        }
    }
}