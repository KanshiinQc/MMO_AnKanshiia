using Godot;

public partial class FightMap : Node2D
{
    public override void _Ready()
    {
        var fightTile = GD.Load<PackedScene>("res://Fight/FightTile.tscn");
        
        for (int i = 0; i < 12; i++)
        {
            for (int y = 0; y < 12; y++)
            {
                var instance = fightTile.Instantiate<Node2D>();
                instance.Position = new Vector2(32 * i, 32 * y);
                AddChild(instance);

                var instance2 = fightTile.Instantiate<Node2D>();
                instance2.Position = new Vector2((32 * i) + 16, (32 * y) - 8);
                AddChild(instance2);

                var instance3 = fightTile.Instantiate<Node2D>();
                instance3.Position = new Vector2((32 * i) + 16, (32 * y) + 8);
                AddChild(instance3);

                var instance4 = fightTile.Instantiate<Node2D>();
                instance4.Position = new Vector2(32 * i, (32 * y) + 16);
                AddChild(instance4);
            }
        }

        // Debug print commented out
        //foreach (Node child in GetChildren())
        //{
        //    if (child is Node2D node)
        //    {
        //        GD.Print($"{node.Position.X}, {node.Position.Y}");
        //    }
        //}
    }
} 