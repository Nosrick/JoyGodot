using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Scripts.GUI.WorldState
{
    public class ActionLogPanel : GUIData
    {
        protected Control LabelContainer { get; set; }
        protected List<RichTextLabel> Text { get; set; }
        
        protected DynamicFont CachedFont { get; set; }

        [Export] protected int LinesToKeep { get; set; }

        protected const int MINIMUM_LINES = 10;

        public override void _Ready()
        {
            base._Ready();

            this.LabelContainer = this.FindNode("LabelContainer") as Control;
            this.Text = new List<RichTextLabel>();
            
            this.CachedFont = (DynamicFont) GlobalConstants.GameManager.GUIManager.FontsInUse["Font"].Duplicate();
            this.CachedFont.Size = 14;
            this.CachedFont.OutlineSize = 1;

            if (this.LinesToKeep == 0)
            {
                this.LinesToKeep = MINIMUM_LINES;
            }

            GlobalConstants.ActionLog.TextAdded += this.AppendLine;
        }

        protected void AppendLine(string textAdded, LogLevel logLevel)
        {
            if (logLevel != LogLevel.Gameplay)
            {
                return;
            }
            
            if (this.Text.Count == this.LinesToKeep)
            {
                var label = this.LabelContainer.GetChild<Label>(0);
                this.LabelContainer.MoveChild(label, this.Text.Count);
                label.Text = textAdded;
            }
            else
            {
                var label = new RichTextLabel
                {
                    MouseFilter = MouseFilterEnum.Ignore,
                    SizeFlagsHorizontal = (int) SizeFlags.Fill,
                    SizeFlagsVertical = (int) SizeFlags.Fill,
                    FitContentHeight = true,
                    BbcodeEnabled = true,
                    RectMinSize = new Vector2(0, 14)
                };
                label.AppendBbcode(this.FormatText(textAdded));
                this.LabelContainer.AddChild(label);
                this.Text.Add(label);
                label.Theme ??= new Theme();
                label.Theme.DefaultFont = this.CachedFont;
            }
        }

        protected string FormatText(string text)
        {
            text = text.Replace("{You}", "[color=green]You[/color]");
            int leftIndex = text.Find("{");
            int rightIndex = text.Find("}");
            if (leftIndex >= 0 && rightIndex >= 1)
            {
                string sub = text.Substring(leftIndex, rightIndex - leftIndex + 1);
                string name = sub.Substring(1, sub.Length - 2);
                text = text.Replace(sub, "[color=red]" + name + "[/color]");
            }

            return text;
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            GlobalConstants.ActionLog.TextAdded -= this.AppendLine;
        }
    }
}