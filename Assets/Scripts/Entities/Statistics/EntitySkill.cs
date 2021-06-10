using System;
using Godot.Collections;

namespace JoyLib.Code.Entities.Statistics
{
    [Serializable]
    public class EntitySkill : IEntitySkill
    {
        public EntitySkill()
        {
        }
        
        public EntitySkill(
            string name, 
            int value, 
            int successThreshold)
        {
            this.Name = name;
            this.Value = value;
            this.SuccessThreshold = successThreshold;
        }

        public int ModifyValue(int value)
        {
            this.Value = Math.Max(0, this.Value + value);
            return this.Value;
        }

        public int SetValue(int value)
        {
            this.Value = Math.Max(0, value);
            return this.Value;
        }

        public string Name
        {
            get;
            protected set;
        }

        public int Value
        {
            get;
            protected set;
        }

        public int SuccessThreshold
        {
            get;
            protected set;
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary();
            
            saveDict.Add("Name", this.Name);
            saveDict.Add("Value", this.Value);
            saveDict.Add("SuccessThreshold", this.SuccessThreshold);

            return saveDict;
        }

        public void Load(Dictionary data)
        {
            throw new NotImplementedException();
        }
    }
}