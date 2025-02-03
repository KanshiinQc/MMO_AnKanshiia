using Godot;
using CLIENT.Interfaces;
using CLIENT.Constants;
using System.Collections.Generic;

namespace CLIENT.Managers
{
    /// <summary>
    /// Handles the display and management of loot items in the game
    /// </summary>
    public class LootSystem : ILootSystem
    {
        private readonly Dictionary<int, string> _itemSprites;
        private GUI _guiNode;
        private readonly ILogger _logger;

        public LootSystem(Dictionary<int, string> itemSprites, GUI guiNode, ILogger logger)
        {
            _itemSprites = itemSprites;
            _guiNode = guiNode;
            _logger = logger;
        }

        public void UpdateGuiReference(GUI newGuiNode)
        {
            _guiNode = newGuiNode;
        }

        public void DisplayLoot(Dictionary<int, int> lootDictionary)
        {
            try
            {
                if (_guiNode == null)
                {
                    _logger.LogWarning("Cannot display loot: GUI not initialized");
                    return;
                }

                _logger.Log($"Displaying loot with {lootDictionary.Count} items");
                _guiNode.ShowLootWindow();
                
                foreach (var (itemId, quantity) in lootDictionary)
                {
                    CreateAndDisplayLootItem(itemId, quantity);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error displaying loot: {ex.Message}");
                throw;
            }
        }

        private void CreateAndDisplayLootItem(int itemId, int quantity)
        {
            try
            {
                var lootImage = CreateLootSprite(itemId);
                if (lootImage != null)
                {
                    AddQuantityLabel(lootImage, quantity);
                    _guiNode.AddLootItem(lootImage);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error creating loot item {itemId}: {ex.Message}");
                throw;
            }
        }

        private Sprite2D CreateLootSprite(int itemId)
        {
            if (!_itemSprites.ContainsKey(itemId))
            {
                _logger.LogWarning($"No sprite found for item ID: {itemId}");
                return null;
            }

            var itemUIScene = GD.Load<PackedScene>(GameConstants.ResourcePaths.ITEM_UI_SCENE);
            var lootImage = itemUIScene.Instantiate<Sprite2D>();
            lootImage.Scale = new Vector2(GameConstants.UI.LOOT_ITEM_SCALE, GameConstants.UI.LOOT_ITEM_SCALE);
            lootImage.Position = new Vector2(itemId * GameConstants.UI.LOOT_ITEM_SPACING, 0);
            lootImage.Texture = GD.Load<Texture2D>(_itemSprites[itemId]);
            
            return lootImage;
        }

        private void AddQuantityLabel(Sprite2D sprite, int quantity)
        {
            if (sprite == null) return;

            var label = sprite.GetNode<Label>("Quantity");
            if (label != null)
            {
                label.Text = quantity.ToString();
            }
            else
            {
                _logger.LogWarning("Quantity label not found on loot sprite");
            }
        }
    }
} 