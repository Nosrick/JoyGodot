using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;
using JoyLib.Code.Entities;
using Array = Godot.Collections.Array;

namespace JoyLib.Code.Quests
{
    [Serializable]
    public class ConcreteQuestStep : IQuestStep
    {
        protected string Description { get; set; }

        protected List<string> m_Tags;

        public IEnumerable<string> Tags
        {
            get => this.m_Tags;
            protected set => this.m_Tags = new List<string>(value);
        }
        
        public ConcreteQuestStep(
            IQuestAction action, 
            IEnumerable<Guid> objects, 
            IEnumerable<Guid> actors,
            IEnumerable<Guid> areas,
            IEnumerable<string> tags)
        {
            this.Action = action;
            this.Items = objects.ToList();
            this.Actors = actors.ToList();
            this.Areas = areas.ToList();
            this.Tags = new List<string>(tags);
        }

        public override string ToString()
        { 
            return this.Description ?? (this.Description = this.Action.AssembleDescription());
        }

        public IQuestAction Action
        {
            get;
            protected set;
        }

        public List<Guid> Items
        {
            get;
            protected set;
        }

        public List<Guid> Actors
        {
            get;
            protected set;
        }

        public List<Guid> Areas
        {
            get;
            protected set;
        }

        public void StartQuest(IEntity questor)
        {
            this.Action.ExecutePrerequisites(questor);
        }
        
        public bool AddTag(string tag)
        {
            if (this.m_Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)) != false)
            {
                return false;
            }
            
            this.m_Tags.Add(tag);
            return true;
        }

        public bool RemoveTag(string tag)
        {
            if (!this.m_Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            
            this.m_Tags.Remove(tag);
            return true;
        }

        public bool HasTag(string tag)
        {
            return this.Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary();

            Array array = new Array(this.Actors.Select(guid => guid.ToString()));
            saveDict.Add("Actors", array);

            array = new Array(this.Areas.Select(guid => guid.ToString()));
            saveDict.Add("Areas", array);

            array = new Array(this.Items.Select(guid => guid.ToString()));
            saveDict.Add("Items", array);

            saveDict.Add("Action", this.Action.Save());

            return saveDict;
        }

        public void Load(string data)
        {
            throw new NotImplementedException();
        }
    }
}
