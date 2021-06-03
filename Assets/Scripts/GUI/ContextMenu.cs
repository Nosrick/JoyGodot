using Godot;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.addons.Managed_Assets;
using JoyLib.Code;
using JoyLib.Code.Unity.GUI;
using Array = Godot.Collections.Array;

public class ContextMenu : GUIData
{
    public delegate void Action();
    
    protected VBoxContainer MainContainer { get; set; }
    protected PackedScene ButtonPrefab { get; set; }
    protected List<ManagedTextButton> ListItems { get; set; }
    protected IDictionary<int, Action> ItemActions { get; set; }
    
    public override void _Ready()
    {
        this.MainContainer = this.GetNode<VBoxContainer>("Margin Container/Main Margin/Main Container");
        this.ButtonPrefab =
            GD.Load<PackedScene>(GlobalConstants.GODOT_ASSETS_FOLDER + "Scenes/Parts/ManagedTextButton.tscn");
        this.ListItems = new List<ManagedTextButton>();
        this.ItemActions = new Dictionary<int, Action>();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        switch (@event)
        {
            case InputEventMouseButton mouseButton:
            {
                if (mouseButton.Pressed 
                    && this.GetRect().HasPoint(mouseButton.Position) == false)
                {
                    this.GUIManager?.CloseGUI(this, this.Name);
                }

                break;
            }
        }
    }

    public void Clear()
    {
        foreach (var item in this.ListItems)
        {
            this.ItemActions.Remove(item.GetHashCode());
            item.Visible = false;
            item.Text = null;
        }
    }

    public override void Display()
    {
        base.Display();
        this.RectPosition = this.GetViewport().GetMousePosition();
    }

    public override bool Close(object sender)
    {
        if (base.Close(sender))
        {
            this.Clear();
            return true;
        }

        return false;
    }

    public void AddItem(string text, Action action)
    {
        var item = this.ListItems.FirstOrDefault(i => i.Visible == false);
        if (item is null)
        {
            item = this.ButtonPrefab.Instance() as ManagedTextButton;
            item.ElementName = "SecondaryAccent";
            item.RectMinSize = new Vector2(0, 32);
            this.CallDeferred(nameof(this.DeferredItemSetup), item);
            this.MainContainer.AddChild(item);
            this.ListItems.Add(item);
        }

        if (item.IsConnected("_Press", this, "PlayAction"))
        {
            item.Disconnect("_Press", this, "PlayAction");
        }
        this.ItemActions.Add(item.GetHashCode(), action);
        item.Name = text;
        item.Text = text;
        item.Visible = true;
        item.Connect("_Press", this, "PlayAction", new Array
        {
            item.GetHashCode()
        });
    }

    protected void DeferredItemSetup(ManagedTextButton button)
    {
        this.GUIManager.SetupManagedComponents(button);
    }

    protected void PlayAction(int hashcode)
    {
        if (this.ItemActions.TryGetValue(hashcode, out Action action))
        {
            action.Invoke();
        }

        this.GUIManager?.CloseGUI(this, this.Name);
    }
}
