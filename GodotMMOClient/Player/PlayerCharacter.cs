using Godot;

public partial class PlayerCharacter : CharacterBody2D
{
    public bool IsLocalPlayer { get; private set; }

    [Export]
    public int ID { get; set; }

    [Export]
    public string Username { get; set; }

    [Export]
    public long PeerID { get; set; }

    [Export]
    public bool IsFighting { get; set; }

    [Export]
    public bool IsMoving { get; private set; }

    [Export]
    public int CurrentHealth { get; set; }

    [Export]
    public int MaxHealth { get; set; }

    private Label _usernameLabel;
    private float _angle;
    private const float Speed = 100;
    private Vector2 _targetPosition;

    public override void _Ready()
    {
        IsLocalPlayer = PeerID == Multiplayer.MultiplayerPeer.GetUniqueId();
        if (IsLocalPlayer)
        {
            GetNode<Camera2D>("Camera2D").Enabled = true;
        }

        _usernameLabel = GetNode<Label>("Username");
        var hud = GetTree().Root.GetNode("HUD") as GUI;
        hud.InitializePlayerHealth(MaxHealth, CurrentHealth);
        _usernameLabel.Text = Username;
    }

    public override void _Process(double delta)
    {
        if (IsLocalPlayer)
        {
            HandleInput();
            var hud = GetTree().Root.GetNode("HUD") as GUI;
            hud.UpdatePlayerHealth(CurrentHealth);
        }
        SetPlayerAnimation(_angle);
    }

    private void HandleInput()
    {
        if (Input.IsActionJustPressed("mouse_left_click") && !IsFighting)
        {
            var clickPos = GetGlobalMousePosition();
            ServerAccessor.GetServerNode().MovePlayer(clickPos.X, clickPos.Y);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsLocalPlayer)
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
        }
    }

    public void UpdatePosition(Vector2 newPosition)
    {
        _targetPosition = newPosition;
    }

    private void OnInputEvent(Node viewport, InputEvent @event, int shapeIdx)
    {
        if (!IsLocalPlayer && !IsFighting)
        {
            if (@event is InputEventMouse mouseEvent)
            {
                if (mouseEvent.IsActionPressed("mouse_left_click"))
                {
                    GD.Print("Clicked");
                    ServerAccessor.GetServerNode().RequestFightWithPlayer(int.Parse(Name));
                }
            }
        }
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