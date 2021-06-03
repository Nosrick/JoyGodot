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
using JoyLib.Code.Events;
using JoyLib.Code.Unity.GUI;

namespace JoyGodot.Assets.Scripts.GUI.WorldState
{
    public class ConversationWindow : GUIData
    {
        protected ManagedUIElement ListenerIcon { get; set; }
        protected Label ListenerName { get; set; }
        protected Label LastSaid { get; set; }
        protected VBoxContainer ItemParent { get; set; }
        protected List<ConversationMenuItem> Items { get; set; }
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
            this.Items = new List<ConversationMenuItem>();

            this.MenuItemPrefab = GD.Load<PackedScene>(
                GlobalConstants.GODOT_ASSETS_FOLDER
                + "Scenes/Parts/ConversationMenuItem.tscn");

            this.ConversationEngine = GlobalConstants.GameManager.ConversationEngine;

            this.ConversationEngine.OnOpen -= this.ConversationDelegate;
            this.ConversationEngine.OnOpen += this.ConversationDelegate;
            /*
            this.ConversationEngine.OnConverse -= this.ConversationDelegate;
            this.ConversationEngine.OnConverse += this.ConversationDelegate;
            */
        }

        public override void Display()
        {
            base.Display();

            this.ConversationEngine.Converse();
        }

        public void SetActors(IEntity speaker, IEntity listener)
        {
            this.Speaker = speaker;
            this.Listener = listener;
            this.ListenerIcon.Clear();
            this.ListenerIcon.AddSpriteState(this.Listener.States.First());
            this.ListenerIcon.OverrideAllColours(this.Listener.States.First().SpriteData.GetCurrentPartColours());

            this.ConversationEngine.SetActors(this.Speaker, this.Listener);

            this.ListenerName.Text = this.ConversationEngine.ListenerInfo;
        }

        protected void ConversationDelegate(ITopic currentTopic, ICollection<ITopic> topics)
        {
            this.CreateMenuItems(currentTopic.Words, topics);
        }
        
        protected void CreateMenuItems(string lastSaid, ICollection<ITopic> topics)
        {
            this.LastSaid.Text = lastSaid;
            
            bool newItems = false;

            if (topics.Count > this.Items.Count)
            {
                for (int i = this.Items.Count; i < topics.Count; i++)
                {
                    ConversationMenuItem child = this.MenuItemPrefab.Instance() as ConversationMenuItem;
                    child.RectMinSize = new Vector2(0, 32);
                    child.HAlign = Label.AlignEnum.Center;
                    child.VAlign = Label.VAlign.Center;
                    child.AutoWrap = true;
                    
                    this.ItemParent.AddChild(child);
                    this.Items.Add(child);
                    newItems = true;
                }
            }

            foreach (ConversationMenuItem item in this.Items)
            {
                item.Hide();
            }

            for (int i = 0; i < topics.Count; i++)
            {
                var current = this.Items[i];
                ITopic currentTopic = topics.ElementAt(i);
                current.Text = currentTopic.Words;
                current.Name = currentTopic.ID;
                current.MyTopic = currentTopic;
                current.Show();
                current.OnClick -= this.OnItemClick;
                current.OnClick += this.OnItemClick;
            }

            if (newItems)
            {
                GlobalConstants.GameManager.GUIManager.SetupManagedComponents(this);
            }
        }

        protected void OnItemClick(ITopic topic)
        {
            var currentTopics = this.ConversationEngine.Converse(topic);

            if (currentTopics.Count == 0)
            {
                this.ButtonClose();
                return;
            }
            
            this.CreateMenuItems(this.ConversationEngine.LastSaidWords, currentTopics);
        }
    }
}