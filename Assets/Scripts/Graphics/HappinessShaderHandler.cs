using Godot;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Events;
using JoyGodot.Assets.Scripts.Settings;

namespace JoyGodot.Assets.Scripts.Graphics
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
            
            this.Enabled = (bool) GlobalConstants.GameManager.SettingsManager.Get(SettingsManager.HAPPINESS_WORLD).ObjectValue;

            GlobalConstants.GameManager.SettingsManager.ValueChanged -= this.SettingChanged;
            GlobalConstants.GameManager.SettingsManager.ValueChanged += this.SettingChanged;

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
        
        protected void SettingChanged(object sender, ValueChangedEventArgs<object> args)
        {
            if (args.Name.Equals(SettingsManager.HAPPINESS_WORLD))
            {
                this.Enabled = (bool) args.NewValue;
                this.SetHappiness(this, new ValueChangedEventArgs<float>
                {
                    NewValue = this.Player.OverallHappiness
                });
            }
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

        public override void _ExitTree()
        {
            base._ExitTree();

            GlobalConstants.GameManager.SettingsManager.ValueChanged -= this.SettingChanged;
        }
    }
}

