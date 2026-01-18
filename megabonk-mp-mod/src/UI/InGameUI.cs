using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Il2CppInterop.Runtime.Attributes;
using MegabonkMP.Core;
using MegabonkMP.Network;
using MegabonkMP.Sync;
using NetworkPlayer = global::MegabonkMP.Network.NetworkPlayer;

namespace MegabonkMP.UI
{
    /// <summary>
    /// In-game multiplayer HUD elements.
    /// Shows player nameplates, health bars, and network status.
    /// </summary>
    public class InGameUI : MonoBehaviour
    {
        private static GameObject _hudCanvas;
        private static readonly Dictionary<int, PlayerNameplate> _nameplates = new();
        private static Text _networkStatsText;
        private static GameObject _pingMarker;
        
        // Settings (from Config)
        private static float _nameplateDistance = 50f;
        private static bool _showNameplates = true;
        private static bool _showHealth = true;
        private static bool _showNetworkStats = false;
        
        public static void Initialize()
        {
            if (_hudCanvas != null) return;
            
            // Create HUD canvas
            _hudCanvas = new GameObject("MegabonkMP_HUDCanvas");
            var canvas = _hudCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            _hudCanvas.AddComponent<CanvasScaler>();
            
            // Create network stats display
            CreateNetworkStatsDisplay();
            
            // Create ping marker prefab
            CreatePingMarkerPrefab();
            
            ModLogger.Info("In-game UI initialized");
        }
        
        private static void CreateNetworkStatsDisplay()
        {
            var statsObj = new GameObject("NetworkStats");
            statsObj.transform.SetParent(_hudCanvas.transform, false);
            
            _networkStatsText = statsObj.AddComponent<Text>();
            _networkStatsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _networkStatsText.fontSize = 12;
            _networkStatsText.color = new Color(1f, 1f, 1f, 0.7f);
            _networkStatsText.alignment = TextAnchor.UpperLeft;
            
            var rect = statsObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(10, -10);
            rect.sizeDelta = new Vector2(200, 100);
            
            statsObj.SetActive(false); // Hidden by default
        }
        
        private static void CreatePingMarkerPrefab()
        {
            _pingMarker = new GameObject("PingMarker");
            _pingMarker.SetActive(false);
            
            // Simple ping indicator
            var image = _pingMarker.AddComponent<Image>();
            image.color = Color.yellow;
            
            var rect = _pingMarker.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(20, 20);
            
            Object.DontDestroyOnLoad(_pingMarker);
        }
        
        [HideFromIl2Cpp]
        public static void UpdateHUD()
        {
            if (!NetworkManager.Instance?.IsConnected ?? true) return;
            
            UpdateNameplates();
            
            if (_showNetworkStats)
            {
                UpdateNetworkStats();
            }
        }
        
        private static void UpdateNameplates()
        {
            if (!_showNameplates) return;
            
            var camera = Camera.main;
            if (camera == null) return;
            
            foreach (var player in NetworkManager.Instance.GetAllPlayers())
            {
                // Skip local player
                if (player.PlayerId == NetworkManager.Instance.LocalPlayerId) continue;
                
                // Get or create nameplate
                if (!_nameplates.TryGetValue(player.PlayerId, out var nameplate))
                {
                    nameplate = CreateNameplate(player);
                    _nameplates[player.PlayerId] = nameplate;
                }
                
                // Get remote player position
                var remotePlayerObj = GameObject.Find($"RemotePlayer_{player.PlayerId}");
                if (remotePlayerObj == null)
                {
                    nameplate.SetVisible(false);
                    continue;
                }
                
                var worldPos = remotePlayerObj.transform.position + Vector3.up * 2.5f;
                var distance = Vector3.Distance(camera.transform.position, worldPos);
                
                // Distance check
                if (distance > _nameplateDistance)
                {
                    nameplate.SetVisible(false);
                    continue;
                }
                
                // Check if in front of camera
                var viewportPos = camera.WorldToViewportPoint(worldPos);
                if (viewportPos.z < 0 || viewportPos.x < 0 || viewportPos.x > 1 || 
                    viewportPos.y < 0 || viewportPos.y > 1)
                {
                    nameplate.SetVisible(false);
                    continue;
                }
                
                // Convert to screen position
                var screenPos = camera.WorldToScreenPoint(worldPos);
                nameplate.SetPosition(screenPos);
                nameplate.SetVisible(true);
                
                // Update health if enabled
                if (_showHealth)
                {
                    // Would get actual health from PlayerSync
                    nameplate.UpdateHealth(player.Latency < 200 ? 1f : 0.5f);
                }
                
                // Scale based on distance
                float scale = Mathf.Clamp(1f - (distance / _nameplateDistance) * 0.5f, 0.5f, 1f);
                nameplate.SetScale(scale);
            }
        }
        
        private static PlayerNameplate CreateNameplate(NetworkPlayer player)
        {
            var obj = new GameObject($"Nameplate_{player.PlayerId}");
            obj.transform.SetParent(_hudCanvas.transform, false);
            
            // Background
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(obj.transform, false);
            var bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.5f);
            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(100, 30);
            
            // Name text
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(obj.transform, false);
            var nameText = nameObj.AddComponent<Text>();
            nameText.text = player.Name;
            nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            nameText.fontSize = 12;
            nameText.color = Color.white;
            nameText.alignment = TextAnchor.MiddleCenter;
            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.sizeDelta = new Vector2(100, 20);
            nameRect.anchoredPosition = new Vector2(0, 5);
            
            // Health bar background
            var healthBgObj = new GameObject("HealthBg");
            healthBgObj.transform.SetParent(obj.transform, false);
            var healthBgImage = healthBgObj.AddComponent<Image>();
            healthBgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            var healthBgRect = healthBgObj.GetComponent<RectTransform>();
            healthBgRect.sizeDelta = new Vector2(80, 6);
            healthBgRect.anchoredPosition = new Vector2(0, -8);
            
            // Health bar fill
            var healthFillObj = new GameObject("HealthFill");
            healthFillObj.transform.SetParent(healthBgObj.transform, false);
            var healthFillImage = healthFillObj.AddComponent<Image>();
            healthFillImage.color = Color.green;
            var healthFillRect = healthFillObj.GetComponent<RectTransform>();
            healthFillRect.anchorMin = new Vector2(0, 0);
            healthFillRect.anchorMax = new Vector2(1, 1);
            healthFillRect.offsetMin = Vector2.zero;
            healthFillRect.offsetMax = Vector2.zero;
            
            return new PlayerNameplate
            {
                GameObject = obj,
                NameText = nameText,
                HealthFill = healthFillRect
            };
        }
        
        private static void UpdateNetworkStats()
        {
            if (_networkStatsText == null) return;
            
            // Calculate stats
            int playerCount = 0;
            float avgLatency = 0f;
            foreach (var player in NetworkManager.Instance.GetAllPlayers())
            {
                playerCount++;
                avgLatency += player.Latency;
            }
            if (playerCount > 0) avgLatency /= playerCount;
            
            _networkStatsText.text = $"Players: {playerCount}\n" +
                                     $"Avg Latency: {avgLatency:F0}ms\n" +
                                     $"Host: {(NetworkManager.Instance.IsHost ? "Yes" : "No")}";
            
            _networkStatsText.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Show a ping marker at world position.
        /// </summary>
        public static void ShowPing(Vector3 worldPos, int pingType, int playerId)
        {
            if (_pingMarker == null || Camera.main == null) return;
            
            // Clone ping marker
            var ping = Object.Instantiate(_pingMarker, _hudCanvas.transform);
            ping.SetActive(true);
            
            // Set color based on type
            var image = ping.GetComponent<Image>();
            image.color = pingType switch
            {
                0 => Color.white,   // Generic
                1 => Color.red,     // Enemy
                2 => Color.green,   // Item
                3 => Color.yellow,  // Danger
                _ => Color.white
            };
            
            // Update position in coroutine and destroy after delay
            var screenPos = Camera.main.WorldToScreenPoint(worldPos);
            ping.GetComponent<RectTransform>().position = screenPos;
            
            // Destroy after 3 seconds
            Object.Destroy(ping, 3f);
        }
        
        /// <summary>
        /// Remove nameplate for disconnected player.
        /// </summary>
        public static void RemoveNameplate(int playerId)
        {
            if (_nameplates.TryGetValue(playerId, out var nameplate))
            {
                if (nameplate.GameObject != null)
                {
                    Object.Destroy(nameplate.GameObject);
                }
                _nameplates.Remove(playerId);
            }
        }
        
        public static void SetShowNetworkStats(bool show)
        {
            _showNetworkStats = show;
            if (_networkStatsText != null)
            {
                _networkStatsText.gameObject.SetActive(show);
            }
        }
    }
    
    public class PlayerNameplate
    {
        public GameObject GameObject;
        public Text NameText;
        public RectTransform HealthFill;
        
        public void SetVisible(bool visible)
        {
            if (GameObject != null)
            {
                GameObject.SetActive(visible);
            }
        }
        
        public void SetPosition(Vector3 screenPos)
        {
            if (GameObject != null)
            {
                GameObject.transform.position = screenPos;
            }
        }
        
        public void SetScale(float scale)
        {
            if (GameObject != null)
            {
                GameObject.transform.localScale = Vector3.one * scale;
            }
        }
        
        public void UpdateHealth(float normalizedHealth)
        {
            if (HealthFill != null)
            {
                HealthFill.anchorMax = new Vector2(normalizedHealth, 1);
            }
        }
    }
}
