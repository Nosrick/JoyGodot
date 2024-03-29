﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyGodot.Assets.Scripts.GUI.Tools
{
    #if TOOLS
    [Tool]
    #endif
    public class StringValueItem : 
        Control, 
        IValueItem<string>,
        ILabelElement
    {
        protected ManagedLabel NameLabel { get; set; }
        protected ManagedLabel ValueLabel { get; set; }

        public ICollection<string> Tooltip
        {
            get => this.m_Tooltip;
            set
            {
                this.m_Tooltip = value;
                foreach (Node child in this.GetChildren())
                {
                    if (child is ITooltipHolder tooltipHolder)
                    {
                        tooltipHolder.Tooltip = value;
                    }
                }
            }
        }

        protected ICollection<string> m_Tooltip;

        public ICollection<string> Values
        {
            get => this.m_Values;
            set
            {
                this.m_Values = value;
                this.Minimum = 0;
                this.Maximum = this.m_Values.Count;
                this.Value = this.m_Values.FirstOrDefault();
            }
        }

        protected ICollection<string> m_Values;

        public bool MouseOver { get; protected set; }

        public int Minimum { get; set; }
        public int Maximum { get; set; }

        public string Selected
        {
            get
            {
                if (this.Index >= 0 && this.Index < this.Values.Count)
                {
                    return this.Values.ElementAt(this.Index);
                }

                return "INDEXING ERROR";
            }
        }

        public int Index
        {
            get => this.m_Index;
            set
            {
                this.m_Index = value;
                this.Value = this.Selected;
            }
        }

        protected int m_Index;

        [Export]
        public bool TitleCase
        {
            get => this.m_TitleCase;
            set
            {
                if (value)
                {
                    if (this.ValueName is null == false)
                    {
                        this.ValueName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.ValueName);
                    }

                    if (this.Value is null == false)
                    {
                        this.Value = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.Value);
                    }
                }

                this.m_TitleCase = value;
            }
        }

        protected bool m_TitleCase;

        [Signal]
        public delegate void ValueChanged(string name, int delta, string newValue);

        public string ValueName
        {
            get => this.NameLabel?.Text;
            set
            {
                if (value is null)
                {
                    this.NameLabel.Text = null;
                    return;
                }
                
                this.Name = value;
                if (this.NameLabel is null)
                {
                    //GD.PushWarning(this.GetType().Name + " NameLabel is null!");

                    return;
                }
                
                this.NameLabel.Text = this.TitleCase 
                    ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value)
                    : value;
            }
        }

        public string Value
        {
            get => this.ValueLabel?.Text;
            set
            {
                if (this.ValueLabel is null)
                {
                    //GD.PushWarning(this.GetType().Name + " ValueLabel is null!");
                    return;
                }

                if (value is null)
                {
                    this.ValueLabel.Text = null;
                }
                else
                {
                    this.ValueLabel.Text = this.TitleCase 
                        ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value)
                        : value;
                }
            }
        }

        public override void _Ready()
        {
            base._Ready();
            this.Values = new List<string>();
            this.ValueLabel = this.GetNodeOrNull("Item Value") as ManagedLabel;
            if (this.ValueLabel is null)
            {
                GD.PushError("ValueLabel of " + this.GetType().Name + " is null!");
            }
            
            foreach (var child in this.GetChildren())
            {
                if (!(child is Control control))
                {
                    continue;
                }
                
                if (control.IsConnected(
                    "mouse_entered",
                    this,
                    nameof(this.OnPointerEnter)))
                {
                    control.Disconnect(
                        "mouse_entered",
                        this,
                        nameof(this.OnPointerEnter));
                }

                control.Connect(
                    "mouse_entered",
                    this,
                    nameof(this.OnPointerEnter));

                if (control.IsConnected(
                    "mouse_exited",
                    this,
                    nameof(this.OnPointerExit)))
                {
                    control.Disconnect(
                        "mouse_exited",
                        this,
                        nameof(this.OnPointerExit));
                }

                control.Connect(
                    "mouse_exited",
                    this,
                    nameof(this.OnPointerExit));
            }

            this.NameLabel = this.GetNodeOrNull("Item Name") as ManagedLabel;
            if (this.NameLabel is null)
            {
                GD.PushError("NameLabel of " + this.GetType().Name + " is null!");
            }
        }

        public void ChangeValue(int delta = 1)
        {
            this.Index += delta;
            if (this.Index < 0)
            {
                this.Index = this.Values.Count - 1;
            }
            else
            {
                this.Index %= this.Values.Count > 0 ? this.Values.Count : 1;
            }
            
            if (this.Index < this.Values.Count)
            {
                this.Value = this.Values.ElementAt(this.Index);
            }
            
            this.EmitSignal("ValueChanged", this.ValueName, delta, this.Value);
        }
        
        public void OnPointerEnter()
        {
            this.MouseOver = true;
            
            GlobalConstants.GameManager.GUIManager.Tooltip?.Show(
                this, 
                this.ValueName,
                null,
                this.Tooltip);
        }

        public void OnPointerExit()
        {
            this.MouseOver = false;
            
            GlobalConstants.GameManager.GUIManager.CloseGUI(this, GUINames.TOOLTIP);
            GlobalConstants.GameManager.GUIManager.Tooltip.Close(this);
        }
    }
}