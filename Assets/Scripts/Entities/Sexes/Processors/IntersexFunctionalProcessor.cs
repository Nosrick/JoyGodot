using System.Collections.Generic;

namespace JoyLib.Code.Entities.Sexes.Processors
{
    public class IntersexFunctionalProcessor : IBioSexProcessor
    {
        public string Name => "intersexfunctional";
        public IEntity CreateChild(IEnumerable<IEntity> parents)
        {
            throw new System.NotImplementedException();
        }
    }
}