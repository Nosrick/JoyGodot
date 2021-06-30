using System.Collections.Generic;

namespace JoyGodot.Assets.Scripts.Rollers
{
    public interface IRollable
    {
        int Roll(int lower, int upper);

        int RollSuccesses(int number, int threshold);

        T SelectFromCollection<T>(IEnumerable<T> collection);
    }
}
