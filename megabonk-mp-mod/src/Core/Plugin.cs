using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System;

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
                HarmonyInstance.PatchAll(typeof(Plugin).Assembly);
                Core.ModLogger.Info("Harmony patches applied");
                
                // Register IL2CPP types for runtime injection
                RegisterIL2CPPTypes();
                
                // Initialize network manager
                _networkManager = new Network.NetworkManager();
                Core.ModLogger.Info("Network manager initialized");
                
                // Initialize UI components
                InitializeUI();
                
                Core.ModLogger.Info($"{PluginInfo.PLUGIN_NAME} loaded successfully!");
            }
            catch (Exception ex)
            {
                Core.ModLogger.Error($"Failed to load plugin: {ex}");
                throw;
            }
        }
        
        public override bool Unload()
        {
            Core.ModLogger.Info("Unloading Megabonk MP...");
            
            // Cleanup
            _networkManager?.Shutdown();
            HarmonyInstance?.UnpatchSelf();
            
            return base.Unload();
        }
        
        /// <summary>
        /// Register custom types with IL2CPP runtime for cross-domain interop.
        /// </summary>
        private void RegisterIL2CPPTypes()
        {
            // Register synchronization components
            ClassInjector.RegisterTypeInIl2Cpp<Sync.PlayerSync>();
            ClassInjector.RegisterTypeInIl2Cpp<Sync.EnemySync>();
            ClassInjector.RegisterTypeInIl2Cpp<Sync.ItemSync>();
            ClassInjector.RegisterTypeInIl2Cpp<Sync.MapSync>();
            
            // Register UI components
            ClassInjector.RegisterTypeInIl2Cpp<UI.LobbyUI>();
            ClassInjector.RegisterTypeInIl2Cpp<UI.InGameUI>();
            
            Core.ModLogger.Info("IL2CPP types registered");
        }
        
        private void InitializeUI()
        {
            // UI initialization will be handled when game loads
            Core.ModLogger.Info("UI system ready");
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
