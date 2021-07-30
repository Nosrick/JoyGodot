using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Entities.Needs
{
    public interface INeedHandler : IHandler<INeed, string>
    {
        ICollection<INeed> GetManyRandomised(IEnumerable<string> names);
        INeed GetRandomised(string name);
        IEnumerable<string> NeedNames { get; }
    }
}