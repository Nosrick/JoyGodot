using Code.Unity.GUI.Managed_Assets;
using Godot;

namespace JoyGodot.addons.Managed_Assets
{
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
				this.EmitSignal("_AlignChanged");
				this.MyLabel.Align = this.HAlign;
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
				this.EmitSignal("_AlignChanged");
				this.MyLabel.Valign = this.m_VAlign;
			}
		}

		protected Label.VAlign m_VAlign;
		
		
		protected Label MyLabel { get; set; }

		public delegate void _AlignChanged();

		[Export]
		public string Text
		{
			get => this.MyLabel.Text;
			set => this.MyLabel.Text = value;
		}

		public override void _EnterTree()
		{
			base._EnterTree();
			this.Initialise();
			this.Connect("_AlignChanged", this, "Repaint");
		}

		protected void Repaint()
		{
			if (this.MyLabel is null == false)
			{
				this.MyLabel.Align = this.HAlign;
				this.MyLabel.Valign = this.VAlign;
			}
		}

		protected override void Initialise()
		{
			base.Initialise();
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
		}
	}
}
