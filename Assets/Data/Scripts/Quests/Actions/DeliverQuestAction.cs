using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.Quests.Actions;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.Scripting;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Data.Scripts.Quests.Actions
{
    public class DeliverQuestAction : AbstractQuestAction
    {
        protected IItemFactory ItemFactory { get; set; }
        
        public RNG Roller { get; protected set; }

        public DeliverQuestAction()
        {
        }
        
        public DeliverQuestAction(
            IEnumerable<Guid> items,
            IEnumerable<Guid> actors,
            IEnumerable<Guid> areas,
             IEnumerable<string> tags,
            IItemFactory itemFactory = null,
            RNG roller = null)
        {
            List<string> tempTags = new List<string>();
            tempTags.Add("deliver");
            tempTags.AddRange(tags);
            this.Items = items.ToList();
            this.Actors = actors.ToList();
            this.Areas = areas.ToList();
            this.Tags = tempTags.ToArray();
            this.Description = this.AssembleDescription();

            this.Roller = roller is null ? new RNG() : roller;
            this.ItemFactory = itemFactory is null || GlobalConstants.GameManager is null == false ? GlobalConstants.GameManager.ItemFactory : itemFactory;
        }

        public override void ExecutePrerequisites(IEntity questor)
        {
            GlobalConstants.ActionLog.Log("Adding the following items to " + questor + " inventory");
            GlobalConstants.ActionLog.Log(this.Items);
            questor.AddContents(GlobalConstants.GameManager.ItemHandler.GetItems(this.Items));
        }

        public override bool ExecutedSuccessfully(IJoyAction action)
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

        public override string AssembleDescription()
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

        public override IQuestAction Create(
            IEntity questor, 
            IEntity provider, 
            IWorldInstance overworld,
            IEnumerable<string> tags)
        {
            this.ItemFactory ??= GlobalConstants.GameManager.ItemFactory;
            
            IItemInstance deliveryItem = null;
            List<IItemInstance> backpack = provider.Contents.ToList();
            if (backpack.Count > 0)
            {
                int result = this.Roller.Roll(0, backpack.Count);

                deliveryItem = backpack[result];
            }
            IEntity endPoint = overworld.GetRandomSentientWorldWide();
            deliveryItem ??= this.ItemFactory.CreateRandomWeightedItem();
            deliveryItem.SetOwner(endPoint.Guid);

            List<string> myTags = new List<string>(tags);
            
            return new DeliverQuestAction(
                new List<Guid> {deliveryItem.Guid},
                new List<Guid> {endPoint.Guid},
                new List<Guid>(),
                myTags);
        }
    }
}