using BepInEx.Logging;
using System;

namespace MegabonkMP.Core
{
    /// <summary>
    /// Centralized logging utility for the multiplayer mod.
    /// Wraps BepInEx logging with convenience methods.
    /// </summary>
    public static class Logger
    {
        private static ManualLogSource _logSource;
        private static bool _initialized;
        
        public static void Initialize(ManualLogSource logSource)
        {
            _logSource = logSource ?? throw new ArgumentNullException(nameof(logSource));
            _initialized = true;
        }
        
        public static void Info(string message)
        {
            if (!_initialized) return;
            _logSource.LogInfo(message);
        }
        
        public static void Warning(string message)
        {
            if (!_initialized) return;
            _logSource.LogWarning(message);
        }
        
        public static void Error(string message)
        {
            if (!_initialized) return;
            _logSource.LogError(message);
        }
        
        public static void Error(string message, Exception ex)
        {
            if (!_initialized) return;
            _logSource.LogError($"{message}: {ex}");
        }
        
        public static void Debug(string message)
        {
            if (!_initialized) return;
            _logSource.LogDebug(message);
        }
        
        /// <summary>
        /// Log network-related messages (respects LogNetworkPackets config).
        /// </summary>
        public static void Network(string message)
        {
            if (!_initialized) return;
            _logSource.LogDebug($"[NET] {message}");
        }
        
        /// <summary>
        /// Log synchronization events.
        /// </summary>
        public static void Sync(string message)
        {
            if (!_initialized) return;
            _logSource.LogDebug($"[SYNC] {message}");
        }
    }
}
