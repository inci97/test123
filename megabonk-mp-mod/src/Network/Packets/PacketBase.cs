using System;
using System.Collections.Generic;
using System.IO;
using MegabonkMP.Core;

namespace MegabonkMP.Network.Packets
{
    /// <summary>
    /// Base interface for all network packets.
    /// </summary>
    public interface IPacket
    {
        PacketType Type { get; }
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }
    
    /// <summary>
    /// Packet type identifiers for serialization.
    /// </summary>
    public enum PacketType : byte
    {
        // Connection packets (0-19)
        ConnectRequest = 0,
        ConnectAccept = 1,
        Disconnect = 2,
        Heartbeat = 3,
        
        // Session packets (20-39)
        PlayerJoin = 20,
        PlayerLeave = 21,
        PlayerReady = 22,
        SessionStart = 23,
        SessionEnd = 24,
        
        // Player sync packets (40-59)
        PlayerPosition = 40,
        PlayerAnimation = 41,
        PlayerHealth = 42,
        PlayerStats = 43,
        PlayerDeath = 44,
        PlayerRespawn = 45,
        
        // Combat packets (60-79)
        WeaponFire = 60,
        ProjectileSpawn = 61,
        ProjectileHit = 62,
        MeleeAttack = 63,
        DamageDealt = 64,
        
        // Enemy packets (80-99)
        EnemySpawn = 80,
        EnemyPosition = 81,
        EnemyDeath = 82,
        EnemyTarget = 83,
        
        // Item packets (100-119)
        ItemSpawn = 100,
        ItemPickup = 101,
        ChestOpen = 102,
        WeaponDrop = 103,
        
        // Map packets (120-139)
        MapSeed = 120,
        RoomTransition = 121,
        EventTrigger = 122,
        Extraction = 123,
        
        // Chat/social packets (140-159)
        ChatMessage = 140,
        Ping = 141,
        Emote = 142
    }
    
    /// <summary>
    /// Registry for packet type instantiation.
    /// </summary>
    public static class PacketRegistry
    {
        private static readonly Dictionary<PacketType, Func<IPacket>> _factories = new();
        
        public static void Initialize()
        {
            // Connection
            Register<ConnectRequestPacket>(PacketType.ConnectRequest);
            Register<ConnectAcceptPacket>(PacketType.ConnectAccept);
            Register<DisconnectPacket>(PacketType.Disconnect);
            Register<HeartbeatPacket>(PacketType.Heartbeat);
            
            // Session
            Register<PlayerJoinPacket>(PacketType.PlayerJoin);
            Register<PlayerLeavePacket>(PacketType.PlayerLeave);
            Register<PlayerReadyPacket>(PacketType.PlayerReady);
            Register<SessionStartPacket>(PacketType.SessionStart);
            
            // Player sync
            Register<PlayerPositionPacket>(PacketType.PlayerPosition);
            Register<PlayerAnimationPacket>(PacketType.PlayerAnimation);
            Register<PlayerHealthPacket>(PacketType.PlayerHealth);
            Register<PlayerStatsPacket>(PacketType.PlayerStats);
            Register<PlayerDeathPacket>(PacketType.PlayerDeath);
            
            // Combat
            Register<WeaponFirePacket>(PacketType.WeaponFire);
            Register<ProjectileSpawnPacket>(PacketType.ProjectileSpawn);
            Register<DamageDealtPacket>(PacketType.DamageDealt);
            
            // Enemy
            Register<EnemySpawnPacket>(PacketType.EnemySpawn);
            Register<EnemyPositionPacket>(PacketType.EnemyPosition);
            Register<EnemyDeathPacket>(PacketType.EnemyDeath);
            
            // Item
            Register<ItemSpawnPacket>(PacketType.ItemSpawn);
            Register<ItemPickupPacket>(PacketType.ItemPickup);
            Register<ChestOpenPacket>(PacketType.ChestOpen);
            
            // Map
            Register<MapSeedPacket>(PacketType.MapSeed);
            Register<RoomTransitionPacket>(PacketType.RoomTransition);
            
            // Chat
            Register<ChatMessagePacket>(PacketType.ChatMessage);
            Register<PingPacket>(PacketType.Ping);
            
            ModLogger.Info($"Registered {_factories.Count} packet types");
        }
        
        private static void Register<T>(PacketType type) where T : IPacket, new()
        {
            _factories[type] = () => new T();
        }
        
        public static IPacket Create(PacketType type)
        {
            if (_factories.TryGetValue(type, out var factory))
            {
                return factory();
            }
            ModLogger.Warning($"Unknown packet type: {type}");
            return null;
        }
    }
    
    /// <summary>
    /// Serialization utilities for packets.
    /// </summary>
    public static class PacketSerializer
    {
        public static byte[] Serialize(IPacket packet)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            
            writer.Write((byte)packet.Type);
            packet.Serialize(writer);
            
            return ms.ToArray();
        }
        
        public static IPacket Deserialize(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);
            
            var type = (PacketType)reader.ReadByte();
            var packet = PacketRegistry.Create(type);
            
            if (packet != null)
            {
                packet.Deserialize(reader);
            }
            
            return packet;
        }
    }
    
    /// <summary>
    /// Extension methods for binary serialization of common types.
    /// </summary>
    public static class BinaryExtensions
    {
        public static void WriteVector3(this BinaryWriter writer, float x, float y, float z)
        {
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
        }
        
        public static (float x, float y, float z) ReadVector3(this BinaryReader reader)
        {
            return (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
        
        public static void WriteQuaternion(this BinaryWriter writer, float x, float y, float z, float w)
        {
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
            writer.Write(w);
        }
        
        public static (float x, float y, float z, float w) ReadQuaternion(this BinaryReader reader)
        {
            return (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
    }
}
