using Godot;

public partial class FightTile : Node2D
{
    private Node2D _selectedTile;

    public override void _Ready()
    {
        _selectedTile = GetNode<Node2D>("SelectedTile");
    }
} 