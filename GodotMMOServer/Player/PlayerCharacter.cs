using Godot;

public partial class PlayerCharacter : CharacterBody2D
{
    [Export]
    public int ID { get; set; }

    [Export]
    public string Username { get; set; }

    [Export]
    public long PeerID { get; set; }

    [Export]
    public bool IsFighting { get; set; }

    public bool IsLocalPlayer { get; private set; }

    [Export]
    public int CurrentHealth { get; set; }

    [Export]
    public int MaxHealth { get; set; }

    private const float Speed = 100;
    private float _angle;
    private Vector2 _targetPosition;
    public bool IsMoving { get; private set; }

    public override void _Ready()
    {
        _targetPosition = Position;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Position.DistanceTo(_targetPosition) > 1)
        {
            IsMoving = true;
            var direction = (_targetPosition - Position).Normalized();
            _angle = Mathf.RadToDeg(Mathf.Atan2(direction.Y, direction.X)) + 180;
            Position = Position.MoveToward(_targetPosition, Speed * (float)delta);
        }
        else
        {
            IsMoving = false;
        }

        SetPlayerAnimation(_angle);
    }

    public void SetTargetPosition(Vector2 newPosition)
    {
        _targetPosition = newPosition;
    }

    private void SetPlayerAnimation(float movingAngle)
    {
        var animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        if (IsMoving)
        {
            if (movingAngle >= 337.5f || movingAngle < 22.5f)
                animatedSprite.Play("WALK_L");
            else if (movingAngle >= 22.5f && movingAngle < 67.5f)
                animatedSprite.Play("WALK_U_L");
            else if (movingAngle >= 67.5f && movingAngle < 112.5f)
                animatedSprite.Play("WALK_U");
            else if (movingAngle >= 112.5f && movingAngle < 157.5f)
                animatedSprite.Play("WALK_U_R");
            else if (movingAngle >= 157.5f && movingAngle < 202.5f)
                animatedSprite.Play("WALK_R");
            else if (movingAngle >= 202.5f && movingAngle < 247.5f)
                animatedSprite.Play("WALK_D_R");
            else if (movingAngle >= 247.5f && movingAngle < 292.5f)
                animatedSprite.Play("WALK_D");
            else if (movingAngle >= 292.5f && movingAngle < 337.5f)
                animatedSprite.Play("WALK_D_L");
        }
        else
        {
            animatedSprite.Stop();
        }
    }
} 