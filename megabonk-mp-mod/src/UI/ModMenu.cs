using UnityEngine;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.DelegateSupport;
using MegabonkMP.Core;
using MegabonkMP.Network;

namespace MegabonkMP.UI
{
    /// <summary>
    /// In-game mod menu using IMGUI.
    /// Press F9 to toggle the menu.
    /// </summary>
    public class ModMenu : MonoBehaviour
    {
        private static ModMenu _instance;
        private bool _showMenu = false;
        private bool _showSettings = false;
        
        // Menu state
        private int _currentTab = 0;
        private readonly string[] _tabs = { "Multiplayer", "Settings", "Debug" };
        
        // Window rects
        private Rect _windowRect = new Rect(50, 50, 450, 500);
        private Rect _settingsRect = new Rect(520, 50, 350, 400);
        
        // Connection settings
        private string _serverAddress = "127.0.0.1";
        private string _serverPort = "7777";
        private string _playerName = "Player";
        private string _maxPlayers = "4";
        
        // Gameplay settings
        private bool _friendlyFire = false;
        private bool _sharedLoot = true;
        private float _xpMultiplier = 2.0f;
        
        // Display settings
        private bool _showNameplates = true;
        private bool _showHealth = true;
        private bool _showNetworkStats = false;
        
        // Scroll positions
        private Vector2 _playerListScroll = Vector2.zero;
        private Vector2 _logScroll = Vector2.zero;
        
        // GUI Styles
        private GUIStyle _windowStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _textFieldStyle;
        private bool _stylesInitialized = false;
        
        public static void Initialize()
        {
            if (_instance != null) return;
            
            var go = new GameObject("MegabonkMP_ModMenu");
            Object.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            _instance = go.AddComponent<ModMenu>();
            
            ModLogger.Info("Mod menu initialized - Press F9 to open");
        }
        
        [HideFromIl2Cpp]
        private void Awake()
        {
            // Load saved settings
            LoadSettings();
        }
        
        [HideFromIl2Cpp]
        private void Update()
        {
            // Toggle menu with F9
            if (Input.GetKeyDown(KeyCode.F9))
            {
                _showMenu = !_showMenu;
                
                // Optionally show/hide cursor
                // Cursor.visible = _showMenu;
                // Cursor.lockState = _showMenu ? CursorLockMode.None : CursorLockMode.Locked;
            }
            
            // Quick connect/disconnect with F10
            if (Input.GetKeyDown(KeyCode.F10))
            {
                if (NetworkManager.Instance?.IsConnected ?? false)
                {
                    NetworkManager.Instance.Shutdown();
                }
            }
        }
        
        [HideFromIl2Cpp]
        private void OnGUI()
        {
            if (!_showMenu) return;
            
            InitStyles();
            
            // Main window - use explicit delegate for IL2CPP
            var mainWindowFunc = DelegateSupport.ConvertDelegate<GUI.WindowFunction>(
                new System.Action<int>(DrawMainWindow));
            _windowRect = GUI.Window(12345, _windowRect, mainWindowFunc, "Megabonk Multiplayer", _windowStyle);
            
            // Settings window (if open)
            if (_showSettings)
            {
                var settingsWindowFunc = DelegateSupport.ConvertDelegate<GUI.WindowFunction>(
                    new System.Action<int>(DrawSettingsWindow));
                _settingsRect = GUI.Window(12346, _settingsRect, settingsWindowFunc, "Settings", _windowStyle);
            }
        }
        
        [HideFromIl2Cpp]
        private void InitStyles()
        {
            if (_stylesInitialized) return;
            
            _windowStyle = new GUIStyle(GUI.skin.window)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            
            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13
            };
            // Set padding separately for IL2CPP compatibility
            _buttonStyle.padding.left = 10;
            _buttonStyle.padding.right = 10;
            _buttonStyle.padding.top = 5;
            _buttonStyle.padding.bottom = 5;
            
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12
            };
            
            _textFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 12
            };
            
            _stylesInitialized = true;
        }
        
        [HideFromIl2Cpp]
        private void DrawMainWindow(int windowId)
        {
            GUILayout.BeginVertical();
            
            // Tab buttons
            GUILayout.BeginHorizontal();
            for (int i = 0; i < _tabs.Length; i++)
            {
                if (GUILayout.Toggle(_currentTab == i, _tabs[i], _buttonStyle, GUILayout.Height(30)))
                {
                    _currentTab = i;
                }
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Tab content
            switch (_currentTab)
            {
                case 0:
                    DrawMultiplayerTab();
                    break;
                case 1:
                    DrawSettingsTab();
                    break;
                case 2:
                    DrawDebugTab();
                    break;
            }
            
            GUILayout.EndVertical();
            
            // Make window draggable
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
        
        [HideFromIl2Cpp]
        private void DrawMultiplayerTab()
        {
            var isConnected = NetworkManager.Instance?.IsConnected ?? false;
            var isHost = NetworkManager.Instance?.IsHost ?? false;
            
            // Connection status
            GUILayout.BeginHorizontal();
            GUILayout.Label("Status:", _labelStyle, GUILayout.Width(60));
            var statusColor = isConnected ? "<color=green>" : "<color=red>";
            var statusText = isConnected ? (isHost ? "Hosting" : "Connected") : "Disconnected";
            GUILayout.Label($"{statusColor}{statusText}</color>", _labelStyle);
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            if (!isConnected)
            {
                // Connection settings
                GUILayout.Label("Player Name:", _labelStyle);
                _playerName = GUILayout.TextField(_playerName, 20, _textFieldStyle);
                
                GUILayout.Space(5);
                
                // Host section
                GUILayout.Label("═══ Host Game ═══", _headerStyle);
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("Port:", _labelStyle, GUILayout.Width(80));
                _serverPort = GUILayout.TextField(_serverPort, 5, _textFieldStyle, GUILayout.Width(80));
                GUILayout.Label("Max Players:", _labelStyle, GUILayout.Width(80));
                _maxPlayers = GUILayout.TextField(_maxPlayers, 1, _textFieldStyle, GUILayout.Width(40));
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                
                if (GUILayout.Button("Host Game", _buttonStyle, GUILayout.Height(35)))
                {
                    HostGame();
                }
                
                GUILayout.Space(15);
                
                // Join section
                GUILayout.Label("═══ Join Game ═══", _headerStyle);
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("Address:", _labelStyle, GUILayout.Width(60));
                _serverAddress = GUILayout.TextField(_serverAddress, _textFieldStyle);
                GUILayout.Label(":", _labelStyle, GUILayout.Width(10));
                _serverPort = GUILayout.TextField(_serverPort, 5, _textFieldStyle, GUILayout.Width(60));
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                
                if (GUILayout.Button("Join Game", _buttonStyle, GUILayout.Height(35)))
                {
                    JoinGame();
                }
            }
            else
            {
                // Connected - show player list and controls
                GUILayout.Label($"Playing as: {_playerName}", _labelStyle);
                
                if (isHost)
                {
                    GUILayout.Label($"Hosting on port {_serverPort}", _labelStyle);
                }
                else
                {
                    GUILayout.Label($"Connected to {_serverAddress}:{_serverPort}", _labelStyle);
                }
                
                GUILayout.Space(10);
                
                // Player list
                GUILayout.Label("═══ Players ═══", _headerStyle);
                
                _playerListScroll = GUILayout.BeginScrollView(_playerListScroll, GUILayout.Height(150));
                
                var players = NetworkManager.Instance?.GetAllPlayers();
                if (players != null)
                {
                    foreach (var player in players)
                    {
                        GUILayout.BeginHorizontal();
                        var readyIcon = player.IsReady ? "✓" : "○";
                        var hostIcon = player.IsHost ? "[Host]" : "";
                        GUILayout.Label($"{readyIcon} {player.PlayerName} {hostIcon}", _labelStyle);
                        GUILayout.EndHorizontal();
                    }
                }
                else
                {
                    GUILayout.Label("No players connected", _labelStyle);
                }
                
                GUILayout.EndScrollView();
                
                GUILayout.Space(10);
                
                // Game controls
                if (isHost)
                {
                    GUILayout.Label("═══ Host Controls ═══", _headerStyle);
                    
                    GUILayout.BeginHorizontal();
                    _friendlyFire = GUILayout.Toggle(_friendlyFire, " Friendly Fire", _labelStyle);
                    _sharedLoot = GUILayout.Toggle(_sharedLoot, " Shared Loot", _labelStyle);
                    GUILayout.EndHorizontal();
                    
                    GUILayout.Space(5);
                    
                    if (GUILayout.Button("Start Game", _buttonStyle, GUILayout.Height(35)))
                    {
                        StartGame();
                    }
                }
                
                GUILayout.Space(10);
                
                // Disconnect button
                if (GUILayout.Button("Disconnect", _buttonStyle, GUILayout.Height(30)))
                {
                    Disconnect();
                }
            }
        }
        
        [HideFromIl2Cpp]
        private void DrawSettingsTab()
        {
            GUILayout.Label("═══ Display Settings ═══", _headerStyle);
            
            _showNameplates = GUILayout.Toggle(_showNameplates, " Show Player Nameplates", _labelStyle);
            _showHealth = GUILayout.Toggle(_showHealth, " Show Health Bars", _labelStyle);
            _showNetworkStats = GUILayout.Toggle(_showNetworkStats, " Show Network Stats (F3)", _labelStyle);
            
            GUILayout.Space(15);
            
            GUILayout.Label("═══ Gameplay Settings ═══", _headerStyle);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label($"XP Multiplier: {_xpMultiplier:F1}x", _labelStyle, GUILayout.Width(150));
            _xpMultiplier = GUILayout.HorizontalSlider(_xpMultiplier, 1.0f, 4.0f);
            GUILayout.EndHorizontal();
            
            GUILayout.Space(15);
            
            GUILayout.Label("═══ Hotkeys ═══", _headerStyle);
            GUILayout.Label("F9 - Toggle this menu", _labelStyle);
            GUILayout.Label("F10 - Quick disconnect", _labelStyle);
            GUILayout.Label("F3 - Toggle network stats", _labelStyle);
            
            GUILayout.Space(15);
            
            if (GUILayout.Button("Save Settings", _buttonStyle, GUILayout.Height(30)))
            {
                SaveSettings();
            }
            
            if (GUILayout.Button("Reset to Defaults", _buttonStyle, GUILayout.Height(25)))
            {
                ResetSettings();
            }
        }
        
        [HideFromIl2Cpp]
        private void DrawDebugTab()
        {
            GUILayout.Label("═══ Debug Info ═══", _headerStyle);
            
            var netManager = NetworkManager.Instance;
            
            GUILayout.Label($"Mod Version: {PluginInfo.PLUGIN_VERSION}", _labelStyle);
            GUILayout.Label($"Connected: {netManager?.IsConnected ?? false}", _labelStyle);
            GUILayout.Label($"Is Host: {netManager?.IsHost ?? false}", _labelStyle);
            GUILayout.Label($"Local Player ID: {netManager?.LocalPlayerId ?? -1}", _labelStyle);
            
            GUILayout.Space(10);
            
            GUILayout.Label("═══ Network Stats ═══", _headerStyle);
            GUILayout.Label($"Ping: {netManager?.Ping ?? 0}ms", _labelStyle);
            GUILayout.Label($"Packets Sent: {netManager?.PacketsSent ?? 0}", _labelStyle);
            GUILayout.Label($"Packets Received: {netManager?.PacketsReceived ?? 0}", _labelStyle);
            
            GUILayout.Space(10);
            
            GUILayout.Label("═══ Console Log ═══", _headerStyle);
            
            _logScroll = GUILayout.BeginScrollView(_logScroll, GUILayout.Height(150));
            GUILayout.Label(ModLogger.GetRecentLogs(), _labelStyle);
            GUILayout.EndScrollView();
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Clear Log", _buttonStyle))
            {
                ModLogger.ClearLogs();
            }
        }
        
        [HideFromIl2Cpp]
        private void DrawSettingsWindow(int windowId)
        {
            // Additional settings window if needed
            GUILayout.Label("Extended Settings", _headerStyle);
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
        
        #region Actions
        
        [HideFromIl2Cpp]
        private void HostGame()
        {
            if (string.IsNullOrEmpty(_playerName))
            {
                ModLogger.Warning("Please enter a player name");
                return;
            }
            
            if (!int.TryParse(_serverPort, out int port) || port < 1 || port > 65535)
            {
                ModLogger.Warning("Invalid port number");
                return;
            }
            
            if (!int.TryParse(_maxPlayers, out int maxPlayers) || maxPlayers < 2 || maxPlayers > 6)
            {
                maxPlayers = 4;
            }
            
            ModLogger.Info($"Hosting game on port {port} with max {maxPlayers} players...");
            NetworkManager.Instance?.Host(port, maxPlayers, _playerName);
        }
        
        [HideFromIl2Cpp]
        private void JoinGame()
        {
            if (string.IsNullOrEmpty(_playerName))
            {
                ModLogger.Warning("Please enter a player name");
                return;
            }
            
            if (string.IsNullOrEmpty(_serverAddress))
            {
                ModLogger.Warning("Please enter a server address");
                return;
            }
            
            if (!int.TryParse(_serverPort, out int port) || port < 1 || port > 65535)
            {
                ModLogger.Warning("Invalid port number");
                return;
            }
            
            ModLogger.Info($"Connecting to {_serverAddress}:{port}...");
            NetworkManager.Instance?.Connect(_serverAddress, port, _playerName);
        }
        
        [HideFromIl2Cpp]
        private void Disconnect()
        {
            ModLogger.Info("Disconnecting...");
            NetworkManager.Instance?.Shutdown();
        }
        
        [HideFromIl2Cpp]
        private void StartGame()
        {
            ModLogger.Info("Starting game...");
            // Would trigger session start packet and game load
        }
        
        [HideFromIl2Cpp]
        private void SaveSettings()
        {
            // Save to BepInEx config
            PlayerPrefs.SetString("MP_PlayerName", _playerName);
            PlayerPrefs.SetString("MP_ServerAddress", _serverAddress);
            PlayerPrefs.SetString("MP_ServerPort", _serverPort);
            PlayerPrefs.SetInt("MP_FriendlyFire", _friendlyFire ? 1 : 0);
            PlayerPrefs.SetInt("MP_SharedLoot", _sharedLoot ? 1 : 0);
            PlayerPrefs.SetFloat("MP_XPMultiplier", _xpMultiplier);
            PlayerPrefs.SetInt("MP_ShowNameplates", _showNameplates ? 1 : 0);
            PlayerPrefs.SetInt("MP_ShowHealth", _showHealth ? 1 : 0);
            PlayerPrefs.SetInt("MP_ShowNetworkStats", _showNetworkStats ? 1 : 0);
            PlayerPrefs.Save();
            
            ModLogger.Info("Settings saved");
        }
        
        [HideFromIl2Cpp]
        private void LoadSettings()
        {
            _playerName = PlayerPrefs.GetString("MP_PlayerName", "Player");
            _serverAddress = PlayerPrefs.GetString("MP_ServerAddress", "127.0.0.1");
            _serverPort = PlayerPrefs.GetString("MP_ServerPort", "7777");
            _friendlyFire = PlayerPrefs.GetInt("MP_FriendlyFire", 0) == 1;
            _sharedLoot = PlayerPrefs.GetInt("MP_SharedLoot", 1) == 1;
            _xpMultiplier = PlayerPrefs.GetFloat("MP_XPMultiplier", 2.0f);
            _showNameplates = PlayerPrefs.GetInt("MP_ShowNameplates", 1) == 1;
            _showHealth = PlayerPrefs.GetInt("MP_ShowHealth", 1) == 1;
            _showNetworkStats = PlayerPrefs.GetInt("MP_ShowNetworkStats", 0) == 1;
        }
        
        [HideFromIl2Cpp]
        private void ResetSettings()
        {
            _playerName = "Player";
            _serverAddress = "127.0.0.1";
            _serverPort = "7777";
            _friendlyFire = false;
            _sharedLoot = true;
            _xpMultiplier = 2.0f;
            _showNameplates = true;
            _showHealth = true;
            _showNetworkStats = false;
            
            SaveSettings();
            ModLogger.Info("Settings reset to defaults");
        }
        
        #endregion
    }
}
