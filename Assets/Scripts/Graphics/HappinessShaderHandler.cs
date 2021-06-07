using Godot;
using JoyLib.Code.Entities;
using JoyLib.Code.Events;

namespace JoyLib.Code.Graphics
{
    public class HappinessShaderHandler : TileMap
    {
        protected bool Enabled { get; set; }

        protected const string HAPPINESS = "happiness";
        protected const string COLOUR = "colour";
        protected IEntity Player { get; set; }
        protected bool Initialised { get; set; }

        public override void _Ready()
        {
            this.Enabled = true;
        }

        public override void _PhysicsProcess(float delta)
        {
            if (this.Initialised)
            {
                return;
            }
            
            this.Player = GlobalConstants.GameManager.Player;

            if (this.Player is null)
            {
                return;
            }

            this.Initialised = this.TileSet is null == false;

            if (this.Initialised == false)
            {
                return;
            }

            this.Player.HappinessChange -= this.SetHappiness;
            this.Player.HappinessChange += this.SetHappiness;
            this.Initialised = true;
            this.SetHappiness(this, new ValueChangedEventArgs<float>
            {
                NewValue = this.Player.OverallHappiness
            });
        }

        protected void SetHappiness(object sender, ValueChangedEventArgs<float> args)
        {
            if (this.Initialised == false)
            {
                return;
            }
            
            float happiness = this.Enabled == false
                ? 1f
                : args.NewValue;

            foreach (int index in this.TileSet.GetTilesIds())
            {
                var shaderMaterial = this.TileSet.TileGetMaterial(index);
                shaderMaterial?.SetShaderParam(HAPPINESS, happiness);
            }
        }
    }
}

