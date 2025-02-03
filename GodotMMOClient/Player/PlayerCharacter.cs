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
        hud?.InitializePlayerHealth(MaxHealth, CurrentHealth);
        _usernameLabel.Text = Username;

        // Enable input processing
        InputPickable = true;
        
        // Setup collision only if Area2D exists
        if (HasNode("Area2D"))
        {
            var area2D = GetNode<Area2D>("Area2D");
            if (area2D != null)
            {
                area2D.CollisionLayer = 1; // Layer for players
                area2D.CollisionMask = 1;  // Can detect other players
                area2D.Monitorable = true;
                area2D.Monitoring = true;
            }
        }
        
        InputEvent += OnInputEvent;
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
            // Check if we're clicking on a UI element
            var hud = GetTree().Root.GetNode<GUI>("HUD");
            if (hud != null)
            {
                // Check if we clicked on the chat box
                var chatBox = hud.GetNode<Control>("ChatBox");
                if (chatBox != null && chatBox.Visible)
                {
                    var mousePos = GetViewport().GetMousePosition();
                    if (chatBox.GetGlobalRect().HasPoint(mousePos))
                    {
                        // Clicked on chat box, don't process movement
                        GD.Print("Clicked on chat box, ignoring movement");
                        return;
                    }
                }

                // Check other UI elements if needed
                var lootWindow = hud.GetNode<Control>("LootWindow");
                if (lootWindow != null && lootWindow.Visible && lootWindow.GetGlobalRect().HasPoint(GetViewport().GetMousePosition()))
                {
                    GD.Print("Clicked on loot window, ignoring movement");
                    return;
                }
            }

            // Get the mouse position in the game world
            var clickPos = GetGlobalMousePosition();
            
            // Check if we clicked on a player
            var spaceState = GetWorld2D().DirectSpaceState;
            var query = PhysicsRayQueryParameters2D.Create(Position, clickPos);
            query.CollideWithAreas = true;
            query.CollideWithBodies = true;
            var result = spaceState.IntersectRay(query);

            // If we didn't hit anything, then move
            if (result.Count == 0)
            {
                ServerAccessor.GetServerNode().MovePlayer(clickPos.X, clickPos.Y);
            }
            // If we hit something, check if it's a player
            else if (result["collider"].As<Node>() is PlayerCharacter)
            {
                // Don't move, the OnInputEvent handler will handle player interaction
                GD.Print("Clicked on a player, not moving");
            }
            // If we hit something else, then move
            else
            {
                ServerAccessor.GetServerNode().MovePlayer(clickPos.X, clickPos.Y);
            }
        }
    }

    private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (!IsLocalPlayer && !IsFighting)
        {
            if (@event is InputEventMouseButton mouseEvent)
            {
                if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
                {
                    GD.Print("Clicked Player " + Name);
                    ServerAccessor.GetServerNode().RequestFightWithPlayer(int.Parse(Name));
                }
            }
        }
    }
} 