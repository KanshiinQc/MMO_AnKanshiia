using Godot;
using System.Collections.Generic;

namespace CLIENT.Interfaces
{
    /// <summary>
    /// Base interface for all managers that require access to the game's GUI.
    /// Provides a standard way to update GUI references when they change or are initialized.
    /// </summary>
    /// <remarks>
    /// This interface is implemented by managers that need to interact with the GUI,
    /// ensuring consistent GUI reference management across the application.
    /// </remarks>
    public interface IGuiDependent
    {
        /// <summary>
        /// Updates the manager's reference to the GUI node.
        /// </summary>
        /// <param name="guiNode">The new GUI reference to be used by the manager.</param>
        /// <remarks>
        /// This method should be called whenever the GUI is initialized or changed,
        /// ensuring the manager always has access to the current GUI instance.
        /// </remarks>
        void UpdateGuiReference(GUI guiNode);
    }

    /// <summary>
    /// Manages the state and transitions of fight sequences in the game.
    /// Handles the visibility and state of maps, resources, and UI elements during fights.
    /// </summary>
    /// <remarks>
    /// This manager is responsible for:
    /// - Transitioning between world and fight maps
    /// - Managing resource visibility during fights
    /// - Handling fight-related UI elements
    /// - Managing camera zoom levels for different fight states
    /// </remarks>
    public interface IFightStateManager : IGuiDependent
    {
        /// <summary>
        /// Initiates a fight sequence, handling all necessary state transitions.
        /// </summary>
        /// <remarks>
        /// This method:
        /// - Toggles map visibility
        /// - Adjusts camera zoom
        /// - Hides resources
        /// - Shows fight UI elements
        /// </remarks>
        void StartFight();

        /// <summary>
        /// Ends a fight sequence and returns players to their original positions.
        /// </summary>
        /// <param name="playerInitialPosition">The position to return the player to after the fight.</param>
        /// <param name="player">The player character to reposition.</param>
        /// <remarks>
        /// This method:
        /// - Restores map visibility
        /// - Resets camera zoom
        /// - Shows resources
        /// - Hides fight UI elements
        /// - Repositions the player
        /// </remarks>
        void EndFight(Vector2 playerInitialPosition, PlayerCharacter player);
    }

    /// <summary>
    /// Manages the display and handling of loot items in the game.
    /// Responsible for creating and displaying item UI elements when loot is received.
    /// </summary>
    /// <remarks>
    /// This manager handles:
    /// - Loot window display
    /// - Item sprite loading and display
    /// - Quantity label management
    /// - Layout of loot items in the UI
    /// </remarks>
    public interface ILootSystem : IGuiDependent
    {
        /// <summary>
        /// Displays a collection of loot items to the player.
        /// </summary>
        /// <param name="lootDictionary">Dictionary mapping item IDs to their quantities.</param>
        /// <remarks>
        /// For each item in the dictionary:
        /// - Creates a sprite with the correct texture
        /// - Sets the appropriate scale and position
        /// - Adds a quantity label
        /// - Adds the item to the loot window
        /// </remarks>
        void DisplayLoot(Dictionary<int, int> lootDictionary);
    }

    /// <summary>
    /// Manages network connections and related events for the game client.
    /// Handles connection establishment, monitoring, and state management.
    /// </summary>
    /// <remarks>
    /// This manager is responsible for:
    /// - Establishing connection to the server
    /// - Monitoring connection status
    /// - Handling connection events (success/failure)
    /// - Managing connection-related UI feedback
    /// </remarks>
    public interface IConnectionManager : IGuiDependent
    {
        /// <summary>
        /// Initiates a connection to the game server.
        /// </summary>
        /// <remarks>
        /// This method:
        /// - Attempts to establish a network connection
        /// - Updates UI loading state
        /// - Handles connection errors
        /// - Sets up network event handlers
        /// </remarks>
        void Connect();

        /// <summary>
        /// Gets the current connection status.
        /// </summary>
        /// <value>True if connected to the server, false otherwise.</value>
        bool IsConnected { get; }
    }
} 