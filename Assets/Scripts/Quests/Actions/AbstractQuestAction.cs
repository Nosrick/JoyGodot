using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Rollers;
using JoyLib.Code.Scripting;
using JoyLib.Code.World;
using Array = Godot.Collections.Array;

namespace JoyLib.Code.Quests
{
    public abstract class AbstractQuestAction : IQuestAction
    {
        public string[] Tags { get; protected set; }
        
        public string Description { get; protected set; }
        
        public List<Guid> Items { get; protected set; }
        
        public List<Guid> Actors { get; protected set; }
        
        public List<Guid> Areas { get; protected set; }
        
        public RNG Roller { get; protected set; }

        public abstract IQuestStep Make(
            IEntity questor, 
            IEntity provider, 
            IWorldInstance overworld,
            IEnumerable<string> tags);

        public abstract bool ExecutedSuccessfully(IJoyAction action);

        public abstract string AssembleDescription();

        public abstract void ExecutePrerequisites(IEntity questor);

        public abstract IQuestAction Create(
            IEnumerable<string> tags, 
            IEnumerable<IItemInstance> items,
            IEnumerable<IJoyObject> actors, 
            IEnumerable<IWorldInstance> areas,
            IItemFactory itemFactory = null);

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary();
            
            saveDict.Add("Tags", new Array(this.Tags));
            saveDict.Add("Description", this.AssembleDescription());
            saveDict.Add("Items", new Array(this.Items.Select(guid => guid.ToString())));
            saveDict.Add("Actors", new Array(this.Actors.Select(guid => guid.ToString())));
            saveDict.Add("Areas", new Array(this.Areas.Select(guid => guid.ToString())));

            return saveDict;
        }

        public void Load(Dictionary data)
        {
            throw new NotImplementedException();
        }
    }
}