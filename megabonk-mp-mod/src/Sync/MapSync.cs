using System;
using UnityEngine;
using Il2CppInterop.Runtime.Attributes;
using MegabonkMP.Core;
using MegabonkMP.Network;
using MegabonkMP.Network.Packets;

namespace MegabonkMP.Sync
{
    /// <summary>
    /// Handles map/level synchronization including procedural generation seed,
    /// room transitions, and event triggers.
    /// </summary>
    public class MapSync : MonoBehaviour
    {
        private static int _currentSeed;
        private static int _currentDifficulty;
        private static int _currentBiome;
        private static int _currentRoomId;
        
        // Room tracking per player
        private static readonly System.Collections.Generic.Dictionary<int, int> _playerRooms = new();
        
        public static int CurrentSeed => _currentSeed;
        public static int CurrentDifficulty => _currentDifficulty;
        
        /// <summary>
        /// Host: Set map seed and broadcast to clients.
        /// Call this when starting a new run.
        /// </summary>
        public static void SetMapSeed(int seed, int difficulty, int biomeId)
        {
            if (!NetworkManager.Instance?.IsHost ?? true)
            {
                ModLogger.Warning("Only host can set map seed");
                return;
            }
            
            _currentSeed = seed;
            _currentDifficulty = difficulty;
            _currentBiome = biomeId;
            
            var packet = new MapSeedPacket
            {
                Seed = seed,
                Difficulty = difficulty,
                BiomeId = biomeId
            };
            
            NetworkManager.Instance.Send(packet, DeliveryMethod.ReliableOrdered);
            ModLogger.Info($"Map seed set: {seed}, difficulty: {difficulty}, biome: {biomeId}");
            
            // Apply to local game (placeholder)
            ApplyMapSeed(seed, difficulty, biomeId);
        }
        
        /// <summary>
        /// Generate a random seed for new run.
        /// </summary>
        public static int GenerateRandomSeed()
        {
            return UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }
        
        /// <summary>
        /// Client: Handle map seed from host.
        /// </summary>
        [HideFromIl2Cpp]
        public static void OnMapSeedReceived(MapSeedPacket packet)
        {
            if (NetworkManager.Instance?.IsHost ?? false) return;
            
            _currentSeed = packet.Seed;
            _currentDifficulty = packet.Difficulty;
            _currentBiome = packet.BiomeId;
            
            ModLogger.Info($"Received map seed: {packet.Seed}");
            ApplyMapSeed(packet.Seed, packet.Difficulty, packet.BiomeId);
        }
        
        /// <summary>
        /// Apply map seed to procedural generation system.
        /// </summary>
        private static void ApplyMapSeed(int seed, int difficulty, int biomeId)
        {
            // Placeholder: Hook into game's MapGeneration system
            // Assets.Scripts.Game.MapGeneration would be patched here
            UnityEngine.Random.InitState(seed);
            ModLogger.Debug($"Applied seed {seed} to random state");
        }
        
        /// <summary>
        /// Notify room transition.
        /// </summary>
        public static void NotifyRoomTransition(int fromRoomId, int toRoomId)
        {
            int playerId = NetworkManager.Instance?.LocalPlayerId ?? -1;
            if (playerId < 0) return;
            
            _currentRoomId = toRoomId;
            _playerRooms[playerId] = toRoomId;
            
            var packet = new RoomTransitionPacket
            {
                PlayerId = playerId,
                FromRoomId = fromRoomId,
                ToRoomId = toRoomId
            };
            
            NetworkManager.Instance?.Send(packet, DeliveryMethod.ReliableOrdered);
        }
        
        /// <summary>
        /// Handle room transition from another player.
        /// </summary>
        [HideFromIl2Cpp]
        public static void OnRoomTransitionReceived(RoomTransitionPacket packet)
        {
            if (packet.PlayerId == NetworkManager.Instance?.LocalPlayerId) return;
            
            _playerRooms[packet.PlayerId] = packet.ToRoomId;
            ModLogger.Debug($"Player {packet.PlayerId} moved to room {packet.ToRoomId}");
            
            // Could trigger UI update showing player locations
        }
        
        /// <summary>
        /// Get room ID for a player.
        /// </summary>
        public static int GetPlayerRoom(int playerId)
        {
            return _playerRooms.TryGetValue(playerId, out int roomId) ? roomId : -1;
        }
        
        /// <summary>
        /// Check if all players are in the same room.
        /// </summary>
        public static bool AreAllPlayersInRoom(int roomId)
        {
            foreach (var player in NetworkManager.Instance?.GetAllPlayers() ?? Array.Empty<NetworkPlayer>())
            {
                if (GetPlayerRoom(player.PlayerId) != roomId)
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Reset state for new session.
        /// </summary>
        public static void Reset()
        {
            _currentSeed = 0;
            _currentDifficulty = 0;
            _currentBiome = 0;
            _currentRoomId = 0;
            _playerRooms.Clear();
        }
    }
}
