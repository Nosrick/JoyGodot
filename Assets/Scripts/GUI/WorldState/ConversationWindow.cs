using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.addons.Managed_Assets;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code;
using JoyLib.Code.Conversation;
using JoyLib.Code.Conversation.Conversations;
using JoyLib.Code.Entities;
using JoyLib.Code.Unity.GUI;

namespace JoyGodot.Assets.Scripts.GUI.WorldState
{
    public class ConversationWindow : GUIData
    {
        protected ManagedUIElement ListenerIcon { get; set; }
        protected Label ListenerName { get; set; }
        protected Label LastSaid { get; set; }
        protected VBoxContainer ItemParent { get; set; }
        protected List<ManagedTextButton> Items { get; set; }
        protected PackedScene MenuItemPrefab { get; set; }
        public IEntity Speaker { get; set; }
        public IEntity Listener { get; set; }
        protected IConversationEngine ConversationEngine { get; set; }

        public override void _Ready()
        {
            base._Ready();

            this.ListenerIcon = this.FindNode("ListenerIcon") as ManagedUIElement;
            this.ListenerName = this.FindNode("ListenerName") as Label;
            this.LastSaid = this.FindNode("LastSaid") as Label;
            this.ItemParent = this.FindNode("ConversationItems") as VBoxContainer;
            this.Items = new List<ManagedTextButton>();

            this.MenuItemPrefab = GD.Load<PackedScene>(
                GlobalConstants.GODOT_ASSETS_FOLDER
                + "Scenes/Parts/ManagedTextButton.tscn");

            this.ConversationEngine = GlobalConstants.GameManager.ConversationEngine;
        }

        public void SetActors(IEntity speaker, IEntity listener)
        {
            this.Speaker = speaker;
            this.Listener = listener;

            this.ListenerName.Text = this.Listener.JoyName;
            this.ListenerIcon.Clear();
            this.ListenerIcon.AddSpriteState(this.Listener.States.First());
            this.ListenerIcon.OverrideAllColours(this.Listener.States.First().SpriteData.GetCurrentPartColours());
        }
        
        protected void CreateMenuItems()
        {
            bool newItems = false;

            if (this.ConversationEngine.CurrentTopics.Length > this.Items.Count)
            {
                for (int i = this.Items.Count; i < this.ConversationEngine.CurrentTopics.Length; i++)
                {
                    ManagedTextButton child = this.MenuItemPrefab.Instance() as ManagedTextButton;
                    this.ItemParent.AddChild(child);
                    this.Items.Add(child);
                    newItems = true;
                }
            }

            foreach (ManagedTextButton item in this.Items)
            {
                item.Hide();
            }

            for (int i = 0; i < this.ConversationEngine.CurrentTopics.Length; i++)
            {
                var current = this.Items[i];
                ITopic currentTopic = this.ConversationEngine.CurrentTopics[i];
                current.Text = currentTopic.Words;
                current.Name = currentTopic.ID;
                current.Show();
                if (current.IsConnected("_Press", this, nameof(this.OnItemClick)))
                {
                    current.Disconnect("_Press", this, nameof(this.OnItemClick));
                }

                current.Connect(
                    "_Press",
                    this,
                    nameof(this.OnItemClick),
                    new Array
                    {
                        currentTopic.ID,
                        i
                    });
            }

            if (newItems)
            {
                GlobalConstants.GameManager.GUIManager.SetupManagedComponents(this);
            }
        }

        protected void OnItemClick(string topic, int index)
        {
            this.ConversationEngine.Converse(topic, index);
        }
    }
}