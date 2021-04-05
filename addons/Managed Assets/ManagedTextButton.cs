using Code.Unity.GUI.Managed_Assets;
using Godot;

namespace JoyGodot.addons.Managed_Assets
{
    public class ManagedTextButton : ManagedButton
    {
        protected Label Text { get; set; }

        public override void _EnterTree()
        {
            base._EnterTree();
            this.Text = this.FindNode("Text") as Label;
            if (this.Text is null)
            {
                this.Text = new Label
                {
                    Name = "Text"
                };
                this.AddChild(this.Text);
            }
        }
    }
}