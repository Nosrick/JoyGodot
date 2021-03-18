using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Rollers;
using JoyLib.Code.Scripting;
using JoyLib.Code.World;

namespace JoyLib.Code.Quests
{
    [Serializable]
    public class DeliverQuestAction : IQuestAction
    {
        
        public string[] Tags { get; protected set; }

        
        public string Description { get; protected set; }
        
        
        public List<Guid> Items { get; protected set; }
        
        public List<Guid> Actors { get; protected set; }
        
        public List<Guid> Areas { get; protected set; }

        
        protected IItemFactory ItemFactory { get; set; }

        
        public RNG Roller { get; protected set; }

        public DeliverQuestAction()
        {
        }
        
        public DeliverQuestAction(
            IEnumerable<IItemInstance> items,
            IEnumerable<IJoyObject> actors,
            IEnumerable<IWorldInstance> areas,
             IEnumerable<string> tags,
            IItemFactory itemFactory = null,
            RNG roller = null)
        {
            List<string> tempTags = new List<string>();
            tempTags.Add("deliver");
            tempTags.AddRange(tags);
            this.Items = items.Select(instance => instance.Guid).ToList();
            this.Actors = actors.Select(instance => instance.Guid).ToList();
            this.Areas = areas.Select(instance => instance.Guid).ToList();
            this.Tags = tempTags.ToArray();
            this.Description = this.AssembleDescription();

            this.Roller = roller is null ? new RNG() : roller;
            this.ItemFactory = itemFactory is null || GlobalConstants.GameManager is null == false ? GlobalConstants.GameManager.ItemFactory : itemFactory;
        }
        
        public IQuestStep Make(IEntity questor, IEntity provider, IWorldInstance overworld, IEnumerable<string> tags)
        {
            IItemInstance deliveryItem = null;
            List<IItemInstance> backpack = provider.Contents.ToList();
            if (backpack.Count > 0)
            {
                int result = this.Roller.Roll(0, backpack.Count);

                deliveryItem = backpack[result];
            }
            IEntity endPoint = overworld.GetRandomSentientWorldWide();
            if(deliveryItem == null)
            {
                deliveryItem = this.ItemFactory.CreateRandomWeightedItem();
            }
            deliveryItem.SetOwner(endPoint.Guid);

            this.Items = new List<Guid> {deliveryItem.Guid};
            this.Actors = new List<Guid> {endPoint.Guid};
            this.Areas = new List<Guid>();

            IQuestStep step = new ConcreteQuestStep(
                this, 
                this.Items, 
                this.Actors, 
                this.Areas,
                this.Tags);
            return step;
        }

        public void ExecutePrerequisites(IEntity questor)
        {
            GlobalConstants.ActionLog.Log("Adding the following items to " + questor + " inventory");
            GlobalConstants.ActionLog.Log(this.Items);
            questor.AddContents(GlobalConstants.GameManager.ItemHandler.GetItems(this.Items));
        }

        public bool ExecutedSuccessfully(IJoyAction action)
        {
            if (action.LastTags.Any(tag => tag.Equals("item", StringComparison.OrdinalIgnoreCase)) == false)
            {
                return false;
            }

            if (action.LastTags.Any(tag =>
                tag.Equals("trade", StringComparison.OrdinalIgnoreCase)
                || tag.Equals("give", StringComparison.OrdinalIgnoreCase)) == false)
            {
                return false;
            }

            if (action.LastParticipants.Select(o => o.Guid).Intersect(this.Actors).Count() != this.Actors.Count)
            {
                return false;
            }

            List<IItemInstance> items = new List<IItemInstance>();
            foreach (object obj in action.LastArgs)
            {
                if (obj is IEnumerable<IItemInstance> toAdd)
                {
                    items.AddRange(toAdd);
                }
                else if (obj is IItemInstance item)
                {
                    items.Add(item);
                }
            }

            return items.Select(instance => instance.Guid).Intersect(this.Items).Count() == this.Items.Count 
                   && action.Successful;
        }

        public string AssembleDescription()
        {
            StringBuilder itemBuilder = new StringBuilder();
            for (int i = 0; i < this.Items.Count; i++)
            {
                if (i > 0 && i < this.Items.Count - 1)
                {
                    itemBuilder.Append(", ");
                }
                if (this.Items.Count > 1 && i == this.Items.Count - 1)
                {
                    itemBuilder.Append("and ");
                }

                string name = GlobalConstants.GameManager.ItemHandler.Get(this.Items[i]).JoyName;
                itemBuilder.Append(name);
            }
            
            StringBuilder actorBuilder = new StringBuilder();
            for(int i = 0; i < this.Actors.Count; i++)
            {
                if (i > 0 && i < this.Actors.Count - 1)
                {
                    actorBuilder.Append(", ");
                }
                if (this.Actors.Count > 1 && i == this.Actors.Count - 1)
                {
                    actorBuilder.Append("or ");
                }

                IEntity entity = GlobalConstants.GameManager.EntityHandler.Get(this.Actors[i]);
                actorBuilder.Append(entity.JoyName);
                actorBuilder.Append(" in ");
                actorBuilder.Append(entity.MyWorld.Name);
            }

            return "Deliver " + itemBuilder.ToString() + " to " + actorBuilder.ToString() + ".";
        }

        public IQuestAction Create(IEnumerable<string> tags,
            IEnumerable<IItemInstance> items,
            IEnumerable<IJoyObject> actors,
            IEnumerable<IWorldInstance> areas,
            IItemFactory itemFactory = null)
        {
            return new DeliverQuestAction(
                items,
                actors,
                areas,
                tags, 
                itemFactory);
        }
    }
}