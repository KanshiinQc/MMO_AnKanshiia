using Godot;

public partial class WorldTile : Node2D
{
    private Area2D _area;
    private CollisionPolygon2D _collisionPolygon;
    // The offset from the collision center to the visual center
    public const float VISUAL_OFFSET_Y = -6;

    public override void _Ready()
    {
        _area = GetNode<Area2D>("Area2D");
        _collisionPolygon = _area.GetNode<CollisionPolygon2D>("CollisionPolygon2D");
        
        // Enable input events on the Area2D
        _area.InputPickable = true;
        _area.MouseEntered += OnMouseEntered;
        _area.MouseExited += OnMouseExited;
    }

    public override void _ExitTree()
    {
        if (_area != null)
        {
            _area.MouseEntered -= OnMouseEntered;
            _area.MouseExited -= OnMouseExited;
        }
    }

    private void OnMouseEntered()
    {
        GetNode<Node2D>("SelectedTile").Visible = true;
    }

    private void OnMouseExited()
    {
        GetNode<Node2D>("SelectedTile").Visible = false;
    }

    public Vector2 GetActualCenter()
    {
        if (_collisionPolygon == null) return GlobalPosition;

        // Calculate the bounds of the collision polygon
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        
        foreach (Vector2 point in _collisionPolygon.Polygon)
        {
            minY = Mathf.Min(minY, point.Y);
            maxY = Mathf.Max(maxY, point.Y);
        }

        // Calculate the center Y offset from the current position
        float collisionCenterOffset = (minY + maxY) / 2;
        
        return new Vector2(GlobalPosition.X, GlobalPosition.Y + collisionCenterOffset + VISUAL_OFFSET_Y);
    }
} 