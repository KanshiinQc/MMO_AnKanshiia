using Godot;

public partial class Fight1v1 : Node
{
    [Export]
    public int Player1Id { get; set; }
    public Vector2 Player1PositionBeforeFight { get; set; }

    [Export]
    public int Player2Id { get; set; }
    public Vector2 Player2PositionBeforeFight { get; set; }

    [Export]
    public int CurrentTurnPlayerId { get; set; }
} 