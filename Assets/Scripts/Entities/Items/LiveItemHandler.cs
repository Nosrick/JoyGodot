using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Collections;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.World;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.Entities.Items
{
    public class LiveItemHandler : ILiveItemHandler
    {
        protected System.Collections.Generic.Dictionary<Guid, IItemInstance> LiveItems { get; set; }
        
        public JSONValueExtractor ValueExtractor { get; protected set; }
        
        protected RNG Roller { get; set; }

        public LiveItemHandler(RNG roller = null)
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.Roller = roller ?? new RNG();
            this.Load();
        }

        public IEnumerable<IItemInstance> Load()
        {
            this.LiveItems = new System.Collections.Generic.Dictionary<Guid, IItemInstance>();

            this.QuestRewards = new NonUniqueDictionary<Guid, Guid>();
            return new IItemInstance[0];
        }

        public bool Add(IItemInstance item)
        {
            if (this.LiveItems.ContainsKey(item.Guid))
            {
                return false;
            }

            this.LiveItems.Add(item.Guid, item);
            if (item.InWorld 
                && item.MyWorld is null == false 
                && item.MyWorld.Items.Any(o => o.Guid == item.Guid) == false)
            {
                item.MyWorld.AddItem(item);
            }
            return true;
        }

        public bool AddItems(IEnumerable<IItemInstance> items, bool addToWorld = false)
        {
            return items.Aggregate(true, (current, item) => current & this.Add(item));
        }

        public bool RemoveItemFromWorld(Guid GUID)
        {
            if (!this.LiveItems.ContainsKey(GUID))
            {
                return false;
            }
            
            IItemInstance item = this.Get(GUID);
            item.MyWorld?.RemoveObject(item.WorldPosition, item);
            return true;

        }

        public bool RemoveItemFromWorld(IItemInstance item)
        {
            if (!this.LiveItems.ContainsKey(item.Guid))
            {
                return false;
            }

            return item.MyWorld.RemoveObject(item.WorldPosition, item);
        }
        
        public bool Destroy(Guid key)
        {
            if (this.LiveItems.ContainsKey(key))
            {
                IItemInstance item = this.LiveItems[key];
                
                //Erase any targets that match this item
                //This is a really quick hack to fix a persistent problem
                //TODO: Find a better way to reference AI targets
                IEnumerable<IEntity> targeting =
                    GlobalConstants.GameManager.Player.MyWorld.Entities.Where(entity =>
                        entity.CurrentTarget.target == item);
                foreach (IEntity entity in targeting)
                {
                    entity.CurrentTarget.target = null;
                }
                
                //this.LiveItems[key].Dispose();
                this.LiveItems[key] = null;
                this.LiveItems.Remove(key);
                item = null;

                return true;
            }

            return false;
        }

        public bool AddItemToWorld(WorldInstance world, Guid GUID)
        {
            if (!this.LiveItems.ContainsKey(GUID))
            {
                return false;
            }
            
            IItemInstance item = this.Get(GUID);
            item.MyWorld = world;
            world.AddItem(item);
            return true;
        }

        public IItemInstance Get(Guid guid)
        {
            return this.LiveItems.TryGetValue(guid, out IItemInstance item) ? item : null;
        }

        public IEnumerable<IItemInstance> GetQuestRewards(Guid questID)
        {
            return this.QuestRewards.ContainsKey(questID) 
                ? this.GetItems(this.QuestRewards.FetchValuesForKey(questID)) 
                : new IItemInstance[0];
        }

        public void CleanUpRewards()
        {
            this.QuestRewards = new NonUniqueDictionary<Guid, Guid>(
                this.QuestRewards
                    .Where(tuple =>
                        GlobalConstants.GameManager.QuestTracker.AllQuests.Any(quest => quest.ID == tuple.Item1)));

            List<Guid> cleanup = this.QuestRewards.Where(tuple =>
                    GlobalConstants.GameManager.QuestTracker.AllQuests.Any(quest => quest.ID == tuple.Item1) == false)
                .Select(tuple => tuple.Item2)
                .ToList();
            foreach (var guid in cleanup)
            {
                this.Destroy(guid);
            }
        }

        public void AddQuestRewards(Guid questID, IEnumerable<IItemInstance> rewards)
        {
            this.AddItems(rewards);
            foreach (Guid reward in rewards.Select(r => r.Guid))
            {
                this.QuestRewards.Add(questID, reward);
            }
        }

        public IEnumerable<IItemInstance> GetItems(IEnumerable<Guid> guids)
        {
            List<IItemInstance> items = new List<IItemInstance>();

            foreach (Guid guid in guids)
            {
                if (this.LiveItems.TryGetValue(guid, out IItemInstance item))
                {
                    items.Add(item);
                }
            }

            return items;
        }

        public void ClearLiveItems()
        {
            this.LiveItems = new System.Collections.Generic.Dictionary<Guid, IItemInstance>();
        }

        ~LiveItemHandler()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            Guid[] keys = this.LiveItems.Keys.ToArray();
            foreach (Guid key in keys)
            {
                //this.LiveItems[key].Dispose();
                this.LiveItems[key] = null;
            }

            this.LiveItems = null;
        }

        public NonUniqueDictionary<Guid, Guid> QuestRewards
        {
            get;
            protected set;
        }

        public IEnumerable<IItemInstance> Values => this.LiveItems.Values.ToList();
        
        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"Items", new Array(this.LiveItems.Select(pair => pair.Value.Save()))}
            };
            
            return saveDict;
        }

        public void Load(Dictionary data)
        {
            this.LiveItems = new System.Collections.Generic.Dictionary<Guid, IItemInstance>();
            this.QuestRewards = new NonUniqueDictionary<Guid, Guid>();
            
            var items = this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(data, "Items");

            foreach (Dictionary itemDict in items)
            {
                IItemInstance item = new ItemInstance();
                item.Load(itemDict);
                this.Add(item);
            }
        }
    }
}
