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
        private Node2D _playersContainer;
        private Node2D _resourcesContainer;
        private Node2D _fightsContainer;
        private Node2D _mobsContainer;
        private Node2D _worldMap;
        #endregion

        #region Variables - Scenes
        private PackedScene _playerScene;
        private PackedScene _worldMapScene;
        private PackedScene _oreScene;
        private PackedScene _fightScene;
        private PackedScene _mobScene;
        #endregion

        #region Variables - Player Usernames
        private Dictionary<long, string> _playerUsernames = new();
        private HashSet<string> _usedUsernames = new HashSet<string>();
        #endregion

        #region Server Setup & Start
        public Server()
        {
            _service = new FodusService();
            _network = new ENetMultiplayerPeer();

            _playerScene = GD.Load<PackedScene>("res://Player/PlayerCharacter.tscn");
            _worldMapScene = GD.Load<PackedScene>("res://Maps/WorldMap.tscn");
            _oreScene = GD.Load<PackedScene>("res://Mining/Ore.tscn");
            _fightScene = GD.Load<PackedScene>("res://Fight/Fight1v1.tscn");
            _mobScene = GD.Load<PackedScene>("res://Mobs/Mob.tscn");

            _network.PeerConnected += OnPeerConnected;
            _network.PeerDisconnected += OnPeerDisconnected;
        }

        public override void _Ready()
        {
            _playersContainer = GetNode<Node2D>("PlayersContainer");
            _resourcesContainer = GetNode<Node2D>("ResourcesContainer");
            _fightsContainer = GetNode<Node2D>("FightsContainer");
            _mobsContainer = GetNode<Node2D>("MobsContainer");
            _worldMap = _worldMapScene.Instantiate() as Node2D;
            _worldMap.Visible = false;

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

            // List of predefined usernames
            var availableNames = new[] { "Paul", "Jonathan", "Michael", "Blanche" };
            
            // Find the first unused name
            username = availableNames.FirstOrDefault(name => !_usedUsernames.Contains(name));
            
            // If all predefined names are used, create a generic one
            if (username == null)
            {
                username = $"Player{_connectedPlayersCount + 1}";
            }

            // Mark the username as used
            _usedUsernames.Add(username);
            
            // Store the username for this player ID
            _playerUsernames[playerId] = username;

            GD.Print($"Player {playerId} connected with username: {username}");
            
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
                // Free up the username when player disconnects
                if (_playerUsernames.TryGetValue(playerId, out string username))
                {
                    _usedUsernames.Remove(username);
                    _playerUsernames.Remove(playerId);
                }

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
        public void RequestFightWithPlayer(int targetPlayerId)
        {
            var requesterId = (int)Multiplayer.GetRemoteSenderId();
            var requesterPlayer = GetPlayerById(requesterId);
            var targetPlayer = GetPlayerById(targetPlayerId);

            GD.Print($"Fight requested: Requester ID {requesterId} ({requesterPlayer?.Username}), Target ID {targetPlayerId} ({targetPlayer?.Username})");

            // Check if players exist
            if (requesterPlayer == null || targetPlayer == null)
            {
                GD.PrintErr($"Could not find one of the players for fight. Requester: {requesterId}, Target: {targetPlayerId}");
                RpcId(requesterId, "NotifyPlayer", "Could not start fight - player not found");
                return;
            }

            // Check if either player is already in a fight
            if (requesterPlayer.IsFighting || targetPlayer.IsFighting)
            {
                RpcId(requesterId, "NotifyPlayer", "One of the players is already in a fight!");
                return;
            }

            // Create a new fight instance using the scene
            var fightInstance = _fightScene.Instantiate<Fight1v1>();
            fightInstance.Player1Id = requesterId;
            fightInstance.Player2Id = targetPlayerId;
            fightInstance.Player1PositionBeforeFight = requesterPlayer.Position;
            fightInstance.Player2PositionBeforeFight = targetPlayer.Position;
            
            _fightInstances.Add(fightInstance);
            _fightsContainer.AddChild(fightInstance, true);

            // Set both players' fighting status
            requesterPlayer.IsFighting = true;
            targetPlayer.IsFighting = true;

            // Notify all other players that these players are in a fight
            foreach (var player in _connectedPeers)
            {
                if (player.PeerID != requesterId && player.PeerID != targetPlayerId)
                {
                    RpcId(player.PeerID, "RemovePlayerFromWorld", requesterId);
                    RpcId(player.PeerID, "RemovePlayerFromWorld", targetPlayerId);
                }
            }

            // Send fight participant information to both players
            RpcId(requesterId, "StartFightWithPlayer", new int[] { requesterId, targetPlayerId });
            RpcId(targetPlayerId, "StartFightWithPlayer", new int[] { requesterId, targetPlayerId });

            // Notify both players
            RpcId(requesterId, "NotifyPlayer", $"Starting fight with {targetPlayer.Username}!");
            RpcId(targetPlayerId, "NotifyPlayer", $"{requesterPlayer.Username} has challenged you to a fight!");

            // Set initial turn
            string firstPlayerName = requesterPlayer.Username;
            RpcId(requesterId, "UpdateFightTurn", firstPlayerName);
            RpcId(targetPlayerId, "UpdateFightTurn", firstPlayerName);

            // Create a timer to end the fight after 3 seconds
            var timer = new Timer();
            AddChild(timer);
            timer.WaitTime = 3.0;
            timer.OneShot = true;
            timer.Timeout += () =>
            {
                GD.Print($"Server timer: Ending fight between {requesterPlayer.Username} and {targetPlayer.Username}");
                var fightId = (int)fightInstance.GetInstanceId();
                GD.Print($"Server timer: Fight ID is {fightId}");
                EndFight(fightId);
                timer.QueueFree();
            };
            timer.Start();
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void RemovePlayerFromWorld(int playerId) { }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void AddPlayerToWorld(int playerId) { }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void StartFightWithPlayer(int[] fightParticipantIds)
        {
            // ... existing implementation ...
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void AttackTile(float positionX, float positionY) { }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void UpdateFightTurn(string username) { }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void TerminateFight(Vector2 playerInitialPosition) { }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void ShowLoot(Dictionary<int, int> lootDictionary) { }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void EndFight(int fightId)
        {
            GD.Print($"Server EndFight: Looking for fight with ID {fightId}");
            
            // Find the fight by its instance ID
            var fight = _fightInstances.FirstOrDefault(f => (int)f.GetInstanceId() == fightId);
            
            if (fight == null)
            {
                GD.PrintErr($"Server EndFight: No fight found with ID {fightId}");
                return;
            }

            GD.Print($"Server EndFight: Found fight - P1: {fight.Player1Id}, P2: {fight.Player2Id}");
            
            var player1 = GetPlayerById(fight.Player1Id);
            var player2 = GetPlayerById(fight.Player2Id);

            // Re-enable world map processing for both players
            player1?.SetProcess(true);
            player2?.SetProcess(true);

            // Reset fighting status
            if (player1 != null) player1.IsFighting = false;
            if (player2 != null) player2.IsFighting = false;

            // Notify all other players that these players are back
            foreach (var player in _connectedPeers)
            {
                if (player.PeerID != fight.Player1Id && player.PeerID != fight.Player2Id)
                {
                    RpcId(player.PeerID, "AddPlayerToWorld", fight.Player1Id);
                    RpcId(player.PeerID, "AddPlayerToWorld", fight.Player2Id);
                }
            }

            // Return players to their original positions
            RpcId(fight.Player1Id, "TerminateFight", fight.Player1PositionBeforeFight);
            RpcId(fight.Player2Id, "TerminateFight", fight.Player2PositionBeforeFight);

            // Tell all clients to clean up the fight instance
            foreach (var player in _connectedPeers)
            {
                RpcId(player.PeerID, "EndFight", fightId);
            }

            // Clean up the fight instance on the server
            _fightInstances.Remove(fight);
            fight.QueueFree();
        }
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

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void SpawnMob(Vector2 position)
        {
            var mobInstance = _mobScene.Instantiate<Node2D>();
            mobInstance.Position = position;
            _mobsContainer.AddChild(mobInstance, true);
        }
    }
} 