using System.Collections.Generic;

namespace JoyLib.Code.Entities.Sexes.Processors
{
    public class MaleProcessor : IBioSexProcessor
    {
        public string Name => "male";
        
        public IEntity CreateChild(IEnumerable<IEntity> parents)
        {
            throw new System.NotImplementedException();
        }
    }
}