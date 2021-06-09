using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.AI;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Events;
using JoyLib.Code.Physics;
using JoyLib.Code.Rollers;
using JoyLib.Code.World.Lighting;
using Array = Godot.Collections.Array;

namespace JoyLib.Code.World
{
    [Serializable]
    public class WorldInstance : 
        IWorldInstance
    {
        public event EmptyEventHandler OnTick;
        
        protected WorldTile[,] m_Tiles;
        protected byte[,] m_Costs;

         
        protected Vector2Int m_Dimensions;

         
        //Worlds and where to access them
        protected System.Collections.Generic.Dictionary<Vector2Int, IWorldInstance> m_Areas;
        
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

        protected HashSet<IItemInstance> m_Items;
        
         
        protected HashSet<Vector2Int> m_Walls;

         
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
            this.m_Areas = new System.Collections.Generic.Dictionary<Vector2Int, IWorldInstance>();
            this.m_Entities = new HashSet<IEntity>();
            this.m_EntityGUIDs = new HashSet<Guid>();
            this.m_Items = new HashSet<IItemInstance>();
            this.m_ItemGUIDs = new HashSet<Guid>();
            this.m_Walls = new HashSet<Vector2Int>();
            this.Guid = GlobalConstants.GameManager.GUIDManager.AssignGUID();

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
            
            this.m_Items = new HashSet<IItemInstance>();
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
            List<IItemInstance> objects = this.m_Items.Where(o => o is IItemInstance item && item.ItemType.LightLevel > 0)
                .Cast<IItemInstance>()
                .ToList();

            objects.AddRange(this.m_Entities.SelectMany(entity =>
                entity.Contents.Where(instance => instance.ItemType.LightLevel > 0)));

            this.LightCalculator.Do(objects, this, this.Dimensions, this.Walls);
        }

        public void AddItem(IItemInstance objectRef)
        {
            this.m_Items.Add(objectRef);
            this.m_ItemGUIDs.Add(objectRef.Guid);
            objectRef.InWorld = true;
            objectRef.MyWorld = this;

            this.IsDirty = true;
        }

        public void AddWall(Vector2Int wallRef)
        {
            this.m_Walls.Add(wallRef);
            this.m_Costs[wallRef.x, wallRef.y] = byte.MaxValue;
        }

        public bool RemoveObject(Vector2Int positionRef, IItemInstance itemRef)
        {
            bool removed = false;

            if (this.m_Items.Any(o => o.WorldPosition.Equals(positionRef) && itemRef.Guid.Equals(o.Guid)) == false)
            {
                return false;
            }

            removed = this.m_Items.Remove(itemRef) & this.m_ItemGUIDs.Remove(itemRef.Guid);

            if (removed)
            {
                this.IsDirty = true;

                itemRef.InWorld = false;
                itemRef.MyWorld = null;

                GlobalConstants.GameManager.ItemPool.Retire(itemRef.MyNode);
            }

            return removed;
        }

        public IJoyObject GetObject(Vector2Int worldPosition)
        {
            return this.m_Items.FirstOrDefault(o => o.WorldPosition == worldPosition);
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

            this.OnTick?.Invoke();

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
        public IEnumerable<IItemInstance> SearchForObjects(IEntity entityRef, IEnumerable<string> tags)
        {
            List<IItemInstance> data = new List<IItemInstance>();

            if (entityRef.VisionProvider.Vision.Any() == false)
            {
                return data;
            }

            List<IItemInstance> inSight = this.m_Items
                .Where(obj => entityRef.VisionProvider.CanSee(entityRef, this, obj.WorldPosition) == true).ToList();
            foreach (IItemInstance obj in inSight)
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

                IEnumerable<Tuple<string, object>> data = entity.GetData(searchCriteria.ToArray());
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
                        new System.Collections.Generic.Dictionary<string, object>
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

            this.OnTick -= entityRef.Tick;
            this.OnTick += entityRef.Tick;

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

            this.OnTick -= entity.Tick;

            this.m_Entities.Remove(entity);
            this.m_EntityGUIDs.Remove(entity.Guid);
            if (destroy)
            {
                this.EntityHandler.Destroy(entity.Guid);
            }

            this.CalculatePlayerIndex();

            GlobalConstants.GameManager?.EntityPool.Retire(entity.MyNode);

            this.IsDirty = true;
        }

        public IEntity GetEntity(Vector2Int positionRef)
        {
            return this.m_Entities.FirstOrDefault(t => t.WorldPosition.Equals(positionRef));
        }

        public PhysicsResult IsObjectAt(Vector2Int worldPosition)
        {
            if (this.Items.Any(o => o.WorldPosition == worldPosition))
            {
                return PhysicsResult.ObjectCollision;
            }

            if (this.Entities.Any(entity => entity.WorldPosition == worldPosition))
            {
                return PhysicsResult.EntityCollision;
            }

            if (this.Walls.Contains(worldPosition))
            {
                return PhysicsResult.WallCollision;
            }

            return PhysicsResult.None;
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
                .Where(wall => viewer.VisionProvider.HasVisibility(viewer, this, wall))
                .ToList();
            return visibleWalls;
        }

        public WorldTile[,] Tiles
        {
            get { return this.m_Tiles; }
        }

        public System.Collections.Generic.Dictionary<Vector2Int, IJoyObject> GetObjectsOfType(string[] tags)
        {
            System.Collections.Generic.Dictionary<Vector2Int, IJoyObject> objects = new System.Collections.Generic.Dictionary<Vector2Int, IJoyObject>();
            foreach (IJoyObject joyObject in this.m_Items)
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

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary();
            
            saveDict.Add("Name", this.Name);
            saveDict.Add("Dimensions", this.Dimensions.Save());

            Array array = new Array();

            foreach (ISerialisationHandler s in this.Entities)
            {
                array.Add(s.Save());
            }
            saveDict.Add("Entities", array);

            array = new Array();
            foreach (ISerialisationHandler s in this.Items)
            {
                array.Add(s.Save());
            }
            saveDict.Add("Items", array);

            array = new Array();
            foreach (ISerialisationHandler s in this.Walls)
            {
                array.Add(s.Save());
            }
            saveDict.Add("Walls", array);

            array = new Array();
            foreach (ISerialisationHandler s in this.Tiles)
            {
                array.Add(s.Save());
            }
            saveDict.Add("Tiles", array);

            Dictionary tempDict = new Dictionary();
            foreach (KeyValuePair<Vector2Int, IWorldInstance> pair in this.Areas)
            {
                tempDict.Add("EntryPoint", pair.Key.Save());
                tempDict.Add("World", pair.Value.Save());
            }
            saveDict.Add("Areas", tempDict);
            
            saveDict.Add("Guid", this.Guid.ToString());

            return saveDict;
        }

        public void Load(string data)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> Tags
        {
            get => this.m_Tags;
            protected set => this.m_Tags = new List<string>(value);
        }

        public System.Collections.Generic.Dictionary<Vector2Int, IWorldInstance> Areas
        {
            get { return this.m_Areas; }
        }

        public HashSet<IItemInstance> Items
        {
            get { return this.m_Items; }
        }

        public HashSet<Vector2Int> Walls
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
            /*
            foreach (IEntity entity in this.m_Entities)
            {
                //entity.Dispose();
            }
            this.m_Entities = null;

            foreach (IJoyObject joyObject in this.m_Objects)
            {
                //joyObject.Dispose();
            }
            this.m_Objects = null;

            GarbageMan.Dispose(this.Walls);
            this.m_Walls = null;

            GarbageMan.Dispose(this.m_Areas);
            this.m_Areas = null;

            //GlobalConstants.GameManager.GUIDManager.ReleaseGUID(this.Guid);
            */
        }

        ~WorldInstance()
        {
            this.Dispose();
        }
    }
}