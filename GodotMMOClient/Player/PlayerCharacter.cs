using Godot;

public partial class PlayerCharacter : CharacterBody2D
{
    public bool IsLocalPlayer { get; set; }

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
        if (HasNode("Area2D"))
        {
            var area2D = GetNode<Area2D>("Area2D");
            if (area2D != null)
            {
                area2D.InputPickable = true;
                area2D.CollisionLayer = 1; // Layer for players
                area2D.CollisionMask = 1;  // Can detect other players
                area2D.Monitorable = true;
                area2D.Monitoring = true;
            }
        }
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
        if (IsLocalPlayer && Input.IsActionJustPressed("mouse_left_click") && !IsFighting)
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

            // Check what we clicked using a point query
            var spaceState = GetWorld2D().DirectSpaceState;
            var parameters = new PhysicsPointQueryParameters2D
            {
                Position = GetGlobalMousePosition(),
                CollideWithAreas = true,
                CollideWithBodies = true
            };
            var results = spaceState.IntersectPoint(parameters);

            if (results.Count > 0)
            {
                var collider = results[0]["collider"].As<Node>();

                if (collider is PlayerCharacter player)
                {
                    if (!player.IsLocalPlayer && !player.IsFighting)
                    {
                        GD.Print("Clicked Player " + player.Username);
                        ServerAccessor.GetServerNode().RequestFightWithPlayer(player.ID);
                    }
                    return;
                }

                else if (collider is Area2D area2 && area2.GetParent() is WorldTile worldTile)
                {
                    var centerPosition = worldTile.GetActualCenter();
                    
                    // If we're already at this tile's center (with some small tolerance), don't move
                    if (Position.DistanceTo(centerPosition) < 1.0f)
                    {
                        GD.Print($"Already at tile center, not moving");
                        return;
                    }

                    GD.Print($"Player {Username} clicked tile. Moving to center: {centerPosition}");
                    ServerAccessor.GetServerNode().MovePlayer(centerPosition.X, centerPosition.Y);
                    return;
                }
            }
        }
    }
} 