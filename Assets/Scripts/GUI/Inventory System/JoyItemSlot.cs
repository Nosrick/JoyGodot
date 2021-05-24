using System;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Items;
using Array = Godot.Collections.Array;

namespace JoyLib.Code.Unity.GUI
{
    public class JoyItemSlot : Control, IManagedElement
    {
        protected TextureProgress CooldownOverlay { get; set; }

        [Export] public string ElementName { get; protected set; }

        public bool Initialised { get; protected set; }

        [Export] public string m_Slot;

        [Export] protected bool m_UseRarityColor = false;

        protected ManagedUIElement Icon { get; set; }

        protected Label StackLabel { get; set; }

        public ItemContainer Container { get; set; }

        protected IItemInstance m_Item;

        public IItemInstance Item
        {
            get { return this.m_Item; }
            set
            {
                this.m_Item = value;
                this.Repaint();
            }
        }

        public bool IsEmpty => this.Item is null;

        protected static Array UnstackActions { get; set; }

        //public IConversationEngine ConversationEngine { get; set; }

        public IGUIManager GuiManager { get; set; }

        public ILiveEntityHandler EntityHandler { get; set; }
        
        protected static DragObject DragData { get; set; }

        public override void _Ready()
        {
            base._Ready();
            this.GetBits();
            this.CooldownOverlay.Visible = false;

            this.Icon._Ready();
        }

        public void Initialise()
        {
            GD.Print(this.GetType().Name + " initialised!");
            this.Initialised = true;
        }

        protected void GetBits()
        {
            if (this.Initialised)
            {
                return;
            }

            /*
            if (GlobalConstants.GameManager is null || this.GUIManager is null == false)
            {
                return;
            }
            */
            UnstackActions = InputMap.GetActionList("unstack");
            this.CooldownOverlay = this.GetNode<TextureProgress>("Cooldown Overlay");
            this.StackLabel = this.GetNode<Label>("Stack");
            this.Icon = this.GetNode<ManagedUIElement>("Icon");

            this.EntityHandler = GlobalConstants.GameManager.EntityHandler;
            this.GuiManager = GlobalConstants.GameManager.GUIManager;
            this.Initialised = true;
        }

        public virtual void Repaint()
        {
            if (this.Icon is null == false)
            {
                if (!this.IsEmpty)
                {
                    this.Icon.Clear();
                    this.Icon.AddSpriteState(this.Item.States.FirstOrDefault());
                    this.Icon.OverrideAllColours(this.Item.States.FirstOrDefault()?.SpriteData.GetCurrentPartColours());
                    this.Icon.Visible = true;
                }
                else
                {
                    this.Icon.Clear();
                    this.Icon.Visible = false;
                }
            }
        }

        public override bool CanDropData(Vector2 position, object data)
        {
            return data is DragObject;
        }

        public override void DropData(Vector2 position, object data)
        {
            if (!(data is DragObject dragObject))
            {
                return;
            }
            var cursor = this.GuiManager.Cursor;
            cursor.DragSprite = null;

            if (this.Container.StackOrAdd(dragObject.Item))
            {
                dragObject.SourceContainer.RemoveItem(dragObject.Item);
            }
        }

        public override object GetDragData(Vector2 position)
        {
            var cursor = this.GuiManager.Cursor;
            cursor.DragSprite = this.Item.States.FirstOrDefault();

            DragData = new DragObject
            {
                Item = this.Item,
                SourceContainer = this.Container,
                SourceSlot = this
            };

            return DragData;
        }

        public override void _Input(InputEvent @event)
        {
            if (!(@event is InputEventMouseButton action))
            {
                return;
            }

            if (action.Pressed)
            {
                this.OnPointerDown(action);
            }
            else
            {
                this.OnPointerUp(action);
            }
        }

        public virtual void OnPointerDown(InputEventMouseButton action)
        {
        }

        public virtual void OnPointerUp(InputEventMouseButton action)
        {
            if (action.Pressed)
            {
                return;
            }

            if (action.ButtonIndex == (int) ButtonList.Left)
            {
                this.OnEndDrag();
            }
            else if (
                this.GetGlobalRect().HasPoint(action.GlobalPosition)
                && action.ButtonIndex == (int) ButtonList.Right
                && this.Item is null == false)
            {
                if (this.Container.UseContextMenu)
                {
                    var contextMenu = this.GuiManager.ContextMenu;
                    contextMenu.Clear();
                    if (this.Container.CanDropItems)
                    {
                        contextMenu.AddItem("Drop", this.DropItem);
                    }

                    if (this.Container.CanUseItems)
                    {
                        foreach (var ability in this.Item.AllAbilities)
                        {
                            if (ability.HasTag("active"))
                            {
                                contextMenu.AddItem(ability.Name, () =>
                                {
                                    this.UseItem(ability.Name);
                                });
                            }
                        }
                    }

                    if (this.Item.HasTag("container"))
                    {
                        contextMenu.AddItem("Open", this.OpenContainer);
                    }
                    
                    this.GuiManager.OpenGUI(GUINames.CONTEXT_MENU);
                }
                else if (this.Container.MoveUsedItem)
                {
                    this.Container.MoveItem(this.Item);
                }
            }
        }

        public virtual void OnBeginDrag()
        {
            //Check if we can start dragging
            if (this.Item is null == false && this.Container.CanDrag)
            {
                /*
                if (UnstackKey.triggered)
                {
                    this.Unstack();
                }
                else
                */
                {
                    var cursor = this.GuiManager.Cursor;
                    cursor.DragSprite = this.Item.States.FirstOrDefault();
                }
            }
        }

        public virtual void OnEndDrag()
        {
            if (DragData is null
                || DragData.SourceSlot != this)
            {
                return;
            }
            
            var cursor = this.GuiManager.Cursor;
            cursor.DragSprite = null;

            if (this.Container.GetGlobalRect().HasPoint(this.GetGlobalMousePosition()) == false)
            {
                DragData = null;
                this.DropItem();
            }
        }

        public virtual void OnPointerEnter()
        {
            if (this.GuiManager.IsActive(GUINames.CONTEXT_MENU) == false)
            {
                this.ShowTooltip();
            }
        }

        public virtual void OnPointerExit()
        {
            this.CloseTooltip();
        }

        protected virtual void ShowTooltip()
        {
            if (this.Container.ShowTooltips && this.Item is null == false)
            {
                this.GuiManager.Tooltip
                    .Show(
                        this.Item.DisplayName,
                        this.Item.States.FirstOrDefault(),
                        this.Item.Tooltip);
            }
        }

        protected virtual void CloseTooltip()
        {
            if (this.Container.ShowTooltips)
            {
                this.GuiManager.CloseGUI(GUINames.TOOLTIP);
                GlobalConstants.GameManager.GUIManager.Tooltip.Close();
            }
        }
        
        protected virtual void DropItem()
        {
            //Check if the item is droppable
            if (this.Item is null || !this.Container.CanDropItems)
            {
                return;
            }
            
            GlobalConstants.GameManager.Player.MyWorld.AddObject(this.Item);
            this.Item.MyNode.Visible = true;
            this.Container.RemoveItem(this.Item);
            this.Item = null;
        }
        
        protected virtual void OpenContainer()
        {
            if (!(this.GuiManager?.OpenGUI(GUINames.INVENTORY_CONTAINER) is ItemContainer container))
            {
                return;
            }
            
            container.JoyObjectOwner = this.Item;
            container.OnEnable();
        }

        protected virtual void UseItem(string abilityName)
        {
            this.Item?.Interact(GlobalConstants.GameManager.Player, abilityName);
        }

        protected class DragObject : Resource
        {
            public IItemInstance Item { get; set; }
            public JoyItemSlot SourceSlot { get; set; }
            public ItemContainer SourceContainer { get; set; }
        }
    }
}