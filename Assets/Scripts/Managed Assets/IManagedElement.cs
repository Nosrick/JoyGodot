namespace JoyGodot.Assets.Scripts.Managed_Assets
{
    public interface IManagedElement
    {
        string ElementName { get; }
        bool Initialised { get; }

        void Initialise();
    }
}