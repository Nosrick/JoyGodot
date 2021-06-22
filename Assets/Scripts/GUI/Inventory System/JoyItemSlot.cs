using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Items;
using JoyGodot.Assets.Scripts.Managed_Assets;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.GUI.Inventory_System
{
    public class JoyItemSlot : 
        Control, 
        IManagedElement,
        ITooltipComponent
    {
        protected TextureProgress CooldownOverlay { get; set; }

        [Export] public string ElementName { get; protected set; }

        public bool Initialised { get; protected set; }

        [Export] protected bool UseRarityColor { get; set; }

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

        public bool MouseOver { get; protected set; }

        public ICollection<string> Tooltip
        {
            get => this.Item?.Tooltip;
            set
            {
            }
        }

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
            this.Initialised = true;
        }

        protected virtual void GetBits()
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
            this.CooldownOverlay = this.FindNode("Cooldown Overlay") as TextureProgress;
            this.StackLabel = this.FindNode("Stack") as Label;
            this.Icon = this.FindNode("Icon") as ManagedUIElement;

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

            if(this.Container.CanAddItem(dragObject.Item, dragObject.SourceContainer.Name) 
               && this.Container.StackOrAdd(dragObject.Item))
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
                    
                    this.GuiManager.OpenGUI(this, GUINames.CONTEXT_MENU);
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
                //DragData = null;
                //this.DropItem();
            }
        }

        public virtual void OnPointerEnter()
        {
            this.MouseOver = true;
            
            if (this.GuiManager.IsActive(GUINames.CONTEXT_MENU) == false)
            {
                this.ShowTooltip();
            }
        }

        public virtual void OnPointerExit()
        {
            this.MouseOver = false;
            
            this.CloseTooltip();
        }

        protected virtual void ShowTooltip()
        {
            if (this.Container.ShowTooltips && this.Item is null == false)
            {
                this.GuiManager.Tooltip
                    .Show(
                        this, 
                        this.Item.DisplayName,
                        this.Item.States.FirstOrDefault(),
                        this.Item.Tooltip);
            }
        }

        protected virtual void CloseTooltip()
        {
            if (this.Container.ShowTooltips)
            {
                this.GuiManager.CloseGUI(this, GUINames.TOOLTIP);
            }
        }
        
        protected virtual void DropItem()
        {
            //Check if the item is droppable
            if (this.Item is null || !this.Container.CanDropItems)
            {
                return;
            }
            
            GlobalConstants.GameManager.Player.MyWorld.AddItem(this.Item);
            this.Item.MyNode.Visible = true;
            this.Container.RemoveItem(this.Item);
            this.Item = null;
        }
        
        protected virtual void OpenContainer()
        {
            if (!(this.GuiManager?.OpenGUI(this, GUINames.INVENTORY_CONTAINER) is ItemContainer container))
            {
                return;
            }
            
            container.ContainerOwner = this.Item;
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