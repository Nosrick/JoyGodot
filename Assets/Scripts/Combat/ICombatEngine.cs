using System.Collections.Generic;
using JoyLib.Code.Entities;

namespace JoyLib.Code.Combat
{
    public interface ICombatEngine
    {
        int MakeAttack(IEntity attacker,
            IEntity defender,
            IEnumerable<string> attackerTags,
            IEnumerable<string> defenderTags);
    }
}