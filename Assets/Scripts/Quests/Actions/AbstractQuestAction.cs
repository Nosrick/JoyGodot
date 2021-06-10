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

        public abstract bool ExecutedSuccessfully(IJoyAction action);

        public abstract string AssembleDescription();

        public abstract void ExecutePrerequisites(IEntity questor);

        public abstract IQuestAction Create(IEntity questor, IEntity provider, IWorldInstance overworld,
            IEnumerable<string> tags);

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"Tags", new Array(this.Tags)},
                {"Description", this.AssembleDescription()},
                {"Items", new Array(this.Items.Select(guid => guid.ToString()))},
                {"Actors", new Array(this.Actors.Select(guid => guid.ToString()))},
                {"Areas", new Array(this.Areas.Select(guid => guid.ToString()))},
                {"Type", this.GetType().Name}
            };
            
            return saveDict;
        }

        public void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.ItemHandler.ValueExtractor;

            this.Tags = valueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Tags").ToArray();
            this.Description = valueExtractor.GetValueFromDictionary<string>(data, "Description");
            this.Items = valueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Items")
                .Select(s => new Guid(s))
                .ToList();
            this.Actors = valueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Actors")
                .Select(s => new Guid(s))
                .ToList();
            this.Areas = valueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Areas")
                .Select(s => new Guid(s))
                .ToList();
        }
    }
}