using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Entities;

namespace JoyGodot.Assets.Scripts.Combat
{
    public interface ICombatEngine
    {
        int MakeAttack(IEntity attacker,
            IEntity defender,
            IEnumerable<string> attackerTags,
            IEnumerable<string> defenderTags);
    }
}