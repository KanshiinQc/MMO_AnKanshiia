using Godot;
using CLIENT.Interfaces;
using CLIENT.Constants;

namespace CLIENT.Managers
{
    /// <summary>
    /// Handles all fight-related state changes and transitions
    /// </summary>
    public class FightStateManager : IFightStateManager
    {
        private readonly Node2D _worldMapNode;
        private readonly Node2D _fightMapNode;
        private readonly Node2D _resourcesContainer;
        private GUI _guiNode;
        private readonly Camera2D _camera;
        private readonly ILogger _logger;

        public FightStateManager(
            Node2D worldMap,
            Node2D fightMap,
            Node2D resources,
            GUI gui,
            Camera2D camera,
            ILogger logger)
        {
            _worldMapNode = worldMap;
            _fightMapNode = fightMap;
            _resourcesContainer = resources;
            _guiNode = gui;
            _camera = camera;
            _logger = logger;
        }

        public void UpdateGuiReference(GUI newGuiNode)
        {
            _guiNode = newGuiNode;
        }

        public void StartFight()
        {
            try
            {
                _logger.Log("Starting fight sequence");
                ToggleMaps();
                SetFightCameraZoom();
                HideResources();
                if (_guiNode != null)
                {
                    _guiNode.ShowFightTurns();
                }
                else
                {
                    _logger.LogWarning("GUI not initialized when starting fight");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error starting fight: {ex.Message}");
                throw;
            }
        }

        public void EndFight(Vector2 playerInitialPosition, PlayerCharacter player)
        {
            try
            {
                _logger.Log("Ending fight sequence");
                ToggleMaps();
                SetWorldCameraZoom();
                ShowResources();
                RepositionPlayer(playerInitialPosition, player);
                if (_guiNode != null)
                {
                    _guiNode.HideFightTurns();
                }
                else
                {
                    _logger.LogWarning("GUI not initialized when ending fight");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error ending fight: {ex.Message}");
                throw;
            }
        }

        private void ToggleMaps()
        {
            _worldMapNode.Visible = !_worldMapNode.Visible;
            _worldMapNode.SetProcess(!_worldMapNode.IsProcessing());
            _fightMapNode.Visible = !_fightMapNode.Visible;
            _fightMapNode.SetProcess(!_fightMapNode.IsProcessing());
        }

        private void SetFightCameraZoom() => 
            _camera.Zoom = new Vector2(GameConstants.UI.FIGHT_CAMERA_ZOOM, GameConstants.UI.FIGHT_CAMERA_ZOOM);

        private void SetWorldCameraZoom() => 
            _camera.Zoom = new Vector2(GameConstants.UI.WORLD_CAMERA_ZOOM, GameConstants.UI.WORLD_CAMERA_ZOOM);

        private void HideResources() => SetResourcesVisibility(false);
        private void ShowResources() => SetResourcesVisibility(true);

        private void SetResourcesVisibility(bool visible)
        {
            foreach (Node resource in _resourcesContainer.GetChildren())
            {
                if (resource is Ore ore)
                {
                    ore.Visible = visible;
                }
            }
        }

        private void RepositionPlayer(Vector2 position, PlayerCharacter player)
        {
            if (player == null)
            {
                _logger.LogWarning("Attempted to reposition null player");
                return;
            }
            player.Position = position;
        }
    }
} 