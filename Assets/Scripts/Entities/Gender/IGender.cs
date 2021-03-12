namespace JoyLib.Code.Entities.Gender
{
    public interface IGender
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