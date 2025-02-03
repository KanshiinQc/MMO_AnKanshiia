namespace CLIENT.Constants
{
    /// <summary>
    /// Contains all constant values used throughout the game.
    /// Centralizes configuration and magic values for better maintenance.
    /// </summary>
    public static class GameConstants
    {
        /// <summary>
        /// Network-related constants including connection details and default credentials.
        /// </summary>
        public static class Network
        {
            /// <summary>
            /// IP address for local development and testing.
            /// </summary>
            public const string LOCAL_IP = "127.0.0.1";

            /// <summary>
            /// IP address for production server.
            /// </summary>
            public const string PRODUCTION_IP = "170.187.179.212";

            /// <summary>
            /// Port number for network communication.
            /// </summary>
            public const int PORT = 1909;

            /// <summary>
            /// Default password used for testing and automatic connections.
            /// </summary>
            public const string DEFAULT_PASSWORD = "defaultpass123";
        }

        /// <summary>
        /// UI-related constants including sizes, scales, and spacing values.
        /// </summary>
        public static class UI
        {
            /// <summary>
            /// Camera zoom level during fight sequences.
            /// </summary>
            public const float FIGHT_CAMERA_ZOOM = 3f;

            /// <summary>
            /// Camera zoom level during normal world navigation.
            /// </summary>
            public const float WORLD_CAMERA_ZOOM = 4f;

            /// <summary>
            /// Scale factor for loot item sprites in the UI.
            /// </summary>
            public const float LOOT_ITEM_SCALE = 4f;

            /// <summary>
            /// Horizontal spacing between loot items in the UI.
            /// </summary>
            public const float LOOT_ITEM_SPACING = 64f;
        }

        /// <summary>
        /// File paths for various game resources and assets.
        /// </summary>
        public static class ResourcePaths
        {
            /// <summary>
            /// Path to the item UI scene file.
            /// </summary>
            public const string ITEM_UI_SCENE = "res://Items/ItemUI.tscn";

            /// <summary>
            /// Path to the main GUI scene file.
            /// </summary>
            public const string GUI_SCENE = "res://0-Frontend/GUI/GUI.tscn";

            /// <summary>
            /// Directory containing item sprite assets.
            /// </summary>
            public const string ITEMS_SPRITES_PATH = "res://Items/Sprites";
        }

        /// <summary>
        /// Standard messages used throughout the game for user feedback.
        /// </summary>
        public static class Messages
        {
            /// <summary>
            /// Message shown when attempting to connect to the server.
            /// </summary>
            public const string CONNECTING = "Attempting to connect to server. Please wait...";

            /// <summary>
            /// Message shown when connection to the server fails.
            /// </summary>
            public const string CONNECTION_FAILED = "Could not connect to server. Is it down?";

            /// <summary>
            /// Message shown when successfully connected to the server.
            /// </summary>
            public const string CONNECTION_SUCCESS = "Connected to server successfully. You can now log in";
        }
    }
} 