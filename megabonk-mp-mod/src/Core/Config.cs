using BepInEx.Configuration;

namespace MegabonkMP.Core
{
    /// <summary>
    /// Configuration management for the multiplayer mod.
    /// Handles all user-configurable settings via BepInEx config system.
    /// </summary>
    public class Config
    {
        // Network settings
        public ConfigEntry<string> ServerAddress { get; private set; }
        public ConfigEntry<int> ServerPort { get; private set; }
        public ConfigEntry<int> MaxPlayers { get; private set; }
        public ConfigEntry<int> TickRate { get; private set; }
        
        // Gameplay settings
        public ConfigEntry<bool> FriendlyFire { get; private set; }
        public ConfigEntry<bool> SharedLoot { get; private set; }
        public ConfigEntry<float> XpMultiplier { get; private set; }
        public ConfigEntry<float> CreditTimerMultiplier { get; private set; }
        
        // UI settings
        public ConfigEntry<bool> ShowPlayerNameplates { get; private set; }
        public ConfigEntry<bool> ShowPlayerHealth { get; private set; }
        public ConfigEntry<float> NameplateDistance { get; private set; }
        
        // Debug settings
        public ConfigEntry<bool> DebugMode { get; private set; }
        public ConfigEntry<bool> LogNetworkPackets { get; private set; }
        public ConfigEntry<bool> ShowNetworkStats { get; private set; }

        public Config(ConfigFile config)
        {
            // Network configuration
            ServerAddress = config.Bind(
                "Network",
                "ServerAddress",
                "127.0.0.1",
                "IP address to connect to or host on"
            );
            
            ServerPort = config.Bind(
                "Network",
                "ServerPort",
                7777,
                new ConfigDescription(
                    "Port for multiplayer connections",
                    new AcceptableValueRange<int>(1024, 65535)
                )
            );
            
            MaxPlayers = config.Bind(
                "Network",
                "MaxPlayers",
                4,
                new ConfigDescription(
                    "Maximum players in a session",
                    new AcceptableValueRange<int>(2, 6)
                )
            );
            
            TickRate = config.Bind(
                "Network",
                "TickRate",
                60,
                new ConfigDescription(
                    "Network updates per second",
                    new AcceptableValueRange<int>(20, 128)
                )
            );
            
            // Gameplay configuration
            FriendlyFire = config.Bind(
                "Gameplay",
                "FriendlyFire",
                false,
                "Allow players to damage each other"
            );
            
            SharedLoot = config.Bind(
                "Gameplay",
                "SharedLoot",
                true,
                "Share loot drops among all players"
            );
            
            XpMultiplier = config.Bind(
                "Gameplay",
                "XpMultiplier",
                2.0f,
                new ConfigDescription(
                    "XP multiplier for multiplayer (2x for 2-4 players recommended)",
                    new AcceptableValueRange<float>(1.0f, 5.0f)
                )
            );
            
            CreditTimerMultiplier = config.Bind(
                "Gameplay",
                "CreditTimerMultiplier",
                1.5f,
                new ConfigDescription(
                    "Credit timer scaling for multiplayer",
                    new AcceptableValueRange<float>(1.0f, 3.0f)
                )
            );
            
            // UI configuration
            ShowPlayerNameplates = config.Bind(
                "UI",
                "ShowPlayerNameplates",
                true,
                "Display nameplates above other players"
            );
            
            ShowPlayerHealth = config.Bind(
                "UI",
                "ShowPlayerHealth",
                true,
                "Show health bars on player nameplates"
            );
            
            NameplateDistance = config.Bind(
                "UI",
                "NameplateDistance",
                50.0f,
                new ConfigDescription(
                    "Maximum distance to show nameplates",
                    new AcceptableValueRange<float>(10.0f, 100.0f)
                )
            );
            
            // Debug configuration
            DebugMode = config.Bind(
                "Debug",
                "DebugMode",
                false,
                "Enable debug logging and features"
            );
            
            LogNetworkPackets = config.Bind(
                "Debug",
                "LogNetworkPackets",
                false,
                "Log all network packets (verbose)"
            );
            
            ShowNetworkStats = config.Bind(
                "Debug",
                "ShowNetworkStats",
                false,
                "Display network statistics overlay"
            );
        }
    }
}
