using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.Events;

namespace JoyGodot.Assets.Scripts.GUI.WorldState
{
    public class NeedsPanel : GUIData
    {
        protected VBoxContainer LabelContainer { get; set; }
        
        protected List<RichTextLabel> Parts { get; set; }
        
        protected DynamicFont CachedFont { get; set; }

        public override void _Ready()
        {
            base._Ready();

            this.Parts = new List<RichTextLabel>();
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

            foreach (RichTextLabel label in this.Parts)
            {
                label.Visible = false;
            }
            
            for (int i = this.Parts.Count; i < needs.Count; i++)
            {
                var label = new RichTextLabel
                {
                    OverrideSelectedFontColor = true,
                    BbcodeEnabled = true,
                    FitContentHeight = true,
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

            for(int i = 0; i < needs.Count; i++)
            {
                var need = needs.ElementAt(i);
                
                if (need.ContributingHappiness)
                {
                    continue;
                }

                RichTextLabel part = this.Parts[i];
                part.Clear();
                part.Visible = true;
                part.Name = need.Name;
                string colourCode = "#" + (need.Value < need.HappinessThreshold / 2 ? Colors.Red.ToHtml(false) : Colors.Yellow.ToHtml(false));
                part.AppendBbcode(
                    "[center][color=" + colourCode + "]" + 
                    CultureInfo.CurrentCulture.TextInfo.ToTitleCase(need.Name) + 
                    "[/color][/center]");
            }
        }
    }
}