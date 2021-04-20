namespace JoyLib.Code.Entities
{
    public interface IEntityTemplateHandler: IHandler<IEntityTemplate, string>
    {
        IEntityTemplate GetRandom();
    }
}