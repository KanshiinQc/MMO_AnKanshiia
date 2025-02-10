using Godot;
using System;

public partial class GUI : Node
{
    private ChatBox _chatBox;
    private ProgressBar _playerHealth;
    private VBoxContainer _lootWindow;
    private AnimatedSprite2D _loadingAnimation;
    private Label _currentPlayerTurnLabel;
    private Label _fpsCounter;

    public override void _Ready()
    {
        _chatBox = GetNode<ChatBox>("ChatBox");
        _playerHealth = GetNode<ProgressBar>("PlayerHealth");
        _lootWindow = GetNode<VBoxContainer>("LootWindow");
        _loadingAnimation = GetNode<AnimatedSprite2D>("Loading");
        _currentPlayerTurnLabel = GetNode<Label>("CurrentPlayerTurn");
        _fpsCounter = GetNode<Label>("FpsCounter");
    }

    public void InitializePlayerHealth(int maxHealth, int currentHealth)
    {
        _playerHealth.MaxValue = maxHealth;
        _playerHealth.Value = currentHealth;
    }

    public void UpdatePlayerHealth(int currentHealth)
    {
        _playerHealth.Value = currentHealth;
        var label = _playerHealth.GetNode<Label>("Label");
        label.Text = currentHealth.ToString();
    }

    public void ShowChat()
    {
        _chatBox.Visible = true;
    }

    public void SetupChatForUser(string username)
    {
        _chatBox.Username = username;
        _chatBox.InputLabel.Text = $"[{username}]";
    }

    public void AddChatMessage(string username, string message)
    {
        _chatBox.AddMessage(username, message);
    }

    public void ShowPlayerHealth()
    {
        _playerHealth.Visible = true;
    }

    public void ShowLootWindow()
    {
        _lootWindow.Visible = true;
    }

    public void AddLootItem(Sprite2D lootImage)
    {
        _lootWindow.AddChild(lootImage, true);
    }

    public void HideLootWindow()
    {
        _lootWindow.Visible = false;
        foreach (Node loot in _lootWindow.GetChildren())
        {
            loot.QueueFree();
        }
    }

    public void Notify(string message)
    {
        GetNode<Label>("Notifications").Text = message;
    }

    private void OnLootWindowGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouse mouseEvent)
        {
            if (mouseEvent.IsActionPressed("mouse_right_click"))
            {
                if (_lootWindow.Visible)
                {
                    HideLootWindow();
                }
            }
        }
    }

    public void ShowLoading()
    {
        _loadingAnimation.Visible = true;
    }

    public void HideLoading()
    {
        _loadingAnimation.Visible = false;
    }

    public void ShowFightTurns()
    {
        _currentPlayerTurnLabel.Visible = true;
    }

    public void HideFightTurns()
    {
        _currentPlayerTurnLabel.Visible = false;
    }

    public void UpdateTurnLabel(string playerName)
    {
        _currentPlayerTurnLabel.Text = $"[{playerName}]'s turn";
    }

    public override void _Process(double delta)
    {
        if (_fpsCounter is not null)
        {
            _fpsCounter.Text = "FPS " + Engine.GetFramesPerSecond();
        }
    }
} 