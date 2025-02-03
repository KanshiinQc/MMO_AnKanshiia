using Godot;

public partial class FightTile : Node2D
{
    private Node2D _selectedTile;

    public override void _Ready()
    {
        _selectedTile = GetNode<Node2D>("SelectedTile");
    }

    private void OnArea2dMouseEntered()
    {
        _selectedTile.Visible = true;
    }

    private void OnArea2dMouseExited()
    {
        _selectedTile.Visible = false;
    }

    private void OnArea2dInputEvent(Node viewport, InputEvent @event, int shapeIdx)
    {
        if (@event.IsActionPressed("mouse_left_click"))
        {
            ServerAccessor.GetServerNode().AttackTile(Position.X, Position.Y);
        }
    }
} 