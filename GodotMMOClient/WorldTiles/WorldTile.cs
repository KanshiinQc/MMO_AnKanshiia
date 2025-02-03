using Godot;

public partial class WorldTile : Node2D
{
    private void OnArea2DMouseEntered()
    {
        GetNode<Node2D>("SelectedTile").Visible = true;
    }

    private void OnArea2DMouseExited()
    {
        GetNode<Node2D>("SelectedTile").Visible = false;
    }

    private void OnArea2DInputEvent(Node viewport, InputEvent @event, int shapeIdx)
    {
        if (@event.IsActionPressed("mouse_left_click"))
        {
            ServerAccessor.GetServerNode().MovePlayer(GetGlobalMousePosition().X, GetGlobalMousePosition().Y);
        }
    }
} 