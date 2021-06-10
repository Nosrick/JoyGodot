using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;
using JoyLib.Code.Events;
using Array = Godot.Collections.Array;

namespace JoyLib.Code.Entities.Items
{
    [Serializable]
    public class EquipmentStorage : IItemContainer, ISerialisationHandler
    {
        protected List<Tuple<string, Guid>> m_Slots;

        public virtual string ContentString { get; }
        public virtual event ItemRemovedEventHandler ItemRemoved;
        public virtual event ItemAddedEventHandler ItemAdded;

        public IEnumerable<IItemInstance> Contents =>
            this.GetSlotsAndContents(false)
                .Select(tuple => tuple.Item2)
                .Distinct();

        public EquipmentStorage()
        {
            this.m_Slots = new List<Tuple<string, Guid>>();
        }

        public EquipmentStorage(IEnumerable<string> slots)
        {
            this.m_Slots = new List<Tuple<string, Guid>>();
            foreach (string slot in slots)
            {
                this.m_Slots.Add(new Tuple<string, Guid>(slot, Guid.Empty));
            }
        }
        
        protected virtual IEnumerable<string> GetRequiredSlots(IItemInstance item)
        {
            List<string> slots = new List<string>();
            if (item == null)
            {
                return slots;
            }

            System.Collections.Generic.Dictionary<string, int> requiredSlots = new System.Collections.Generic.Dictionary<string, int>();

            foreach (string slot in item.ItemType.Slots)
            {
                if (requiredSlots.ContainsKey(slot))
                {
                    requiredSlots[slot] += 1;
                }
                else
                {
                    requiredSlots.Add(slot, 1);
                }
            }

            System.Collections.Generic.Dictionary<string, int> copySlots = new System.Collections.Generic.Dictionary<string, int>(requiredSlots);

            foreach (Tuple<string, Guid> tuple in this.m_Slots)
            {
                foreach (KeyValuePair<string, int> pair in requiredSlots)
                {
                    if (pair.Key.Equals(tuple.Item1, StringComparison.OrdinalIgnoreCase)
                        && tuple.Item2 == Guid.Empty
                        && copySlots[pair.Key] > 0)
                    {
                        copySlots[pair.Key] -= 1;
                        slots.Add(tuple.Item1);
                    }
                }
            }

            return slots;
        }
        
        public virtual bool Contains(IItemInstance actor)
        {
            return this.m_Slots.Any(tuple => actor.Guid.Equals(tuple.Item2));
        }

        public virtual IItemInstance GetSlotContents(string slot)
        {
            Tuple<string, Guid> slotTuple =
                this.m_Slots.FirstOrDefault(tuple => tuple.Item1.Equals(slot, StringComparison.OrdinalIgnoreCase));
            if (slotTuple is null == false && slotTuple.Item2 != Guid.Empty)
            {
                //return GlobalConstants.GameManager.ItemHandler.Get(slotTuple.Item2);
            }

            return null;
        }

        public virtual IEnumerable<Tuple<string, IItemInstance>> GetSlotsAndContents(bool getEmpty = true)
        {
            if (getEmpty == false)
            {
                return this.m_Slots
                    .Where(tuple => tuple.Item2 != Guid.Empty)
                    .Select(tuple =>
                        new Tuple<string, IItemInstance>(tuple.Item1,
                            GlobalConstants.GameManager.ItemHandler.Get(tuple.Item2)));
            }

            return this.m_Slots
                .Select(tuple => new Tuple<string, IItemInstance>(
                    tuple.Item1,
                    tuple.Item2 != Guid.Empty ? GlobalConstants.GameManager.ItemHandler.Get(tuple.Item2) : null));
        }

        public virtual bool CanAddContents(IItemInstance actor)
        {
            int slots = this.GetRequiredSlots(actor).Count();
            return !this.Contains(actor)
                   && slots > 0
                   && slots == actor.ItemType.Slots.Count();
        }

        public virtual bool AddContents(IItemInstance actor)
        {
            if (!this.CanAddContents(actor))
            {
                return false;
            }

            IEnumerable<string> slots = this.GetRequiredSlots(actor);

            return this.AddContents(actor, slots);
        }

        public virtual bool AddContents(IItemInstance actor, IEnumerable<string> slots)
        {
            if (!this.CanAddContents(actor))
            {
                return false;
            }

            List<string> openSlots = this.GetRequiredSlots(actor).ToList();
            List<string> slotList = slots.ToList();
            int matches = 0;
            foreach (string slot in slotList)
            {
                int index = openSlots.FindIndex(s => s.Equals(slot, StringComparison.OrdinalIgnoreCase));
                if (index > -1)
                {
                    openSlots.RemoveAt(index);
                    matches += 1;
                }

                if (matches == slotList.Count)
                {
                    break;
                }
            }

            if (matches != slotList.Count)
            {
                return false;
            }

            foreach (string slot in slotList)
            {
                int index = this.m_Slots.FindIndex(
                    s => s.Item1.Equals(slot, StringComparison.InvariantCulture)
                         && s.Item2 == Guid.Empty);
                this.m_Slots[index] = new Tuple<string, Guid>(slot, actor.Guid);
            }
            
            this.ItemAdded?.Invoke(this, new ItemChangedEventArgs() { Item = actor });
            return true;
        }

        public virtual bool AddContents(IEnumerable<IItemInstance> actors)
        {
            bool result = actors.Aggregate(true, (current, actor) => current & this.AddContents(actor));

            return result;
        }

        public virtual bool RemoveContents(IItemInstance actor)
        {
            if (this.m_Slots.All(s => actor.Guid.Equals(s.Item2) == false))
            {
                return false;
            }
            List<Tuple<string, Guid>> slots = this.m_Slots.Where(s => actor.Guid.Equals(s.Item2)).ToList();
            if (slots.Any() == false)
            {
                return false;
            }

            foreach (Tuple<string, Guid> slot in slots)
            {
                int index = this.m_Slots.IndexOf(slot);
                this.m_Slots[index] = new Tuple<string, Guid>(slot.Item1, Guid.Empty);
            }
            this.ItemRemoved?.Invoke(this, new ItemChangedEventArgs { Item = actor });

            return true;
        }
        
        public virtual bool RemoveContents(IEnumerable<IItemInstance> actors)
        {
            return actors.Aggregate(true, (current, actor) => current & this.RemoveContents(actor));
        }

        public virtual void Clear()
        {
            List<IItemInstance> copy = new List<IItemInstance>(this.Contents);
            foreach (IItemInstance item in copy)
            {
                this.RemoveContents(item);
            }
        }

        public virtual int AddSlot(string slot)
        {
            this.m_Slots.Add(new Tuple<string, Guid>(slot, Guid.Empty));

            return this.m_Slots.Count(s => s.Item1.Equals(slot, StringComparison.OrdinalIgnoreCase));
        }
        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary();

            Array slotArray = new Array();

            foreach ((string slot, Guid item) in this.m_Slots)
            {
                slotArray.Add(new Dictionary
                {
                    {slot, item.ToString()}
                });
            }

            saveDict.Add("Slots", slotArray);

            return saveDict;
        }

        public void Load(string data)
        {
            throw new NotImplementedException();
        }
    }
}