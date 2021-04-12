using Code.Unity.GUI.Managed_Assets;
using Godot;

namespace JoyGodot.addons.Managed_Assets
{
#if TOOLS
    [Tool]
#endif
    public class ManagedTextButton : ManagedButton
    {
        /// <summary>
        /// <para>Text alignment policy for the button's text, use one of the <see cref="T:Godot.Button.TextAlign" /> constants.</para>
        /// </summary>
        [Export]
        public Label.AlignEnum HAlign
        {
            get => this.m_HAlign;
            set
            {
                this.m_HAlign = value;
                if (this.MyLabel is null == false)
                {
                    this.MyLabel.Align = this.m_HAlign;
                }

                this.EmitSignal("_AlignChanged");
            }
        }

        protected Label.AlignEnum m_HAlign;

        [Export]
        public Label.VAlign VAlign
        {
            get => this.m_VAlign;
            set
            {
                this.m_VAlign = value;
                if (this.MyLabel is null == false)
                {
                    this.MyLabel.Valign = this.m_VAlign;
                }

                this.EmitSignal("_AlignChanged");
            }
        }

        protected Label.VAlign m_VAlign;


        protected Label MyLabel { get; set; }

        [Signal]
        public delegate void _AlignChanged();

        [Export]
        public string Text
        {
            get =>this.MyLabel?.Text;
            set
            {
                if (this.MyLabel is null)
                {
                    this.m_TextToSet = value;
                    return;
                }

                this.MyLabel.Text = value;
            }
        }

        protected string m_TextToSet;

        [Export]
        public override Font CustomFont
        {
            get => this.HasFontOverride("font") ? this.GetFont("font") : null;
            set
            {
                this.AddFontOverride("default", value);
                if (this.MyLabel is null)
                {
                    GD.Print("Label is null in " + this.GetType().Name);
                    this.m_FontOverride = value;
                    return;
                }

                GD.Print("Setting font override");
                GD.Print(this.GetChildren());
                this.MyLabel.AddFontOverride("font", value);
            }
        }

        protected Font m_FontOverride;

        protected override void Initialise()
        {
            if (this.Initialised)
            {
                return;
            }

            this.MyLabel = this.GetNodeOrNull("Text") as Label;
            if (this.MyLabel is null)
            {
                GD.Print("Creating label in " + this.GetType().Name);
                this.MyLabel = new Label
                {
                    Name = "Text",
                    AnchorBottom = 1,
                    AnchorRight = 1,
                    Align = this.m_HAlign,
                    Valign = this.m_VAlign,
                    Text = this.m_TextToSet
                };
                this.AddChild(this.MyLabel);
#if TOOLS
                this.MyLabel.Owner = this.GetTree()?.EditedSceneRoot;
#endif
                this.MyLabel.AddFontOverride("font", this.m_FontOverride);
                this.m_TextToSet = null;
                this.m_FontOverride = null;
            }

            base.Initialise();
        }
    }
}