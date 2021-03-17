using Godot;
using JoyLib.Code.Entities.AI.LOS.Providers;

namespace JoyLib.Code.Helpers
{
    public static class LightLevelHelper
    {
        private static float Normalise(int light, int minValue = 0, int maxValue = GlobalConstants.MAX_LIGHT)
        {
            int adjusted = light == 0 ? 1 : light;
            return (adjusted - minValue) / (float)(maxValue - minValue);
        }

        public static Color GetColour(int light, IVision vision)
        {
            float lerp = Normalise(light);
            Color displayColour = vision.DarkColour.LinearInterpolate(vision.LightColour, lerp);
            if(light > vision.MaximumLightLevel)
            {
                displayColour.a = 1f;
            }
            else if (light > vision.MaximumComfortLevel)
            {
                float alpha = Normalise(light, vision.MaximumComfortLevel, GlobalConstants.MAX_LIGHT);
                displayColour.a = alpha;
            }
            else if (light < vision.MinimumLightLevel)
            {
                displayColour.a = 1f;
            }
            else if(light < vision.MinimumComfortLevel)
            {
                float alpha = Normalise(light, 0, vision.MinimumComfortLevel);
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
