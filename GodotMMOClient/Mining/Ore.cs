using Godot;

public partial class Ore : Node2D
{
    [Export]
    public int NetworkUID { get; set; }

    private Area2D _clickableArea;

    public override void _Ready()
    {
        _clickableArea = GetNode<Area2D>("Area2D");
    }

    private void OnArea2dInputEvent(Node viewport, InputEvent @event, int shapeIdx)
    {
        if (@event is InputEventMouse mouseEvent)
        {
            if (mouseEvent.IsActionPressed("mouse_left_click"))
            {
                GD.Print("Clicked");
                ServerAccessor.GetServerNode().MineOre(NetworkUID);
            }
        }
    }
} 