using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Entities.Gender
{
    public interface IGender : ISerialisationHandler
    {
        string Possessive { get; }
        string PersonalSubject { get; }
        string PersonalObject { get; }
        string Reflexive { get; }
        string Name { get; }

        string PossessivePlural { get; }
        string ReflexivePlural { get; }
        
        string IsOrAre { get; }
    }
}