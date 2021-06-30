using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Entities
{
    public interface IEntityTemplateHandler: IHandler<IEntityTemplate, string>
    {
        IEntityTemplate GetRandom();
    }
}