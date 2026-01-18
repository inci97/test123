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
    /// Handles synchronization of enemy entities (host-authoritative).
    /// Host sends enemy spawns and positions, clients interpolate.
    /// </summary>
    public class EnemySync : MonoBehaviour
    {
        // Network ID mapping to game objects
        private static readonly Dictionary<int, SyncedEnemy> _enemies = new();
        private static readonly object _syncLock = new();
        private static int _nextEnemyNetId = 1;
        
        // Update rates per roadmap (30Hz for enemies)
        private float _positionSendTimer;
        private const float PositionSendRate = 1f / 30f;
        
        // Enemy cap per roadmap (400-600 based on player count)
        private static int _enemyCap = 400;
        
        public static void SetEnemyCap(int playerCount)
        {
            // Scale: 400 base, +50 per additional player
            _enemyCap = 400 + (playerCount - 1) * 50;
            if (_enemyCap > 600) _enemyCap = 600;
            ModLogger.Info($"Enemy cap set to {_enemyCap} for {playerCount} players");
        }
        
        [HideFromIl2Cpp]
        public void Update()
        {
            if (!NetworkManager.Instance?.IsConnected ?? true) return;
            
            if (NetworkManager.Instance.IsHost)
            {
                SendEnemyPositionUpdates(Time.deltaTime);
            }
            else
            {
                UpdateEnemyInterpolation(Time.deltaTime);
            }
        }
        
        /// <summary>
        /// Host: Register a spawned enemy for network sync.
        /// </summary>
        public static int RegisterEnemy(GameObject enemyGO, int enemyTypeId, int roomId)
        {
            if (!NetworkManager.Instance?.IsHost ?? true) return -1;
            if (_enemies.Count >= _enemyCap) return -1;
            
            int netId = _nextEnemyNetId++;
            var pos = enemyGO.transform.position;
            
            var synced = new SyncedEnemy
            {
                NetId = netId,
                TypeId = enemyTypeId,
                GameObject = enemyGO,
                RoomId = roomId,
                LastSyncedPos = pos
            };
            
            lock (_syncLock)
            {
                _enemies[netId] = synced;
            }
            
            // Broadcast spawn to clients
            var packet = new EnemySpawnPacket
            {
                EnemyNetId = netId,
                EnemyTypeId = enemyTypeId,
                PosX = pos.x,
                PosY = pos.y,
                PosZ = pos.z,
                Health = GetEnemyHealth(enemyGO),
                RoomId = roomId
            };
            
            NetworkManager.Instance.Send(packet, DeliveryMethod.ReliableOrdered);
            return netId;
        }
        
        /// <summary>
        /// Host: Notify enemy death.
        /// </summary>
        public static void NotifyEnemyDeath(int netId, int killerPlayerId, int xpReward, int creditReward)
        {
            if (!NetworkManager.Instance?.IsHost ?? true) return;
            
            var packet = new EnemyDeathPacket
            {
                EnemyNetId = netId,
                KillerPlayerId = killerPlayerId,
                XpReward = xpReward,
                CreditReward = creditReward
            };
            
            NetworkManager.Instance.Send(packet, DeliveryMethod.ReliableOrdered);
            
            lock (_syncLock)
            {
                _enemies.Remove(netId);
            }
        }
        
        private void SendEnemyPositionUpdates(float deltaTime)
        {
            _positionSendTimer += deltaTime;
            if (_positionSendTimer < PositionSendRate) return;
            _positionSendTimer = 0f;
            
            lock (_syncLock)
            {
                foreach (var kvp in _enemies)
                {
                    var enemy = kvp.Value;
                    if (enemy.GameObject == null) continue;
                    
                    var pos = enemy.GameObject.transform.position;
                    
                    // Only send if moved significantly
                    if (Vector3.Distance(pos, enemy.LastSyncedPos) < 0.1f) continue;
                    enemy.LastSyncedPos = pos;
                    
                    var rb = enemy.GameObject.GetComponent<Rigidbody>();
                    var vel = rb != null ? rb.velocity : Vector3.zero;
                    
                    var packet = new EnemyPositionPacket
                    {
                        EnemyNetId = enemy.NetId,
                        PosX = pos.x,
                        PosY = pos.y,
                        PosZ = pos.z,
                        VelX = vel.x,
                        VelY = vel.y,
                        VelZ = vel.z,
                        State = enemy.CurrentState
                    };
                    
                    NetworkManager.Instance.Send(packet, DeliveryMethod.Unreliable);
                }
            }
        }
        
        private void UpdateEnemyInterpolation(float deltaTime)
        {
            lock (_syncLock)
            {
                foreach (var enemy in _enemies.Values)
                {
                    enemy.Interpolate(deltaTime);
                }
            }
        }
        
        /// <summary>
        /// Client: Handle enemy spawn packet.
        /// </summary>
        [HideFromIl2Cpp]
        public static void OnEnemySpawnReceived(EnemySpawnPacket packet)
        {
            if (NetworkManager.Instance?.IsHost ?? false) return;
            
            lock (_syncLock)
            {
                if (_enemies.ContainsKey(packet.EnemyNetId)) return;
                
                // Spawn enemy prefab based on type (placeholder)
                var enemyGO = SpawnEnemyPrefab(packet.EnemyTypeId);
                if (enemyGO == null) return;
                
                enemyGO.transform.position = new Vector3(packet.PosX, packet.PosY, packet.PosZ);
                
                var synced = new SyncedEnemy
                {
                    NetId = packet.EnemyNetId,
                    TypeId = packet.EnemyTypeId,
                    GameObject = enemyGO,
                    RoomId = packet.RoomId,
                    Health = packet.Health,
                    LastSyncedPos = enemyGO.transform.position
                };
                
                _enemies[packet.EnemyNetId] = synced;
            }
        }
        
        /// <summary>
        /// Client: Handle enemy position packet.
        /// </summary>
        [HideFromIl2Cpp]
        public static void OnEnemyPositionReceived(EnemyPositionPacket packet)
        {
            if (NetworkManager.Instance?.IsHost ?? false) return;
            
            lock (_syncLock)
            {
                if (!_enemies.TryGetValue(packet.EnemyNetId, out var enemy)) return;
                
                enemy.TargetPosition = new Vector3(packet.PosX, packet.PosY, packet.PosZ);
                enemy.Velocity = new Vector3(packet.VelX, packet.VelY, packet.VelZ);
                enemy.CurrentState = packet.State;
            }
        }
        
        /// <summary>
        /// Client: Handle enemy death packet.
        /// </summary>
        [HideFromIl2Cpp]
        public static void OnEnemyDeathReceived(EnemyDeathPacket packet)
        {
            lock (_syncLock)
            {
                if (_enemies.TryGetValue(packet.EnemyNetId, out var enemy))
                {
                    // Play death effect and destroy
                    if (enemy.GameObject != null)
                    {
                        UnityEngine.Object.Destroy(enemy.GameObject);
                    }
                    _enemies.Remove(packet.EnemyNetId);
                }
            }
            
            // Award XP/credits to killer (if local player)
            if (packet.KillerPlayerId == NetworkManager.Instance?.LocalPlayerId)
            {
                // Call game systems to award rewards (placeholder)
                ModLogger.Debug($"Awarded {packet.XpReward} XP, {packet.CreditReward} credits");
            }
        }
        
        // Placeholder methods
        private static float GetEnemyHealth(GameObject enemyGO) => 100f;
        private static GameObject SpawnEnemyPrefab(int typeId) => new GameObject($"Enemy_{typeId}");
    }
    
    public class SyncedEnemy
    {
        public int NetId;
        public int TypeId;
        public GameObject GameObject;
        public int RoomId;
        public float Health;
        public Vector3 LastSyncedPos;
        public Vector3 TargetPosition;
        public Vector3 Velocity;
        public byte CurrentState;
        
        private const float InterpolationSpeed = 10f;
        
        public void Interpolate(float deltaTime)
        {
            if (GameObject == null) return;
            
            // Smooth interpolation towards target
            var current = GameObject.transform.position;
            var target = TargetPosition + Velocity * deltaTime;
            GameObject.transform.position = Vector3.Lerp(current, target, InterpolationSpeed * deltaTime);
        }
    }
}
