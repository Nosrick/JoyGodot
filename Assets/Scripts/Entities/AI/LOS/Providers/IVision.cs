using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.Entities.AI.LOS.Providers
{
    public interface IVision
    {
        int MinimumLightLevel { get; }
        int MaximumLightLevel { get; }
        
        int MinimumComfortLevel { get; }
        int MaximumComfortLevel { get; }
        
        Color DarkColour { get; }
        Color LightColour { get; }
        
        string Name
        {
            get;
        }

        IEnumerable<Vector2Int> Vision
        {
            get;
        }

        Vector2Int[] GetVisibleWalls(IEntity viewer, IWorldInstance world);

        bool CanSee(IEntity viewer, IWorldInstance world, int x, int y);
        bool CanSee(IEntity viewer, IWorldInstance world, Vector2Int point);

        bool HasVisibility(IEntity viewer, IWorldInstance world, int x, int y);
        bool HasVisibility(IEntity viewer, IWorldInstance world, Vector2Int point);

        Rect2Int GetVisionRect(IEntity viewer);

        Rect2Int GetFullVisionRect(IEntity viewer);

        void Update(IEntity viewer, IWorldInstance world);
    }
}