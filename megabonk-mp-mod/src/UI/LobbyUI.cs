using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Il2CppInterop.Runtime.Attributes;
using MegabonkMP.Core;
using MegabonkMP.Network;

namespace MegabonkMP.UI
{
    /// <summary>
    /// Lobby UI for multiplayer session management.
    /// Handles host/join, player list, and ready states.
    /// </summary>
    public class LobbyUI : MonoBehaviour
    {
        private static GameObject _lobbyCanvas;
        private static GameObject _lobbyPanel;
        private static bool _isOpen;
        
        // UI Elements
        private static Text _titleText;
        private static InputField _addressInput;
        private static InputField _portInput;
        private static InputField _nameInput;
        private static Button _hostButton;
        private static Button _joinButton;
        private static Button _disconnectButton;
        private static Button _readyButton;
        private static Button _startButton;
        private static Transform _playerListContent;
        private static Text _statusText;
        
        // Player list items
        private static readonly Dictionary<int, GameObject> _playerListItems = new();
        
        /// <summary>
        /// Add multiplayer button to main menu.
        /// </summary>
        public static void AddMultiplayerButton()
        {
            try
            {
                // Find main menu buttons container
                var mainMenu = GameObject.Find("MainMenu");
                if (mainMenu == null)
                {
                    Logger.Warning("MainMenu not found, creating standalone button");
                }
                
                // Create multiplayer button
                var mpButton = CreateButton("MultiplayerButton", "Multiplayer", OnMultiplayerClicked);
                if (mainMenu != null)
                {
                    mpButton.transform.SetParent(mainMenu.transform, false);
                }
                
                Logger.Info("Multiplayer button added to menu");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to add multiplayer button", ex);
            }
        }
        
        private static void OnMultiplayerClicked()
        {
            if (_isOpen)
            {
                CloseLobby();
            }
            else
            {
                OpenLobby();
            }
        }
        
        public static void OpenLobby()
        {
            if (_lobbyCanvas == null)
            {
                CreateLobbyUI();
            }
            
            _lobbyCanvas.SetActive(true);
            _isOpen = true;
            UpdateUI();
        }
        
        public static void CloseLobby()
        {
            if (_lobbyCanvas != null)
            {
                _lobbyCanvas.SetActive(false);
            }
            _isOpen = false;
        }
        
        private static void CreateLobbyUI()
        {
            // Create canvas
            _lobbyCanvas = new GameObject("MegabonkMP_LobbyCanvas");
            var canvas = _lobbyCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            _lobbyCanvas.AddComponent<CanvasScaler>();
            _lobbyCanvas.AddComponent<GraphicRaycaster>();
            
            // Create panel background
            _lobbyPanel = CreatePanel("LobbyPanel", new Vector2(600, 500));
            _lobbyPanel.transform.SetParent(_lobbyCanvas.transform, false);
            
            var panelRect = _lobbyPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            
            // Title
            _titleText = CreateText("TitleText", "Megabonk Multiplayer", 24);
            _titleText.transform.SetParent(_lobbyPanel.transform, false);
            var titleRect = _titleText.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 220);
            
            // Connection inputs
            float inputY = 160;
            
            var nameLabel = CreateText("NameLabel", "Player Name:", 14);
            nameLabel.transform.SetParent(_lobbyPanel.transform, false);
            nameLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200, inputY);
            
            _nameInput = CreateInputField("NameInput", "Player");
            _nameInput.transform.SetParent(_lobbyPanel.transform, false);
            _nameInput.GetComponent<RectTransform>().anchoredPosition = new Vector2(50, inputY);
            
            inputY -= 40;
            
            var addressLabel = CreateText("AddressLabel", "Address:", 14);
            addressLabel.transform.SetParent(_lobbyPanel.transform, false);
            addressLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200, inputY);
            
            _addressInput = CreateInputField("AddressInput", "127.0.0.1");
            _addressInput.transform.SetParent(_lobbyPanel.transform, false);
            _addressInput.GetComponent<RectTransform>().anchoredPosition = new Vector2(50, inputY);
            
            inputY -= 40;
            
            var portLabel = CreateText("PortLabel", "Port:", 14);
            portLabel.transform.SetParent(_lobbyPanel.transform, false);
            portLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200, inputY);
            
            _portInput = CreateInputField("PortInput", "7777");
            _portInput.transform.SetParent(_lobbyPanel.transform, false);
            _portInput.GetComponent<RectTransform>().anchoredPosition = new Vector2(50, inputY);
            
            // Buttons
            float buttonY = 40;
            
            _hostButton = CreateButton("HostButton", "Host Game", OnHostClicked);
            _hostButton.transform.SetParent(_lobbyPanel.transform, false);
            _hostButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, buttonY);
            
            _joinButton = CreateButton("JoinButton", "Join Game", OnJoinClicked);
            _joinButton.transform.SetParent(_lobbyPanel.transform, false);
            _joinButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(100, buttonY);
            
            buttonY -= 50;
            
            _disconnectButton = CreateButton("DisconnectButton", "Disconnect", OnDisconnectClicked);
            _disconnectButton.transform.SetParent(_lobbyPanel.transform, false);
            _disconnectButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, buttonY);
            
            _readyButton = CreateButton("ReadyButton", "Ready", OnReadyClicked);
            _readyButton.transform.SetParent(_lobbyPanel.transform, false);
            _readyButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(100, buttonY);
            
            buttonY -= 50;
            
            _startButton = CreateButton("StartButton", "Start Game", OnStartClicked);
            _startButton.transform.SetParent(_lobbyPanel.transform, false);
            _startButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, buttonY);
            
            // Player list
            var playerListLabel = CreateText("PlayerListLabel", "Players:", 16);
            playerListLabel.transform.SetParent(_lobbyPanel.transform, false);
            playerListLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200, -80);
            
            var playerListObj = new GameObject("PlayerListContent");
            playerListObj.transform.SetParent(_lobbyPanel.transform, false);
            _playerListContent = playerListObj.transform;
            var plRect = playerListObj.AddComponent<RectTransform>();
            plRect.anchoredPosition = new Vector2(0, -140);
            plRect.sizeDelta = new Vector2(400, 100);
            
            // Status text
            _statusText = CreateText("StatusText", "Not connected", 12);
            _statusText.transform.SetParent(_lobbyPanel.transform, false);
            _statusText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -220);
            
            // Close button
            var closeButton = CreateButton("CloseButton", "X", CloseLobby);
            closeButton.transform.SetParent(_lobbyPanel.transform, false);
            var closeRect = closeButton.GetComponent<RectTransform>();
            closeRect.anchoredPosition = new Vector2(280, 230);
            closeRect.sizeDelta = new Vector2(30, 30);
            
            // Subscribe to network events
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnConnectionStateChanged += OnConnectionStateChanged;
                NetworkManager.Instance.OnPlayerConnected += OnPlayerConnected;
                NetworkManager.Instance.OnPlayerDisconnected += OnPlayerDisconnected;
            }
            
            Logger.Info("Lobby UI created");
        }
        
        private static void OnHostClicked()
        {
            if (NetworkManager.Instance == null) return;
            
            if (!int.TryParse(_portInput.text, out int port)) port = 7777;
            
            NetworkManager.Instance.Host(port, 4);
            UpdateStatus("Hosting game...");
        }
        
        private static void OnJoinClicked()
        {
            if (NetworkManager.Instance == null) return;
            
            string address = _addressInput.text;
            if (!int.TryParse(_portInput.text, out int port)) port = 7777;
            string playerName = _nameInput.text;
            
            NetworkManager.Instance.Connect(address, port, playerName);
            UpdateStatus($"Connecting to {address}:{port}...");
        }
        
        private static void OnDisconnectClicked()
        {
            NetworkManager.Instance?.Shutdown();
            UpdateStatus("Disconnected");
        }
        
        private static void OnReadyClicked()
        {
            // Toggle ready state
            // Would send PlayerReadyPacket
            Logger.Debug("Ready toggled");
        }
        
        private static void OnStartClicked()
        {
            if (NetworkManager.Instance?.IsHost ?? false)
            {
                // Check all players ready
                // Start game with SessionStartPacket
                Logger.Debug("Starting game...");
            }
        }
        
        private static void OnConnectionStateChanged(ConnectionState state)
        {
            UpdateStatus($"Status: {state}");
            UpdateUI();
        }
        
        private static void OnPlayerConnected(NetworkPlayer player)
        {
            AddPlayerToList(player);
        }
        
        private static void OnPlayerDisconnected(NetworkPlayer player)
        {
            RemovePlayerFromList(player.PlayerId);
        }
        
        private static void AddPlayerToList(NetworkPlayer player)
        {
            if (_playerListContent == null) return;
            
            var item = new GameObject($"PlayerItem_{player.PlayerId}");
            item.transform.SetParent(_playerListContent, false);
            
            var text = item.AddComponent<Text>();
            text.text = $"{player.Name} {(player.IsHost ? "(Host)" : "")} {(player.IsReady ? "✓" : "")}";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 14;
            text.color = Color.white;
            
            var rect = item.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 20);
            rect.anchoredPosition = new Vector2(0, -_playerListItems.Count * 25);
            
            _playerListItems[player.PlayerId] = item;
        }
        
        private static void RemovePlayerFromList(int playerId)
        {
            if (_playerListItems.TryGetValue(playerId, out var item))
            {
                Destroy(item);
                _playerListItems.Remove(playerId);
            }
        }
        
        private static void UpdateUI()
        {
            bool connected = NetworkManager.Instance?.IsConnected ?? false;
            bool isHost = NetworkManager.Instance?.IsHost ?? false;
            
            if (_hostButton != null) _hostButton.interactable = !connected;
            if (_joinButton != null) _joinButton.interactable = !connected;
            if (_disconnectButton != null) _disconnectButton.interactable = connected;
            if (_readyButton != null) _readyButton.interactable = connected && !isHost;
            if (_startButton != null) _startButton.interactable = connected && isHost;
            if (_addressInput != null) _addressInput.interactable = !connected;
            if (_portInput != null) _portInput.interactable = !connected;
        }
        
        private static void UpdateStatus(string status)
        {
            if (_statusText != null)
            {
                _statusText.text = status;
            }
        }
        
        // UI Helper methods
        private static GameObject CreatePanel(string name, Vector2 size)
        {
            var panel = new GameObject(name);
            var image = panel.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            var rect = panel.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            return panel;
        }
        
        private static Text CreateText(string name, string content, int fontSize)
        {
            var obj = new GameObject(name);
            var text = obj.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            var rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 30);
            return text;
        }
        
        private static InputField CreateInputField(string name, string defaultValue)
        {
            var obj = new GameObject(name);
            var image = obj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            var text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 0);
            textRect.offsetMax = new Vector2(-5, 0);
            
            var input = obj.AddComponent<InputField>();
            input.textComponent = text;
            input.text = defaultValue;
            
            var rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 30);
            
            return input;
        }
        
        private static Button CreateButton(string name, string label, Action onClick)
        {
            var obj = new GameObject(name);
            var image = obj.AddComponent<Image>();
            image.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            var text = textObj.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var button = obj.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener((UnityEngine.Events.UnityAction)(() => onClick?.Invoke()));
            
            var rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120, 35);
            
            return button;
        }
    }
}
