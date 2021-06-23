using Godot;

namespace JoyGodot.Assets.Scripts.GUI
{
    public class ConstrainedDragHandle : DragHandle
    {
        protected enum ConstraintType
        {
            Anchors,
            Pixels
        }
        
        [Export] protected ConstraintType Constraint { get; set; }
        
        [Export] protected bool VerticalConstraint { get; set; }
        [Export] protected bool HorizontalConstraint { get; set; }
        
        [Export] public float XMin { get; set; }
        [Export] public float XMax { get; set; }
        
        [Export] public float YMin { get; set; }
        [Export] public float YMax { get; set; }

        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.ButtonIndex != (int) ButtonList.Left)
                {
                    return;
                }
                if (mouseButton.Pressed
                    && this.GetGlobalRect().HasPoint(mouseButton.GlobalPosition))
                {
                    this.Dragging = true;
                    this.FirstMove = true;
                }
                else if (mouseButton.Pressed == false)
                {
                    this.Dragging = false;
                }
            }
            else if (@event is InputEventMouseMotion mouseMotion)
            {
                if (!this.Dragging)
                {
                    return;
                }
                
                if (this.FirstMove)
                {
                    this.FirstMove = false;
                }
                else
                {
                    switch (this.Constraint)
                    {
                        case ConstraintType.Anchors:
                            if (this.HorizontalConstraint)
                            {
                                if (this.Parent.AnchorLeft > mouseMotion.Position.x
                                    || this.Parent.AnchorRight < mouseMotion.Position.x)
                                {
                                    return;
                                }

                                this.Parent.RectGlobalPosition += new Vector2(mouseMotion.Relative.x, 0);
                            }

                            if (this.VerticalConstraint)
                            {
                                if (this.Parent.AnchorTop > mouseMotion.Position.y
                                    || this.Parent.AnchorBottom < mouseMotion.Position.y)
                                {
                                    return;
                                }

                                this.Parent.RectGlobalPosition += new Vector2(0, mouseMotion.Relative.y);
                            }
                            break;
                        
                        case ConstraintType.Pixels:
                            if (this.HorizontalConstraint)
                            {
                                if (mouseMotion.GlobalPosition.x < this.XMin
                                    || mouseMotion.GlobalPosition.x > this.XMax)
                                {
                                    return;
                                }

                                this.Parent.RectGlobalPosition += new Vector2(mouseMotion.Relative.x, 0);
                            }

                            if (this.VerticalConstraint)
                            {
                                if (mouseMotion.GlobalPosition.y < this.YMin
                                    || mouseMotion.GlobalPosition.y > this.YMax)
                                {
                                    return;
                                }

                                this.Parent.RectGlobalPosition += new Vector2(0, mouseMotion.Relative.y);
                            }
                            break;
                    }
                }
            }
        }
    }
    
}