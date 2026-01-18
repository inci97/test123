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
    /// Handles synchronization of items, pickups, and chests.
    /// Host-authoritative for item spawns; pickup requests validated by host.
    /// </summary>
    public class ItemSync : MonoBehaviour
    {
        private static readonly Dictionary<int, SyncedItem> _items = new();
        private static readonly Dictionary<int, SyncedChest> _chests = new();
        private static readonly object _syncLock = new();
        private static int _nextItemNetId = 1;
        private static int _nextChestNetId = 1;
        
        // Pending pickup requests (client-side)
        private static readonly Dictionary<int, float> _pendingPickups = new();
        private const float PickupTimeout = 2f;
        
        /// <summary>
        /// Host: Register item spawn and broadcast.
        /// </summary>
        public static int SpawnItem(int itemTypeId, byte rarity, Vector3 position, int sourceEntityId = -1)
        {
            if (!NetworkManager.Instance?.IsHost ?? true) return -1;
            
            int netId = _nextItemNetId++;
            
            var item = new SyncedItem
            {
                NetId = netId,
                TypeId = itemTypeId,
                Rarity = rarity,
                Position = position,
                SourceEntityId = sourceEntityId
            };
            
            lock (_syncLock)
            {
                _items[netId] = item;
            }
            
            var packet = new ItemSpawnPacket
            {
                ItemNetId = netId,
                ItemTypeId = itemTypeId,
                Rarity = rarity,
                PosX = position.x,
                PosY = position.y,
                PosZ = position.z,
                SourceEntityId = sourceEntityId
            };
            
            NetworkManager.Instance.Send(packet, DeliveryMethod.ReliableOrdered);
            ModLogger.Debug($"Item {netId} spawned (type {itemTypeId}, rarity {rarity})");
            
            return netId;
        }
        
        /// <summary>
        /// Client: Request to pick up item (host validates).
        /// </summary>
        public static void RequestPickup(int itemNetId)
        {
            if (NetworkManager.Instance?.IsHost ?? false)
            {
                // Host can pick up directly
                ProcessPickup(itemNetId, NetworkManager.Instance.LocalPlayerId);
                return;
            }
            
            // Track pending request
            _pendingPickups[itemNetId] = Time.time;
            
            var packet = new ItemPickupPacket
            {
                ItemNetId = itemNetId,
                PlayerId = NetworkManager.Instance.LocalPlayerId
            };
            
            NetworkManager.Instance.Send(packet, DeliveryMethod.ReliableOrdered);
        }
        
        /// <summary>
        /// Host: Process pickup request from client.
        /// </summary>
        [HideFromIl2Cpp]
        public static void OnPickupRequest(ItemPickupPacket packet)
        {
            if (!NetworkManager.Instance?.IsHost ?? true) return;
            
            lock (_syncLock)
            {
                if (!_items.ContainsKey(packet.ItemNetId))
                {
                    // Item already picked up or doesn't exist
                    ModLogger.Debug($"Pickup denied: item {packet.ItemNetId} not found");
                    return;
                }
            }
            
            // Validate and process
            ProcessPickup(packet.ItemNetId, packet.PlayerId);
            
            // Broadcast to all clients
            NetworkManager.Instance.Send(packet, DeliveryMethod.ReliableOrdered);
        }
        
        /// <summary>
        /// Client: Handle pickup confirmation.
        /// </summary>
        [HideFromIl2Cpp]
        public static void OnPickupConfirmed(ItemPickupPacket packet)
        {
            if (NetworkManager.Instance?.IsHost ?? false) return;
            
            _pendingPickups.Remove(packet.ItemNetId);
            
            lock (_syncLock)
            {
                if (_items.TryGetValue(packet.ItemNetId, out var item))
                {
                    // Destroy visual
                    if (item.GameObject != null)
                    {
                        UnityEngine.Object.Destroy(item.GameObject);
                    }
                    _items.Remove(packet.ItemNetId);
                }
            }
            
            // If local player, add to inventory
            if (packet.PlayerId == NetworkManager.Instance?.LocalPlayerId)
            {
                // Call game inventory system (placeholder)
                ModLogger.Debug($"Picked up item {packet.ItemNetId}");
            }
        }
        
        private static void ProcessPickup(int itemNetId, int playerId)
        {
            lock (_syncLock)
            {
                if (_items.TryGetValue(itemNetId, out var item))
                {
                    // Award item to player (placeholder for game integration)
                    if (item.GameObject != null)
                    {
                        UnityEngine.Object.Destroy(item.GameObject);
                    }
                    _items.Remove(itemNetId);
                    ModLogger.Debug($"Player {playerId} picked up item {itemNetId}");
                }
            }
        }
        
        /// <summary>
        /// Client: Handle item spawn from host.
        /// </summary>
        [HideFromIl2Cpp]
        public static void OnItemSpawnReceived(ItemSpawnPacket packet)
        {
            if (NetworkManager.Instance?.IsHost ?? false) return;
            
            lock (_syncLock)
            {
                if (_items.ContainsKey(packet.ItemNetId)) return;
                
                var position = new Vector3(packet.PosX, packet.PosY, packet.PosZ);
                
                // Spawn item visual (placeholder)
                var itemGO = SpawnItemVisual(packet.ItemTypeId, packet.Rarity, position);
                
                var item = new SyncedItem
                {
                    NetId = packet.ItemNetId,
                    TypeId = packet.ItemTypeId,
                    Rarity = packet.Rarity,
                    Position = position,
                    SourceEntityId = packet.SourceEntityId,
                    GameObject = itemGO
                };
                
                _items[packet.ItemNetId] = item;
            }
        }
        
        /// <summary>
        /// Register and sync chest.
        /// </summary>
        public static int RegisterChest(GameObject chestGO, int roomId)
        {
            if (!NetworkManager.Instance?.IsHost ?? true) return -1;
            
            int netId = _nextChestNetId++;
            
            var chest = new SyncedChest
            {
                NetId = netId,
                GameObject = chestGO,
                RoomId = roomId,
                IsOpened = false
            };
            
            lock (_syncLock)
            {
                _chests[netId] = chest;
            }
            
            return netId;
        }
        
        /// <summary>
        /// Request to open chest.
        /// </summary>
        public static void RequestChestOpen(int chestNetId)
        {
            var packet = new ChestOpenPacket
            {
                ChestNetId = chestNetId,
                PlayerId = NetworkManager.Instance?.LocalPlayerId ?? -1,
                RoomId = GetChestRoomId(chestNetId)
            };
            
            NetworkManager.Instance?.Send(packet, DeliveryMethod.ReliableOrdered);
        }
        
        /// <summary>
        /// Host: Process chest open request.
        /// </summary>
        [HideFromIl2Cpp]
        public static void OnChestOpenRequest(ChestOpenPacket packet)
        {
            if (!NetworkManager.Instance?.IsHost ?? true) return;
            
            lock (_syncLock)
            {
                if (!_chests.TryGetValue(packet.ChestNetId, out var chest)) return;
                if (chest.IsOpened) return;
                
                chest.IsOpened = true;
                
                // Spawn loot items (placeholder - integrate with game loot tables)
                var lootPos = chest.GameObject?.transform.position ?? Vector3.zero;
                SpawnChestLoot(lootPos, chest.RoomId);
            }
            
            // Broadcast to all clients
            NetworkManager.Instance.Send(packet, DeliveryMethod.ReliableOrdered);
        }
        
        /// <summary>
        /// Client: Handle chest opened notification.
        /// </summary>
        [HideFromIl2Cpp]
        public static void OnChestOpened(ChestOpenPacket packet)
        {
            lock (_syncLock)
            {
                if (_chests.TryGetValue(packet.ChestNetId, out var chest))
                {
                    chest.IsOpened = true;
                    // Play open animation (placeholder)
                }
            }
        }
        
        private static void SpawnChestLoot(Vector3 position, int roomId)
        {
            // Placeholder: spawn random items based on game loot tables
            // Actual implementation would hook into game's loot system
            int itemCount = UnityEngine.Random.Range(1, 4);
            for (int i = 0; i < itemCount; i++)
            {
                var offset = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    0.5f,
                    UnityEngine.Random.Range(-1f, 1f)
                );
                SpawnItem(
                    UnityEngine.Random.Range(1, 100), // Random item type
                    (byte)UnityEngine.Random.Range(0, 5), // Random rarity
                    position + offset
                );
            }
        }
        
        private static int GetChestRoomId(int chestNetId)
        {
            lock (_syncLock)
            {
                return _chests.TryGetValue(chestNetId, out var chest) ? chest.RoomId : -1;
            }
        }
        
        // Placeholder
        private static GameObject SpawnItemVisual(int typeId, byte rarity, Vector3 position)
        {
            var go = new GameObject($"Item_{typeId}_{rarity}");
            go.transform.position = position;
            return go;
        }
        
        /// <summary>
        /// Cleanup timed-out pickup requests.
        /// </summary>
        [HideFromIl2Cpp]
        public void Update()
        {
            var now = Time.time;
            var expired = new List<int>();
            
            foreach (var kvp in _pendingPickups)
            {
                if (now - kvp.Value > PickupTimeout)
                {
                    expired.Add(kvp.Key);
                }
            }
            
            foreach (var id in expired)
            {
                _pendingPickups.Remove(id);
            }
        }
    }
    
    public class SyncedItem
    {
        public int NetId;
        public int TypeId;
        public byte Rarity;
        public Vector3 Position;
        public int SourceEntityId;
        public GameObject GameObject;
    }
    
    public class SyncedChest
    {
        public int NetId;
        public GameObject GameObject;
        public int RoomId;
        public bool IsOpened;
    }
}
