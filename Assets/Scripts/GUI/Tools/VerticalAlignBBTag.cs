using Godot;

namespace JoyLib.Code.Unity.GUI.Tools
{
    [Tool]
    public class VerticalAlignBBTag : RichTextEffect
    {
        public string bbCode = "vcenter";

        public override bool _ProcessCustomFx(CharFXTransform charFx)
        {
            GD.Print("TEST");
            
            int height = 0;
            if (charFx.Env.Contains("height"))
            {
                height = (int) charFx.Env["height"];
            }

            if (height > 0)
            {
                charFx.Offset = new Vector2(charFx.Offset.x, height / 2f);
            }
            
            return true;
        }
    }
}