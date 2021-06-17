using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Godot;
using JoyGodot.Assets.Scripts.Entities.AI.LOS.Boards;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.Entities.AI.LOS.Providers
{
    public class BaseVisionProvider : IVision
    {
        public string Name { get; protected set; }
        
        public int MinimumLightLevel { get; protected set; }
        
        public int MaximumLightLevel { get; protected set; }
        
        public int MinimumComfortLevel { get; protected set; }
        
        public int MaximumComfortLevel { get; protected set; }

        public Color DarkColour { get; protected set; }
        
        public Color LightColour { get; protected set; }

        protected HashSet<Vector2Int> m_Vision;

        public BaseVisionProvider(
            Color darkColour,
            Color lightColour,
            IFOVHandler algorithm,
            int minimumLightLevel = 0,
            int minimumComfortLevel = 0,
            int maximumLightLevel = GlobalConstants.MAX_LIGHT,
            int maximumComfortLevel = GlobalConstants.MAX_LIGHT,
            string name = null)
        {
            if (name.IsNullOrEmpty() == false)
            {
                this.Name = name;
            }

            this.DarkColour = darkColour;
            this.LightColour = lightColour;
            this.Algorithm = algorithm;
            this.m_Vision = new HashSet<Vector2Int>();
            this.MinimumLightLevel = minimumLightLevel;
            this.MaximumLightLevel = maximumLightLevel;
            this.MinimumComfortLevel = minimumComfortLevel;
            this.MaximumComfortLevel = maximumComfortLevel;
            this.m_Vision = new HashSet<Vector2Int>();
        }

        public virtual bool CanSee(IEntity viewer, IWorldInstance world, int x, int y)
        {
            return this.CanSee(viewer, world, new Vector2Int(x, y));
        }

        public virtual bool CanSee(IEntity viewer, IWorldInstance world, Vector2Int point)
        {
            return this.m_Vision.Any(p => p.Equals(point));
        }

        public virtual bool HasVisibility(IEntity viewer, IWorldInstance world, int x, int y)
        {
            return this.HasVisibility(viewer, world, new Vector2Int(x, y));
        }

        public virtual bool HasVisibility(IEntity viewer, IWorldInstance world, Vector2Int point)
        {
            int lightLevel = world.LightCalculator?.Light?.GetLight(point) ?? 0;
            return this.CanSee(viewer, world, point) && lightLevel >= this.MinimumLightLevel && lightLevel <= this.MaximumLightLevel;
        }

        public virtual Rect2 GetFullVisionRect(IEntity viewer)
        {
            Rect2 visionRect = new Rect2(0, 0, viewer.MyWorld.Dimensions.x, viewer.MyWorld.Dimensions.y);
            return visionRect;
        }

        public virtual Vector2Int[] GetVisibleWalls(IEntity viewer, IWorldInstance world)
        {
            Vector2Int[] visibleWalls = viewer.MyWorld.Walls.Where(
                    wall => viewer.VisionProvider.CanSee(viewer, world, wall))
                .ToArray();
            return visibleWalls;
        }

        public virtual Rect2 GetVisionRect(IEntity viewer)
        {
            Rect2 visionRect = new Rect2(viewer.WorldPosition.ToVec2(), new Vector2(viewer.VisionMod * 2 + 1, viewer.VisionMod * 2 + 1));
            return visionRect;
        }

        public virtual void Update(IEntity viewer, IWorldInstance world)
        {
            this.Vision = new HashSet<Vector2Int>();

            this.Board = this.Algorithm.Do(
                viewer,
                world,
                world.Dimensions,
                world.Walls);

            this.Vision = this.Board.GetVision();
        }

        public IEnumerable<Vector2Int> Vision 
        { 
            get => this.m_Vision;
            protected set => this.m_Vision = new HashSet<Vector2Int>(value);
        }

        protected IFOVHandler Algorithm
        {
            get;
            set;
        }

        protected IFOVBoard Board
        {
            get;
            set;
        }
    }
}