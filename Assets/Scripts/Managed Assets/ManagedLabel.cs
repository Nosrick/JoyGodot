using Godot;

namespace JoyGodot.Assets.Scripts.GUI.Managed_Assets
{
#if TOOLS
    [Tool]
#endif
    public class ManagedLabel : ManagedUIElement
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
            get
            {
                /*
                if (this.MyLabel is null)
                {
                    GD.Print("Calling Initialise() from Text property");
                    //GD.Print(this.GetPath());
                    this.Initialise();
                }
                */

                return this.MyLabel?.Text ?? this.m_TextToSet;
            }
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
        public Font CustomFont
        {
            get => this.HasFontOverride("font") ? this.GetFont("font") : null;
            set
            {
                if (this.IsInsideTree() == false)
                {
                    GD.Print("Not inside tree when trying to set font!");
                    this.m_FontOverride = value;
                    return;
                }

                GD.Print(value is null ? "Clearing font" : "Setting font");

                this.AddFontOverride("font", value);
                if (this.MyLabel is null && this.IsInsideTree())
                {
                    GD.Print("Label needs to be created");
                }

                this.m_FontOverride = value;
                this.MyLabel?.AddFontOverride("font", value);
            }
        }

        protected Font m_FontOverride;

        protected override void Initialise()
        {
            if (this.Initialised)
            {
                return;
            }

            base.Initialise();

            this.MyLabel = this.GetNodeOrNull("Text") as Label;
            if (this.MyLabel is null)
            {
                GD.Print("Creating label");
                this.MyLabel = new Label
                {
                    Name = "Text",
                    AnchorBottom = 1,
                    AnchorRight = 1,
                    Text = this.m_TextToSet,
                    Align = this.HAlign,
                    Valign = this.VAlign
                };
                this.AddChild(this.MyLabel);
                this.MyLabel.Owner = this.GetTree().EditedSceneRoot;
                this.MyLabel.AddFontOverride("font", this.m_FontOverride);
                this.m_TextToSet = null;
                this.m_FontOverride = null;
            }
        }
    }
}