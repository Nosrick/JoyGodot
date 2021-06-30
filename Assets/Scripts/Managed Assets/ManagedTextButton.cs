using System.Globalization;
using Godot;

namespace JoyGodot.Assets.Scripts.Managed_Assets
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

        [Export]
        public bool AutoWrap
        {
            get => this.m_AutoWrap;
            set
            {
                this.m_AutoWrap = value;
                if (this.MyLabel is null == false)
                {
                    this.MyLabel.Autowrap = value;
                }
            }
        }

        protected bool m_AutoWrap;
        
        protected Control ChildParent { get; set; }
        public Label MyLabel { get; protected set; }

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
                this.UpdateFontOverride();
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
                    this.UpdateFontOverride();
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
                    this.UpdateFontOverride();
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
                    this.UpdateFontOverride();
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
            get => this.m_CustomFont;
            set
            {
                if (value is null)
                {
                    this.m_CustomFont = null;
                }
                else
                {
                    this.m_CustomFont = (DynamicFont) value.Duplicate();
                }
                
                if (this.MyLabel is null)
                {
                    //GD.PushWarning("Label not found!");
                }
                this.UpdateFontOverride();
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

        protected void UpdateFontOverride()
        {
            this.AddFontOverride("font", this.m_CustomFont);
            if (this.MyLabel is null == false)
            {
                this.MyLabel.AddFontOverride("font", this.m_CustomFont);
            }
        }

        public override void Initialise()
        {
            if (this.Initialised)
            {
                return;
            }

            base.Initialise();

            this.ChildParent = this.FindNode("ChildParent") as Control;
            this.MyLabel = this.FindNode("Text") as Label;
            if (this.MyLabel is null)
            {
                GD.PushWarning("Label is null in " + this.GetType().Name);
                this.MyLabel = new Label
                {
                    Name = "Text",
                    AnchorBottom = 1,
                    AnchorRight = 1
                };
                if (this.ChildParent is null)
                {
                    this.AddChild(this.MyLabel);
                }
                else
                {
                    this.ChildParent.AddChild(this.MyLabel);
                }
            }
            else
            {
                //GD.Print("Label found!");
            }
            this.OutlineColour = this.OutlineColour;
            this.OutlineThickness = this.OutlineThickness;
            this.FontColour = this.FontColour;
            this.FontSize = this.FontSize;
            
            this.MyLabel.Align = this.HAlign;
            this.MyLabel.Valign = this.VAlign;

            this.MyLabel.Text = this.m_TextToSet;
            this.MyLabel.Autowrap = this.m_AutoWrap;
        }
    }
}