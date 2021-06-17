using System.Collections.Generic;

namespace JoyGodot.Assets.Scripts.Entities.Sexes.Processors
{
    public interface IBioSexProcessor
    {
        string Name { get; }
        IEntity CreateChild(IEnumerable<IEntity> parents);
    }
}