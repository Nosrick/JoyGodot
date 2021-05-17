using Godot;
using JoyLib.Code.Unity.GUI;

namespace JoyLib.Code.Godot
{
    public class CheatInterface : GUIData
    {
        public void FillNeeds()
        {
            var player = GlobalConstants.GameManager.Player;

            if (player is null)
            {
                return;
            }

            foreach (var need in player.Needs.Values)
            {
                need.Fulfill(need.HappinessThreshold);
            }

            player.HappinessIsDirty = true;
        }

        public void EmptyNeeds()
        {
            var player = GlobalConstants.GameManager.Player;

            if (player is null)
            {
                return;
            }

            foreach (var need in player.Needs.Values)
            {
                need.Decay(need.HappinessThreshold);
            }

            player.HappinessIsDirty = true;
        }
    }
}