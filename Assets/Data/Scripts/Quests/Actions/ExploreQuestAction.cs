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
    public class ExploreQuestAction : AbstractQuestAction
    {
        public ExploreQuestAction()
        {
            this.Roller = new RNG();
            this.Items = new List<Guid>();
            this.Actors = new List<Guid>();
            this.Areas = new List<Guid>();
            this.Tags = new string[0];
            this.Description = string.Empty;
        }

        public ExploreQuestAction(
            IEnumerable<Guid> items,
            IEnumerable<Guid> actors,
            IEnumerable<Guid> areas,
            IEnumerable<string> tags,
            RNG roller = null)
        {
            this.Roller = roller ?? new RNG(); 
            List<string> tempTags = new List<string>();
            tempTags.Add("exploration");
            tempTags.AddRange(tags);
            this.Items = items.ToList();
            this.Actors = actors.ToList();
            this.Areas = areas.ToList();
            this.Tags = tempTags.ToArray();
            this.Description = this.AssembleDescription();
        }

        public override bool ExecutedSuccessfully(IJoyAction action)
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

        public override string AssembleDescription()
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

        public override void ExecutePrerequisites(IEntity questor)
        {
        }

        public override IQuestAction Create(
            IEntity questor, 
            IEntity provider, 
            IWorldInstance overworld,
            IEnumerable<string> tags)
        {
            List<IWorldInstance> worlds = overworld.GetWorlds(overworld);

            List<string> myTags = new List<string>(tags);

            worlds = worlds.Where(instance => questor.HasDataKey(instance.Name) == false).ToList();
            if (worlds.Any() == false)
            {
                GlobalConstants.ActionLog.Log(questor + " has explored the whole world!", LogLevel.Warning);
                worlds = overworld.GetWorlds(overworld);
            }
            
            int result = this.Roller.Roll(0, worlds.Count);

            return new ExploreQuestAction(
                new List<Guid>(),
                new List<Guid>(),
                new List<Guid> { worlds[result].Guid },
                myTags);
        }
    }
}