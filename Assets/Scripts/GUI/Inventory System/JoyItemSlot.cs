using System;
using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Conversation;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Abilities;
using JoyLib.Code.Entities.Items;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace JoyLib.Code.Unity.GUI
{
    public class JoyItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerDownHandler,
        IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        protected Image m_CooldownOverlay;
        
        [SerializeField]
        protected TextMeshProUGUI m_Cooldown;

        [SerializeField]
        protected bool m_UseRarityColor = false;
        
        [SerializeField]
        protected ManagedUISprite m_Icon;
        
        [SerializeField]
        protected TextMeshProUGUI m_Stack;

        public string m_Slot;
        
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
        
        protected static InputAction UnstackKey { get; set; }
        
        public IConversationEngine ConversationEngine { get; set; }

        public IGUIManager GUIManager { get; set; }

        public ILiveEntityHandler EntityHandler { get; set; }

        protected IEntity Player { get; set; }
        
        protected static DragObject DragObject { get; set; }
        
        protected static GraphicRaycaster Raycaster { get; set; }

        public void OnEnable()
        {
            this.GetBits();
            this.m_CooldownOverlay.gameObject.SetActive(false);

            if (Raycaster is null)
            {
                Raycaster = GameObject.Find("MainUI").GetComponent<GraphicRaycaster>();
            }
            this.m_Icon.Awake();
        }

        protected void GetBits()
        {
            if (GlobalConstants.GameManager is null || this.GUIManager is null == false)
            {
                return;
            }
            UnstackKey = InputSystem.ListEnabledActions().First(action => action.name.Equals("unstack", StringComparison.OrdinalIgnoreCase));
            this.ConversationEngine = GlobalConstants.GameManager.ConversationEngine;
            this.GUIManager = GlobalConstants.GameManager.GUIManager;
            this.EntityHandler = GlobalConstants.GameManager.EntityHandler;
        }
        
        public virtual void Repaint()
        {
            if (this.m_Icon is null == false)
            {
                if (!this.IsEmpty)
                {
                    this.m_Icon.Clear();
                    this.m_Icon.AddSpriteState(this.Item.MonoBehaviourHandler.CurrentSpriteState, true);
                    this.m_Icon.gameObject.SetActive(true);
                }
                else 
                {
                    this.m_Icon.Clear();
                    this.m_Icon.gameObject.SetActive(false);
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
            */
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.dragging || eventData.button != PointerEventData.InputButton.Right)
            {
                return;
            }

            ContextMenu menu = this.GUIManager.Get(GUINames.CONTEXT_MENU).GetComponent<ContextMenu>();
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
                        menu.AddMenuItem(ability.Name, this.OnUse);
                    }
                    //menu.AddMenuItem("Use", this.OnUse);
                }

                this.GUIManager.OpenGUI(GUINames.CONTEXT_MENU);
                this.GUIManager.CloseGUI(GUINames.TOOLTIP);
                menu.Show();
            }
        }
        
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (this.GUIManager.IsActive(GUINames.CONTEXT_MENU) == false)
            {
                this.ShowTooltip();
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            this.CloseTooltip();
        }
        
        protected virtual void ShowTooltip() 
        {
            if (this.Container.ShowTooltips && this.Item is null == false)
            {
                this.GUIManager.OpenGUI(GUINames.TOOLTIP)
                    .GetComponent<Tooltip>()
                    .Show(
                        this.Item.DisplayName,
                        null,
                        this.Item.MonoBehaviourHandler.CurrentSpriteState,
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

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            //Check if we can start dragging
            if (this.Item is null == false && this.Container.CanDrag)
            {
                if (UnstackKey.triggered)
                {
                    this.Unstack();
                }
                else
                {
                    this.GUIManager.OpenGUI(GUINames.CURSOR)
                        .GetComponent<Cursor>()
                        .Show(this.Item.MonoBehaviourHandler.CurrentSpriteState);
                    DragObject = new DragObject
                    {
                        Item = this.Item,
                        SourceContainer = this.Container,
                        SourceSlot = this
                    };
                }
            }
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            //GlobalConstants.ActionLog.AddText(eventData.pointerCurrentRaycast.gameObject.name);

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

            this.EndDrag();
        }

        public virtual void OnDrop(PointerEventData eventData)
        {
        }

        protected virtual void EndDrag()
        {
            this.Repaint();
            this.GUIManager.Get(GUINames.CURSOR).GetComponent<Cursor>().Reset();
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right)
            {
                return;
            }
            
            this.Container.MoveItem(this.Item);
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
    }

    public struct DragObject
    {
        public IItemInstance Item { get; set; }
        public JoyItemSlot SourceSlot { get; set; }
        public ItemContainer SourceContainer { get; set; }
    }
}