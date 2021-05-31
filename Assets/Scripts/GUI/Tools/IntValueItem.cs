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

        [Export] public bool UseRestriction { get; set; }

        public int PointRestriction { get; set; }

        public int IncreaseCost { get; set; }
        public int DecreaseCost { get; set; }

        public ICollection<string> Tooltip { get; set; }

        public bool MouseOver { get; protected set; }

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
                    GD.PushWarning(this.GetType().Name + " NameLabel is null!");
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
                    GD.PushWarning(this.GetType().Name + " ValueLabel is null!");
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

        public void ChangeValue(bool increase)
        {
            if (this.UseRestriction)
            {
                if (increase
                    && (this.IncreaseCost > this.PointRestriction
                    || this.Value == this.Maximum))
                {
                    return;
                }

                if (increase == false
                    && this.Value == this.Minimum)
                {
                    return;
                }
            }

            int newValue = this.Value + (increase ? 1 : -1);
            if (newValue > this.Maximum || newValue < this.Minimum)
            {
                return;
            }

            this.Value = newValue;
            this.EmitSignal("ValueChanged", this.ValueName, increase ? this.IncreaseCost : this.DecreaseCost, newValue);
        }

        public void OnPointerEnter()
        {
            this.MouseOver = true;

            GlobalConstants.GameManager.GUIManager.Tooltip?.Show(
                this,
                this.Name,
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