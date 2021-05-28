using System.Globalization;
using Godot;
using JoyGodot.Assets.Scripts.Managed_Assets;
using JoyLib.Code.Unity.GUI;

namespace JoyGodot.Assets.Scripts.GUI.Managed_Assets
{
#if TOOLS
    [Tool]
#endif
    public class ManagedLabel :
        ManagedUIElement,
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

        [Export]
        public bool TitleCase
        {
            get => this.m_TitleCase;
            set
            {
                this.m_TitleCase = value;
                this.Text = this.Text;
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

        [Export] public bool AutoSize { get; set; }
        [Export] public bool OverrideSize { get; set; }
        [Export] public bool OverrideColour { get; set; }
        [Export] public bool OverrideOutline { get; set; }

        public bool HasFont => this.Theme.DefaultFont is null == false;
        public bool HasFontColours => this.m_HasFontColours;

        protected bool m_HasFontColours;

        [Export]
        public bool CacheFont
        {
            get => this.m_CacheFont;
            set
            {
                this.m_CacheFont = value;
                if (value)
                {
                    if (this.m_CustomFont is null)
                    {
                        return;
                    }
                    this.CustomFont = (DynamicFont) this.m_CustomFont.Duplicate();
                }
            }
        }

        protected bool m_CacheFont = true;

        [Export]
        public DynamicFont CustomFont
        {
            get => this.Theme?.DefaultFont as DynamicFont;
            set
            {
                if (value is null)
                {
                    this.m_CustomFont = null;
                }
                else
                {
                    this.m_CustomFont = this.CacheFont ? (DynamicFont) value.Duplicate() : value;
                }

                if (this.MyLabel is null)
                {
                    GD.Print("Label not found!");
                }

                GD.Print(value is null ? "Clearing font" : "Setting font");
                this.UpdateTheme();
            }
        }

        protected DynamicFont m_CustomFont;

        [Export]
        public Color FontColour
        {
            get => this.m_FontColour;
            set
            {
                if (this.OverrideColour == false)
                {
                    return;
                }
                
                this.m_FontColour = value;
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
                if (this.OverrideSize
                    && this.m_CustomFont is null == false)
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
                if (this.OverrideOutline
                    && this.m_CustomFont is null == false)
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
                if (this.OverrideOutline
                    && this.m_CustomFont is null == false)
                {
                    this.m_CustomFont.OutlineSize = value;
                    this.UpdateTheme();
                }
            }
        }

        protected int m_OutlineThickness;

        [Export] public int FontMinSize { get; set; }

        [Export] public int FontMaxSize { get; set; }

        protected void UpdateTheme()
        {
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

            if (this.Theme is null)
            {
                this.Theme = new Theme();
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

            //this.MyLabel.AddFontOverride("font", this.m_CustomFont);
            this.MyLabel.Text = this.m_TextToSet;
        }
    }
}