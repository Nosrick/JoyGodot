using System.Collections.Generic;

namespace JoyLib.Code.Entities.Sexes.Processors
{
    public interface IBioSexProcessor
    {
        string Name { get; }
        IEntity CreateChild(IEnumerable<IEntity> parents);
    }
}