using System.Text;

namespace JoyLib.Code.Cultures
{
    public struct NameData
    {
        public string name;
        public int[] chain;
        public string[] genders;
        public int[] groups;

        public NameData(string nameRef, int[] chainRef, string[] gendersRef, int[] groupsRef)
        {
            this.name = nameRef;
            this.chain = chainRef;
            this.genders = gendersRef;
            this.groups = groupsRef;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Name: " + this.name);
            builder.AppendLine("Chain: " + GlobalConstants.ActionLog.CollectionWalk(this.chain));
            builder.AppendLine("Genders: " + GlobalConstants.ActionLog.CollectionWalk(this.genders));
            builder.AppendLine("Groups: " + GlobalConstants.ActionLog.CollectionWalk(this.groups));
            return builder.ToString();
        }
    }
}