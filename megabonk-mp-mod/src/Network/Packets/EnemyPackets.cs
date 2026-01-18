using System.IO;

namespace MegabonkMP.Network.Packets
{
    /// <summary>
    /// Enemy spawned (host-authoritative).
    /// </summary>
    public class EnemySpawnPacket : IPacket
    {
        public PacketType Type => PacketType.EnemySpawn;
        public int EnemyNetId { get; set; }
        public int EnemyTypeId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float Health { get; set; }
        public int RoomId { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(EnemyNetId);
            writer.Write(EnemyTypeId);
            writer.Write(PosX);
            writer.Write(PosY);
            writer.Write(PosZ);
            writer.Write(Health);
            writer.Write(RoomId);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            EnemyNetId = reader.ReadInt32();
            EnemyTypeId = reader.ReadInt32();
            PosX = reader.ReadSingle();
            PosY = reader.ReadSingle();
            PosZ = reader.ReadSingle();
            Health = reader.ReadSingle();
            RoomId = reader.ReadInt32();
        }
    }
    
    /// <summary>
    /// Enemy position update (sent at 30Hz).
    /// Uses batch approach for efficiency.
    /// </summary>
    public class EnemyPositionPacket : IPacket
    {
        public PacketType Type => PacketType.EnemyPosition;
        public int EnemyNetId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float VelX { get; set; }
        public float VelY { get; set; }
        public float VelZ { get; set; }
        public byte State { get; set; } // 0=Idle, 1=Patrol, 2=Chase, 3=Attack
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(EnemyNetId);
            writer.Write(PosX);
            writer.Write(PosY);
            writer.Write(PosZ);
            writer.Write(VelX);
            writer.Write(VelY);
            writer.Write(VelZ);
            writer.Write(State);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            EnemyNetId = reader.ReadInt32();
            PosX = reader.ReadSingle();
            PosY = reader.ReadSingle();
            PosZ = reader.ReadSingle();
            VelX = reader.ReadSingle();
            VelY = reader.ReadSingle();
            VelZ = reader.ReadSingle();
            State = reader.ReadByte();
        }
    }
    
    /// <summary>
    /// Enemy died.
    /// </summary>
    public class EnemyDeathPacket : IPacket
    {
        public PacketType Type => PacketType.EnemyDeath;
        public int EnemyNetId { get; set; }
        public int KillerPlayerId { get; set; }
        public int XpReward { get; set; }
        public int CreditReward { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(EnemyNetId);
            writer.Write(KillerPlayerId);
            writer.Write(XpReward);
            writer.Write(CreditReward);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            EnemyNetId = reader.ReadInt32();
            KillerPlayerId = reader.ReadInt32();
            XpReward = reader.ReadInt32();
            CreditReward = reader.ReadInt32();
        }
    }
}
