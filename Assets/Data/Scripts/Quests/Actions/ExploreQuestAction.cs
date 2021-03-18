using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Helpers;
using JoyLib.Code.Rollers;
using JoyLib.Code.Scripting;
using JoyLib.Code.World;

namespace JoyLib.Code.Quests
{
    [Serializable]
    public class ExploreQuestAction : IQuestAction
    {
        
        public string[] Tags { get; protected set; }
        
        public string Description { get; protected set; }
        
        
        public List<Guid> Items { get; protected set; }
        
        
        public List<Guid> Areas { get; protected set; }
        
        
        public List<Guid> Actors { get; protected set; }

        
        public RNG Roller { get; protected set; }

        public ExploreQuestAction()
        {}
        
        public ExploreQuestAction(
            IEnumerable<IItemInstance> items,
            IEnumerable<IJoyObject> actors,
            IEnumerable<IWorldInstance> areas,
            IEnumerable<string> tags,
            RNG roller = null)
        {
            this.Roller = roller is null ? new RNG() : roller; 
            List<string> tempTags = new List<string>();
            tempTags.Add("exploration");
            tempTags.AddRange(tags);
            this.Items = items.Select(instance => instance.Guid).ToList();
            this.Actors = actors.Select(instance => instance.Guid).ToList();
            this.Areas = areas.Select(instance => instance.Guid).ToList();
            this.Tags = tempTags.ToArray();
            this.Description = this.AssembleDescription();
        }
        
        public IQuestStep Make(IEntity questor, IEntity provider, IWorldInstance overworld, IEnumerable<string> tags)
        {
            List<IWorldInstance> worlds = overworld.GetWorlds(overworld);

            worlds = worlds.Where(instance => questor.HasDataKey(instance.Name) == false).ToList();
            if (worlds.Any() == false)
            {
                GlobalConstants.ActionLog.Log(questor + " has explored the whole world!", LogLevel.Warning);
                worlds = overworld.GetWorlds(overworld);
            }
            
            int result = this.Roller.Roll(0, worlds.Count);

            this.Items = new List<Guid>();
            this.Actors = new List<Guid>();
            this.Areas = new List<Guid> { worlds[result].Guid };

            IQuestStep step = new ConcreteQuestStep(
                this, 
                this.Items, 
                this.Actors, 
                this.Areas,
                this.Tags);
            return step;
        }

        public bool ExecutedSuccessfully(IJoyAction action)
        {
            if (action.Name.Equals("enterworldaction", StringComparison.OrdinalIgnoreCase) == false)
            {
                return false;
            }

            foreach (object obj in action.LastArgs)
            {
                if (obj is IWorldInstance world)
                {
                    if (this.Areas.Contains(world.Guid) == false)
                    {
                        return false;
                    }
                }
            }

            IWorldInstance overworld = GlobalConstants.GameManager.Player.MyWorld.GetOverworld();
            List<IWorldInstance> worlds = overworld.GetWorlds(overworld)
                .Where(instance => this.Areas.Contains(instance.Guid))
                .ToList();
            return worlds.All(world => action.LastParticipants.First().HasDataKey(world.Name)) && action.Successful;
        }

        public string AssembleDescription()
        {
            StringBuilder builder = new StringBuilder();

            IWorldInstance overworld = GlobalConstants.GameManager.Player.MyWorld.GetOverworld();
            List<IWorldInstance> worlds = overworld.GetWorlds(overworld)
                .Where(instance => this.Areas.Contains(instance.Guid))
                .ToList();

            for(int i = 0; i < this.Areas.Count; i++)
            {
                if (i > 0 && i < this.Items.Count - 1)
                {
                    builder.Append(", ");
                }
                if (this.Items.Count > 1 && i == this.Items.Count - 1)
                {
                    builder.Append("and ");
                }
                builder.Append(worlds[i].Name);
            }
            
            return "Go to " + builder + ".";
        }

        public void ExecutePrerequisites(IEntity questor)
        {
        }

        public IQuestAction Create(IEnumerable<string> tags,
            IEnumerable<IItemInstance> items,
            IEnumerable<IJoyObject> actors,
            IEnumerable<IWorldInstance> areas,
            IItemFactory itemFactory = null)
        {
            return new ExploreQuestAction(
                items,
                actors,
                areas,
                tags);
        }
    }
}