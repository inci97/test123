using System;
using System.Collections.Generic;
using UnityEngine;
using Il2CppInterop.Runtime.Attributes;
using MegabonkMP.Core;
using MegabonkMP.Network;
using MegabonkMP.Network.Packets;

namespace MegabonkMP.Sync
{
    /// <summary>
    /// Handles synchronization of player state across the network.
    /// Manages position interpolation, animation sync, and health updates.
    /// </summary>
    public class PlayerSync : MonoBehaviour
    {
        // Local player reference
        private static GameObject _localPlayer;
        private static Transform _localTransform;
        
        // Remote player tracking
        private static readonly Dictionary<int, RemotePlayer> _remotePlayers = new();
        private static readonly object _syncLock = new();
        
        // Interpolation settings
        private const float InterpolationDelay = 0.1f; // 100ms buffer
        private const int MaxPositionBuffer = 20;
        
        // Update timing
        private float _positionSendTimer;
        private float _positionSendRate = 1f / 60f; // 60Hz
        private float _healthSendTimer;
        private float _healthSendRate = 0.1f; // 10Hz or on change
        
        // State tracking for delta compression
        private Vector3 _lastSentPosition;
        private float _lastSentHealth;
        private const float PositionThreshold = 0.01f;
        
        public static void Initialize(GameObject localPlayer)
        {
            _localPlayer = localPlayer;
            _localTransform = localPlayer?.transform;
            ModLogger.Info("PlayerSync initialized with local player");
        }
        
        [HideFromIl2Cpp]
        public void Update()
        {
            if (!NetworkManager.Instance?.IsConnected ?? true) return;
            
            // Send local player updates
            if (_localTransform != null)
            {
                SendPositionUpdates(Time.deltaTime);
                SendHealthUpdates(Time.deltaTime);
            }
            
            // Interpolate remote players
            UpdateRemotePlayers(Time.deltaTime);
        }
        
        private void SendPositionUpdates(float deltaTime)
        {
            _positionSendTimer += deltaTime;
            if (_positionSendTimer < _positionSendRate) return;
            _positionSendTimer = 0f;
            
            var pos = _localTransform.position;
            
            // Delta check to reduce bandwidth
            if (Vector3.Distance(pos, _lastSentPosition) < PositionThreshold) return;
            _lastSentPosition = pos;
            
            var rb = _localPlayer.GetComponent<Rigidbody>();
            var vel = rb != null ? rb.velocity : Vector3.zero;
            
            var packet = new PlayerPositionPacket
            {
                PlayerId = NetworkManager.Instance.LocalPlayerId,
                PosX = pos.x,
                PosY = pos.y,
                PosZ = pos.z,
                VelX = vel.x,
                VelY = vel.y,
                VelZ = vel.z,
                RotationY = _localTransform.eulerAngles.y,
                Timestamp = (uint)(Time.time * 1000)
            };
            
            NetworkManager.Instance.Send(packet, DeliveryMethod.Unreliable);
        }
        
        private void SendHealthUpdates(float deltaTime)
        {
            _healthSendTimer += deltaTime;
            if (_healthSendTimer < _healthSendRate) return;
            _healthSendTimer = 0f;
            
            // Get health from game systems (placeholder - needs game hooks)
            float currentHealth = GetLocalPlayerHealth();
            if (Math.Abs(currentHealth - _lastSentHealth) < 0.1f) return;
            _lastSentHealth = currentHealth;
            
            var packet = new PlayerHealthPacket
            {
                PlayerId = NetworkManager.Instance.LocalPlayerId,
                CurrentHealth = currentHealth,
                MaxHealth = GetLocalPlayerMaxHealth(),
                Shield = GetLocalPlayerShield(),
                Overheal = GetLocalPlayerOverheal()
            };
            
            NetworkManager.Instance.Send(packet, DeliveryMethod.ReliableOrdered);
        }
        
        private void UpdateRemotePlayers(float deltaTime)
        {
            lock (_syncLock)
            {
                foreach (var remote in _remotePlayers.Values)
                {
                    remote.Interpolate(deltaTime);
                }
            }
        }
        
        /// <summary>
        /// Handle incoming position packet.
        /// </summary>
        [HideFromIl2Cpp]
        public static void OnPositionReceived(PlayerPositionPacket packet)
        {
            if (packet.PlayerId == NetworkManager.Instance?.LocalPlayerId) return;
            
            lock (_syncLock)
            {
                if (!_remotePlayers.TryGetValue(packet.PlayerId, out var remote))
                {
                    remote = SpawnRemotePlayer(packet.PlayerId);
                    if (remote == null) return;
                }
                
                remote.AddPositionSnapshot(new PositionSnapshot
                {
                    Position = new Vector3(packet.PosX, packet.PosY, packet.PosZ),
                    Velocity = new Vector3(packet.VelX, packet.VelY, packet.VelZ),
                    RotationY = packet.RotationY,
                    Timestamp = packet.Timestamp
                });
            }
        }
        
        /// <summary>
        /// Handle incoming health packet.
        /// </summary>
        [HideFromIl2Cpp]
        public static void OnHealthReceived(PlayerHealthPacket packet)
        {
            if (packet.PlayerId == NetworkManager.Instance?.LocalPlayerId) return;
            
            lock (_syncLock)
            {
                if (_remotePlayers.TryGetValue(packet.PlayerId, out var remote))
                {
                    remote.UpdateHealth(packet.CurrentHealth, packet.MaxHealth, 
                                       packet.Shield, packet.Overheal);
                }
            }
        }
        
        /// <summary>
        /// Spawn a visual representation of a remote player.
        /// </summary>
        private static RemotePlayer SpawnRemotePlayer(int playerId)
        {
            try
            {
                // Create remote player object (uses local player as template)
                var remoteGO = new GameObject($"RemotePlayer_{playerId}");
                
                // Copy visual components from local player (placeholder)
                // In actual implementation, this would clone the player model
                
                var remote = new RemotePlayer(playerId, remoteGO);
                _remotePlayers[playerId] = remote;
                
                ModLogger.Info($"Spawned remote player {playerId}");
                return remote;
            }
            catch (Exception ex)
            {
                ModLogger.Error($"Failed to spawn remote player {playerId}", ex);
                return null;
            }
        }
        
        /// <summary>
        /// Remove a remote player.
        /// </summary>
        public static void RemoveRemotePlayer(int playerId)
        {
            lock (_syncLock)
            {
                if (_remotePlayers.TryGetValue(playerId, out var remote))
                {
                    remote.Destroy();
                    _remotePlayers.Remove(playerId);
                    ModLogger.Info($"Removed remote player {playerId}");
                }
            }
        }
        
        // Placeholder methods for game integration
        private static float GetLocalPlayerHealth() => 100f;
        private static float GetLocalPlayerMaxHealth() => 100f;
        private static float GetLocalPlayerShield() => 0f;
        private static float GetLocalPlayerOverheal() => 0f;
    }
    
    /// <summary>
    /// Represents a remote player with interpolation.
    /// </summary>
    public class RemotePlayer
    {
        public int PlayerId { get; }
        public GameObject GameObject { get; }
        public Transform Transform => GameObject?.transform;
        
        // Health display
        public float CurrentHealth { get; private set; }
        public float MaxHealth { get; private set; }
        public float Shield { get; private set; }
        public float Overheal { get; private set; }
        
        // Interpolation buffer
        private readonly Queue<PositionSnapshot> _positionBuffer = new();
        private const int MaxBuffer = 20;
        private float _interpolationTime;
        
        public RemotePlayer(int playerId, GameObject gameObject)
        {
            PlayerId = playerId;
            GameObject = gameObject;
        }
        
        public void AddPositionSnapshot(PositionSnapshot snapshot)
        {
            while (_positionBuffer.Count >= MaxBuffer)
            {
                _positionBuffer.Dequeue();
            }
            _positionBuffer.Enqueue(snapshot);
        }
        
        public void UpdateHealth(float current, float max, float shield, float overheal)
        {
            CurrentHealth = current;
            MaxHealth = max;
            Shield = shield;
            Overheal = overheal;
        }
        
        public void Interpolate(float deltaTime)
        {
            if (Transform == null || _positionBuffer.Count < 2) return;
            
            _interpolationTime += deltaTime;
            
            // Find two snapshots to interpolate between
            var snapshots = _positionBuffer.ToArray();
            PositionSnapshot from = snapshots[0];
            PositionSnapshot to = snapshots[1];
            
            // Calculate interpolation factor
            float duration = (to.Timestamp - from.Timestamp) / 1000f;
            if (duration <= 0) duration = 0.016f; // Default to ~60fps
            
            float t = Mathf.Clamp01(_interpolationTime / duration);
            
            // Interpolate position
            Transform.position = Vector3.Lerp(from.Position, to.Position, t);
            
            // Interpolate rotation
            var fromRot = Quaternion.Euler(0, from.RotationY, 0);
            var toRot = Quaternion.Euler(0, to.RotationY, 0);
            Transform.rotation = Quaternion.Slerp(fromRot, toRot, t);
            
            // Move to next snapshot pair when interpolation complete
            if (t >= 1f && _positionBuffer.Count > 2)
            {
                _positionBuffer.Dequeue();
                _interpolationTime = 0f;
            }
        }
        
        public void Destroy()
        {
            if (GameObject != null)
            {
                UnityEngine.Object.Destroy(GameObject);
            }
        }
    }
    
    public struct PositionSnapshot
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float RotationY;
        public uint Timestamp;
    }
}
