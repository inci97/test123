using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Diagnostics;
using UnityEngine;

namespace MegabonkMP.Core
{
    /// <summary>
    /// Main entry point for the Megabonk Multiplayer mod.
    /// BepInEx 6 IL2CPP plugin that initializes all multiplayer systems.
    /// </summary>
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Megabonk.exe")]
    public class Plugin : BasePlugin
    {
        public static Plugin Instance { get; private set; }
        public static Harmony HarmonyInstance { get; private set; }

        // Core systems
        private Network.NetworkManager _networkManager;
        private Config _config;

        // Performance monitoring
        private float _updateTimer;
        private const float UpdateInterval = 1.0f; // Log stats every second

        public override void Load()
        {
            Instance = this;

            // Initialize logging
            Log.LogInfo($"Loading {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION}");

            try
            {
                // Load configuration
                _config = new Config(Config);
                Core.ModLogger.Initialize(Log);
                Core.ModLogger.Info("Configuration loaded");

                // Initialize Harmony for patching
                HarmonyInstance = new Harmony(PluginInfo.PLUGIN_GUID);
                var patchedMethods = HarmonyInstance.PatchAll(typeof(Plugin).Assembly);
                Core.ModLogger.Info($"Harmony patches applied: {patchedMethods.Count()} methods patched");

                // Register IL2CPP types for runtime injection
                RegisterIL2CPPTypes();

                // Initialize network manager
                _networkManager = new Network.NetworkManager();
                Core.ModLogger.Info("Network manager initialized");

                // Initialize UI components
                InitializeUI();

                // Initialize sync systems
                InitializeSyncSystems();

                Core.ModLogger.Info($"{PluginInfo.PLUGIN_NAME} loaded successfully!");
            }
            catch (Exception ex)
            {
                Core.ModLogger.Error($"Failed to load plugin: {ex}");
                Log.LogError($"Plugin load failed: {ex.Message}");
                throw;
            }
        }

        public override bool Unload()
        {
            Core.ModLogger.Info("Unloading Megabonk MP...");

            try
            {
                // Cleanup sync systems
                CleanupSyncSystems();

                // Cleanup
                _networkManager?.Shutdown();
                HarmonyInstance?.UnpatchSelf();

                Core.ModLogger.Info("Megabonk MP unloaded successfully");
                return base.Unload();
            }
            catch (Exception ex)
            {
                Core.ModLogger.Error($"Error during unload: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Register custom types with IL2CPP runtime for cross-domain interop.
        /// </summary>
        private void RegisterIL2CPPTypes()
        {
            try
            {
                // Register synchronization components
                ClassInjector.RegisterTypeInIl2Cpp<Sync.PlayerSync>();
                ClassInjector.RegisterTypeInIl2Cpp<Sync.EnemySync>();
                ClassInjector.RegisterTypeInIl2Cpp<Sync.ItemSync>();
                ClassInjector.RegisterTypeInIl2Cpp<Sync.MapSync>();

                // Register UI components
                ClassInjector.RegisterTypeInIl2Cpp<UI.LobbyUI>();
                ClassInjector.RegisterTypeInIl2Cpp<UI.InGameUI>();
                ClassInjector.RegisterTypeInIl2Cpp<UI.ModMenu>();

                Core.ModLogger.Info("IL2CPP types registered successfully");
            }
            catch (Exception ex)
            {
                Core.ModLogger.Error($"Failed to register IL2CPP types: {ex}");
                throw;
            }
        }

        private void InitializeUI()
        {
            try
            {
                // Initialize the in-game mod menu
                UI.ModMenu.Initialize();
                Core.ModLogger.Info("In-game mod menu initialized - Press F9 to open");
            }
            catch (Exception ex)
            {
                Core.ModLogger.Error($"Failed to initialize UI: {ex}");
            }
        }

        private void InitializeSyncSystems()
        {
            try
            {
                // Sync systems will initialize themselves when the game loads
                // They hook into Unity's Update() cycle automatically
                Core.ModLogger.Info("Sync systems ready for initialization");
            }
            catch (Exception ex)
            {
                Core.ModLogger.Error($"Failed to initialize sync systems: {ex}");
            }
        }

        private void CleanupSyncSystems()
        {
            try
            {
                // Cleanup will be handled by Unity's OnDestroy when scenes change
                Core.ModLogger.Info("Sync systems cleanup completed");
            }
            catch (Exception ex)
            {
                Core.ModLogger.Error($"Error during sync cleanup: {ex}");
            }
        }

        /// <summary>
        /// Called every frame by Unity. Used for performance monitoring.
        /// </summary>
        public void Update()
        {
            _updateTimer += Time.deltaTime;
            if (_updateTimer >= UpdateInterval)
            {
                _updateTimer = 0;

                // Log performance stats periodically
                if (_networkManager?.IsConnected ?? false)
                {
                    var netManager = _networkManager;
                    Core.ModLogger.Debug($"Network Stats - Ping: {netManager.Ping}ms, " +
                                       $"Sent: {netManager.PacketsSent}, Received: {netManager.PacketsReceived}");
                }
            }
        }
    }
    }
    
    /// <summary>
    /// Plugin metadata constants.
    /// </summary>
    public static class PluginInfo
    {
        public const string PLUGIN_GUID = "com.megabonk.multiplayer";
        public const string PLUGIN_NAME = "Megabonk Multiplayer";
        public const string PLUGIN_VERSION = "0.1.0";
    }
}
