using System.Globalization;
using Godot;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyGodot.Assets.Scripts.GUI.Managed_Assets
{
#if TOOLS
    [Tool]
#endif
    public class ManagedLabel :
        ManagedUIElement,
        ILabelElement
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

        [Export]
        public bool TitleCase
        {
            get => this.m_TitleCase;
            set
            {
                if (value)
                {
                    this.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.Text);
                }

                this.m_TitleCase = value;
            }
        }

        protected bool m_TitleCase;

        protected Label MyLabel { get; set; }

        [Signal]
        public delegate void _AlignChanged();

        [Export]
        public string Text
        {
            get => this.MyLabel?.Text ?? this.m_TextToSet;
            set
            {
                if (value is null)
                {
                    this.m_TextToSet = null;
                }
                else
                {
                    this.m_TextToSet = this.TitleCase
                        ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value)
                        : value;
                }

                if (this.MyLabel is null)
                {
                    return;
                }

                this.MyLabel.Text = this.m_TextToSet;
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

        public override void Initialise()
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
#if TOOLS
                this.MyLabel.Owner = this.GetTree()?.EditedSceneRoot;
#endif
                this.MyLabel.AddFontOverride("font", this.m_FontOverride);
                this.m_TextToSet = null;
                this.m_FontOverride = null;
            }
        }
    }
}