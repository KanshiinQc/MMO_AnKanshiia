using Godot;

public partial class Login : Control
{
    [Export]
    public string Username { get; set; }
    
    [Export]
    public string Password { get; set; }

    private LineEdit _usernameInput;
    private LineEdit _passwordInput;

    public override void _Ready()
    {
        _usernameInput = GetNode<LineEdit>("Username");
        _passwordInput = GetNode<LineEdit>("Password");
    }

    private void OnButtonLoginPressed()
    {
        Username = _usernameInput.Text;
        Password = _passwordInput.Text;
        ServerAccessor.GetServerNode().SendLoginRequest(Username, Password);
    }

    private void OnButtonRegisterPressed()
    {
        Username = _usernameInput.Text;
        Password = _passwordInput.Text;
        ServerAccessor.GetServerNode().SendRegistrationRequest(Username, Password);
    }

    private void OnPasswordTextSubmitted(string newText)
    {
        Username = _usernameInput.Text;
        Password = _passwordInput.Text;
        ServerAccessor.GetServerNode().SendLoginRequest(Username, Password);
    }

    private void OnUsernameTextSubmitted(string newText)
    {
        Username = _usernameInput.Text;
        Password = _passwordInput.Text;
        ServerAccessor.GetServerNode().SendLoginRequest(Username, Password);
    }

    public void AutoLogin(string username, string password)
    {
        Username = username;
        Password = password;
        ServerAccessor.GetServerNode().SendLoginRequest(username, password);
    }
} 