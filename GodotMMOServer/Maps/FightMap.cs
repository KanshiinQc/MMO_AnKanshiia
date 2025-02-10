using Godot;
using System;

public partial class FightMap : Node2D
{
    [Export]
    public int Player1Id { get; set; }
    public Vector2 Player1PositionBeforeFight { get; set; }

    [Export]
    public int Player2Id { get; set; }
    public Vector2 Player2PositionBeforeFight { get; set; }

    public int FightWinnerId { get; set; }

    [Export]
    public int CurrentTurnPlayerId { get; set; }

    [Export]
    public bool IsCurrentlyUsed { get; set; }
}
