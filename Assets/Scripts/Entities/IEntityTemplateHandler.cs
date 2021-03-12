using System.Collections.Generic;

namespace JoyLib.Code.Entities
{
    public interface IEntityTemplateHandler: IHandler<IEntityTemplate, string>
    {
        IEntityTemplate Get(string type);
        IEntityTemplate GetRandom();
        
    }
}