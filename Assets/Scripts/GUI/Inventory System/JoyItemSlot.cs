using System;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Items;
using Array = Godot.Collections.Array;

namespace JoyLib.Code.Unity.GUI
{
    public class JoyItemSlot : Control, IManagedElement
    {
        protected TextureProgress m_CooldownOverlay;
        
        [Export]
        public string ElementName { get; protected set; }
        
        public bool Initialised { get; protected set; }
        
        [Export]
        public string m_Slot;

        [Export]
        protected bool m_UseRarityColor = false;
        
        protected ManagedUIElement m_Icon;
        
        protected Label m_Stack;
        
        public ItemContainer Container { get; set; }

        protected IItemInstance m_Item;

        public IItemInstance Item
        {
            get
            {
                return this.m_Item;
            }
            set
            {
                this.m_Item = value;
                this.Repaint();
            }
        }

        public bool IsEmpty => this.Item is null;
        
        protected static Array UnstackActions { get; set; }
        
        //public IConversationEngine ConversationEngine { get; set; }

        public IGUIManager GUIManager { get; set; }

        public ILiveEntityHandler EntityHandler { get; set; }

        protected IEntity Player { get; set; }
        
        protected static DragObject DragObject { get; set; }

        public void OnEnable()
        {
            this.GetBits();
            this.m_CooldownOverlay.Visible = false;

            this.m_Icon._Ready();
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
            this.m_Stack = (Label) this.FindNode("Stack");
            this.m_Icon = new ManagedUIElement
            {
                AnchorTop = 0.1f, 
                AnchorBottom = 0.9f, 
                AnchorRight = 0.9f, 
                AnchorLeft = 0.1f,
                MarginTop = 0,
                MarginBottom = 0,
                MarginLeft = 0,
                MarginRight = 0
            };
            this.m_Icon._Ready();
            this.Initialised = true;
            /*this.ConversationEngine = GlobalConstants.GameManager.ConversationEngine;
            this.GUIManager = GlobalConstants.GameManager.GUIManager;
            this.EntityHandler = GlobalConstants.GameManager.EntityHandler;*/
        }

        public virtual void Repaint()
        {
            if (this.m_Icon is null == false)
            {
                if (!this.IsEmpty)
                {
                    this.m_Icon.Clear();
                    this.m_Icon.AddSpriteState(this.Item.States[0]);
                    this.m_Icon.Visible = true;
                }
                else
                {
                    this.m_Icon.Clear();
                    this.m_Icon.Visible = false;
                }
            }
        }

        public override void DropData(Vector2 position, object data)
        {
            base.DropData(position, data);
        }

        public override object GetDragData(Vector2 position)
        {
            return base.GetDragData(position);
        }

        public override void _Input(InputEvent @event)
        {
            if (!(@event is InputEventAction action))
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
        
        public virtual void OnPointerDown(InputEventAction action)
        {
            if (action.Pressed == false)
            {
                return;
            }

            if (action.Action.Equals("begin drag", StringComparison.OrdinalIgnoreCase))
            {
                this.OnBeginDrag();
            }
        }

        public virtual void OnPointerUp(InputEventAction action)
        {
            if (action.Pressed)
            {
                return;
            }
            if (action.Action.Equals("begin drag", StringComparison.OrdinalIgnoreCase))
            {
                this.OnEndDrag();
            }
            else if (action.Action.Equals("open context menu", StringComparison.OrdinalIgnoreCase))
            {
                if (this.Container.UseContextMenu)
                { }
                else if(this.Container.MoveUsedItem)
                {
                    this.Container.MoveItem(this.Item);
                }
            }
        }
        /*

        //ContextMenu menu = this.GUIManager.Get(GUINames.CONTEXT_MENU).GetComponent<ContextMenu>();
        if (menu is null || this.Container.UseContextMenu == false)
        {
            return;
        }

        this.GetBits();

            if (this.Item is null == false)
            {
                menu.Clear();
                if (this.Item.HasTag("container"))
                {
                    menu.AddMenuItem("Open", this.OpenContainer);
                }

                List<IAbility> abilities = this.Item.AllAbilities.Where(ability => 
                        ability.Tags.Any(tag => tag.Equals("active", StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                if (this.Container.CanUseItems && abilities.Any())
                {
                    foreach (IAbility ability in abilities)
                    {
                        //menu.AddMenuItem(ability.Name, this.OnUse);
                    }
                    //menu.AddMenuItem("Use", this.OnUse);
                }

                this.GUIManager.OpenGUI(GUINames.CONTEXT_MENU);
                this.GUIManager.CloseGUI(GUINames.TOOLTIP);
                //menu.Show();
            }
        }
*/
        
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
                    DragObject = new DragObject
                    {
                        Item = this.Item,
                        SourceContainer = this.Container,
                        SourceSlot = this
                    };
                    Cursor cursor = (Cursor)this.GUIManager.OpenGUI(GUINames.CURSOR);
                    cursor.DragObject?.Clear();
                    cursor.DragObject?.AddSpriteState(this.Item.States[0]);
                }
            }
        }
        
        public virtual void OnEndDrag()
        {
            //GlobalConstants.ActionLog.AddText(eventData.pointerCurrentRaycast.gameObject.name);

            /*
            GameObject goResult = eventData.pointerCurrentRaycast.gameObject;

            if (goResult is null)
            {
                if (this.Container.CanDrag
                    && this.Container.CanDropItems)
                {
                    this.DropItem();
                }
            }
            else
            {
                JoyItemSlot resultSlot = goResult.GetComponentInParent<JoyItemSlot>();
                if (resultSlot is null == false)
                {
                    if (resultSlot.Container is null == false
                        && resultSlot.Container != this.Container
                        && this.Container.CanDrag
                        && resultSlot.Container.CanDrag)
                    {
                        this.Container.StackOrSwap(resultSlot.Container, this.Item);
                    }
                }
                else
                {
                    ItemContainer container = goResult.GetComponentInParent<ItemContainer>();
                    if (container is null == false)
                    {
                        if (container != this.Container
                            && this.Container.CanDrag
                            && container.CanDrag)
                        {
                            this.Container.StackOrSwap(container, this.Item);
                        }
                    }
                    else
                    {
                        if (this.Container.CanDropItems)
                        {
                            this.DropItem();
                        }
                    }
                }
            }
            */

            Cursor cursor = this.GUIManager.Get(GUINames.CURSOR) as Cursor;
            cursor?.DragObject?.Clear();
            this.EndDrag();
        }

        protected virtual void EndDrag()
        {
            this.Repaint();
        }
        
        public virtual void OnPointerEnter()
        {
            if (this.GUIManager.IsActive(GUINames.CONTEXT_MENU) == false)
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
                ((Tooltip) this.GUIManager.OpenGUI(GUINames.TOOLTIP))
                    .Show(
                        this.Item.DisplayName,
                        null,
                        this.Item.States[0],
                        this.Item.Tooltip);
            }
        }

        protected virtual void CloseTooltip() 
        {
            if (this.Container.ShowTooltips)
            {
                this.GUIManager.CloseGUI(GUINames.TOOLTIP);
            }
        }

        //TODO: Add item stacking
            /*
            if (this.m_Stack != null) 
            {
                if (!IsEmpty && ObservedItem.MaxStack > 1 )
                {
                    //Updates the stack and enables it.
                    this.m_Stack.text = ObservedItem.Stack.ToString();
                    this.m_Stack.enabled = true;
                }
                else
                {
                    //If there is no item in this slot, disable stack field
                    this.m_Stack.enabled = false;
                }
            }
        }

        protected virtual void OpenContainer()
        {
            if (this.Item.HasTag("container"))
            {
                this.GUIManager?.OpenGUI(GUINames.INVENTORY_CONTAINER);
                ItemContainer container = this.GUIManager?.Get(GUINames.INVENTORY_CONTAINER)
                    .GetComponent<ItemContainer>();
                container.Owner = this.Item;
                container.OnEnable();
            }
        }

        protected virtual void DropItem()
        {
            this.GetBits();

            //Check if the item is droppable
            if (this.Item is null == false && this.Container.CanDropItems)
            {
                if (this.Item.MonoBehaviourHandler is ItemBehaviourHandler itemBehaviourHandler)
                {
                    itemBehaviourHandler.DropItem();
                }

                this.Container.RemoveItem(this.Item);
                this.Item.InWorld = true;
                this.Item = null;
            }
        }

        public virtual void Unstack()
        {
            
        }

        public virtual void OnUse()
        {
            if (this.Item is null)
            {
                return;
            }

            if (this.Container.CanUseItems && this.Container.Owner is IEntity entity)
            {
                this.Item.Interact(entity);
                this.Container.OnEnable();
            }
            else if (this.Container.MoveUsedItem)
            {
                this.Container.MoveItem(this.Item);
            }
        }
        */
    }

    public struct DragObject
    {
        public IItemInstance Item { get; set; }
        public JoyItemSlot SourceSlot { get; set; }
        public ItemContainer SourceContainer { get; set; }
    }
}