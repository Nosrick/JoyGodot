using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Cultures
{
    public interface ICultureHandler : IHandler<ICulture, string>
    {
        ICulture GetByCultureName(string name);
        List<ICulture> GetByCreatureType(string type);
        
        IEnumerable<ICulture> Cultures { get; }
    }
}