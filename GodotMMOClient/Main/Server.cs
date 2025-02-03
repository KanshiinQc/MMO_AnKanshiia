using Godot;
using System.Collections.Generic;
using System.Linq;

namespace CLIENT
{
    public partial class Server : Node
    {
        #region Variables - Networking
        private ENetMultiplayerPeer _network = new();
        //private string _ip = "170.187.179.212";
        private string _ip = "127.0.0.1";
        private int _port = 1909;
        #endregion

        #region Variables - Node References
        private Node2D _playersContainer;
        private Node2D _resourcesContainer;
        private Node2D _fightsContainer;
        private Node2D _worldMapNode;
        private Node2D _fightMapNode;
        #endregion

        #region Variables - UI
        private GUI _guiNode;
        private Login _loginNode;
        #endregion

        #region Variables - Game Data
        private Dictionary<int, string> _itemSprites = new();
        #endregion

        #region Initialization
        public override void _Ready()
        {
            _playersContainer = GetNode<Node2D>("PlayersContainer");
            _resourcesContainer = GetNode<Node2D>("ResourcesContainer");
            _fightsContainer = GetNode<Node2D>("FightsContainer");
            _worldMapNode = GetNode<Node2D>("WorldMap");
            _fightMapNode = GetNode<Node2D>("FightMap");

            LoadItemSprites();
            LoadGUI();
            ConnectToServer();
        }

        private void LoadItemSprites()
        {
            int number = 0;
            foreach (string filePath in DirAccess.GetFilesAt("res://Items/Sprites"))
            {
                if (filePath.GetExtension() == "png")
                {
                    _itemSprites[number] = $"res://Items/Sprites/{filePath.GetFile()}";
                    number++;
                }
            }
        }

        private void LoadGUI()
        {
            var guiStart = GD.Load<PackedScene>("res://0-Frontend/GUI/GUI.tscn");
            _guiNode = guiStart.Instantiate() as GUI;
            GetTree().Root.CallDeferred("add_child", _guiNode, true);
            _loginNode = _guiNode.GetNode<Login>("Login");
        }
        #endregion

        #region Connection Handling
        private void ConnectToServer()
        {
            NotifyPlayer("Attempting to connect to server. Please wait...");
            var error = _network.CreateClient(_ip, _port);
            if (error != Error.Ok)
            {
                GD.PrintErr($"Failed to create client: {error}");
                _OnConnectionFailed();
                return;
            }
            Multiplayer.MultiplayerPeer = _network;
            Multiplayer.ConnectedToServer += _OnConnectionSucceeded;
            Multiplayer.ConnectionFailed += _OnConnectionFailed;
            GD.Print($"Attempting to connect to {_ip}:{_port}");
        }

        private void _OnConnectionFailed()
        {
            _guiNode.HideLoading();
            NotifyPlayer("Could not connect to server. Is it down?");
        }

        private void _OnConnectionSucceeded()
        {
            _guiNode.HideLoading();
            NotifyPlayer("Connected to server successfully. You can now log in");
        }
        #endregion

        #region Authentication RPCs
        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void AutomaticallyConnectPeer(long playerId, string username)
        {
            string password = "defaultpass123";
            if (_loginNode == null)
            {
                CallDeferred("AutomaticallyConnectPeer", playerId, username);
                return;
            }

            _loginNode.Username = username;
            _loginNode.Password = password;

            GD.Print($"Attempting automatic login for user: {username}");
            SendLoginRequest(username, password);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void ValidateLoginRequest(string username, string password)
        {
            // Client-side implementation is empty as this is called on the server
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void ValidateRegistrationRequest(string username, string password)
        {
            // Client-side implementation is empty as this is called on the server
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void LoginSuccess(string message)
        {
            ConnectPlayer();
            MakeGameWorldObjectsVisible();
            SetupChatBoxUI();
            SetupPlayerUI();
            NotifyPlayer(message);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void LoginFailed(string message)
        {
            NotifyPlayer(message);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void RegistrationSuccess(string message)
        {
            NotifyPlayer(message);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void RegistrationFailed(string message)
        {
            NotifyPlayer(message);
        }
        #endregion

        #region Player Action RPCs
        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void ConnectPlayer()
        {
            RpcId(1, "ConnectPlayer");
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void MovePlayer(float xPos, float yPos)
        {
            RpcId(1, "MovePlayer", xPos, yPos);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void MineOre(int networkUID)
        {
            RpcId(1, "MineOre", networkUID);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void SendKeyInput(Vector2 velocity)
        {
            RpcId(1, "SendKeyInput", velocity);
        }
        #endregion

        #region Fight System RPCs
        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void RequestFightWithPlayer(int playerId)
        {
            RpcId(1, "RequestFightWithPlayer", playerId);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void StartFightWithPlayer()
        {
            _worldMapNode.Visible = !_worldMapNode.Visible;
            _worldMapNode.SetProcess(!_worldMapNode.IsProcessing());
            _fightMapNode.Visible = !_fightMapNode.Visible;
            _fightMapNode.SetProcess(!_fightMapNode.IsProcessing());
            GetViewport().GetCamera2D().Zoom = new Vector2(3, 3);

            foreach (Node resource in _resourcesContainer.GetChildren())
            {
                if (resource is Ore ore)
                {
                    ore.Visible = false;
                }
            }
            _guiNode.ShowFightTurns();
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void AttackTile(float positionX, float positionY)
        {
            RpcId(1, "AttackTile", positionX, positionY);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void UpdateFightTurn(string username)
        {
            _guiNode.UpdateTurnLabel(username);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void TerminateFight(Vector2 playerInitialPosition)
        {
            _worldMapNode.Visible = !_worldMapNode.Visible;
            _worldMapNode.SetProcess(!_worldMapNode.IsProcessing());
            _fightMapNode.Visible = !_fightMapNode.Visible;
            _fightMapNode.SetProcess(!_fightMapNode.IsProcessing());
            GetViewport().GetCamera2D().Zoom = new Vector2(4, 4);

            foreach (Node resource in _resourcesContainer.GetChildren())
            {
                if (resource is Ore ore)
                {
                    ore.Visible = true;
                }
            }

            var player = GetLocalPlayer();
            player.Position = playerInitialPosition;
            _guiNode.HideFightTurns();
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void ShowLoot(Dictionary<int, int> lootDictionary)
        {
            _guiNode.ShowLootWindow();
            
            var keys = lootDictionary.Keys.ToArray();
            var values = lootDictionary.Values.ToArray();
            
            for (int i = 0; i < lootDictionary.Count; i++)
            {
                var itemUIScene = GD.Load<PackedScene>("res://Items/ItemUI.tscn");
                var lootImage = itemUIScene.Instantiate<Sprite2D>();
                lootImage.Scale = new Vector2(4, 4);
                lootImage.Position = new Vector2(i * 64, 0);

                var imageLoad = GD.Load<Texture2D>(_itemSprites[keys[i]]);
                lootImage.Texture = imageLoad;

                var label = lootImage.GetNode<Label>("Quantity");
                label.Text = values[i].ToString();

                _guiNode.AddLootItem(lootImage);
            }
        }
        #endregion

        #region Communication RPCs
        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void NotifyPlayer(string message)
        {
            _guiNode.Notify(message);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void RequestAddMessageToChat(string message)
        {
            RpcId(1, "RequestAddMessageToChat", message);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void AddMessageToChat(string username, string message)
        {
            _guiNode.AddChatMessage(username, message);
        }
        #endregion

        #region Authentication Methods
        public void SendLoginRequest(string username, string password)
        {
            RpcId(1, "ValidateLoginRequest", username, password);
        }

        public void SendRegistrationRequest(string username, string password)
        {
            RpcId(1, "ValidateRegistrationRequest", username, password);
        }
        #endregion

        #region UI Methods
        private void SetupChatBoxUI()
        {
            _guiNode.ShowChat();
            _guiNode.SetupChatForUser(_loginNode.Username);
        }

        private void SetupPlayerUI()
        {
            _loginNode.QueueFree();
            _guiNode.ShowPlayerHealth();
        }

        private void MakeGameWorldObjectsVisible()
        {
            _worldMapNode.Visible = true;
            _playersContainer.Visible = true;
            _resourcesContainer.Visible = true;
        }
        #endregion

        #region Utility Methods
        private PlayerCharacter GetLocalPlayer()
        {
            foreach (PlayerCharacter player in _playersContainer.GetChildren())
            {
                if (player is PlayerCharacter playerCharacter && playerCharacter.IsLocalPlayer)
                {
                    return playerCharacter;
                }
            }
            return null;
        }

        private Fight1v1 GetLocalPlayerFight()
        {
            var localPlayer = GetLocalPlayer();
            foreach (Node fight in _fightsContainer.GetChildren())
            {
                if (fight is Fight1v1 fight1v1)
                {
                    if (fight1v1.Player1Id == localPlayer.PeerID || fight1v1.Player2Id == localPlayer.PeerID)
                    {
                        return fight1v1;
                    }
                }
            }
            return null;
        }

        private PlayerCharacter GetPlayerById(int playerId)
        {
            foreach (Node player in _playersContainer.GetChildren())
            {
                if (player is PlayerCharacter playerCharacter && playerCharacter.PeerID == playerId)
                {
                    return playerCharacter;
                }
            }
            return null;
        }
        #endregion
    }
} 