using System;
using System.Collections.Generic;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.AI;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Events;
using JoyLib.Code.World.Lighting;

namespace JoyLib.Code.World
{
    public interface IWorldInstance : ITagged, IDisposable, IGuidHolder, ITickable
    {
        HashSet<Guid> EntityGUIDs { get; }
        HashSet<Guid> ItemGUIDs { get; }
        WorldTile[,] Tiles { get; }
        byte[,] Costs { get; }
        LightCalculator LightCalculator { get; }
        Dictionary<Vector2Int, IWorldInstance> Areas { get; }
        HashSet<IJoyObject> Objects { get; }
        HashSet<IEntity> Entities { get; }
        Dictionary<Vector2Int, IJoyObject> Walls { get; }
        Vector2Int SpawnPoint { get; set; }
        IWorldInstance Parent { get; set; }
        string Name { get; }
        IEntity Player { get; }
        Vector2Int Dimensions { get; }
        bool IsDirty { get; }
        
        bool Initialised { get; }

        void Initialise();
        
        void SetDateTime(DateTime dateTime);
        void AddObject(IJoyObject objectRef);
        bool RemoveObject(Vector2Int positionRef, IItemInstance itemRef);
        IJoyObject GetObject(Vector2Int WorldPosition);
        void Tick();
        IEnumerable<IJoyObject> SearchForObjects(IEntity entityRef, IEnumerable<string> tags);
        IEnumerable<IEntity> SearchForEntities(IEntity actor, IEnumerable<string> searchCriteria);
        IEntity GetRandomSentient();
        IEntity GetRandomSentientWorldWide();
        List<IWorldInstance> GetWorlds(IWorldInstance parent);
        Vector2Int GetTransitionPointForParent();
        IWorldInstance GetOverworld();
        IWorldInstance GetPlayerWorld(IWorldInstance parent);
        void SwapPosition(IEntity left, IEntity right);
        IItemInstance PickUpObject(IEntity entityRef);
        void AddEntity(IEntity entityRef);
        void RemoveEntity(Vector2Int positionRef, bool destroy = false);
        IEntity GetEntity(Vector2Int positionRef);
        Sector GetSectorFromPoint(Vector2Int point);
        List<Vector2Int> GetVisibleWalls(IEntity viewer);
        Dictionary<Vector2Int, IJoyObject> GetObjectsOfType(string[] tags);
        void AddArea(Vector2Int key, IWorldInstance value);

        event EmptyEventHandler OnTick;
    }
}