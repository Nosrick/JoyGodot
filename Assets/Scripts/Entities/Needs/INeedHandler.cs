using System.Collections.Generic;

namespace JoyLib.Code.Entities.Needs
{
    public interface INeedHandler : IHandler<INeed, string>
    {
        ICollection<INeed> GetMany(IEnumerable<string> names);
        ICollection<INeed> GetManyRandomised(IEnumerable<string> names);
        INeed GetRandomised(string name);
        IEnumerable<string> NeedNames { get; }
    }
}