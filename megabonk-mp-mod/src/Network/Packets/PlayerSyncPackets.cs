using System.Collections.Generic;
using System.IO;

namespace MegabonkMP.Network.Packets
{
    /// <summary>
    /// Player position/movement synchronization.
    /// Sent at high frequency (60Hz critical priority).
    /// </summary>
    public class PlayerPositionPacket : IPacket
    {
        public PacketType Type => PacketType.PlayerPosition;
        public int PlayerId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float VelX { get; set; }
        public float VelY { get; set; }
        public float VelZ { get; set; }
        public float RotationY { get; set; }
        public uint Timestamp { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(PosX);
            writer.Write(PosY);
            writer.Write(PosZ);
            writer.Write(VelX);
            writer.Write(VelY);
            writer.Write(VelZ);
            writer.Write(RotationY);
            writer.Write(Timestamp);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            PlayerId = reader.ReadInt32();
            PosX = reader.ReadSingle();
            PosY = reader.ReadSingle();
            PosZ = reader.ReadSingle();
            VelX = reader.ReadSingle();
            VelY = reader.ReadSingle();
            VelZ = reader.ReadSingle();
            RotationY = reader.ReadSingle();
            Timestamp = reader.ReadUInt32();
        }
    }
    
    /// <summary>
    /// Player animation state.
    /// </summary>
    public class PlayerAnimationPacket : IPacket
    {
        public PacketType Type => PacketType.PlayerAnimation;
        public int PlayerId { get; set; }
        public int AnimationId { get; set; }
        public float AnimationTime { get; set; }
        public bool IsGrounded { get; set; }
        public float MoveSpeed { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(AnimationId);
            writer.Write(AnimationTime);
            writer.Write(IsGrounded);
            writer.Write(MoveSpeed);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            PlayerId = reader.ReadInt32();
            AnimationId = reader.ReadInt32();
            AnimationTime = reader.ReadSingle();
            IsGrounded = reader.ReadBoolean();
            MoveSpeed = reader.ReadSingle();
        }
    }
    
    /// <summary>
    /// Player health/shield/overheal synchronization.
    /// </summary>
    public class PlayerHealthPacket : IPacket
    {
        public PacketType Type => PacketType.PlayerHealth;
        public int PlayerId { get; set; }
        public float CurrentHealth { get; set; }
        public float MaxHealth { get; set; }
        public float Shield { get; set; }
        public float Overheal { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(CurrentHealth);
            writer.Write(MaxHealth);
            writer.Write(Shield);
            writer.Write(Overheal);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            PlayerId = reader.ReadInt32();
            CurrentHealth = reader.ReadSingle();
            MaxHealth = reader.ReadSingle();
            Shield = reader.ReadSingle();
            Overheal = reader.ReadSingle();
        }
    }
    
    /// <summary>
    /// Player stats synchronization.
    /// Uses dictionary approach for the 60+ stat types.
    /// </summary>
    public class PlayerStatsPacket : IPacket
    {
        public PacketType Type => PacketType.PlayerStats;
        public int PlayerId { get; set; }
        public Dictionary<int, float> Stats { get; set; } = new();
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(Stats.Count);
            foreach (var kvp in Stats)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }
        }
        
        public void Deserialize(BinaryReader reader)
        {
            PlayerId = reader.ReadInt32();
            int count = reader.ReadInt32();
            Stats.Clear();
            for (int i = 0; i < count; i++)
            {
                int key = reader.ReadInt32();
                float value = reader.ReadSingle();
                Stats[key] = value;
            }
        }
    }
    
    /// <summary>
    /// Player death notification.
    /// </summary>
    public class PlayerDeathPacket : IPacket
    {
        public PacketType Type => PacketType.PlayerDeath;
        public int PlayerId { get; set; }
        public int KillerId { get; set; } // -1 for environment
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(KillerId);
            writer.Write(PosX);
            writer.Write(PosY);
            writer.Write(PosZ);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            PlayerId = reader.ReadInt32();
            KillerId = reader.ReadInt32();
            PosX = reader.ReadSingle();
            PosY = reader.ReadSingle();
            PosZ = reader.ReadSingle();
        }
    }
}
