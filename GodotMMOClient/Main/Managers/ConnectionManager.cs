using Godot;
using CLIENT.Interfaces;
using CLIENT.Constants;
using System;

namespace CLIENT.Managers
{
    /// <summary>
    /// Manages network connections and related events
    /// </summary>
    public class ConnectionManager : IConnectionManager
    {
        private readonly ENetMultiplayerPeer _network;
        private readonly string _ip;
        private readonly int _port;
        private GUI _guiNode;
        private readonly Action<string> _notifyPlayer;
        private readonly ILogger _logger;
        private readonly Node _nodeContext;

        public bool IsConnected { get; private set; }

        public ConnectionManager(
            ENetMultiplayerPeer network,
            string ip,
            int port,
            GUI guiNode,
            Action<string> notifyPlayer,
            ILogger logger,
            Node nodeContext)
        {
            _network = network;
            _ip = ip;
            _port = port;
            _guiNode = guiNode;
            _notifyPlayer = notifyPlayer;
            _logger = logger;
            _nodeContext = nodeContext;
            IsConnected = false;
        }

        public void UpdateGuiReference(GUI newGuiNode)
        {
            _guiNode = newGuiNode;
        }

        public void Connect()
        {
            try
            {
                _logger.Log($"Attempting to connect to {_ip}:{_port}");
                _notifyPlayer(GameConstants.Messages.CONNECTING);
                
                var error = _network.CreateClient(_ip, _port);
                if (error != Error.Ok)
                {
                    HandleConnectionError(error);
                    return;
                }
                
                SetupMultiplayerConnection();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Connection error: {ex.Message}");
                HandleConnectionError(Error.Failed);
            }
        }

        private void HandleConnectionError(Error error)
        {
            IsConnected = false;
            _logger.LogError($"Failed to create client: {error}");
            _nodeContext.CallDeferred(nameof(OnConnectionFailed));
        }

        private void SetupMultiplayerConnection()
        {
            try
            {
                _nodeContext.Multiplayer.MultiplayerPeer = _network;
                _nodeContext.Multiplayer.ConnectedToServer += OnConnectionSucceeded;
                _nodeContext.Multiplayer.ConnectionFailed += OnConnectionFailed;
                _logger.Log($"Connection setup complete, waiting for server response");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error setting up multiplayer connection: {ex.Message}");
                HandleConnectionError(Error.Failed);
            }
        }

        private void OnConnectionFailed()
        {
            IsConnected = false;
            if (_guiNode != null)
            {
                _guiNode.HideLoading();
            }
            else
            {
                _logger.LogWarning("GUI not initialized when handling connection failure");
            }
            _notifyPlayer(GameConstants.Messages.CONNECTION_FAILED);
            _logger.LogWarning("Connection to server failed");
        }

        private void OnConnectionSucceeded()
        {
            IsConnected = true;
            if (_guiNode != null)
            {
                _guiNode.HideLoading();
            }
            else
            {
                _logger.LogWarning("GUI not initialized when handling connection success");
            }
            _notifyPlayer(GameConstants.Messages.CONNECTION_SUCCESS);
            _logger.Log("Successfully connected to server");
        }
    }
} 