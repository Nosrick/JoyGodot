using System;
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
                            Vector2 parentPosition = this.Parent.RectPosition;
                            Vector2 grandparentSize = this.Parent.GetParentControl().RectSize;
                            Vector2 proposedMove = parentPosition + mouseMotion.Relative;
                            
                            Vector2 localProposedPosition = proposedMove / grandparentSize;
                            
                            GD.Print("relative parent position " + localProposedPosition);
                            
                            Vector2 offset = Vector2.Zero;

                            if (this.HorizontalConstraint)
                            {
                                if (this.XMin > localProposedPosition.x
                                    || this.XMax < localProposedPosition.x)
                                {
                                    return;
                                }

                                offset.x += mouseMotion.Relative.x;
                            }

                            if (this.VerticalConstraint)
                            {
                                if (this.YMin > localProposedPosition.y
                                    || this.YMax < localProposedPosition.y)
                                {
                                    return;
                                }

                                offset.y += mouseMotion.Relative.y;
                            }

                            this.Parent.RectPosition += offset;
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
                    
                            this.Parent.RectGlobalPosition = new Vector2(
                                this.HorizontalConstraint
                                    ? Mathf.Clamp(this.Parent.RectGlobalPosition.x, this.XMin, this.XMax)
                                    : this.Parent.RectGlobalPosition.x,
                                this.VerticalConstraint
                                    ? Mathf.Clamp(this.Parent.RectGlobalPosition.y, this.YMin, this.YMax)
                                    : this.Parent.RectGlobalPosition.y);
                            break;
                    }
                }
            }
        }
    }
    
}