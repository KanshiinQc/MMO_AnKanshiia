using Godot;

public partial class WorldMap : Node2D
{
    public override void _Ready()
    {
        var worldTile = GD.Load<PackedScene>("res://WorldTiles/WorldTile.tscn");
        
        for (int i = 0; i < 14; i++)
        {
            for (int y = 0; y < 10; y++)
            {
                var instance = worldTile.Instantiate<Node2D>();
                instance.Position = new Vector2(32 * i, 32 * y);
                AddChild(instance);

                var instance2 = worldTile.Instantiate<Node2D>();
                instance2.Position = new Vector2((32 * i) + 16, (32 * y) - 8);
                AddChild(instance2);

                var instance3 = worldTile.Instantiate<Node2D>();
                instance3.Position = new Vector2((32 * i) + 16, (32 * y) + 8);
                AddChild(instance3);

                var instance4 = worldTile.Instantiate<Node2D>();
                instance4.Position = new Vector2(32 * i, (32 * y) + 16);
                AddChild(instance4);
            }
        }
    }
} 