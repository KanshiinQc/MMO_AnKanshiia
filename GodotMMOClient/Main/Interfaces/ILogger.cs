namespace CLIENT.Interfaces
{
    /// <summary>
    /// Provides a standardized logging interface for the application.
    /// Handles different levels of logging with appropriate formatting and output.
    /// </summary>
    /// <remarks>
    /// This interface defines three logging levels:
    /// - Info: For general information and normal operation logs
    /// - Warning: For non-critical issues that might need attention
    /// - Error: For critical issues that need immediate attention
    /// 
    /// Implementation should handle:
    /// - Appropriate formatting of messages
    /// - Output to appropriate logging channels
    /// - Timestamp addition if needed
    /// - Log level indication in the output
    /// </remarks>
    public interface ILogger
    {
        /// <summary>
        /// Logs an informational message about normal operation.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <remarks>
        /// Use this method for:
        /// - Tracking normal program flow
        /// - Debugging information
        /// - Status updates
        /// - General information that might be useful for debugging
        /// </remarks>
        void Log(string message);

        /// <summary>
        /// Logs a warning message about potential issues.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        /// <remarks>
        /// Use this method for:
        /// - Non-critical issues that don't prevent operation
        /// - Potential problems that might need attention
        /// - Deprecated feature usage
        /// - Unexpected but handleable situations
        /// </remarks>
        void LogWarning(string message);

        /// <summary>
        /// Logs an error message about critical issues.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <remarks>
        /// Use this method for:
        /// - Critical errors that prevent normal operation
        /// - Exceptions that need immediate attention
        /// - Security issues
        /// - Resource access failures
        /// - Network connection failures
        /// </remarks>
        void LogError(string message);
    }
} 