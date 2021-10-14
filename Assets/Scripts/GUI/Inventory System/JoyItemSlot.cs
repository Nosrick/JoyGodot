using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Items;
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

        protected ItemStack m_ItemStack;

        public ItemStack ItemStack
        {
            get => this.m_ItemStack;
            set
            {
                if (value is null)
                {
                    this.m_ItemStack?.Clear();
                    this.StackLabel.Text = string.Empty;
                }
                else
                {
                    if (this.m_ItemStack is null == false)
                    {
                        this.m_ItemStack.ItemAdded -= this.UpdateStackLabelEvent;
                        this.m_ItemStack.ItemRemoved -= this.UpdateStackLabelEvent;
                    }
                    
                    this.m_ItemStack = value;
                    this.m_ItemStack.ItemAdded += this.UpdateStackLabelEvent;
                    this.m_ItemStack.ItemRemoved += this.UpdateStackLabelEvent;
                }
                this.UpdateStackLabel();
                this.Repaint();
            }
        }

        public bool IsEmpty => this.ItemStack.Contents.Any() == false;

        protected static Array UnstackActions { get; set; }

        public IGUIManager GuiManager { get; set; }

        public ILiveEntityHandler EntityHandler { get; set; }

        public bool MouseOver { get; protected set; }

        public ICollection<string> Tooltip
        {
            get => new List<string> { this.ItemStack?.ContentString };
            set { }
        }

        protected static DragObject DragData { get; set; }

        [Export] public string DragBeginSoundName { get; protected set; }

        protected AudioStream DragBeginAudioStream { get; set; }

        [Export] public string DragEndSoundName { get; protected set; }

        protected AudioStream DragEndAudioStream { get; set; }

        protected AudioStreamPlayer AudioPlayer { get; set; }
        
        [Export]
        public virtual string Slot
        {
            get => this.m_Slot;
            set
            {
                this.m_Slot = value;
            }
        }

        protected string m_Slot;

        public bool HasSlot => this.Slot.IsNullOrEmpty() == false;

        public override void _Ready()
        {
            base._Ready();

            this.GetBits();
            this.ItemStack = new ItemStack();
            this.CooldownOverlay.Visible = false;

            this.Icon._Ready();
        }

        public void Initialise()
        {
            this.Initialised = true;
        }

        protected virtual bool UpdateStackLabelEvent(IItemContainer sender, IItemInstance item)
        {
            this.UpdateStackLabel();
            return true;
        }

        protected virtual void UpdateStackLabel()
        {
            int contentCount = this.m_ItemStack.Contents.Count();
            this.StackLabel.Text = contentCount > 1 ? contentCount.ToString() : string.Empty;
        }

        protected virtual void GetBits()
        {
            if (this.Initialised)
            {
                return;
            }

            UnstackActions = InputMap.GetActionList("unstack");
            this.CooldownOverlay = this.FindNode("Cooldown Overlay") as TextureProgress;
            this.StackLabel = this.FindNode("Stack") as Label;
            this.Icon = this.FindNode("Icon") as ManagedUIElement;

            if (this.DragBeginSoundName.IsNullOrEmpty() == false)
            {
                this.DragBeginAudioStream = GlobalConstants.GameManager.AudioHandler.Get(this.DragBeginSoundName);
            }

            if (this.DragEndSoundName.IsNullOrEmpty() == false)
            {
                this.DragEndAudioStream = GlobalConstants.GameManager.AudioHandler.Get(this.DragEndSoundName);
            }

            this.AudioPlayer = this.FindNode("AudioPlayer") as AudioStreamPlayer;
            if (this.AudioPlayer is null)
            {
                var player = new AudioStreamPlayer();
                this.AudioPlayer = player;
                this.AddChild(player);
            }

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
                    this.Icon.AddSpriteState(this.ItemStack.DisplayState);
                    this.Icon.OverrideAllColours(this.ItemStack.DisplayState?.SpriteData.GetCurrentPartColours());
                    this.Icon.Visible = true;
                }
                else
                {
                    this.Icon.Clear();
                    this.Icon.Visible = false;
                }
            }
        }

        public virtual bool CanAddItem(IItemInstance item)
        {
            return this.HasSlot 
                ? item.ItemType.Slots.Any(s => s.Equals(this.Slot, StringComparison.OrdinalIgnoreCase)) 
                : this.ItemStack.CanAddContents(item);
        }

        public override bool CanDropData(Vector2 position, object data)
        {
            if (data is DragObject dragObject)
            {
                if (dragObject.SourceContainer == this.Container)
                {
                    return true;
                }
                
                var myContainer = this.Container.ContainerOwner;
                var otherContainer = dragObject.SourceContainer.ContainerOwner;
                var myItemStack = this.ItemStack;
                var myContents = new List<IItemInstance>(myItemStack.Contents);
                var otherItemStack = dragObject.ItemStack;
                var otherContents = new List<IItemInstance>(otherItemStack.Contents);
                
                if(myContainer.CanRemoveContents(myContents)
                    && myContainer.CanAddContents(otherContents)
                    && otherContainer.CanRemoveContents(otherContents)
                    && otherContainer.CanAddContents(myContents))
                {
                    return true;
                }
            }
            
            return false;
        }

        public override void DropData(Vector2 position, object data)
        {
            if (!(data is DragObject dragObject))
            {
                return;
            }

            var cursor = this.GuiManager.Cursor;
            cursor.DragSprite = null;
                
            var myContainer = this.Container.ContainerOwner;
            var otherContainer = dragObject.SourceContainer.ContainerOwner;
            var myItemStack = this.ItemStack;
            var myContents = new List<IItemInstance>(myItemStack.Contents);
            var otherItemStack = dragObject.ItemStack;
            var otherContents = new List<IItemInstance>(otherItemStack.Contents);
            
            if (this.Container.StackOrSwap(
                new List<JoyItemSlot> { dragObject.SourceSlot },
                new List<JoyItemSlot> { this },
                dragObject.SourceContainer,
                this.Container))
            {
                myContainer.RemoveContents(myContents);
                myContainer.AddContents(otherContents);
                otherContainer.RemoveContents(otherContents);
                otherContainer.AddContents(myContents);
                this.PlayEndDragSound();
            }
        }

        public override object GetDragData(Vector2 position)
        {
            if (this.ItemStack is null)
            {
                return null;
            }

            var cursor = this.GuiManager.Cursor;
            cursor.DragSprite = this.ItemStack.DisplayState;

            DragData = new DragObject
            {
                ItemStack = this.ItemStack,
                SourceContainer = this.Container,
                SourceSlot = this
            };

            this.PlayBeginDragSound();

            return DragData;
        }

        public override void _GuiInput(InputEvent @event)
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
        { }

        protected void PlayBeginDragSound()
        {
            if (this.AudioPlayer.Playing)
            {
                return;
            }

            this.AudioPlayer.Stream = this.DragBeginAudioStream;
            this.AudioPlayer.Play();
        }

        protected void PlayEndDragSound()
        {
            if (this.AudioPlayer.Playing)
            {
                return;
            }

            this.AudioPlayer.Stream = this.DragEndAudioStream;
            this.AudioPlayer.Play();
        }

        public virtual void OnPointerUp(InputEventMouseButton action)
        {
            if (action.Pressed)
            {
                return;
            }

            if (action.ButtonIndex == (int)ButtonList.Left)
            {
                this.OnEndDrag();
            }
            else if (
                this.GetGlobalRect().HasPoint(action.GlobalPosition)
                && action.ButtonIndex == (int)ButtonList.Right
                && this.ItemStack.Empty == false)
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
                        foreach (var ability in this.ItemStack.Contents.SelectMany(instance => instance.AllAbilities))
                        {
                            if (ability.HasTag("active"))
                            {
                                contextMenu.AddItem(ability.Name, () => { this.UseItem(ability.Name); });
                            }
                        }
                    }

                    if (this.ItemStack.Contents.FirstOrDefault()?.HasTag("container") == true)
                    {
                        contextMenu.AddItem("Open Container", this.OpenContainer);
                    }

                    if (this.ItemStack.Contents.Count() > 1)
                    {
                        contextMenu.AddItem("Open Stack", this.OpenItemStack);
                    }

                    this.GuiManager.OpenGUI(this, GUINames.CONTEXT_MENU);
                }
                else if (this.Container.MoveUsedItem)
                {
                    this.Container.MoveStack(this.ItemStack);
                }
            }
        }

        public virtual void OnBeginDrag()
        {
            //Check if we can start dragging
            if (this.ItemStack.Empty == false && this.Container.CanDrag)
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
                    cursor.DragSprite = this.ItemStack.DisplayState;
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
            if (this.Container.ShowTooltips && this.ItemStack.Empty == false)
            {
                this.GuiManager.Tooltip
                    .Show(
                        this,
                        this.ItemStack.JoyName,
                        this.ItemStack.DisplayState,
                        this.ItemStack.Contents.Count() > 1 
                            ? new List<string> { this.ItemStack.ContentString }
                            : this.ItemStack.Contents.First().Tooltip);
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
            if (this.ItemStack.Empty || !this.Container.CanDropItems)
            {
                return;
            }

            foreach (IItemInstance item in this.ItemStack.Contents)
            {
                GlobalConstants.GameManager.Player.MyWorld.AddItem(item);
            }
            this.Container.ContainerOwner.RemoveContents(this.ItemStack.Contents);
        }

        protected virtual void OpenContainer()
        {
            if (!(this.GuiManager?.OpenGUI(this, GUINames.INVENTORY_CONTAINER) is ItemContainer container))
            {
                return;
            }

            container.ContainerOwner = this.ItemStack.Contents.First();
            container.OnEnable();
        }

        protected virtual void OpenItemStack()
        {
            if (!(this.GuiManager?.OpenGUI(this, GUINames.INVENTORY_CONTAINER) is ItemContainer container))
            {
                return;
            }

            container.ContainerOwner = this.ItemStack;
            container.OnEnable();
        }

        protected virtual void UseItem(string abilityName)
        {
            this.ItemStack.Contents.FirstOrDefault()?.Interact(GlobalConstants.GameManager.Player, abilityName);
        }

        public override string ToString()
        {
            return this.ItemStack.ContentString;
        }
    }
}