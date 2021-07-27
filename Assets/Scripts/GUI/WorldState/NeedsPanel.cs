using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.Events;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Scripts.GUI.WorldState
{
    public class NeedsPanel : GUIData
    {
        protected VBoxContainer LabelContainer { get; set; }

        protected List<Label> Parts { get; set; }

        protected DynamicFont CachedFont { get; set; }

        public override void _Ready()
        {
            base._Ready();

            this.Parts = new List<Label>();
            this.LabelContainer = this.FindNode("TextContainer") as VBoxContainer;
            this.CachedFont = GlobalConstants.GameManager.GUIManager.FontsInUse["Font"];
            this.SetNeeds();
            this.Player.NeedChange += this.OnNeedsChange;
        }

        protected void OnNeedsChange(object sender, ValueChangedEventArgs<int> args)
        {
            this.SetNeeds();
        }

        protected void SetNeeds()
        {
            if (this.Player is null)
            {
                return;
            }

            ICollection<INeed> needs = this.Player.Needs.Values;

            foreach (Label label in this.Parts)
            {
                label.Visible = false;
            }

            for (int i = this.Parts.Count; i < needs.Count; i++)
            {
                var label = new Label
                {
                    Align = Label.AlignEnum.Center,
                    Valign = Label.VAlign.Center,
                    RectMinSize = new Vector2(0, 16)
                };
                label.Theme ??= new Theme();
                DynamicFont dynamicFont = (DynamicFont) this.CachedFont.Duplicate();
                dynamicFont.Size = 16;
                dynamicFont.OutlineSize = 1;
                label.Theme.DefaultFont = dynamicFont;
                this.Parts.Add(label);
                this.LabelContainer.AddChild(label);
                label.Visible = false;
            }

            for (int i = 0; i < needs.Count; i++)
            {
                var need = needs.ElementAt(i);

                if (need.ContributingHappiness)
                {
                    continue;
                }

                Label part = this.Parts[i];
                part.Visible = true;
                part.Name = need.Name;
                string titleCase = need.DisplayName.ToTitleCase();
                part.Text = need.Value < need.HappinessThreshold / 2
                    ? "Very " + titleCase
                    : titleCase;
            }
        }
    }
}