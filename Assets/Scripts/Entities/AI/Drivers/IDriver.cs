namespace JoyLib.Code.Entities.AI.Drivers
{
    public interface IDriver
    {
        bool PlayerControlled { get; }
        
        void Locomotion(IEntity vehicle);

        void Interact();
    }
}