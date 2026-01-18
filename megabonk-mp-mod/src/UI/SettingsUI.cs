using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using MegabonkMP.Core;
using MegabonkMP.Network;

namespace MegabonkMP.UI
{
    /// <summary>
    /// Settings UI for multiplayer configuration.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        private static GameObject _settingsPanel;
        private static bool _isOpen;
        
        // UI Elements
        private static Toggle _showNameplatesToggle;
        private static Toggle _showHealthToggle;
        private static Slider _nameplateDistanceSlider;
        private static Toggle _friendlyFireToggle;
        private static Toggle _sharedLootToggle;
        private static Toggle _debugModeToggle;
        private static Toggle _networkStatsToggle;
        
        public static void Open(Transform parent)
        {
            if (_settingsPanel == null)
            {
                Create(parent);
            }
            
            _settingsPanel.SetActive(true);
            _isOpen = true;
            LoadSettings();
        }
        
        public static void Close()
        {
            if (_settingsPanel != null)
            {
                _settingsPanel.SetActive(false);
            }
            _isOpen = false;
            SaveSettings();
        }
        
        public static void Toggle(Transform parent)
        {
            if (_isOpen)
            {
                Close();
            }
            else
            {
                Open(parent);
            }
        }
        
        private static void Create(Transform parent)
        {
            _settingsPanel = new GameObject("MegabonkMP_Settings");
            _settingsPanel.transform.SetParent(parent, false);
            
            var image = _settingsPanel.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
            
            var rect = _settingsPanel.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 350);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            
            float yPos = 140;
            
            // Title
            var title = CreateLabel("Multiplayer Settings", 18);
            title.transform.SetParent(_settingsPanel.transform, false);
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
            
            yPos -= 50;
            
            // UI Settings
            var uiHeader = CreateLabel("UI Settings", 14);
            uiHeader.transform.SetParent(_settingsPanel.transform, false);
            uiHeader.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, yPos);
            
            yPos -= 30;
            
            _showNameplatesToggle = CreateToggle("Show Nameplates", true);
            _showNameplatesToggle.transform.SetParent(_settingsPanel.transform, false);
            _showNameplatesToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
            
            yPos -= 30;
            
            _showHealthToggle = CreateToggle("Show Health Bars", true);
            _showHealthToggle.transform.SetParent(_settingsPanel.transform, false);
            _showHealthToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
            
            yPos -= 30;
            
            _nameplateDistanceSlider = CreateSlider("Nameplate Distance", 10, 100, 50);
            _nameplateDistanceSlider.transform.SetParent(_settingsPanel.transform, false);
            _nameplateDistanceSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
            
            yPos -= 50;
            
            // Gameplay Settings (Host only)
            var gameplayHeader = CreateLabel("Gameplay (Host)", 14);
            gameplayHeader.transform.SetParent(_settingsPanel.transform, false);
            gameplayHeader.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, yPos);
            
            yPos -= 30;
            
            _friendlyFireToggle = CreateToggle("Friendly Fire", false);
            _friendlyFireToggle.transform.SetParent(_settingsPanel.transform, false);
            _friendlyFireToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
            
            yPos -= 30;
            
            _sharedLootToggle = CreateToggle("Shared Loot", true);
            _sharedLootToggle.transform.SetParent(_settingsPanel.transform, false);
            _sharedLootToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
            
            yPos -= 50;
            
            // Debug Settings
            var debugHeader = CreateLabel("Debug", 14);
            debugHeader.transform.SetParent(_settingsPanel.transform, false);
            debugHeader.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, yPos);
            
            yPos -= 30;
            
            _networkStatsToggle = CreateToggle("Show Network Stats", false);
            _networkStatsToggle.transform.SetParent(_settingsPanel.transform, false);
            _networkStatsToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
            _networkStatsToggle.onValueChanged.AddListener(new UnityAction<bool>(OnNetworkStatsToggled));
            
            // Close button
            var closeBtn = CreateButton("Close", Close);
            closeBtn.transform.SetParent(_settingsPanel.transform, false);
            closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -140);
            
            ModLogger.Info("Settings UI created");
        }
        
        private static void LoadSettings()
        {
            // Load from config (placeholder)
            if (_showNameplatesToggle != null) _showNameplatesToggle.isOn = true;
            if (_showHealthToggle != null) _showHealthToggle.isOn = true;
            if (_nameplateDistanceSlider != null) _nameplateDistanceSlider.value = 50;
            if (_friendlyFireToggle != null) _friendlyFireToggle.isOn = false;
            if (_sharedLootToggle != null) _sharedLootToggle.isOn = true;
            if (_networkStatsToggle != null) _networkStatsToggle.isOn = false;
            
            // Disable host-only options if not host
            bool isHost = NetworkManager.Instance?.IsHost ?? false;
            if (_friendlyFireToggle != null) _friendlyFireToggle.interactable = isHost;
            if (_sharedLootToggle != null) _sharedLootToggle.interactable = isHost;
        }
        
        private static void OnNetworkStatsToggled(bool value)
        {
            InGameUI.SetShowNetworkStats(value);
        }
        
        private static void SaveSettings()
        {
            // Save to config (placeholder)
            ModLogger.Debug("Settings saved");
        }
        
        // UI Helpers
        private static Text CreateLabel(string text, int fontSize)
        {
            var obj = new GameObject("Label");
            var label = obj.AddComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = fontSize;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleCenter;
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 25);
            return label;
        }
        
        private static Toggle CreateToggle(string label, bool defaultValue)
        {
            var obj = new GameObject(label + "Toggle");
            
            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(obj.transform, false);
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.25f, 0.25f, 0.25f, 1f);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(20, 20);
            bgRect.anchoredPosition = new Vector2(-120, 0);
            
            // Checkmark
            var check = new GameObject("Checkmark");
            check.transform.SetParent(bg.transform, false);
            var checkImage = check.AddComponent<Image>();
            checkImage.color = Color.green;
            var checkRect = check.GetComponent<RectTransform>();
            checkRect.sizeDelta = new Vector2(14, 14);
            
            // Label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(obj.transform, false);
            var labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = 14;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(200, 20);
            labelRect.anchoredPosition = new Vector2(20, 0);
            
            var toggle = obj.AddComponent<Toggle>();
            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;
            toggle.isOn = defaultValue;
            
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 25);
            
            return toggle;
        }
        
        private static Slider CreateSlider(string label, float min, float max, float defaultValue)
        {
            var obj = new GameObject(label + "Slider");
            
            // Label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(obj.transform, false);
            var labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = 12;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(150, 20);
            labelRect.anchoredPosition = new Vector2(-75, 10);
            
            // Slider background
            var bg = new GameObject("Background");
            bg.transform.SetParent(obj.transform, false);
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(200, 10);
            bgRect.anchoredPosition = new Vector2(0, -5);
            
            // Fill
            var fill = new GameObject("Fill");
            fill.transform.SetParent(bg.transform, false);
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.4f, 0.6f, 1f, 1f);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0.5f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            
            // Handle
            var handle = new GameObject("Handle");
            handle.transform.SetParent(bg.transform, false);
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(15, 15);
            
            var slider = obj.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = defaultValue;
            
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 40);
            
            return slider;
        }
        
        private static Button CreateButton(string label, System.Action onClick)
        {
            var obj = new GameObject(label + "Button");
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
            
            var button = obj.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener((UnityEngine.Events.UnityAction)(() => onClick?.Invoke()));
            
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30);
            
            return button;
        }
    }
}
