using System.Collections.Generic;

namespace JoyLib.Code.Cultures
{
    public interface ICultureHandler : IHandler<ICulture, string>
    {
        ICulture GetByCultureName(string name);
        List<ICulture> GetByCreatureType(string type);
        
        IEnumerable<ICulture> Cultures { get; }
    }
}