using System;
using Castle.Core.Internal;
using Godot;

namespace JoyLib.Code.Helpers
{
    public static class GraphicsHelper
    {
        public static NinePatchRect.AxisStretchMode ParseStretchMode(string mode)
        {
            if (mode.IsNullOrEmpty())
            {
                return NinePatchRect.AxisStretchMode.Stretch;
            }
            
            if (mode.Equals("tile", StringComparison.OrdinalIgnoreCase))
            {
                return NinePatchRect.AxisStretchMode.Tile;
            }

            if (mode.Equals("tile-fit", StringComparison.OrdinalIgnoreCase))
            {
                return NinePatchRect.AxisStretchMode.TileFit;
            }

            return NinePatchRect.AxisStretchMode.Stretch;
        }
    }
}