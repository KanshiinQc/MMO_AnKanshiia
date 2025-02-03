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
    }

    private void HandleInput()
    {
        if (Input.IsActionJustPressed("mouse_left_click") && !IsFighting)
        {
            var clickPos = GetGlobalMousePosition();
            ServerAccessor.GetServerNode().MovePlayer(clickPos.X, clickPos.Y);
        }
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
} 