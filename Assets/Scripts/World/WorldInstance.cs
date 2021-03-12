﻿using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.AI;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Helpers;
using JoyLib.Code.Rollers;
using JoyLib.Code.World.Lighting;

namespace JoyLib.Code.World
{
    [Serializable]
    public class WorldInstance : IWorldInstance
    {
        public event EventHandler OnTick;
        
        protected WorldTile[,] m_Tiles;
        protected byte[,] m_Costs;

         
        protected Vector2Int m_Dimensions;

         
        //Worlds and where to access them
        protected Dictionary<Vector2Int, IWorldInstance> m_Areas;
        
        [NonSerialized]
        protected IWorldInstance m_Parent;

          
        public HashSet<Guid> EntityGUIDs
        {
            get => this.m_EntityGUIDs;
            protected set => this.m_EntityGUIDs = value;
        }

        protected HashSet<Guid> m_EntityGUIDs;
        
        protected HashSet<IEntity> m_Entities;

        public HashSet<IEntity> Entities => this.m_Entities;

          
        public HashSet<Guid> ItemGUIDs
        {
            get => this.m_ItemGUIDs;
            protected set => this.m_ItemGUIDs = value;
        }

        protected HashSet<Guid> m_ItemGUIDs;

        protected HashSet<IJoyObject> m_Objects;
        
         
        protected Dictionary<Vector2Int, IJoyObject> m_Walls;

         
        protected Vector2Int m_SpawnPoint;

        protected static DateTime s_DateTime;

         
        protected string m_Name;
        
         
        protected Guid m_GUID;
        
         
        public LightCalculator LightCalculator { get; protected set; }

        public bool Initialised { get; protected set; }

        protected static Node2D FogOfWarHolder { get; set; }
        
        protected static Node2D WallHolder { get; set; }
        
        protected static Node2D ObjectHolder { get; set; }
        
        protected static Node2D EntityHolder { get; set; }
        
        [NonSerialized]
        protected ILiveEntityHandler EntityHandler;
        
         
        protected RNG Roller;
        
         
        protected List<string> m_Tags;

        /// <summary>
        /// A template for adding stuff to later. A blank WorldInstance.
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="tags"></param>
        /// <param name="name"></param>
        /// <param name="entityHandler"></param>
        /// <param name="roller"></param>
        public WorldInstance(
            WorldTile[,] tiles, 
            IEnumerable<string> tags, 
            string name, 
            ILiveEntityHandler entityHandler,
            RNG roller = null)
        {
            this.EntityHandler = entityHandler;
            this.Roller = roller ?? new RNG();

            this.m_Dimensions = new Vector2Int(tiles.GetLength(0), tiles.GetLength(1));

            this.Name = name;
            this.Tags = new List<string>(tags);
            this.m_Tiles = tiles;
            this.m_Areas = new Dictionary<Vector2Int, IWorldInstance>();
            this.m_Entities = new HashSet<IEntity>();
            this.m_EntityGUIDs = new HashSet<Guid>();
            this.m_Objects = new HashSet<IJoyObject>();
            this.m_ItemGUIDs = new HashSet<Guid>();
            this.m_Walls = new Dictionary<Vector2Int, IJoyObject>();
            //this.Guid = GlobalConstants.GameManager.GUIDManager.AssignGUID();

            this.LightCalculator = new LightCalculator();

            this.m_Costs = new byte[this.m_Tiles.GetLength(0), this.m_Tiles.GetLength(1)];
            for (int x = 0; x < this.m_Costs.GetLength(0); x++)
            {
                for (int y = 0; y < this.m_Costs.GetLength(1); y++)
                {
                    this.m_Costs[x, y] = 1;
                }
            }

            this.Initialise();
        }

        /// <summary>
        /// For creating a well-established WorldInstance
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="areas"></param>
        /// <param name="entities"></param>
        /// <param name="objects"></param>
        /// <param name="tags"></param>
        /// <param name="name"></param>
        public WorldInstance(WorldTile[,] tiles, Dictionary<Vector2Int, IWorldInstance> areas, HashSet<IEntity> entities,
            HashSet<IJoyObject> objects, Dictionary<Vector2Int, IJoyObject> walls, string[] tags, string name)
        {
            this.Name = name;
            this.Tags = new List<string>(tags);
            this.m_Tiles = tiles;
            this.m_Dimensions = new Vector2Int(tiles.GetLength(0), tiles.GetLength(1));
            this.m_Areas = areas;
            this.m_Entities = entities;
            this.m_EntityGUIDs = new HashSet<Guid>(this.m_Entities.Select(entity => entity.Guid));
            this.m_Objects = objects;
            this.m_ItemGUIDs = new HashSet<Guid>(this.m_Objects.Select(o => o.Guid).ToList());
            this.m_Walls = walls;
            //this.Guid = GlobalConstants.GameManager.GUIDManager.AssignGUID();
            this.CalculatePlayerIndex();

            this.m_Costs = new byte[this.m_Tiles.GetLength(0), this.m_Tiles.GetLength(1)];
            for (int x = 0; x < this.m_Costs.GetLength(0); x++)
            {
                for (int y = 0; y < this.m_Costs.GetLength(1); y++)
                {
                    this.m_Costs[x, y] = 1;
                }
            }

            this.LightCalculator = new LightCalculator();

            foreach (Vector2Int position in this.m_Walls.Keys)
            {
                this.m_Costs[position.x, position.y] = byte.MaxValue;
            }
            
            this.Initialise();
        }

        public void Initialise()
        {
            if (this.Initialised)
            {
                return;
            }

            //this.EntityHandler = GlobalConstants.GameManager.EntityHandler;

            //FogOfWarHolder = FogOfWarHolder ?? ("WorldFog");
            //WallHolder = WallHolder ? WallHolder : GameObject.Find("WorldWalls");
            //ObjectHolder = ObjectHolder ? ObjectHolder : GameObject.Find("WorldObjects");
            //EntityHolder = EntityHolder ? EntityHolder : GameObject.Find("WorldEntities");
            
            this.m_Objects = new HashSet<IJoyObject>();
            this.m_Entities = new HashSet<IEntity>();
            //this.m_EntityGUIDs = new List<long>();
            //this.m_ItemGUIDs = new List<long>();

            foreach (IWorldInstance child in this.m_Areas.Values)
            {
                child.Initialise();
            }
            
            this.CalculatePlayerIndex();
            
            this.Initialised = true;
        }

        public void SetDateTime(DateTime dateTime)
        {
            s_DateTime = dateTime;
        }

        protected void CalculatePlayerIndex()
        {
            IEntity player = this.m_Entities.FirstOrDefault(entity => entity.PlayerControlled);
            if (player is null == false)
            {
                this.Player = player;
                this.EntityHandler.SetPlayer(player);
            }
        }

        protected void CalculateLightLevels()
        {
            //Do objects first
            List<IItemInstance> objects = this.m_Objects.Where(o => o is IItemInstance item && item.ItemType.LightLevel > 0)
                .Cast<IItemInstance>()
                .ToList();

            objects.AddRange(this.m_Entities.SelectMany(entity =>
                entity.Contents.Where(instance => instance.ItemType.LightLevel > 0)));

            this.LightCalculator.Do(objects, this, this.Dimensions, this.Walls.Keys);
        }

        public void AddObject(IJoyObject objectRef)
        {
            if (objectRef.IsWall)
            {
                this.AddWall(objectRef);
            }
            else
            {
                this.m_Objects.Add(objectRef);
                this.m_ItemGUIDs.Add(objectRef.Guid);
                if (objectRef is IItemInstance item)
                {
                    item.InWorld = true;
                }
            }

            objectRef.MyWorld = this;

            this.IsDirty = true;
        }

        protected void AddWall(IJoyObject wallRef)
        {
            this.m_Walls.Add(wallRef.WorldPosition, wallRef);
            this.m_Costs[wallRef.WorldPosition.x, wallRef.WorldPosition.y] = byte.MaxValue;
        }

        public bool RemoveObject(Vector2Int positionRef, IItemInstance itemRef)
        {
            bool removed = false;

            if (this.m_Objects.Any(o => o.WorldPosition.Equals(positionRef) && itemRef.Guid.Equals(o.Guid)) == false)
            {
                return false;
            }

            removed = this.m_Objects.Remove(itemRef) & this.m_ItemGUIDs.Remove(itemRef.Guid);

            if (removed)
            {
                this.IsDirty = true;

                itemRef.InWorld = false;
                itemRef.MyWorld = null;
            }

            return removed;
        }

        public IJoyObject GetObject(Vector2Int worldPosition)
        {
            return this.m_Objects.FirstOrDefault(o => o.WorldPosition == worldPosition);
        }

        public void Tick()
        {
            DateTime oldTime = s_DateTime;
            this.CalculateLightLevels();
            if (this.HasTag("overworld"))
            {
                s_DateTime = s_DateTime.AddHours(1.0);
            }
            else
            {
                s_DateTime = s_DateTime.AddSeconds(6.0);
            }

            this.OnTick?.Invoke(this, EventArgs.Empty);

            this.IsDirty = false;
        }

        /// <summary>
        /// Searches for any objects that match the tags specified, which the entity can see.
        /// </summary>
        /// <param name="entityRef"></param>
        /// <param name="tags"></param>
        /// <param name="objectType"></param>
        /// <param name="intentRef"></param>
        /// <returns></returns>
        public IEnumerable<IJoyObject> SearchForObjects(IEntity entityRef, IEnumerable<string> tags)
        {
            List<IJoyObject> data = new List<IJoyObject>();

            if (entityRef.VisionProvider.Vision.Any() == false)
            {
                return data;
            }

            List<IJoyObject> inSight = this.m_Objects
                .Where(obj => entityRef.VisionProvider.CanSee(entityRef, this, obj.WorldPosition) == true).ToList();
            foreach (IJoyObject obj in inSight)
            {
                IEnumerable<string> intersect = obj.Tags.Intersect(tags);
                if (tags.Any() == false || intersect.SequenceEqual(tags))
                {
                    data.Add(obj);
                }
            }

            return data;
        }

        public IEnumerable<IEntity> SearchForEntities(IEntity actor, IEnumerable<string> searchCriteria)
        {
            List<IEntity> searchEntities = new List<IEntity>();

            foreach (IEntity entity in this.m_Entities)
            {
                if (actor.Guid == entity.Guid
                    || !actor.VisionProvider.CanSee(actor, this, entity.WorldPosition))
                {
                    continue;
                }

                IEnumerable<Tuple<string, int>> data = entity.GetData(searchCriteria.ToArray());
                IEnumerable<string> tags = data.Select(x => x.Item1);
                if (tags.SequenceEqual(searchCriteria))
                {
                    searchEntities.Add(entity);
                }
            }

            return searchEntities;
        }

        public IEntity GetRandomSentient()
        {
            if (this.m_Entities.Count == 0)
            {
                return null;
            }

            List<IEntity> sentients = this.m_Entities.Where(x => x.Sentient).ToList();

            if (!(this.Player is null))
            {
                sentients = sentients.Where(entity => entity.Guid.Equals(this.Player.Guid) == false).ToList();
            }

            return sentients.Count > 0 ? sentients[this.Roller.Roll(0, sentients.Count)] : null;
        }

        public IEntity GetRandomSentientWorldWide()
        {
            List<IWorldInstance> worlds = this.GetWorlds(this.GetOverworld());
            int result = this.Roller.Roll(0, worlds.Count);
            IEntity entity = worlds[result].GetRandomSentient();

            int breakout = 0;
            while (entity == null && breakout < 100)
            {
                result = this.Roller.Roll(0, worlds.Count);
                entity = worlds[result].GetRandomSentient();
                breakout++;
            }

            return entity;
        }

        public List<IWorldInstance> GetWorlds(IWorldInstance parent)
        {
            List<IWorldInstance> worlds = new List<IWorldInstance>();
            worlds.Add(parent);
            for (int i = 0; i < worlds.Count; i++)
            {
                foreach (IWorldInstance world in worlds[i].Areas.Values)
                {
                    List<IWorldInstance> newWorlds = this.GetWorlds(world);
                    for (int j = 0; j < newWorlds.Count; j++)
                    {
                        if (!worlds.Contains(newWorlds[j]))
                            worlds.Add(newWorlds[j]);
                    }
                }
            }

            return worlds;
        }

        public Vector2Int GetTransitionPointForParent()
        {
            foreach (KeyValuePair<Vector2Int, IWorldInstance> pair in this.Parent.Areas)
            {
                if (pair.Value.Guid == this.Guid)
                {
                    return pair.Key;
                }
            }

            return new Vector2Int(-1, -1);
        }

        public IWorldInstance GetOverworld()
        {
            if (this.Parent == null)
            {
                return this;
            }
            else
            {
                return this.Parent.GetOverworld();
            }
        }

        public IWorldInstance GetPlayerWorld(IWorldInstance parent)
        {
            if (parent.Entities.Any(x => x.PlayerControlled))
            {
                return parent;
            }

            foreach (IWorldInstance world in parent.Areas.Values)
            {
                return this.GetPlayerWorld(world);
            }

            return null;
        }

        public void SwapPosition(IEntity left, IEntity right)
        {
            Vector2Int tempPosition = right.WorldPosition;
            right.Move(left.WorldPosition);
            left.Move(tempPosition);
        }

        public IItemInstance PickUpObject(IEntity entityRef)
        {
            if (this.GetObject(entityRef.WorldPosition) is IItemInstance item)
            {
                List<string> tags = new List<string> {"pick up"};
                bool newOwner = true;
                if (item.OwnerGUID != default && item.OwnerGUID != entityRef.Guid)
                {
                    tags.Add("theft");
                    newOwner = false;
                }

                entityRef.FetchAction("additemaction")
                    .Execute(new IJoyObject[] {entityRef, item},
                        tags.ToArray(),
                        new Dictionary<string, object>
                        {
                            {"newOwner", newOwner}
                        });

                this.RemoveObject(entityRef.WorldPosition, item);

                return item;
            }

            return null;
        }

        public void AddEntity(IEntity entityRef)
        {
            this.m_Entities.Add(entityRef);
            this.m_EntityGUIDs.Add(entityRef.Guid);
            this.EntityHandler.Add(entityRef);

            //Initialise a new GameObject here at some point

            this.CalculatePlayerIndex();
            this.IsDirty = true;

            entityRef.MyWorld = this;
        }

        public void RemoveEntity(Vector2Int positionRef, bool destroy = false)
        {
            IEntity entity = this.m_Entities.FirstOrDefault(e => e.WorldPosition == positionRef);

            if (entity is null)
            {
                return;
            }

            this.m_Entities.Remove(entity);
            this.m_EntityGUIDs.Remove(entity.Guid);
            if (destroy)
            {
                this.EntityHandler.Destroy(entity.Guid);
            }

            this.CalculatePlayerIndex();

            //GlobalConstants.GameManager?.EntityPool.Retire(entity.MonoBehaviourHandler.gameObject);

            this.IsDirty = true;
        }

        public IEntity GetEntity(Vector2Int positionRef)
        {
            return this.m_Entities.FirstOrDefault(t => t.WorldPosition.Equals(positionRef));
        }

        public Sector GetSectorFromPoint(Vector2Int point)
        {
            int xCentreBegin, xRightBegin;
            int yCentreBegin, yBottomBegin;

            xCentreBegin = this.m_Tiles.GetLength(0) / 3;
            xRightBegin = xCentreBegin * 2;

            yCentreBegin = this.m_Tiles.GetLength(1) / 3;
            yBottomBegin = yCentreBegin * 2;

            int sectorX;
            int sectorY;

            if (point.x < xCentreBegin)
                sectorX = 0;
            else if (point.x < xRightBegin)
                sectorX = 1;
            else
                sectorX = 2;

            if (point.y < yCentreBegin)
                sectorY = 0;
            else if (point.y < yBottomBegin)
                sectorY = 1;
            else
                sectorY = 2;

            switch (sectorX)
            {
                case 0:
                    switch (sectorY)
                    {
                        case 0:
                            return Sector.NorthWest;

                        case 1:
                            return Sector.North;

                        case 2:
                            return Sector.NorthEast;
                    }

                    break;

                case 1:
                    switch (sectorY)
                    {
                        case 0:
                            return Sector.West;

                        case 1:
                            return Sector.Centre;

                        case 2:
                            return Sector.East;
                    }

                    break;

                case 2:
                    switch (sectorY)
                    {
                        case 0:
                            return Sector.SouthWest;

                        case 1:
                            return Sector.South;

                        case 2:
                            return Sector.SouthEast;
                    }

                    break;
            }

            return Sector.Centre;
        }

        public List<Vector2Int> GetVisibleWalls(IEntity viewer)
        {
            List<Vector2Int> visibleWalls = this.Walls
                .Where(wall => viewer.VisionProvider.CanSee(viewer, this, wall.Key))
                .ToDictionary(wall => wall.Key, wall => wall.Value).Keys.ToList();
            return visibleWalls;
        }

        public WorldTile[,] Tiles
        {
            get { return this.m_Tiles; }
        }

        public Dictionary<Vector2Int, IJoyObject> GetObjectsOfType(string[] tags)
        {
            Dictionary<Vector2Int, IJoyObject> objects = new Dictionary<Vector2Int, IJoyObject>();
            foreach (IJoyObject joyObject in this.m_Objects)
            {
                int matches = 0;
                foreach (string tag in tags)
                {
                    if (joyObject.HasTag(tag))
                    {
                        matches++;
                    }
                }

                if (matches == tags.Length || (tags.Length < joyObject.Tags.Count() && matches > 0))
                {
                    objects.Add(joyObject.WorldPosition, joyObject);
                }
            }

            return objects;
        }

        public void AddArea(Vector2Int key, IWorldInstance value)
        {
            value.Parent = this;
            this.m_Areas.Add(key, value);
        }

        public bool HasTag(string tag)
        {
            return this.m_Tags.Contains(tag.ToLower());
        }

        public bool AddTag(string tag)
        {
            if (this.m_Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            this.m_Tags.Add(tag);
            return true;
        }

        public bool RemoveTag(string tag)
        {
            if (this.Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)))
            {
                string match = this.Tags.First(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
                return this.m_Tags.Remove(match);
            }

            return false;
        }

        public IEnumerable<string> Tags
        {
            get => this.m_Tags;
            protected set => this.m_Tags = new List<string>(value);
        }

        public Dictionary<Vector2Int, IWorldInstance> Areas
        {
            get { return this.m_Areas; }
        }

        public HashSet<IJoyObject> Objects
        {
            get { return this.m_Objects; }
        }

        public Dictionary<Vector2Int, IJoyObject> Walls
        {
            get { return this.m_Walls; }
        }

        public Vector2Int SpawnPoint
        {
            get { return this.m_SpawnPoint; }
            set { this.m_SpawnPoint = value; }
        }

        public IWorldInstance Parent
        {
            get { return this.m_Parent; }
            set { this.m_Parent = value; }
        }

        public Guid Guid
        {
            get { return this.m_GUID; }
            protected set { this.m_GUID = value; }
        }

        public string Name
        {
            get { return this.m_Name; }
            protected set { this.m_Name = value; }
        }

        public IEntity Player
        {
            get;
            protected set;
        }

        public Vector2Int Dimensions
        {
            get
            {
                return this.m_Dimensions;
            }
        }

         
        public bool IsDirty { get; protected set; }

        public byte[,] Costs
        {
            get { return this.m_Costs; }
        }

        public void Dispose()
        {
            foreach (IEntity entity in this.m_Entities)
            {
                entity.Dispose();
            }
            this.m_Entities = null;

            foreach (IJoyObject joyObject in this.m_Objects)
            {
                joyObject.Dispose();
            }
            this.m_Objects = null;

            GarbageMan.Dispose(this.Walls);
            this.m_Walls = null;

            GarbageMan.Dispose(this.m_Areas);
            this.m_Areas = null;

            //GlobalConstants.GameManager.GUIDManager.ReleaseGUID(this.Guid);
        }

        ~WorldInstance()
        {
            this.Dispose();
        }
    }
}