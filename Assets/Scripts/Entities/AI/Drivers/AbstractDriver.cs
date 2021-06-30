namespace JoyGodot.Assets.Scripts.Entities.AI.Drivers
{
    public abstract class AbstractDriver : IDriver
    {
        public virtual bool PlayerControlled => false;

        public abstract void Interact();

        public abstract void Locomotion(IEntity vehicle);
    }
}