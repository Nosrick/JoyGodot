using System.Globalization;
using System.Xml.Schema;
using Code.Unity.GUI.Managed_Assets;
using Godot;
using JoyGodot.Assets.Scripts.Managed_Assets;
using JoyLib.Code.Unity.GUI;

namespace JoyGodot.addons.Managed_Assets
{
#if TOOLS
    [Tool]
#endif
    public class ManagedTextButton :
        ManagedButton,
        ILabelElement,
        IManagedFonts
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
            get => this.MyLabel?.Text ?? this.m_TextToSet;
            set
            {
                this.m_TextToSet = value;
                if (this.MyLabel is null)
                {
                    return;
                }

                this.MyLabel.Text = value;
            }
        }

        protected string m_TextToSet;

        [Export] public bool AutoSize { get; set; }
        [Export] public bool OverrideSize { get; set; }
        [Export] public bool OverrideColour { get; set; }
        [Export] public bool OverrideOutline { get; set; }

        public bool HasFont => this.Theme.DefaultFont is null == false;
        public bool HasFontColours => this.m_HasFontColours;

        protected bool m_HasFontColours;

        [Export]
        public Color FontColour
        {
            get => this.m_FontColour;
            set
            {
                this.m_FontColour = value;
                if (this.OverrideColour == false)
                {
                    return;
                }

                this.MyLabel?.AddColorOverride("font_color", this.m_FontColour);
                this.m_HasFontColours = true;
                this.UpdateTheme();
            }
        }

        protected Color m_FontColour;

        [Export]
        public int FontSize
        {
            get => this.m_FontSize;
            set
            {
                this.m_FontSize = value;
                if (this.OverrideSize == false)
                {
                    return;
                }

                if (this.m_CustomFont is null == false)
                {
                    this.m_CustomFont.Size = this.m_FontSize;
                    this.UpdateTheme();
                }
            }
        }

        protected int m_FontSize;

        [Export]
        public Color OutlineColour
        {
            get => this.m_OutlineColour;
            set
            {
                this.m_OutlineColour = value;
                if (this.OverrideOutline == false)
                {
                    return;
                }

                if (this.m_CustomFont is null == false)
                {
                    this.m_CustomFont.OutlineColor = value;
                    this.UpdateTheme();
                }
            }
        }

        protected Color m_OutlineColour;

        [Export]
        public int OutlineThickness
        {
            get => this.m_OutlineThickness;
            set
            {
                this.m_OutlineThickness = value;
                if (this.OverrideOutline == false)
                {
                    return;
                }

                if (this.m_CustomFont is null == false)
                {
                    this.m_CustomFont.OutlineSize = value;
                    this.UpdateTheme();
                }
            }
        }

        protected int m_OutlineThickness;

        [Export] public int FontMinSize { get; set; }

        [Export] public int FontMaxSize { get; set; }

        [Export] public bool CacheFont { get; set; }

        [Export]
        public DynamicFont CustomFont
        {
            get => this.Theme?.DefaultFont as DynamicFont;
            set
            {
                this.m_CustomFont = this.CacheFont ? (DynamicFont) value.Duplicate() : value;

                if (this.MyLabel is null)
                {
                    GD.Print("Label not found!");
                    if (this.IsInsideTree())
                    {
                        GD.Print(this.GetPath());
                    }
                }

                GD.Print(value is null ? "Clearing font" : "Setting font");
                this.UpdateTheme();
            }
        }

        protected DynamicFont m_CustomFont;

        [Export]
        public bool TitleCase
        {
            get => this.m_TitleCase;
            set
            {
                if (value)
                {
                    this.Text = this.Text is null
                        ? this.Text
                        : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.Text);
                }

                this.m_TitleCase = value;
            }
        }

        protected bool m_TitleCase;

        protected void UpdateTheme()
        {
            if (this.Theme is null)
            {
                GD.Print("Theme is null for " + this.GetPath());
                return;
            }

            this.Theme.DefaultFont = this.m_CustomFont;
            if (this.MyLabel is null == false)
            {
                this.MyLabel.Theme = this.Theme;
            }
        }

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
                GD.Print("Creating label in " + this.GetType().Name);
                this.MyLabel = new Label
                {
                    Name = "Text",
                    AnchorBottom = 1,
                    AnchorRight = 1
                };
                this.AddChild(this.MyLabel);
#if TOOLS
                this.MyLabel.Owner = this.GetTree()?.EditedSceneRoot;
#endif
            }
            this.OutlineColour = this.OutlineColour;
            this.OutlineThickness = this.OutlineThickness;
            this.FontColour = this.FontColour;
            this.FontSize = this.FontSize;
            
            this.MyLabel.Align = this.HAlign;
            this.MyLabel.Valign = this.VAlign;

            this.MyLabel.AddFontOverride("font", this.m_CustomFont);
            this.MyLabel.Text = this.m_TextToSet;
        }
    }
}