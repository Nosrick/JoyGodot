﻿using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Calendar;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.AI;
using JoyGodot.Assets.Scripts.Events;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Physics;
using JoyGodot.Assets.Scripts.World.Lighting;
using JoyGodot.Assets.Scripts.World.WorldInfo;

namespace JoyGodot.Assets.Scripts.World
{
    public interface IWorldInstance : 
        ITagged, 
        IDisposable, 
        IGuidHolder, 
        ITickable,
        ISerialisationHandler
    {
        HashSet<Guid> EntityGUIDs { get; }
        HashSet<Guid> ItemGUIDs { get; }
        WorldTile[,] Tiles { get; }
        byte[,] Costs { get; }
        LightCalculator LightCalculator { get; }
        Dictionary<Vector2Int, IWorldInstance> Areas { get; }
        HashSet<IItemInstance> Items { get; }
        HashSet<IEntity> Entities { get; }
        HashSet<Vector2Int> Walls { get; }
        Vector2Int SpawnPoint { get; set; }
        IWorldInstance Parent { get; set; }
        string Name { get; }
        Vector2Int Dimensions { get; }
        bool IsDirty { get; }
        
        bool Initialised { get; }

        void Initialise();
        
        void SetDateTime(JoyDateTime dateTime);
        void AddItem(IItemInstance objectRef);
        void AddWall(Vector2Int wall);
        
        bool RemoveObject(Vector2Int positionRef, IItemInstance itemRef);
        IJoyObject GetObject(Vector2Int WorldPosition);
        PhysicsResult IsObjectAt(Vector2Int worldPosition);
        void Tick();
        IEnumerable<IItemInstance> SearchForObjects(IEntity entityRef, IEnumerable<string> tags);
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