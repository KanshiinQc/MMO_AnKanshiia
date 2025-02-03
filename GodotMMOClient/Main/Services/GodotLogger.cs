using Godot;
using CLIENT.Interfaces;

namespace CLIENT.Services
{
    /// <summary>
    /// Implementation of ILogger that uses Godot's built-in logging system
    /// </summary>
    public class GodotLogger : ILogger
    {
        public void Log(string message)
        {
            GD.Print($"[INFO] {message}");
        }

        public void LogWarning(string message)
        {
            GD.PushWarning($"[WARNING] {message}");
        }

        public void LogError(string message)
        {
            GD.PrintErr($"[ERROR] {message}");
        }
    }
} 