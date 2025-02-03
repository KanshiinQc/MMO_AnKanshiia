using Godot;

public partial class Ore : Node2D
{
    [Export]
    public ulong NetworkUID { get; set; }

    private AnimatedSprite2D _animation;
    private Timer _oreRespawnTimer;

    public override void _Ready()
    {
        NetworkUID = GetInstanceId();
        Name = NetworkUID.ToString();
        
        _animation = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _oreRespawnTimer = GetNode<Timer>("Timer");
        
        _animation.Frame = 1;
        _oreRespawnTimer.WaitTime = GD.RandRange(20, 60);
    }

    public bool TryMine()
    {
        var wasMined = false;
        if (_animation.Frame == 1)
        {
            _animation.Frame = 0;
            wasMined = true;
            _oreRespawnTimer.Start();
        }
        return wasMined;
    }

    private void OnTimerTimeout()
    {
        _animation.Frame = 1;
        _oreRespawnTimer.WaitTime = GD.RandRange(20, 60);
        _oreRespawnTimer.Start();
    }
} 