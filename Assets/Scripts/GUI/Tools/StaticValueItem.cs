using System.Collections.Generic;
using System.Globalization;
using Godot;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyGodot.Assets.Scripts.GUI.Tools
{
    public class StaticValueItem : 
        Control, 
        IValueItem<string>
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
        public bool MouseOver { get; protected set; }

        public ICollection<string> Values { get; set; }
        
        [Export]
        public bool TitleCase
        {
            get => this.m_TitleCase;
            set
            {
                this.m_TitleCase = value;
                
                if (value && this.ValueName is null == false)
                {
                    this.ValueName = this.ValueName;
                }
            }
        }

        protected bool m_TitleCase;
        
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
                    GD.PushWarning(this.GetType().Name + " NameLabel is null!");
                    return;
                }
                
                this.NameLabel.Text = this.TitleCase 
                    ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value) 
                    : value;
            }
        }

        public string Value
        {
            get => this.CachedValue;
            set
            {
                this.CachedValue = this.TitleCase 
                    ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value) 
                    : value;
                
                if (this.ValueLabel is null)
                {
                    GD.PushWarning(this.GetType().Name + " ValueLabel is null!");
                }
                else
                {
                    this.ValueLabel.Text = this.CachedValue;
                }
            }
        }
        
        protected string CachedValue { get; set; }
        
        public int Minimum { get; set; }
        public int Maximum { get; set; }
        
        public override void _Ready()
        {
            base._Ready();

            this.ValueLabel = this.GetNodeOrNull("Item Value") as ManagedLabel;
            this.NameLabel = this.GetNodeOrNull("Item Name") as ManagedLabel;
            
            if (this.ValueLabel is null == false)
            {
                this.ValueLabel.Text = this.CachedValue;
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