using Godot;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyGodot.Assets.Scripts.GUI.WorldState
{
    public class EntryBanner : GUIData
    {
        protected ManagedLabel Title { get; set; }
        protected Tween Tween { get; set; }

        public string TitleText
        {
            get => this.Title.Text;
            set => this.Title.Text = value;
        }

        public override void _Ready()
        {
            base._Ready();
            this.Title = GD.Load<PackedScene>(
                    GlobalConstants.GODOT_ASSETS_FOLDER +
                    "Scenes/Parts/ManagedLabel.tscn")
                .Instance<ManagedLabel>();
            this.Title.ElementName = "DefaultWindowSmooth";
            this.AddChild(this.Title);

            this.Tween = new Tween();
            this.AddChild(this.Tween);

            this.Modulate = new Color(1, 1, 1, 0);
        }

        public override void Display()
        {
            this.GrabPlayer();
            this.Visible = true;
            this.CallDeferred(nameof(this.TweenIn));
        }

        protected void TweenIn()
        {
            //this.Tween.Stop(this, "modulate");
            this.Tween.InterpolateProperty(
                this,
                "modulate",
                new Color(1, 1, 1, 0),
                Colors.White,
                0.5f, 
                Tween.TransitionType.Linear,
                Tween.EaseType.InOut, 
                0.5f);
            this.Tween.Start();

            this.CallDeferred(nameof(this.TweenOut));
        }

        protected void TweenOut()
        {
            this.Tween.InterpolateProperty(
                this,
                "modulate",
                Colors.White,
                new Color(1, 1, 1, 0),
                0.5f,
                Tween.TransitionType.Linear,
                Tween.EaseType.InOut,
                2f);
            this.Tween.Start();
        }
    }
}