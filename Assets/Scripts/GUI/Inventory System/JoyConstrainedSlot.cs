﻿using Godot;

namespace JoyGodot.Assets.Scripts.GUI.Inventory_System
{
    public class JoyConstrainedSlot : JoyItemSlot
    {
        [Export]
        public string Slot
        {
            get => this.m_Slot;
            set
            {
                this.m_Slot = value;
                if (this.SlotLabel is null == false)
                {
                    this.SlotLabel.Text = this.m_Slot;
                }
            }
        }

        protected string m_Slot;
        
        public Label SlotLabel { get; protected set; }

        protected override void GetBits()
        {
            base.GetBits();
            this.SlotLabel = this.GetNode<Label>("Slot Name");
            this.SlotLabel.Text = this.Slot;
        }

        public override string ToString()
        {
            return this.Slot + ": " + base.ToString();
        }
    }
}