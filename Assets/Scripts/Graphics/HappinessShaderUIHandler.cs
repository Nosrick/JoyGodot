using JoyLib.Code.Entities;
using JoyLib.Code.Events;

namespace JoyLib.Code.Graphics
{
    /*
    public class HappinessShaderUIHandler : MonoBehaviour
    {
        protected Image[] Images { get; set; }
        protected bool Enabled { get; set; }

        protected const string _HAPPINESS = "_Happiness";

        protected bool PlayerDetected { get; set; }

        protected void Start()
        {
            this.Images = this.GetComponentsInChildren<Image>();

            GlobalConstants.GameManager.SettingsManager.OnSettingChange -= this.UpdateSetting;
            GlobalConstants.GameManager.SettingsManager.OnSettingChange += this.UpdateSetting;
            
            JoyUIShaderSetting setting = GlobalConstants.GameManager.SettingsManager.GetSetting("Joy UI Shader") as JoyUIShaderSetting;
            this.Enabled = setting?.value ?? false;

            this.SetHappiness(1f);
        }

        protected void UpdateSetting(SettingChangedEventArgs args)
        {
            if (args.Setting is JoyUIShaderSetting shaderSetting)
            {
                this.Enabled = shaderSetting.value;
            }
        }

        // Update is called once per frame
        protected void Update()
        {
            IEntity player = GlobalConstants.GameManager?.Player;

            if (player is null)
            {
                return;
            }

            if (this.PlayerDetected == false)
            {
                this.PlayerDetected = true;
                player.HappinessIsDirty = true;
            }
            
            if (player.HappinessIsDirty == false)
            {
                return;
            }

            this.SetHappiness(player.OverallHappiness);
        }

        protected void SetHappiness(float happinessRef)
        {
            float happiness = this.Enabled == false
                ? 1f
                : happinessRef;

            foreach (var renderer in this.Images)
            {
                renderer.material.SetFloat(_HAPPINESS, happiness);
            }
        }
    }
    */
}