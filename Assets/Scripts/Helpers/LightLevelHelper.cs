using Godot;
using JoyGodot.Assets.Scripts.Entities.AI.LOS.Providers;

namespace JoyGodot.Assets.Scripts.Helpers
{
    public static class LightLevelHelper
    {
        private static float Normalise(int minValue, int maxValue, int light)
        {
            int adjusted = light == 0 ? 1 : light;
            return (adjusted - minValue) / (float)(maxValue - minValue);
        }

        public static Color GetColour(int light, IVision vision)
        {
            float lerp = Normalise(0, GlobalConstants.MAX_LIGHT, light);
            Color displayColour = vision.DarkColour.LinearInterpolate(vision.LightColour, lerp);
            if(light > vision.MaximumLightLevel || light < vision.MinimumLightLevel)
            {
                displayColour.a = 1f;
            }
            else if (light > vision.MaximumComfortLevel)
            {
                float alpha = Normalise(vision.MaximumComfortLevel, GlobalConstants.MAX_LIGHT, light);
                displayColour.a = alpha;
            }
            else if(light < vision.MinimumComfortLevel)
            {
                float alpha = Normalise(0, vision.MinimumComfortLevel, light);
                displayColour.a = 1f - alpha;
            }
            else
            {
                displayColour.a = 0f;
            }
            return displayColour;
        }
    }
}
