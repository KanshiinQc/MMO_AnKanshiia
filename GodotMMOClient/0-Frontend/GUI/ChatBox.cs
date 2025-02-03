using Godot;

public partial class ChatBox : VBoxContainer
{
    private RichTextLabel _chatLog;
    private Label _inputLabel;
    private LineEdit _inputField;

    public string Username { get; set; } = "User";
    public Label InputLabel => _inputLabel;

    public override void _Ready()
    {
        _chatLog = GetNode<RichTextLabel>("RichTextLabel");
        _inputLabel = GetNode<Label>("HBoxContainer/Label");
        _inputField = GetNode<LineEdit>("HBoxContainer/LineEdit");
    }

    public override void _Input(InputEvent @event)
    {
        if (!Visible) return;

        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.IsActionPressed("Key_Enter"))
            {
                _inputField.GrabFocus();
            }
            if (keyEvent.IsActionPressed("Key_Escape"))
            {
                _inputField.ReleaseFocus();
            }
        }
    }

    public void AddMessage(string username, string text)
    {
        _chatLog.Text += $"\n[{username}]: {text}";
    }

    private void OnLineEditTextSubmitted(string newText)
    {
        if (newText.Length > 0)
        {
            AddMessage(Username, newText);
            ServerAccessor.GetServerNode().RequestAddMessageToChat(newText);
            _inputField.Text = "";
        }
    }
} 