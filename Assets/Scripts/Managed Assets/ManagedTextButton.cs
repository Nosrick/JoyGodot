#if TOOLS
using Code.Unity.GUI.Managed_Assets;
using Godot;

namespace JoyGodot.addons.Managed_Assets
{
	[Tool]
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
			get
			{
				if (this.MyLabel is null)
				{
					this.Initialise();
				}
				return this.MyLabel?.Text;
			}
			set
			{
				if (this.MyLabel is null)
				{
					this.Initialise();
				}
				this.MyLabel.Text = value;
			}
		}

		public override void _EnterTree()
		{
			base._EnterTree();
			this.Initialise();
		}

		protected override void Initialise()
		{
			if (this.Initialised)
			{
				return;
			}
			
			GD.Print(nameof(this.Initialise));
			this.MyLabel = this.FindNode("Text") as Label;
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
			}
			base.Initialise();
		}
	}
}
#endif
