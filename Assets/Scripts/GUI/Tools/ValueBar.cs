using System.Globalization;
using Godot;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Events;

namespace JoyGodot.Assets.Scripts.GUI.Tools
{
    public class ValueBar : Control
    {
        protected Label NameLabel { get; set; }
        protected Label ValueLabel { get; set; }
        protected TextureRect Background { get; set; }
        protected TextureRect Foreground { get; set; }

        public IDerivedValue DerivedValue
        {
            get => this.m_DerivedValue;
            set
            {
                this.m_DerivedValue = value;
                this.Name = value.Name;
                this.NameLabel.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.Name);
                this.Foreground.SelfModulate =
                    GlobalConstants.GameManager.DerivedValueHandler.GetBarColour(value.Name);
                Color textColour = GlobalConstants.GameManager.DerivedValueHandler.GetTextColour(value.Name);
                Color outlineColour = GlobalConstants.GameManager.DerivedValueHandler.GetOutlineColour(value.Name);
                this.NameLabel.AddColorOverride(
                    "font_color",
                    textColour);
                this.NameLabel.AddColorOverride(
                    "font_outline_modulate",
                    outlineColour);
                this.ValueLabel.AddColorOverride(
                    "font_color",
                    textColour);
                this.ValueLabel.AddColorOverride(
                    "font_outline_modulate",
                    outlineColour);
                this.SetValue();
            }
        }

        protected IDerivedValue m_DerivedValue;

        public override void _Ready()
        {
            base._Ready();
            
            this.Foreground = this.FindNode("Foreground") as TextureRect;
            this.Background = this.FindNode("Background") as TextureRect;
            this.NameLabel = this.FindNode("Name") as Label;
            this.ValueLabel = this.FindNode("Value") as Label;
        }

        protected void SetValue()
        {
            float value = this.DerivedValue.Value / (float) this.DerivedValue.Maximum;
            this.Foreground.AnchorRight = value > 0 ? value : 0;
            this.ValueLabel.Text = this.DerivedValue.Value + "/" + this.DerivedValue.Maximum;
        }

        public void OnDerivedValueChange(object sender, ValueChangedEventArgs<int> args)
        {
            this.SetValue();
        }
    }
}