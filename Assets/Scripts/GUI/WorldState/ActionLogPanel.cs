using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Scripts.GUI.WorldState
{
    public class ActionLogPanel : GUIData
    {
        protected Control LabelContainer { get; set; }
        protected List<Label> Text { get; set; }
        
        protected DynamicFont CachedFont { get; set; }

        [Export] protected int LinesToKeep { get; set; }

        public override void _Ready()
        {
            base._Ready();

            this.LabelContainer = this.FindNode("LabelContainer") as Control;
            this.Text = new List<Label>();
            
            this.CachedFont = (DynamicFont) GlobalConstants.GameManager.GUIManager.FontsInUse["Font"].Duplicate();
            this.CachedFont.Size = 12;

            if (this.LinesToKeep == 0)
            {
                this.LinesToKeep = ActionLog.LINES_TO_KEEP;
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
                var label = new Label
                {
                    MouseFilter = MouseFilterEnum.Ignore,
                    Text = textAdded,
                    Align = Label.AlignEnum.Left,
                    SizeFlagsHorizontal = (int) SizeFlags.ExpandFill,
                    SizeFlagsVertical = (int) SizeFlags.ExpandFill
                };
                this.LabelContainer.AddChild(label);
                this.Text.Add(label);
                label.Theme ??= new Theme();
                label.Theme.SetFont("font", nameof(Label), this.CachedFont);
            }
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            GlobalConstants.ActionLog.TextAdded -= this.AppendLine;
        }
    }
}