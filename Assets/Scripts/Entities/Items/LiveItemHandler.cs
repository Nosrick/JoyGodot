using System;
using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Collections;
using JoyLib.Code.Rollers;
using JoyLib.Code.World;

namespace JoyLib.Code.Entities.Items
{
    public class LiveItemHandler : ILiveItemHandler
    {
        protected Dictionary<Guid, IItemInstance> LiveItems { get; set; }
        
        protected RNG Roller { get; set; }

        public LiveItemHandler(RNG roller = null)
        {
            this.Roller = roller ?? new RNG();
            this.Load();
        }

        public IEnumerable<IItemInstance> Load()
        {
            this.LiveItems = new Dictionary<Guid, IItemInstance>();

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
                && item.MyWorld.Objects.Any(o => o.Guid == item.Guid) == false)
            {
                item.MyWorld.AddObject(item);
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
            //LiveItems.Remove(GUID);
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
                
                /*
                //Erase any targets that match this item
                //This is a really quick hack to fix a persistent problem
                //TODO: Find a better way to reference AI targets
                IEnumerable<IEntity> targeting =
                    GlobalConstants.GameManager.Player.MyWorld.Entities.Where(entity =>
                        entity.CurrentTarget.target == item);
                targeting.ForEach(entity => entity.CurrentTarget.target = null);
                */
                
                this.LiveItems[key].Dispose();
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
            world.AddObject(item);
            return true;
        }

        public IItemInstance Get(Guid guid)
        {
            if (this.LiveItems.ContainsKey(guid))
            {
                return this.LiveItems[guid];
            }
            throw new InvalidOperationException("No item found with GUID " + guid);
        }

        public IEnumerable<IItemInstance> GetQuestRewards(Guid questID)
        {
            return this.QuestRewards.ContainsKey(questID) 
                ? this.GetItems(this.QuestRewards.FetchValuesForKey(questID)) 
                : new IItemInstance[0];
        }

        public void CleanUpRewards()
        {
            /*
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
                IItemInstance item = this.Get(guid);
                if (item.MonoBehaviourHandler is null)
                {
                    GlobalConstants.ActionLog.AddText("No MBH found on " + item);
                }
                item.Dispose();
            }
            */
        }

        public void AddQuestReward(Guid questID, Guid reward)
        {
            this.QuestRewards.Add(questID, reward);
        }

        public void AddQuestRewards(Guid questID, IEnumerable<Guid> rewards)
        {
            foreach (Guid reward in rewards)
            {
                this.QuestRewards.Add(questID, reward);
            }
        }

        public void AddQuestRewards(Guid questID, IEnumerable<IItemInstance> rewards)
        {
            this.AddQuestRewards(questID, rewards.Select(instance => instance.Guid));
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
            this.LiveItems = new Dictionary<Guid, IItemInstance>();
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
                this.LiveItems[key].Dispose();
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
    }
}
