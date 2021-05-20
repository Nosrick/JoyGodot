using System.Collections.Generic;
using System.Globalization;
using Castle.Core.Internal;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyLib.Code.Unity.GUI
{
    #if TOOLS
    [Tool]
    #endif
    public class IntValueItem : 
        Control, 
        IValueItem<int>,
        ILabelElement
    {
        protected ManagedLabel NameLabel { get; set; }
        protected ManagedLabel ValueLabel { get; set; }

        public ICollection<int> Values { get; set; }
        public int Maximum { get; set; }
        public int Minimum { get; set; }
        
        [Export]
        public bool UseRestriction { get; set; }
        
        public int PointRestriction { get; set; }

        public ICollection<string> Tooltip { get; set; }
        
        protected bool MouseInside { get; set; }

        [Export]
        public bool TitleCase
        {
            get => this.m_TitleCase;
            set
            {
                if (value && this.ValueName is null == false)
                {
                    this.ValueName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.ValueName);
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

        public int Value
        {
            get
            {
                this.CachedValue = int.TryParse(this.ValueLabel.Text, out int value) ? value : 0;

                return this.CachedValue;
            } 
            set
            {
                if (this.ValueLabel is null)
                {
                    GD.Print(this.GetType().Name + " ValueLabel is null!");
                    if (this.IsInsideTree())
                    {
                        GD.Print(this.GetPath());
                    }
                }
                else
                {
                    this.ValueLabel.Text = value.ToString();
                }
                
                this.CachedValue = value;
            }
        }
        protected int CachedValue { get; set; }

        public override void _Ready()
        {
            base._Ready();

            this.ValueLabel = this.GetNodeOrNull("Item Value") as ManagedLabel;
            this.NameLabel = this.GetNodeOrNull("Item Name") as ManagedLabel;
            
            if (this.ValueLabel is null == false)
            {
                this.ValueLabel.Text = this.CachedValue.ToString();
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
        }

        public void ChangeValue(int delta = 1)
        {
            if (this.UseRestriction && delta > this.PointRestriction)
            {
                return;
            }
            
            int newValue = this.Value + delta;
            if (newValue > this.Maximum || newValue < this.Minimum)
            {
                return;
            }

            this.Value = newValue;
            this.EmitSignal("ValueChanged", this.ValueName, delta, newValue);
        }

        public void OnPointerEnter()
        {
            GlobalConstants.GameManager.GUIManager.Tooltip?.Show(
                this.Name,
                null,
                this.Tooltip);
        }

        public void OnPointerExit()
        {
            GlobalConstants.GameManager.GUIManager.CloseGUI(GUINames.TOOLTIP);
            GlobalConstants.GameManager.GUIManager.Tooltip.Close();
        }
    }
}