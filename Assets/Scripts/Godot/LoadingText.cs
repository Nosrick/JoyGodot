using Godot;

namespace JoyGodot.Assets.Scripts.Godot
{
    public class LoadingText : Label
    {
        public override void _Ready()
        { }

        public override void _Process(float delta)
        {
            if (GlobalConstants.GameManager is null == false)
            {
                this.Text = GlobalConstants.GameManager.LoadingMessage;
            }
        }
    }
}