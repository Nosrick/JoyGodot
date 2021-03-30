using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code.Graphics;
using JoyLib.Code.States;

namespace JoyLib.Code.Unity.GUI.MainMenuState
{
    public class MainMenuHandler : GUIData
    {
        public override void _Ready()
        {
            ManagedUIElement background = new ManagedUIElement
            {
                AnchorBottom = 1,
                AnchorRight = 1
            };

            ISpriteState state = new SpriteState(
                "default",
                GlobalConstants.GameManager.ObjectIconHandler.GetSprites("Windows", "DefaultWindow").First());
            
            background.AddSpriteState(state);
            
            this.AddChild(background);
            this.MoveChild(background, 0);
        }

        public void NewGame()
        {
            GlobalConstants.GameManager.SetNextState(new CharacterCreationState());
            GlobalConstants.GameManager.GUIManager.CloseAllGUIs();
        }
    }
}