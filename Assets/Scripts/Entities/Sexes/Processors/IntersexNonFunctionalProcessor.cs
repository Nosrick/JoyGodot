using System.Collections.Generic;

namespace JoyGodot.Assets.Scripts.Entities.Sexes.Processors
{
    public class IntersexNonFunctionalProcessor : IBioSexProcessor
    {
        public string Name => "intersexnonfunctional";
        public IEntity CreateChild(IEnumerable<IEntity> parents)
        {
            throw new System.NotImplementedException();
        }
    }
}