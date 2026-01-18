using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MegabonkMP.Core
{
    /// <summary>
    /// Centralized logging utility for the multiplayer mod.
    /// Wraps BepInEx logging with convenience methods.
    /// </summary>
    public static class ModLogger
    {
        private static ManualLogSource _logSource;
        private static bool _initialized;
        
        // Log history for in-game display
        private static readonly List<string> _logHistory = new List<string>();
        private static readonly int _maxLogEntries = 100;
        private static readonly object _logLock = new object();
        
        public static void Initialize(ManualLogSource logSource)
        {
            _logSource = logSource ?? throw new ArgumentNullException(nameof(logSource));
            _initialized = true;
        }
        
        private static void AddToHistory(string level, string message)
        {
            lock (_logLock)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                _logHistory.Add($"[{timestamp}] [{level}] {message}");
                
                while (_logHistory.Count > _maxLogEntries)
                {
                    _logHistory.RemoveAt(0);
                }
            }
        }
        
        /// <summary>
        /// Get recent log entries for display.
        /// </summary>
        public static string GetRecentLogs(int count = 20)
        {
            lock (_logLock)
            {
                var sb = new StringBuilder();
                var start = Math.Max(0, _logHistory.Count - count);
                for (int i = start; i < _logHistory.Count; i++)
                {
                    sb.AppendLine(_logHistory[i]);
                }
                return sb.ToString();
            }
        }
        
        /// <summary>
        /// Clear log history.
        /// </summary>
        public static void ClearLogs()
        {
            lock (_logLock)
            {
                _logHistory.Clear();
            }
        }
        
        public static void Info(string message)
        {
            if (!_initialized) return;
            _logSource.LogInfo(message);
            AddToHistory("INFO", message);
        }
        
        public static void Warning(string message)
        {
            if (!_initialized) return;
            _logSource.LogWarning(message);
            AddToHistory("WARN", message);
        }
        
        public static void Error(string message)
        {
            if (!_initialized) return;
            _logSource.LogError(message);
            AddToHistory("ERR", message);
        }
        
        public static void Error(string message, Exception ex)
        {
            if (!_initialized) return;
            _logSource.LogError($"{message}: {ex}");
            AddToHistory("ERR", $"{message}: {ex.Message}");
        }
        
        public static void Debug(string message)
        {
            if (!_initialized) return;
            _logSource.LogDebug(message);
            AddToHistory("DBG", message);
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
