using Godot;
using System.Collections.Generic;
using System.Linq;
using SERVER.Services;
using SERVER.Helpers;
using System;

namespace SERVER
{
    public partial class Server : Node
    {
        #region Services
        private readonly FodusService _service;
        #endregion

        #region Variables - Networking
        private ENetMultiplayerPeer _network;
        private const int PORT = 1909;
        private const int MAX_PLAYERS = 100;
        private int _connectedPlayersCount;
        #endregion

        #region Variables - Players & Fights Lists
        private List<PlayerCharacter> _connectedPeers = new();
        private List<Fight1v1> _fightInstances = new();
        #endregion

        #region Variables - NodeContainers
        private Node _playersContainer;
        private Node _resourcesContainer;
        private Node _fightsContainer;
        private Node _worldMap;
        #endregion

        #region Variables - Scenes
        private PackedScene _playerScene;
        private PackedScene _worldMapScene;
        private PackedScene _oreScene;
        #endregion

        #region Variables - Player Usernames
        private Dictionary<long, string> _playerUsernames = new();
        #endregion

        #region Server Setup & Start
        public Server()
        {
            _service = new FodusService();
            _network = new ENetMultiplayerPeer();

            _playerScene = GD.Load<PackedScene>("res://Player/PlayerCharacter.tscn");
            _worldMapScene = GD.Load<PackedScene>("res://Maps/WorldMap.tscn");
            _oreScene = GD.Load<PackedScene>("res://Mining/Ore.tscn");

            _network.PeerConnected += OnPeerConnected;
            _network.PeerDisconnected += OnPeerDisconnected;
        }

        public override void _Ready()
        {
            _playersContainer = GetNode<Node>("PlayersContainer");
            _resourcesContainer = GetNode<Node>("ResourcesContainer");
            _fightsContainer = GetNode<Node>("FightsContainer");

            StartServer();
        }

        private void StartServer()
        {
            CreateServer();
            SpawnWorldMap();
            SpawnResources();
            GD.Print("Server Started");
        }

        private void CreateServer()
        {
            var error = _network.CreateServer(PORT, MAX_PLAYERS);
            if (error != Error.Ok)
            {
                GD.PrintErr($"Failed to create server: {error}");
                return;
            }
            Multiplayer.MultiplayerPeer = _network;
            GD.Print($"Server listening on port {PORT}");
        }

        private void SpawnWorldMap()
        {
            _worldMap = _worldMapScene.Instantiate();
            AddChild(_worldMap, true);
        }

        private void SpawnResources()
        {
            const int startingX = 16;
            const int startingY = 4;
            const int spaceBetween = 32;

            for (int n = 0; n < 4; n++)
            {
                for (int m = 0; m < 4; m++)
                {
                    if (m % 2 == 1)
                    {
                        var oreInstance = _oreScene.Instantiate<Node2D>();
                        oreInstance.Position = new Vector2(startingX + (n * spaceBetween), startingY + (m * spaceBetween));
                        _resourcesContainer.AddChild(oreInstance, true);
                    }
                }
            }
        }

        #endregion

        #region Player Connections Handling
        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void AutomaticallyConnectPeer(long playerId, string username)
        {
            string password = "defaultpass123";
            
            // Create the test account if it doesn't exist
            if (!_service.IsUsernameAvailable(username))
            {
                RpcId(playerId, "AutomaticallyConnectPeer", playerId, username);
            }
            else 
            {
                _service.CreateUser(username, password);
                RpcId(playerId, "AutomaticallyConnectPeer", playerId, username);
            }
        }

        private void OnPeerConnected(long playerId)
        {
            var test = _network.GetPeer((int)playerId);
            string username;
            if (_connectedPlayersCount == 0)
                username = "Paul";
            else if (_connectedPlayersCount == 1)
                username = "Jonathan";
            else if (_connectedPlayersCount == 2)
                username = "Michael";
            else if (_connectedPlayersCount == 3)
                username = "Blanche";
            else
                username = $"Player{_connectedPlayersCount + 1}";

            // Use CallDeferred to ensure the peer is fully registered before sending RPCs
            CallDeferred("DeferredAutomaticConnect", playerId, username);
        }

        private void DeferredAutomaticConnect(long playerId, string username)
        {
            string password = "defaultpass123";
            
            // Create the test account if it doesn't exist
            if (!_service.IsUsernameAvailable(username))
            {
                RpcId(playerId, "AutomaticallyConnectPeer", playerId, username);
            }
            else 
            {
                _service.CreateUser(username, password);
                RpcId(playerId, "AutomaticallyConnectPeer", playerId, username);
            }
            
            _connectedPlayersCount++;
        }

        private void OnPeerDisconnected(long playerId)
        {
            _connectedPlayersCount--;
            var player = _playersContainer.GetNode<PlayerCharacter>(playerId.ToString());
            if (player != null)
            {
                _service.SaveUserData(player);
                _connectedPeers.Remove(player);
                player.QueueFree();
            }
            GD.Print($"User {playerId} Disconnected");
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void ConnectPlayer()
        {
            var playerId = Multiplayer.GetRemoteSenderId();
            
            // Get the username that was used during login
            string username = _playerUsernames.GetValueOrDefault(playerId, $"Player{_connectedPlayersCount + 1}");
            
            var user = _service.GetUserByUsername(username);
            var playerInstance = PlayerHelper.ConstructPlayerInstance(playerId, user, _playerScene);
            _connectedPeers.Add(playerInstance);
            _playersContainer.AddChild(playerInstance, true);
        }

        #region Player Authentication
        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void ValidateLoginRequest(string username, string password)
        {
            var playerId = Multiplayer.GetRemoteSenderId();

            if (true/*_service.ValidateCredentials(username, password)*/)
            {
                // Store the username for this player
                _playerUsernames[playerId] = username;
                RpcId(playerId, "LoginSuccess", "Login successful!");
            }
            else
            {
                RpcId(playerId, "LoginFailed", "Invalid username or password");
            }
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void ValidateRegistrationRequest(string username, string password)
        {
            var playerId = Multiplayer.GetRemoteSenderId();

            if (username.Length < 8)
            {
                RpcId(playerId, "RegistrationFailed", "Username must be at least 8 characters");
                return;
            }
            
            if (password.Length < 8)
            {
                RpcId(playerId, "RegistrationFailed", "Password must be at least 8 characters");
                return;
            }

            if (!_service.IsUsernameAvailable(username))
            {
                RpcId(playerId, "RegistrationFailed", "Username already exists");
                return;
            }

            _service.CreateUser(username, password);
            RpcId(playerId, "RegistrationSuccess", "Account created successfully");
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void LoginSuccess(string message) { }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void LoginFailed(string message) { }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void RegistrationSuccess(string message) { }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void RegistrationFailed(string message) { }
        #endregion

        #endregion

        #region Player Actions
        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void MovePlayer(float xPos, float yPos)
        {
            var playerId = Multiplayer.GetRemoteSenderId();
            var player = GetPlayerById(playerId);
            
            if (player != null && !player.IsFighting)
            {
                var newPosition = new Vector2(xPos, yPos);
                player.SetTargetPosition(newPosition);
            }
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void MineOre(int networkUID) 
        {
            var playerId = (int)Multiplayer.GetRemoteSenderId();
            var oreToMine = _resourcesContainer.GetNode<Ore>(networkUID.ToString());
            
            if (oreToMine.TryMine())
            {
                var quantity = GD.RandRange(10, 30);
                _service.AddItemToPlayer(playerId, 0, quantity);
                RpcId(playerId, "NotifyPlayer", $"{quantity} ores added to inventory");
            }
            else
            {
                RpcId(playerId, "NotifyPlayer", "Cannot mine this ore yet..");
            }
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void SendKeyInput(Vector2 velocity) 
        {
            var player = GetPlayerById((int)Multiplayer.GetRemoteSenderId());
            player.Velocity = velocity;
        }

        #endregion
        
        #region Fights
        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void RequestFightWithPlayer(int playerId) { }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void StartFightWithPlayer() { }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void AttackTile(float positionX, float positionY) { }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void UpdateFightTurn(string username) { }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void TerminateFight(Vector2 playerInitialPosition) { }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void ShowLoot(Dictionary<int, int> lootDictionary) { }
        #endregion

        #region Chat & Notifications
        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void NotifyPlayer(string message) { }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void RequestAddMessageToChat(string message)
        {
            var requesterId = (int)Multiplayer.GetRemoteSenderId();
            var requesterPlayer = GetPlayerById(requesterId);
            foreach (var player in _connectedPeers.Where(p => p.PeerID != requesterId))
            {
                RpcId(player.PeerID, "AddMessageToChat", requesterPlayer.Username, message);
            }
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void AddMessageToChat(string username, string message) { }
        #endregion

        #region Utilities
        private Fight1v1 GetFightInstanceByPlayerId(int playerId)
        {
            return _fightInstances.FirstOrDefault(fight => 
                fight.Player1Id == playerId || fight.Player2Id == playerId);
        }

        private PlayerCharacter GetPlayerById(int playerId)
        {
            return _connectedPeers.FirstOrDefault(player => player.PeerID == playerId);
        }
        #endregion
    }
} 