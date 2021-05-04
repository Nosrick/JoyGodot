using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Castle.Core.Internal;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyLib.Code.Unity.GUI
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

        public ICollection<string> Tooltip { get; set; }

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
        
        public int Minimum { get; set; }
        public int Maximum { get; set; }

        protected int Index { get; set; }

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
        public delegate void ValueChanged(string name, int delta, int newValue);

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
                    GD.Print(this.GetType().Name + " NameLabel is null!");
                    if (this.IsInsideTree())
                    {
                        GD.Print(this.GetPath());
                    }

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
                    GD.Print(this.GetType().Name + " ValueLabel is null!");
                    if (this.IsInsideTree())
                    {
                        GD.Print(this.GetPath());
                    }

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
            GD.Print(this.GetType().Name + " initialising");
            this.Values = new List<string>();
            this.ValueLabel = this.GetNodeOrNull("Item Value") as ManagedLabel;
            if (this.ValueLabel is null)
            {
                GD.Print("ValueLabel of " + this.GetType().Name + " is null!");
                if (this.IsInsideTree())
                {
                    GD.Print(this.GetPath());
                }
            }

            this.Tooltip = new List<string> {"Example tooltip"};
            
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
                GD.Print("NameLabel of " + this.GetType().Name + " is null!");
                if (this.IsInsideTree())
                {
                    GD.Print(this.GetPath());
                }
            }
            GD.Print(this.GetType().Name + " finished initialising");
        }

        public void ChangeValue(int delta = 1)
        {
            GD.Print("Calling " + nameof(this.ChangeValue));
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
            if (Input.IsActionPressed("tooltip_show") == false
                || this.Tooltip.IsNullOrEmpty())
            {
                return;
            }
            
            GlobalConstants.GameManager.GUIManager.Tooltip?.Show(
                this.Name,
                null,
                this.Tooltip);
        }

        public void OnPointerExit()
        {
            GlobalConstants.GameManager.GUIManager.Tooltip?.Hide();
        }
    }
}