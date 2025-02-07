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
        private Node2D _mobsContainer;
        private Node2D _worldMapNode;
        private CharacterBody2D _playerNode;
        #endregion

        #region Variables - UI
        private GUI _guiNode;
        private Login _loginNode;
        #endregion

        #region Variables - Game Data
        private Dictionary<int, string> _itemSprites = new();
        private int[] _currentFightParticipants;  // Store current fight participants
        #endregion

        #region Initialization
        public override void _Ready()
        {
            _playersContainer = GetNode<Node2D>("PlayersContainer");
            _resourcesContainer = GetNode<Node2D>("ResourcesContainer");
            _fightsContainer = GetNode<Node2D>("FightsContainer");
            _mobsContainer = GetNode<Node2D>("MobsContainer");
            _worldMapNode = GetNode<Node2D>("WorldMap");

            LoadItemSprites();
            LoadGUI();
            ConnectToServer();

            Multiplayer.ConnectedToServer += OnConnectionSucceeded;
            Multiplayer.ConnectionFailed += OnConnectionFailed;
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
            GD.Print($"Attempting to connect to {_ip}:{_port}");

            var error = _network.CreateClient(_ip, _port);
            if(error is not Error.Ok)
            {
                return;
            }

            Multiplayer.MultiplayerPeer = _network;
        }

        private void OnConnectionFailed()
        {
            _guiNode.HideLoading();
            NotifyPlayer("Could not connect to server. Is it down?");
        }

        private void OnConnectionSucceeded()
        {
            _guiNode.HideLoading();
            NotifyPlayer("Connected to server successfully. You can now log in");
        }
        #endregion

        #region Authentication RPCs
        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void TriggerPlayerAutoLogin(long playerId, string username)
        {
            string password = "defaultpass123";
            if (_loginNode == null)
            {
                CallDeferred("TriggerPlayerAutoLogin", playerId, username);
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
        public void RemovePlayerFromWorld(int playerId)
        {
            

        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void AddPlayerToWorld(int playerId)
        {
            var player = GetPlayerById(playerId);
            if (player != null)
            {
                player.Visible = true;
                player.SetProcess(true);
                player.SetProcessInput(true);
                
                // Only try to access Area2D if it exists
                if (player.HasNode("Area2D"))
                {
                    var area2D = player.GetNode<Area2D>("Area2D");
                    if (area2D != null)
                    {
                        area2D.Monitorable = true;
                        area2D.Monitoring = true;
                    }
                }
            }
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void StartFightWithPlayer(int[] fightParticipantIds)
        {
            _currentFightParticipants = fightParticipantIds;
            GD.Print($"StartFightWithPlayer: Received participants: [{string.Join(", ", fightParticipantIds)}]");
            
            var localPlayer = GetLocalPlayer();
            GD.Print($"Starting fight for local player: ID {localPlayer?.PeerID}, IsLocal: {localPlayer?.IsLocalPlayer}");

            // Print all fights in container before starting
            var actualFightCount = _fightsContainer.GetChildren().Count(n => n is FightMap);
            GD.Print($"Current actual fights in container before starting: {actualFightCount}");
            foreach (Node existingFight in _fightsContainer.GetChildren())
            {
                if (existingFight is FightMap fight1v1)
                {
                    GD.Print($"Existing fight - P1: {fight1v1.Player1Id}, P2: {fight1v1.Player2Id}");
                }
                else
                {
                    GD.Print($"Found non-fight node in container: {existingFight.GetType()}");
                }
            }

            if (localPlayer != null)
            {
                // Instead of hiding the local player, just disable processing
                localPlayer.SetProcess(false);
                // Keep the player visible
                localPlayer.Visible = true;
                GD.Print($"Disabled local player processing: {localPlayer.Username}");
            }
            else
            {
                GD.PrintErr("Could not find local player when starting fight!");
                return;
            }

            // Hide all players not in the fight
            foreach (Node node in _playersContainer.GetChildren())
            {
                if (node is not PlayerCharacter playerCharacter)
                    continue;

                // If the player is not in the fight participants list, hide them
                if (!fightParticipantIds.Contains((int)playerCharacter.PeerID))
                {
                    playerCharacter.Visible = false;
                }
            }

            // Switch to fight map
            _playersContainer.SetProcess(false);
            _playersContainer.Visible = false;

            _worldMapNode.Visible = false;
            _worldMapNode.SetProcess(false);
            
            // Adjust camera for fight scene
            GetViewport().GetCamera2D().Zoom = new Vector2(3, 3);

            // Hide world resources during fight
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
            GD.Print($"TerminateFight: Returning player to position {playerInitialPosition}");
            
            var localPlayer = GetLocalPlayer();
            if (localPlayer != null)
            {
                // Re-enable processing of the local player and restore position
                localPlayer.SetProcess(true);
                localPlayer.SetProcessInput(true);
                localPlayer.Visible = true;
                localPlayer.Position = playerInitialPosition;
                GD.Print($"TerminateFight: Restored player {localPlayer.Username} to position {playerInitialPosition}");
            }
            else
            {
                GD.PrintErr("TerminateFight: Could not find local player!");
            }

            // Switch back to world map
            _worldMapNode.Visible = true;
            _worldMapNode.SetProcess(true);
            
            // Restore world camera zoom
            GetViewport().GetCamera2D().Zoom = new Vector2(4, 4);

            // Show all players again
            foreach (Node node in _playersContainer.GetChildren())
            {
                if (node is PlayerCharacter playerCharacter)
                {
                    playerCharacter.Visible = true;
                }
            }

            // Show world resources again
            foreach (Node resource in _resourcesContainer.GetChildren())
            {
                if (resource is Ore ore)
                {
                    ore.Visible = true;
                }
            }
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

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void EndFight(int fightId)
        {
            GD.Print($"EndFight: Ending fight with ID {fightId}");
            
            // Find and remove the fight instance from the container
            foreach (Node node in _fightsContainer.GetChildren())
            {
                if (node is FightMap fight && (int)fight.GetInstanceId() == fightId)
                {
                    GD.Print($"EndFight: Found fight instance to remove - P1: {fight.Player1Id}, P2: {fight.Player2Id}");
                    fight.QueueFree();
                    break;
                }
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
            _mobsContainer.Visible = true;
        }
        #endregion

        #region Utility Methods
        private PlayerCharacter GetLocalPlayer()
        {
            //// CREATE A METHOD TO GET THE LOCAL PLAYER FROM HIS CURRENT FIGHT... THEN KEEP THIS ONE GENERIC
            //var playerList = GetNode<Node2D>("playersContainer");
            //var playerList2 = GetNode<Node2D>("playersContainer").GetChildren();

            //var test2 = playerList.Where(c => c.GetType() == typeof(PlayerCharacter)) as List<PlayerCharacter>;

            //return test2.Find(pl => pl.PeerID == Multiplayer.GetUniqueId());

            ////var localPeerId = Multiplayer.GetUniqueId();
            ////GD.Print($"Looking for local player with peer ID: {localPeerId}");

            ////foreach (Node node in _playersContainer.GetChildren())
            ////{
            ////    // Skip non-PlayerCharacter nodes
            ////    if (node is not PlayerCharacter playerCharacter)
            ////    {
            ////        GD.Print($"Skipping non-player node: {node.Name}");
            ////        continue;
            ////    }

            ////    GD.Print($"Checking player: ID {playerCharacter.PeerID}, IsLocal: {playerCharacter.IsLocalPlayer}");
                
            ////    // Check both the IsLocalPlayer flag and the peer ID
            ////    if (playerCharacter.IsLocalPlayer || playerCharacter.PeerID == localPeerId)
            ////    {
            ////        GD.Print($"Found local player: {playerCharacter.Username} (ID: {playerCharacter.PeerID})");
            ////        return playerCharacter;
            ////    }
            ////}
            
            ////GD.PrintErr($"No local player found among {_playersContainer.GetChildCount()} players");
            return null;
        }

        private FightMap GetLocalPlayerFight()
        {
            var localPlayer = GetLocalPlayer();
            if (localPlayer == null) 
            {
                GD.PrintErr("GetLocalPlayerFight: Local player is null");
                return null;
            }

            GD.Print($"GetLocalPlayerFight: Looking for fight with local player ID: {localPlayer.PeerID}");
            GD.Print($"GetLocalPlayerFight: Number of fights in container: {_fightsContainer.GetChildCount()}");
            
            foreach (FightMap fight in _fightsContainer.GetChildren().Where(children => children.GetType() == typeof(FightMap)))
            {
                GD.Print($"fight: Checking fight - P1: {fight.Player1Id}, P2: {fight.Player2Id}");
                if (fight.Player1Id == localPlayer.PeerID || fight.Player2Id == localPlayer.PeerID)
                {
                    GD.Print($"GetLocalPlayerFight: Found fight for local player!");
                    return fight;
                }
            }
            
            GD.PrintErr($"GetLocalPlayerFight: No fight found for player {localPlayer.Username} (ID: {localPlayer.PeerID})");
            return null;
        }

        private PlayerCharacter GetPlayerById(int playerId)
        {
            foreach (Node node in _playersContainer.GetChildren())
            {
                // Skip non-PlayerCharacter nodes
                if (node is not PlayerCharacter playerCharacter)
                    continue;

                if (playerCharacter.PeerID == playerId)
                {
                    return playerCharacter;
                }
            }
            return null;
        }
        #endregion

        #region Mob Management
        public void RequestSpawnMob(Vector2 position)
        {
            RpcId(1, "SpawnMob", position);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        public void SpawnMob(Vector2 position)
        {
            // Client doesn't need to implement the logic as MultiplayerSpawner handles it
        }
        #endregion

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey eventKey)
            {
                if (eventKey.Pressed && eventKey.Keycode == Key.S)
                {
                    // Generate random position and spawn mob
                    var random = new RandomNumberGenerator();
                    random.Randomize();
                    float x = random.RandfRange(10, 200);
                    float y = random.RandfRange(10, 200);
                    RequestSpawnMob(new Vector2(x, y));
                }
            }
        }
    }
} 