using Godot;
using System.Collections.Generic;
using System.Linq;
using CLIENT.Interfaces;
using CLIENT.Managers;
using CLIENT.Services;
using CLIENT.Constants;

namespace CLIENT
{
    /// <summary>
    /// Main server node that handles network communication and game state
    /// </summary>
    public partial class Server : Node
    {
        #region Variables - Dependencies
        private readonly ILogger _logger;
        private IFightStateManager _fightManager;
        private ILootSystem _lootSystem;
        private IConnectionManager _connectionManager;
        #endregion

        #region Variables - Networking
        private ENetMultiplayerPeer _network = new();
        private string _ip = GameConstants.Network.LOCAL_IP;
        private int _port = GameConstants.Network.PORT;
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

        public Server()
        {
            _logger = new GodotLogger();
        }

        #region Initialization
        public override void _Ready()
        {
            try
            {
                InitializeNodeReferences();
                InitializeDependencies();
                LoadItemSprites();
                LoadGUI();
                _connectionManager.Connect();
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error during initialization: {ex.Message}");
                throw;
            }
        }

        private void InitializeNodeReferences()
        {
            _playersContainer = GetNode<Node2D>("PlayersContainer");
            _resourcesContainer = GetNode<Node2D>("ResourcesContainer");
            _fightsContainer = GetNode<Node2D>("FightsContainer");
            _worldMapNode = GetNode<Node2D>("WorldMap");
            _fightMapNode = GetNode<Node2D>("FightMap");
        }

        private void InitializeDependencies()
        {
            _fightManager = new FightStateManager(
                _worldMapNode,
                _fightMapNode,
                _resourcesContainer,
                _guiNode,
                GetViewport().GetCamera2D(),
                _logger
            );

            _lootSystem = new LootSystem(
                _itemSprites,
                _guiNode,
                _logger
            );

            _connectionManager = new ConnectionManager(
                _network,
                _ip,
                _port,
                _guiNode,
                NotifyPlayer,
                _logger,
                this
            );
        }

        private void LoadItemSprites()
        {
            try
            {
                int number = 0;
                foreach (string filePath in DirAccess.GetFilesAt(GameConstants.ResourcePaths.ITEMS_SPRITES_PATH))
                {
                    if (filePath.GetExtension() == "png")
                    {
                        _itemSprites[number] = $"{GameConstants.ResourcePaths.ITEMS_SPRITES_PATH}/{filePath.GetFile()}";
                        number++;
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error loading item sprites: {ex.Message}");
                throw;
            }
        }

        private void LoadGUI()
        {
            try
            {
                var guiStart = GD.Load<PackedScene>(GameConstants.ResourcePaths.GUI_SCENE);
                _guiNode = guiStart.Instantiate() as GUI;
                GetTree().Root.CallDeferred("add_child", _guiNode, true);
                _loginNode = _guiNode.GetNode<Login>("Login");
                
                // Update all manager GUI references after initialization
                if (_connectionManager != null)
                {
                    _connectionManager.UpdateGuiReference(_guiNode);
                }
                else
                {
                    _logger.LogWarning("ConnectionManager not initialized when loading GUI");
                }

                if (_fightManager != null)
                {
                    _fightManager.UpdateGuiReference(_guiNode);
                }
                else
                {
                    _logger.LogWarning("FightManager not initialized when loading GUI");
                }

                if (_lootSystem != null)
                {
                    _lootSystem.UpdateGuiReference(_guiNode);
                }
                else
                {
                    _logger.LogWarning("LootSystem not initialized when loading GUI");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error loading GUI: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Authentication RPCs
        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void AutomaticallyConnectPeer(long playerId, string username)
        {
            try
            {
                if (_loginNode == null)
                {
                    CallDeferred("AutomaticallyConnectPeer", playerId, username);
                    return;
                }

                _loginNode.Username = username;
                _loginNode.Password = GameConstants.Network.DEFAULT_PASSWORD;

                _logger.Log($"Attempting automatic login for user: {username}");
                SendLoginRequest(username, GameConstants.Network.DEFAULT_PASSWORD);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error in automatic peer connection: {ex.Message}");
            }
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
            try
            {
                ConnectPlayer();
                MakeGameWorldObjectsVisible();
                SetupChatBoxUI();
                SetupPlayerUI();
                NotifyPlayer(message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error in login success: {ex.Message}");
            }
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
            try
            {
                _fightManager.StartFight();
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error starting fight: {ex.Message}");
            }
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
            try
            {
                var player = GetLocalPlayer();
                _fightManager.EndFight(playerInitialPosition, player);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error terminating fight: {ex.Message}");
            }
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void ShowLoot(Dictionary<int, int> lootDictionary)
        {
            try
            {
                _lootSystem.DisplayLoot(lootDictionary);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error showing loot: {ex.Message}");
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
            _logger.LogWarning("Local player not found");
            return null;
        }

        private Fight1v1 GetLocalPlayerFight()
        {
            var localPlayer = GetLocalPlayer();
            if (localPlayer == null) return null;

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
            _logger.LogWarning($"Player with ID {playerId} not found");
            return null;
        }
        #endregion
    }
} 